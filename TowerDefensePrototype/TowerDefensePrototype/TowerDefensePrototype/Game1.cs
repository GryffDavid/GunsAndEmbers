using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;
using GameDataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;

namespace TowerDefensePrototype
{
    public enum TrapType { Wall, Spikes, Catapult, Fire, Ice, Barrel, SawBlade, Line, Trigger };
    public enum TurretType { MachineGun, Cannon, FlameThrower, Lightning, Cluster, FelCannon, Beam, Freeze, Boomerang, Grenade, GasGrenade, Shotgun, PersistentBeam };
    public enum InvaderType { Soldier, BatteringRam, Airship, Archer, Tank, Spider, Slime, SuicideBomber, FireElemental, TestInvader, StationaryCannon };
    public enum HeavyProjectileType { CannonBall, FlameThrower, Arrow, Acid, Torpedo, ClusterBomb, ClusterBombShell, FelProjectile, Boomerang, Grenade,  GasGrenade };
    public enum LightProjectileType { MachineGun, Freeze, Lightning, Beam, Pulse, Shotgun, PersistentBeam };

    public enum GameState { Menu, Loading, Playing, Paused, ProfileSelect, Options, ProfileManagement, GettingName, Victory};
    public enum ProfileState { Standard, Upgrades, Stats };
    public enum ProfileManagementState { Loadout, Upgrades, Stats };

    public enum InvaderBehaviour { AttackTraps, AttackTower, AttackTurrets };
    
    public enum SpecialType { AirStrike };
    public enum DamageType { Fire, Electric, Concussive, Kinetic, Radiation };
    public enum TurretFireType { FullAuto, SemiAuto, Single, Beam };
    
    public enum Weather { Snow };
    public enum WorldType { Snowy };    
   
    public enum GeneralPowerup { ExplosivePower, BlastRadii, RepairTower };
    public enum TurretPowerup { DoubleShot, SlowInvaders, ArcSight };
    public enum TrapPowerup { RepairTraps };
    public enum TowerPowerup { ShieldCooldown, InsideShield, InvulnerableRanged };

    //For handling button presses and releases
    public enum MousePosition { Inside, Outside };
    public enum ButtonSpriteState { Released, Hover, Pressed };

    public struct StackedUpgrade
    {
        public float GatlingSpeed, GatlingDamage, GatlingAccuracy;
        public float CannonSpeed, CannonDamage, CannonBlastRadius;
    };

    #region Invader damage type structs - DOT, Slow, Freeze
    public class DamageOverTimeStruct
    {
        //Initial damage is the first burst of damage done to the invader on contact
        //Damage is the damage done to the invader every tick after that
        //Milliseconds is how long the DOT effect lasts for
        //Interval is the time between the ticks
        public float Milliseconds, Damage, Interval, InitialDamage;
        public Color Color;
    };

    public class SlowStruct
    {
        public float Milliseconds, SpeedPercentage;
    };

    public class FreezeStruct
    {
        public float Milliseconds;
    };
    #endregion


    //This should possibly be moved into a base class which the powerups inherit from
    public struct PowerupEffect
    {
        public float PowerupValue;
        public Nullable<GeneralPowerup> GeneralPowerup;
        public Nullable<TurretPowerup> TurretPowerup;
        public Nullable<TrapPowerup> TrapPowerup;
        public Nullable<TowerPowerup> TowerPowerup;
    };

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Variable declarations

        GraphicsDeviceManager graphics;
        ContentManager SecondaryContent;
        SpriteBatch spriteBatch, targetBatch;
        RenderTarget2D GameRenderTarget, MenuRenderTarget, UIRenderTarget;
        RenderTarget2D ShaderTarget1, ShaderTarget2;
        RenderTarget2D CrepuscularMap;
        Texture2D ScreenTex;
        
        string Error = "";

        #region XNA Declarations
        static Random Random = new Random();
        BinaryFormatter formatter = new BinaryFormatter();

        //Sprites
        #region Decal sprites
        Texture2D BloodDecal1, ExplosionDecal1, BulletDecal1;
        #endregion

        #region Particle sprites
        Texture2D BlankTexture, ShellCasing, LightningShellCasing, Coin, RoundSparkParticle,
                  HealthBarSprite, SplodgeParticle, SmokeParticle, FireParticle, FireParticle2, ExplosionParticle, ExplosionParticle2, BallParticle, SparkParticle,
                  BulletTrailCap, BulletTrailSegment, BlurrySnowflake, FocusedSnowflake, MachineBullet;
        #endregion

        #region Icon sprites
        Texture2D LockIcon, HealthIcon, OverHeatIcon, CurrencyIcon;

        //Turret icons
        public Texture2D BeamTurretIcon, BoomerangTurretIcon, CannonTurretIcon, ClusterTurretIcon, FelCannonTurretIcon, FlameThrowerTurretIcon,
                         FreezeTurretIcon, GrenadeTurretIcon, GasGrenadeTurretIcon, LightningTurretIcon, MachineGunTurretIcon, PersistentBeamTurretIcon, ShotgunTurretIcon;

        //Trap Icons
        public Texture2D CatapultTrapIcon, IceTrapIcon, TarTrapIcon, WallTrapIcon, BarrelTrapIcon, FireTrapIcon, LineTrapIcon, SawBladeTrapIcon,
                         SpikesTrapIcon;

        //Damage Icons
        public Texture2D ConcussiveDamageIcon, KineticDamageIcon, FireDamageIcon, ElectricDamageIcon, RadiationDamageIcon;

        #endregion

        #region Cursor sprites
        Texture2D CurrentCursorTexture, PrimaryCursorTexture, DefaultCursor, CrosshairCursor;

        //Turret Cursors        
        public Texture2D MachineGunTurretCursor, CannonTurretCursor, FlameThrowerTurretCursor, LightningTurretCursor, ClusterTurretCursor,
                         FelCannonTurretCursor, BeamTurretCursor, FreezeTurretCursor, BoomerangTurretCursor, GrenadeTurretCursor, GasGrenadeTurretCursor,
                         ShotgunTurretCursor,
                         PersistentBeamTurretCursor;

        //Trap Cursors
        public Texture2D WallTrapCursor, SpikesTrapCursor, CatapultTrapCursor, FireTrapCursor, IceTrapCursor, TarTrapCursor, BarrelTrapCursor,
                         SawBladeTrapCursor, LineTrapCursor, TriggerTrapCursor;
        #endregion

        #region Trap sprites
        List<Texture2D> WallSprite, BarrelTrapSprite, CatapultTrapSprite, IceTrapSprite, TarTrapSprite,
                        LineTrapSprite, SawBladeTrapSprite, SpikeTrapSprite, FireTrapSprite;
        #endregion

        #region Turret sprites
        Texture2D TurretSelectBox;
        public Texture2D MachineGunTurretBase, MachineGunTurretBarrel;
        public Texture2D CannonTurretBase, CannonTurretBarrel;
        public Texture2D LightningTurretBase, LightningTurretBarrel;
        public Texture2D BeamTurretBase, BeamTurretBarrel;
        public Texture2D ClusterTurretBase, ClusterTurretBarrel;
        public Texture2D GrenadeTurretBase, GrenadeTurretBarrel;
        public Texture2D GasGrenadeTurretBase, GasGrenadeTurretBarrel;
        public Texture2D ShotgunTurretBase, ShotgunTurretBarrel;
        public Texture2D FlameThrowerTurretBase, FlameThrowerTurretBarrel;
        public Texture2D PersistentBeamTurretBase, PersistentBeamTurretBarrel;
        public Texture2D FelCannonTurretBase, FelCannonTurretBarrel;
        public Texture2D BoomerangTurretBase, BoomerangTurretBarrel;
        #endregion

        #region Enemy sprites
        public Texture2D IceBlock, Shadow;
        public List<Texture2D> SoldierSprite, BatteringRamSprite, AirshipSprite, ArcherSprite, TankSprite, SpiderSprite,
                               SlimeSprite, SuicideBomberSprite, FireElementalSprite, TestInvaderSprite, StationaryCannonSprite;

        #endregion

        #region Button sprites
        Texture2D SelectButtonSprite, ButtonLeftSprite, ButtonRightSprite, LeftArrowSprite, RightArrowSprite, SmallButtonSprite,
                  TextBoxSprite, WeaponBoxSprite, TurretSlotButtonSprite, DialogBox, DiamondButtonSprite, ShortButtonLeftSprite,
                  ShortButtonRightSprite;
        #endregion

        #region This is for the horizontal health bars
        Texture2D WhiteBlock;
        #endregion

        #region Colour declarations
        public Color HalfWhite = Color.Lerp(Color.White, Color.Transparent, 0.5f);

        public Color FireColor = new Color(Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 150);
        public Color FireColor2 = new Color(255, Color.DarkOrange.G, Color.DarkOrange.B, 50);

        public Color ExplosionColor = new Color(255, 255, 255, 150);
        public Color ExplosionColor2 = new Color(255, 255, 255, 50);

        public Color DirtColor = new Color(51, 31, 0, 100);
        public Color DirtColor2 = new Color(51, 31, 0, 125);

        public Color SmokeColor1 = Color.Lerp(Color.DarkGray, Color.Transparent, 0.1f);
        public Color SmokeColor2 = Color.Lerp(Color.Gray, Color.Transparent, 0.1f);

        public Color MenuColor = Color.White;
        #endregion        

        #region Weather Sprites
        Texture2D SnowDust1, SnowDust2, SnowDust3, SnowDust4, SnowDust5;
        #endregion

        #region Grass sprites
        Texture2D GrassBlade1;
        #endregion

        #region Fonts
        SpriteFont DefaultFont, TooltipFont, ResourceFont, BigUIFont,
                   RobotoBold40_2, RobotoRegular40_2, RobotoRegular20_2, RobotoRegular20_0, RobotoItalic20_0;
        #endregion

        #region Projectile sprites
        Texture2D CannonBallSprite, GrenadeSprite, GasGrenadeSprite, FlameSprite, ClusterBombSprite, ClusterShellSprite, BoomerangSprite, AcidSprite;
        #endregion

        #region Powerup Icon sprites

        #endregion

        Texture2D TerrainShrub1, TooltipBox, FlareMap1;
        Texture2D PowerupDeliveryPod, PowerupDeliveryFin;
        Texture2D MysteryEmployerTexture;

        Vector2 CursorPosition, ActualResolution;
        Rectangle ScreenRectangle;        

        MouseState CurrentMouseState, PreviousMouseState;
        KeyboardState CurrentKeyboardState, PreviousKeyboardState;

        int Resources, TrapLimit, TowerButtons, ProfileNumber, LevelNumber, limit, TotalParticles;
        int CurrentWaveIndex = 0;
        int CurrentInvaderIndex = 0;
        int MaxWaves = 0;

        string FileName, ContainerName, ProfileName, LoadingContentString;

        bool ReadyToPlace, IsLoading, AllLoaded, Slow;
        bool DialogVisible = false;
        bool Diagnostics = false;
        bool Tutorial = false;
        bool StartWave = false;
        bool Victory = false;

        float MenuSFXVolume, MenuMusicVolume, VictoryTime, CurrentInvaderTime, CurrentWaveTime, CurrentWavePauseTime;

        double Seconds, ResolutionOffsetRatio;

        Effect HealthBarEffect, ShockWaveEffect, ShieldBubbleEffect, BackgroundEffect, ButtonBlurEffect, CrepuscularEffect;
        Color CursorColor = Color.White;
        Matrix Projection, MouseTransform, QuadProjection;
        #endregion

        #region Sound effects
        SoundEffect MenuClick, FireTrapStart, LightningSound, CannonExplosion, CannonFire,
                    MachineShot1, GroundImpact, Ricochet1, Ricochet2, Ricochet3, MenuWoosh,
                    PlaceTrap, Splat1, Splat2, MenuMusic, Implosion, TurretOverheat;

        SoundEffectInstance MenuMusicInstance, TurretOverheatInstance;
        #endregion

        #region List declarations
        List<Button> TowerButtonList, MainMenuButtonList, PauseButtonList,
                     ProfileButtonList, ProfileDeleteList, PlaceWeaponList, UpgradeButtonList, 
                     SpecialAbilitiesButtonList;

        List<Trap> TrapList;
        List<Turret> TurretList;
        List<Invader> InvaderList;

        List<HeavyProjectile> HeavyProjectileList, InvaderHeavyProjectileList;
        List<LightProjectile> InvaderLightProjectileList, LightProjectileList;
        List<TimerHeavyProjectile> TimedProjectileList;

        List<Emitter> YSortedEmitterList, EmitterList2, AlphaEmitterList, AdditiveEmitterList;
        List<Particle> ShellCasingList, CoinList;

        List<string> PauseMenuNameList;

        List<Explosion> ExplosionList, EnemyExplosionList;

        List<LightningBolt> LightningList;
        List<BulletTrail> TrailList, InvaderTrailList;

        List<NumberChange> NumberChangeList = new List<NumberChange>();

        List<StaticSprite> TerrainSpriteList, WeatherSpriteList;

        List<Decal> DecalList = new List<Decal>();
        List<Light> LightList = new List<Light>();

        List<WeaponBox> SelectTurretList, SelectTrapList;

        List<CooldownButton> CooldownButtonList = new List<CooldownButton>();

        List<UIWeaponInfoTip> UIWeaponInfoList = new List<UIWeaponInfoTip>();
        List<UITrapQuickInfo> UITrapQuickInfoList = new List<UITrapQuickInfo>();

        List<UIOutline> UITrapOutlineList = new List<UIOutline>();
        List<UIOutline> UITurretOutlineList = new List<UIOutline>();
        //List<UIOutline> UIInvaderOutlineList = new List<UIOutline>();

        List<ShellCasing> VerletShells = new List<ShellCasing>();
        List<GrassBlade> GrassBladeList = new List<GrassBlade>();
        #endregion

        #region Custom class declarations
        Button ProfileBackButton, ProfileManagementPlay, ProfileManagementBack,
               OptionsBack, OptionsSFXUp, OptionsSFXDown, OptionsMusicUp, OptionsMusicDown,
               GetNameOK, GetNameBack, MoveTurretsLeft, MoveTurretsRight, MoveTrapsLeft, MoveTrapsRight,
               VictoryRetry, VictoryContinue, VictoryBack;
        Tower Tower;
        StaticSprite Ground, ForeGround, MainMenuBackground, SkyBackground, TextBox, MenuTower;
        AnimatedSprite LoadingAnimation;
        Nullable<TrapType> SelectedTrap;
        Nullable<TurretType> SelectedTurret;
        Nullable<SpecialType> SelectedSpecial;
        LightProjectile CurrentProjectile, CurrentInvaderProjectile;
        GameState GameState;
        ProfileManagementState ProfileManagementState;
        Thread LoadingThread;
        Profile CurrentProfile;
        StorageDevice Device;
        Stream OpenFile;        
        Settings CurrentSettings, DefaultSettings;
        TextInput NameInput;
        DialogBox ExitDialog, DeleteProfileDialog, MainMenuDialog, ProfileMenuDialog, NoWeaponsDialog, NameLengthDialog;
        Tooltip InGameInformation;
        Camera Camera;
        public StackedUpgrade StackedUpgrade = new StackedUpgrade();
        Level CurrentLevel;
        Wave CurrentWave = null;
        BulletTrail Trail;
        LightningBolt Lightning = new LightningBolt(Vector2.One, Vector2.Zero, Color.White, 1);
        LightningBolt Bolt = new LightningBolt(Vector2.One, Vector2.Zero, Color.White, 1);
        Trap NewTrap;
        Button StartWaveButton;
        WaveCountDown WaveCountDown;
        Turret CurrentTurret;
        UITurretInfo UITurretInfo;
        Tabs ProfileManagementTabs;
        
        //UIInvaderInfo UIInvaderInfo;
        #endregion

        #region Upgrade values
        float MachineGunTurretSpeed = 0;
        float MachineGunTurretAccuracy = 0;
        float MachineGunTurretDamage = 0;

        float CannonTurretSpeed = 0;
        float CannonTurretDamage = 0;
        float CannonTurretBlastRadius = 0;
        #endregion

        UISlopedBar HealthBar, ShieldBar;
        
        BasicEffect QuadEffect;        

        FrameRateCounter FPSCounter = new FrameRateCounter();

        SpecialAbility CurrentSpecialAbility;

        Emitter BlurryEmitter, FocusedEmitter;

        float CurrentWeatherTime;
        Nullable<Weather> CurrentWeather;
        #endregion

        PowerupDelivery PowerupDelivery;
        StoryDialogueBox StoryDialogueBox;
        StoryDialogue StoryDialogue;

        //Powerups need to be drawn in a stack [Power 30] [Blast 15] [TrapDistance 60] at the middle-top of the screen
        List<Powerup> PowerupsList = new List<Powerup>();
        List<UIPowerupIcon> UIPowerupsList = new List<UIPowerupIcon>();
        List<UIPowerupInfo> UIPowerupsInfoList = new List<UIPowerupInfo>();


        List<Emitter> GasEmitterList = new List<Emitter>();
        public Game1()
        {
            DefaultSettings = new Settings
            {
                FullScreen = false,
                SFXVolume = 1.0f,
                MusicVolume = 1.1f,
                TimesPlayed = 0,
                ResWidth = 1920,
                ResHeight = 1080
            };

            LoadSettings();
            CurrentSettings.TimesPlayed++;
            SaveSettings();

            graphics = new GraphicsDeviceManager(this) 
            { 
                PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8 
            };

            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            //IsFixedTimeStep = true;
            //TargetElapsedTime = TimeSpan.FromMilliseconds(8);
            Content.RootDirectory = "Content";

            #region Set up game window

            //Use ratio to calculate backbuffer size depending on the players selected resolution and the actual screen ratio
            //e.g. Selected resolution = 1280x720, screen size = 1920x1200, screen ratio = 16:10, game ratio = 16:9
            //Full screen resolution = 1280x800, render size = 1280x720, difference = 80/2
            //This makes sure that if the screen isn't 16:9 the player gets a letterboxed view instead of a distorted game.
            //CurrentSettings.FullScreen = true;

            if (CurrentSettings.FullScreen == true)
            {
                Vector2 ScreenSize = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
                double ScreenRatio = ScreenSize.X / ScreenSize.Y;
                double Reciprocal = 1 / ScreenRatio;

                Vector2 SelectedResolution = new Vector2(CurrentSettings.ResWidth, CurrentSettings.ResHeight);
                ActualResolution = new Vector2(SelectedResolution.X, (float)(SelectedResolution.X * Reciprocal));
                ResolutionOffsetRatio = 1920.0 / (float)(CurrentSettings.ResWidth);
                graphics.PreferredBackBufferHeight = (int)ActualResolution.Y;
                graphics.PreferredBackBufferWidth = (int)ActualResolution.X;
                graphics.IsFullScreen = true;
            }
            else
            {
                ActualResolution = new Vector2(CurrentSettings.ResWidth, CurrentSettings.ResHeight);
                graphics.PreferredBackBufferHeight = (int)ActualResolution.Y;
                graphics.PreferredBackBufferWidth = (int)ActualResolution.X;
                ResolutionOffsetRatio = 1920.0 / (float)(CurrentSettings.ResWidth);
                graphics.IsFullScreen = false;
            }

            graphics.ApplyChanges();
            #endregion
        }

        protected override void Initialize()
        {
            SecondaryContent = new ContentManager(Content.ServiceProvider, Content.RootDirectory);
            GameState = GameState.Menu;
            ProfileManagementState = ProfileManagementState.Loadout;
            ContainerName = "Profiles";
            TrapLimit = 8;
            Camera = new Camera();
            Tower = new Tower("Tower", new Vector2(100, 500), 350, 120, 3, 5000);
            TowerButtons = (int)Tower.Slots;
            ScreenRectangle = new Rectangle(-256, -256, 1920 + 256, 1080 + 256);

            QuadProjection = Matrix.CreateOrthographicOffCenter(0,
                                    1920,
                                    1080,
                                    0, 0, 1);
                                    //* Matrix.CreateRotationZ(MathHelper.ToRadians(5.5f));

            QuadEffect = new BasicEffect(graphics.GraphicsDevice);
            QuadEffect.Projection = QuadProjection;
            QuadEffect.VertexColorEnabled = true;

            LoadFonts();

            #region Load button sprites
            DialogBox = SecondaryContent.Load<Texture2D>("DialogBox");

            ButtonLeftSprite = SecondaryContent.Load<Texture2D>("Buttons/ButtonLeft");
            ButtonRightSprite = SecondaryContent.Load<Texture2D>("Buttons/ButtonRight");

            ShortButtonLeftSprite = SecondaryContent.Load<Texture2D>("Buttons/ShortButtonLeft");
            ShortButtonRightSprite = SecondaryContent.Load<Texture2D>("Buttons/ShortButtonRight");

            LeftArrowSprite = SecondaryContent.Load<Texture2D>("Buttons/LeftArrow");
            RightArrowSprite = SecondaryContent.Load<Texture2D>("Buttons/RightArrow");

            SelectButtonSprite = SecondaryContent.Load<Texture2D>("Buttons/Button");

            SmallButtonSprite = SecondaryContent.Load<Texture2D>("Buttons/SmallButton");

            TextBoxSprite = SecondaryContent.Load<Texture2D>("Buttons/TextBox");

            WeaponBoxSprite = SecondaryContent.Load<Texture2D>("Buttons/WeaponBox");

            TurretSlotButtonSprite = SecondaryContent.Load<Texture2D>("Buttons/TurretSlotButton");

            DiamondButtonSprite = SecondaryContent.Load<Texture2D>("Buttons/DiamondButton");

            WhiteBlock = SecondaryContent.Load<Texture2D>("WhiteBlock");
            #endregion

            #region Initialise Main Menu

            MainMenuButtonList = new List<Button>();
            MainMenuButtonList.Add(new Button(ButtonLeftSprite, new Vector2(-300, 130), null, null, null, "          play", RobotoRegular20_2, "Left", Color.White));
            MainMenuButtonList.Add(new Button(ButtonLeftSprite, new Vector2(-300, 130 + ((64 + 50) * 1)), null, null, null, "          tutorial", RobotoRegular20_2, "Left", Color.White));
            MainMenuButtonList.Add(new Button(ButtonLeftSprite, new Vector2(-300, 130 + ((64 + 50) * 2)), null, null, null, "          options", RobotoRegular20_2, "Left", Color.White));
            MainMenuButtonList.Add(new Button(ButtonLeftSprite, new Vector2(-300, 130 + ((64 + 50) * 3)), null, null, null, "          credits", RobotoRegular20_2, "Left", Color.White));
            MainMenuButtonList.Add(new Button(ButtonLeftSprite, new Vector2(-300, 1080 - 50 - 32), null, null, null, "          exit", RobotoRegular20_2, "Left", Color.White));

            MainMenuBackground = new StaticSprite("Backgrounds/MenuBackground", new Vector2(0, 0));
            MainMenuBackground.DrawDepth = 1f;

            foreach (Button button in MainMenuButtonList)
            {
                button.LoadContent();
            }

            #endregion

            #region Initialise Pause Menu
            PauseMenuNameList = new List<string>();
            PauseMenuNameList.Add("resume game");
            PauseMenuNameList.Add("options");
            PauseMenuNameList.Add("main menu");
            PauseMenuNameList.Add("profile menu");
            PauseMenuNameList.Add("exit");

            PauseButtonList = new List<Button>();
            for (int i = 0; i < 4; i++)
            {
                PauseButtonList.Add(new Button(ButtonLeftSprite, new Vector2(0, 130 + ((64 + 50) * i)), null, null, null, PauseMenuNameList[i], RobotoRegular20_2, "Left", Color.White));
                PauseButtonList[i].LoadContent();
            }

            PauseButtonList.Add(new Button(ButtonLeftSprite, new Vector2(0, 1080 - 50 - 32), null, null, null, PauseMenuNameList[4], RobotoRegular20_2, "Left", Color.White));
            PauseButtonList[4].LoadContent();
            #endregion

            #region Initialise Options Menu

            OptionsSFXUp = new Button(RightArrowSprite, new Vector2(640 + 32, 316));
            OptionsSFXUp.LoadContent();

            OptionsSFXDown = new Button(LeftArrowSprite, new Vector2(640 - 50 - 32, 316));
            OptionsSFXDown.LoadContent();

            OptionsMusicUp = new Button(RightArrowSprite, new Vector2(640 + 32, 380));
            OptionsMusicUp.LoadContent();

            OptionsMusicDown = new Button(LeftArrowSprite, new Vector2(640 - 50 - 32, 380));
            OptionsMusicDown.LoadContent();

            OptionsBack = new Button(ButtonLeftSprite, new Vector2(0, 1080 - 32 - 50), null, null, null, "back", RobotoRegular20_2, "Left", Color.White);
            OptionsBack.LoadContent();

            #endregion

            #region Initialise Profile Select Menu

            ProfileButtonList = new List<Button>();
            for (int i = 0; i < 4; i++)
            {
                ProfileButtonList.Add(new Button(ButtonLeftSprite, new Vector2(50, 130 + (i * 114)), null, null, null, "empty", RobotoRegular20_2, "Centre", Color.White));
                ProfileButtonList[i].LoadContent();
            }

            ProfileDeleteList = new List<Button>();
            for (int i = 0; i < 4; i++)
            {
                ProfileDeleteList.Add(new Button(SmallButtonSprite, new Vector2(0, 130 + (i * 114)), null, null, null, "X", RobotoRegular20_2, "Left", Color.White));
                ProfileDeleteList[i].LoadContent();
            }

            ProfileBackButton = new Button(ButtonRightSprite, new Vector2(1920 + 300, 1080 - 32 - 50), null, null, null, "back     ", RobotoRegular20_2, "Right", Color.White);
            ProfileBackButton.LoadContent();

            #endregion

            #region Initialise Profile Management Menu
            ProfileManagementTabs = new Tabs(new Vector2(0,0), WhiteBlock, RobotoRegular20_2, null, "loadout", "upgrades", "stats");

            ProfileManagementPlay = new Button(ButtonRightSprite, new Vector2(1920 - 450 + 300, 1080 - 32 - 50), null, null, null, "play     ", RobotoRegular20_2, "Right", Color.White);
            ProfileManagementPlay.LoadContent();

            ProfileManagementBack = new Button(ButtonLeftSprite, new Vector2(-300, 1080 - 32 - 50), null, null, null, "     back", RobotoRegular20_2, "Left", Color.White);
            ProfileManagementBack.LoadContent();

            //ProfileManagementHeader = new StaticSprite("Buttons/ProfileManagementHeader", new Vector2(0, 0));
            //ProfileManagementHeader.LoadContent(SecondaryContent);
            //ProfileMenuTitle = new StaticSprite("ProfileMenuTitle", new Vector2(0, 32));
            //ProfileMenuTitle.LoadContent(SecondaryContent);

            //ProfileManagementUpgrades = new Button(ButtonLeftSprite, new Vector2(100, 100), null, null, null, "Upgrades", ButtonFont);
            //ProfileManagementUpgrades.LoadContent();

            MoveTurretsRight = new Button(RightArrowSprite, new Vector2(1681, 258));
            MoveTurretsRight.LoadContent();

            MoveTurretsLeft = new Button(LeftArrowSprite, new Vector2(189, 258));
            MoveTurretsLeft.LoadContent();

            MoveTrapsRight = new Button(RightArrowSprite, new Vector2(1681, 660));
            MoveTrapsRight.LoadContent();

            MoveTrapsLeft = new Button(LeftArrowSprite, new Vector2(189, 660));
            MoveTrapsLeft.LoadContent();

            PlaceWeaponList = new List<Button>();
            for (int i = 0; i < 8; i++)
            {
                PlaceWeaponList.Add(new Button(SelectButtonSprite, new Vector2(565 + (i * 100), 900), null, new Vector2(1f, 1f), null, "", null, "Left", null, true));
                //PlaceWeaponList[i].NextScale = new Vector2(0.35f, 0.35f);
                PlaceWeaponList[i].LoadContent();
            }

            MenuTower = new StaticSprite("Tower", new Vector2(1920 - 300, 150));
            MenuTower.LoadContent(SecondaryContent);

            #endregion

            #region Initialise Get Name Menu
            NameInput = new TextInput(new Vector2(1920 / 2 - 215, 1080 / 2 - 40), 350, RobotoRegular20_2, Color.White);
            //NameInput.LoadContent(SecondaryContent);

            GetNameBack = new Button(ButtonLeftSprite, new Vector2(0, 1080 - 32 - 50), null, null, null, "     back", RobotoRegular20_2, "Left", Color.White);
            GetNameBack.CurrentPosition.X = -300;
            GetNameBack.LoadContent();

            GetNameOK = new Button(ButtonRightSprite, new Vector2(1920 - 450, 1080 - 32 - 50), null, null, null, "create     ", RobotoRegular20_2, "Right", Color.White);
            GetNameOK.LoadContent();

            TextBox = new StaticSprite("Buttons/TextBox", new Vector2((1920 / 2) - 225, (1080 / 2) - 50));
            TextBox.LoadContent(SecondaryContent);

            #endregion

            #region Initialise Upgrades Menu
            //UpgradesBack = new Button(ButtonLeftSprite, new Vector2(0, 1080 - 32 - 50), null, null, null, "Back", ButtonFont, "Left");
            //UpgradesBack.LoadContent();

            //UpgradeButtonList = new List<Button>();
            //for (int i = 0; i < 10; i++)
            //{
            //    UpgradeButtonList.Add(new Button(WeaponBoxSprite, new Vector2(100 + (i * 92), 100), null, new Vector2(0.5f, 0.5f), Color.White, "Poop", ButtonFont, "Left", Color.White, false));
            //    UpgradeButtonList[i].NextScale = new Vector2(0.5f, 0.5f);
            //    UpgradeButtonList[i].LoadContent();
            //}
            #endregion            

            LoadingAnimation = new AnimatedSprite("LoadingAnimation", new Vector2(1920 / 2 - 65, 1080 / 2 - 65), new Vector2(131, 131), 17, 30, HalfWhite, Vector2.One, true);
            LoadingAnimation.LoadContent(SecondaryContent);
            IsLoading = false;

            base.Initialize();
        }

        protected override void Draw(GameTime gameTime)
        {
            #region Get real position of mouse
            if (CurrentSettings.FullScreen == false)
            {
                MouseTransform = Matrix.CreateTranslation(new Vector3(Mouse.GetState().X, Mouse.GetState().Y, 0)) *
                                 Matrix.CreateTranslation(new Vector3(0, 0, 0)) *
                                 Matrix.CreateScale(new Vector3((float)(ResolutionOffsetRatio / 2), (float)(ResolutionOffsetRatio / 2), 1));
            }
            else
            {
                MouseTransform = Matrix.CreateTranslation(new Vector3(0, -(ActualResolution.Y - CurrentSettings.ResHeight) / 2, 0)) *
                                 Matrix.CreateScale(new Vector3((float)(ResolutionOffsetRatio), (float)(ResolutionOffsetRatio), 1));

            }

            Vector2 worldMouse = Vector2.Transform(new Vector2(Mouse.GetState().X, Mouse.GetState().Y), MouseTransform);

            CursorPosition.X = worldMouse.X;
            CursorPosition.Y = worldMouse.Y;
            #endregion

            GraphicsDevice.SetRenderTarget(MenuRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            #region Draw menus
            
            if (GameState != GameState.Playing)
            {
                spriteBatch.Begin();
                if (GameState != GameState.Paused && GameState != GameState.Victory)
                    MainMenuBackground.Draw(GraphicsDevice, QuadEffect);
                spriteBatch.End();

                #region Draw Profile Select Menu
                if (GameState == GameState.ProfileSelect)
                {
                    spriteBatch.Begin();
                        foreach (Button button in ProfileButtonList)
                        {
                            button.Draw(spriteBatch);
                        }

                        foreach (Button button in ProfileDeleteList)
                        {
                            button.Draw(spriteBatch);
                        }

                        ProfileBackButton.Draw(spriteBatch);
                    spriteBatch.End();
                }
                #endregion

                #region Draw Profile Managment Screen
                if (GameState == GameState.ProfileManagement)
                {
                    spriteBatch.Begin();
                        //ProfileMenuTitle.Draw(spriteBatch);


                        //spriteBatch.DrawString(RobotoRegular20_2, CurrentProfile.Name, new Vector2(16, 16), HalfWhite);
                        //ProfileManagementHeader.Draw(spriteBatch);

                        ProfileManagementTabs.Draw(spriteBatch, QuadEffect, GraphicsDevice);

                        switch (ProfileManagementState)
                        {
                                                                                                                                            #region Loadout screen
                        case ProfileManagementState.Loadout:
                            spriteBatch.DrawString(RobotoRegular20_2, "turrets", new Vector2(158 + 113, 105 - 32), Color.White);
                            spriteBatch.DrawString(RobotoRegular20_2, "traps", new Vector2(158 + 113, 120 + 130 + 60 + 200 - 36), Color.White);
                            spriteBatch.DrawString(RobotoRegular20_2, "selected weapons", new Vector2(118, 1080 - 224), Color.White);

                            MoveTurretsRight.Draw(spriteBatch);
                            MoveTurretsLeft.Draw(spriteBatch);

                            MoveTrapsRight.Draw(spriteBatch);
                            MoveTrapsLeft.Draw(spriteBatch);

                            #region Draw the boxes to select turrets from
                            foreach (WeaponBox turretBox in SelectTurretList)
                            {
                                turretBox.Draw(spriteBatch, GraphicsDevice, QuadEffect);
                            }
                            #endregion

                            #region Draw the boxes to select traps from
                            foreach (WeaponBox trapBox in SelectTrapList)
                            {
                                trapBox.Draw(spriteBatch, GraphicsDevice, QuadEffect);
                            }
                            #endregion

                            #region Draw the slots where weapons are placed
                            foreach (Button button in PlaceWeaponList)
                            {
                                button.Draw(spriteBatch);
                            }
                            #endregion

                            break;
                        #endregion

                                        #region Stats screen
                        case ProfileManagementState.Stats:

                            break;
                        #endregion

                                        #region Upgrades screen
                        case ProfileManagementState.Upgrades:

                            break;
                        #endregion
                        }

                        ProfileManagementPlay.Draw(spriteBatch);
                        ProfileManagementBack.Draw(spriteBatch);
                    spriteBatch.End();
                }
                #endregion

                #region Draw Loading Screen
                if (GameState == GameState.Loading)
                {
                    spriteBatch.Begin();
                        if (AllLoaded == false)
                            LoadingAnimation.Draw(spriteBatch);

                        spriteBatch.DrawString(RobotoBold40_2, AllLoaded.ToString(), new Vector2(100, 100), Color.White);

                        if (LoadingContentString != null)
                            spriteBatch.DrawString(RobotoBold40_2, LoadingContentString, new Vector2(200, 200), Color.White);
                    spriteBatch.End();
                }
                #endregion

                #region Draw Main Menu
                if (GameState == GameState.Menu)
                {
                    spriteBatch.Begin();
                        foreach (Button button in MainMenuButtonList)
                        {
                            button.Draw(spriteBatch);
                        }
                    spriteBatch.End();
                }
                #endregion

                #region Draw Options Menu
                if (GameState == GameState.Options)
                {
                    spriteBatch.Begin();
                        OptionsSFXUp.Draw(spriteBatch);
                        OptionsSFXDown.Draw(spriteBatch);

                        OptionsMusicUp.Draw(spriteBatch);
                        OptionsMusicDown.Draw(spriteBatch);

                        OptionsBack.Draw(spriteBatch);

                        string SFXVol, MusicVol;
                        SFXVol = MenuSFXVolume.ToString();
                        MusicVol = MenuMusicVolume.ToString();

                        if (SFXVol.Length == 1)
                            SFXVol = SFXVol.Insert(0, "0");

                        if (MusicVol.Length == 1)
                            MusicVol = MusicVol.Insert(0, "0");

                        spriteBatch.DrawString(DefaultFont, SFXVol, new Vector2(640 - 16, 320), Color.White);
                        spriteBatch.DrawString(DefaultFont, MusicVol, new Vector2(640 - 16, 384), Color.White);
                    spriteBatch.End();
                }
                #endregion

                #region Draw GetName Menu
                if (GameState == GameState.GettingName)
                {
                    spriteBatch.Begin();
                        spriteBatch.DrawString(RobotoRegular20_2, "profile name", 
                                               new Vector2(TextBox.Position.X, TextBox.Position.Y - DefaultFont.MeasureString("E").Y), 
                                               Color.White);
                        TextBox.Draw(spriteBatch);

                        GetNameBack.Draw(spriteBatch);
                        GetNameOK.Draw(spriteBatch);

                        NameInput.Draw(spriteBatch);
                    spriteBatch.End();
                }
                #endregion

                #region Draw menu Dialog boxes
                if (GameState != GameState.Loading)
                {
                    spriteBatch.Begin();
                    if (ExitDialog != null)
                    {
                        ExitDialog.Draw(spriteBatch);
                    }

                    if (DeleteProfileDialog != null)
                    {
                        DeleteProfileDialog.Draw(spriteBatch);
                    }

                    if (NoWeaponsDialog != null)
                    {
                        NoWeaponsDialog.Draw(spriteBatch);
                    }

                    if (NameLengthDialog != null)
                    {
                        NameLengthDialog.Draw(spriteBatch);
                    }
                    spriteBatch.End();
                }
                #endregion

                spriteBatch.Begin();
                CursorDraw(spriteBatch);
                spriteBatch.End();
                //spriteBatch.Draw(MenuFade, ScreenRectangle, MenuColor);

                #region Draw pause menu buttons
                if (GameState == GameState.Paused)
                {
                    spriteBatch.Begin();
                        foreach (Button button in PauseButtonList)
                        {
                            button.Draw(spriteBatch);
                        }
                    spriteBatch.End();
                }
                #endregion

                //Unexpected error, might need to move this out again if it breaks things
                //spriteBatch.End();
            }
            #endregion

            //This line is to stop the "unexpected error has occurred" bug
            if (GameState != GameState.Loading)
            {
                GraphicsDevice.SetRenderTarget(GameRenderTarget);
                GraphicsDevice.Clear(Color.Transparent);

                #region Draw things in game that SHOULD be shaken - Non-diegetic elements
                if (GameState == GameState.Playing || GameState == GameState.Paused || GameState == GameState.Victory && IsLoading == false)
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred,
                                      BlendState.AlphaBlend,
                                      null, null, null, null,
                                      Camera.Transformation(GraphicsDevice));

                    SkyBackground.Draw(spriteBatch);
                    Ground.Draw(spriteBatch);

                    foreach (Decal decal in DecalList)
                    {
                        decal.Draw(spriteBatch);
                    }

                    //FocusedEmitter.Draw(spriteBatch);

                    Tower.Draw(spriteBatch);

                    foreach (Emitter emitter in EmitterList2)
                    {
                        emitter.Draw(spriteBatch);
                    }

                    //BlurryEmitter.Draw(spriteBatch);

                    spriteBatch.End();
                }
                #endregion

                #region Draw stuff that SHOULD be shaken with ADDITIVE blending
                if (GameState == GameState.Playing || GameState == GameState.Paused || GameState == GameState.Victory && IsLoading == false)
                {
                    spriteBatch.Begin(SpriteSortMode.Immediate,
                                      BlendState.Additive,
                                      null, null, null, null,
                                      Camera.Transformation(GraphicsDevice));

                    foreach (Emitter emitter in AlphaEmitterList)
                    {
                        emitter.Draw(spriteBatch);
                    }
                    spriteBatch.End();
                }
                #endregion

                #region Draw things sorted according to their Y value - To create depth illusion... Also can be shaken
                //This second spritebatch sorts everthing Back to Front, 
                //to make sure that the invaders are drawn correctly according to their Y value
                if (GameState == GameState.Playing || GameState == GameState.Paused || GameState == GameState.Victory && IsLoading == false)
                {
                    spriteBatch.Begin(
                        SpriteSortMode.FrontToBack,
                        BlendState.AlphaBlend,
                        null, null, null, null,
                        Camera.Transformation(GraphicsDevice));

                    if (PowerupDelivery != null)
                    {
                        PowerupDelivery.Draw(spriteBatch);
                    }

                    foreach (Particle shellCasing in ShellCasingList)
                    {
                        shellCasing.Draw(spriteBatch);
                        //shellCasing.DrawShadow(spriteBatch);
                    }

                    foreach (ShellCasing shell in VerletShells)
                    {
                        shell.Draw(spriteBatch);
                    }

                    foreach (Emitter emitter in YSortedEmitterList)
                    {
                        emitter.Draw(spriteBatch);
                    }

                    foreach (GrassBlade blade in GrassBladeList)
                    {
                        blade.Draw(spriteBatch);
                    }

                    #region Draw turrets
                    foreach (Turret turret in TurretList)
                    {
                        if (turret != null && turret.Active == true)
                        {
                            turret.Draw(spriteBatch);
                        }
                    }
                    #endregion

                    GraphicsDevice.BlendState = BlendState.AlphaBlend;

                    #region Draw invaders
                    foreach (Invader invader in InvaderList)
                    {
                        invader.Draw(spriteBatch, GraphicsDevice, QuadEffect);
                    }
                    #endregion

                    #region Draw traps
                    foreach (Trap trap in TrapList)
                    {
                        trap.Draw(spriteBatch);
                    }
                    #endregion

                    #region Draw heavy projectiles
                    foreach (HeavyProjectile heavyProjectile in HeavyProjectileList)
                    {
                        if (heavyProjectile.HeavyProjectileType != HeavyProjectileType.FelProjectile)
                        {
                            heavyProjectile.Draw(spriteBatch);
                        }
                    }

                    foreach (HeavyProjectile heavyProjectile in InvaderHeavyProjectileList)
                    {
                        heavyProjectile.Draw(spriteBatch);
                    }

                    foreach (TimerHeavyProjectile timedProjectile in TimedProjectileList)
                    {
                        timedProjectile.Draw(spriteBatch);
                    }
                    #endregion

                    if (CurrentSpecialAbility != null)
                        CurrentSpecialAbility.Draw(spriteBatch);

                    //There was a disposed object error here and I don't know why
                    //Seems to have something to do with the turrets
                    //EDIT: OK, seems to have been to do with the verlet shells - keep an eye on it though
                    spriteBatch.End();
                }
                #endregion

                #region Draw with additive blending - Makes stuff look glowy
                if (GameState == GameState.Playing || GameState == GameState.Paused || GameState == GameState.Victory && IsLoading == false)
                {
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null);
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    foreach (LightningBolt lightningBolt in LightningList)
                    {
                        lightningBolt.Draw(spriteBatch);
                    }

                    foreach (BulletTrail trail in TrailList)
                    {
                        trail.Draw(spriteBatch);
                    }

                    foreach (BulletTrail trail in InvaderTrailList)
                    {
                        trail.Draw(spriteBatch);
                    }

                    foreach (Particle coin in CoinList)
                    {
                        coin.Draw(spriteBatch);
                    }

                    foreach (HeavyProjectile heavyProjectile in HeavyProjectileList)
                    {
                        if (heavyProjectile.HeavyProjectileType == HeavyProjectileType.FelProjectile)
                            heavyProjectile.Draw(spriteBatch);
                    }

                    foreach (Emitter emitter in AdditiveEmitterList)
                    {
                        emitter.Draw(spriteBatch);
                    }
                    spriteBatch.End();
                }
                #endregion

                #region Draw stuff that on top of the game (Not UI) that isn't additive blended and not y-sorted
                spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);
                //if (GameState == GameState.Playing || GameState == GameState.Paused || GameState == GameState.Victory && IsLoading == false)
                //{
                //  foreach (StaticSprite weatherSprite in WeatherSpriteList)
                //    {
                //        weatherSprite.Draw(spriteBatch);
                //    }
                //}
                spriteBatch.End();
                #endregion
            }

            GraphicsDevice.SetRenderTarget(UIRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);

            #region Draw the UI elements
            if (GameState == GameState.Playing || GameState == GameState.Paused || GameState == GameState.Victory && IsLoading == false)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null);

                #region Draw diagnostics
                if (Diagnostics == true)
                {
                    spriteBatch.DrawString(DefaultFont, Slow.ToString(), Vector2.Zero, Color.Red);
                    spriteBatch.DrawString(DefaultFont, CurrentProfile.LevelNumber.ToString(), new Vector2(100, 200), Color.Purple);
                    spriteBatch.DrawString(DefaultFont, "Seconds:" + Seconds.ToString(), new Vector2(0, 16), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "Resources:" + Resources.ToString(), new Vector2(0, 32), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "CurrentWaveIndex:" + (CurrentWaveIndex + 1).ToString() + "/" + MaxWaves, new Vector2(0, 48), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "ShieldOn:" + Tower.ShieldOn.ToString(), new Vector2(0, 64), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "CurrentShieldTime:" + Tower.CurrentShieldTime.ToString(), new Vector2(0, 80), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "CurrentWaveTime:" + CurrentWaveTime.ToString(), new Vector2(0, 96), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "HeavyProjectiles:" + HeavyProjectileList.Count.ToString(), new Vector2(0, 112), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "LightProjectiles:" + LightProjectileList.Count.ToString(), new Vector2(0, 124), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "TrailList:" + TrailList.Count.ToString(), new Vector2(0, 136), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "Emitter1:" + YSortedEmitterList.Count.ToString(), new Vector2(0, 148), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "Emitter2:" + EmitterList2.Count.ToString(), new Vector2(0, 160), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "InvaderHeavy:" + InvaderHeavyProjectileList.Count.ToString(), new Vector2(0, 172), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "Invaders:" + InvaderList.Count.ToString(), new Vector2(0, 184), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "Particles:" + TotalParticles.ToString(), new Vector2(0, 200), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "Shells:" + ShellCasingList.Count.ToString(), new Vector2(0, 216), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "Lightning:" + LightningList.Count.ToString(), new Vector2(0, 232), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, gameTime.ElapsedGameTime.ToString(), new Vector2(1100, 0), Color.Red);
                    spriteBatch.DrawString(DefaultFont, CurrentSettings.TimesPlayed.ToString(), new Vector2(0, 248), Tower.Color);
                    spriteBatch.DrawString(DefaultFont, "InvaderTime:" + CurrentInvaderTime.ToString(), new Vector2(0, 248 + 16), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "WaveTime:" + CurrentWaveTime.ToString(), new Vector2(0, 248 + 32), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "PauseTime:" + CurrentWavePauseTime, new Vector2(0, 248 + 32 + 16), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "UIQuickInfo:" + UITrapQuickInfoList.Count.ToString(), new Vector2(0, 248 + 64), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "UITrapOutlines:" + UITrapOutlineList.Count.ToString(), new Vector2(0, 248 + 64 + 16), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "UITurretOutlines:" + UITurretOutlineList.Count.ToString(), new Vector2(0, 248 + 64 + 32), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "WeatherSprites:" + WeatherSpriteList.Count.ToString(), new Vector2(0, 248 + 64 + 32 + 16), Color.Lime);
                    
                    spriteBatch.DrawString(DefaultFont, (gameTime.ElapsedGameTime.TotalMilliseconds).ToString(), new Vector2(1920 - 50, 0), Color.Red);
                    FPSCounter.Draw(spriteBatch);
                }
                #endregion
                //Draw the damage numbers for when the invaders are damaged
                foreach (NumberChange numChange in NumberChangeList)
                {
                    numChange.Draw(spriteBatch); 
                }

                spriteBatch.Draw(CurrencyIcon, new Rectangle(485, 1080 - 52, 24, 24), Color.White);
                spriteBatch.DrawString(RobotoRegular20_0, Resources.ToString(), new Vector2(552 - RobotoRegular20_0.MeasureString(Resources.ToString()).X, 1080 - 50), Color.White);

                #region Draw the player status info - health/shiled bars, turret and invader info
                spriteBatch.Draw(HealthIcon, new Rectangle((int)HealthBar.Position.X - 4, (int)HealthBar.Position.Y, HealthIcon.Width, HealthIcon.Height), null, Color.White, 0, new Vector2(HealthIcon.Width / 2, HealthIcon.Height / 2), SpriteEffects.None, 0);

                foreach (EffectPass pass in QuadEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    HealthBar.Draw(GraphicsDevice);
                    ShieldBar.Draw(GraphicsDevice);
                }

                if (CurrentTurret != null && UITurretInfo != null)
                    UITurretInfo.Draw(spriteBatch, GraphicsDevice);

                //if (UIInvaderInfo != null)
                //    UIInvaderInfo.Draw(spriteBatch, GraphicsDevice);
                #endregion

                #region Drawing buttons
                if (StartWaveButton != null)
                    StartWaveButton.Draw(spriteBatch);

                foreach (Button button in SpecialAbilitiesButtonList)
                {
                    button.Draw(spriteBatch);
                }

                foreach (CooldownButton button in CooldownButtonList)
                {
                    button.Draw(QuadEffect, GraphicsDevice);
                }

                //Draw tooltips for the buttons
                if (InGameInformation != null)
                    InGameInformation.Draw(spriteBatch);
                #endregion                

                #region Draw turret bars
                foreach (Turret turret in TurretList)
                {
                    if (turret != null)
                    {
                        if (turret.Active == true)
                        {
                            turret.DrawBars(GraphicsDevice, QuadEffect);
                        }
                    }
                }
                #endregion

                #region Draw trap bars
                foreach (Trap trap in TrapList)
                {
                    trap.DrawBars(GraphicsDevice, QuadEffect);
                }
                #endregion

                #region Draw Powerup delivery bars
                if (PowerupDelivery != null)
                {
                    if (PowerupDelivery.HealthBar != null)
                        PowerupDelivery.DrawBars(GraphicsDevice, QuadEffect);
                }
                #endregion

                #region Draw UIPowerup Icons
                foreach (UIPowerupIcon uiPowerup in UIPowerupsList)
                {
                    uiPowerup.Draw(spriteBatch, GraphicsDevice, QuadEffect);
                }

                foreach (UIPowerupInfo uiPowerupInfo in UIPowerupsInfoList)
                {
                    uiPowerupInfo.Draw(spriteBatch, GraphicsDevice, QuadEffect);
                }

                UIPowerupsList.RemoveAll(UIPowerup => UIPowerup.Active == false);
                #endregion

                #region Draw outlines around traps when moused-over
                foreach (Trap trap in TrapList)
                {
                    if (trap != null && trap.DestinationRectangle.Contains(VectorToPoint(CursorPosition)))
                    {
                        foreach (UIOutline trapOutline in UITrapOutlineList)
                        {
                            if (trapOutline.Trap == trap)
                                trapOutline.Draw(spriteBatch);
                        }
                    }
                }
                #endregion

                #region Draw outlines around turrets when moused-over
                foreach (Turret turret in TurretList)
                {
                    if (turret != null && turret.SelectBox.Contains(VectorToPoint(CursorPosition)))
                    {
                        foreach (UIOutline turretOutline in UITurretOutlineList)
                        {
                            turretOutline.Draw(spriteBatch);
                        }
                    }
                }
                #endregion

                #region Draw outlines around invaders when moused over
                //foreach (Invader invader in InvaderList)
                //{
                //    if (invader != null && invader.DestinationRectangle.Contains(VectorToPoint(CursorPosition)))
                //    {
                //        foreach (UIOutline invaderOutline in UIInvaderOutlineList)
                //        {
                //            invaderOutline.Draw(spriteBatch);
                //        }
                //    }
                //}
                #endregion

                if (GameState != GameState.Paused)
                    StoryDialogueBox.Draw(spriteBatch, GraphicsDevice, QuadEffect);

                //if (UIWeaponInfo != null)
                foreach (UIWeaponInfoTip uiWeaponInfo in UIWeaponInfoList)
                {
                    if (uiWeaponInfo != null)
                    uiWeaponInfo.Draw(spriteBatch, GraphicsDevice, QuadEffect);
                }

                foreach (UITrapQuickInfo uiTrapQuickInfo in UITrapQuickInfoList)
                {
                    uiTrapQuickInfo.Draw(GraphicsDevice, QuadEffect, spriteBatch);
                }
                spriteBatch.End();

                #region Draw UI stuff that needs additive blending
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                foreach (Button towerSlot in TowerButtonList)
                {
                    towerSlot.Draw(spriteBatch);
                }
                spriteBatch.End();
                #endregion
            }

            
            #region Draw pause menu Dialog boxes
            if (GameState == GameState.Paused)
            {
                spriteBatch.Begin();

                //if (ExitDialog != null)
                //{
                //    ExitDialog.Draw(spriteBatch);
                //}

                if (MainMenuDialog != null)
                {
                    MainMenuDialog.Draw(spriteBatch);
                }

                if (ProfileMenuDialog != null)
                {
                    ProfileMenuDialog.Draw(spriteBatch);
                }
                //There's been an unexpected error here a few times. 
                //Moved it inside the "If GameState" check to see if it helps?
                spriteBatch.End();
            }
            #endregion

            

            #region Draw the victory screen
            if (GameState == GameState.Victory)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(WhiteBlock, new Rectangle(665, 180, 590, 720), null, Color.Lerp(Color.Black, Color.Transparent, 0.5f), 0, new Vector2(0, 0), SpriteEffects.None, 1);
                spriteBatch.DrawString(RobotoRegular40_2, "mission completed", new Vector2(1920 / 2, 210), Color.White, 0, RobotoBold40_2.MeasureString("mission completed") / 2, 1, SpriteEffects.None, 0);
                VictoryContinue.Draw(spriteBatch);
                spriteBatch.End();
            }
            #endregion

            #region Draw the cursor on top of EVERYTHING
            if (GameState != GameState.Loading)
            {
                spriteBatch.Begin();
                CursorDraw(spriteBatch);
                spriteBatch.End();
            }

            if (GameState == GameState.Playing && CurrentTurret != null)
            {
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.LinearWrap,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, HealthBarEffect);

                HealthBarEffect.Parameters["meterValue"].SetValue(((CurrentTurret.MaxHeat / 100) * CurrentTurret.CurrentHeat) / 100);
                spriteBatch.Draw(HealthBarSprite, new Vector2(CursorPosition.X, CursorPosition.Y), null, Color.White, MathHelper.ToRadians(-90),
                           new Vector2(HealthBarSprite.Width / 2, HealthBarSprite.Height / 2), 1f, SpriteEffects.None, 0);

                //HealthBarEffect.Parameters["meterValue"].SetValue((((float)CurrentTurret.ElapsedTime / 100) * (float)CurrentTurret.FireDelay) / 100);
                //spriteBatch.Draw(HealthBarSprite, new Vector2(CursorPosition.X, CursorPosition.Y), null, Color.White, MathHelper.ToRadians(-90),
                //           new Vector2(HealthBarSprite.Width / 2, HealthBarSprite.Height / 2), 1f, SpriteEffects.FlipVertically, 0);

                spriteBatch.End();
            }
            #endregion
            #endregion

            ScreenTex = GameRenderTarget;

            #region Draw the crepusuclar rays
            //GraphicsDevice.SetRenderTarget(CrepuscularMap);
            //GraphicsDevice.Clear(Color.Black);

            //if (GameState == GameState.Playing || GameState == GameState.Paused || GameState == GameState.Victory && IsLoading == false)
            //{
            //    spriteBatch.Begin();
            //    spriteBatch.Draw(FlareMap1, new Rectangle(0, 0, 1920, 1080), Color.White);
            //    spriteBatch.Draw(GameRenderTarget, new Rectangle(0, 0, 1920, 1080), Color.Black);
            //    spriteBatch.End();
            //}
            #endregion

            #region Apply pixel shaders to game world
            //Layer up all the shaders that need to be applied to the game world - Not including the interface
            if (GameState == GameState.Playing || GameState == GameState.Paused)
            {
                ShieldBubbleEffect.Parameters["ScreenTexture"].SetValue(GameRenderTarget);
                ShockWaveEffect.Parameters["ScreenTexture"].SetValue(GameRenderTarget);

                if (Tower.CurrentShield > 0)
                {
                    ScreenTex = HandleShaders(GameRenderTarget
                                              , ShockWaveEffect.CurrentTechnique.Passes[0]
                                              //, ShieldBubbleEffect.CurrentTechnique.Passes[0]
                                              );
                }
                else
                {
                    ScreenTex = HandleShaders(GameRenderTarget
                                              , ShockWaveEffect.CurrentTechnique.Passes[0]
                                              );
                }
            }
            #endregion

            
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            #region DRAW ACTUAL GAME
            if (GameState == GameState.Playing || GameState == GameState.Paused || GameState == GameState.Victory)
            {
                if (ScreenTex != null)
                {
                    //CrepuscularEffect.Parameters["ColorMap"].SetValue(ScreenTex);
                    targetBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null);
                    //CrepuscularEffect.CurrentTechnique.Passes[0].Apply();
                    targetBatch.Draw(ScreenTex, new Rectangle(0, (int)(ActualResolution.Y - CurrentSettings.ResHeight) / 2,
                                     CurrentSettings.ResWidth, CurrentSettings.ResHeight), null, Color.White, 0, Vector2.Zero,
                                     SpriteEffects.None, 0);
                    targetBatch.DrawString(DefaultFont, Error, new Vector2(100, 100), Color.Red);
                    targetBatch.End();
                }
            }
            #endregion

            #region DRAW THE MENU ITEMS
            //Check loading screen animation state
            if (GameState != GameState.Playing)
            {
                if (MenuRenderTarget != null)
                {
                    //Keeps getting an unexpected error here. Might need to make another render target just for the loading screen
                    targetBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                    targetBatch.Draw(MenuRenderTarget, new Rectangle(0, (int)(ActualResolution.Y - CurrentSettings.ResHeight) / 2,
                                     CurrentSettings.ResWidth, CurrentSettings.ResHeight), null, Color.White, 0, Vector2.Zero,
                                     SpriteEffects.None, 0);
                    targetBatch.DrawString(DefaultFont, Error, new Vector2(100, 100), Color.Red);
                    targetBatch.End();
                }
            }
            #endregion

            #region DRAW THE UI ITEMS, MOUSE, ETC.
            if (GameState == GameState.Playing ||
                GameState == GameState.Victory ||
                GameState == GameState.Paused &&
                UIRenderTarget != null)
            {
                targetBatch.Begin();
                targetBatch.Draw(UIRenderTarget, new Rectangle(0, (int)(ActualResolution.Y - CurrentSettings.ResHeight) / 2,
                                 CurrentSettings.ResWidth, CurrentSettings.ResHeight), null, Color.White, 0, Vector2.Zero,
                                 SpriteEffects.None, 0);
                targetBatch.DrawString(DefaultFont, Error, new Vector2(100, 100), Color.Red);
                targetBatch.End();
            }
            #endregion

            base.Draw(gameTime);
        }

        protected override void Update(GameTime gameTime)
        {
            //This is just the stuff that needs to be updated every step
            //This is where I call all the smaller procedures that I broke the update into   
            CurrentMouseState = Mouse.GetState();
            CurrentKeyboardState = Keyboard.GetState();
            
            Slow = gameTime.IsRunningSlowly;

            FPSCounter.Update(gameTime);

            if (SelectedTurret != null)
                SelectedTrap = null;

            if (SelectedTrap != null)
                SelectedTurret = null;

            #region Open pause menu
            if (CurrentMouseState.LeftButton == ButtonState.Released &&
                CurrentKeyboardState.IsKeyUp(Keys.Escape) &&
                PreviousKeyboardState.IsKeyDown(Keys.Escape))
            {
                switch (GameState)
                {
                    case GameState.Playing:
                        GameState = GameState.Paused;
                        break;

                    case GameState.Paused:
                        GameState = GameState.Playing;
                        break;
                }
            }
            #endregion

            #region In game update
            if (GameState == GameState.Playing && IsLoading == false)
            {
                #region Show diagnostics
                if (CurrentMouseState.LeftButton == ButtonState.Released &&
                   CurrentKeyboardState.IsKeyUp(Keys.F1) &&
                   PreviousKeyboardState.IsKeyDown(Keys.F1))
                {
                    switch (Diagnostics)
                    {
                        case false:
                            Diagnostics = true;
                            break;

                        case true:
                            Diagnostics = false;
                            break;
                    }
                }
                #endregion

                #region TEST - Create powerup delivery
                if (CurrentMouseState.LeftButton == ButtonState.Released &&
                    CurrentKeyboardState.IsKeyUp(Keys.Enter) &&
                    PreviousKeyboardState.IsKeyDown(Keys.Enter))
                {
                    PowerupDelivery = new PowerupDelivery(PowerupDeliveryPod, new Vector2(300, -32), BallParticle, PowerupDeliveryFin);
                    PowerupDelivery.Powerup = new Powerup(6000, new PowerupEffect() { GeneralPowerup = GeneralPowerup.ExplosivePower, PowerupValue = 30 });
                }
                #endregion

                #region TEST- Use special ability
                if (CurrentKeyboardState.IsKeyDown(Keys.E) &&
                    PreviousKeyboardState.IsKeyUp(Keys.E))
                {
                    CurrentSpecialAbility = new AirStrikeSpecial();
                }
                
                if (CurrentSpecialAbility != null)
                    CurrentSpecialAbility.Update(gameTime);
                #endregion

                Seconds += gameTime.ElapsedGameTime.TotalMilliseconds;

                HealthBar.Update(Tower.MaxHP, Tower.CurrentHP, gameTime);
                ShieldBar.Update(Tower.MaxShield, Tower.CurrentShield, gameTime);

                Tower.Update(gameTime);

                //InvaderUpdate(gameTime);
                
                RightClickClearSelected();

                SelectButtonsUpdate(gameTime);
                TowerButtonUpdate(gameTime);
                SpecialAbilitiesButtonUpdate(gameTime);

                AttackTower();
                AttackTraps();

                RangedAttackTower(gameTime);
                RangedAttackTraps();

                HandleWaves(gameTime);

                if (WaveCountDown != null && WaveCountDown.CurrentSeconds > -1)
                    WaveCountDown.Update(gameTime);

                if (GameState != GameState.Playing)
                    return;

                if (InGameInformation != null)
                    InGameInformation.Update(CursorPosition, gameTime);

                if (StoryDialogueBox != null)
                    StoryDialogueBox.Update(gameTime);

                foreach (Powerup powerup in PowerupsList)
                {
                    powerup.Update(gameTime);
                }

                PowerupsList.RemoveAll(Powerup => Powerup.Active == false);

                #region Handle Particle Emitters and Particles
                UpdateWeather(gameTime);

                foreach (ShellCasing shell in VerletShells)
                {
                    shell.Update(gameTime);
                }

                VerletShells.RemoveAll(Shell => Shell.Active == false);

                foreach (GrassBlade blade in GrassBladeList)
                {
                    blade.Update(gameTime);
                }

                #region Handle EmitterList2
                foreach (Emitter emitter in EmitterList2)
                {
                    emitter.Update(gameTime);
                }

                EmitterList2.RemoveAll(Emitter => Emitter.Active == false);
                EmitterList2.RemoveAll(Emitter => Emitter.AddMore == false && Emitter.ParticleList.Count == 0);
                #endregion

                #region Handle EmitterList
                foreach (Emitter emitter in YSortedEmitterList)
                {
                    emitter.Update(gameTime);
                }

                YSortedEmitterList.RemoveAll(Emitter => Emitter.Active == false);
                YSortedEmitterList.RemoveAll(Emitter => Emitter.AddMore == false && Emitter.ParticleList.Count == 0);
                #endregion

                #region Handle AlphaEmitterList
                foreach (Emitter emitter in AlphaEmitterList)
                {
                    emitter.Update(gameTime);
                }

                AlphaEmitterList.RemoveAll(Emitter => Emitter.Active == false);
                AlphaEmitterList.RemoveAll(Emitter => Emitter.AddMore == false && Emitter.ParticleList.Count == 0);
                #endregion

                #region Handle AdditiveEmitterList
                foreach (Emitter emitter in AdditiveEmitterList)
                {
                    emitter.Update(gameTime);
                }

                AdditiveEmitterList.RemoveAll(Emitter => Emitter.Active == false);
                AdditiveEmitterList.RemoveAll(Emitter => Emitter.AddMore == false && Emitter.ParticleList.Count == 0);
                #endregion

                #region Handle GasEmitterList
                foreach (Emitter emitter in GasEmitterList)
                {
                    emitter.Update(gameTime);
                }

                GasEmitterList.RemoveAll(Emitter => Emitter.Active == false);
                GasEmitterList.RemoveAll(Emitter => Emitter.AddMore == false && Emitter.ParticleList.Count == 0);
                #endregion


                //This just handles the shell casings which aren't created by an emitter, they are just in a list
                //Because they have to only be emitted every time the Machine Gun is fired
                foreach (Particle shellCasing in ShellCasingList)
                {
                    shellCasing.Update(gameTime);

                    #region This makes the shell casings bounce when an explosion happens near them
                    foreach (Explosion explosion in ExplosionList)
                    {
                        foreach (Particle Casing in ShellCasingList)
                        {
                            //Vector distance between invader and explosion
                            float Dist = MathHelper.Distance(Casing.DestinationRectangle.Center.X, explosion.Position.X);

                            //List of X values between the casing and the explosion      

                            //[----------dist-----------]
                            //cas--------wall--------expl
                            var InvaderToExplosion = Enumerable.Range((int)Casing.DestinationRectangle.Center.X, (int)Dist);

                            //[----------dist-----------]
                            //expl-------wall---------cas
                            var ExplosionToInvader = Enumerable.Range((int)explosion.Position.X, (int)Dist);

                            List<Trap> TempList = new List<Trap>();
                            TempList = TrapList.FindAll(Trap => Trap.TrapType == TrapType.Wall);

                            if (!TempList.Any(Trap => ExplosionToInvader.Contains(Trap.DestinationRectangle.Center.X)) &&
                                !TempList.Any(Trap => InvaderToExplosion.Contains(Trap.DestinationRectangle.Center.X)) &&
                                Dist < explosion.BlastRadius)
                            {
                                if (Casing.BouncedOnGround == true)
                                {
                                    Casing.BouncedOnGround = false;
                                    Casing.CanBounce = true;
                                    Casing.Gravity = 0.2f;
                                    Casing.RotationIncrement = (float)RandomDouble(-6, 6);
                                    Casing.Velocity.Y = (float)RandomDouble(-7, -2);

                                    if (Casing.CurrentPosition.Y >= Casing.MaxY)
                                        Casing.CurrentPosition.Y = Casing.MaxY - 1;

                                    float VelocityX = (explosion.Damage / 100 * (100 - (100 / explosion.BlastRadius) * Dist)) / 20;

                                    if (Casing.CurrentPosition.X < explosion.Position.X)
                                        Casing.Velocity.X = -VelocityX;

                                    if (Casing.CurrentPosition.X >= explosion.Position.X)
                                        Casing.Velocity.X = VelocityX;
                                }
                            }
                        }
                    }
                    #endregion
                }
                ShellCasingList.RemoveAll(Shell => Shell.Active == false);


                foreach (Particle coin in CoinList)
                {
                    coin.Update(gameTime);
                }
                CoinList.RemoveAll(Coin => Coin.Active == false);

                TotalParticles = 0;
                foreach (Emitter emitter in YSortedEmitterList)
                {
                    TotalParticles += emitter.ParticleList.Count;
                }

                foreach (Emitter emitter2 in EmitterList2)
                {
                    TotalParticles += emitter2.ParticleList.Count;
                }
                #endregion

                #region Projectile update stuff
                //Explosions have to be updated first so that if an explosion is created after this, it's not removed until 
                //the next update. i.e. Create explosion first, explosion does damage, remove explosion from list
                ExplosionsUpdate(gameTime);
                ExplosionList.RemoveAll(Explosion => Explosion.Active == false);

                if (ExplosionList.Count > 0)
                {
                    foreach (Explosion explosion in ExplosionList)
                    {
                        explosion.Active = false;
                    }
                }

                HeavyProjectileUpdate(gameTime);
                HeavyProjectileList.RemoveAll(HeavyProjectile => HeavyProjectile.Active == false && HeavyProjectile.EmitterList.All(Emitter => Emitter.ParticleList.Count == 0));

                TimedProjectileUpdate(gameTime);
                TimedProjectileList.RemoveAll(TimedProjectile => TimedProjectile.Active == false && TimedProjectile.EmitterList.All(Emitter => Emitter.ParticleList.Count == 0));

                InvaderHeavyProjectileUpdate(gameTime);
                InvaderHeavyProjectileList.RemoveAll(InvaderHeavyProjectile => InvaderHeavyProjectile.Active == false);

                LightProjectileUpdate();
                LightProjectileList.RemoveAll(LightProjectile => LightProjectile.Active == false);

                InvaderLightProjectileUpdate(gameTime);
                InvaderLightProjectileList.RemoveAll(InvaderLightProjectile => InvaderLightProjectile.Active == false);

                EnemyExplosionsUpdate(gameTime);
                #endregion

                #region Turret stuff.
                TurretUpdate(gameTime);
                #endregion

                #region Trap stuff
                TrapPlacement();
                TrapCollision();
                TrapUpdate(gameTime);

                foreach (Trap trap in TrapList)
                {
                    if (trap.CurrentHP <= 0)
                    {
                        trap.Active = false;
                    }
                }

                TrapList.RemoveAll(Trap => Trap.Active == false);

                //for (int i = 0; i < TrapList.Count; i++)
                //{
                //    if (TrapList[i].Active == false)
                //        TrapList.RemoveAt(i);
                //}
                #endregion

                #region Powerup Delivery stuff
                if (PowerupDelivery != null)
                {
                    //Powerup delivery ground-collision emitters go here 
                    //Angle - new Vector2(180 - MathHelper.ToDegrees(PowerupDelivery.Rotation), 180 - MathHelper.ToDegrees(PowerupDelivery.Rotation)),
                    if (PowerupDelivery.Position.Y > PowerupDelivery.MaxY)
                    {
                        float DeliveryAngle = -MathHelper.ToDegrees(PowerupDelivery.Rotation) - 180;

                        Emitter DustEmitter = new Emitter(SmokeParticle,
                                        new Vector2(PowerupDelivery.Position.X, PowerupDelivery.MaxY - 10),
                                        new Vector2(60, 120), new Vector2(3f, 8f), new Vector2(90, 150), 0.3f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(0.5f, 1.2f), DirtColor2, DirtColor, 0.05f, 0.4f, 5, 1, false,
                                        new Vector2(PowerupDelivery.MaxY, PowerupDelivery.MaxY + 8), false, PowerupDelivery.MaxY / 1080,
                                        null, null, null, null, null, null, new Vector2(0.05f, 0.05f), true, true);
                        YSortedEmitterList.Add(DustEmitter);

                        Emitter DebrisEmitter = new Emitter(SplodgeParticle,
                                        new Vector2(PowerupDelivery.Position.X, PowerupDelivery.MaxY),
                                        new Vector2(40, 140),
                                        new Vector2(2, 8), new Vector2(10, 80), 1f, true, new Vector2(0, 360),
                                        new Vector2(1, 3), new Vector2(0.007f, 0.05f), Color.DarkSlateGray, Color.SaddleBrown,
                                        0.2f, 0.1f, 1, 5, true, new Vector2(PowerupDelivery.MaxY, PowerupDelivery.MaxY + 8), null, PowerupDelivery.MaxY / 1080,
                                        null, null, null, null, null, null, new Vector2(0.02f, 0.02f), null, null, null);
                        YSortedEmitterList.Add(DebrisEmitter);

                        Emitter DebrisEmitter2 = new Emitter(SplodgeParticle,
                                new Vector2(PowerupDelivery.Position.X, PowerupDelivery.MaxY),
                                new Vector2(70, 110),
                                new Vector2(2, 8), new Vector2(20, 60), 1f, true, new Vector2(0, 360), new Vector2(1, 3),
                                new Vector2(0.01f, 0.02f), Color.Gray, Color.SaddleBrown, 0.2f, 0.1f, 1, 5, true,
                                new Vector2(PowerupDelivery.MaxY + 8, PowerupDelivery.MaxY + 16), null, PowerupDelivery.MaxY / 1080,
                                null, null, null, null, null, null, new Vector2(0.02f, 0.02f), null, null, null);
                        YSortedEmitterList.Add(DebrisEmitter2);

                        for (int i = 0; i < 3; i++)
                        {
                            Emitter SparkEmitter = new Emitter(RoundSparkParticle,
                                    new Vector2(PowerupDelivery.Position.X, PowerupDelivery.MaxY - 20),
                                    new Vector2(0, 0),
                                    new Vector2(1, 2), new Vector2(180, 200), 0.8f, true, new Vector2(0, 360), new Vector2(1, 3),
                                    new Vector2(0.1f, 0.3f), Color.Orange, Color.OrangeRed, 0.05f, 1f, 10, 5, true,
                                    new Vector2(PowerupDelivery.MaxY, PowerupDelivery.MaxY + 8), null, PowerupDelivery.MaxY / 1080,
                                    true, true, new Vector2(7, 9), new Vector2(DeliveryAngle - 40, DeliveryAngle), 0.2f, true);
                            AlphaEmitterList.Add(SparkEmitter);
                        }

                        for (int i = 0; i < 3; i++)
                        {
                            Emitter SparkEmitter = new Emitter(RoundSparkParticle,
                                    new Vector2(PowerupDelivery.Position.X, PowerupDelivery.MaxY - 20),
                                    new Vector2(0, 0),
                                    new Vector2(1, 2), new Vector2(180, 200), 0.8f, true, new Vector2(0, 360), new Vector2(1, 3),
                                    new Vector2(0.1f, 0.3f), Color.Orange, Color.OrangeRed, 0.05f, 1f, 10, 5, true,
                                    new Vector2(PowerupDelivery.MaxY, PowerupDelivery.MaxY + 8), null, PowerupDelivery.MaxY / 1080,
                                    true, true, new Vector2(7, 9), new Vector2(DeliveryAngle, DeliveryAngle + 40), 0.2f, true);
                            AlphaEmitterList.Add(SparkEmitter);
                        }

                        Emitter SmokeEmitter = new Emitter(SmokeParticle,
                                        new Vector2(PowerupDelivery.Position.X, PowerupDelivery.MaxY - 10),
                                        new Vector2(80, 100), new Vector2(0.25f, 2f), new Vector2(90, 130), 0.3f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(0.3f, 0.5f), SmokeColor1, SmokeColor2, -0.05f, 6f, 50, 5, false,
                                        new Vector2(PowerupDelivery.MaxY, PowerupDelivery.MaxY + 8), false, PowerupDelivery.MaxY / 1080,
                                        null, null, null, null, null, null, new Vector2(0.005f, 0.005f), true, true, null, true);
                        YSortedEmitterList.Add(SmokeEmitter);

                        PowerupDelivery.Position.Y = PowerupDelivery.MaxY;

                        DecalList.Add(new Decal(ExplosionDecal1, PowerupDelivery.Position, 0, new Vector2(0, 0), PowerupDelivery.MaxY, 1));
                    }
                    
                    //This needs to be moved
                    PowerupDelivery.Update(gameTime);
                    if (PowerupDelivery.CurrentHP <= 0)
                    {                        
                        PowerupsList.Add(PowerupDelivery.Powerup);
                        UIPowerupsList.Add(new UIPowerupIcon(PowerupDelivery.Powerup, GetPowerupIcon(PowerupDelivery.Powerup), new Vector2(1920 / 2, 32)));
                        UIPowerupsInfoList.Add(new UIPowerupInfo(new Vector2(1920/2, 32 + 32), PowerupDelivery.Powerup, RobotoRegular20_0));
                        PowerupDelivery = null;
                    }
                }

                #endregion

                #region Update effects
                //Not completely sure why this is here specifically. It should be somewhere else. Keep an eye on it.
                InvaderList.RemoveAll(Invader => Invader.Active == false);

                foreach (LightningBolt lightningBolt in LightningList)
                {
                    lightningBolt.Update(gameTime);
                }
                //Still need to check that the lightning bolt list depopulates itself as necessary
                LightningList.RemoveAll(Bolt => Bolt.Alpha <= 0);

                foreach (BulletTrail trail in TrailList)
                {
                    trail.Update(gameTime);
                }
                TrailList.RemoveAll(Trail => Trail.Color == Color.Transparent);

                foreach (BulletTrail trail in InvaderTrailList)
                {
                    trail.Update(gameTime);
                }
                InvaderTrailList.RemoveAll(Trail => Trail.Color == Color.Transparent);

                foreach (Decal decal in DecalList)
                {
                    decal.Update(gameTime);
                }

                ShockWaveEffect.Parameters["CurrentTime"].SetValue(ShockWaveEffect.Parameters["CurrentTime"].GetValueSingle() + (float)(gameTime.ElapsedGameTime.TotalSeconds));
                ShieldBubbleEffect.Parameters["CurrentTime"].SetValue(ShieldBubbleEffect.Parameters["CurrentTime"].GetValueSingle() + (float)(gameTime.ElapsedGameTime.TotalSeconds));
                #endregion

                #region Handle number changes for damage etc.
                foreach (NumberChange numChange in NumberChangeList)
                {
                    numChange.Update(gameTime);
                }

                NumberChangeList.RemoveAll(NumChange => NumChange.Active == false);
                #endregion

                #region Special Ability stuff
                UpdateSpecialAbilities(gameTime);
                #endregion

                #region Show turret info
                if (CurrentTurret != null)
                {
                    UITurretInfo.CurrentTurret = CurrentTurret;
                    UITurretInfo.Update(gameTime);                    
                 
                    UITurretInfo.TurretIconTexture = (Texture2D)this.GetType().GetField(CurrentTurret.TurretType.ToString() + "TurretIcon").GetValue(this);
                    UITurretInfo.DamageIconTexture = (Texture2D)this.GetType().GetField(CurrentTurret.DamageType.ToString() + "DamageIcon").GetValue(this);
                }
                #endregion

                #region Update UI Powerup Icons
                foreach (UIPowerupIcon uiPowerup in UIPowerupsList)
                {
                    uiPowerup.Update(gameTime);

                    if (uiPowerup.DestinationRectangle.Contains(VectorToPoint(CursorPosition)))
                    {
                        foreach (UIPowerupInfo uiPowerupInfo in UIPowerupsInfoList)
                        {
                            if (uiPowerupInfo.Powerup == uiPowerup.CurrentPowerup)
                            {
                                uiPowerupInfo.Visible = true;
                            }
                            else
                            {
                                uiPowerupInfo.Visible = false;
                            }
                        }
                    }
                    else
                    {
                        foreach (UIPowerupInfo uiPowerupInfo in UIPowerupsInfoList)
                        {
                            if (uiPowerupInfo.Powerup == uiPowerup.CurrentPowerup)
                            {
                                uiPowerupInfo.Visible = false;
                            }
                        }
                    }
                }

                foreach (UIPowerupInfo uiPowerupInfo in UIPowerupsInfoList)
                {
                    uiPowerupInfo.Update(gameTime);
                }
                #endregion

                //Powerups have to be applied before the invaders are updates and after the explosions and projectiles are updated
                //Create explosions/projectiles
                //Powerup explosions/projectiles
                //Damage invaders
                //Remove explosions/projectiles

                ApplyPowerups();

                InvaderUpdate(gameTime);
            }
            #endregion

            MenuButtonsUpdate(gameTime);

            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;

            if (IsLoading == true && AllLoaded == false)
                LoadingAnimation.Update(gameTime);

            base.Update(gameTime);
        }

        #region CONTENT that needs to be loaded
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            targetBatch = new SpriteBatch(GraphicsDevice);

            GameRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            CrepuscularMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);

            UIRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080, 
                false, SurfaceFormat.Rgba64, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
            
            MenuRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            ShaderTarget1 = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            ShaderTarget2 = new RenderTarget2D(GraphicsDevice, 1920, 1080);

            MainMenuBackground.LoadContent(SecondaryContent);

            BlankTexture = SecondaryContent.Load<Texture2D>("Blank");

            DefaultCursor = SecondaryContent.Load<Texture2D>("Cursors/DefaultCursor");
            CrosshairCursor = SecondaryContent.Load<Texture2D>("Cursors/Crosshair");

            TooltipBox = SecondaryContent.Load<Texture2D>("InformationBox");

            CurrencyIcon = SecondaryContent.Load<Texture2D>("Icons/CurrencyIcon");

            #region Cursor sprites
            //Turret Cursors        
            MachineGunTurretCursor = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/MachineGunTurretIcon");
            CannonTurretCursor = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/MachineGunTurretIcon");
            FlameThrowerTurretCursor = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/MachineGunTurretIcon");
            LightningTurretCursor = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/MachineGunTurretIcon");
            ClusterTurretCursor = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/MachineGunTurretIcon");
            FelCannonTurretCursor = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/MachineGunTurretIcon");
            BeamTurretCursor = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/MachineGunTurretIcon");
            FreezeTurretCursor = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/MachineGunTurretIcon");
            BoomerangTurretCursor = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/MachineGunTurretIcon");
            GrenadeTurretCursor = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/MachineGunTurretIcon");
            GasGrenadeTurretCursor = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/MachineGunTurretIcon");
            ShotgunTurretCursor = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/MachineGunTurretIcon");
            PersistentBeamTurretCursor = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/MachineGunTurretIcon");

            //Trap Cursors
            WallTrapCursor = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/WallTrapIcon");
            SpikesTrapCursor = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/FireTrapIcon");
            CatapultTrapCursor = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/FireTrapIcon");
            FireTrapCursor = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/FireTrapIcon");
            IceTrapCursor = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/FireTrapIcon");
            TarTrapCursor = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/TarTrapIcon");
            BarrelTrapCursor = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/FireTrapIcon");
            SawBladeTrapCursor = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/FireTrapIcon");
            LineTrapCursor = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/FireTrapIcon");
            TriggerTrapCursor = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/FireTrapIcon");
            #endregion

            #region Load icon sprites

            LockIcon = SecondaryContent.Load<Texture2D>("Icons/LockIcon");
            HealthIcon = SecondaryContent.Load<Texture2D>("Icons/HealthIcon");
            OverHeatIcon = SecondaryContent.Load<Texture2D>("Icons/OverHeatIcon");

            //Turret icons
            BeamTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/BeamTurretIcon");
            BoomerangTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/BoomerangTurretIcon");
            CannonTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/CannonTurretIcon");
            ClusterTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/ClusterTurretIcon");
            FelCannonTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/FelCannonTurretIcon");
            FlameThrowerTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/FlameThrowerTurretIcon");
            FreezeTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/FreezeTurretIcon");
            GrenadeTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/GrenadeTurretIcon");
            GasGrenadeTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/GrenadeTurretIcon");
            LightningTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/LightningTurretIcon");
            MachineGunTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/MachineGunTurretIcon");
            PersistentBeamTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/PersistentBeamTurretIcon");
            ShotgunTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/ShotgunTurretIcon");

            //Trap Icons
            CatapultTrapIcon = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/CatapultTrapIcon");
            IceTrapIcon = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/IceTrapIcon");
            TarTrapIcon = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/TarTrapIcon");
            WallTrapIcon = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/WallTrapIcon");
            BarrelTrapIcon = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/BarrelTrapIcon");
            FireTrapIcon = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/FireTrapIcon");
            LineTrapIcon = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/LineTrapIcon");
            SawBladeTrapIcon = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/SawBladeTrapIcon");
            SpikesTrapIcon = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/SpikesTrapIcon");

            //Damage Type Icons
            ConcussiveDamageIcon = SecondaryContent.Load<Texture2D>("Icons/DamageTypeIcons/ConcussiveDamageIcon");
            KineticDamageIcon = SecondaryContent.Load<Texture2D>("Icons/DamageTypeIcons/ConcussiveDamageIcon");
            FireDamageIcon = SecondaryContent.Load<Texture2D>("Icons/DamageTypeIcons/FireDamageIcon");
            RadiationDamageIcon = SecondaryContent.Load<Texture2D>("Icons/DamageTypeIcons/ConcussiveDamageIcon");
            ElectricDamageIcon = SecondaryContent.Load<Texture2D>("Icons/DamageTypeIcons/ConcussiveDamageIcon");
            #endregion

            #region Menu sounds
            MenuClick = SecondaryContent.Load<SoundEffect>("Sounds/MenuPing");
            MenuWoosh = SecondaryContent.Load<SoundEffect>("Sounds/MenuWoosh");

            //MenuMusic = SecondaryContent.Load<SoundEffect>("Sounds/MenuMusic1");
            //MenuMusicInstance = MenuMusic.CreateInstance();
            //MenuMusicInstance.IsLooped = true;
            //MenuMusicInstance.Play();
            #endregion

            Projection = Matrix.CreateOrthographicOffCenter(0, 1920, 1080, 0, 0, 1);

            BackgroundEffect = SecondaryContent.Load<Effect>("Shaders/BackgroundEffect");
            BackgroundEffect.Parameters["MatrixTransform"].SetValue(Projection);

            ButtonBlurEffect = SecondaryContent.Load<Effect>("Shaders/ButtonBlurEffect");
            ButtonBlurEffect.Parameters["MatrixTransform"].SetValue(Projection);

            FPSCounter.spriteFont = DefaultFont;
        }

        protected override void UnloadContent()
        {
            Content.Unload();
            SecondaryContent.Unload();
        }


        private void LoadGameContent()
        {
            if (GameState == GameState.Loading && IsLoading == false)
            {
                ProfileManagementPlay.JustClicked = false;

                LoadingContentString = "Starting Load";

                IsLoading = true;
                AllLoaded = false;

                ReadyToPlace = false;

                FlareMap1 = Content.Load<Texture2D>("FlareMap1");

                LoadingContentString = "Loading Audio";
                LoadGameSounds();

                LoadingContentString = "Loading Special Particles";
                LightningShellCasing = Content.Load<Texture2D>("Particles/LightningTurretShell");
                ShellCasing = Content.Load<Texture2D>("Particles/MachineShell");
                MachineBullet = Content.Load<Texture2D>("Particles/MachineBullet");
                Coin = Content.Load<Texture2D>("Particles/Coin");

                Tower.LoadContent(Content);

                LoadingContentString = "Loading Invaders";
                LoadInvaderSprites();

                LoadingContentString = "Loading Turrets";
                LoadTurretSprites();

                LoadingContentString = "Loading Traps";
                LoadTrapSprites();

                LoadingContentString = "Loading Projectiles";
                LoadProjectileSprites();

                LoadingContentString = "Loading Weather";
                LoadWeatherSprites();

                LoadingContentString = "Setting Up Lists";
                #region List Creating Code
                //This code just creates the lists for the buttons and traps with the right number of possible slots
                TrapList = new List<Trap>();

                TurretList = new List<Turret>();
                for (int i = 0; i < TowerButtons; i++)
                {
                    TurretList.Add(null);
                    //TurretList[i].LoadContent(Content);
                }

                InvaderList = new List<Invader>();

                HeavyProjectileList = new List<HeavyProjectile>();
                TimedProjectileList = new List<TimerHeavyProjectile>();
                InvaderHeavyProjectileList = new List<HeavyProjectile>();
                InvaderLightProjectileList = new List<LightProjectile>();
                LightningList = new List<LightningBolt>();
                TrailList = new List<BulletTrail>();
                InvaderTrailList = new List<BulletTrail>();
                YSortedEmitterList = new List<Emitter>();
                EmitterList2 = new List<Emitter>();
                AlphaEmitterList = new List<Emitter>();
                AdditiveEmitterList = new List<Emitter>();
                ExplosionList = new List<Explosion>();
                EnemyExplosionList = new List<Explosion>();
                ShellCasingList = new List<Particle>();
                CoinList = new List<Particle>();
                LightProjectileList = new List<LightProjectile>();
                TerrainSpriteList = new List<StaticSprite>();
                WeatherSpriteList = new List<StaticSprite>();
                //NumberChangeList = new List<NumberChange>();
                #endregion

                LoadingContentString = "Setting Up Buttons";
                #region Setting up the buttons
                TowerButtonList = new List<Button>();
                SpecialAbilitiesButtonList = new List<Button>();

                for (int i = 0; i < TowerButtons; i++)
                {
                    TowerButtonList.Add(new Button(TurretSlotButtonSprite, new Vector2(40 + Tower.DestinationRectangle.Width - 32, 500 + ((38 + 90) * i) - 32)));
                    TowerButtonList[i].LoadContent();
                }

                CooldownButtonList.Clear();

                //Not clearing these lists causes a Disposed Object error to show up for some reason
                //Don't forget to clear the lists between levels or even when moving from menu back to game
                UIWeaponInfoList.Clear();

                UITrapQuickInfoList.Clear();
                UITrapOutlineList.Clear();
                UITurretOutlineList.Clear();
                UITurretInfo = null;
                //CurrentTurret = null;
                ClearTurretSelect();
                ClearSelected();
                VerletShells.Clear();

                for (int i = 0; i < 8; i++)
                {
                    CooldownButton button = new CooldownButton(new Vector2(565 + (i * 100), 1080 - 80), new Vector2(90, 65), 1, PlaceWeaponList[i].IconTexture);

                    if (CurrentProfile.Buttons[i] != null)
                    {
                        UIWeaponInfoTip uiWeaponInfo = new UIWeaponInfoTip(Vector2.Zero, null, null);

                        if (CurrentProfile.Buttons[i].CurrentTurret != null)
                        {
                            uiWeaponInfo = new UIWeaponInfoTip(new Vector2(button.CurrentPosition.X, button.CurrentPosition.Y - 32),
                                                            ApplyTurretUpgrades((TurretType)CurrentProfile.Buttons[i].CurrentTurret, 0), null);
                        }

                        if (CurrentProfile.Buttons[i].CurrentTrap != null)
                        {
                            uiWeaponInfo = new UIWeaponInfoTip(new Vector2(button.CurrentPosition.X, button.CurrentPosition.Y - 32),
                                                              null, ApplyTrapUpgrades((TrapType)CurrentProfile.Buttons[i].CurrentTrap, Vector2.Zero));
                        }


                        uiWeaponInfo.CurrencyIcon = CurrencyIcon;
                        uiWeaponInfo.RobotoBold40_2 = RobotoBold40_2;
                        uiWeaponInfo.RobotoRegular20_0 = RobotoRegular20_0;
                        uiWeaponInfo.RobotoRegular20_2 = RobotoRegular20_2;
                        uiWeaponInfo.RobotoItalic20_0 = RobotoItalic20_0;

                        uiWeaponInfo.LoadContent(Content);
                        UIWeaponInfoList.Add(uiWeaponInfo);
                    }
                    else
                    {
                        UIWeaponInfoList.Add(null);
                    }

                    CooldownButtonList.Add(button);
                }

                //if (Tutorial == true)
                //{
                //    SelectButtonList[0].IconTexture = MachineGunTurretIcon;
                //    SelectButtonList[0].LoadContent();

                //    SelectButtonList[1].IconTexture = FireTrapIcon;
                //    SelectButtonList[1].LoadContent();
                //}

                SpecialAbilitiesButtonList.Add(new Button(DiamondButtonSprite, new Vector2(1450, 1080-120)));
                SpecialAbilitiesButtonList.Add(new Button(DiamondButtonSprite, new Vector2(1450+64+4, 1080 - 120)));
                SpecialAbilitiesButtonList.Add(new Button(DiamondButtonSprite, new Vector2(1450 + 32 + 2, 1080 - 120 - 32 - 2)));
                SpecialAbilitiesButtonList.Add(new Button(DiamondButtonSprite, new Vector2(1450 + 32 + 2, 1080 - 120 + 32 + 2)));

                foreach (Button button in SpecialAbilitiesButtonList)
                {
                    button.LoadContent();
                }
                
                #endregion

                LoadingContentString = "Setting Up Dialogue";
                MysteryEmployerTexture = Content.Load<Texture2D>("MysteryEmployer");
                StoryDialogueBox = new StoryDialogueBox(new Vector2(-400, 64), new Vector2(0, 64), new Vector2(500, 180), MysteryEmployerTexture, RobotoRegular20_0, StoryDialogue, CurrentProfile.Name);
                //StoryDialogueBox.Text = StoryDialogue.Lines[0];
                //StoryDialogueBox.ReplaceName(CurrentProfile.Name);
                //StoryDialogueBox.WrapText();

                LoadingContentString = "Loading Shaders";
                CrepuscularEffect = Content.Load<Effect>("Shaders/CrepuscularRaysEffect");
                CrepuscularEffect.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(0, 1920, 1080, 0, 0, 1));

                LoadingContentString += "1";
                HealthBarEffect = Content.Load<Effect>("Shaders/HealthBarEffect");
                HealthBarEffect.Parameters["MatrixTransform"].SetValue(Projection);
                HealthBarSprite = Content.Load<Texture2D>("HealthBarSprite");

                LoadingContentString += "2";
                ShockWaveEffect = Content.Load<Effect>("Shaders/ShockWaveEffect");
                ShockWaveEffect.Parameters["MatrixTransform"].SetValue(Projection);

                LoadingContentString += "3";
                ShieldBubbleEffect = Content.Load<Effect>("Shaders/ShieldBubbleEffect");
                LoadingContentString += "Cont";
                ShieldBubbleEffect.Parameters["MatrixTransform"].SetValue(Projection);
                LoadingContentString += "Matr";
                ShieldBubbleEffect.Parameters["NormalTexture"].SetValue(Content.Load<Texture2D>("ShieldNormalMap"));
                LoadingContentString += "Norm";
                ShieldBubbleEffect.Parameters["DistortionTexture"].SetValue(Content.Load<Texture2D>("ShieldDistortionMap"));
                LoadingContentString += "Dist";

                LoadingContentString = "Setting Up Waves";
                MaxWaves = CurrentLevel.WaveList.Count;
                CurrentWaveIndex = 0;

                StartWaveButton = new Button(ButtonRightSprite, new Vector2(1920-(ButtonRightSprite.Width/3), 200), null, null, null, "start waves", RobotoRegular20_2, "Right");
                StartWaveButton.LoadContent();

                Lightning.LoadContent(Content);
                Bolt.LoadContent(Content);

                LoadingContentString = "Loading Decals";
                ExplosionDecal1 = Content.Load<Texture2D>("Decals/ExplosionDecal1");
                BloodDecal1 = Content.Load<Texture2D>("Decals/BloodDecal1");

                LoadingContentString = "Loading Particles";
                //Texture2D SplodgeParticle, SmokeParticle, FireParticle, BallParticle;
                SplodgeParticle = Content.Load<Texture2D>("Particles/Splodge");
                SmokeParticle = Content.Load<Texture2D>("Particles/Smoke");
                FireParticle = Content.Load<Texture2D>("Particles/FireParticle");
                FireParticle2 = Content.Load<Texture2D>("Particles/FireParticle2");
                ExplosionParticle = Content.Load<Texture2D>("Particles/ExplosionParticle");
                ExplosionParticle2 = Content.Load<Texture2D>("Particles/ExplosionParticle2");
                BallParticle = Content.Load<Texture2D>("Particles/GlowBall");
                SparkParticle = Content.Load<Texture2D>("Particles/Spark");
                RoundSparkParticle = Content.Load<Texture2D>("Particles/RoundSpark");

                BulletTrailCap = Content.Load<Texture2D>("Particles/Cap");
                BulletTrailSegment = Content.Load<Texture2D>("Particles/Segment");

                TerrainShrub1 = Content.Load<Texture2D>("Terrain/Shrub");

                GrassBlade1 = Content.Load<Texture2D>("GrassBlade1");

                PowerupDeliveryPod = Content.Load<Texture2D>("PowerupDeliveryPod");
                PowerupDeliveryFin = Content.Load<Texture2D>("PowerupDeliveryFin");

                //for (int i = 0; i < 15; i++)
                //{
                //    Vector2 newPos = new Vector2(Random.Next(0, 1920), Random.Next(690, 930));

                //    if (TerrainSpriteList.Any(Sprite => Vector2.Distance(Sprite.Position, newPos) < 150))
                //    {
                //        newPos = new Vector2(Random.Next(0, 1920), Random.Next(690, 930));
                //    }

                //    StaticSprite terrainSprite = new StaticSprite(TerrainShrub1, newPos);
                //    terrainSprite.DrawDepth = (float)(terrainSprite.DestinationRectangle.Bottom / 1080.0);
                //    TerrainSpriteList.Add(terrainSprite);                    
                //}

                //for (int x = 250; x < 1920; x += 80)
                //{
                //    for (int y = 690; y < 930; y += 80)
                //    {
                //        for (int i = 0; i < 10; i++)
                //        {
                //            GrassBladeList.Add(new GrassBlade(GrassBlade1,
                //                new Vector2(x + Random.Next(-40, 40), y + Random.Next(-40, 40)),
                //                new Vector2(Random.Next(2, 6), Random.Next(20, 40))));
                //        }
                //    }
                //}

                LoadingContentString = "Loading UI";
                UITurretInfo = new UITurretInfo();
                UITurretInfo.Texture = WhiteBlock;
                UITurretInfo.OverHeatIconTexture = OverHeatIcon;
                UITurretInfo.Font = BigUIFont;

                HealthBar = new UISlopedBar(new Vector2(1920/2 - 810/2, 980), new Vector2(800 + 15, 15), Color.Lerp(Color.DarkRed, Color.LightGray, 0.2f), false, 15, 0);
                ShieldBar = new UISlopedBar(new Vector2(1920 / 2 - 810 / 2 + 5, 970), new Vector2(810 - 5 + 15, 10), Color.Lerp(Color.White, Color.LightGray, 0.2f), true, 0, 10);

                AllLoaded = true;
                LoadingContentString = "All Loaded - Press any key to continue";

                //IsLoading = false;

                //GameState = GameState.Playing;
            }
        } //This is called as a separate thread to be displayed while content is loaded

        private void UnloadGameContent()
        {
            //Clear some variables that aren't cleared as they're not lists that are reset back to new List<> when
            //the "List Creating Code" is called
            PowerupDelivery = null;
            SelectedTrap = null;
            SelectedTurret = null;
            DecalList.Clear();

            Content.Unload();
        } //This is called when the player exits to the main menu. Unloads game content, not menu content


        private void LoadGameSounds()
        {
            //Load all the in-game sounds here
            FireTrapStart = Content.Load<SoundEffect>("Sounds/FireTrapStart");
            LightningSound = Content.Load<SoundEffect>("Sounds/LightningSound");
            CannonExplosion = Content.Load<SoundEffect>("Sounds/CannonExplosion");
            CannonFire = Content.Load<SoundEffect>("Sounds/CannonFire");
            MachineShot1 = Content.Load<SoundEffect>("Sounds/Shot11");
            PlaceTrap = Content.Load<SoundEffect>("Sounds/PlaceTrap");
            GroundImpact = Content.Load<SoundEffect>("Sounds/Shot12");
            Ricochet1 = Content.Load<SoundEffect>("Sounds/Ricochet1");
            Ricochet2 = Content.Load<SoundEffect>("Sounds/Ricochet2");
            Ricochet3 = Content.Load<SoundEffect>("Sounds/Ricochet3");
            Splat1 = Content.Load<SoundEffect>("Sounds/Splat1");
            Splat2 = Content.Load<SoundEffect>("Sounds/Splat2");
            Implosion = Content.Load<SoundEffect>("Sounds/Implosion2");
            TurretOverheat = Content.Load<SoundEffect>("Sounds/TurretOverheat");
        }

        private void LoadInvaderSprites()
        {
            IceBlock = Content.Load<Texture2D>("IceBlock");
            Shadow = Content.Load<Texture2D>("Shadow");

            //SoldierSprite = new List<Texture2D>(4);
            //SoldierSprite.Add(Content.Load<Texture2D>("Invaders/Soldier/SoldierWalk"));
            //SoldierSprite.Add(Content.Load<Texture2D>("Invaders/Soldier/SoldierStand"));
            //SoldierSprite.Add(Content.Load<Texture2D>("Invaders/Soldier/SoldierMelee"));

            SoldierSprite = new List<Texture2D>(3) 
            {
                Content.Load<Texture2D>("Invaders/Soldier/SoldierWalk"),
                Content.Load<Texture2D>("Invaders/Soldier/SoldierStand"),
                Content.Load<Texture2D>("Invaders/Soldier/SoldierMelee")
            };

            StationaryCannonSprite = new List<Texture2D>(2)
            {
                Content.Load<Texture2D>("Invaders/StationaryCannon/StationaryCannonBase"),
                Content.Load<Texture2D>("Invaders/StationaryCannon/StationaryCannonBarrel")
            };


            //SoldierSprite = Content.Load<Texture2D>("Invaders/Soldier3");
            //BatteringRamSprite = Content.Load<Texture2D>("Invaders/Invader");
            //AirshipSprite = Content.Load<Texture2D>("Invaders/Invader");
            //ArcherSprite = Content.Load<Texture2D>("Invaders/Invader");
            //TankSprite = Content.Load<Texture2D>("Invaders/BlankTank");
            //SpiderSprite = Content.Load<Texture2D>("Invaders/Invader");
            //SlimeSprite = Content.Load<Texture2D>("Invaders/Invader");
            //SuicideBomberSprite = Content.Load<Texture2D>("Invaders/Invader");
            //FireElementalSprite = Content.Load<Texture2D>("Invaders/Invader");
            //TestInvaderSprite = Content.Load<Texture2D>("Invaders/Invader");
        }

        private void LoadTrapSprites()
        {
            WallSprite = new List<Texture2D>(1)
            {
                Content.Load<Texture2D>("Traps/WallTrap")
            };

            BarrelTrapSprite = new List<Texture2D>(1)
            {
                Content.Load<Texture2D>("Traps/Trap")
            };

            CatapultTrapSprite = new List<Texture2D>(1)
            {
                Content.Load<Texture2D>("Traps/Trap")
            };

            IceTrapSprite = new List<Texture2D>(1)
            {
                Content.Load<Texture2D>("Traps/Trap")
            };

            TarTrapSprite = new List<Texture2D>(1)
            {
                Content.Load<Texture2D>("Traps/Trap")
            };

            LineTrapSprite = new List<Texture2D>(1)
            {
                Content.Load<Texture2D>("Traps/Trap")
            };

            SawBladeTrapSprite = new List<Texture2D>(1)
            {
                Content.Load<Texture2D>("Traps/Trap")
            };

            SpikeTrapSprite = new List<Texture2D>(1)
            {
                Content.Load<Texture2D>("Traps/Trap")
            };

            FireTrapSprite = new List<Texture2D>(1)
            {
                Content.Load<Texture2D>("Traps/FireTrap")
            };
        }

        private void LoadProjectileSprites()
        {
            CannonBallSprite = Content.Load<Texture2D>("Projectiles/CannonRound");
            CannonBallSprite = Content.Load<Texture2D>("Projectiles/CannonRound");
            GrenadeSprite = Content.Load<Texture2D>("Projectiles/Grenade");
            GasGrenadeSprite = Content.Load<Texture2D>("Projectiles/Grenade");
            FlameSprite = Content.Load<Texture2D>("Projectiles/CannonRound");
            ClusterBombSprite = Content.Load<Texture2D>("Projectiles/CannonRound");
            ClusterShellSprite = Content.Load<Texture2D>("Projectiles/CannonRound");
            BoomerangSprite = Content.Load<Texture2D>("Projectiles/CannonRound");
            AcidSprite = Content.Load<Texture2D>("Projectiles/CannonRound");
            //"Projectiles/Torpedo";
            //"Projectiles/ClusterBombShell";
            //"Projectiles/AcidProjectileSprite";
            //"Projectiles/CannonRound";
            //"Projectiles/Arrow";
            //"Projectiles/CannonRound";
            //"Projectiles/ClusterBomb";
        }

        private void LoadTurretSprites()
        {
            TurretSelectBox = Content.Load<Texture2D>("SelectBox");

            MachineGunTurretBase = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBase");
            MachineGunTurretBarrel = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBarrel");

            CannonTurretBase = Content.Load<Texture2D>("Turrets/CannonTurret/CannonTurretBase");
            CannonTurretBarrel = Content.Load<Texture2D>("Turrets/CannonTurret/CannonTurretBarrel");

            LightningTurretBase = Content.Load<Texture2D>("Turrets/LightningTurret/LightningTurretBase");
            LightningTurretBarrel = Content.Load<Texture2D>("Turrets/LightningTurret/LightningTurretBarrel");

            BeamTurretBase = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBase");
            BeamTurretBarrel = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBarrel");

            ClusterTurretBase = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBase");
            ClusterTurretBarrel = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBarrel");

            GrenadeTurretBase = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBase");
            GrenadeTurretBarrel = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBarrel");

            GasGrenadeTurretBase = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBase");
            GasGrenadeTurretBarrel = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBarrel");

            ShotgunTurretBase = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBase");
            ShotgunTurretBarrel = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBarrel");

            FlameThrowerTurretBase = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBase");
            FlameThrowerTurretBarrel = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBarrel");

            PersistentBeamTurretBase = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBase");
            PersistentBeamTurretBarrel = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBarrel");

            FelCannonTurretBase = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBase");
            FelCannonTurretBarrel = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBarrel");

            BoomerangTurretBase = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBase");
            BoomerangTurretBarrel = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBarrel");
        }

        private void LoadWeatherSprites()
        {
            SnowDust1 = Content.Load<Texture2D>("Weather/SnowDust1");
            SnowDust2 = Content.Load<Texture2D>("Weather/SnowDust2");
            SnowDust3 = Content.Load<Texture2D>("Weather/SnowDust3");
            SnowDust4 = Content.Load<Texture2D>("Weather/SnowDust4");
            SnowDust5 = Content.Load<Texture2D>("Weather/SnowDust5");
            BlurrySnowflake = Content.Load<Texture2D>("Weather/BlurrySnowflake");
            FocusedSnowflake = Content.Load<Texture2D>("Weather/FocusedSnowflake");

            //BlurryEmitter = new Emitter(BlurrySnowflake, new Vector2(0, -64), new Vector2(270, 270), new Vector2(0.95f, 1.8f), 
            //    new Vector2(1500, 2500), 0.5f, true, new Vector2(0, 360), new Vector2(0, 5), new Vector2(0.1f, 0.25f), 
            //    Color.White, Color.White, 0.0005f, -1, 25, 1, false, new Vector2(690, 930));

            //FocusedEmitter = new Emitter(FocusedSnowflake, new Vector2(0, -64), new Vector2(-90, -90), new Vector2(0.5f, 0.75f), 
            //    new Vector2(2000, 3000), 0.25f, true, new Vector2(0, 360), new Vector2(-1f, 1f), new Vector2(0.1f, 0.25f), 
            //    Color.White, Color.White, 0.0005f, -1, 25, 1, true, new Vector2(690, 930), false, 0.25f, true, false, null, null, null, null, null, true, true, null);
    
        }

        private void LoadFonts()
        {
            DefaultFont = SecondaryContent.Load<SpriteFont>("Fonts/RobotoRegular20_2");
            //ButtonFont = SecondaryContent.Load<SpriteFont>("Fonts/RobotoRegular20_2");
            TooltipFont = SecondaryContent.Load<SpriteFont>("Fonts/RobotoRegular20_0");
            BigUIFont = SecondaryContent.Load<SpriteFont>("Fonts/RobotoRegular40_2");
            ResourceFont = Content.Load<SpriteFont>("Fonts/RobotoRegular20_2");


            RobotoBold40_2 = SecondaryContent.Load<SpriteFont>("Fonts/RobotoBold40_2");
            RobotoRegular40_2 = SecondaryContent.Load<SpriteFont>("Fonts/RobotoRegular40_2");
            RobotoRegular20_2 = SecondaryContent.Load<SpriteFont>("Fonts/RobotoRegular20_2");
            RobotoRegular20_0 = SecondaryContent.Load<SpriteFont>("Fonts/RobotoRegular20_0");
            RobotoItalic20_0 = SecondaryContent.Load<SpriteFont>("Fonts/RobotoItalic20_0");
        }
        #endregion


        #region BUTTON stuff that needs to be done every step
        private void TowerButtonUpdate(GameTime gameTime)
        {
            //This places the selected turret type into the right slot on the tower when the tower slot has been clicked
            int Index;

            foreach (Button towerButton in TowerButtonList)
            {
                if (this.IsActive == true)
                {
                    towerButton.Update(CursorPosition, gameTime);

                    if (towerButton.JustClicked == true && SelectedTurret != null)
                    {
                        #region Create an effect under the turret as it's placed
                        if (SelectedTurret != null)
                        {
                            Emitter Sparks = new Emitter(BallParticle, towerButton.CurrentPosition,
                                new Vector2(0, 360), new Vector2(1, 3), new Vector2(5, 15), 1f, true, new Vector2(0, 360),
                                new Vector2(2, 5), new Vector2(0.25f, 0.25f), Color.Blue, Color.DeepSkyBlue, 0.0f, 0.05f, 1, 10,
                                false, new Vector2(0, 1080), false, null, false, false);

                            Emitter Smoke = new Emitter(SmokeParticle, towerButton.CurrentPosition,
                                new Vector2(0, 360), new Vector2(1, 3), new Vector2(5, 15), 1f, true, new Vector2(0, 360),
                                new Vector2(2, 5), new Vector2(1f, 1f), Color.Blue, Color.DeepSkyBlue, 0.0f, 0.05f, 1, 10,
                                false, new Vector2(0, 1080), false, null, false, false);

                            AlphaEmitterList.Add(Sparks);
                            AlphaEmitterList.Add(Smoke);
                        }
                        #endregion

                        Index = TowerButtonList.IndexOf(towerButton);
                        if (Resources >= TurretCost(SelectedTurret.Value))
                        {
                            TowerButtonList[Index].ButtonActive = false;
                            Turret newTurret = ApplyTurretUpgrades(SelectedTurret.Value, Index);

                            switch (newTurret.TurretType)
                            {
                                default:
                                    var TurretBase = this.GetType().GetField(newTurret.TurretType.ToString() + "TurretBase").GetValue(this);
                                    var TurretBarrel = this.GetType().GetField(newTurret.TurretType.ToString() + "TurretBarrel").GetValue(this);
                                    newTurret.TurretBase = (Texture2D)TurretBase;
                                    newTurret.TurretBarrel = (Texture2D)TurretBarrel;
                                    break;
                            }

                            switch (newTurret.TurretType)
                            {
                                case TurretType.MachineGun:
                                    newTurret.AmmoBelt = new AmmoBelt(newTurret.Position, MachineBullet);
                                    break;
                            }

                            newTurret.Initialize(Content);
                            TurretList[Index] = newTurret;

                            Resources -= TurretCost(SelectedTurret.Value);

                            CurrentTurret = newTurret;
                            SelectedTurret = null;
                            TurretList[Index].Selected = true;
                        }
                    }
                }
            }
        }

        private void SelectButtonsUpdate(GameTime gameTime)
        {
            //This makes sure that when the button at the bottom of the screen is clicked, the corresponding trap or turret is actually selected//
            //This will code will need to be added to every time that a new trap/turret is added to the game.
            int Index;

            foreach (CooldownButton button in CooldownButtonList)
            {
                int ButtonIndex = CooldownButtonList.IndexOf(button);

                if (CurrentProfile.Buttons[ButtonIndex] != null)
                {
                    TrapType? trap = CurrentProfile.Buttons[ButtonIndex].CurrentTrap as TrapType?;
                    TurretType? turret = CurrentProfile.Buttons[ButtonIndex].CurrentTurret as TurretType?;

                    if (trap != null)
                        if (Resources >= TrapCost(trap.Value))
                        {
                            button.IconColor = Color.White;
                        }
                        else
                        {
                            button.IconColor = Color.Gray;
                        }

                    if (turret != null)
                        if (Resources >= TurretCost(turret.Value))
                        {
                            button.IconColor = Color.White;
                        }
                        else
                        {
                            button.IconColor = Color.Gray;
                        }
                }
            }

            foreach (CooldownButton button in CooldownButtonList)
            {
                button.Update(gameTime, CursorPosition);

                if (CurrentProfile.Buttons[CooldownButtonList.IndexOf(button)] != null)
                {
                    if (button.CurrentButtonState == ButtonSpriteState.Hover || button.CurrentButtonState == ButtonSpriteState.Pressed)
                    {
                        int Ind = CooldownButtonList.IndexOf(button);
                        UIWeaponInfoList[Ind].Visible = true;
                    }
                    else
                    {
                        //A problem occurs here when placing a weapon in the second slot and not the first slot
                        int Ind = CooldownButtonList.IndexOf(button);
                        UIWeaponInfoList[Ind].Visible = false;
                    }
                }

                if (button.JustClicked == true)
                {
                    ClearTurretSelect();
                    Index = CooldownButtonList.IndexOf(button);

                    #region Check the layout of the buttons from the player profile
                    Action CheckLayout = new Action(() =>
                        {
                            if (Index <= CurrentProfile.Buttons.Count - 1)
                            {
                                #region Handle Trap selection
                                if (CurrentProfile.Buttons[Index] != null && CurrentProfile.Buttons[Index].CurrentTurret == null)
                                    if (Resources >= TrapCost(CurrentProfile.Buttons[Index].CurrentTrap.Value) && TrapLimit > TrapList.Count)
                                        SelectedTrap = CurrentProfile.Buttons[Index].CurrentTrap.Value;
                                #endregion

                                #region Handle Turret selection
                                if (CurrentProfile.Buttons[Index] != null && CurrentProfile.Buttons[Index].CurrentTrap == null)
                                    if (Resources >= TurretCost(CurrentProfile.Buttons[Index].CurrentTurret.Value))
                                        SelectedTurret = CurrentProfile.Buttons[Index].CurrentTurret.Value;
                                #endregion
                            }
                            else
                            {
                                SelectedTrap = null;
                                SelectedTurret = null;
                            }
                        });

                    #endregion

                    switch (Index)
                    {
                        default:
                            CheckLayout();
                            if (SelectedTrap != null || SelectedTurret != null)
                                ReadyToPlace = true;
                            break;
                    }
                }
            }
        }

        private void MenuButtonsUpdate(GameTime gameTime)
        {
            if (this.IsActive == true)
            {
                if (MenuColor != Color.Transparent && GameState != GameState.Playing)
                {
                    MenuColor = Color.Lerp(MenuColor, Color.Transparent, 0.05f);
                }

                #region Handling Main Menu Button Presses
                if (GameState == GameState.Menu && DialogVisible == false)
                {
                    int Index;

                    foreach (Button button in MainMenuButtonList)
                    {
                        button.Update(CursorPosition, gameTime);

                        if (button.PlayHover == true)
                        {
                            MenuWoosh.Play();
                        }

                        if (button.CurrentButtonState == ButtonSpriteState.Hover)
                        {
                            button.NextPosition.X = 0;
                        }

                        if (button.CurrentButtonState == ButtonSpriteState.Released)
                        {
                            button.NextPosition.X = -50;
                        }

                        if (button.JustClicked == true)
                        {
                            Index = MainMenuButtonList.IndexOf(button);
                            MenuClick.Play();

                            switch (Index)
                            {
                                case 0:
                                    button.ResetState();
                                    ProfileBackButton.CurrentPosition.X = 1920 + 300;
                                    MenuColor = Color.White;
                                    GameState = GameState.ProfileSelect;
                                    SetProfileNames();
                                    break;

                                case 1:
                                    Tutorial = true;
                                    UnloadGameContent();
                                    Tower.CurrentHP = 100;
                                    Tower.MaxHP = 100;
                                    Tower.CurrentShield = 20;
                                    MenuColor = Color.White;
                                    SelectedTrap = null;
                                    SelectedTurret = null;                                    
                                    Assembly assembly = Assembly.Load("TowerDefensePrototype");
                                    Type t = assembly.GetType("TowerDefensePrototype.TutorialLevel");
                                    CurrentLevel = (Level)Activator.CreateInstance(t);

                                    CurrentWaveIndex = 0;
                                    CurrentInvaderIndex = 0;
                                    CurrentWave = CurrentLevel.WaveList[0];

                                    List<Weapon> TutorialWeaponList = new List<Weapon>();
                                    for (int i = 0; i < 10; i++)
                                    {
                                        TutorialWeaponList.Add(new Weapon(null, null));
                                    }
                                    TutorialWeaponList[0].CurrentTurret = TurretType.MachineGun;
                                    TutorialWeaponList[1].CurrentTrap = TrapType.Fire;

                                    CurrentProfile = new Profile()
                                    {
                                        Name = "Tutorial",
                                        LevelNumber = 0,

                                        Points = 0,

                                        Fire = true,
                                        MachineGun = true,

                                        Credits = 0,

                                        ShotsFired = 0,

                                        Buttons = TutorialWeaponList
                                    };


                                    GameState = GameState.Loading;
                                    LoadingThread = new Thread(LoadGameContent);
                                    LoadingThread.Start();
                                    IsLoading = false;
                                    break;

                                case 2:
                                    button.ResetState();
                                    MenuColor = Color.White;
                                    GameState = GameState.Options;
                                    MenuSFXVolume = CurrentSettings.SFXVolume * 10;
                                    MenuMusicVolume = CurrentSettings.MusicVolume * 10;
                                    break;

                                case 3:

                                    break;

                                case 4:
                                    button.ResetState();
                                    ExitDialog = new DialogBox(DialogBox, ShortButtonLeftSprite, ShortButtonRightSprite, RobotoRegular20_0, new Vector2(1920 / 2, 1080 / 2), "exit", "Do you want to exit?", "cancel");
                                    ExitDialog.LoadContent();
                                    DialogVisible = true;
                                    break;
                            }
                        }
                    }
                }
                #endregion

                #region Handling Pause Menu Button Presses
                if (GameState == GameState.Paused && DialogVisible == false)
                {
                    int Index;

                    foreach (Button button in PauseButtonList)
                    {
                        button.Update(CursorPosition, gameTime);

                        if (button.JustClicked == true)
                        {
                            MenuClick.Play();

                            Index = PauseButtonList.IndexOf(button);

                            switch (Index)
                            {
                                case 0:
                                    button.ResetState();
                                    GameState = GameState.Playing;
                                    break;

                                case 1:

                                    break;

                                case 2:
                                    button.ResetState();
                                    MainMenuDialog = new DialogBox(DialogBox, ShortButtonLeftSprite, ShortButtonRightSprite, RobotoRegular20_0, new Vector2(1920 / 2, 1080 / 2), "yes", "Are you sure you want to return to the main menu? All progress will be lost.", "no");
                                    MainMenuDialog.LoadContent();
                                    DialogVisible = true;
                                    break;

                                case 3:
                                    button.ResetState();
                                    ProfileMenuDialog = new DialogBox(DialogBox, ShortButtonLeftSprite, ShortButtonRightSprite, RobotoRegular20_0, new Vector2(1920 / 2, 1080 / 2), "yes", "Are you sure you want to return to your profile menu? All progress will be lost.", "no");
                                    ProfileMenuDialog.LoadContent();
                                    DialogVisible = true;
                                    break;

                                case 4:
                                    button.ResetState();
                                    ExitDialog = new DialogBox(DialogBox, ShortButtonLeftSprite, ShortButtonRightSprite, RobotoRegular20_0, new Vector2(1920 / 2, 1080 / 2), "exit", "Do you want to exit? All progress will be lost.", "cancel");
                                    ExitDialog.LoadContent();
                                    DialogVisible = true;
                                    break;
                            }
                        }
                    }
                }
                #endregion

                #region Handling Profile Management Button Presses
                if (GameState == GameState.ProfileManagement && DialogVisible == false)
                {
                    //Match the state of the tabs with the ProfileManagementState
                    ProfileManagementTabs.Update(gameTime);
                    ProfileManagementState = (ProfileManagementState)(ProfileManagementTabs.SelectedIndex);

                    ProfileManagementPlay.Update(CursorPosition, gameTime);
                    ProfileManagementBack.Update(CursorPosition, gameTime);

                    switch (ProfileManagementState)
                    {
                        #region Loadout screen
                        case ProfileManagementState.Loadout:

                            #region Move turrets right
                            MoveTurretsRight.Update(CursorPosition, gameTime);
                            if (MoveTurretsRight.JustClicked == true && 
                                SelectTurretList[0].DestinationRectangle.Left < MoveTurretsLeft.DestinationRectangle.Right)
                            {
                                foreach (WeaponBox turretBox in SelectTurretList)
                                {
                                    turretBox.Position += new Vector2(282, 0);
                                    turretBox.UpdateQuads();
                                    turretBox.SetUpBars();
                                }
                            }
                            #endregion

                            #region Move turrets left
                            MoveTurretsLeft.Update(CursorPosition, gameTime);
                            if (MoveTurretsLeft.JustClicked == true &&
                                SelectTurretList[SelectTurretList.Count - 1].DestinationRectangle.Right > MoveTurretsRight.DestinationRectangle.Left)
                            {
                                foreach (WeaponBox turretBox in SelectTurretList)
                                {
                                    turretBox.Position -= new Vector2(282, 0);
                                    turretBox.UpdateQuads();
                                    turretBox.SetUpBars();
                                }
                            }
                            #endregion

                            #region Move traps right
                            MoveTrapsRight.Update(CursorPosition, gameTime);
                            if (MoveTrapsRight.JustClicked == true && 
                                SelectTrapList[0].DestinationRectangle.Left < MoveTrapsLeft.DestinationRectangle.Right)
                            {
                                foreach (WeaponBox trapBox in SelectTrapList)
                                {
                                    trapBox.Position += new Vector2(282, 0);
                                    trapBox.UpdateQuads();
                                    trapBox.SetUpBars();
                                }
                            }
                            #endregion

                            #region Move traps left
                            MoveTrapsLeft.Update(CursorPosition, gameTime);
                            if (MoveTrapsLeft.JustClicked == true &&
                               SelectTrapList[SelectTrapList.Count - 1].DestinationRectangle.Right > MoveTrapsRight.DestinationRectangle.Left)
                            {
                                foreach (WeaponBox trapBox in SelectTrapList)
                                {
                                    trapBox.Position -= new Vector2(282, 0);
                                    trapBox.UpdateQuads();
                                    trapBox.SetUpBars();
                                }
                            }
                            #endregion


                            #region Select turret boxes
                            foreach (WeaponBox turretBox in SelectTurretList)
                            {
                                turretBox.Update(gameTime);

                                if (turretBox.DestinationRectangle.Left > MoveTurretsLeft.DestinationRectangle.Right &&
                                    turretBox.DestinationRectangle.Right < MoveTurretsRight.DestinationRectangle.Left)
                                {
                                    turretBox.Visible = true;
                                }
                                else
                                {
                                    turretBox.Visible = false;
                                }

                                if (turretBox.JustClicked == true && turretBox.Visible == true)
                                {
                                    string WeaponName = turretBox.ContainsTurret.ToString();
                                    var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

                                    if ((bool)Available == true)
                                        SelectedTurret = turretBox.ContainsTurret;
                                }
                            }
                            #endregion

                            #region Select trap boxes
                            foreach (WeaponBox trapBox in SelectTrapList)
                            {
                                trapBox.Update(gameTime);

                                if (trapBox.DestinationRectangle.Left > MoveTrapsLeft.DestinationRectangle.Right &&
                                    trapBox.DestinationRectangle.Right < MoveTrapsRight.DestinationRectangle.Left)
                                {
                                    trapBox.Visible = true;
                                }
                                else
                                {
                                    trapBox.Visible = false;
                                }

                                if (trapBox.JustClicked == true && trapBox.Visible == true)
                                {
                                    string WeaponName = trapBox.ContainsTrap.ToString();
                                    var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

                                    if ((bool)Available == true)
                                        SelectedTrap = trapBox.ContainsTrap;
                                }
                            }
                            #endregion


                            #region Place Weapon Buttons
                            foreach (Button button in PlaceWeaponList)
                            {
                                button.Update(CursorPosition, gameTime);

                                if (button.JustClicked == true)
                                {
                                    int Index = PlaceWeaponList.IndexOf(button);
                                    Button test = PlaceWeaponList[Index];

                                    if (SelectedTrap == null && SelectedTurret == null && CurrentProfile.Buttons[PlaceWeaponList.IndexOf(button)] != null)
                                    {
                                        SelectedTurret = CurrentProfile.Buttons[PlaceWeaponList.IndexOf(button)].CurrentTurret;
                                        SelectedTrap = CurrentProfile.Buttons[PlaceWeaponList.IndexOf(button)].CurrentTrap;
                                        CurrentProfile.Buttons[PlaceWeaponList.IndexOf(button)] = null;
                                        button.IconTexture = null;
                                        HandlePlacedIcons();
                                        return;
                                    }

                                    if (SelectedTurret != null)
                                        switch (SelectedTurret)
                                        {
                                            default:
                                                CurrentProfile.Buttons[PlaceWeaponList.IndexOf(button)] = new Weapon(SelectedTurret, null);
                                                SelectedTurret = null;
                                                SelectedTrap = null;
                                                break;
                                        }

                                    if (SelectedTrap != null)
                                        switch (SelectedTrap)
                                        {
                                            default:
                                                CurrentProfile.Buttons[PlaceWeaponList.IndexOf(button)] = new Weapon(null, SelectedTrap);
                                                SelectedTurret = null;
                                                SelectedTrap = null;
                                                break;
                                        }

                                    HandlePlacedIcons();
                                }

                                if (button.JustRightClicked == true)
                                {
                                    CurrentProfile.Buttons[PlaceWeaponList.IndexOf(button)] = null;
                                    button.IconTexture = null;
                                    button.LoadContent();
                                }
                            }
                            #endregion

                            break;
                        #endregion

                        case ProfileManagementState.Stats:

                            break;

                        case ProfileManagementState.Upgrades:

                            break;
                    }

                    RightClickClearSelected();

                    #region Play Button
                    if (ProfileManagementPlay.CurrentButtonState == ButtonSpriteState.Released)
                    {
                        ProfileManagementPlay.NextPosition.X = 1920 - 400;
                    }

                    if (ProfileManagementPlay.CurrentButtonState == ButtonSpriteState.Hover)
                    {
                        ProfileManagementPlay.NextPosition.X = 1920 - 450;
                    }

                    if (ProfileManagementPlay.PlayHover == true)
                    {
                        MenuWoosh.Play();
                    }

                    if (ProfileManagementPlay.JustClicked == true)
                    {
                        ProfileManagementPlay.ResetState();
                        MenuClick.Play();

                        if (CurrentProfile != null)
                        {
                            if (CurrentProfile.Buttons.All(Weapon => Weapon == null))
                            {
                                NoWeaponsDialog = new DialogBox(DialogBox, ShortButtonLeftSprite, ShortButtonRightSprite, RobotoRegular20_0, new Vector2(1920 / 2, 1080 / 2), "OK", "You have no weapons to use!", "");
                                NoWeaponsDialog.LoadContent();
                                DialogVisible = true;
                            }
                            else
                            {
                                UnloadGameContent();
                                Tower.CurrentHP = Tower.MaxHP;
                                Tower.CurrentShield = Tower.MaxShield;
                                MenuColor = Color.White;                                
                                LevelNumber = CurrentProfile.LevelNumber;                                
                                LoadLevel(LevelNumber);
                                //LoadUpgrades();
                                StorageDevice.BeginShowSelector(this.SaveProfile, null);
                                GameState = GameState.Loading;
                                LoadingThread = new Thread(LoadGameContent);
                                LoadingThread.Start();
                                IsLoading = false;
                            }
                        }
                    }
                    #endregion

                    #region Back Button
                    if (ProfileManagementBack.CurrentButtonState == ButtonSpriteState.Released)
                    {
                        ProfileManagementBack.NextPosition.X = -50;
                    }

                    if (ProfileManagementBack.CurrentButtonState == ButtonSpriteState.Hover)
                    {
                        ProfileManagementBack.NextPosition.X = 0;
                    }

                    if (ProfileManagementBack.PlayHover == true)
                    {
                        MenuWoosh.Play();
                    }

                    if (ProfileManagementBack.JustClicked == true)
                    {
                        MenuClick.Play();
                        ProfileBackButton.CurrentPosition.X = 1920 - 150;
                        MenuColor = Color.White;
                        SetProfileNames();
                        SelectedTrap = null;
                        SelectedTurret = null;
                        StorageDevice.BeginShowSelector(this.SaveProfile, null);
                        GameState = GameState.ProfileSelect;
                        ProfileManagementBack.ResetState();
                    }
                    #endregion
                }
                #endregion

                #region Handling Profile Select Menu Button Presses
                if (GameState == GameState.ProfileSelect && DialogVisible == false)
                {
                    int Index;

                    foreach (Button button in ProfileButtonList)
                    {
                        button.Update(CursorPosition, gameTime);

                        if (button.PlayHover == true)
                        {
                            MenuWoosh.Play();
                        }

                        if (button.JustClicked == true)
                        {
                            button.ResetState();
                            MenuColor = Color.White;
                            MenuClick.Play();
                            ProfileManagementBack.CurrentPosition.X = -300;
                            ProfileManagementPlay.CurrentPosition.X = 1920 - 150;
                            Index = ProfileButtonList.IndexOf(button);

                            switch (Index)
                            {
                                case 0:
                                    ProfileNumber = 1;
                                    FileName = "Profile1.sav";
                                    StorageDevice.BeginShowSelector(this.HandleProfile, null);
                                    break;

                                case 1:
                                    ProfileNumber = 2;
                                    FileName = "Profile2.sav";
                                    StorageDevice.BeginShowSelector(this.HandleProfile, null);
                                    break;

                                case 2:
                                    ProfileNumber = 3;
                                    FileName = "Profile3.sav";
                                    StorageDevice.BeginShowSelector(this.HandleProfile, null);
                                    break;

                                case 3:
                                    ProfileNumber = 4;
                                    FileName = "Profile4.sav";
                                    StorageDevice.BeginShowSelector(this.HandleProfile, null);
                                    break;
                            }
                        }
                    }

                    foreach (Button button in ProfileDeleteList)
                    {
                        button.Update(CursorPosition, gameTime);

                        if (button.JustClicked == true)
                        {
                            button.ResetState();
                            Index = ProfileDeleteList.IndexOf(button);

                            switch (Index)
                            {
                                case 0:
                                    FileName = "Profile1.sav";
                                    StorageDevice.BeginShowSelector(this.CheckFileDelete, null);
                                    break;

                                case 1:
                                    FileName = "Profile2.sav";
                                    StorageDevice.BeginShowSelector(this.CheckFileDelete, null);
                                    break;

                                case 2:
                                    FileName = "Profile3.sav";
                                    StorageDevice.BeginShowSelector(this.CheckFileDelete, null);
                                    break;

                                case 3:
                                    FileName = "Profile4.sav";
                                    StorageDevice.BeginShowSelector(this.CheckFileDelete, null);
                                    break;
                            }
                        }
                    }

                    ProfileBackButton.Update(CursorPosition, gameTime);

                    if (ProfileBackButton.JustClicked == true)
                    {
                        ProfileBackButton.ResetState();
                        foreach (Button button in MainMenuButtonList)
                        {
                            button.NextPosition.X = 0;
                            button.CurrentPosition.X = -300;
                        }
                        MenuClick.Play();
                        MenuColor = Color.White;
                        GameState = GameState.Menu;
                    }

                    if (ProfileBackButton.CurrentButtonState == ButtonSpriteState.Hover)
                    {
                        ProfileBackButton.NextPosition.X = 1920 - 450;
                    }

                    if (ProfileBackButton.CurrentButtonState == ButtonSpriteState.Released)
                    {
                        ProfileBackButton.NextPosition.X = 1920 - 400;
                    }

                    if (ProfileBackButton.PlayHover == true)
                    {
                        MenuWoosh.Play();
                    }
                }
                #endregion

                #region Handling Options Button Presses
                if (GameState == GameState.Options)
                {
                    OptionsBack.Update(CursorPosition, gameTime);

                    OptionsSFXUp.Update(CursorPosition, gameTime);
                    OptionsSFXDown.Update(CursorPosition, gameTime);

                    OptionsMusicUp.Update(CursorPosition, gameTime);
                    OptionsMusicDown.Update(CursorPosition, gameTime);

                    if (OptionsBack.PlayHover == true)
                    {
                        MenuWoosh.Play();
                    }

                    if (OptionsBack.JustClicked == true)
                    {
                        OptionsBack.ResetState();
                        MenuClick.Play();
                        MenuColor = Color.White;
                        SaveSettings();
                        GameState = GameState.Menu;
                    }

                    #region SFX Volume change
                    if (OptionsSFXUp.JustClicked == true)
                    {
                        if (MenuSFXVolume < 10)
                        {
                            MenuSFXVolume++;
                            CurrentSettings.SFXVolume = MenuSFXVolume / 10;
                            SoundEffect.MasterVolume = CurrentSettings.SFXVolume;
                            MenuClick.Play();
                        }
                    }

                    if (OptionsSFXDown.JustClicked == true)
                    {
                        if (MenuSFXVolume > 0)
                        {
                            MenuSFXVolume--;
                            CurrentSettings.SFXVolume = MenuSFXVolume / 10;
                            SoundEffect.MasterVolume = CurrentSettings.SFXVolume;
                            MenuClick.Play();
                        }
                    }
                    #endregion

                    #region Music volume change
                    if (OptionsMusicUp.JustClicked == true)
                    {
                        if (MenuMusicVolume < 10)
                        {
                            MenuMusicVolume++;
                            CurrentSettings.MusicVolume = MenuMusicVolume / 10;
                            MediaPlayer.Volume = CurrentSettings.MusicVolume;
                            MenuClick.Play();
                        }
                    }

                    if (OptionsMusicDown.JustClicked == true)
                    {
                        if (MenuMusicVolume > 0)
                        {
                            MenuMusicVolume--;
                            CurrentSettings.MusicVolume = MenuMusicVolume / 10;
                            MediaPlayer.Volume = CurrentSettings.MusicVolume;
                            MenuClick.Play();
                        }
                    }
                    #endregion


                    #region Control volumes with scroll wheel
                    #region SFX Volume
                    if (OptionsSFXUp.CurrentButtonState == ButtonSpriteState.Hover ||
                        OptionsSFXDown.CurrentButtonState == ButtonSpriteState.Hover)
                    {
                        if (CurrentMouseState.ScrollWheelValue > PreviousMouseState.ScrollWheelValue)
                        {
                            if (MenuSFXVolume < 10)
                            {
                                MenuSFXVolume++;
                                CurrentSettings.SFXVolume = MenuSFXVolume / 10;
                                SoundEffect.MasterVolume = CurrentSettings.SFXVolume;
                                MenuClick.Play();
                            }
                        }
                    }

                    if (OptionsSFXUp.CurrentButtonState == ButtonSpriteState.Hover ||
                        OptionsSFXDown.CurrentButtonState == ButtonSpriteState.Hover)
                    {
                        if (CurrentMouseState.ScrollWheelValue < PreviousMouseState.ScrollWheelValue)
                        {
                            if (MenuSFXVolume > 0)
                            {
                                MenuSFXVolume--;
                                CurrentSettings.SFXVolume = MenuSFXVolume / 10;
                                SoundEffect.MasterVolume = CurrentSettings.SFXVolume;
                                MenuClick.Play();
                            }
                        }
                    }
                    #endregion

                    #region Music Volume
                    if (OptionsMusicUp.CurrentButtonState == ButtonSpriteState.Hover ||
                        OptionsMusicDown.CurrentButtonState == ButtonSpriteState.Hover)
                    {
                        if (CurrentMouseState.ScrollWheelValue > PreviousMouseState.ScrollWheelValue)
                        {
                            if (MenuMusicVolume < 10)
                            {
                                MenuMusicVolume++;
                                CurrentSettings.MusicVolume = MenuMusicVolume / 10;
                                SoundEffect.MasterVolume = CurrentSettings.MusicVolume;
                                MenuClick.Play();
                            }
                        }
                    }

                    if (OptionsMusicUp.CurrentButtonState == ButtonSpriteState.Hover ||
                        OptionsMusicDown.CurrentButtonState == ButtonSpriteState.Hover)
                    {
                        if (CurrentMouseState.ScrollWheelValue < PreviousMouseState.ScrollWheelValue)
                        {
                            if (MenuMusicVolume > 0)
                            {
                                MenuMusicVolume--;
                                CurrentSettings.MusicVolume = MenuMusicVolume / 10;
                                SoundEffect.MasterVolume = CurrentSettings.MusicVolume;
                                MenuClick.Play();
                            }
                        }
                    }
                    #endregion
                    #endregion
                }
                #endregion

                #region Handling GetName Button Presses
                if (GameState == GameState.GettingName && DialogVisible == false)
                {
                    GetNameBack.Update(CursorPosition, gameTime);
                    GetNameOK.Update(CursorPosition, gameTime);
                    NameInput.Update();
                    NameInput.Active = true;

                    if (GetNameBack.CurrentButtonState == ButtonSpriteState.Hover)
                    {
                        GetNameBack.NextPosition.X = 0;
                    }

                    if (GetNameBack.CurrentButtonState == ButtonSpriteState.Released)
                    {
                        GetNameBack.NextPosition.X = -50;
                    }

                    if (GetNameOK.CurrentButtonState == ButtonSpriteState.Hover)
                    {
                        GetNameOK.NextPosition.X = 1920 - 450;
                    }

                    if (GetNameOK.CurrentButtonState == ButtonSpriteState.Released)
                    {
                        GetNameOK.NextPosition.X = 1920 - 400;
                    }

                    if (GetNameOK.PlayHover == true)
                    {
                        MenuWoosh.Play();
                    }

                    if (GetNameBack.PlayHover == true)
                    {
                        MenuWoosh.Play();
                    }

                    if (GetNameBack.JustClicked == true)
                    {
                        ProfileBackButton.CurrentPosition.X = 1920 - 150;
                        GetNameBack.ResetState();
                        MenuColor = Color.White;
                        MenuClick.Play();
                        NameInput.TypePosition = 0;
                        NameInput.RealString = "";
                        GameState = GameState.ProfileSelect;
                    }

                    if ((GetNameOK.JustClicked == true) ||
                        (CurrentKeyboardState.IsKeyUp(Keys.Enter) && PreviousKeyboardState.IsKeyDown(Keys.Enter)))
                    {
                        GetNameOK.ResetState();

                        if (NameInput.RealString.Length < 3)
                        {
                            NameLengthDialog = new DialogBox(DialogBox, ShortButtonLeftSprite, ShortButtonRightSprite, RobotoRegular20_0, new Vector2(1920 / 2, 1080 / 2), "OK", "Your name is too short.", "");
                            NameLengthDialog.LoadContent();
                            DialogVisible = true;
                        }

                        if (NameInput.RealString.All(Char => Char == ' '))
                        {
                            NameLengthDialog = new DialogBox(DialogBox, ShortButtonLeftSprite, ShortButtonRightSprite, RobotoRegular20_0, new Vector2(1920 / 2, 1080 / 2), "OK", "Your name cannot be blank.", "");
                            NameLengthDialog.LoadContent();
                            DialogVisible = true;
                        }

                        if (NameInput.RealString.Length >= 3 && !NameInput.RealString.All(Char => Char == ' '))
                        {
                            GetNameOK.ResetState();
                            MenuColor = Color.White;
                            MenuClick.Play();
                            AddNewProfile();
                        }
                    }
                }
                #endregion                

                #region Handling Upgrades Menu Button Presses
                //if (GameState == GameState.Upgrades && DialogVisible == false)
                //{
                //    UpgradesBack.Update(CursorPosition, gameTime);

                //    foreach (Button button in UpgradeButtonList)
                //    {
                //        button.Update(CursorPosition, gameTime);

                //        int index = UpgradeButtonList.IndexOf(button);

                //        if (button.JustClicked == true)
                //        {
                //            switch (index)
                //            {
                //                case 0:
                //                    CurrentProfile.UpgradesList.Add(new Upgrade2());
                //                    break;
                //            }
                //        }
                //    }

                //    if (UpgradesBack.JustClicked == true)
                //    {
                //        GameState = GameState.ProfileManagement;
                //    }
                //}
                #endregion

                #region Handling DialogBox Button Presses
                if (ExitDialog != null)
                {
                    ExitDialog.Update(CursorPosition, gameTime);

                    if (ExitDialog.LeftButton.JustClicked == true)
                    {
                        MenuClick.Play();

                        if (GameState == GameState.Paused)
                            StorageDevice.BeginShowSelector(this.SaveProfile, null);

                        Content.Unload();
                        SecondaryContent.Unload();

                        this.Exit();
                    }

                    if (ExitDialog.RightButton.JustClicked == true)
                    {
                        MenuClick.Play();
                        DialogVisible = false;
                        ExitDialog = null;
                    }
                }

                if (DeleteProfileDialog != null)
                {
                    DeleteProfileDialog.Update(CursorPosition, gameTime);

                    if (DeleteProfileDialog.LeftButton.JustClicked == true)
                    {
                        MenuClick.Play();
                        StorageDevice.BeginShowSelector(this.DeleteProfile, null);
                        SetProfileNames();
                        DialogVisible = false;
                        DeleteProfileDialog = null;
                        return;
                    }

                    if (DeleteProfileDialog.RightButton.JustClicked == true)
                    {
                        MenuClick.Play();
                        DialogVisible = false;
                        DeleteProfileDialog = null;
                    }
                }

                if (ProfileMenuDialog != null)
                {
                    ProfileMenuDialog.Update(CursorPosition, gameTime);

                    if (ProfileMenuDialog.LeftButton.JustClicked == true)
                    {
                        MenuClick.Play();
                        StorageDevice.BeginShowSelector(this.SaveProfile, null);
                        DialogVisible = false;
                        ProfileMenuDialog = null;
                        ResetUpgrades();
                        //DrawableList.Clear();
                        UnloadGameContent();
                        GameState = GameState.ProfileManagement;
                        return;
                    }

                    if (ProfileMenuDialog.RightButton.JustClicked == true)
                    {
                        MenuClick.Play();
                        DialogVisible = false;
                        ProfileMenuDialog = null;
                    }
                }

                if (MainMenuDialog != null)
                {
                    MainMenuDialog.Update(CursorPosition, gameTime);

                    if (MainMenuDialog.LeftButton.JustClicked == true)
                    {
                        MenuClick.Play();
                        StorageDevice.BeginShowSelector(this.SaveProfile, null);
                        CurrentProfile = null;
                        ResetUpgrades();
                        //DrawableList.Clear();
                        UnloadGameContent();
                        DialogVisible = false;
                        MainMenuDialog = null;
                        GameState = GameState.Menu;
                        return;
                    }

                    if (MainMenuDialog.RightButton.JustClicked == true)
                    {
                        MenuClick.Play();
                        DialogVisible = false;
                        MainMenuDialog = null;
                    }
                }

                if (NoWeaponsDialog != null)
                {
                    NoWeaponsDialog.Update(CursorPosition, gameTime);

                    if (NoWeaponsDialog.LeftButton.JustClicked == true)
                    {
                        MenuClick.Play();
                        DialogVisible = false;
                        NoWeaponsDialog = null;
                    }
                }

                if (NameLengthDialog != null)
                {
                    NameLengthDialog.Update(CursorPosition, gameTime);

                    if (NameLengthDialog.LeftButton.JustClicked == true)
                    {
                        MenuClick.Play();
                        DialogVisible = false;
                        NameLengthDialog = null;
                    }
                }
                #endregion

                #region Handling Victory Screen Button Presses
                if (GameState == GameState.Victory)
                {
                    if (Victory == true)
                        VictoryContinue.Update(CursorPosition, gameTime);
                    else
                        VictoryRetry.Update(CursorPosition, gameTime);

                    if (VictoryContinue.JustClicked == true)
                    {
                        CurrentProfile.LevelNumber++;
                        StorageDevice.BeginShowSelector(this.SaveProfile, null);
                        GameState = GameState.ProfileManagement;
                    }
                }
                #endregion

                #region Load screen
                if (AllLoaded == true && GameState == GameState.Loading)
                {
                    //Move to the game if the play presses a key and all the content is loaded
                    if (CurrentKeyboardState.GetPressedKeys().Count() > 0)
                    {
                        IsLoading = false;
                        GameState = GameState.Playing;
                    }
                }
                #endregion
            }
        }

        private void SpecialAbilitiesButtonUpdate(GameTime gameTime)
        {
            foreach (Button button in SpecialAbilitiesButtonList)
            {
                button.Update(CursorPosition, gameTime);
            }
        }
        #endregion

        #region INVADER stuff that needs to be done every step
        private void InvaderUpdate(GameTime gameTime)
        {
            //This does all the stuff that would normally be in the Update call, but because this 
            //class became rather unwieldy, I broke up each call into separate smaller ones 
            //so that it's easier to manage
            foreach (Invader invader in InvaderList)
            {
                #region Check if this invader is being moused-over
                //////if (invader.DestinationRectangle.Contains(VectorToPoint(CursorPosition)))
                //////{
                //////    UIInvaderInfo = new UIInvaderInfo(invader);
                //////    UIInvaderInfo.Texture = WhiteBlock;
                //////    UIInvaderInfo.Font = BigUIFont;
                //////}

                //////if (InvaderList.All(Invader => !Invader.DestinationRectangle.Contains(VectorToPoint(CursorPosition))))
                //////{
                //////    UIInvaderInfo = null;
                //////}

                ////if (invader.DestinationRectangle.Contains(VectorToPoint(CursorPosition)) && UIInvaderOutlineList.Count == 0)
                ////{
                ////    UIOutline UIInvaderOutline = new UIOutline(invader.Position, new Vector2(invader.DestinationRectangle.Width, invader.DestinationRectangle.Height), null, null, invader);
                ////    UIInvaderOutline.OutlineTexture = TurretSelectBox;
                ////    UIInvaderOutlineList.Add(UIInvaderOutline);
                ////}

                ////if (!invader.DestinationRectangle.Contains(VectorToPoint(CursorPosition)))
                ////{
                ////    UIInvaderOutlineList.RemoveAll(invaderOutline => invaderOutline.Invader == invader);
                ////}

                ////foreach (UIOutline invaderOutline in UIInvaderOutlineList)
                ////{
                ////    invaderOutline.Position = invaderOutline.Invader.Position;
                ////}
                #endregion

                if (invader.CurrentHP < invader.PreviousHP)
                {
                    NumberChangeList.Add(new NumberChange(RobotoRegular20_0, invader.Position, new Vector2(0, -1.5f), 0 - (int)(invader.PreviousHP - invader.CurrentHP)));
                }

                invader.PreviousHP = invader.CurrentHP;
                invader.Update(gameTime, CursorPosition);

                #region This controls what happens when the invaders die, depending on their type
                if (invader.CurrentHP <= 0)
                {
                    for (int i = 0; i < invader.ResourceValue / 10; i++)
                    {
                        CoinList.Add(new Particle(Coin, new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                                    (float)RandomDouble(70, 110), (float)RandomDouble(6, 8), 200f, 1f, true, (float)RandomDouble(0, 360), (float)RandomDouble(-5, 5), 0.75f,
                                    Color.White, Color.White, 0.2f, true, MathHelper.Clamp(invader.MaxY + (float)RandomDouble(2, 32), 630, 960), false, null, true, true, false, false));
                    }

                    switch (invader.InvaderType)
                    {
                        case InvaderType.Soldier:
                            {
                                EmitterList2.Add(new Emitter(SplodgeParticle, 
                                new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                                new Vector2(0, 360), new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), 
                                new Vector2(1, 3), new Vector2(0.02f, 0.06f), RandomColor(Color.Red, Color.DarkRed, Color.DarkRed), 
                                RandomColor(Color.Red, Color.DarkRed, Color.DarkRed), 0.1f, 0.2f, 20, 10, true,
                                new Vector2(invader.MaxY, invader.MaxY), false, 1.0f, true, false, null, null, null, null, null, true, true));

                                EmitterList2.Add(new Emitter(SmokeParticle, 
                                new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                                new Vector2(0, 360), new Vector2(0.25f, 0.75f), new Vector2(20, 60), 1f, true, new Vector2(0, 360), 
                                new Vector2(0.5f, 2f), new Vector2(0.2f, 0.6f), RandomColor(Color.Red, Color.DarkRed, Color.DarkRed), 
                                RandomColor(Color.Red, Color.DarkRed, Color.DarkRed), 0.01f, 0.2f, 10, 1, true,
                                new Vector2(invader.MaxY, invader.MaxY), false, 1.0f, true, false, null, null, null, null, null, true, true));

                                Decal NewDecal = new Decal(BloodDecal1, new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Bottom),
                                                          (float)RandomDouble(0, 0), invader.YRange, invader.MaxY, 0.1f);

                                DecalList.Add(NewDecal);
                            }
                            break;

                        case InvaderType.SuicideBomber:
                            EmitterList2.Add(new Emitter(SplodgeParticle, 
                            new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                            new Vector2(0, 360), new Vector2(0.25f, 0.5f), new Vector2(25, 50), 0.5f, true, 
                            new Vector2(0, 360), new Vector2(1, 3), new Vector2(0.02f, 0.06f), Color.DarkRed, Color.Red, 
                            0.2f, 0.2f, 20, 10, true, new Vector2(invader.MaxY, invader.MaxY)));

                            EnemyExplosionList.Add(new Explosion(new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y), 100, invader.TowerAttackPower));
                            break;

                        case InvaderType.Spider:
                            EmitterList2.Add(new Emitter(SplodgeParticle, 
                            new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                            new Vector2(0, 360), new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), 
                            new Vector2(1, 3), new Vector2(0.02f, 0.06f), Color.Green, Color.Lime, 0.1f, 0.2f, 20, 10, true, 
                            new Vector2(invader.MaxY, invader.MaxY)));
                            break;

                        case InvaderType.Tank:
                            EnemyExplosionList.Add(new Explosion(new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y), 200, invader.TowerAttackPower));

                            Emitter ExplosionEmitter2 = new Emitter(SplodgeParticle, 
                                    new Vector2(invader.DestinationRectangle.Center.X, invader.MaxY), new Vector2(0, 180), new Vector2(1, 4), 
                                    new Vector2(20, 50), 2f, true, new Vector2(0, 360), new Vector2(1, 3), new Vector2(0.02f, 0.06f), 
                                    Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 0.2f, 20, 10, true, 
                                    new Vector2(invader.MaxY + 8, invader.MaxY + 8));

                            YSortedEmitterList.Add(ExplosionEmitter2);
                           // DrawableList.Add(ExplosionEmitter2);

                            Emitter ExplosionEmitter = new Emitter(FireParticle,
                                    new Vector2(invader.DestinationRectangle.Center.X, invader.MaxY),
                                    new Vector2(0, 180), new Vector2(3, 8), new Vector2(1, 15), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 20, false,
                                    new Vector2(0, 1080), true);

                            YSortedEmitterList.Add(ExplosionEmitter);
                            //DrawableList.Add(ExplosionEmitter);
                            //Color SmokeColor1 = Color.Lerp(Color.DarkGray, Color.Transparent, 0.5f);
                            //Color SmokeColor2 = Color.Lerp(Color.Gray, Color.Transparent, 0.5f);

                            Emitter newEmitter2 = new Emitter(SmokeParticle,
                                    new Vector2(invader.DestinationRectangle.Center.X, invader.MaxY),
                                    new Vector2(80, 100), new Vector2(0.5f, 1f), new Vector2(20, 40), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(0, 0), new Vector2(1, 2), SmokeColor1, SmokeColor2, 0.0f, 0.3f, 1, 1, false,
                                    new Vector2(0, 1080), false);

                            EmitterList2.Add(newEmitter2);
                            //DrawableList.Add(newEmitter2);
                            break;

                        case InvaderType.Airship:
                            EmitterList2.Add(new Emitter(SplodgeParticle, 
                                new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                                new Vector2(0, 360), new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), 
                                new Vector2(1, 3), new Vector2(0.02f, 0.06f), Color.DarkRed, Color.Red, 
                                0.1f, 0.2f, 20, 10, true, new Vector2(525, 630)));
                            break;

                        case InvaderType.Slime:
                            EmitterList2.Add(new Emitter(SplodgeParticle, 
                                new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                                new Vector2(0, 360), new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), 
                                new Vector2(1, 3), new Vector2(0.02f, 0.06f), Color.HotPink, Color.LightPink, 0.1f, 0.2f, 
                                20, 10, true, new Vector2(invader.MaxY, invader.MaxY)));
                            break;
                    }

                    invader.Active = false;
                    Resources += invader.ResourceValue;
                }
                #endregion

                #region This controls the gravity and the ground collisions for the invader if they aren't an airborne type
                if (invader.Airborne == false)
                {
                    if ((invader.DestinationRectangle.Bottom + invader.Velocity.Y) > invader.MaxY)
                    {
                        invader.Velocity.Y = 0;
                        invader.Gravity = 0;
                        invader.Position = new Vector2(invader.Position.X, invader.MaxY - invader.DestinationRectangle.Height);
                        invader.InAir = false;
                    }

                    if (invader.DestinationRectangle.Bottom < invader.MaxY)
                    {
                        invader.Gravity = 0.2f;
                    }
                }
                #endregion

                HeavyRangedInvader rangedInvader = invader as HeavyRangedInvader;

                if (rangedInvader != null)
                {
                    switch (rangedInvader.InvaderType)
                    {
                        case InvaderType.Archer:
                            {
                                var value = Vector2.Distance(rangedInvader.Position, new Vector2(Tower.DestinationRectangle.Left, Tower.DestinationRectangle.Bottom));

                                if (value <= rangedInvader.MinDistance)
                                {
                                    rangedInvader.Velocity.X = 0;
                                    rangedInvader.InRange = true;
                                }
                            }
                            break;

                        case InvaderType.StationaryCannon:
                            {
                                var value = Vector2.Distance(rangedInvader.Position, new Vector2(Tower.DestinationRectangle.Left, Tower.DestinationRectangle.Bottom));

                                if (value <= rangedInvader.MinDistance)
                                {
                                    rangedInvader.Velocity.X = 0;
                                    rangedInvader.InRange = true;
                                }
                            }
                            break;
                    }
                }
            }

            foreach (Emitter emitter in GasEmitterList)
            {
                foreach (Invader invader in InvaderList)
                {
                    if (emitter.ParticleList.Any(Particle => Particle.DestinationRectangle.Intersects(invader.DestinationRectangle) && 
                        invader.DestinationRectangle.Bottom < Particle.DestinationRectangle.Bottom &&
                        Particle.CurrentTransparency > 0.05f))
                    {
                        invader.DamageOverTime(new DamageOverTimeStruct() 
                        { 
                            Color = Color.LawnGreen, 
                            Damage = 3, 
                            InitialDamage = 30, 
                            Interval = 50, 
                            Milliseconds = 4000 
                        }, Color.LawnGreen);
                    }
                }
            }
                       

            #region This controls what happens when an explosion happens near the invader
            if (ExplosionList.Count > 0)
                foreach (Explosion explosion in ExplosionList)
                {
                    ////Change the properties of the explosions based on powerups.
                    //foreach (Powerup powerup in PowerupsList)
                    //{
                    //    switch (powerup.CurrentEffect.GeneralPowerup)
                    //    {
                    //        case GeneralPowerup.ExplosivePower:
                    //            explosion.Damage = (float)PercentageChange(explosion.Damage, powerup.CurrentEffect.PowerupValue);
                    //            break;

                    //        case GeneralPowerup.BlastRadii:
                    //            explosion.BlastRadius = (float)PercentageChange(explosion.BlastRadius, powerup.CurrentEffect.PowerupValue);
                    //            break;
                    //    }
                    //}
                    ////explosion.BlastRadius = explosion.BlastRadius * PowerUp.BlastRadiusValue;

                    List<Trap> WallList = new List<Trap>();
                    WallList = TrapList.FindAll(Trap => Trap.TrapType == TrapType.Wall);

                    InvaderList = InvaderList.OrderBy(Invader => Vector2.Distance(PointToVector(Invader.DestinationRectangle.Center), explosion.Position)).ToList();

                    foreach (Invader invader in InvaderList)
                    {
                        if (limit < 5)
                        {
                            //Distance between the explosion and the centre of the invader
                            float InvaderToExpl = Vector2.Distance(PointToVector(invader.DestinationRectangle.Center), explosion.Position);

                            Vector3 RayDirection = new Vector3(PointToVector(invader.DestinationRectangle.Center), 0) - new Vector3(explosion.Position, 0);
                            RayDirection.Normalize();

                            Ray ExplosionRay = new Ray(new Vector3(explosion.Position, 0), RayDirection);

                            if (WallList.Count < 1 && InvaderToExpl <= explosion.BlastRadius)
                            {
                                double Damage = explosion.Damage - (InvaderToExpl / (2 * explosion.BlastRadius)) * explosion.Damage;
                                invader.CurrentHP -= (float)Damage;

                                Vector2 DirectionToInvader = explosion.Position - new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y);
                                switch (invader.InvaderType)
                                {
                                    case InvaderType.Soldier:
                                        Emitter BloodEmitter = new Emitter(SplodgeParticle,
                                            new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                                            new Vector2(
                                            MathHelper.ToDegrees(-(float)Math.Atan2(-DirectionToInvader.Y, -DirectionToInvader.X)) - (float)RandomDouble(0, 45),
                                            MathHelper.ToDegrees(-(float)Math.Atan2(-DirectionToInvader.Y, -DirectionToInvader.X)) + (float)RandomDouble(0, 45)),
                                            new Vector2((float)(0.1 * Damage), (float)(0.23 * Damage)), new Vector2(80, 130), 0.5f, true, new Vector2(0, 360),
                                            new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.Red, Color.DarkRed,
                                            0.1f, 0.1f, 3, 5, true, new Vector2(invader.MaxY - 8, invader.MaxY + 8), false, 1, true, false,
                                            null, null, null, null, null, true, true, 1000);

                                        YSortedEmitterList.Add(BloodEmitter);
                                        //DrawableList.Add(BloodEmitter);
                                        break;                                        
                                }
                            }

                            if (InvaderToExpl <= explosion.BlastRadius)
                            {
                                if (WallList.Any(Wall => Wall.BoundingBox.Intersects(ExplosionRay) > InvaderToExpl) ||
                                    WallList.Any(Wall => Wall.BoundingBox.Intersects(ExplosionRay) == null))
                                {
                                    double Damage = explosion.Damage - (InvaderToExpl / (2 * explosion.BlastRadius)) * explosion.Damage;
                                    invader.CurrentHP -= (float)Damage;

                                    Vector2 DirectionToInvader = explosion.Position - new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y);
                                    switch (invader.InvaderType)
                                    {
                                        case InvaderType.Soldier:
                                            Emitter BloodEmitter = new Emitter(SplodgeParticle,
                                            new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                                            new Vector2(
                                            MathHelper.ToDegrees(-(float)Math.Atan2(-DirectionToInvader.Y, -DirectionToInvader.X)) - (float)RandomDouble(0, 45),
                                            MathHelper.ToDegrees(-(float)Math.Atan2(-DirectionToInvader.Y, -DirectionToInvader.X)) + (float)RandomDouble(0, 45)),
                                            new Vector2((float)(0.1 * Damage), (float)(0.23 * Damage)), new Vector2(80, 130), 0.5f, true, new Vector2(0, 360),
                                            new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.Red, Color.DarkRed,
                                            0.1f, 0.1f, 3, 5, true, new Vector2(invader.MaxY - 8, invader.MaxY + 8), false, 1, true, false,
                                            null, null, null, null, null, true, true, 1000);

                                            YSortedEmitterList.Add(BloodEmitter);
                                            //DrawableList.Add(BloodEmitter);
                                            break;
                                    }
                                }
                            }

                            limit++;
                        }
                    }

                    limit = 0;
                    explosion.Active = false;
                }
            #endregion
        }


        private void AttackTraps()
        {
            foreach (Invader invader in InvaderList)
            {
                foreach (Trap trap in TrapList)
                {
                    if (invader.DestinationRectangle.Intersects(trap.DestinationRectangle))
                    {
                        switch (invader.InvaderType)
                        {
                            #region Soldier
                            case InvaderType.Soldier:
                                switch (invader.Behaviour)
                                {
                                    #region Default - If the invader should be attacking the tower or turrets, then only stop if the trap is solid
                                    default:
                                        if (trap.Solid == true)
                                        {
                                            invader.Velocity.X = 0;

                                            if (invader.CanAttack == true)
                                            {
                                                trap.CurrentHP -= invader.TrapAttackPower;
                                            }
                                        }
                                        break;
                                    #endregion

                                    #region Attack Traps - stop at the traps regardless of whether they're solid or not
                                    case InvaderBehaviour.AttackTraps:
                                        invader.Velocity.X = 0;

                                        if (invader.CanAttack == true)
                                        {
                                            trap.CurrentHP -= invader.TrapAttackPower;
                                        }
                                        break;
                                    #endregion
                                }
                                break;
                            #endregion
                        }
                    }
                }

                #region If an invader is not colliding with a wall trap or the tower, move as normal

                //If the invader is not colliding with the tower
                if (Vector2.Distance(
                    PointToVector(invader.DestinationRectangle.Center),
                    new Vector2(Tower.DestinationRectangle.Right, invader.DestinationRectangle.Center.Y)) > 5)
                {
                    //If the invader is not colliding with a trap
                    if (!TrapList.Any(Trap => Trap.DestinationRectangle.Intersects(invader.DestinationRectangle)))
                    {
                        if (invader.GetType().BaseType.Name != "HeavyRangedInvader")
                        {
                            if (invader.InAir == false)
                            {
                                if (invader.Slow == false)
                                    invader.Velocity.X = (float)(invader.Direction.X * invader.Speed);
                                else
                                    invader.Velocity.X = (float)(invader.Direction.X * invader.SlowSpeed);
                            }
                        }
                        else
                        //If it is a heavy ranged invader, check to see if it's in/out of range before changing speed
                        {
                            HeavyRangedInvader rangedInvader = invader as HeavyRangedInvader;

                            var value = Vector2.Distance(rangedInvader.Position, new Vector2(Tower.DestinationRectangle.Left, Tower.DestinationRectangle.Bottom));

                            if (value <= rangedInvader.MinDistance)
                            {
                                rangedInvader.Velocity.X = 0;
                                rangedInvader.InRange = true;
                            }
                            else
                            {
                                rangedInvader.InRange = false;

                                if (rangedInvader.InAir == false)
                                {
                                    if (rangedInvader.Slow == false)
                                        rangedInvader.Velocity.X = (float)(invader.Direction.X * invader.Speed);
                                    else
                                        rangedInvader.Velocity.X = (float)(invader.Direction.X * invader.SlowSpeed);
                                }
                            }
                            //if (value > rangedInvader.Ran
                        }
                    }
                }
                //if (!TrapList.Any(Trap =>
                //    Trap.DestinationRectangle.Intersects(invader.DestinationRectangle) &&
                //    Trap.Solid == true) &&
                //    Vector2.Distance(
                //    PointToVector(invader.DestinationRectangle.Center),
                //    new Vector2(Tower.DestinationRectangle.Right, invader.DestinationRectangle.Center.Y)) > 5)
                //{
                //    if (invader.Slow == false)
                //        invader.Velocity.X = (float)(invader.Direction.X * invader.Speed);
                //    else
                //        invader.Velocity.X = (float)(invader.Direction.X * invader.SlowSpeed);
                //}
                #endregion
            }
        }

        private void AttackTower()
        {
            foreach (Invader invader in InvaderList)
            {
                if (Vector2.Distance(
                    PointToVector(invader.DestinationRectangle.Center),
                    new Vector2(Tower.DestinationRectangle.Right, invader.DestinationRectangle.Center.Y)) <= 5)
                {
                    invader.Velocity.X = 0;

                    if (invader.CanAttack == true)
                        Tower.TakeDamage(invader.TowerAttackPower);

                    //ShieldBubbleEffect.Parameters["EffectStrength"].SetValue(1);
                }
            }
        }

        private void RangedAttackTower(GameTime gameTime)
        {
            #region Make heavy ranged invaders fire
            foreach (Invader invader in InvaderList)
            {
                HeavyRangedInvader rangedInvader = invader as HeavyRangedInvader;


                if (rangedInvader != null && rangedInvader.CanAttack == true &&
                   (rangedInvader.DestinationRectangle.Left - Tower.DestinationRectangle.Right) > rangedInvader.DistanceRange.X &&
                   (rangedInvader.DestinationRectangle.Left - Tower.DestinationRectangle.Right) < rangedInvader.DistanceRange.Y)
                {
                    switch (rangedInvader.InvaderType)
                    {
                        case InvaderType.Spider:
                            {
                                HeavyProjectile heavyProjectile = new AcidProjectile(AcidSprite, RoundSparkParticle,
                                new Vector2(rangedInvader.DestinationRectangle.Center.X, rangedInvader.DestinationRectangle.Center.Y),
                                Random.Next((int)(rangedInvader.PowerRange.X), (int)(rangedInvader.PowerRange.Y)),
                                -MathHelper.ToRadians(Random.Next((int)(rangedInvader.AngleRange.X), (int)(rangedInvader.AngleRange.Y))),
                                0.2f, rangedInvader.RangedAttackPower);

                                heavyProjectile.YRange = new Vector2(invader.Bottom, invader.Bottom);
                                heavyProjectile.Initialize();
                                InvaderHeavyProjectileList.Add(heavyProjectile);
                            }
                            break;

                        case InvaderType.Archer:
                            {
                                HeavyProjectile heavyProjectile = new AcidProjectile(AcidSprite, RoundSparkParticle,
                                new Vector2(rangedInvader.DestinationRectangle.Center.X, rangedInvader.DestinationRectangle.Center.Y),
                                Random.Next((int)(rangedInvader.PowerRange.X), (int)(rangedInvader.PowerRange.Y)),
                                -MathHelper.ToRadians(Random.Next((int)(rangedInvader.AngleRange.X), (int)(rangedInvader.AngleRange.Y))),
                                0.2f, rangedInvader.RangedAttackPower);

                                heavyProjectile.YRange = new Vector2(invader.Bottom, invader.Bottom);
                                heavyProjectile.Initialize();
                                InvaderHeavyProjectileList.Add(heavyProjectile);
                            }
                            break;
                    }
                }
            }
            #endregion

            #region Make light ranged invaders fire
            foreach (Invader invader in InvaderList)
            {
                LightRangedInvader rangedInvader = invader as LightRangedInvader;

                if (rangedInvader != null)
                {
                    float DistToTower = Vector2.Distance(rangedInvader.Position, new Vector2(Tower.DestinationRectangle.Left, Tower.DestinationRectangle.Bottom));

                    if (DistToTower > rangedInvader.Range.Y)
                    {
                        rangedInvader.CurrentAttackDelay = 0;
                    }

                    if (rangedInvader.CanAttack == true)
                    {
                        switch (rangedInvader.InvaderType)
                        {
                            case InvaderType.TestInvader:
                                {
                                    if (rangedInvader.CurrentBurst < rangedInvader.MaxBurst)
                                    {
                                        double angle = -MathHelper.ToRadians((float)RandomDouble(rangedInvader.AngleRange.X, rangedInvader.AngleRange.Y));
                                        Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                                        LightProjectile lightProjectile = new MachineGunProjectile(rangedInvader.Position, direction, rangedInvader.RangedAttackPower);
                                        InvaderLightProjectileList.Add(lightProjectile);
                                        rangedInvader.CurrentBurst++;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            #endregion
        }

        private void RangedAttackTraps()
        {
            //Handle the Enemy Projectiles when they hit a trap, based on the traptype and the projectiletype

            //foreach (HeavyProjectile heavyProjectile in InvaderHeavyProjectileList)
            //{
            //    if (TrapList.Any(Trap => Trap.DestinationRectangle.Intersects(heavyProjectile.CollisionRectangle))
            //        && heavyProjectile.Active == true)
            //    {
            //        int index = TrapList.IndexOf(TrapList.First(Trap => Trap.DestinationRectangle.Intersects(heavyProjectile.CollisionRectangle)));

            //        switch (TrapList[index].TrapType)
            //        {
            //            #region Wall
            //            case TrapType.Wall:
            //                {
            //                    switch (heavyProjectile.HeavyProjectileType)
            //                    {
            //                        case HeavyProjectileType.Acid:
            //                            {
            //                                Emitter newEmitter = new Emitter(SplodgeParticle, new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
            //                                    new Vector2(-(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) - 45,
            //                                        -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) + 45),
            //                                        new Vector2(2, 3), new Vector2(100, 200), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
            //                                    new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true,
            //                                    new Vector2(TrapList[index].DestinationRectangle.Bottom, TrapList[index].DestinationRectangle.Bottom));
            //                                EmitterList2.Add(newEmitter);
            //                                EmitterList2[EmitterList2.IndexOf(newEmitter)].LoadContent(Content);
            //                                heavyProjectile.Active = false;
            //                                TrapList[index].CurrentHP -= heavyProjectile.Damage;
            //                                heavyProjectile.Emitter.AddMore = false;
            //                            }
            //                            break;

            //                        case HeavyProjectileType.Arrow:

            //                            break;

            //                        case HeavyProjectileType.CannonBall:

            //                            break;

            //                        case HeavyProjectileType.FlameThrower:

            //                            break;
            //                    }

            //                }
            //                break;
            //            #endregion

            //            case TrapType.Barrel:
            //                {

            //                }
            //                break;

            //            case TrapType.Catapult:
            //                {

            //                }
            //                break;

            //            case TrapType.Fire:
            //                {

            //                }
            //                break;

            //            case TrapType.Ice:
            //                {

            //                }
            //                break;

            //            case TrapType.Spikes:
            //                {

            //                }
            //                break;

            //            case TrapType.Tar:
            //                {

            //                }
            //                break;
            //        }
            //    }
            //}
        }


        private void InvaderHeavyProjectileUpdate(GameTime gameTime)
        {
            foreach (HeavyProjectile heavyProjectile in InvaderHeavyProjectileList)
            {
                heavyProjectile.Update(gameTime);

                #region What happens when an enemy projectile hits the ground
                if (heavyProjectile.Position.Y > heavyProjectile.MaxY && heavyProjectile.Active == true)
                {
                    switch (heavyProjectile.HeavyProjectileType)
                    {
                        case HeavyProjectileType.Acid:
                            {
                                Emitter newEmitter = new Emitter(SplodgeParticle, new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                new Vector2(0, 180), new Vector2(2, 3), new Vector2(10, 25), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                new Vector2(0.02f, 0.06f), Color.Lime, Color.LimeGreen, 0.2f, 0.2f, 20, 10, true, new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 8));
                                EmitterList2.Add(newEmitter);
                            }
                            break;

                        case HeavyProjectileType.CannonBall:
                            {
                                Color DirtColor = new Color();
                                DirtColor.A = 150;
                                DirtColor.R = 51;
                                DirtColor.G = 31;
                                DirtColor.B = 0;

                                Color DirtColor2 = DirtColor;
                                DirtColor2.A = 125;

                                Emitter newEmitter = new Emitter(SplodgeParticle, new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                    new Vector2(0, 180), new Vector2(2, 3), new Vector2(10, 25), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                    new Vector2(0.02f, 0.06f), DirtColor, DirtColor2, 0.2f, 0.2f, 20, 10, true, new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 8));
                                EmitterList2.Add(newEmitter);
                            }
                            break;
                    }

                    #region Deactivate the enemy projectile
                    foreach (Emitter emitter in heavyProjectile.EmitterList)
                    {
                        emitter.AddMore = false;
                    }
                    heavyProjectile.Active = false;
                    heavyProjectile.Velocity = Vector2.Zero;
                    #endregion
                }
                #endregion

                if (TurretList.Any(turret => turret != null && turret.BarrelRectangle.Intersects(heavyProjectile.DestinationRectangle)))
                {
                    //if (heavyProjectile.Active == true)
                    //{
                    //    Turret hitTurret = TurretList[TurretList.FindIndex(turret => turret.BarrelRectangle.Intersects(heavyProjectile.DestinationRectangle))];
                    //    hitTurret.CurrentHealth -= heavyProjectile.Damage;

                    //    heavyProjectile.Active = false;
                    //    heavyProjectile.Emitter.AddMore = false;
                    //    heavyProjectile.Velocity = Vector2.Zero;
                    //}
                    //hitTurret.Active = false;

                    //TowerButtonList[TurretList.IndexOf(hitTurret)].ButtonActive = true;
                    //TurretList[TurretList.IndexOf(hitTurret)] = null;
                }

                #region What happens when an enemy projectiles hits the tower
                if (heavyProjectile.MaxY < Tower.DestinationRectangle.Bottom)
                    if (heavyProjectile.DestinationRectangle.Intersects(Tower.DestinationRectangle)
                        && heavyProjectile.Active == true)
                    {
                        switch (heavyProjectile.HeavyProjectileType)
                        {
                            case HeavyProjectileType.Acid:
                                {
                                    Tower.TakeDamage(heavyProjectile.Damage);
                                    Emitter newEmitter = new Emitter(SplodgeParticle, new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                                    new Vector2(-(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) - 45,
                                    -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) + 45),
                                    new Vector2(2, 3), new Vector2(100, 200), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                    new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true,
                                    new Vector2(Tower.DestinationRectangle.Bottom, Tower.DestinationRectangle.Bottom));

                                    EmitterList2.Add(newEmitter);
                                }
                                break;

                            case HeavyProjectileType.Arrow:
                                {

                                }
                                break;

                            case HeavyProjectileType.CannonBall:
                                {
                                    Tower.TakeDamage(heavyProjectile.Damage);
                                    Emitter newEmitter = new Emitter(SplodgeParticle, new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                                    new Vector2(-(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) - 45,
                                    -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) + 45),
                                    new Vector2(2, 3), new Vector2(100, 200), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                    new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true,
                                    new Vector2(Tower.DestinationRectangle.Bottom, Tower.DestinationRectangle.Bottom));

                                    EmitterList2.Add(newEmitter);
                                }
                                break;

                            case HeavyProjectileType.FlameThrower:
                                {

                                }
                                break;
                        }

                        #region Deactivate the enemy projectile
                        heavyProjectile.Active = false;
                        foreach (Emitter emitter in heavyProjectile.EmitterList)
                        {
                            emitter.AddMore = false;
                        }
                        heavyProjectile.Velocity = Vector2.Zero;
                        #endregion
                    }
                #endregion
            }
        }

        private void InvaderLightProjectileUpdate(GameTime gameTime)
        {
            #region Determine what effect to create
            Action<Vector2, Vector2, LightProjectileType> CreateEffect = (Vector2 CollisionStart, Vector2 CollisionEnd, LightProjectileType LightProjectileType) =>
            {
                switch (LightProjectileType)
                {
                    #region MachineGun
                    case LightProjectileType.MachineGun:
                        Trail = new BulletTrail(CollisionStart, CollisionEnd);
                        Trail.Segment = BulletTrailSegment;
                        Trail.Cap = BulletTrailCap;
                        Trail.SetUp();
                        InvaderTrailList.Add(Trail);
                        break;
                    #endregion

                    #region Lightning
                    case LightProjectileType.Lightning:
                        for (int i = 0; i < 5; i++)
                        {
                            Lightning = new LightningBolt(CollisionStart, CollisionEnd, Color.MediumPurple, 0.02f);
                            LightningList.Add(Lightning);
                        }
                        break;
                    #endregion

                    #region Beam
                    case LightProjectileType.Beam:
                        for (int i = 0; i < 5; i++)
                        {
                            Lightning = new LightningBolt(CollisionStart, CollisionEnd, RandomColor(Color.Red, Color.Orange), 0.02f, 1);
                            LightningList.Add(Lightning);
                        }

                        for (int i = 0; i < 5; i++)
                        {
                            Bolt = new LightningBolt(CollisionStart, CollisionEnd, RandomColor(Color.Red, Color.Orange), 0.02f, 200);
                            LightningList.Add(Bolt);
                        }
                        break;
                    #endregion

                    #region Freeze
                    case LightProjectileType.Freeze:
                        Emitter FlashSparks = new Emitter(BallParticle, CollisionStart,
                        new Vector2(0, 360), new Vector2(1, 3), new Vector2(5, 15), 1f, true, new Vector2(0, 360),
                        new Vector2(2, 5), new Vector2(0.25f, 0.25f), Color.Blue, Color.DeepSkyBlue, 0.0f, 0.1f, 1, 10,
                        false, new Vector2(0, 1080), false, null, false, false);

                        Emitter FlashSmoke = new Emitter(SmokeParticle, CollisionStart,
                        new Vector2(0, 360), new Vector2(1, 2), new Vector2(5, 15), 1f, true, new Vector2(0, 360),
                        new Vector2(2, 5), new Vector2(1f, 2f), Color.Blue, Color.DeepSkyBlue, 0.0f, 0.1f, 1, 1,
                        false, new Vector2(0, 1080), false, null, false, false);

                        AlphaEmitterList.Add(FlashSparks);
                        AlphaEmitterList.Add(FlashSmoke);

                        for (int i = 0; i < 15; i++)
                        {
                            Lightning = new LightningBolt(CollisionStart, CollisionEnd, RandomColor(Color.Blue, Color.White), 0.02f, 1);
                            LightningList.Add(Lightning);
                        }

                        for (int i = 0; i < 5; i++)
                        {
                            Bolt = new LightningBolt(CollisionStart, CollisionEnd, RandomColor(Color.Blue, Color.White), 0.02f, 200);
                            LightningList.Add(Bolt);
                        }
                        break;
                    #endregion

                    #region Shotgun
                    case LightProjectileType.Shotgun:
                        Trail = new BulletTrail(CollisionStart, CollisionEnd);
                        Trail.Segment = BulletTrailSegment;
                        Trail.Cap = BulletTrailCap;
                        Trail.SetUp();
                        InvaderTrailList.Add(Trail);
                        break;
                    #endregion
                }
            };
            #endregion

            foreach (LightProjectile lightProjectile in InvaderLightProjectileList)
            {
                //Check if the light projectile hits a trap or the tower or a turret here.
                CurrentInvaderProjectile = lightProjectile;
                Vector2 CollisionEnd;

                if (CurrentInvaderProjectile != null)
                {
                    //If the projectile intersects the tower, create the correct effect
                    if (Tower.BoundingBox.Intersects(CurrentInvaderProjectile.Ray) != null)
                    {
                        var DistToTower = Tower.BoundingBox.Intersects(CurrentInvaderProjectile.Ray);

                        CollisionEnd = new Vector2(CurrentInvaderProjectile.Position.X + (CurrentInvaderProjectile.Ray.Direction.X * (float)DistToTower),
                                                   CurrentInvaderProjectile.Position.Y + (CurrentInvaderProjectile.Ray.Direction.Y * (float)DistToTower));

                        CreateEffect(CurrentInvaderProjectile.Position, CollisionEnd, CurrentInvaderProjectile.LightProjectileType);

                        Emitter SparkEmitter = new Emitter(SparkParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                        new Vector2(
                        MathHelper.ToDegrees(-(float)Math.Atan2(-CurrentInvaderProjectile.Ray.Direction.Y, -CurrentInvaderProjectile.Ray.Direction.X)) - (float)RandomDouble(0, 45),
                        MathHelper.ToDegrees(-(float)Math.Atan2(-CurrentInvaderProjectile.Ray.Direction.Y, -CurrentInvaderProjectile.Ray.Direction.X)) + (float)RandomDouble(0, 45)),
                        new Vector2(1, 3), new Vector2(30, 60), 0.5f, true, new Vector2(0, 360),
                        new Vector2(1, 1), new Vector2(1f, 0.5f), Color.Yellow, Color.White,
                        0.1f, 0.1f, 1, 5, true, new Vector2(Tower.DestinationRectangle.Bottom, Tower.DestinationRectangle.Bottom), false, 1, true, true, null, null, null, true);

                        AdditiveEmitterList.Add(SparkEmitter);

                        Tower.CurrentHP -= CurrentInvaderProjectile.Damage;
                    }

                    //If the tower doesn't hit the tower, the ground or a solid trap
                    if (CurrentInvaderProjectile.Active == true && Tower.BoundingBox.Intersects(CurrentInvaderProjectile.Ray) == null)
                    {
                        CollisionEnd = new Vector2(CurrentInvaderProjectile.Position.X + (CurrentInvaderProjectile.Ray.Direction.X * 1920),
                                                   CurrentInvaderProjectile.Position.Y + (CurrentInvaderProjectile.Ray.Direction.Y * 1920));

                        CreateEffect(CurrentInvaderProjectile.Position, CollisionEnd, CurrentInvaderProjectile.LightProjectileType);
                    }

                    CurrentInvaderProjectile = null;
                    lightProjectile.Active = false;
                }
            }

            //for (int i = 0; i < InvaderLightProjectileList.Count; i++)
            //{
            //    if (InvaderLightProjectileList[i].Active == false)
            //        InvaderLightProjectileList.RemoveAt(i);
            //}
        }


        private void EnemyExplosionsUpdate(GameTime gameTime)
        {
            if (EnemyExplosionList.Count > 0)
            {
                foreach (Explosion explosion in EnemyExplosionList)
                {
                    if (Vector2.Distance(new Vector2(Tower.DestinationRectangle.Right, explosion.Position.Y), explosion.Position) < explosion.BlastRadius &&
                        explosion.Active == true)
                    {
                        float Distance = Vector2.Distance(new Vector2(Tower.DestinationRectangle.Right, Tower.DestinationRectangle.Bottom), explosion.Position);
                        Tower.TakeDamage((int)(explosion.Damage / 100 * (100 - (100 / explosion.BlastRadius) * Distance)));
                    }

                    //Add in the code here that will damage the traps when there's an explosion near the trap

                    foreach (Trap trap in TrapList)
                    {
                        float Distance = Vector2.Distance(new Vector2(trap.DestinationRectangle.Center.X, explosion.Position.Y), explosion.Position);

                        if (Distance < explosion.BlastRadius && explosion.Active == true)
                        {
                            switch (trap.TrapType)
                            {
                                case TrapType.Barrel:
                                    trap.CurrentHP -= ((int)(explosion.Damage / 100 * (100 - (100 / explosion.BlastRadius) * Distance)));
                                    break;
                            }
                        }
                    }

                    explosion.Active = false;
                }
            }
        }
        #endregion

        #region TURRET stuff that needs to be called every step
        private void TurretUpdate(GameTime gameTime)
        {
            #region Using the keyboard to select a turret
            if (CurrentKeyboardState.IsKeyDown(Keys.D1))
            {
                if (TurretList[0] != null)
                {
                    ClearTurretSelect();
                    TurretList[0].Selected = true;
                    CurrentTurret = TurretList[0];
                }
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.D2))
            {
                if (TurretList[1] != null)
                {
                    ClearTurretSelect();
                    TurretList[1].Selected = true;
                    CurrentTurret = TurretList[1];
                }
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.D3))
            {
                if (TurretList[2] != null)
                {
                    ClearTurretSelect();
                    TurretList[2].Selected = true;
                    CurrentTurret = TurretList[2];
                }
            }
            #endregion

            #region Handle overheat effects
            if (TurretList.Any(Turret => Turret != null && Turret.Overheated == true))
            {
                if (TurretOverheatInstance == null)
                {
                    foreach (Turret turret in TurretList)
                    {
                        if (turret != null && turret.Overheated == true)
                        {
                            Color SmokeColor = Color.Gray;
                            SmokeColor.A = 200;

                            Color SmokeColor2 = Color.WhiteSmoke;
                            SmokeColor.A = 175;

                            Emitter SmokeEmitter = new Emitter(SmokeParticle, new Vector2(turret.Position.X, turret.Position.Y - turret.BarrelRectangle.Height / 2),
                                        new Vector2(70, 110), new Vector2(0.2f, 0.5f), new Vector2(250, 350), 0.05f, true, new Vector2(-20, 20),
                                        new Vector2(-4, 4), new Vector2(0.5f, 0.5f), SmokeColor, SmokeColor2, 0.0f, turret.MaxHeatTime / 1000, 200, 1, false,
                                        new Vector2(0, 1080), false, CursorPosition.Y / 1080);

                            turret.EmitterList.Add(SmokeEmitter);
                        }
                    }

                    TurretOverheatInstance = TurretOverheat.CreateInstance();
                    TurretOverheatInstance.IsLooped = true;
                    TurretOverheatInstance.Play();
                }
            }

            if (TurretList.Any(Turret => Turret != null) &&
                TurretOverheatInstance != null &&
                TurretOverheatInstance.State == SoundState.Playing)
            {
                List<Turret> NotNullTurrets = TurretList.FindAll(Turret => Turret != null);

                if (NotNullTurrets.All(Turret => Turret.Overheated == false) && TurretOverheatInstance.Volume > 0)
                {
                    TurretOverheatInstance.Volume -= 0.05f;
                }
            }

            if (TurretOverheatInstance != null && TurretOverheatInstance.Volume <= 0.05f)
            {
                TurretOverheatInstance.Stop();
                TurretOverheatInstance = null;
            }
            #endregion

            foreach (Turret turret in TurretList)
            {
                if (turret != null && turret.Active == true)
                {
                    turret.Update(gameTime, CursorPosition);

                    #region If the left mouse button is pressed, shoot the turret
                    if (turret.Selected == true)
                    {
                        switch (turret.TurretFireType)
                        {
                            #region Full Auto
                            case TurretFireType.FullAuto:
                                if (CurrentMouseState.LeftButton == ButtonState.Pressed)
                                {
                                    #region If the turret selectbox currently contains the mouse cursor, don't allow that turret to shoot
                                    var fullTurrets = TurretList.Where(FullTurret => FullTurret != null).ToList();

                                    if (fullTurrets.Any(Turret => Turret.SelectBox.Contains(VectorToPoint(CursorPosition))))
                                    {
                                        break;
                                    }
                                    #endregion

                                    if (turret.CanShoot == true && this.IsActive == true)
                                    {
                                        turret.ElapsedTime = 0;
                                        turret.Animated = true;

                                        if (turret.CurrentAnimation.TotalFrames > 0)
                                            turret.CurrentFrame = 1;
                                        else
                                            turret.CurrentFrame = 0;

                                        TurretShoot();
                                    }
                                }
                                break;
                            #endregion

                            #region Semi Auto
                            case TurretFireType.SemiAuto:
                                if (CurrentMouseState.LeftButton == ButtonState.Pressed &&
                                    PreviousMouseState.LeftButton == ButtonState.Released)
                                {
                                    #region If the turret selectbox currently contains the mouse cursor, don't allow that turret to shoot
                                    var fullTurrets = TurretList.Where(FullTurret => FullTurret != null).ToList();

                                    if (fullTurrets.Any(Turret => Turret.SelectBox.Contains(VectorToPoint(CursorPosition))))
                                    {
                                        break;
                                    }
                                    #endregion

                                    if (turret.CanShoot == true && this.IsActive == true)
                                    {
                                        turret.ElapsedTime = 0;
                                        turret.Animated = true;

                                        if (turret.CurrentAnimation.TotalFrames > 0)
                                            turret.CurrentFrame = 1;
                                        else
                                            turret.CurrentFrame = 0;

                                        TurretShoot();
                                    }
                                }
                                break;
                            #endregion

                            #region Beam
                            case TurretFireType.Beam:
                                if (CurrentMouseState.LeftButton == ButtonState.Pressed &&
                                    PreviousMouseState.LeftButton == ButtonState.Released)
                                {
                                    #region If the turret selectbox currently contains the mouse cursor, don't allow that turret to shoot
                                    var fullTurrets = TurretList.Where(FullTurret => FullTurret != null).ToList();

                                    if (fullTurrets.Any(Turret => Turret.SelectBox.Contains(VectorToPoint(CursorPosition))))
                                    {
                                        break;
                                    }
                                    #endregion

                                    if (turret.CanShoot == true && this.IsActive == true)
                                    {
                                        turret.ElapsedTime = 0;
                                        turret.Animated = true;

                                        if (turret.CurrentAnimation.TotalFrames > 0)
                                            turret.CurrentFrame = 1;
                                        else
                                            turret.CurrentFrame = 0;

                                        TurretShoot();
                                    }
                                }
                                break;
                            #endregion
                        }
                    }
                    #endregion

                    #region Check what turret is currently moused-over
                    if (turret.SelectBox.Contains(VectorToPoint(CursorPosition)) && UITurretOutlineList.Count == 0)
                    {
                        UIOutline turretOutline = new UIOutline(new Vector2(turret.SelectBox.X, turret.SelectBox.Y), new Vector2(turret.SelectBox.Width, turret.SelectBox.Height), null, turret);
                        turretOutline.OutlineTexture = TurretSelectBox;
                        UITurretOutlineList.Add(turretOutline);
                    }

                    if (!turret.SelectBox.Contains(VectorToPoint(CursorPosition)))
                    {
                        UITurretOutlineList.RemoveAll(turretOutline => turretOutline.Turret == turret);
                    }
                    #endregion

                    #region Select the turret when it's clicked
                    if (turret.JustClicked == true)
                    {
                        ClearSelected();
                        turret.Selected = true;
                        CurrentTurret = turret;

                        //Makes sure two turrets cannot be selected at the same time
                        foreach (Turret turret2 in TurretList)
                        {
                            if (turret2 != null && turret2 != turret)
                            {
                                turret2.Selected = false;
                            }
                        }
                    }
                    #endregion

                    #region Remove turret when middle clicked and refund resources
                    if (turret.Active == true &&
                        turret.SelectBox.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)) &&
                        CurrentMouseState.MiddleButton == ButtonState.Released &&
                        PreviousMouseState.MiddleButton == ButtonState.Pressed &&
                        turret.CurrentHealth == turret.MaxHealth)
                    {
                        Resources += turret.ResourceCost;
                        turret.Active = false;
                        TowerButtonList[TurretList.IndexOf(turret)].ButtonActive = true;
                        TurretList[TurretList.IndexOf(turret)] = null;

                        //Remove the outline from around the turret
                        UITurretOutlineList.RemoveAll(uiOutline => uiOutline.Turret == turret);
                        return;
                    }
                    #endregion
                }
            }

            //foreach (AmmoBelt belt in AmmoBeltList)
            //{
            //    belt.Nodes[0].CurrentPosition = TurretList[0].BarrelCenter + (TurretList[0].FireDirection * belt.ShellTexture.Width / 2);
            //    belt.Nodes2[0].CurrentPosition = TurretList[0].BarrelCenter - (TurretList[0].FireDirection * belt.ShellTexture.Width / 2);
            //    belt.Update(gameTime);
            //}
        }

        private void TurretShoot()
        {
            foreach (Turret turret in TurretList)
            {
                if (turret != null &&
                    turret.Active == true &&
                    turret.Selected == true)
                {
                    Vector2 MousePosition, Direction;
                    HeavyProjectile HeavyProjectile;

                    MousePosition = CursorPosition;

                    Direction = turret.FireDirection;
                    Direction.Normalize();

                    switch (turret.TurretType)
                    {
                        #region Machine Gun
                        case TurretType.MachineGun:
                            {
                                MachineShot1.Play();

                                CurrentProfile.ShotsFired++;

                                LightProjectileList.Add(new MachineGunProjectile(new Vector2(turret.BarrelCenter.X,
                                                                                             turret.BarrelCenter.Y), Direction));
                                Emitter FlashEmitter = new Emitter(ExplosionParticle,
                                    new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                    new Vector2(
                                    MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X))),
                                    new Vector2(15, 30), new Vector2(1, 3), 1f, true, new Vector2(0, 360),
                                    new Vector2(-1, 1), new Vector2(1, 3), ExplosionColor, ExplosionColor2, 0.0f, 0.05f, 0.5f, 1,
                                    false, new Vector2(0, 1080), true);
                                YSortedEmitterList.Add(FlashEmitter);

                                VerletShells.Add(new ShellCasing(new Vector2(turret.BarrelCenter.X, turret.BarrelCenter.Y), new Vector2(5, 5), ShellCasing));

                                //turret.AmmoBelt.Sticks2.RemoveAt(turret.AmmoBelt.Sticks2.Count - 1);

                                for (int i = 0; i < (int)turret.AmmoBelt.Nodes2.Count / 2; i++)
                                {
                                    turret.AmmoBelt.Nodes2[i].PreviousPosition.X += 8;
                                    turret.AmmoBelt.Nodes[i].PreviousPosition.X += 8;

                                    turret.AmmoBelt.Nodes2[i].PreviousPosition.Y -= 8;
                                    turret.AmmoBelt.Nodes[i].PreviousPosition.Y -= 8;
                                }

                                turret.CurrentHeat += turret.ShotHeat;
                            }
                            break;
                        #endregion

                        #region Cannon turret
                        case TurretType.Cannon:
                            {
                                double a, b;

                                a = turret.FireDirection.X;
                                b = turret.FireDirection.Y;

                                CannonFire.Play();

                                Emitter FlashEmitter = new Emitter(ExplosionParticle,
                                         new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                         new Vector2(
                                         MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)),
                                         MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X))),
                                         new Vector2(50, 70), new Vector2(1, 3), 0.15f, true, new Vector2(0, 360),
                                         new Vector2(-1, 1), new Vector2(2, 4), ExplosionColor, ExplosionColor2, 0.0f, 0.05f, 1f, 2,
                                         false, new Vector2(0, 1080), true, 1);
                                YSortedEmitterList.Add(FlashEmitter);

                                Emitter FlashEmitter2 = new Emitter(ExplosionParticle,
                                        new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                        new Vector2(
                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) + 80,
                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) + 90),
                                        new Vector2(10, 15), new Vector2(1, 3), 0.15f, true, new Vector2(0, 360),
                                        new Vector2(-1, 1), new Vector2(2, 4), ExplosionColor, ExplosionColor2, 0.0f, 0.05f, 1f, 2,
                                        false, new Vector2(0, 1080), true, 1);
                                YSortedEmitterList.Add(FlashEmitter2);

                                Emitter FlashEmitter3 = new Emitter(ExplosionParticle,
                                        new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                        new Vector2(
                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) - 90,
                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) - 80),
                                        new Vector2(10, 15), new Vector2(1, 3), 0.15f, true, new Vector2(0, 360),
                                        new Vector2(-1, 1), new Vector2(2, 4), ExplosionColor, ExplosionColor2, 0.0f, 0.05f, 1f, 2,
                                        false, new Vector2(0, 1080), true, 1);
                                YSortedEmitterList.Add(FlashEmitter3);

                                Emitter SmokeEmitter = new Emitter(SmokeParticle,
                                        new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                        new Vector2(
                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)),
                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X))), new Vector2(4, 8),
                                        new Vector2(30, 40), 0.2f, true, new Vector2(0, 360), new Vector2(0, 0),
                                        new Vector2(0.8f, 1.2f), SmokeColor1, SmokeColor2, 0f, 0.05f, 1, 1, false,
                                        new Vector2(0, 1080), true, null, null, null, null, null, null, null, new Vector2(0.05f, 0.05f), true, true);
                                YSortedEmitterList.Add(SmokeEmitter);

                                //HeavyProjectile = new CannonBall(new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 16, turret.FireRotation, 0.2f, turret.Damage, turret.BlastRadius,
                                //    new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));
                                //Calculate average Y position of all existing invaders here
                                //Use that value to determine the max Y value of the projectile so that accuracy is improved
                                //And it's less frustrating to fire heavy projectigles because they don't land where expected
                                if (InvaderList.Count > 0)
                                {
                                    float AvgY = 0;
                                    foreach (Invader invader in InvaderList)
                                    {
                                        AvgY += invader.MaxY;
                                    }

                                    AvgY /= InvaderList.Count;

                                    HeavyProjectile = new CannonBall(CannonBallSprite, SmokeParticle,
                                        new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), turret.LaunchVelocity,
                                        turret.FireRotation, 0.2f, turret.Damage, turret.BlastRadius,
                                        new Vector2(AvgY - 32, AvgY + 32));
                                }
                                else
                                {
                                    HeavyProjectile = new CannonBall(CannonBallSprite, SmokeParticle,
                                        new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), turret.LaunchVelocity,
                                        turret.FireRotation, 0.2f, turret.Damage, turret.BlastRadius,
                                        new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));
                                }

                                HeavyProjectile.Initialize();
                                HeavyProjectileList.Add(HeavyProjectile);

                                //Should probably figure out what this does
                                double d, v, g, y;
                                v = HeavyProjectile.Speed;
                                g = HeavyProjectile.Gravity;
                                y = HeavyProjectile.Position.Y;

                                d = (v * Math.Cos(HeavyProjectile.Angle) / g) * (v * Math.Sin(HeavyProjectile.Angle) + Math.Sqrt(Math.Pow(v * Math.Sin(HeavyProjectile.Angle), 2) + 2 * g * y));

                                if (d < 0)
                                {
                                    HeavyProjectile.MaxY = MathHelper.Clamp(HeavyProjectile.MaxY, Tower.DestinationRectangle.Bottom, HeavyProjectile.YRange.Y);
                                }

                                Vector2 BarrelStart = new Vector2((float)Math.Cos(turret.Rotation) * (45),
                                                                  (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height / 2));


                                //This needs to be replaced with code that adds a verlet-based shell
                                //Particle NewShellCasing = new Particle(ShellCasing,
                                //    new Vector2(turret.BarrelRectangle.X - BarrelStart.X, turret.BarrelRectangle.Y - BarrelStart.Y),
                                //    turret.Rotation - MathHelper.ToRadians((float)RandomDouble(175, 185)),
                                //    (float)RandomDouble(1, 3), 500, 1f, true, (float)RandomDouble(-10, 10),
                                //    (float)RandomDouble(-6, 6), 1f, Color.Orange, Color.Lerp(Color.White, Color.Transparent, 0.25f), 0.35f, true, Random.Next(Tower.DestinationRectangle.Bottom, Tower.DestinationRectangle.Bottom + 32),
                                //    false, null, true, true, true, false, 0, RandomOrientation(SpriteEffects.None, SpriteEffects.FlipVertically), 4000
                                //    );

                                //ShellCasingList.Add(NewShellCasing);
                            }
                            break;
                        #endregion

                        #region Flamethrower turret
                        case TurretType.FlameThrower:
                            {
                                HeavyProjectile = new FlameProjectile(FlameSprite, FireParticle2, new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                    (float)RandomDouble(7, 9), turret.Rotation, 0.3f, 5,
                                    new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));

                                foreach (Emitter emitter in HeavyProjectile.EmitterList)
                                {
                                    emitter.AngleRange = new Vector2(
                                   -(MathHelper.ToDegrees((float)Math.Atan2(-HeavyProjectile.Velocity.Y, -HeavyProjectile.Velocity.X))) - 20,
                                   -(MathHelper.ToDegrees((float)Math.Atan2(-HeavyProjectile.Velocity.Y, -HeavyProjectile.Velocity.X))) + 20);
                                }

                                HeavyProjectile.Initialize();
                                HeavyProjectileList.Add(HeavyProjectile);
                            }
                            break;
                        #endregion

                        #region Lightning turret
                        case TurretType.Lightning:
                            {
                                LightProjectileList.Add(new LightningProjectile(new Vector2(turret.BarrelCenter.X, turret.BarrelCenter.Y), Direction));

                                Vector2 BarrelStart = new Vector2((float)Math.Cos(turret.Rotation) * (45),
                                                                  (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height / 2));

                                Particle NewShellCasing = new Particle(LightningShellCasing,
                                    new Vector2(turret.BarrelRectangle.X - BarrelStart.X, turret.BarrelRectangle.Y - BarrelStart.Y),
                                    turret.Rotation - MathHelper.ToRadians((float)RandomDouble(175, 185)),
                                    (float)RandomDouble(3, 5), 500, 1f, true, (float)RandomDouble(turret.Rotation, turret.Rotation),
                                    (float)RandomDouble(-6, 6), 1f, Color.White, Color.Lerp(Color.White, Color.Transparent, 0.25f), 0.35f, true, Random.Next(Tower.DestinationRectangle.Bottom, Tower.DestinationRectangle.Bottom + 32),
                                    false, null, true, true, true, false, new Vector2(0,0), SpriteEffects.None, 4000
                                    );

                                ShellCasingList.Add(NewShellCasing);

                                LightningSound.Play();
                            }
                            break;
                        #endregion

                        #region Cluster turret
                        case TurretType.Cluster:
                            {
                                TimerHeavyProjectile heavyProjectile;

                                Emitter FlashEmitter = new Emitter(ExplosionParticle,
                                         new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                         new Vector2(
                                         MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)),
                                         MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X))),
                                         new Vector2(50, 70), new Vector2(1, 3), 0.15f, true, new Vector2(0, 360),
                                         new Vector2(-1, 1), new Vector2(2, 4), ExplosionColor, ExplosionColor2, 0.0f, 0.05f, 1f, 2,
                                         false, new Vector2(0, 1080), true, 1);
                                YSortedEmitterList.Add(FlashEmitter);

                                Emitter FlashEmitter2 = new Emitter(ExplosionParticle,
                                        new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                        new Vector2(
                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) + 80,
                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) + 90),
                                        new Vector2(10, 15), new Vector2(1, 3), 0.15f, true, new Vector2(0, 360),
                                        new Vector2(-1, 1), new Vector2(2, 4), ExplosionColor, ExplosionColor2, 0.0f, 0.05f, 1f, 2,
                                        false, new Vector2(0, 1080), true, 1);
                                YSortedEmitterList.Add(FlashEmitter2);

                                Emitter FlashEmitter3 = new Emitter(ExplosionParticle,
                                        new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                        new Vector2(
                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) - 90,
                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) - 80),
                                        new Vector2(10, 15), new Vector2(1, 3), 0.15f, true, new Vector2(0, 360),
                                        new Vector2(-1, 1), new Vector2(2, 4), ExplosionColor, ExplosionColor2, 0.0f, 0.05f, 1f, 2,
                                        false, new Vector2(0, 1080), true, 1);
                                YSortedEmitterList.Add(FlashEmitter3);

                                Emitter SmokeEmitter = new Emitter(SmokeParticle,
                                        new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                        new Vector2(
                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)),
                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X))), new Vector2(4, 8),
                                        new Vector2(30, 40), 0.2f, true, new Vector2(0, 360), new Vector2(0, 0),
                                        new Vector2(0.8f, 1.2f), SmokeColor1, SmokeColor2, 0f, 0.05f, 1, 1, false,
                                        new Vector2(0, 1080), true, null, null, null, null, null, null, null, new Vector2(0.08f, 0.08f), true, true);
                                YSortedEmitterList.Add(SmokeEmitter);


                                heavyProjectile = new ClusterBombShell(1000, ClusterBombSprite, SmokeParticle,
                                                    new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 12, turret.Rotation, 0.2f, 5,
                                                    new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));
                                heavyProjectile.Initialize();
                                TimedProjectileList.Add(heavyProjectile);

                                Particle NewShellCasing = new Particle(ShellCasing,
                                    new Vector2(turret.BarrelRectangle.X, turret.BarrelRectangle.Y),
                                    turret.Rotation - MathHelper.ToRadians((float)RandomDouble(175, 185)),
                                    (float)RandomDouble(3, 6), 500, 1f, true, (float)RandomDouble(-10, 10),
                                    (float)RandomDouble(-6, 6), 1.5f, Color.White, Color.White, 0.2f, true,
                                    Random.Next(Tower.DestinationRectangle.Bottom, Tower.DestinationRectangle.Bottom + 32),
                                    false, null, true);
                                ShellCasingList.Add(NewShellCasing);
                            }
                            break;
                        #endregion

                        #region Fel Cannon
                        case TurretType.FelCannon:
                            {
                                HeavyProjectile = new FelProjectile(BlankTexture, RoundSparkParticle, SmokeParticle,
                                        new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 5, turret.Rotation, 0.01f, turret.Damage, 100,
                                        new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));

                                HeavyProjectile.Initialize();
                                HeavyProjectileList.Add(HeavyProjectile);
                            }
                            break;
                        #endregion

                        #region Beam turret
                        case TurretType.Beam:
                            {
                                LightProjectileList.Add(new BeamProjectile(new Vector2(turret.BarrelCenter.X, turret.BarrelCenter.Y), Direction));
                            }
                            break;
                        #endregion

                        #region Freeze turret
                        case TurretType.Freeze:
                            {
                                LightProjectileList.Add(new FreezeProjectile(new Vector2(turret.BarrelCenter.X, turret.BarrelCenter.Y), Direction));
                            }
                            break;
                        #endregion

                        #region Boomerang turret
                        case TurretType.Boomerang:
                            {
                                HeavyProjectile = new BoomerangProjectile(BoomerangSprite, SmokeParticle, new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 12, turret.Rotation, 0.2f, turret.Damage, 100,
                                    new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));

                                HeavyProjectile.Initialize();
                                HeavyProjectileList.Add(HeavyProjectile);
                                //DrawableList.Add(HeavyProjectile);
                            }
                            break;
                        #endregion

                        #region Grenade launcher turret
                        case TurretType.Grenade:
                            {
                                TimerHeavyProjectile heavyProjectile;

                                heavyProjectile = new GrenadeProjectile(2500, GrenadeSprite, SmokeParticle,
                                    new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 16, turret.Rotation, 0.3f, 5,
                                    new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));

                                heavyProjectile.Initialize();
                                TimedProjectileList.Add(heavyProjectile);
                                //DrawableList.Add(heavyProjectile);

                                turret.CurrentHeat += turret.ShotHeat;
                            }
                            break;
                        #endregion

                        #region Gas Grenade Launcher Turret
                        case TurretType.GasGrenade:
                            {
                                TimerHeavyProjectile heavyProjectile;

                                heavyProjectile = new GasGrenadeProjectile(2500, GrenadeSprite, SmokeParticle,
                                    new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 16, turret.Rotation, 0.3f, 5,
                                    new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));

                                heavyProjectile.Initialize();
                                TimedProjectileList.Add(heavyProjectile);
                                //DrawableList.Add(heavyProjectile);

                                turret.CurrentHeat += turret.ShotHeat;
                            }
                            break;
                        #endregion

                        #region Shotgun turret
                        case TurretType.Shotgun:
                            {
                                //MachineShot1.Play();

                                CurrentProfile.ShotsFired++;

                                for (int i = 0; i < 5; i++)
                                {
                                    LightProjectileList.Add(new ShotgunProjectile(new Vector2(turret.BarrelCenter.X,
                                                                                              turret.BarrelCenter.Y), Direction));

                                    turret.ChangeFireDirection();
                                    Direction = turret.FireDirection;
                                    Direction.Normalize();
                                }

                                Emitter FlashEmitter = new Emitter(ExplosionParticle,
                                    new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                    new Vector2(
                                    MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X))),
                                    new Vector2(15, 30), new Vector2(1, 3), 1f, true, new Vector2(0, 360),
                                    new Vector2(-1, 1), new Vector2(1, 3), ExplosionColor, ExplosionColor2, 0.0f, 0.05f, 0.5f, 1,
                                    false, new Vector2(0, 1080), true);
                                YSortedEmitterList.Add(FlashEmitter);

                                Particle NewShellCasing = new Particle(ShellCasing,
                                    new Vector2(turret.BarrelCenter.X, turret.BarrelCenter.Y),
                                    turret.Rotation - MathHelper.ToRadians((float)RandomDouble(175, 185)),
                                    (float)RandomDouble(4, 6), 500, 1f, true, (float)RandomDouble(-10, 10),
                                    (float)RandomDouble(-6, 6), 0.7f, Color.Orange, Color.Lerp(Color.White, Color.Transparent, 0.25f), 0.2f, true, Random.Next(600, 630),
                                    false, null, true);
                                ShellCasingList.Add(NewShellCasing);
                                //DrawableList.Add(NewShellCasing);                                

                                turret.CurrentHeat += turret.ShotHeat;
                            }
                            break;
                        #endregion

                        #region Persistent beam turret
                        case TurretType.PersistentBeam:
                            {
                                CurrentProfile.ShotsFired++;

                                if (!LightProjectileList.Any(LightProjectile => LightProjectile.LightProjectileType == LightProjectileType.PersistentBeam))
                                    LightProjectileList.Add(new PersistentBeamProjectile(new Vector2(turret.BarrelCenter.X,
                                                                                                    turret.BarrelCenter.Y), Direction));

                                turret.CurrentHeat += turret.ShotHeat;
                            }
                            break;
                        #endregion
                    }
                }
            }
        }

        private void HeavyProjectileUpdate(GameTime gameTime)
        {
            #region Handle Arrows
            int ArrowCount = HeavyProjectileList.Count(HeavyProjectile => HeavyProjectile.HeavyProjectileType == HeavyProjectileType.Arrow);

            if (ArrowCount > 2)
            {
                HeavyProjectileList[HeavyProjectileList.IndexOf(HeavyProjectileList.First(
                    HeavyProjectile => HeavyProjectile.HeavyProjectileType == HeavyProjectileType.Arrow))].Fade = true;

                HeavyProjectileList[HeavyProjectileList.IndexOf(HeavyProjectileList.First(
                    HeavyProjectile => HeavyProjectile.HeavyProjectileType == HeavyProjectileType.Arrow))].Active = false;
            }
            #endregion

            foreach (HeavyProjectile heavyProjectile in HeavyProjectileList)
            {
                heavyProjectile.Update(gameTime);

                switch (heavyProjectile.HeavyProjectileType)
                {
                    #region Default
                    //default:
                    //    //This makes sure that the trails from the projectiles appear correctly oriented while falling
                    //    foreach (Emitter emitter in heavyProjectile.EmitterList)
                    //    {
                    //        emitter.AngleRange = new Vector2(
                    //            -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) - 20,
                    //            -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) + 20);
                    //    }
                    //    break;
                    #endregion

                    #region Boomerang
                    case HeavyProjectileType.Boomerang:
                        {
                            //heavyProjectile.Node1.Velocity.X -= 2f;
                        }
                        break;
                    #endregion

                    #region Fel projectile
                    case HeavyProjectileType.FelProjectile:
                        for (int i = 0; i < 5; i++)
                        {
                            Bolt = new LightningBolt(heavyProjectile.Position, new Vector2(heavyProjectile.Position.X + (float)RandomDouble(-30, 30f), heavyProjectile.Position.Y + (float)RandomDouble(-30f, 30f)), Color.LimeGreen, 0.25f);
                            LightningList.Add(Bolt);
                        }
                        break;
                    #endregion
                }

                #region This makes sure that particles are emitted from the wall in the correct position when a projectile hits it
                if (TrapList.Any(Trap => Trap.DestinationRectangle.Intersects(heavyProjectile.CollisionRectangle) &&
                    Trap.TrapType == TrapType.Wall))
                {
                    int index = TrapList.IndexOf(TrapList.First(Trap => Trap.DestinationRectangle.Intersects(heavyProjectile.CollisionRectangle)));
                    if (heavyProjectile.Active == true)
                    {
                        switch (heavyProjectile.HeavyProjectileType)
                        {
                            #region Cannonball
                            case HeavyProjectileType.CannonBall:
                                Emitter newEmitter = new Emitter(SplodgeParticle, new Vector2(heavyProjectile.DestinationRectangle.Center.X, heavyProjectile.DestinationRectangle.Center.Y),
                                                     new Vector2(
                                                         -(float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X) - 180 - 45,
                                                         (float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X) - 180 + 45),
                                                     new Vector2(2, 3), new Vector2(100, 200), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                                     new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true,
                                                     new Vector2(TrapList[index].DestinationRectangle.Bottom, TrapList[index].DestinationRectangle.Bottom), false, null, true, true);

                                EmitterList2.Add(newEmitter);

                                //for (int i = 0; i < 5; i++)
                                //{
                                //    Emitter SparkEmitter2 = new Emitter(RoundSparkParticle, new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                                //    new Vector2(0, 360), new Vector2(0.5f, 1.5f), new Vector2(30, 60), 0.8f, true, new Vector2(0, 360), new Vector2(1, 3),
                                //    new Vector2(0.1f, 0.3f), Color.Orange, Color.OrangeRed, 0.05f, 0.5f, 1, 1, true, new Vector2(TrapList[index].DestinationRectangle.Bottom, TrapList[index].DestinationRectangle.Bottom + 8),
                                //    true, null, true, true, new Vector2(3, 6), new Vector2(
                                //                         -(float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X) - 180 - (float)RandomDouble(0, 45),
                                //                         -(float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X) - 180 + (float)RandomDouble(0,45)), 0.05f, true);

                                //    AlphaEmitterList.Add(SparkEmitter2);
                                //}
                                break;
                            #endregion
                        }

                        heavyProjectile.Active = false;

                        foreach (Emitter emitter in heavyProjectile.EmitterList)
                        {
                            emitter.AddMore = false;
                        }
                    }
                }
                #endregion

                #region This controls what happens when a heavy projectile collides with the ground
                if (heavyProjectile.Position.Y >= heavyProjectile.MaxY && heavyProjectile.Active == true)
                {
                    switch (heavyProjectile.HeavyProjectileType)
                    {
                        #region CannonBall projectile
                        case HeavyProjectileType.CannonBall:
                            {
                                //heavyProjectile.Node1.CurrentPosition.Y += 10;
                                //heavyProjectile.Node2.CurrentPosition.Y += 10;
                                #region Regular ground
                                Emitter ExplosionEmitter = new Emitter(ExplosionParticle,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(20, 160), new Vector2(6, 14), new Vector2(5, 7), 1f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(3, 4), Color.Lerp(ExplosionColor, Color.Transparent, 0.5f),
                                        Color.Lerp(ExplosionColor2, Color.Transparent, 0.5f), 0.0f, 0.1f, 1, 1, false,
                                        new Vector2(heavyProjectile.MaxY, heavyProjectile.MaxY + 8), true, heavyProjectile.MaxY / 1080);
                                YSortedEmitterList.Add(ExplosionEmitter);

                                Emitter ExplosionEmitter2 = new Emitter(ExplosionParticle,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(70, 110), new Vector2(6, 12), new Vector2(7, 18), 1f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(3, 4), Color.Lerp(ExplosionColor, Color.Transparent, 0.5f),
                                        Color.Lerp(ExplosionColor2, Color.DarkRed, 0.5f), 0.0f, 0.15f, 10, 1, false,
                                        new Vector2(heavyProjectile.MaxY, heavyProjectile.MaxY + 8), true, heavyProjectile.MaxY / 1080);
                                YSortedEmitterList.Add(ExplosionEmitter2);

                                Emitter SmokeEmitter = new Emitter(SmokeParticle,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY - 30),
                                        new Vector2(0, 0), new Vector2(0f, 1f), new Vector2(30, 40), 0.8f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(0.8f, 1.2f), SmokeColor1, SmokeColor2, -0.1f, 0.4f, 20, 1, false,
                                        new Vector2(heavyProjectile.MaxY, heavyProjectile.MaxY + 8), false, heavyProjectile.MaxY / 1080,
                                        null, null, null, null, null, null, null, true, true);
                                YSortedEmitterList.Add(SmokeEmitter);

                                Emitter ExplosionEmitter3 = new Emitter(ExplosionParticle2,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(85, 95), new Vector2(2, 4), new Vector2(25, 40), 0.35f, true, new Vector2(0, 0),
                                        new Vector2(0, 0), new Vector2(0.085f, 0.2f), FireColor,
                                        Color.Lerp(Color.Red, FireColor2, 0.5f), -0.1f, 0.05f, 10, 1, false,
                                        new Vector2(heavyProjectile.MaxY, heavyProjectile.MaxY + 8), true, heavyProjectile.MaxY / 1080,
                                        null, null, null, null, null, null, new Vector2(0.0025f, 0.0025f), true, true, 50);
                                YSortedEmitterList.Add(ExplosionEmitter3);

                                Emitter DebrisEmitter = new Emitter(SplodgeParticle,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(70, 110), new Vector2(5, 7), new Vector2(30, 110), 1f, true, new Vector2(0, 360),
                                        new Vector2(1, 3), new Vector2(0.007f, 0.05f), Color.DarkSlateGray, Color.SaddleBrown,
                                        0.2f, 0.2f, 5, 1, true, new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 16), null, heavyProjectile.MaxY / 1080);
                                YSortedEmitterList.Add(DebrisEmitter);

                                Emitter DebrisEmitter2 = new Emitter(SplodgeParticle,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY), new Vector2(80, 100),
                                        new Vector2(2, 8), new Vector2(80, 150), 1f, true, new Vector2(0, 360), new Vector2(1, 3),
                                        new Vector2(0.01f, 0.02f), Color.Gray, Color.SaddleBrown, 0.2f, 0.3f, 10, 5, true,
                                        new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 16), null, heavyProjectile.MaxY / 1080);
                                YSortedEmitterList.Add(DebrisEmitter2);

                                Emitter SparkEmitter = new Emitter(RoundSparkParticle,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY - 20), new Vector2(0, 360),
                                        new Vector2(1, 4), new Vector2(120, 140), 1f, true, new Vector2(0, 360), new Vector2(1, 3),
                                        new Vector2(0.1f, 0.3f), Color.Orange, Color.OrangeRed, 0.05f, 0.1f, 2, 7, true,
                                        new Vector2(heavyProjectile.MaxY + 4, heavyProjectile.MaxY + 16), null, heavyProjectile.MaxY / 1080);
                                AlphaEmitterList.Add(SparkEmitter);

                                //DrawableList.Add(SparkEmitter);

                                //PERFECT SPARK EMITTER FOR HITTING WALL
                                //EMITTER HAS NICE TRAJECTORY ETC.
                                //for (int i = 0; i < 15; i++)
                                //{
                                //    Emitter SparkEmitter2 = new Emitter(RoundSparkParticle, new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY - 20),
                                //    new Vector2(0, 360), new Vector2(0.5f, 1.5f), new Vector2(30, 60), 100f, true, new Vector2(0, 360), new Vector2(1, 3),
                                //    new Vector2(0.1f, 0.3f), Color.Orange, Color.OrangeRed, 0.05f, 0.5f, 1, 10, true, new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 8),
                                //    false, null, false, true, new Vector2(8, 10), new Vector2(50, 130), 0.6f, true);

                                //    AlphaEmitterList.Add(SparkEmitter2);
                                //}

                                //float YDist = heavyProjectile.YRange.Y - heavyProjectile.MaxY;
                                //float YDist2 = heavyProjectile.YRange.Y - heavyProjectile.YRange.X;

                                //float PercentY = (100 / YDist2) * YDist;
                                //float thing = (100 - PercentY) / 100;

                                //thing = MathHelper.Clamp(thing, 0.5f, 1f);

                                #endregion

                                Decal NewDecal = new Decal(ExplosionDecal1, new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                                                          (float)RandomDouble(0, 0), heavyProjectile.YRange, heavyProjectile.MaxY, 1f);

                                DecalList.Add(NewDecal);

                                ShockWaveEffect.Parameters["CenterCoords"].SetValue(new Vector2(1 / (1920 / heavyProjectile.Position.X), 1 / (1080 / heavyProjectile.Position.Y)));
                                ShockWaveEffect.Parameters["WaveParams"].SetValue(new Vector4(1, 0.5f, 0.06f, 60));
                                ShockWaveEffect.Parameters["CurrentTime"].SetValue(0);

                                Explosion newExplosion = new Explosion(heavyProjectile.Position, heavyProjectile.BlastRadius, heavyProjectile.Damage);
                                ExplosionList.Add(newExplosion);

                                CannonExplosion.Play();
                            }
                            break;
                        #endregion

                        #region ClusterBomb (Child of the ClusterBombShell) projectile
                        case HeavyProjectileType.ClusterBomb:
                            {
                                Emitter ExplosionEmitter2 = new Emitter(SplodgeParticle, new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                new Vector2(0, 180), new Vector2(1, 4), new Vector2(10, 30), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                new Vector2(0.02f, 0.06f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 0.2f, 20, 10, true, new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 8));

                                YSortedEmitterList.Add(ExplosionEmitter2);

                                Emitter ExplosionEmitter = new Emitter(ExplosionParticle,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(0, 180), new Vector2(1, 5), new Vector2(1, 20), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 7, false,
                                        new Vector2(0, 1080), true);

                                YSortedEmitterList.Add(ExplosionEmitter);

                                Color SmokeColor1 = Color.Lerp(Color.DarkGray, Color.Transparent, 0.5f);
                                Color SmokeColor2 = Color.Lerp(Color.Gray, Color.Transparent, 0.5f);

                                Emitter newEmitter2 = new Emitter(SmokeParticle,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(80, 100), new Vector2(0.5f, 1f), new Vector2(20, 40), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(1, 2), SmokeColor1, SmokeColor2, 0.0f, 0.3f, 1, 1, false,
                                        new Vector2(0, 1080), false);

                                EmitterList2.Add(newEmitter2);


                                Decal NewDecal = new Decal(ExplosionDecal1, new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                                                         (float)RandomDouble(0, 0), heavyProjectile.YRange, heavyProjectile.MaxY, 0.3f);

                                DecalList.Add(NewDecal);

                                Explosion newExplosion = new Explosion(heavyProjectile.Position, 300, heavyProjectile.Damage);
                                ExplosionList.Add(newExplosion);
                            }
                            break;
                        #endregion

                        #region Flame projectiles from FlameThrower
                        case HeavyProjectileType.FlameThrower:
                            {
                                //Emitter newEmitter = new Emitter(SmokeParticle, new Vector2(heavyProjectile.Position.X,
                                //    heavyProjectile.DestinationRectangle.Bottom), new Vector2(20, 160), new Vector2(1f, 1.6f), new Vector2(10, 12), 0.25f, false,
                                //    new Vector2(-20, 20), new Vector2(-4, 4), new Vector2(0.6f, 0.8f), FireColor, FireColor2, -0.2f, 0.5f, 3, 1, false, new Vector2(0, 1080));
                                //YSortedEmitterList.Add(newEmitter);
                                ////DrawableList.Add(newEmitter);

                                Emitter FireEmitter = new Emitter(FireParticle,
                                    new Vector2(heavyProjectile.Position.X, heavyProjectile.DestinationRectangle.Bottom), new Vector2(60, 120),
                                    new Vector2(0.5f, 0.75f), new Vector2(90, 140), 0.65f, true,
                                    new Vector2(-4, 4), new Vector2(-1f, 1f), new Vector2(0.075f, 0.15f), FireColor, FireColor2,
                                    0.0f, 0.5f, 75, 1, false, new Vector2(0, 1080), true, heavyProjectile.DestinationRectangle.Bottom / 1080f, null, null, null, null, null, null, null, true, true, 150);
                                FireEmitter.TextureName = "FireParticle";
                                YSortedEmitterList.Add(FireEmitter);

                                Emitter SmokeEmitter = new Emitter(SmokeParticle,
                                    new Vector2(heavyProjectile.Position.X, heavyProjectile.DestinationRectangle.Bottom - 4),
                                    new Vector2(85, 95), new Vector2(0.2f, 0.5f), new Vector2(250, 350), 0.25f, true, new Vector2(-20, 20),
                                    new Vector2(-2, 2), new Vector2(0.6f, 1f), Color.DimGray, Color.DarkGray, 0.0f, 0.5f, 150, 1, false,
                                    new Vector2(0, 1080), true, (heavyProjectile.DestinationRectangle.Bottom - 1) / 1080f, null, null, null, null, null, null, null, true, true, 250);
                                SmokeEmitter.TextureName = "SmokeParticle";
                                YSortedEmitterList.Add(SmokeEmitter);

                                //Emitter SparkEmitter = new Emitter(RoundSparkParticle,
                                //       new Vector2(NewTrap.Position.X + NewTrap.TextureList[0].Width / 2, NewTrap.Position.Y + NewTrap.TextureList[0].Height - 16),
                                //       new Vector2(80, 100),
                                //       new Vector2(1, 4), new Vector2(60, 180), 1f, true, new Vector2(0, 360), new Vector2(1, 3),
                                //       new Vector2(0.1f, 0.3f), Color.LightYellow, Color.White, -0.001f, -1f, 100, 1, false,
                                //       new Vector2(0, 1080), null, (NewTrap.DestinationRectangle.Bottom - 2) / 1080f);
                                //SparkEmitter.TextureName = "SparkParticle";

                                foreach (Emitter emitter in heavyProjectile.EmitterList)
                                {
                                    emitter.AddMore = false;
                                }
                                heavyProjectile.Active = false;
                            }
                            break;

                        case HeavyProjectileType.Arrow:
                            {
                                foreach (Emitter emitter in heavyProjectile.EmitterList)
                                {
                                    emitter.AddMore = false;
                                }
                                heavyProjectile.Rotate = false;
                                heavyProjectile.Velocity = Vector2.Zero;
                            }
                            break;
                        #endregion

                        #region Acid projectiles
                        case HeavyProjectileType.Acid:
                            {

                            }
                            break;
                        #endregion

                        #region FelProjectile
                        case HeavyProjectileType.FelProjectile:
                            {
                                Color LightningColor;

                                Implosion.Play();
                                Emitter NewEmitter = new Emitter(FireParticle, heavyProjectile.Position,
                            new Vector2(0, 360), new Vector2(0.5f, 0.75f), new Vector2(20, 40), 0.01f, true, new Vector2(-20, 20),
                            new Vector2(-4, 4), new Vector2(1, 2f), Color.LimeGreen, Color.YellowGreen, 0.0f, 0.8f, 10, 1, false, new Vector2(0, 1080),
                            false, 0, false, false);
                                AlphaEmitterList.Add(NewEmitter);

                                foreach (Invader invader in InvaderList)
                                {
                                    //bool thing = RandomBool();
                                    //if (thing == true)
                                    //    LightningColor = Color.Purple;
                                    //else
                                    LightningColor = Color.LimeGreen;

                                    //invader.Position = Vector2.Lerp(invader.Position, heavyProjectile.Position, 0.5f);
                                    //invader.MaxY = MathHelper.Lerp(invader.Position.Y, heavyProjectile.Position.Y, 0.25f);
                                    //ExplosionList.Add(new Explosion(heavyProjectile.Position, 400, 500));
                                    if (Vector2.Distance(invader.Position, heavyProjectile.Position) < 300)
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            Bolt = new LightningBolt(heavyProjectile.Position, new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y), LightningColor, 0.018f);
                                            LightningList.Add(Bolt);
                                        }

                                        invader.DamageOverTime(new DamageOverTimeStruct() { Milliseconds = 800, Damage = 1, Interval = 0.1f }, LightningColor);
                                        invader.Freeze(new FreezeStruct() { Milliseconds = 1000 }, LightningColor);
                                    }
                                }
                            }
                            break;
                        #endregion
                    }

                    #region Deactivate the Projectile
                    if (heavyProjectile.HeavyProjectileType != HeavyProjectileType.Arrow)
                    {
                        foreach (Emitter emitter in heavyProjectile.EmitterList)
                        {
                            emitter.AddMore = false;
                        }

                        heavyProjectile.Active = false;
                        heavyProjectile.Velocity = Vector2.Zero;
                    }
                    #endregion
                }
                #endregion

                #region HeavyPojectile Intersects an invader
                if (InvaderList.Any(Invader => Invader.DestinationRectangle.Intersects(heavyProjectile.DestinationRectangle)) && heavyProjectile.Active == true)
                {
                    int Index = InvaderList.IndexOf(InvaderList.First(Invader => Invader.DestinationRectangle.Intersects(heavyProjectile.DestinationRectangle)));

                    #region Small function to deactivate projectiles
                    Action DeactivateProjectile = new Action(() =>
                    {
                        if (heavyProjectile.Velocity != Vector2.Zero)
                        {
                            heavyProjectile.Velocity = Vector2.Zero;
                            heavyProjectile.Active = false;

                            foreach (Emitter emitter in heavyProjectile.EmitterList)
                            {
                                emitter.AddMore = false;
                            }
                        }
                    }
                    );
                    #endregion

                    switch (heavyProjectile.HeavyProjectileType)
                    {
                        #region CannonBall projectile
                        case HeavyProjectileType.CannonBall:
                            switch (InvaderList[Index].InvaderType)
                            {
                                #region Airship
                                case InvaderType.Airship:
                                    InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                                    DeactivateProjectile.Invoke();
                                    break;
                                #endregion

                                #region Tank
                                case InvaderType.Tank:
                                    InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                                    DeactivateProjectile.Invoke();

                                    Emitter newEmitter = new Emitter(SplodgeParticle, new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                                    new Vector2(-(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) - 45,
                                    -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) + 90),
                                    new Vector2(2, 3), new Vector2(100, 200), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                    new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true,
                                    new Vector2(InvaderList[Index].MaxY, InvaderList[Index].MaxY), false, 1f);

                                    YSortedEmitterList.Add(newEmitter);
                                    break;
                                #endregion
                            }
                            break;
                        #endregion

                        #region Flame projectile from FlameThrower
                        case HeavyProjectileType.FlameThrower:
                            switch (InvaderList[Index].InvaderType)
                            {
                                #region Airship
                                case InvaderType.Airship:
                                    if (InvaderList[Index].Burning == false)
                                        InvaderList[Index].DamageOverTime(new DamageOverTimeStruct() { Milliseconds = 1000, Damage = 1, Interval = 100 }, Color.Red);

                                    DeactivateProjectile.Invoke();
                                    break;
                                #endregion

                                #region Soldier
                                case InvaderType.Soldier:
                                    if (InvaderList[Index].Burning == false)
                                    {
                                        InvaderList[Index].FireEmitter = new Emitter(FireParticle2,
                                            InvaderList[Index].Position, new Vector2(60, 120),
                                            new Vector2(0.5f, 0.75f), new Vector2(70, 110), 0.85f, true,
                                            new Vector2(-4, 4), new Vector2(-1f, 1f), new Vector2(0.07f, 0.15f), FireColor2, FireColor,
                                            0.0f, -1, 10, 1, false, new Vector2(0, 1080), true, CursorPosition.Y / 1080,
                                            null, null, null, null, null, null, null, true, true, 150);

                                        InvaderList[Index].DamageOverTime(new DamageOverTimeStruct() { Milliseconds = 1000, Damage = 1, Interval = 100 }, Color.Red);
                                    }

                                    DeactivateProjectile.Invoke();
                                    break;
                                #endregion

                                #region Spider
                                case InvaderType.Spider:
                                    if (InvaderList[Index].Burning == false)
                                        InvaderList[Index].DamageOverTime(new DamageOverTimeStruct() { Milliseconds = 1000, Damage = 1, Interval = 100 }, Color.Red);

                                    DeactivateProjectile.Invoke();
                                    break;
                                #endregion

                                #region Slime
                                case InvaderType.Slime:
                                    if (InvaderList[Index].Burning == false)
                                        InvaderList[Index].DamageOverTime(new DamageOverTimeStruct() { Milliseconds = 1000, Damage = 1, Interval = 100 }, Color.Red);

                                    DeactivateProjectile.Invoke();
                                    break;
                                #endregion

                                #region Tank
                                case InvaderType.Tank:
                                    if (InvaderList[Index].Burning == false)
                                        InvaderList[Index].DamageOverTime(new DamageOverTimeStruct() { Milliseconds = 1000, Damage = 1, Interval = 100 }, Color.Red);

                                    DeactivateProjectile.Invoke();
                                    break;
                                #endregion
                            }
                            break;
                        #endregion

                        #region Arrow projectile
                        case HeavyProjectileType.Arrow:
                            if (heavyProjectile.Velocity != Vector2.Zero)
                            {
                                switch (InvaderList[Index].InvaderType)
                                {
                                    #region Airship
                                    case InvaderType.Airship:
                                        InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                                        DeactivateProjectile.Invoke();
                                        break;
                                    #endregion

                                    #region Soldier
                                    case InvaderType.Soldier:
                                        InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                                        DeactivateProjectile.Invoke();
                                        break;
                                    #endregion

                                    #region Slime
                                    case InvaderType.Slime:
                                        InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                                        DeactivateProjectile.Invoke();
                                        break;
                                    #endregion

                                    #region Spider
                                    case InvaderType.Spider:
                                        InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                                        DeactivateProjectile.Invoke();
                                        break;
                                    #endregion

                                    #region Tank
                                    case InvaderType.Tank:
                                        InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                                        DeactivateProjectile.Invoke();
                                        break;
                                    #endregion
                                }
                            }
                            break;
                        #endregion

                        #region ClusterBomb projectile
                        case HeavyProjectileType.ClusterBomb:
                            //if (heavyProjectile.Velocity != Vector2.Zero)
                            //{
                            //    switch (InvaderList[Index].InvaderType)
                            //    {

                            //        case InvaderType.Airship:
                            //            InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                            //            DeactivateProjectile.Invoke();
                            //            break;

                            //        case InvaderType.Soldier:
                            //            InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                            //            DeactivateProjectile.Invoke();
                            //            break;

                            //        case InvaderType.Slime:
                            //            InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                            //            DeactivateProjectile.Invoke();
                            //            break;

                            //        case InvaderType.Spider:
                            //            InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                            //            DeactivateProjectile.Invoke();
                            //            break;

                            //        case InvaderType.Tank:
                            //            InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                            //            DeactivateProjectile.Invoke();

                            //            Emitter newEmitter = new Emitter(SplodgeParticle, new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                            //            new Vector2(-(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) - 45,
                            //                -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) + 90),
                            //                new Vector2(2, 3), new Vector2(100, 200), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                            //            new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true,
                            //            new Vector2(InvaderList[Index].MaxY, InvaderList[Index].MaxY));

                            //            EmitterList2.Add(newEmitter);

                            //            EmitterList2[EmitterList2.IndexOf(newEmitter)].LoadContent(Content);
                            //            break;
                            //    }
                            //}
                            break;
                        #endregion
                    }
                }
                #endregion
            }
        }

        private void TimedProjectileUpdate(GameTime gameTime)
        {
            foreach (TimerHeavyProjectile timedProjectile in TimedProjectileList)
            {
                #region Deactivate the Projectile
                Action DeactivateProjectile = new Action(() =>
                {
                    foreach (Emitter emitter in timedProjectile.EmitterList)
                    {
                        emitter.AddMore = false;
                    }

                    timedProjectile.Active = false;
                    timedProjectile.Velocity = Vector2.Zero;
                });
                #endregion

                timedProjectile.Update(gameTime);

                switch (timedProjectile.HeavyProjectileType)
                {
                    case HeavyProjectileType.Grenade:
                        if (timedProjectile.Node1.Velocity == new Vector2(0, 0) &&
                            timedProjectile.Node2.Velocity == new Vector2(0, 0) &&
                            timedProjectile.Node1.CurrentPosition.Y >= timedProjectile.MaxY &&
                             timedProjectile.Node2.CurrentPosition.Y >= timedProjectile.MaxY)
                        {
                            timedProjectile.EmitterList[0].AngleRange = new Vector2(0, 180);
                            timedProjectile.EmitterList[0].SpeedRange = new Vector2(0.25f, 0.5f);
                            //timedProjectile.EmitterList[0].Gravity = -0.05f;

                            if (timedProjectile.CurrentTime >= timedProjectile.MaxTime - 500)
                                timedProjectile.EmitterList[0].AddMore = false;
                        }
                        break;

                    case HeavyProjectileType.GasGrenade:
                        if (timedProjectile.Node1.Velocity == new Vector2(0, 0) &&
                            timedProjectile.Node2.Velocity == new Vector2(0, 0) &&
                            timedProjectile.Node1.CurrentPosition.Y >= timedProjectile.MaxY &&
                             timedProjectile.Node2.CurrentPosition.Y >= timedProjectile.MaxY)
                        {
                            timedProjectile.EmitterList[0].AngleRange = new Vector2(0, 180);
                            timedProjectile.EmitterList[0].SpeedRange = new Vector2(0.25f, 0.5f);
                            //timedProjectile.EmitterList[0].Gravity = -0.05f;

                            if (timedProjectile.CurrentTime >= timedProjectile.MaxTime - 500)
                                timedProjectile.EmitterList[0].AddMore = false;
                        }
                        break;
                }

                #region This controls what happens when a TimedProjectile intersects the ground
                if (timedProjectile.Position.Y > timedProjectile.MaxY && timedProjectile.Active == true)
                {
                    switch (timedProjectile.HeavyProjectileType)
                    {
                        case HeavyProjectileType.ClusterBombShell:
                            {
                                Emitter ExplosionEmitter2 = new Emitter(SplodgeParticle, new Vector2(timedProjectile.Position.X, timedProjectile.MaxY),
                                    new Vector2(0, 180), new Vector2(1, 4), new Vector2(10, 30), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                    new Vector2(0.02f, 0.06f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 0.2f, 20, 10, true, new Vector2(timedProjectile.MaxY + 8, timedProjectile.MaxY + 8));

                                YSortedEmitterList.Add(ExplosionEmitter2);
                                //DrawableList.Add(ExplosionEmitter2);
                                DeactivateProjectile();
                            }
                            break;

                        case HeavyProjectileType.Grenade:
                            { 
                                //Grenade should leave puffs of dust as it bounces

                                //if (timedProjectile.Detonated == false &&
                                //    timedProjectile.Node1.Velocity.Y != 0 &&
                                //    timedProjectile.Node2.Velocity.Y != 0 &&
                                //    timedProjectile.Node2.Friction != 1.0f ||
                                //    timedProjectile.Node1.Friction != 1.0f)
                                //{
                                //    //Emitter DebrisEmitter2 = new Emitter(SplodgeParticle, new Vector2(timedProjectile.Position.X, timedProjectile.Position.Y),
                                //    //                            new Vector2(80, 100), new Vector2(3, 5), new Vector2(20, 40), 0.85f, true,
                                //    //                            new Vector2(0, 360), new Vector2(1, 3),
                                //    //                            new Vector2(0.01f, 0.03f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f,
                                //    //                            0.1f, 2, 3, true, new Vector2(timedProjectile.Position.Y + 8, timedProjectile.Position.Y + 16), null, timedProjectile.Position.Y / 1080,
                                //    //                            true, true, null, null, null, null, null, true, true, 50f, false);
                                //    //YSortedEmitterList.Add(DebrisEmitter2);

                                //    Emitter DustEmitter = new Emitter(SmokeParticle,
                                //                   new Vector2(timedProjectile.Position.X, timedProjectile.Position.Y),
                                //                   new Vector2(80, 100), new Vector2(1f, 2.5f), new Vector2(70, 100), 0.8f, true, new Vector2(0, 360),
                                //                   new Vector2(0.5f, 1f), new Vector2(0.2f, 0.5f), DirtColor2, DirtColor, 0.03f, 0.02f, 5, 2, false,
                                //                   new Vector2(0, 1080), false, timedProjectile.Position.Y / 1080,
                                //                   null, null, null, null, null, null, 0.08f, true, true);
                                //    YSortedEmitterList.Add(DustEmitter);
                                //}
                            }
                            break;
                    }
                }
                #endregion

                #region This controls what happens when a TimedProjectile hits a wall
                if (TrapList.Any(Trap => Trap.DestinationRectangle.Intersects(timedProjectile.CollisionRectangle) &&
                    Trap.TrapType == TrapType.Wall))
                {
                    int index = TrapList.IndexOf(TrapList.First(Trap => Trap.DestinationRectangle.Intersects(timedProjectile.CollisionRectangle)));

                    Trap trap = TrapList[index];

                    if (timedProjectile.Active == true)
                    {
                        switch (timedProjectile.HeavyProjectileType)
                        {
                            case HeavyProjectileType.Grenade:
                                //Make grenade bounce off of walls
                                break;
                        }
                    }
                }
                #endregion

                #region What happens when a TimedProjectile runs out of time
                if (timedProjectile.Detonated == true && timedProjectile.Active == true)
                {
                    switch (timedProjectile.HeavyProjectileType)
                    {
                        #region Cluster bomb shell
                        case HeavyProjectileType.ClusterBombShell:
                            {
                                Emitter ExplosionEmitter = new Emitter(ExplosionParticle,
                                        new Vector2(timedProjectile.Position.X, timedProjectile.Position.Y),
                                        new Vector2(0, 180), new Vector2(1, 5), new Vector2(1, 20), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 7, false,
                                        new Vector2(0, 1080), true);

                                YSortedEmitterList.Add(ExplosionEmitter);
                                //DrawableList.Add(ExplosionEmitter);

                                AddCluster(5, timedProjectile.Position, new Vector2(0, 360), HeavyProjectileType.ClusterBomb, timedProjectile.YRange, 2f);
                                timedProjectile.Active = false;
                            }
                            break;
                        #endregion

                        #region Grenade
                        case HeavyProjectileType.Grenade:
                            {
                                Explosion newExplosion = new Explosion(timedProjectile.Position, 30, 10);
                                ExplosionList.Add(newExplosion);

                                Emitter ExplosionEmitter = new Emitter(ExplosionParticle,
                                        new Vector2(timedProjectile.Sticks.Center.X, timedProjectile.Position.Y),
                                        new Vector2(20, 160), new Vector2(6, 14), new Vector2(5, 7), 1f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(3, 4), Color.Lerp(ExplosionColor, Color.Transparent, 0.5f),
                                        Color.Lerp(ExplosionColor2, Color.Transparent, 0.5f), 0.0f, 0.1f, 1, 1, false,
                                        new Vector2(timedProjectile.MaxY, timedProjectile.MaxY + 8), true, timedProjectile.MaxY / 1080);
                                YSortedEmitterList.Add(ExplosionEmitter);

                                Emitter ExplosionEmitter2 = new Emitter(ExplosionParticle,
                                        new Vector2(timedProjectile.Sticks.Center.X, timedProjectile.Position.Y),
                                        new Vector2(70, 110), new Vector2(6, 12), new Vector2(7, 18), 1f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(3, 4), Color.Lerp(ExplosionColor, Color.Transparent, 0.5f),
                                        Color.Lerp(ExplosionColor2, Color.DarkRed, 0.5f), 0.0f, 0.15f, 10, 1, false,
                                        new Vector2(timedProjectile.MaxY, timedProjectile.MaxY + 8), true, timedProjectile.MaxY / 1080);
                                YSortedEmitterList.Add(ExplosionEmitter2);

                                Emitter SmokeEmitter = new Emitter(SmokeParticle,
                                        new Vector2(timedProjectile.Sticks.Center.X, timedProjectile.Sticks.Center.Y),
                                        new Vector2(0, 360), new Vector2(0f, 1f), new Vector2(30, 40), 0.8f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(0.8f, 1.2f), SmokeColor1, SmokeColor2, -0.02f, 0.4f, 20, 1, false,
                                        new Vector2(timedProjectile.MaxY, timedProjectile.MaxY + 8), false, timedProjectile.MaxY / 1080,
                                        null, null, null, null, null, null, null, true, true);
                                YSortedEmitterList.Add(SmokeEmitter);

                                Emitter ExplosionEmitter3 = new Emitter(ExplosionParticle2,
                                        new Vector2(timedProjectile.Sticks.Center.X, timedProjectile.Position.Y),
                                        new Vector2(0, 360), new Vector2(1, 2), new Vector2(25, 40), 0.35f, true, new Vector2(0, 0),
                                        new Vector2(0, 0), new Vector2(0.085f, 0.2f), FireColor,
                                        Color.Lerp(Color.Red, FireColor2, 0.5f), -0.1f, 0.05f, 10, 1, false,
                                        new Vector2(timedProjectile.MaxY, timedProjectile.MaxY + 8), true, timedProjectile.MaxY / 1080,
                                        null, null, null, null, null, null, new Vector2(0.0025f, 0.0025f), true, true, 50);
                                YSortedEmitterList.Add(ExplosionEmitter3);

                                if (timedProjectile.Position.Y >= timedProjectile.MaxY)
                                {
                                    Decal NewDecal = new Decal(ExplosionDecal1, new Vector2(timedProjectile.Sticks.Center.X, timedProjectile.Sticks.Center.Y),
                                                         (float)RandomDouble(0, 0), timedProjectile.YRange, timedProjectile.MaxY, 0.5f);

                                    DecalList.Add(NewDecal);

                                    Emitter DebrisEmitter = new Emitter(SplodgeParticle,
                                            new Vector2(timedProjectile.Sticks.Center.X, timedProjectile.MaxY),
                                            new Vector2(40, 140), new Vector2(5, 7), new Vector2(30, 110), 1f, true, new Vector2(0, 360),
                                            new Vector2(1, 3), new Vector2(0.007f, 0.05f), Color.DarkSlateGray, Color.SaddleBrown,
                                            0.2f, 0.2f, 5, 1, true, new Vector2(timedProjectile.MaxY + 8, timedProjectile.MaxY + 16), null, timedProjectile.MaxY / 1080);
                                    YSortedEmitterList.Add(DebrisEmitter);

                                    Emitter DebrisEmitter2 = new Emitter(SplodgeParticle,
                                            new Vector2(timedProjectile.Sticks.Center.X, timedProjectile.Position.Y), new Vector2(50, 130),
                                            new Vector2(2, 8), new Vector2(80, 150), 1f, true, new Vector2(0, 360), new Vector2(1, 3),
                                            new Vector2(0.01f, 0.02f), Color.Gray, Color.SaddleBrown, 0.2f, 0.3f, 10, 5, true,
                                            new Vector2(timedProjectile.MaxY + 8, timedProjectile.MaxY + 16), null, timedProjectile.MaxY / 1080);
                                    YSortedEmitterList.Add(DebrisEmitter2);
                                }

                                Emitter SparkEmitter = new Emitter(RoundSparkParticle,
                                        new Vector2(timedProjectile.Sticks.Center.X, timedProjectile.Sticks.Center.Y), new Vector2(0, 360),
                                        new Vector2(1, 4), new Vector2(120, 140), 1f, true, new Vector2(0, 360), new Vector2(1, 3),
                                        new Vector2(0.1f, 0.3f), Color.Orange, Color.OrangeRed, 0.05f, 0.1f, 2, 7, true,
                                        new Vector2(timedProjectile.MaxY + 4, timedProjectile.MaxY + 16), null, timedProjectile.MaxY / 1080);
                                AlphaEmitterList.Add(SparkEmitter);

                                timedProjectile.Active = false;
                            }
                            break;
                        #endregion

                        #region Gas Grenade
                        case HeavyProjectileType.GasGrenade:
                            #region This makes a radiation symbol from gas
                            //Emitter gasEmitter = new Emitter(SmokeParticle, timedProjectile.Sticks.Center, new Vector2(0, 360),
                            //    new Vector2(0.5f, 6.0f), new Vector2(1000, 3000), 0.51f, true, new Vector2(0, 360), new Vector2(-0.5f, 0.5f),
                            //    new Vector2(0.5f, 1.0f), Color.ForestGreen, Color.LawnGreen, -0.002f, 1f, 5, 1, false, new Vector2(0, 1080), false,
                            //    1, null, null, null, null, null, null, new Vector2(0.05f, 0.05f), true, true, 500, true);
                            //EmitterList2.Add(gasEmitter);

                            //Emitter gasEmitter2 = new Emitter(SmokeParticle, timedProjectile.Sticks.Center, new Vector2(270 + 120 - 30, 270 + 120 + 30),
                            //    new Vector2(0.2f, 1.0f), new Vector2(200, 400), 0.51f, true, new Vector2(0, 360), new Vector2(-0.5f, 0.5f),
                            //    new Vector2(0.25f, 0.5f), Color.GreenYellow, Color.LimeGreen, -0.002f, 0.85f, 5, 1, false, new Vector2(0, 1080), false,
                            //    1, null, null, null, null, null, null, new Vector2(0.0051f,0.0051f), true, true, 500, true);
                            //EmitterList2.Add(gasEmitter2);

                            //Emitter gasEmitter3 = new Emitter(SmokeParticle, timedProjectile.Sticks.Center, new Vector2(270 - 120 - 30, 270 - 120 + 30),
                            //    new Vector2(0.2f, 1.0f), new Vector2(200, 400), 0.51f, true, new Vector2(0, 360), new Vector2(-0.5f, 0.5f),
                            //    new Vector2(0.25f, 0.5f), Color.GreenYellow, Color.LimeGreen, -0.002f, 0.85f, 5, 1, false, new Vector2(0, 1080), false,
                            //    1, null, null, null, null, null, null, new Vector2(0.0051f, 0.0051f), true, true, 500, true);
                            //EmitterList2.Add(gasEmitter3);

                            //Emitter gasEmitter4 = new Emitter(SmokeParticle, timedProjectile.Sticks.Center, new Vector2(270 - 30, 270 + 30),
                            //    new Vector2(0.2f, 1.0f), new Vector2(200, 400), 0.51f, true, new Vector2(0, 360), new Vector2(-0.5f, 0.5f),
                            //    new Vector2(0.25f, 0.5f), Color.GreenYellow, Color.LimeGreen, -0.002f, 0.85f, 5, 1, false, new Vector2(0, 1080), false,
                            //    1, null, null, null, null, null, null, new Vector2(0.0051f, 0.0051f), true, true, 500, true);
                            //EmitterList2.Add(gasEmitter4);

                            //Emitter gasEmitter5 = new Emitter(SmokeParticle, timedProjectile.Sticks.Center, new Vector2(0, 360),
                            //    new Vector2(0f, 0.05f), new Vector2(200, 400), 0.51f, true, new Vector2(0, 360), new Vector2(-0.5f, 0.5f),
                            //    new Vector2(0.25f, 0.5f), Color.GreenYellow, Color.LimeGreen, -0.002f, 0.85f, 10, 1, false, new Vector2(0, 1080), false,
                            //    1, null, null, null, null, null, null, new Vector2(0.0051f, 0.0051f), true, true, 500, true);
                            //EmitterList2.Add(gasEmitter5);
                            #endregion

                            Emitter gasEmitter = new Emitter(SmokeParticle, timedProjectile.Sticks.Center, new Vector2(-10, 190),
                                new Vector2(0.5f, 6.0f), new Vector2(800, 1000), 0.25f, true, new Vector2(0, 360), new Vector2(-0.5f, 0.5f),
                                new Vector2(0.5f, 1.0f), Color.LawnGreen, Color.GreenYellow, 0.003f, 1f, 2, 2, true, new Vector2(timedProjectile.MaxY, timedProjectile.MaxY + 16), false,
                                timedProjectile.Sticks.Center.Y / 1080, true, false, null, null, null, null, new Vector2(0.038f, 0.08f), true, true, 500, true, true);
                            YSortedEmitterList.Add(gasEmitter);

                            Emitter gasEmitter2 = new Emitter(SmokeParticle, timedProjectile.Sticks.Center, new Vector2(0, 10),
                                new Vector2(0.5f, 12.0f), new Vector2(800, 1000), 0.25f, true, new Vector2(0, 360), new Vector2(-0.5f, 0.5f),
                                new Vector2(0.5f, 1.0f), Color.LawnGreen, Color.GreenYellow, 0.003f, 2f, 10, 2, true, new Vector2(timedProjectile.MaxY, timedProjectile.MaxY + 16), true,
                                timedProjectile.Sticks.Center.Y / 1080, true, false, null, null, null, null, new Vector2(0.05f, 0.05f), true, true, 500, true, true);
                            YSortedEmitterList.Add(gasEmitter2);

                            Emitter gasEmitter3 = new Emitter(SmokeParticle, timedProjectile.Sticks.Center, new Vector2(170, 180),
                                new Vector2(0.5f, 12.0f), new Vector2(800, 1000), 0.25f, true, new Vector2(0, 360), new Vector2(-0.5f, 0.5f),
                                new Vector2(0.5f, 1.0f), Color.LawnGreen, Color.GreenYellow, 0.003f, 1f, 10, 2, true, new Vector2(timedProjectile.MaxY, timedProjectile.MaxY + 16), true,
                                timedProjectile.Sticks.Center.Y / 1080, true, false, null, null, null, null, new Vector2(0.05f, 0.05f), true, true, 500, true, true);
                            YSortedEmitterList.Add(gasEmitter3);

                            GasEmitterList.Add(gasEmitter);
                            GasEmitterList.Add(gasEmitter2);
                            GasEmitterList.Add(gasEmitter3);
                            break;
                        #endregion
                    }

                    #region Deactivate the Projectile
                    foreach (Emitter emitter in timedProjectile.EmitterList)
                    {
                        emitter.AddMore = false;
                    }

                    timedProjectile.Active = false;
                    timedProjectile.Velocity = Vector2.Zero;
                    #endregion
                }

                #endregion
            }
        }

        private void AddCluster(int number, Vector2 position, Vector2 angleRange, HeavyProjectileType type, Vector2 yRange, float speed)
        {
            HeavyProjectile heavyProjectile;

            for (int i = 0; i < number; i++)
            {
                //MaxY = Random.Next((int)minY, 930);
                switch (type)
                {
                    case HeavyProjectileType.ClusterBomb:
                        heavyProjectile = new ClusterBomb(ClusterShellSprite, SmokeParticle, position, speed, (float)RandomDouble(angleRange.X, angleRange.Y), 0.3f, 5, yRange);
                        break;

                    default:
                        heavyProjectile = null;
                        break;
                }

                if (heavyProjectile != null)
                {
                    heavyProjectile.Initialize();
                    HeavyProjectileList.Add(heavyProjectile);
                }
            }
        }

        private void LightProjectileUpdate()
        {
            //Ground.BoundingBox = new BoundingBox(new Vector3(0, MathHelper.Clamp(CursorPosition.Y, 690, 1080), 0), new Vector3(1920, MathHelper.Clamp(CursorPosition.Y + 1, 800, 1080), 0));
            //Ground.BoundingBox.Max = new Vector3(1920, MathHelper.Clamp(CursorPosition.Y + 1, 800, 1080), 0);

            //This needs to also be limited to the point below the currently selected turret
            if (CurrentTurret != null)
                Ground.BoundingBox.Min = new Vector3(0, MathHelper.Clamp(CursorPosition.Y, Math.Max(690, CurrentTurret.BaseRectangle.Bottom + 16), 960), 0);
            else
                Ground.BoundingBox.Min = new Vector3(0, MathHelper.Clamp(CursorPosition.Y, 690, 960), 0);

            #region Determine what effect to create
            Action<Vector2, Vector2, LightProjectileType> CreateEffect = (Vector2 CollisionStart, Vector2 CollisionEnd, LightProjectileType LightProjectileType) =>
            {
                switch (LightProjectileType)
                {
                    #region MachineGun
                    case LightProjectileType.MachineGun:
                        Trail = new BulletTrail(CollisionStart, CollisionEnd);
                        Trail.Segment = BulletTrailSegment;
                        Trail.Cap = BulletTrailCap;
                        Trail.SetUp();
                        TrailList.Add(Trail);
                        break;
                    #endregion

                    #region Lightning
                    case LightProjectileType.Lightning:
                        for (int i = 0; i < 5; i++)
                        {
                            Lightning = new LightningBolt(CollisionStart, CollisionEnd, Color.MediumPurple, 0.02f, 150f, false);
                            LightningList.Add(Lightning);
                        }
                        break;
                    #endregion

                    #region Beam
                    case LightProjectileType.Beam:
                        for (int i = 0; i < 5; i++)
                        {
                            Lightning = new LightningBolt(CollisionStart, CollisionEnd, RandomColor(Color.Red, Color.Orange), 0.02f, 1);
                            LightningList.Add(Lightning);
                        }

                        for (int i = 0; i < 5; i++)
                        {
                            Bolt = new LightningBolt(CollisionStart, CollisionEnd, RandomColor(Color.Red, Color.Orange), 0.02f, 200);
                            LightningList.Add(Bolt);
                        }
                        break;
                    #endregion

                    #region Freeze
                    case LightProjectileType.Freeze:
                        Emitter FlashSparks = new Emitter(BallParticle, CollisionStart,
                        new Vector2(0, 360), new Vector2(1, 3), new Vector2(5, 15), 1f, true, new Vector2(0, 360),
                        new Vector2(2, 5), new Vector2(0.25f, 0.25f), Color.Blue, Color.DeepSkyBlue, 0.0f, 0.1f, 1, 10,
                        false, new Vector2(0, 1080), false, null, false, false);

                        Emitter FlashSmoke = new Emitter(SmokeParticle, CollisionStart,
                        new Vector2(0, 360), new Vector2(1, 2), new Vector2(5, 15), 1f, true, new Vector2(0, 360),
                        new Vector2(2, 5), new Vector2(1f, 2f), Color.Blue, Color.DeepSkyBlue, 0.0f, 0.1f, 1, 1,
                        false, new Vector2(0, 1080), false, null, false, false);

                        AlphaEmitterList.Add(FlashSparks);
                        AlphaEmitterList.Add(FlashSmoke);

                        for (int i = 0; i < 15; i++)
                        {
                            Lightning = new LightningBolt(CollisionStart, CollisionEnd, RandomColor(Color.Blue, Color.White), 0.02f, 1, false);
                            LightningList.Add(Lightning);
                        }

                        for (int i = 0; i < 5; i++)
                        {
                            Bolt = new LightningBolt(CollisionStart, CollisionEnd, RandomColor(Color.Blue, Color.White), 0.02f, 200, true);
                            LightningList.Add(Bolt);
                        }
                        break;
                    #endregion

                    #region Shotgun
                    case LightProjectileType.Shotgun:
                        Trail = new BulletTrail(CollisionStart, CollisionEnd);
                        Trail.Segment = BulletTrailSegment;
                        Trail.Cap = BulletTrailCap;
                        Trail.SetUp();
                        TrailList.Add(Trail);
                        break;
                    #endregion

                    #region Persistent
                    case LightProjectileType.PersistentBeam:
                        Trail = new BulletTrail(CollisionStart, CollisionEnd);
                        Trail.Segment = BulletTrailSegment;
                        Trail.Cap = BulletTrailCap;
                        Trail.SetUp();
                        TrailList.Add(Trail);
                        break;
                    #endregion
                }
            };
            #endregion

            #region Determine what the invader does when hit by a projectile
            Action<Turret, Invader, Vector2> InvaderEffect = (Turret Turret, Invader hitInvader, Vector2 collisionEnd) =>
            {
                float TurretInvaderDist = Vector2.Distance(Turret.BarrelEnd, collisionEnd);
                if (Turret.Range != 0)
                {
                    //The damage beyond the range needs to be reduced. i.e. every pixel after 500 px needs to reduce damage
                    if (TurretInvaderDist <= Turret.Range)
                    {
                        switch (Turret.TurretFireType)
                        {
                            default:
                                hitInvader.TurretDamage(-Turret.Damage);
                                break;

                            case TurretFireType.Beam:
                                hitInvader.HitByBeam = true;
                                hitInvader.BeamDelay = Turret.FireDelay;

                                if (hitInvader.CurrentBeamDelay >= hitInvader.BeamDelay)
                                {
                                    hitInvader.TurretDamage(-Turret.Damage);
                                    hitInvader.CurrentBeamDelay = 0;
                                }
                                break;
                        }
                    }
                    else
                    {
                        float OverRange = TurretInvaderDist - Turret.Range;
                        float DamageReduction = Math.Min((100 / (2 * Turret.Range)) * OverRange, 50);
                        float FinalDamage = ((100 - DamageReduction) / 100) * Turret.Damage;
                        switch (Turret.TurretFireType)
                        {
                            default:
                                hitInvader.TurretDamage((int)-FinalDamage);
                                break;

                            case TurretFireType.Beam:
                                hitInvader.HitByBeam = true;
                                hitInvader.BeamDelay = Turret.FireDelay;

                                if (hitInvader.CurrentBeamDelay >= hitInvader.BeamDelay)
                                {
                                    hitInvader.TurretDamage((int)-FinalDamage);
                                    hitInvader.CurrentBeamDelay = 0;
                                }
                                break;
                        }                        
                    }
                }
                else
                {
                    switch (Turret.TurretFireType)
                    {
                        default:
                            hitInvader.TurretDamage(-Turret.Damage); 
                            break;

                        case TurretFireType.Beam:
                            hitInvader.HitByBeam = true;
                            hitInvader.BeamDelay = Turret.FireDelay;

                            if (hitInvader.CurrentBeamDelay >= hitInvader.BeamDelay)
                            {
                                hitInvader.TurretDamage(-Turret.Damage);
                                hitInvader.CurrentBeamDelay = 0;
                            }
                            break;
                    }               
                }

                //hitInvader.TurretDamage(-Turret.Damage);
                //if (hitInvader.CurrentHP <= 0)
                //    Resources += hitInvader.ResourceValue;

                switch (Turret.TurretType)
                {
                    #region Freeze
                    case TurretType.Freeze:
                        switch (hitInvader.InvaderType)
                        {
                            #region Default
                            default:
                                hitInvader.Freeze(new FreezeStruct() { Milliseconds = 3000 }, Color.SkyBlue);

                                Emitter InvaderSparks = new Emitter(BallParticle, new Vector2(hitInvader.DestinationRectangle.Center.X, hitInvader.DestinationRectangle.Bottom),
                                new Vector2(70, 110), new Vector2(1, 3), new Vector2(5, 15), 1f, true, new Vector2(0, 360),
                                new Vector2(2, 5), new Vector2(0.25f, 0.25f), Color.Blue, Color.DeepSkyBlue, 0.0f, 0.1f, 1, 10,
                                false, new Vector2(0, 1080), false, 0, false, false);

                                Emitter InvaderSmoke = new Emitter(SmokeParticle, new Vector2(hitInvader.DestinationRectangle.Center.X, hitInvader.DestinationRectangle.Bottom),
                                new Vector2(70, 110), new Vector2(1, 3), new Vector2(5, 15), 1f, true, new Vector2(0, 360),
                                new Vector2(2, 5), new Vector2(1f, 1f), Color.Blue, Color.DeepSkyBlue, 0.0f, 0.1f, 1, 10,
                                false, new Vector2(0, 1080), false, 0, false, false);

                                AlphaEmitterList.Add(InvaderSparks);
                                AlphaEmitterList.Add(InvaderSmoke);
                                break;
                            #endregion
                        }
                        break;
                    #endregion

                    #region Lightning
                    case TurretType.Lightning:
                        {
                            switch (hitInvader.InvaderType)
                            {
                                #region Default
                                default:
                                    foreach (Invader invader in InvaderList)
                                    {
                                        if (Vector2.Distance(new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                                            new Vector2(hitInvader.DestinationRectangle.Center.X, hitInvader.DestinationRectangle.Center.Y)) < 150)
                                        {
                                            for (int i = 0; i < 2; i++)
                                            {
                                                Bolt = new LightningBolt(collisionEnd,
                                                    new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                                                    Color.MediumPurple, 0.02f, 150f, true);

                                                LightningList.Add(Bolt);
                                            }

                                            if (invader != hitInvader)
                                            {
                                                invader.CurrentHP -= 5;
                                                //NumberChangeList.Add(new NumberChange(DefaultFont, invader.Position, new Vector2(0, -1), 5));
                                            }
                                        }
                                    }
                                    break;
                                #endregion
                            }
                        }
                        break;
                    #endregion

                    #region Machine Gun
                    case TurretType.MachineGun:
                        {
                            switch (hitInvader.InvaderType)
                            {
                                #region Soldier
                                case InvaderType.Soldier:
                                    Emitter BloodEmitter = new Emitter(SplodgeParticle,
                                    new Vector2(hitInvader.DestinationRectangle.Center.X, collisionEnd.Y),
                                    new Vector2(
                                    MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - (float)RandomDouble(0, 45),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) + (float)RandomDouble(0, 45)),
                                    new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360),
                                    new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.DarkRed, Color.Red,
                                    0.1f, 0.1f, 10, 5, true, new Vector2(hitInvader.MaxY, hitInvader.MaxY), false, 1, true, false);

                                    Emitter BloodEmitter2 = new Emitter(SplodgeParticle,
                                    new Vector2(hitInvader.DestinationRectangle.Center.X, collisionEnd.Y),
                                    new Vector2(
                                    MathHelper.ToDegrees(-(float)Math.Atan2(-CurrentProjectile.Ray.Direction.Y, -CurrentProjectile.Ray.Direction.X)) - (float)RandomDouble(0, 45),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(-CurrentProjectile.Ray.Direction.Y, -CurrentProjectile.Ray.Direction.X)) + (float)RandomDouble(0, 45)),
                                    new Vector2(1, 3), new Vector2(30, 60), 0.5f, true, new Vector2(0, 360),
                                    new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.DarkRed, Color.Red,
                                    0.1f, 0.1f, 10, 5, true, new Vector2(hitInvader.MaxY, hitInvader.MaxY), false, 1, true, false);

                                    YSortedEmitterList.Add(BloodEmitter);
                                    //DrawableList.Add(BloodEmitter);

                                    YSortedEmitterList.Add(BloodEmitter2);
                                    //DrawableList.Add(BloodEmitter2);
                                    break;
                                #endregion
                            }
                        }
                        break;
                    #endregion
                }
            };
            #endregion

            #region Determine what the trap does when hit by a projectile
            Action<Vector2, Turret, Trap> TrapEffect = (Vector2 CollisionEnd, Turret turret, Trap Trap) =>
            {
                switch (turret.TurretType)
                {
                    default:
                        switch (Trap.TrapType)
                        {
                            case TrapType.Wall:
                                Emitter Emitter = new Emitter(SplodgeParticle, CollisionEnd,
                                    new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                    new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360),
                                    new Vector2(1, 3), new Vector2(0.02f, 0.05f), Color.Gray, Color.DarkGray,
                                    0.1f, 0.1f, 10, 2, true, new Vector2(Trap.DestinationRectangle.Bottom, Trap.DestinationRectangle.Bottom), false, 1, true, false);

                                YSortedEmitterList.Add(Emitter);
                                //DrawableList.Add(Emitter);
                                break;

                            case TrapType.Barrel:
                                Trap.CurrentHP -= turret.Damage;
                                break;
                        }
                        break;
                }
            };
            #endregion

            #region Determine what the ground does when hit by a projectile
            Action<Vector2, Vector2, LightProjectileType> GroundEffect = (Vector2 CollisionStart, Vector2 CollisionEnd, LightProjectileType LightProjectileType) =>
            {
                switch (LightProjectileType)
                {
                    #region MachineGun
                    case LightProjectileType.MachineGun:
                        Emitter DebrisEmitter = new Emitter(SplodgeParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                                            new Vector2(60, 120), new Vector2(2, 4), new Vector2(20, 40), 0.85f, true,
                                                            new Vector2(0, 360), new Vector2(1, 3),
                                                            new Vector2(0.02f, 0.04f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f,
                                                            0.1f, 5, 1, true, new Vector2(CollisionEnd.Y + 8, CollisionEnd.Y + 16), null, CollisionEnd.Y / 1080,
                                                            true, true, null, null, null, null, null, true, true, 50f, false);
                        YSortedEmitterList.Add(DebrisEmitter);

                        Emitter DebrisEmitter2 = new Emitter(SplodgeParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                                            new Vector2(80, 100), new Vector2(3, 5), new Vector2(20, 40), 0.85f, true,
                                                            new Vector2(0, 360), new Vector2(1, 3),
                                                            new Vector2(0.01f, 0.03f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f,
                                                            0.1f, 2, 3, true, new Vector2(CollisionEnd.Y + 8, CollisionEnd.Y + 16), null, CollisionEnd.Y / 1080,
                                                            true, true, null, null, null, null, null, true, true, 50f, false);
                        YSortedEmitterList.Add(DebrisEmitter2);

                        Emitter DustEmitter = new Emitter(SmokeParticle,
                                       new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                       new Vector2(80, 100), new Vector2(2f, 4f), new Vector2(70, 100), 0.8f, true, new Vector2(0, 360),
                                       new Vector2(0.5f, 1f), new Vector2(0.2f, 0.5f), DirtColor2, DirtColor, 0.03f, 0.02f, 5, 2, false,
                                       new Vector2(0, 1080), false, CollisionEnd.Y / 1080,
                                       null, null, null, null, null, null, new Vector2(0.08f, 0.08f), true, true);
                        YSortedEmitterList.Add(DustEmitter);

                        //Emitter SparkEmitter = new Emitter(RoundSparkParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                        //                                   new Vector2(0, 180), new Vector2(8,10), new Vector2(2, 15), 0.5f, true, 
                        //                                   new Vector2(0, 360), new Vector2(1, 1), new Vector2(0.01f, 0.5f), 
                        //                                   Color.Orange, Color.OrangeRed, -0.0f, 0.1f, 10, 1, true, 
                        //                                   new Vector2(CollisionEnd.Y + 8, CollisionEnd.Y + 8), false, 
                        //                                   1, true, true, null, null, null, true);
                        //AlphaEmitterList.Add(SparkEmitter);

                        Decal NewDecal = new Decal(ExplosionDecal1, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                                   (float)RandomDouble(0, 0), new Vector2(690, 930), CollisionEnd.Y, 0.2f);

                        DecalList.Add(NewDecal);
                        break;
                    #endregion

                    #region Lightning
                    case LightProjectileType.Lightning:

                        break;
                    #endregion

                    #region Beam
                    case LightProjectileType.Beam:

                        break;
                    #endregion

                    #region Freeze
                    case LightProjectileType.Freeze:

                        break;
                    #endregion

                    #region Pulse Gun
                    case LightProjectileType.Pulse:
                        Emitter PulseDebrisEmitter = new Emitter(SplodgeParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                        new Vector2(60, 120), new Vector2(2, 4), new Vector2(20, 40), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                        new Vector2(0.01f, 0.03f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 0.1f, 2, 2, true, new Vector2(CollisionEnd.Y + 8, CollisionEnd.Y + 8));
                        YSortedEmitterList.Add(PulseDebrisEmitter);
                        //DrawableList.Add(PulseDebrisEmitter);

                        Emitter PulseSmokeEmitter = new Emitter(SmokeParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y - 4),
                        new Vector2(90, 90), new Vector2(0.5f, 1f), new Vector2(20, 30), 1f, true, new Vector2(0, 0),
                        new Vector2(-2, 2), new Vector2(0.5f, 1f), DirtColor, DirtColor2, 0f, 0.02f, 10, 1, false, new Vector2(0, 1080), false);
                        YSortedEmitterList.Add(PulseSmokeEmitter);
                        //DrawableList.Add(PulseSmokeEmitter);

                        Emitter PulseSparkEmitter = new Emitter(BallParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                        new Vector2(0, 0), new Vector2(0, 0), new Vector2(2, 5), 1f, true, new Vector2(0, 0),
                        new Vector2(0, 0), new Vector2(0.25f, 0.25f), FireColor, FireColor2, 0f, 0.1f, 500, 1,
                        false, new Vector2(0, 1080));
                        AlphaEmitterList.Add(PulseSparkEmitter);
                        break;
                    #endregion
                }
            };
            #endregion


            foreach (Turret turret in TurretList)
            {
                foreach (LightProjectile projectile in LightProjectileList)
                {
                    CurrentProjectile = projectile;

                    #region Check what was hit by the projectile - Invader, Trap, PowerUp Delivery, Ground or Sky
                    if (turret != null && turret.Active == true && turret.Selected == true && CurrentProjectile != null)
                    {
                        #region Variables used for checking bullet collisions
                        //List of traps that intersect with the projectile ray AND are solid
                        List<Trap> HitTraps = TrapList.FindAll(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) != null && Trap.Solid == true);

                        //List of invaders that intersect with the projectile ray
                        List<Invader> HitInvaders = InvaderList.FindAll(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) != null);

                        Vector2 CollisionEnd;

                        Trap HitTrap;
                        Invader HitInvader;

                        float? MinDistInv, MinDistTrap, MinDistDelivery;
                        float MinDist;
                        #endregion

                        MinDistTrap = HitTraps.Min(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray));
                        HitTrap = HitTraps.Find(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) == MinDistTrap);

                        MinDistInv = HitInvaders.Min(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray));
                        HitInvader = HitInvaders.Find(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) == MinDistInv);

                        #region Add the minimum distances to the list of distances
                        //Consider changing this to an enumerated. Might be more efficient
                        List<float> DistanceList = new List<float>();

                        if (MinDistInv != null)
                            DistanceList.Add((float)MinDistInv);

                        if (MinDistTrap != null)
                            DistanceList.Add((float)MinDistTrap);

                        if (PowerupDelivery != null)
                        {
                            if (CurrentProjectile.Ray.Intersects(PowerupDelivery.BoundingBox) != null)
                            {
                                MinDistDelivery = CurrentProjectile.Ray.Intersects(PowerupDelivery.BoundingBox);
                                DistanceList.Add((float)MinDistDelivery);
                            }
                        }
                        #endregion

                        if (HitInvaders.Count == 0)
                        {
                            foreach (Invader invader in InvaderList)
                            {
                                invader.CurrentBeamDelay = 0;
                                invader.HitByBeam = false;
                            }
                        }

                        //If there was a collision with an object
                        if (DistanceList.Count > 0)
                        {
                            //Find the closest collision (Trap, invader of powerup deliver)
                            MinDist = DistanceList.Min();

                            #region Trap was closest
                            if (MinDist == MinDistTrap)
                            {
                                CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)MinDist),
                                                           turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)MinDist));

                                CreateEffect(turret.BarrelEnd, CollisionEnd, CurrentProjectile.LightProjectileType);
                                TrapEffect(CollisionEnd, turret, HitTrap);
                            }
                            #endregion

                            #region Invader was closest
                            if (MinDist == MinDistInv)
                            {
                                CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)MinDist),
                                                           turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)MinDist));

                                CreateEffect(turret.BarrelEnd, CollisionEnd, CurrentProjectile.LightProjectileType);
                                InvaderEffect(turret, HitInvader, CollisionEnd);

                                foreach (Invader invader in InvaderList)
                                {
                                    if (invader != HitInvader)
                                    {
                                        invader.CurrentBeamDelay = 0;
                                        invader.HitByBeam = false;
                                    }
                                }
                            }
                            #endregion

                            #region Powerup Delivery was closest
                            if (PowerupDelivery != null && MinDist == CurrentProjectile.Ray.Intersects(PowerupDelivery.BoundingBox))
                            {
                                CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)MinDist),
                                                           turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)MinDist));

                                CreateEffect(turret.BarrelEnd, CollisionEnd, CurrentProjectile.LightProjectileType);
                                //TrapEffect(CollisionEnd, turret, HitTrap);
                                PowerupDelivery.CurrentHP -= turret.Damage;
                            }
                            #endregion
                        }
                        
                        #region Ground
                        //If a projectile doesn't hit a trap or an invader but does hit the ground
                        if (HitTraps.All(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                            InvaderList.All(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                            CurrentProjectile.Ray.Intersects(Ground.BoundingBox) != null &&
                            CurrentProjectile.Active == true)
                        {
                            if (PowerupDelivery == null || CurrentProjectile.Ray.Intersects(PowerupDelivery.BoundingBox) == null)
                            {
                                var DistToGround = CurrentProjectile.Ray.Intersects(Ground.BoundingBox);

                                if (DistToGround != null)
                                {
                                    CollisionEnd = new Vector2(turret.BarrelCenter.X + (CurrentProjectile.Ray.Direction.X * (float)DistToGround),
                                                               turret.BarrelCenter.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToGround));

                                    CollisionEnd.Y += (float)RandomDouble(-turret.AngleOffset * 5, turret.AngleOffset * 5);

                                    CreateEffect(turret.BarrelEnd, CollisionEnd, CurrentProjectile.LightProjectileType);
                                    GroundEffect(turret.BarrelEnd, CollisionEnd, CurrentProjectile.LightProjectileType);
                                }
                            }
                        }
                        #endregion

                        #region Sky

                        //If a projectile doesn't hit a trap, an invader or the ground
                        if (HitTraps.All(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                            InvaderList.All(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                            CurrentProjectile.Ray.Intersects(Ground.BoundingBox) == null &&
                            CurrentProjectile.Active == true)
                        {
                            if (PowerupDelivery == null || CurrentProjectile.Ray.Intersects(PowerupDelivery.BoundingBox) == null)
                            {
                                CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * 1920),
                                                           turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * 1920));

                                CreateEffect(turret.BarrelEnd, CollisionEnd, CurrentProjectile.LightProjectileType);
                            }
                        }
                        #endregion

                        #region This is to make the persistent beam projectiles work properly
                        //i.e. The beam doesn't disappear until the mouse is released
                        //Should probably be relocated to the fire-type code
                        if (CurrentProjectile != null)
                        {
                            if (turret.TurretFireType == TurretFireType.Beam)
                            {
                                CurrentProjectile.Ray.Direction = new Vector3(turret.FireDirection, 0);

                                if (PreviousMouseState.LeftButton == ButtonState.Pressed &&
                                    CurrentMouseState.LeftButton == ButtonState.Released)
                                {
                                    CurrentProjectile = null;
                                    projectile.Active = false;
                                }

                                if (turret.CurrentHeat >= turret.MaxHeat)
                                {
                                    CurrentProjectile = null;
                                    projectile.Active = false;
                                }
                            }
                            else
                            {
                                CurrentProjectile = null;
                                projectile.Active = false;
                            }
                        }
                        #endregion
                    }

                    if (CurrentProjectile == null)
                    {
                        foreach (Invader invader in InvaderList)
                        {
                            invader.HitByBeam = false;
                        }
                    }
                    #endregion
                }
            }
        }

        private void ExplosionsUpdate(GameTime gameTime)
        {
            //This just controls random behaviour that occurs when an explosion happens, such as grass moving etc.
            //foreach (Explosion explosion in ExplosionList)
            //{
            //    foreach (GrassBlade blade in GrassBladeList)
            //    {
            //        if (Vector2.Distance(new Vector2(blade.BaseRectangle.Center.X, blade.BaseRectangle.Center.Y), explosion.Position) < explosion.BlastRadius / 5)
            //        {
            //            Vector2 explosionDelta = new Vector2(blade.BaseRectangle.Center.X, blade.BaseRectangle.Center.Y) - explosion.Position;
            //            float Angle = (float)Math.Atan2(explosionDelta.Y, explosionDelta.X);

            //            Emitter newEmitter = new Emitter(GrassCenter, new Vector2(blade.BaseRectangle.Center.X, blade.BaseRectangle.Bottom),
            //                new Vector2(Angle - Random.Next(10), Angle + Random.Next(10)), new Vector2(2, 5), new Vector2(500, 1000), 1f, true,
            //                new Vector2(0, 360), new Vector2(0, 3), new Vector2(0.05f, 0.2f), Color.White, Color.White,
            //                0.2f, 0.1f, 100, 1, true,
            //                new Vector2(explosion.Position.Y + 5, explosion.Position.Y + 15), false, null, true, true);
            //            YSortedEmitterList.Add(newEmitter);
            //        }
            //    }

            //    GrassBladeList.RemoveAll(GrassBlade => Vector2.Distance(new Vector2(GrassBlade.BaseRectangle.Center.X, GrassBlade.BaseRectangle.Center.Y), explosion.Position) < explosion.BlastRadius / 5);
            //}

            //foreach (Explosion explosion in ExplosionList)
            //{
            //    foreach (Particle particle in FocusedEmitter.ParticleList)
            //    {
            //        if (Vector2.Distance(particle.CurrentPosition, explosion.Position) < explosion.BlastRadius)
            //        {
            //            particle.CurrentHP /= 10;
            //            //Vector2 explosionDelta = particle.CurrentPosition - explosion.Position;
            //            //float Angle = (float)Math.Atan2(explosionDelta.Y, explosionDelta.X);

            //            //particle.Velocity = explosionDelta * (float)(RandomDouble(0.02f, 0.03f));
            //            //particle.Friction = 0.05f;
            //        }
            //    }
            //}

        }
        #endregion

        #region TRAP stuff that needs to be called every step
        private void TrapUpdate(GameTime gameTime)
        {
            foreach (Trap trap in TrapList)
            {
                trap.Update(gameTime);

                #region Update the trap quick info
                if (UITrapQuickInfoList.Count > 0)
                {
                    foreach (UITrapQuickInfo trapQuickInfo in UITrapQuickInfoList)
                    {
                        trapQuickInfo.Update(gameTime);
                    }
                }
                #endregion

                #region Check what trap is currently moused-over
                if (trap.DestinationRectangle.Contains(VectorToPoint(CursorPosition)) && UITrapOutlineList.Count == 0)
                {
                    //The outline around the trap
                    UIOutline trapOutline = new UIOutline(trap.Position, new Vector2(trap.DestinationRectangle.Width, trap.DestinationRectangle.Height), trap, null);
                    trapOutline.OutlineTexture = TurretSelectBox;
                    UITrapOutlineList.Add(trapOutline);

                    //The trap quick info
                    UITrapQuickInfo uiTrapQuickInfo = new UITrapQuickInfo(trap.Position, trap);
                    uiTrapQuickInfo.BoldFont = RobotoBold40_2;
                    uiTrapQuickInfo.Font = RobotoRegular20_0;
                    uiTrapQuickInfo.Italics = RobotoItalic20_0;
                    UITrapQuickInfoList.Add(uiTrapQuickInfo);
                }

                if (!trap.DestinationRectangle.Contains(VectorToPoint(CursorPosition)))
                {
                    UITrapOutlineList.RemoveAll(trapOutline => trapOutline.Trap == trap);
                    UITrapQuickInfoList.RemoveAll(trapQuickInfo => trapQuickInfo.Trap == trap);
                }
                #endregion

                #region Remove trap if middle-clicked
                if (trap.DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)) &&
                    CurrentMouseState.MiddleButton == ButtonState.Released &&
                    PreviousMouseState.MiddleButton == ButtonState.Pressed)
                {
                    if (trap.Active == true && trap.CurrentHP == trap.MaxHP && trap.CurrentDetonateLimit == trap.DetonateLimit)
                    {
                        trap.Active = false;
                        Resources += trap.ResourceCost;

                        //Remove the outline from around the trap
                        UITrapOutlineList.RemoveAll(uiOutline => uiOutline.Trap == trap);
                    }
                }
                #endregion

                #region Specific behaviour based on the trap type
                switch (trap.TrapType)
                {
                    #region Fire trap behaviour
                    case TrapType.Fire:
                        #region Make the fire react to nearby explosion
                        foreach (Explosion explosion in ExplosionList)
                        {
                            if (Vector2.Distance(new Vector2(trap.DestinationRectangle.Center.X, trap.DestinationRectangle.Bottom),
                                explosion.Position) <= explosion.BlastRadius && explosion.Active == true)
                            {
                                float angle1, Dist;
                                float VectorX, VectorY;

                                Dist = Vector2.Distance(explosion.Position, PointToVector(trap.DestinationRectangle.Center));

                                VectorY = trap.DestinationRectangle.Center.Y - explosion.Position.Y;
                                VectorX = trap.DestinationRectangle.Center.X - explosion.Position.X;

                                angle1 = MathHelper.ToDegrees(-(float)Math.Atan2(VectorY, VectorX));

                                trap.CurrentAffectedTime = 0;

                                foreach (Emitter emitter in trap.TrapEmitterList)
                                {
                                    emitter.AngleRange = new Vector2(angle1, angle1);
                                    emitter.SpeedRange = new Vector2(1, 1.5f);

                                    foreach (Particle particle in emitter.ParticleList)
                                    {
                                        particle.Angle = (float)RandomDouble(angle1, angle1);
                                        particle.Speed = (float)RandomDouble(1, 1.5f);
                                    }
                                }
                            }
                        }

                        foreach (Explosion enemyExplosion in EnemyExplosionList)
                        {
                            if (Vector2.Distance(new Vector2(trap.DestinationRectangle.Center.X, trap.DestinationRectangle.Bottom),
                                enemyExplosion.Position) <= enemyExplosion.BlastRadius && enemyExplosion.Active == true)
                            {
                                if (trap.DestinationRectangle.Center.X > enemyExplosion.Position.X)
                                {
                                    trap.CurrentAffectedTime = 0;

                                    foreach (Emitter emitter in trap.TrapEmitterList)
                                    {
                                        emitter.AngleRange = new Vector2(0, 45);
                                        emitter.SpeedRange = new Vector2(1, 1.5f);
                                    }
                                }

                                if (trap.DestinationRectangle.Center.X <= enemyExplosion.Position.X)
                                {
                                    trap.CurrentAffectedTime = 0;

                                    foreach (Emitter emitter in trap.TrapEmitterList)
                                    {
                                        emitter.AngleRange = new Vector2(135, 180);
                                        emitter.SpeedRange = new Vector2(1, 1.5f);
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Cancel out the effect created by the explosion
                        if (trap.Affected == false)
                        {
                            foreach (Emitter emitter in trap.TrapEmitterList)
                            {
                                if (emitter.TextureName.Contains("Fire"))
                                {
                                    emitter.AngleRange = Vector2.Lerp(emitter.AngleRange, new Vector2(60, 120), 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                                    emitter.SpeedRange = Vector2.Lerp(emitter.SpeedRange, new Vector2(0.5f, 0.75f), 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                                }

                                if (emitter.TextureName.Contains("Smoke"))
                                {
                                    emitter.AngleRange = Vector2.Lerp(emitter.AngleRange, new Vector2(85, 95), 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                                    emitter.SpeedRange = Vector2.Lerp(emitter.SpeedRange, new Vector2(0.2f, 0.5f), 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                                }

                                if (emitter.TextureName.Contains("Spark"))
                                {
                                    emitter.AngleRange = Vector2.Lerp(emitter.AngleRange, new Vector2(80, 100), 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                                    emitter.SpeedRange = Vector2.Lerp(emitter.SpeedRange, new Vector2(1f, 4f), 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                                }
                            }
                        }
                        #endregion

                        #region Make less fire if the trap isn't ready to detonate yet
                        if (trap.CurrentDetonateDelay < trap.DetonateDelay)
                        {
                            foreach (Emitter emitter in trap.TrapEmitterList)
                            {
                                if (emitter.TextureName.Contains("Fire"))
                                {
                                    emitter.HPRange = new Vector2(25, 50);
                                    emitter.ScaleRange = new Vector2(0.01f, 0.05f);
                                }

                                if (emitter.TextureName.Contains("Smoke"))
                                {
                                    emitter.HPRange = new Vector2(25, 50);
                                    emitter.ScaleRange = new Vector2(0.1f, 0.3f);
                                }

                                if (emitter.TextureName.Contains("Spark"))
                                {
                                    emitter.HPRange = new Vector2(25, 50);
                                }
                            }
                        }
                        #endregion

                        #region Reset the trap back to it's original state if it's ready to detonate again
                        if (trap.CurrentDetonateDelay >= trap.DetonateDelay)
                        {
                            foreach (Emitter emitter in trap.TrapEmitterList)
                            {
                                if (emitter.TextureName.Contains("Fire"))
                                {
                                    emitter.HPRange = Vector2.Lerp(emitter.HPRange, new Vector2(90, 140), 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                                    emitter.ScaleRange = Vector2.Lerp(emitter.ScaleRange, new Vector2(0.075f, 0.15f), 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                                }

                                if (emitter.TextureName.Contains("Smoke"))
                                {
                                    emitter.HPRange = Vector2.Lerp(emitter.HPRange, new Vector2(250, 350), 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                                    emitter.ScaleRange = Vector2.Lerp(emitter.ScaleRange, new Vector2(0.2f, 0.5f), 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                                }

                                if (emitter.TextureName.Contains("Spark"))
                                {
                                    emitter.HPRange = Vector2.Lerp(emitter.HPRange, new Vector2(60, 180), 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                                }
                            }

                            //Emitter FireEmitter = new Emitter(FireParticle,
                            //    new Vector2(NewTrap.Position.X + NewTrap.TextureList[0].Width / 2, NewTrap.Position.Y + NewTrap.TextureList[0].Height), new Vector2(60, 120),
                            //    new Vector2(0.5f, 0.75f), new Vector2(90, 140), 0.85f, true,
                            //    new Vector2(-4, 4), new Vector2(-1f, 1f), new Vector2(0.075f, 0.15f), FireColor, FireColor2,
                            //    0.0f, -1, 75, 1, false, new Vector2(0, 1080), true, CursorPosition.Y / 1080, null, null, null, null, null, null, null, true, true, 150);
                            //FireEmitter.TextureName = "FireParticle";

                            //Emitter SmokeEmitter = new Emitter(SmokeParticle,
                            //    new Vector2(NewTrap.Position.X + NewTrap.TextureList[0].Width / 2, NewTrap.Position.Y + NewTrap.TextureList[0].Height - 4),
                            //    new Vector2(85, 95), new Vector2(0.2f, 0.5f), new Vector2(250, 350), 0.9f, true, new Vector2(-20, 20),
                            //    new Vector2(-2, 2), new Vector2(0.6f, 1f), SmokeColor, SmokeColor2, 0.0f, -1, 150, 1, false,
                            //    new Vector2(0, 1080), true, (CursorPosition.Y - 1) / 1080, null, null, null, null, null, null, null, true, true, 250);
                            //SmokeEmitter.TextureName = "SmokeParticle";

                            //Emitter SparkEmitter = new Emitter(RoundSparkParticle,
                            //       new Vector2(NewTrap.Position.X + NewTrap.TextureList[0].Width / 2, NewTrap.Position.Y + NewTrap.TextureList[0].Height - 4),
                            //       new Vector2(80, 100),
                            //       new Vector2(1, 4), new Vector2(60, 180), 1f, true, new Vector2(0, 360), new Vector2(1, 3),
                            //       new Vector2(0.1f, 0.3f), Color.LightYellow, Color.White, -0.001f, -1f, 100, 1, false,
                            //       new Vector2(0, 1080));
                            //SparkEmitter.TextureName = "SparkParticle";

                        }
                        break;
                        #endregion
                    #endregion

                    #region Barrel trap behaviour
                    case TrapType.Barrel:
                        if (trap.CurrentHP <= 0)
                        {
                            trap.Active = false;
                            Emitter ExplosionEmitter2 = new Emitter(SplodgeParticle, new Vector2(trap.Position.X, trap.DestinationRectangle.Bottom),
                                       new Vector2(0, 180), new Vector2(1, 4), new Vector2(10, 30), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                       new Vector2(0.02f, 0.06f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 0.2f, 20, 10, true, new Vector2(trap.DestinationRectangle.Bottom + 8, trap.DestinationRectangle.Bottom + 8));

                            YSortedEmitterList.Add(ExplosionEmitter2);
                            //DrawableList.Add(ExplosionEmitter2);

                            Emitter ExplosionEmitter = new Emitter(FireParticle,
                                    new Vector2(trap.Position.X, trap.DestinationRectangle.Bottom),
                                    new Vector2(0, 180), new Vector2(1, 5), new Vector2(1, 20), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 7, false,
                                    new Vector2(0, 1080), true);

                            YSortedEmitterList.Add(ExplosionEmitter);
                            //DrawableList.Add(ExplosionEmitter);

                            Color SmokeColor1 = Color.Lerp(Color.DarkGray, Color.Transparent, 0.5f);
                            Color SmokeColor2 = Color.Lerp(Color.Gray, Color.Transparent, 0.5f);

                            Emitter newEmitter2 = new Emitter(SmokeParticle,
                                    new Vector2(trap.Position.X, trap.DestinationRectangle.Bottom),
                                    new Vector2(80, 100), new Vector2(0.5f, 1f), new Vector2(20, 40), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(0, 0), new Vector2(1, 2), SmokeColor1, SmokeColor2, 0.0f, 0.3f, 1, 1, false,
                                    new Vector2(0, 1080), false);

                            EmitterList2.Add(newEmitter2);

                            Explosion newExplosion = new Explosion(PointToVector(trap.DestinationRectangle.Center), 150, 100);
                            ExplosionList.Add(newExplosion); 
                        }
                        break;

                    #endregion
                }
                #endregion

                #region Update all of the particle emitters for the traps
                foreach (Emitter emitter in trap.TrapEmitterList)
                {
                    //emitter.DrawDepth = trap.BoundingBox.Max.Y / 1080;

                    if (emitter.AddMore == false && emitter.ParticleList.Count == 0)
                    {
                        trap.Active = false;
                    }
                }
                #endregion
            }
        }

        private void TrapPlacement()
        {
            //if (SelectedTrap != null && 
            //    (CursorPosition.Y - CursorPosition.Y % 32) >= 672 && 
            //    (CursorPosition.Y - CursorPosition.Y % 32) <= 896)
            //{
            //    CursorColor = Color.White;
            //}
            //else
            //{
            //    CursorColor = Color.Gray;
            //}

            if (CurrentMouseState.LeftButton == ButtonState.Released &&
                PreviousMouseState.LeftButton == ButtonState.Pressed &&
                ReadyToPlace == true &&
                TrapList.Count < TrapLimit &&
                CursorPosition.X > (Tower.Position.X + Tower.Texture.Width) &&
                (CursorPosition.Y - CursorPosition.Y % 32) <= 896 &&
                (CursorPosition.Y - CursorPosition.Y % 32) >= 672 &&
                SelectedTrap != null &&
                TrapList.All(Trap => !Trap.DestinationRectangle.Contains(VectorToPoint(CursorPosition))))
            {
                //Still need to do a better check if there is a trap under where a new trap is going to be placed
                if (CursorPosition.Y < 672 || CursorPosition.Y > 928)
                {
                    return;
                }

                Vector2 NewTrapPosition = new Vector2(CursorPosition.X - CursorPosition.X % 32, CursorPosition.Y - CursorPosition.Y % 32);

                if (Resources >= TrapCost(SelectedTrap.Value))
                {
                    Resources -= TrapCost(SelectedTrap.Value);
                    
                    //PlaceUpgradedTrap(ApplyTrapUpgrades(SelectedTrap, NewTrapPosition));
                    Trap newTrap = ApplyTrapUpgrades(SelectedTrap.Value, NewTrapPosition);
                    PlaceUpgradedTrap(newTrap, NewTrapPosition);
                    //Trap newTrap = ApplyTrapUpgrades(SelectedTrap.Value, NewTrapPosition);
                    //TrapList.Add(newTrap);

                    //Make sure that the cooldowns are applied only to trap buttons as they're placed
                    //i.e. not when the button is clicked
                    for (int i = 0; i < CooldownButtonList.Count; i++)
                    {
                        if (CurrentProfile.Buttons[i] != null && CurrentProfile.Buttons[i].CurrentTrap == newTrap.TrapType)
                        {
                            CooldownButtonList[i].CoolingDown = true;
                        }
                    }

                    #region Effects to create when a trap is placed, e.g. A dust puff when a wall is placed
                    switch (newTrap.TrapType)
                    {
                        #region Wall
                        case TrapType.Wall:
                            YSortedEmitterList.Add(new Emitter(SmokeParticle,
                                new Vector2(newTrap.DestinationRectangle.Center.X, newTrap.DestinationRectangle.Bottom),
                                new Vector2(160, 200), new Vector2(1, 4), new Vector2(20, 40), 0.25f, true, new Vector2(0, 360),
                                new Vector2(0, 1.5f), new Vector2(0.1f, 0.5f), DirtColor, DirtColor2, -0.05f, 0.2f, 3, 1, false,
                                new Vector2(0, 1080), false, 0.9f, null, null, null, null, null, null, new Vector2(0.05f, 0.05f)));

                            YSortedEmitterList.Add(new Emitter(SmokeParticle,
                                new Vector2(newTrap.DestinationRectangle.Center.X, newTrap.DestinationRectangle.Bottom),
                                new Vector2(-20, 20), new Vector2(1, 4), new Vector2(20, 40), 0.25f, true, new Vector2(0, 360),
                                new Vector2(0, 1.5f), new Vector2(0.1f, 0.5f), DirtColor, DirtColor2, -0.05f, 0.1f, 3, 1, false,
                                new Vector2(0, 1080), false, 0.9f, null, null, null, null, null, null, new Vector2(0.05f, 0.05f)));
                            break;
                        #endregion

                        #region Fire
                        case TrapType.Fire:
                            AdditiveEmitterList.Add(new Emitter(RoundSparkParticle,
                                new Vector2(newTrap.DestinationRectangle.Center.X, newTrap.DestinationRectangle.Bottom),
                                new Vector2(160, 200), new Vector2(1, 4), new Vector2(20, 40), 1f, true, new Vector2(0, 360),
                                new Vector2(0, 1.5f), new Vector2(0.25f, 0.25f), FireColor, FireColor2, -0.05f, 0.2f, 3, 1, false,
                                new Vector2(0, 1080), false, 0.9f, null, null, null, null, null, null, new Vector2(0.05f, 0.05f)));

                            AdditiveEmitterList.Add(new Emitter(RoundSparkParticle,
                                new Vector2(newTrap.DestinationRectangle.Center.X, newTrap.DestinationRectangle.Bottom),
                                new Vector2(-20, 20), new Vector2(1, 4), new Vector2(20, 40), 1f, true, new Vector2(0, 360),
                                new Vector2(0, 1.5f), new Vector2(0.25f, 0.25f), FireColor, FireColor2, -0.05f, 0.1f, 3, 1, false,
                                new Vector2(0, 1080), false, 0.9f, null, null, null, null, null, null, new Vector2(0.05f, 0.05f)));
                            break;

                            
                        #endregion
                    }
                    #endregion
                }
            }
        }        

        private void TrapCollision()
        {
            foreach (Invader invader in InvaderList)
            {
                if (TrapList.Any(Trap => Trap.DestinationRectangle.Intersects(invader.DestinationRectangle)))
                {
                    Trap HitTrap = TrapList.First(Trap => Trap.DestinationRectangle.Intersects(invader.DestinationRectangle));

                    if (TrapList[TrapList.IndexOf(HitTrap)].CanTrigger == true)
                    {
                        invader.TrapDamage(HitTrap);

                        if (HitTrap.CurrentDetonateLimit > 0)
                            HitTrap.CurrentDetonateLimit -= 1;

                        HitTrap.CurrentDetonateDelay = 0;

                        switch (HitTrap.TrapType)
                        {
                            #region Fire Trap
                            case TrapType.Fire:
                                switch (HitTrap.CurrentDetonateLimit)
                                {
                                    default:
                                        invader.FireEmitter = new Emitter(FireParticle2,
                                            invader.Position, new Vector2(60, 120),
                                            new Vector2(0.5f, 0.75f), new Vector2(70, 110), 0.85f, true,
                                            new Vector2(-4, 4), new Vector2(-1f, 1f), new Vector2(0.07f, 0.15f), FireColor2, FireColor,
                                            0.0f, -1, 10, 1, false, new Vector2(0, 1080), true, CursorPosition.Y / 1080, 
                                            null, null, null, null, null, null, null, true, true, 150);

                                        //invader.FireEmitter = new Emitter(FireParticle, invader.Position,
                                        //new Vector2(0, 90), new Vector2(0.25f * 1.5f, 0.5f * 1.5f), new Vector2(40 * 0.75f, 60 * 0.75f), 0.85f, true, new Vector2(-4, 4),
                                        //new Vector2(-1, -1), new Vector2(0.2f, 0.5f), FireColor2, FireColor, 0.0f, -1, 75, 1, false, new Vector2(0, 1080),
                                        //false, invader.DrawDepth + 0.1f);

                                        HitTrap.TrapEmitterList[1].EndColor =
                                            Color.Lerp(HitTrap.TrapEmitterList[1].EndColor, Color.Red, 0.1f);

                                        HitTrap.TrapEmitterList[1].StartColor =
                                                Color.Lerp(HitTrap.TrapEmitterList[1].EndColor, Color.Red, 0.1f);
                                        break;
                                }
                                break;
                            #endregion

                            #region Sawblade Trap
                            case TrapType.SawBlade:
                                switch (invader.InvaderType)
                                {
                                    case InvaderType.Soldier:
                                    case InvaderType.SuicideBomber:
                                        EmitterList2.Add(new Emitter(SplodgeParticle, new Vector2(invader.DestinationRectangle.Center.X, HitTrap.DestinationRectangle.Center.Y),
                                        new Vector2(0, 65), new Vector2(2, 4), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), new Vector2(1, 3),
                                        new Vector2(0.02f, 0.06f), Color.DarkRed, Color.Red, 0.1f, 0.3f, 20, 20, true, new Vector2(invader.MaxY, invader.MaxY)));
                                        break;

                                    case InvaderType.Slime:
                                        EmitterList2.Add(new Emitter(SplodgeParticle, new Vector2(invader.DestinationRectangle.Center.X, HitTrap.DestinationRectangle.Center.Y),
                                        new Vector2(0, 65), new Vector2(2, 4), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), new Vector2(1, 3),
                                        new Vector2(0.02f, 0.06f), Color.HotPink, Color.LightPink, 0.1f, 0.3f, 20, 20, true, new Vector2(invader.MaxY, invader.MaxY)));
                                        break;
                                }
                                break;
                            #endregion

                            #region Catapult Trap
                            case TrapType.Catapult:
                                invader.Trajectory(new Vector2(5, -13));                                
                                break;
                            #endregion

                            #region Ice Trap
                            case TrapType.Ice:
                                invader.Freeze(new FreezeStruct()
                                {
                                    Milliseconds = 3000
                                }, Color.LightBlue);
                                break;
                            #endregion

                            #region Line Trap
                            case TrapType.Line:

                                break;
                            #endregion

                            #region Spikes Trap
                            case TrapType.Spikes:

                                break;
                            #endregion

                            #region Trigger Trap
                            case TrapType.Trigger:

                                break;
                            #endregion
                        }
                    }
                }

                if (TrapList.All(Trap => !Trap.BoundingBox.Intersects(invader.BoundingBox)))
                {
                    invader.VulnerableToTrap = true;
                }
            }
        }
        #endregion

        #region SPECIAL stuff that needs to be called every step
        public void UpdateSpecialAbilities(GameTime gameTime)
        {
            if (CurrentSpecialAbility != null)
            {
                CurrentSpecialAbility.Update(gameTime);

                switch (CurrentSpecialAbility.SpecialType)
                {
                    case SpecialType.AirStrike:
                        AirStrikeSpecial airStrike = (AirStrikeSpecial)CurrentSpecialAbility;

                        if (airStrike.CurrentTime > airStrike.TimeInterval)
                        {
                            TimerHeavyProjectile AirStrikeProjectile = new ClusterBombShell(1000, ClusterBombSprite, SmokeParticle, airStrike.CurrentPosition, 10, 0, 0.2f, 5, new Vector2(690, 930));
                            AirStrikeProjectile.Initialize();
                            TimedProjectileList.Add(AirStrikeProjectile);
                            airStrike.CurrentTime = 0;
                        }
                        break;
                }
            }
        }
        #endregion

        private void UpdateWeather(GameTime gameTime)
        {
            ////Update the weather effects such as snow, sun, rain etc.
            //CurrentWeatherTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            //switch (CurrentWeather)
            //{
            //    case Weather.Snow:
            //        {
            //            if (CurrentWeatherTime > 250)
            //            {
            //                CurrentWeatherTime = 0;
            //                WeatherSpriteList.Add(
            //                    new StaticSprite(RandomTexture(SnowDust1, SnowDust2, SnowDust3, SnowDust4, SnowDust5),
            //                        new Vector2(Random.Next(250, 1920), Random.Next(690, 930)),
            //                        new Vector2(0.5f, 0.5f), HalfWhite,
            //                        new Vector2((float)RandomDouble(0.01f, 0.07f), 0), false, false, null, null, 2500, true));
            //            }

            //            foreach (StaticSprite sprite in WeatherSpriteList)
            //            {
            //                sprite.Update(gameTime);
            //            }

            //            BlurryEmitter.Update(gameTime);
            //            FocusedEmitter.Update(gameTime);

            //            BlurryEmitter.ParticleList.RemoveAll(Particle => Particle.CurrentPosition.Y > 1080);
            //            FocusedEmitter.ParticleList.RemoveAll(Particle => Particle.CurrentPosition.Y > 1080);

            //            BlurryEmitter.Position = new Vector2(Random.Next(0, 1920), -64);
            //            FocusedEmitter.Position = new Vector2(Random.Next(0, 1920), -64);
            //        }
            //        break;                    
            //}

            //for (int i = 0; i < WeatherSpriteList.Count; i++)
            //{
            //    if (WeatherSpriteList[i].Active == false)
            //        WeatherSpriteList.RemoveAt(i);
            //}

            //WeatherSpriteList.RemoveAll(Sprite => Sprite.Active == false);
        }


        #region Handling player profile data
        public void HandleProfile(IAsyncResult result)
        {
            Device = StorageDevice.EndShowSelector(result);

            if (Device != null && Device.IsConnected)
            {
                IAsyncResult r = Device.BeginOpenContainer(ContainerName, null, null);

                result.AsyncWaitHandle.WaitOne();

                StorageContainer container = Device.EndOpenContainer(r);

                if (container.FileExists(FileName))
                {
                    StorageDevice.BeginShowSelector(this.LoadProfile, null);
                    LoadUpgrades();

                    SetUpWeaponSelection();

                    GameState = GameState.ProfileManagement;

                    HandleSelectIcons();
                    HandlePlacedIcons();
                }
                else
                {
                    GameState = TowerDefensePrototype.GameState.GettingName;
                    GetNameBack.CurrentPosition.X = -300;
                    GetNameOK.CurrentPosition.X = 1920 - 150;
                }
            }
        }

        public void NewProfile(IAsyncResult result)
        {
            Device = StorageDevice.EndShowSelector(result);

            if (Device != null && Device.IsConnected)
            {
                IAsyncResult r = Device.BeginOpenContainer(ContainerName, null, null);

                result.AsyncWaitHandle.WaitOne();

                StorageContainer container = Device.EndOpenContainer(r);

                if (container.FileExists(FileName))
                    container.DeleteFile(FileName);

                CurrentProfile.Buttons = new List<Weapon>();
                for (int i = 0; i < 10; i++)
                {
                    CurrentProfile.Buttons.Add(null);
                }

                foreach (Button button in PlaceWeaponList)
                {
                    button.IconTexture = null;
                    button.LoadContent();
                }

                Stream stream = container.CreateFile(FileName);

                using (var memoryStream = new MemoryStream())
                {
                    formatter.Serialize(memoryStream, CurrentProfile);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    var bytes = new byte[memoryStream.Length];
                    memoryStream.Read(bytes, 0, (int)memoryStream.Length);

                    byte[] keyArray;
                    string key = "5rg9rt48u23498129u0123jijrdfn48031n";

                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    hashmd5.Clear();

                    TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                    tdes.Key = keyArray;
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform cTransform = tdes.CreateEncryptor();
                    byte[] resultArray = cTransform.TransformFinalBlock(bytes, 0, bytes.Length);
                    tdes.Clear();

                    var encryptedBytes = resultArray;
                    stream.Write(encryptedBytes, 0, encryptedBytes.Length);
                }

                SetUpWeaponSelection();
                HandleSelectIcons();

                stream.Close();

                container.Dispose();

                result.AsyncWaitHandle.Close();
            }
        }

        public void LoadProfile(IAsyncResult result)
        {
            Device = StorageDevice.EndShowSelector(result);

            result = Device.BeginOpenContainer(ContainerName, null, null);
            result.AsyncWaitHandle.WaitOne();

            StorageContainer container = Device.EndOpenContainer(result);

            result.AsyncWaitHandle.Close();

            OpenFile = container.OpenFile(FileName, FileMode.Open);

            byte[] toDecryptArray = new byte[OpenFile.Length];
            OpenFile.Read(toDecryptArray, 0, (int)OpenFile.Length);

            byte[] keyArray;
            string key = "5rg9rt48u23498129u0123jijrdfn48031n";

            MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
            keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            hashmd5.Clear();

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toDecryptArray, 0, (int)toDecryptArray.Length);
            tdes.Clear();

            var thing = UTF8Encoding.UTF8.GetString(resultArray);

            Stream stream = new MemoryStream(resultArray);

            var Profile = formatter.Deserialize(stream);
            CurrentProfile = (Profile)Profile;

            OpenFile.Close();
            container.Dispose();

            HandlePlacedIcons();
        }

        public void SaveProfile(IAsyncResult result)
        {
            Device = StorageDevice.EndShowSelector(result);

            FileName = "Profile" + ProfileNumber.ToString() + ".sav";

            if (Device != null && Device.IsConnected)
            {
                IAsyncResult r = Device.BeginOpenContainer(ContainerName, null, null);

                result.AsyncWaitHandle.WaitOne();

                StorageContainer container = Device.EndOpenContainer(r);

                if (container.FileExists(FileName))
                    container.DeleteFile(FileName);

                Stream stream = container.CreateFile(FileName);

                using (var memoryStream = new MemoryStream())
                {
                    formatter.Serialize(memoryStream, CurrentProfile);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    var bytes = new byte[memoryStream.Length];
                    memoryStream.Read(bytes, 0, (int)memoryStream.Length);

                    byte[] keyArray;
                    string key = "5rg9rt48u23498129u0123jijrdfn48031n";

                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                    hashmd5.Clear();

                    TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                    tdes.Key = keyArray;
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform cTransform = tdes.CreateEncryptor();
                    byte[] resultArray = cTransform.TransformFinalBlock(bytes, 0, bytes.Length);
                    tdes.Clear();

                    var encryptedBytes = resultArray;
                    stream.Write(encryptedBytes, 0, encryptedBytes.Length);
                }

                stream.Close();

                container.Dispose();

                result.AsyncWaitHandle.Close();
            }
        }

        public void DeleteProfile(IAsyncResult result)
        {
            Device = StorageDevice.EndShowSelector(result);

            if (Device != null && Device.IsConnected)
            {
                IAsyncResult r = Device.BeginOpenContainer(ContainerName, null, null);

                result.AsyncWaitHandle.WaitOne();

                StorageContainer container = Device.EndOpenContainer(r);

                if (container.FileExists(FileName))
                    container.DeleteFile(FileName);

                container.Dispose();

                result.AsyncWaitHandle.Close();
            }
        }

        public void GetProfileName(IAsyncResult result)
        {
            Profile ThisProfile;

            Device = StorageDevice.EndShowSelector(result);

            result = Device.BeginOpenContainer(ContainerName, null, null);
            result.AsyncWaitHandle.WaitOne();

            StorageContainer container = Device.EndOpenContainer(result);

            result.AsyncWaitHandle.Close();

            if (container.FileExists(FileName))
            {
                OpenFile = container.OpenFile(FileName, FileMode.Open);

                byte[] toDecryptArray = new byte[OpenFile.Length];
                OpenFile.Read(toDecryptArray, 0, (int)OpenFile.Length);

                byte[] keyArray;
                string key = "5rg9rt48u23498129u0123jijrdfn48031n";

                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toDecryptArray, 0, (int)toDecryptArray.Length);
                tdes.Clear();

                var thing = UTF8Encoding.UTF8.GetString(resultArray);

                Stream stream = new MemoryStream(resultArray);

                var Profile = formatter.Deserialize(stream);
                ThisProfile = (Profile)Profile;


                OpenFile.Close();

                container.Dispose();

                ProfileName = ThisProfile.Name;
            }
            else
            {
                ProfileName = "+";
            }
        }

        public void SetProfileNames()
        {
            for (int i = 0; i < 4; i++)
            {
                FileName = "Profile" + (i + 1).ToString() + ".sav";
                StorageDevice.BeginShowSelector(this.GetProfileName, null);
                ProfileButtonList[i].Text = ProfileName;
            }
        }

        public void CheckFileDelete(IAsyncResult result)
        {
            Profile ThisProfile;

            Device = StorageDevice.EndShowSelector(result);

            result = Device.BeginOpenContainer(ContainerName, null, null);
            result.AsyncWaitHandle.WaitOne();

            StorageContainer container = Device.EndOpenContainer(result);

            result.AsyncWaitHandle.Close();

            if (container.FileExists(FileName))
            {
                MenuClick.Play();

                OpenFile = container.OpenFile(FileName, FileMode.Open);


                byte[] toDecryptArray = new byte[OpenFile.Length];
                OpenFile.Read(toDecryptArray, 0, (int)OpenFile.Length);

                byte[] keyArray;
                string key = "5rg9rt48u23498129u0123jijrdfn48031n";

                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toDecryptArray, 0, (int)toDecryptArray.Length);
                tdes.Clear();

                var thing = UTF8Encoding.UTF8.GetString(resultArray);

                Stream stream = new MemoryStream(resultArray);

                var Profile = formatter.Deserialize(stream);
                ThisProfile = (Profile)Profile;

                OpenFile.Close();

                container.Dispose();

                ProfileName = ThisProfile.Name;

                DeleteProfileDialog = new DialogBox(DialogBox, ShortButtonLeftSprite, ShortButtonRightSprite, RobotoRegular20_0, 
                                                    new Vector2(1920 / 2, 1080 / 2), "delete", 
                                                    "Do you want to delete " + ThisProfile.Name + "?", "cancel");
                DeleteProfileDialog.LoadContent();
                DialogVisible = true;
            }
            else
                return;
        }

        public void AddNewProfile()
        {
            List<string> TempList = new List<string>();

            for (int i = 0; i < 10; i++)
            {
                TempList.Add(null);
            }

            List<Upgrade> TempList2 = new List<Upgrade>();

            CurrentProfile = new Profile()
            {
                Name = NameInput.RealString,
                LevelNumber = 1,

                Points = 0,

                //Turrets available on profile creation
                Cannon = true,
                MachineGun = true,
                FlameThrower = true,
                Cluster = true,
                Lightning = true,
                FelCannon = true,
                Beam = true,
                Freeze = true,
                Grenade = true,
                Shotgun = true,
                PulseGun = true,
                PersistentBeam = true,
                Boomerang = true,
                GasGrenade = true,

                //Traps available on profile creation
                Fire = true,
                Spikes = true,
                SawBlade = true,
                Wall = true,
                Ice = true,
                Barrel = true,
                Line = true,
                Trigger = true,
                Catapult = true,

                Credits = 0,

                ShotsFired = 0,

                UpgradesList = TempList2
            };

            StorageDevice.BeginShowSelector(this.NewProfile, null);

            NameInput.RealString = "";
            NameInput.TypePosition = 0;

            GameState = GameState.ProfileManagement;
        }
        #endregion

        #region Handle Settings
        public void LoadSettings()
        {
            if (!System.IO.Directory.Exists("Content\\Settings"))
                System.IO.Directory.CreateDirectory("Content\\Settings");

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));

            if (File.Exists("Content\\Settings\\Settings.xml"))
            {
                Stream stream = new FileStream("Content\\Settings\\Settings.xml", FileMode.Open);
                CurrentSettings = (Settings)serializer.Deserialize(stream);
                stream.Close();
                ApplySettings();
            }
            else
            {
                Stream stream = new FileStream("Content\\Settings\\Settings.xml", FileMode.Create);
                CurrentSettings = DefaultSettings;
                serializer.Serialize(stream, CurrentSettings);
                stream.Close();
                ApplySettings();
            }
        }

        public void DeleteSettings()
        {
            if (System.IO.Directory.Exists("Content\\Settings"))
                System.IO.Directory.Delete("Content\\Settings", true);
        }

        public void SaveSettings()
        {
            if (!System.IO.Directory.Exists("Content\\Settings"))
                System.IO.Directory.CreateDirectory("Content\\Settings");

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));

            if (File.Exists("Content\\Settings\\Settings.xml"))
            {
                Stream stream = new FileStream("Content\\Settings\\Settings.xml", FileMode.Create);
                serializer.Serialize(stream, CurrentSettings);
                stream.Close();
            }
        }

        public void ApplySettings()
        {
            //Apply the current settings to the game, making sure the changes are applied
            SoundEffect.MasterVolume = CurrentSettings.SFXVolume;
            MediaPlayer.Volume = CurrentSettings.MusicVolume;
        }
        #endregion

        #region Handle upgrades
        public Turret ApplyTurretUpgrades(TurretType turretType, int index)
        {
            //This takes the type of turret and the button index of the tower slot
            //creates a new instance of that turret, applies the upgrades to it and returns the 
            //final turret object with the upgrades applied
            Type turretObject = Type.GetType("TowerDefensePrototype." + turretType.ToString() + "Turret");
            
            object newTurret;

            if (index > -1)
            {
                newTurret = Activator.CreateInstance(turretObject, TowerButtonList[index].CurrentPosition); ;
            }
            else
            {
                newTurret = Activator.CreateInstance(turretObject, Vector2.Zero); ;
            }

            Turret UpgradedTurret = (Turret)newTurret;

            switch (UpgradedTurret.TurretType)
            {
                case TurretType.MachineGun:
                    UpgradedTurret.FireDelay = (int)PercentageChange(UpgradedTurret.FireDelay, -MachineGunTurretSpeed);
                    UpgradedTurret.Damage = (int)PercentageChange(UpgradedTurret.Damage, MachineGunTurretDamage);
                    UpgradedTurret.AngleOffset = (int)PercentageChange(UpgradedTurret.AngleOffset, -MachineGunTurretAccuracy);
                    break;

                case TurretType.Cannon:
                    UpgradedTurret.FireDelay = (int)PercentageChange(UpgradedTurret.FireDelay, -CannonTurretSpeed);
                    UpgradedTurret.Damage = (int)PercentageChange(UpgradedTurret.Damage, CannonTurretDamage);
                    UpgradedTurret.BlastRadius = (int)PercentageChange(UpgradedTurret.BlastRadius, CannonTurretBlastRadius);
                    break;
            }

            return UpgradedTurret;
        }

        //Apply upgrades, return upgraded trap
        public Trap ApplyTrapUpgrades(TrapType trapType, Vector2 trapPosition)
        {
            switch(trapType)
            {
                #region Wall
                case TrapType.Wall:
                    {
                        NewTrap = new Wall(trapPosition);
                    }
                    break;
                #endregion

                #region Fire
                case TrapType.Fire:
                    {
                        NewTrap = new FireTrap(trapPosition);
                    }
                    break;
                #endregion

                #region Spikes
                case TrapType.Spikes:
                    {
                        NewTrap = new SpikeTrap(trapPosition);
                    }
                    break;
                #endregion

                #region Catapult
                case TrapType.Catapult:
                    {
                        NewTrap = new CatapultTrap(trapPosition);
                    }
                    break;
                #endregion

                #region Ice
                case TrapType.Ice:
                    {
                        NewTrap = new IceTrap(trapPosition);
                    }
                    break;
                #endregion

                #region Barrel
                case TrapType.Barrel:
                    {
                        NewTrap = new BarrelTrap(trapPosition);
                    }
                    break;
                #endregion

                #region Sawblade
                case TrapType.SawBlade:
                    {
                        NewTrap = new SawBladeTrap(trapPosition);
                    }
                    break;
                #endregion

                #region Line
                case TrapType.Line:
                    {
                        NewTrap = new LineTrap(trapPosition);
                    }
                    break;
                #endregion
            }

            return NewTrap;
        }

        //Place upgraded trap, load textures, play sounds etc.
        public void PlaceUpgradedTrap(Trap trap, Vector2 trapPosition)
        {

            /* The following is called separately inside each case so that the Destination Rectangle can be known
                        
                        NewTrap.Position = trapPosition;
                        TrapList.Add(NewTrap);
                        NewTrap.Initialize();
              
               The following is then used to calculate the draw depth for the emitters
                        
                        (float)NewTrap.DestinationRectangle.Bottom / 1080
              
               The "(float)" (Or "1080f") is inserted otherwise it results in an integer number which is rounded to 0 which is useless
              
               There might be a better way to do this by setting up the destination rectangle in the specific trap initialiser
               i.e. in newTrap = new FireTrap(Position); 
             */

            switch (trap.TrapType)
            {
                #region Wall
                case TrapType.Wall:
                    {
                        NewTrap = trap;
                        NewTrap.TextureList = WallSprite;
                        NewTrap.CurrentTexture = NewTrap.TextureList[0];

                        NewTrap.Position = trapPosition;
                        TrapList.Add(NewTrap);
                        NewTrap.Initialize();
                    }
                    break;
                #endregion

                #region Fire
                case TrapType.Fire:
                    {
                        FireTrapStart.Play();

                        NewTrap = trap;
                        NewTrap.TextureList = FireTrapSprite;
                        NewTrap.CurrentTexture = NewTrap.TextureList[0];

                        NewTrap.Position = trapPosition;
                        TrapList.Add(NewTrap);
                        NewTrap.Initialize();

                        Color SmokeColor = Color.DarkGray;
                        SmokeColor.A = 200;

                        Color SmokeColor2 = Color.Gray;
                        SmokeColor.A = 175;

                        Emitter FireEmitter = new Emitter(FireParticle,
                            new Vector2(NewTrap.Position.X + NewTrap.TextureList[0].Width / 2, NewTrap.Position.Y + NewTrap.TextureList[0].Height - 12), new Vector2(60, 120),
                            new Vector2(0.5f, 0.75f), new Vector2(90, 140), 0.85f, true,
                            new Vector2(-4, 4), new Vector2(-1f, 1f), new Vector2(0.075f, 0.15f), FireColor, FireColor2,
                            0.0f, -1, 75, 1, false, new Vector2(0, 1080), true, NewTrap.DestinationRectangle.Bottom / 1080f, null, null, null, null, null, null, null, true, true, 150);
                        FireEmitter.TextureName = "FireParticle";

                        Emitter SmokeEmitter = new Emitter(SmokeParticle,
                            new Vector2(NewTrap.Position.X + NewTrap.TextureList[0].Width / 2, NewTrap.Position.Y + NewTrap.TextureList[0].Height - 16),
                            new Vector2(85, 95), new Vector2(0.2f, 0.5f), new Vector2(250, 350), 0.9f, true, new Vector2(-20, 20),
                            new Vector2(-2, 2), new Vector2(0.6f, 1f), SmokeColor, SmokeColor2, 0.0f, -1, 150, 1, false,
                            new Vector2(0, 1080), true, (NewTrap.DestinationRectangle.Bottom - 1) / 1080f, null, null, null, null, null, null, null, true, true, 250);
                        SmokeEmitter.TextureName = "SmokeParticle";

                        Emitter SparkEmitter = new Emitter(RoundSparkParticle,
                               new Vector2(NewTrap.Position.X + NewTrap.TextureList[0].Width / 2, NewTrap.Position.Y + NewTrap.TextureList[0].Height - 16),
                               new Vector2(80, 100),
                               new Vector2(1, 4), new Vector2(60, 180), 1f, true, new Vector2(0, 360), new Vector2(1, 3),
                               new Vector2(0.1f, 0.3f), Color.LightYellow, Color.White, -0.001f, -1f, 100, 1, false,
                               new Vector2(0, 1080), null, (NewTrap.DestinationRectangle.Bottom - 2) / 1080f);
                        SparkEmitter.TextureName = "SparkParticle";

                        NewTrap.TrapEmitterList.Add(SmokeEmitter);
                        NewTrap.TrapEmitterList.Add(FireEmitter);
                        NewTrap.TrapEmitterList.Add(SparkEmitter);
                    }
                    break;
                #endregion

                #region Spikes
                case TrapType.Spikes:
                    {
                        NewTrap = trap;
                        NewTrap.TextureList = SpikeTrapSprite;
                        NewTrap.CurrentTexture = NewTrap.TextureList[0];

                        NewTrap.Position = trapPosition;
                        TrapList.Add(NewTrap);
                        NewTrap.Initialize();
                    }
                    break;
                #endregion

                #region Catapult
                case TrapType.Catapult:
                    {
                        NewTrap = trap;
                        NewTrap.TextureList = CatapultTrapSprite;
                        NewTrap.CurrentTexture = NewTrap.TextureList[0];

                        NewTrap.Position = trapPosition;
                        TrapList.Add(NewTrap);
                        NewTrap.Initialize();
                    }
                    break;
                #endregion

                #region Ice
                case TrapType.Ice:
                    {
                        //NewTrap = trap;
                        //NewTrap.TextureList = IceTrapSprite;

                        //NewTrap.Position = trapPosition;
                        //TrapList.Add(NewTrap);
                        //NewTrap.Initialize();

                        NewTrap = trap;
                        NewTrap.TextureList = IceTrapSprite;
                        NewTrap.CurrentTexture = NewTrap.TextureList[0];

                        NewTrap.Position = trapPosition;
                        TrapList.Add(NewTrap);
                        NewTrap.Initialize();
                    }
                    break;
                #endregion

                #region Barrel
                case TrapType.Barrel:
                    {
                        NewTrap = trap;
                        NewTrap.TextureList = BarrelTrapSprite;
                        NewTrap.CurrentTexture = NewTrap.TextureList[0];

                        NewTrap.Position = trapPosition;
                        TrapList.Add(NewTrap);
                        NewTrap.Initialize();
                    }
                    break;
                #endregion

                #region Sawblade
                case TrapType.SawBlade:
                    {
                        NewTrap = trap;
                        NewTrap.TextureList = SawBladeTrapSprite;
                        NewTrap.CurrentTexture = NewTrap.TextureList[0];

                        NewTrap.Position = trapPosition;
                        TrapList.Add(NewTrap);
                        NewTrap.Initialize();
                    }
                    break;
                #endregion

                #region Line
                case TrapType.Line:
                    {
                        NewTrap = trap;
                        NewTrap.TextureList = LineTrapSprite;
                        NewTrap.CurrentTexture = NewTrap.TextureList[0];

                        NewTrap.Position = trapPosition;
                        TrapList.Add(NewTrap);
                        NewTrap.Initialize();

                        Emitter FireEmitter = new Emitter(FireParticle, new Vector2(NewTrap.Position.X, NewTrap.Position.Y),
                          new Vector2(0, 0), new Vector2(1.5f, 2.0f), new Vector2(40 * 1.5f, 60 * 1.5f), 0.01f, true, new Vector2(-4, 4),
                          new Vector2(-4, 4), new Vector2(1 * 1.5f, 2 * 1.5f), FireColor, FireColor2, 0.0f, -1, 25 * 1.5f, 3, false, new Vector2(0, 1080),
                          false, CursorPosition.Y / 1080);

                        NewTrap.TrapEmitterList.Add(FireEmitter);
                    }
                    break;
                #endregion
            }

            

            ReadyToPlace = false;
            PlaceTrap.Play();
            ClearSelected();

        }
        

        public void LoadUpgrades()
        {
            foreach (Upgrade upgrade in CurrentProfile.UpgradesList)
            {
                StackedUpgrade.GatlingSpeed += upgrade.GatlingSpeed;
                StackedUpgrade.GatlingDamage += upgrade.GatlingDamage;
                StackedUpgrade.GatlingAccuracy += upgrade.GatlingAccuracy;
            }

            MachineGunTurretSpeed += StackedUpgrade.GatlingSpeed;
            MachineGunTurretAccuracy += StackedUpgrade.GatlingAccuracy;
            MachineGunTurretDamage += StackedUpgrade.GatlingDamage;

        }

        public void ResetUpgrades()
        {
            StackedUpgrade = new StackedUpgrade();
            MachineGunTurretSpeed = 0;
            CannonTurretSpeed = 0;
            CannonTurretDamage = 0;
            CannonTurretBlastRadius = 0;
        }
        #endregion

        
        public void HandleWaves(GameTime gameTime)
        {
            //------------------------------
            //OVERFLOW means that the next wave starts without the button 
            //appearing, but only after all the current invaders are dead
            //------------------------------
            //NON-OVERFLOW means that the button appears and must be pressed 
            //before the next wave starts, but the current invaders must be dead
            //------------------------------
            //PAUSE means that there's a 6 second break in the middle of a wave and 
            //after those 6 seconds, new invaders are added regardless of
            //whether the previous ones are dead or not.
            //------------------------------

            if (GameState == GameState.Playing && IsLoading == false)
            {
                CurrentWave = CurrentLevel.WaveList[CurrentWaveIndex];

                #region Use the "Start Waves" button to begin the next wave
                if (StartWaveButton != null)
                {
                    StartWaveButton.Update(CursorPosition, gameTime);

                    if (StartWaveButton.JustClicked == true)
                    {
                        StartWave = true;
                        CurrentWaveTime = 0;
                        CurrentInvaderIndex = 0;
                        CurrentInvaderTime = 0;

                        if (CurrentWaveIndex > 0)
                            CurrentWaveIndex++;

                        StartWaveButton = null;
                    }
                }
                #endregion

                #region Add to the time between Waves
                if (CurrentWaveTime < CurrentWave.TimeToNextWave)
                {
                    if (StartWave == true)
                    {
                        CurrentWaveTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    }
                }
                #endregion

                if (CurrentWaveTime >= CurrentWave.TimeToNextWave)
                {
                    #region Add to the time between Invaders
                    if (CurrentInvaderTime < CurrentWave.TimeToNextInvader)
                        CurrentInvaderTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    #endregion

                    if (CurrentInvaderTime >= CurrentWave.TimeToNextInvader)
                    {
                        #region If there are still invaders to be added
                        if (CurrentInvaderIndex < CurrentWave.InvaderList.Count)
                        {
                            #region If the element is an invader
                            if (CurrentWave.InvaderList[CurrentInvaderIndex] is Invader)
                            {
                                Invader nextInvader = (Invader)CurrentWave.InvaderList[CurrentInvaderIndex];

                                switch (nextInvader.InvaderType)
                                {
                                    default:
                                        string invaderName = nextInvader.InvaderType.ToString();
                                        var invaderSpriteList = (List<Texture2D>)this.GetType().GetField(invaderName + "Sprite").GetValue(this);
                                        nextInvader.TextureList = invaderSpriteList;
                                        break;
                                }

                                nextInvader.IceBlock = IceBlock;
                                nextInvader.Shadow = Shadow;


                                nextInvader.Direction.X *= (float)RandomDouble(0.75f, 1.5f);
                                //nextInvader.CurrentMoveVector = nextInvader.Direction;

                                //foreach (Invader CurrentInvader in InvaderList)
                                //{
                                //    if (nextInvader.MaxY == CurrentInvader.MaxY)
                                //    {
                                //        nextInvader.MaxY += 2;
                                //    }
                                //}

                                if (InvaderList.Find(Invader => Invader.MaxY == nextInvader.MaxY) != null)
                                {
                                    nextInvader.MaxY += 2;
                                }
                                                                
                                nextInvader.Initialize();
                                nextInvader.Update(gameTime, CursorPosition);
                                nextInvader.InvaderOutline = new UIOutline(nextInvader.Position, new Vector2(nextInvader.FrameSize.X, nextInvader.FrameSize.Y), null, null, nextInvader);
                                nextInvader.InvaderOutline.OutlineTexture = TurretSelectBox;
                                InvaderList.Add(nextInvader);
                                CurrentInvaderTime = 0;
                                CurrentWave.InvaderList[CurrentInvaderIndex] = null;
                                CurrentInvaderIndex++;
                                return;
                            }
                            #endregion

                            #region If the element is a pause
                            if (CurrentWave.InvaderList[CurrentInvaderIndex] is int)
                            {
                                if (CurrentWavePauseTime < (int)CurrentWave.InvaderList[CurrentInvaderIndex])
                                {
                                    CurrentWavePauseTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                                }
                                else
                                {
                                    CurrentInvaderIndex++;
                                    CurrentInvaderTime = 0;
                                    CurrentWavePauseTime = 0;
                                    return;
                                }
                            }
                            #endregion

                            #region This is used to change the delay between invaders at any interval while the wave is being used
                            //This can be used to reduce the delay between when invaders are added. Speed up the adding process without having
                            //to create a whole new waves with a different timing value
                            if (CurrentWave.InvaderList[CurrentInvaderIndex] is float)
                            {
                                CurrentWave.TimeToNextInvader = (float)CurrentWave.InvaderList[CurrentInvaderIndex];
                                CurrentInvaderIndex++;
                                CurrentInvaderTime = 0;
                                CurrentWavePauseTime = 0;
                                return;
                            }
                            #endregion
                        }
                        #endregion
                        else
                        #region If there are no more invaders to be added from this wave
                        {
                            if (CurrentWave.Overflow == true)
                            {
                                if (InvaderList.Count == 0)
                                {
                                    CurrentWaveTime = 0;
                                    CurrentInvaderIndex = 0;
                                    CurrentInvaderTime = 0;
                                    CurrentWaveIndex++;

                                    ////Victory conditions
                                    //if (CurrentWaveIndex > CurrentLevel.WaveList.Count - 1 && 
                                    //    InvaderList.Count == 0)
                                    //{
                                    //    Victory = true;
                                    //    GameState = GameState.Victory;
                                    //}
                                }
                            }
                            else
                            {
                                //ADD THE START WAVE BUTTON HERE
                                if (StartWave == true && StartWaveButton == null && InvaderList.Count == 0)
                                {
                                    StartWaveButton = new Button(ButtonLeftSprite, new Vector2(1000, 200));
                                    StartWaveButton.LoadContent();
                                    StartWave = false;
                                }
                            }
                        }
                        #endregion
                    }
                }

                #region Handle victory conditions
                if (InvaderList.Count == 0 && 
                    CurrentWaveIndex > CurrentLevel.WaveList.Count - 1)
                {
                    Victory = true;
                    VictoryContinue = new Button(ButtonLeftSprite, new Vector2(-300, 834), null, null, null, "continue", RobotoRegular20_2, "Left", null, false);
                    VictoryContinue.NextPosition = new Vector2(665, 834);
                    VictoryContinue.LoadContent();
                    GameState = GameState.Victory;
                }
                #endregion
            }
        }

        public void LoadLevel(int number)
        {
            //CurrentLevel = Content.Load<Level>("Levels/Level" + number);
            //var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

            Assembly assembly = Assembly.Load("TowerDefensePrototype");
            Type t = assembly.GetType("TowerDefensePrototype.Level" + number);
            CurrentLevel = (Level)Activator.CreateInstance(t);

            //Load the story dialogue used for this level here
            StoryDialogue = Content.Load<StoryDialogue>("StoryDialogue/StoryDialogue" + number);

            //Load the resources used for this specific level (Can't be generalised) i.e. backgroung textures
            CurrentLevel.LoadContent(Content);
            CurrentWeather = CurrentLevel.StartWeather;

            #region Use those resources to create the background and foreground sprites
            Ground = new StaticSprite(CurrentLevel.GroundTexture, new Vector2(0, 1080 - 500 + 70));
            Ground.DrawDepth = 1.0f;
            Ground.BoundingBox = new BoundingBox(new Vector3(0, MathHelper.Clamp(CursorPosition.Y, 690, 1080), 0), new Vector3(1920, MathHelper.Clamp(CursorPosition.Y + 1, 800, 1080), 0));

            ForeGround = new StaticSprite(CurrentLevel.ForegroundTexture, new Vector2(0, 1080 - 400));
            ForeGround.DrawDepth = 1;

            SkyBackground = new StaticSprite(CurrentLevel.SkyBackgroundTexture, new Vector2(0, 0));
            #endregion

            CurrentWaveIndex = 0;
            CurrentInvaderIndex = 0;

            CurrentWaveTime = 0;
            CurrentInvaderTime = 0;

            CurrentWave = CurrentLevel.WaveList[0];

            StartWave = false;

            Resources = CurrentLevel.Resources;
        }

        //Draw the correct cursor
        private void CursorDraw(SpriteBatch spriteBatch)
        {
            if (GameState != GameState.Playing)
            {
                CurrentCursorTexture = BlankTexture;
                PrimaryCursorTexture = DefaultCursor;
            }

            if (GameState == GameState.Playing || GameState == GameState.ProfileManagement)
            {
                if (GameState == GameState.Playing)
                {
                    if (TurretList.Any(Turret => Turret != null && Turret.Selected == true))
                    {
                        PrimaryCursorTexture = CrosshairCursor;
                    }
                    else
                    {
                        PrimaryCursorTexture = DefaultCursor;
                    }
                }

                switch (SelectedTrap)
                {
                    case null:
                        if (SelectedTurret == null)
                        {
                            CurrentCursorTexture = BlankTexture;
                        }
                        break;

                    default:
                        {
                            string CursorName = SelectedTrap.ToString() + "TrapCursor";
                            var Cursor = this.GetType().GetField(CursorName).GetValue(this);
                            CurrentCursorTexture = (Texture2D)Cursor;
                        }
                        break;
                }

                switch (SelectedTurret)
                {
                    case null:
                        if (SelectedTrap == null)
                        {
                            CurrentCursorTexture = BlankTexture;
                        }
                        break;

                    default:
                        {
                            string CursorName = SelectedTurret.ToString() + "TurretCursor";
                            var Cursor = this.GetType().GetField(CursorName).GetValue(this);
                            CurrentCursorTexture = (Texture2D)Cursor;
                        }
                        break;
                }
            }


            if (GameState != GameState.Loading)
            {
                if (GameState != GameState.Playing)
                {
                    spriteBatch.Draw(CurrentCursorTexture, new Rectangle((int)CursorPosition.X - (CurrentCursorTexture.Width / 2),
                                    (int)CursorPosition.Y - CurrentCursorTexture.Height, CurrentCursorTexture.Width,
                                    CurrentCursorTexture.Height), null, CursorColor, MathHelper.ToRadians(0), Vector2.Zero,
                                    SpriteEffects.None, 1);

                    spriteBatch.Draw(PrimaryCursorTexture, new Rectangle((int)CursorPosition.X, (int)CursorPosition.Y,
                                     PrimaryCursorTexture.Width, PrimaryCursorTexture.Height), null,
                                     CursorColor, MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, 1);
                }
                else
                {
                    if (SelectedTrap != null)
                    {
                        spriteBatch.Draw(CurrentCursorTexture, new Rectangle((int)(CursorPosition.X - CursorPosition.X % 32),
                                        (int)(CursorPosition.Y - CursorPosition.Y % 32), CurrentCursorTexture.Width, CurrentCursorTexture.Height),
                                        null, CursorColor, MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, 1);
                    }
                    else
                    {
                        spriteBatch.Draw(CurrentCursorTexture, new Rectangle((int)(CursorPosition.X), (int)(CursorPosition.Y),
                                    CurrentCursorTexture.Width, CurrentCursorTexture.Height),
                                    null, CursorColor, MathHelper.ToRadians(0), new Vector2(CurrentCursorTexture.Width / 2, CurrentCursorTexture.Height), SpriteEffects.None, 1);
                    }


                    if (PrimaryCursorTexture != CrosshairCursor)
                    {
                        spriteBatch.Draw(PrimaryCursorTexture, new Rectangle((int)CursorPosition.X, (int)CursorPosition.Y,
                                         PrimaryCursorTexture.Width, PrimaryCursorTexture.Height), null, Color.White,
                                         MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, 1);
                    }
                    else
                    {
                        spriteBatch.Draw(PrimaryCursorTexture, new Rectangle((int)CursorPosition.X - PrimaryCursorTexture.Width / 2,
                                         (int)CursorPosition.Y - PrimaryCursorTexture.Height / 2, PrimaryCursorTexture.Width,
                                         PrimaryCursorTexture.Height), null, Color.White, MathHelper.ToRadians(0), Vector2.Zero,
                                         SpriteEffects.None, 1);
                    }
                }
            }
        }
        

        #region Some cool functions
        public double RandomDouble(double a, double b)
        {
            return a + Random.NextDouble() * (b - a);
        }

        public double PercentageChange(double number, double percentage) //Changes a number by a percentage. i.e. PercentageChange(100, 25) returns 125
        {
            double newNumber = number;
            newNumber = newNumber + ((newNumber / 100) * percentage);
            return newNumber;
        }

        public Vector2 PointToVector(Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        public Point VectorToPoint(Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }

        private bool RandomBool()
        {
            if ((float)Random.NextDouble() > 0.5f)
                return true;
            else
                return false;
        }

        private Color RandomColor(params Color[] colors)
        {
            List<Color> ColorList = colors.ToList();

            return ColorList[Random.Next(0, ColorList.Count)];
        }

        private Texture2D RandomTexture(params Texture2D[] textures)
        {
            List<Texture2D> TextureList = textures.ToList();

            return TextureList[Random.Next(0, TextureList.Count)];
        }

        private SpriteEffects RandomOrientation(params SpriteEffects[] Orientations)
        {
            List<SpriteEffects> OrientationList = new List<SpriteEffects>();

            foreach (SpriteEffects orientation in Orientations)
            {
                OrientationList.Add(orientation);
            }

            return OrientationList[Random.Next(0, OrientationList.Count)];
        }
        #endregion


        #region Various functions to clear current selections
        private void ClearTurretSelect()
        {
            //This forces all turrets to become un-selected.
            foreach (Turret turret in TurretList)
            {
                if (turret != null)
                    turret.Selected = false;
            }
        }

        private void RightClickClearSelected()
        {
            if (CurrentMouseState.RightButton == ButtonState.Released && 
                PreviousMouseState.RightButton == ButtonState.Pressed)
            {
                SelectedTurret = null;
                SelectedTrap = null;
                CurrentTurret = null;

                if (GameState == GameState.Playing)
                {
                    ReadyToPlace = false;

                    foreach (Turret turret in TurretList)
                    {
                        if (turret != null)
                            turret.Selected = false;
                    }
                }
            }
        }

        private void ClearSelected()
        {
            CurrentTurret = null;
            SelectedTurret = null;
            SelectedTrap = null;
            ReadyToPlace = false;
        }
        #endregion


        private Texture2D GetPowerupIcon(Powerup powerup)
        {
            if (powerup.CurrentEffect.GeneralPowerup != null)
            switch (powerup.CurrentEffect.GeneralPowerup)
            {
                case GeneralPowerup.BlastRadii:
                    return KineticDamageIcon;

                case GeneralPowerup.ExplosivePower:
                    return KineticDamageIcon;

                case GeneralPowerup.RepairTower:
                    return KineticDamageIcon;
            }

            if (powerup.CurrentEffect.TowerPowerup != null)
            switch (powerup.CurrentEffect.TowerPowerup)
            {
                case null:
                    break;
            }

            if (powerup.CurrentEffect.TrapPowerup != null)
            switch (powerup.CurrentEffect.TrapPowerup)
            {
                case null:
                    break;
            }

            if (powerup.CurrentEffect.TurretPowerup != null)
            switch (powerup.CurrentEffect.TurretPowerup)
            {
                case null:
                    break;
            }


            return WhiteBlock;
        }

        private void HandlePlacedIcons()
        {
            //foreach (Button button in PlaceWeaponList)
            //{
            //    button.IconTexture = null;
            //}

            foreach (Button button in PlaceWeaponList)
            {
                button.IconTexture = null;

                int Index = PlaceWeaponList.IndexOf(button);
                Button PressedButton = PlaceWeaponList[Index];

                //Change the icon of the placed weapon button when a weapon is placed into a slot - TURRETS
                if (CurrentProfile.Buttons[Index] != null)
                {
                    switch (CurrentProfile.Buttons[Index].CurrentTurret)
                    {
                        case null:

                            break;

                        default:
                            {
                                string WeaponName = CurrentProfile.Buttons[Index].CurrentTurret.ToString();
                                var icon = this.GetType().GetField(WeaponName + "TurretIcon").GetValue(this);
                                PressedButton.IconTexture = (Texture2D)icon;
                            }
                            break;
                    }
                }


                //Change the icon of the placed weapon button when a weapon is placed into a slot - TRAPS
                if (CurrentProfile.Buttons[Index] != null)
                {
                    switch (CurrentProfile.Buttons[Index].CurrentTrap)
                    {
                        case null:

                            break;

                        default:
                            {
                                string WeaponName = CurrentProfile.Buttons[Index].CurrentTrap.ToString();
                                var icon = this.GetType().GetField(WeaponName + "TrapIcon").GetValue(this);
                                PressedButton.IconTexture = (Texture2D)icon;
                            }
                            break;
                    }
                }
            }
        }

        private void HandleSelectIcons()
        {
            foreach (WeaponBox turretBox in SelectTurretList)
            {
                switch (turretBox.ContainsTurret)
                {
                    case null:

                        break;

                    default:
                        string WeaponName = turretBox.ContainsTurret.ToString();
                        var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

                        if ((bool)Available == true)
                        {
                            turretBox.Locked = false;
                        }
                        else
                        {
                            turretBox.Locked = true;
                            turretBox.WeaponName = "Locked";
                        }
                        break;
                }
            }

            foreach (WeaponBox trapBox in SelectTrapList)
            {
                switch (trapBox.ContainsTrap)
                {
                    case null:

                        break;

                    default:
                        string WeaponName = trapBox.ContainsTrap.ToString();
                        var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

                        if ((bool)Available == true)
                        {
                            trapBox.Locked = false;
                        }
                        else
                        {
                            trapBox.Locked = true;
                            trapBox.WeaponName = "Locked";
                        }
                        break;
                }
            }
        }

        private void SetUpWeaponSelection()
        {
            #region Set up the turret and trap select boxes for the profile management screen
            #region Turret boxes
            SelectTurretList = new List<WeaponBox>();
            var turretTypes = Enum.GetValues(typeof(TurretType));

            for (int i = 0; i < turretTypes.Length; i++)
            {
                WeaponBox turretBox = new WeaponBox(new Vector2(158 + 113 + (i * 282), 500 - 32), ApplyTurretUpgrades((TurretType)(i), -1), null);

                turretBox.ContainsTurret = (TurretType)turretTypes.GetValue(i);

                turretBox.CurrencyIcon = CurrencyIcon;
                turretBox.RobotoBold40_2 = RobotoBold40_2;
                turretBox.RobotoRegular20_0 = RobotoRegular20_0;
                turretBox.RobotoRegular20_2 = RobotoRegular20_2;
                turretBox.RobotoItalic20_0 = RobotoItalic20_0;
                turretBox.LoadContent(SecondaryContent);

                turretBox.Visible = false;
                SelectTurretList.Add(turretBox);
            }
            #endregion

            #region Trap boxes
            SelectTrapList = new List<WeaponBox>();
            var trapTypes = Enum.GetValues(typeof(TrapType));

            for (int i = 0; i < trapTypes.Length; i++)
            {
                WeaponBox trapBox = new WeaponBox(new Vector2(158 + 113 + (i * 282), 902 - 32), null, ApplyTrapUpgrades((TrapType)(i), Vector2.Zero));

                trapBox.ContainsTrap = (TrapType)trapTypes.GetValue(i);

                trapBox.CurrencyIcon = CurrencyIcon;
                trapBox.RobotoBold40_2 = RobotoBold40_2;
                trapBox.RobotoRegular20_0 = RobotoRegular20_0;
                trapBox.RobotoRegular20_2 = RobotoRegular20_2;
                trapBox.RobotoItalic20_0 = RobotoItalic20_0;
                trapBox.LoadContent(SecondaryContent);

                trapBox.Visible = false;
                SelectTrapList.Add(trapBox);
            }
            #endregion
            #endregion
        }


        private int TurretCost(TurretType turretType)
        {
            int cost = 0;

            switch (turretType)
            {
                case TurretType.MachineGun:
                    cost = new MachineGunTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.Cannon:
                    cost = new CannonTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.FlameThrower:
                    cost = new FlameThrowerTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.Lightning:
                    cost = new LightningTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.Cluster:
                    cost = new ClusterTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.FelCannon:
                    cost = new FelCannonTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.Beam:
                    cost = new BeamTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.Freeze:
                    cost = new FreezeTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.Boomerang:
                    cost = new BoomerangTurret(Vector2.Zero).ResourceCost;
                    break;

                case TurretType.Grenade:
                    cost = new GrenadeTurret(Vector2.Zero).ResourceCost;
                    break;
            }

            return cost;
        }
        private int TrapCost(TrapType trapType)
        {
            int cost = 0;

            switch (trapType)
            {
                case TrapType.Fire:
                    cost = new FireTrap(Vector2.Zero).ResourceCost;
                    break;

                case TrapType.Catapult:
                    cost = new CatapultTrap(Vector2.Zero).ResourceCost;
                    break;

                case TrapType.Ice:
                    cost = new IceTrap(Vector2.Zero).ResourceCost;
                    break;

                case TrapType.SawBlade:
                    cost = new SawBladeTrap(Vector2.Zero).ResourceCost;
                    break;

                case TrapType.Spikes:
                    cost = new SpikeTrap(Vector2.Zero).ResourceCost;
                    break;

                case TrapType.Wall:
                    cost = new Wall(Vector2.Zero).ResourceCost;
                    break;

                case TrapType.Barrel:
                    cost = new BarrelTrap(Vector2.Zero).ResourceCost;
                    break;
            }

            return cost;
        }

        public Texture2D HandleShaders(Texture2D screenTexture, params EffectPass[] effects)
        {
            Rectangle DestRect = new Rectangle(0, 0, 1920, 1080);

            //Draw the screen texture to Target1
            GraphicsDevice.SetRenderTarget(ShaderTarget1);
            GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.Draw(screenTexture, DestRect, Color.White);
            spriteBatch.End();

            //Apply each of the effects. They stack up on top of each other.
            for (int i = 0; i < effects.Count(Effect => Effect != null); i++)
            {
                //Render Target1 to Target2 with the current effect (i)
                GraphicsDevice.SetRenderTarget(ShaderTarget2);
                GraphicsDevice.Clear(Color.Transparent);
                try
                {
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                    effects[i].Apply();
                    spriteBatch.Draw(ShaderTarget1, DestRect, Color.White);
                    spriteBatch.End();
                }
                catch (Exception e)
                {
                    Error = e.ToString();
                }

                //Render Target2 back to Target1 so that the next effect can be stacked
                GraphicsDevice.SetRenderTarget(ShaderTarget1);
                GraphicsDevice.Clear(Color.Transparent);

                spriteBatch.Begin();
                spriteBatch.Draw(ShaderTarget2, DestRect, Color.White);
                spriteBatch.End();
            }

            return ShaderTarget1;
        }

        public void UnlockWeapons()
        {
            switch (CurrentProfile.LevelNumber)
            {
                case 1:

                    break;

                case 2:
                    CurrentProfile.Cannon = true;
                    break;
            }

            //Save the profile data here
        }

        public void ApplyPowerups()
        {
            foreach (Powerup powerup in PowerupsList)
            {
                #region General powerups
                switch (powerup.CurrentEffect.GeneralPowerup)
                {
                    case GeneralPowerup.ExplosivePower:
                        foreach (Explosion explosion in ExplosionList)
                        {
                            explosion.Damage = (float)PercentageChange(explosion.Damage, powerup.CurrentEffect.PowerupValue);
                        }
                        break;

                    case GeneralPowerup.BlastRadii:
                        foreach (Explosion explosion in ExplosionList)
                        {
                            explosion.BlastRadius = (float)PercentageChange(explosion.BlastRadius, powerup.CurrentEffect.PowerupValue);
                        }
                        break;
                }
                #endregion

                #region Tower powerups
                switch (powerup.CurrentEffect.TowerPowerup)
                {
                    case TowerPowerup.InsideShield:

                        break;
                }
                #endregion

                #region Turret powerups
                switch (powerup.CurrentEffect.TurretPowerup)
                {
                    case TurretPowerup.ArcSight:

                        break;
                }
                #endregion

                #region Trap powerups
                switch (powerup.CurrentEffect.TrapPowerup)
                {
                    case TrapPowerup.RepairTraps:

                        break;
                }
                #endregion
            }
        }
    }
}