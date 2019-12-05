using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;
using System.Diagnostics;
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
    #region Enums and structs

    #region Game control states
    public enum GameState { Menu, Loading, Playing, Paused, ProfileSelect, Options, ProfileManagement, GettingName, Victory };
    public enum ProfileState { Standard, Upgrades, Stats };
    public enum ProfileManagementState { Loadout, Upgrades, Stats };
    #endregion

    #region Button states
    public enum MousePosition { Inside, Outside };
    public enum ButtonSpriteState { Released, Hover, Pressed };
    #endregion

    #region Button Click
    //Using a non-generic delegate so that I can pass it to all the buttons when they're initialised
    public delegate void ButtonClickHappenedEventHandler(object source, ButtonClickEventArgs e);
    public enum MouseButton { Left, Right, Middle };
    public class ButtonClickEventArgs : EventArgs
    {
        public MouseButton ClickedButton { get; set; }
    }
    #endregion

    #region Powerup Types
    public enum GeneralPowerup { ExplosivePower, BlastRadii, RepairTower };
    public enum TurretPowerup { DoubleShot, SlowInvaders, ArcSight };
    public enum TrapPowerup { RepairTraps };
    public enum TowerPowerup { ShieldCooldown, InsideShield, InvulnerableRanged };

    //This should possibly be moved into a base class which the powerups inherit from
    public struct PowerupEffect
    {
        public float PowerupValue;
        public Nullable<GeneralPowerup> GeneralPowerup;
        public Nullable<TurretPowerup> TurretPowerup;
        public Nullable<TrapPowerup> TrapPowerup;
        public Nullable<TowerPowerup> TowerPowerup;
    };
    #endregion

    public enum DamageType { Fire, Electric, Concussive, Kinetic, Radiation };
        
    public enum Weather { Snow };
    public enum WorldType { Snowy };

    public struct StackedUpgrade
    {
        public float GatlingSpeed, GatlingDamage, GatlingAccuracy;
        public float CannonSpeed, CannonDamage, CannonBlastRadius;
    };

    public class RangedDamageStruct
    {
        public InvaderFireType FireType; //Whether the invader fires a single projectile, fires a burst or fires a beam etc.
        public Vector2 DistanceRange; //How far away from the tower the invader will be before stopping to fire
        public Vector2 AngleRange; //The angle that the projectile is fired at.
        public Vector2 LaunchVelocityRange; //The range of speeds that the invader can use to launch a heavy projectile

        public bool InTowerRange = false;
        public bool InTrapRange = false;
        public float DistToTower = 1920;
        public float DistToTrap, TrapPosition;
        public float MinTowerRange, MinTrapRange;

        public float RangedDamage; //How much damage the projectile does
        public float LaunchVelocity; //How fast the heavy projectile is travelling when launched
        public float CurrentFireDelay, MaxFireDelay; //How many milliseconds between shots
        public int CurrentBurstShots, MaxBurstShots; //How many shots are fired in a row before a longer recharge is needed
    }
    #endregion
    
    public class Game1 : Game
    {
        #region Events
        #region For events

        #region Explosion
        public EventHandler<ExplosionEventArgs> ExplosionHappenedEvent;
        public class ExplosionEventArgs : EventArgs
        {
            public Explosion Explosion { get; set; }
        }
        protected virtual void CreateExplosion(Explosion explosion, object source)
        {
            if (ExplosionHappenedEvent != null)
                OnExplosionHappened(source, new ExplosionEventArgs() { Explosion = explosion });
        }
        #endregion

        #region Trap collision
        public EventHandler<TrapCollisionEventArgs> TrapCollisionEvent;
        public class TrapCollisionEventArgs : EventArgs
        {
            public Trap Trap { get; set; }
            public Invader Invader { get; set; }
        }
        protected virtual void CreateCollision(Trap trap, Invader invader)
        {
            if (TrapCollisionEvent != null)
                OnTrapCollision(this, new TrapCollisionEventArgs() { Invader = invader, Trap = trap });
        }
        #endregion

        #region Light Projectile Fired
        public EventHandler<LightProjectileEventArgs> LightProjectileFiredEvent;
        public class LightProjectileEventArgs : EventArgs
        {
            public LightProjectile Projectile { get; set; }
        }
        protected virtual void CreateLightProjectile(LightProjectile lightProjectile, object source)
        {
            if (LightProjectileFiredEvent != null)
            {
                OnLightProjectileFired(source, new LightProjectileEventArgs() { Projectile = lightProjectile });
                //Debug.WriteLine("Source: " + source.ToString() + " Type: " + lightProjectile.LightProjectileType.ToString(), "Projectile");
            }
        }
        #endregion

        #region Heavy Projectile Collision
        public EventHandler<HeavyProjectileEventArgs> HeavyProjectileCollisionEvent;
        public class HeavyProjectileEventArgs : EventArgs
        {
            public HeavyProjectile Projectile { get; set; }
            public object collisionObject { get; set; }
        }
        protected virtual void CreateHeavyProjectileCollision(HeavyProjectile HeavyProjectile, object source, object collision)
        {
            if (HeavyProjectileCollisionEvent != null)
            {
                OnHeavyProjectileCollision(source, new HeavyProjectileEventArgs() { Projectile = HeavyProjectile, collisionObject = collision });
                //Debug.WriteLine("Source: " + source.ToString() + " Type: " + HeavyProjectile.HeavyProjectileType.ToString(), "Projectile");
            }
        }
        #endregion

        #region Turret Shoot
        public EventHandler<TurretShootEventArgs> TurretShootEvent;
        public class TurretShootEventArgs : EventArgs
        {
            public Turret Turret { get; set; }
        }
        protected virtual void CreateTurretShoot(Turret turret)
        {
            if (TurretShootEvent != null)
                OnTurretShoot(this, new TurretShootEventArgs() { Turret = turret });
        }
        #endregion

        #region Right-click
        public EventHandler<TurretShootEventArgs> RightClickEvent;
        protected virtual void CreateRightClick()
        {
            if (RightClickEvent != null)
                OnRightClick(this, null);
        }
        #endregion

        #region Trap Placement
        public EventHandler<TrapCollisionEventArgs> TrapPlacementEvent;
        public class TrapPlacementEventArgs : EventArgs
        {
            public TrapType Trap { get; set; }
        }
        protected virtual void CreateTrapPlacement(TrapType trap)
        {
            if (TrapPlacementEvent != null)
                OnTrapPlacement(this, new TrapPlacementEventArgs() { Trap = trap });
        }
        #endregion
        #endregion

        protected override void OnExiting(object sender, EventArgs args)
        {
            Content.Unload();
            SecondaryContent.Unload();
            base.OnExiting(sender, args);
        }

        public void OnRightClick(object source, EventArgs e)
        {
            //Clear the selections when the right mouse button is clicked
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
        #endregion

        #region Variable declarations
        GameTime CurrentGameTime;
        GraphicsDeviceManager graphics;
        ContentManager SecondaryContent;
        SpriteBatch spriteBatch, targetBatch;
        RenderTarget2D GameRenderTarget, MenuRenderTarget, UIRenderTarget;
        RenderTarget2D ShaderTarget1, ShaderTarget2;
        RenderTarget2D LightColorMap, LightNormalMap, LightShadowMap, DepthMap, EmissiveMap;

        Rectangle ScreenDrawRectangle;

        private GameState _GameState;
        public GameState GameState
        {
            get { return _GameState; }
            set
            {
                _GameState = value;
            }
        }

        private DialogBox _CurrentDialogBox;
        public DialogBox CurrentDialogBox
        {
            get { return _CurrentDialogBox; }
            set
            {
                _CurrentDialogBox = value;
                DialogBox dBox = value as DialogBox;
            }
        }

        #region XNA Declarations
        static Random Random = new Random();
        BinaryFormatter formatter = new BinaryFormatter();

        //Sprites
        #region Decal sprites
        Texture2D BloodDecal1, ExplosionDecal1;
        #endregion

        #region Particle sprites
        Texture2D BlankTexture, ShellCasing, LightningShellCasing, Coin, RoundSparkParticle, HealthParticle,
                  HealthBarSprite, SplodgeParticle, SmokeParticle, FireParticle, FireParticle2, ExplosionParticle,
                  ExplosionParticle2, BallParticle, SparkParticle, BulletTrailCap, BulletTrailSegment, BlurrySnowflake,
                  FocusedSnowflake, MachineBullet;
        #endregion

        #region Icon sprites
        Texture2D LockIcon, HealthIcon, OverHeatIcon, CurrencyIcon, PowerUnitIcon;

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
        public List<TrapAnimation> WallAnimations, BarrelTrapAnimations, CatapultTrapAnimations, IceTrapAnimations, TarTrapAnimations,
                            LineTrapAnimations, SawBladeTrapAnimations, SpikeTrapAnimations, FireTrapAnimations;
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
        public Texture2D FreezeTurretBase, FreezeTurretBarrel;
        public Texture2D BoomerangTurretBase, BoomerangTurretBarrel;
        #endregion

        #region Enemy sprites
        Texture2D IceBlock, Shadow;
        public List<InvaderAnimation> SoldierAnimations, BatteringRamAnimations, AirshipAnimations, ArcherAnimations,
                               TankAnimations, SpiderAnimations, SlimeAnimations, SuicideBomberAnimations,
                               FireElementalAnimations, TestInvaderAnimations, StationaryCannonAnimations,
                               HealDroneAnimations, JumpManAnimations, RifleManAnimations;

        public InvaderAnimation StationaryCannonBarrelAnimation;
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
        public Color ExplosionColor3 = Color.Lerp(Color.Red, new Color(255, Color.DarkOrange.G, Color.DarkOrange.B, 50), 0.5f);

        public Color DirtColor = new Color(51, 31, 0, 100);
        public Color DirtColor2 = new Color(51, 31, 0, 125);

        public Color SmokeColor1 = Color.Lerp(Color.DarkGray, Color.Transparent, 0.1f);
        public Color SmokeColor2 = Color.Lerp(Color.Gray, Color.Transparent, 0.1f);

        //THESE WERE FOR THE FIRE TRAP SMOKE EMITTER

        //Color SmokeColor = Color.DarkGray;
        //SmokeColor.A = 200;

        //Color SmokeColor2 = Color.Gray;
        //SmokeColor.A = 165;


        //public Color LightFlashColor = Color.Lerp(Color.LemonChiffon, Color.Transparent, 0.5f);
        public Color LightFlashColor = Color.Lerp(Color.Lerp(Color.LightGoldenrodYellow, Color.OrangeRed, 0.8f), Color.Transparent, 0.5f);

        public Color FireLightColor = Color.Lerp(Color.Lerp(Color.LightGoldenrodYellow, Color.OrangeRed, 0.6f), Color.Transparent, 0.5f);
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

        Texture2D TerrainShrub1;
        Texture2D PowerupDeliveryPod, PowerupDeliveryFin;
        Texture2D MysteryEmployerTexture;

        Vector2 CursorPosition, ActualResolution;
        Rectangle ScreenRectangle;

        MouseState CurrentMouseState, PreviousMouseState;
        KeyboardState CurrentKeyboardState, PreviousKeyboardState;

        private ButtonState _LeftButtonState;
        public ButtonState LeftButtonState
        {
            get { return _LeftButtonState; }
            set 
            { 
                _LeftButtonState = value;
                if (value == ButtonState.Released)
                {
                    if (GameState == GameState.Playing)
                    {
                        if (CurrentBeam != null)
                        {
                            CurrentTurret.ElapsedTime = 0;
                            CurrentBeam = null;
                            foreach (Invader invader in InvaderList.Where(Invader => Invader.HitByBeam == true))
                            {
                                invader.HitByBeam = false;
                            }
                        }
                    }
                }
            }
        }

        private ButtonState _RightButtonState;
        public ButtonState RightButtonState
        {
            get { return _RightButtonState; }
            set { _RightButtonState = value; }
        }


        int Resources, TowerButtons, ProfileNumber, LevelNumber, limit, TotalParticles;
        int CurrentWaveIndex = 0;
        int CurrentInvaderIndex = 0;
        int MaxWaves = 0;

        string FileName, ContainerName, ProfileName;

        bool ReadyToPlace, IsLoading, AllLoaded, Slow;
        bool DialogVisible = false;
        bool Diagnostics = false;
        bool BoundingBoxes = false;
        bool Tutorial = false;
        bool StartWave = false;
        bool Victory = false;

        float MenuSFXVolume, MenuMusicVolume, VictoryTime, CurrentInvaderTime, CurrentWaveTime, CurrentWavePauseTime;

        double Seconds, ResolutionOffsetRatio;

        Effect HealthBarEffect, ShockWaveEffect, ShieldBubbleEffect, CrepuscularEffect;
        Color CursorColor = Color.White;
        Matrix Projection, MouseTransform, QuadProjection;
        #endregion

        #region Sound effects
        SoundEffect MenuClick, FireTrapStart, LightningSound, CannonExplosion, CannonFire,
                    MachineShot1, GroundImpact, Ricochet1, Ricochet2, Ricochet3, MenuWoosh,
                    PlaceTrap, Splat1, Splat2, MenuMusic, Implosion, TurretOverheat;

        //Ambient sounds
        //SoundEffect PolarWindAmbience;

        SoundEffectInstance MenuMusicInstance, TurretOverheatInstance;
        #endregion

        #region List declarations
        List<Button> TowerButtonList, MainMenuButtonList, PauseButtonList,
                     ProfileButtonList, ProfileDeleteList, PlaceWeaponList,
                     SpecialAbilitiesButtonList;//,  UpgradeButtonList;

        List<Trap> TrapList;
        List<Turret> TurretList;
        List<Invader> InvaderList;

        List<HeavyProjectile> HeavyProjectileList;

        List<Emitter> YSortedEmitterList, AlphaEmitterList, AdditiveEmitterList, GasEmitterList;
        List<Particle> CoinList;

        List<string> PauseMenuNameList;
        
        List<LightningBolt> LightningList;
        List<BulletTrail> TrailList;

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

        List<ShellCasing> VerletShells = new List<ShellCasing>();
        List<GrassBlade> GrassBladeList = new List<GrassBlade>();

        //Powerups need to be drawn in a stack [Power 30] [Blast 15] [TrapDistance 60] at the middle-top of the screen
        List<Powerup> PowerupsList = new List<Powerup>();
        List<UIPowerupIcon> UIPowerupsList = new List<UIPowerupIcon>();
        List<UIPowerupInfo> UIPowerupsInfoList = new List<UIPowerupInfo>();
        List<SoundEffectInstance> LevelAmbience = new List<SoundEffectInstance>();
        #endregion

        #region Custom class declarations
        Button ProfileBackButton, ProfileManagementPlay, ProfileManagementBack,
               OptionsBack, OptionsSFXUp, OptionsSFXDown, OptionsMusicUp, OptionsMusicDown,
               GetNameOK, GetNameBack, MoveTurretsLeft, MoveTurretsRight, MoveTrapsLeft, MoveTrapsRight,
               VictoryRetry, VictoryContinue, VictoryBack;
        Tower Tower;
        StaticSprite Ground, ForeGround, SkyBackground, TextBox, MenuTower;
        StaticSprite MenuBackground1;
        AnimatedSprite LoadingAnimation;
        Nullable<TrapType> SelectedTrap;
        Nullable<TurretType> SelectedTurret;
        //Nullable<SpecialType> SelectedSpecial;
        LightProjectile CurrentProjectile;
        ProfileManagementState ProfileManagementState;
        Thread LoadingThread;
        Profile CurrentProfile;
        StorageDevice Device;
        Stream OpenFile;
        Settings CurrentSettings, DefaultSettings;
        TextInput NameInput;
        DialogBox ExitDialog, DeleteProfileDialog, MainMenuDialog, ProfileMenuDialog, NoWeaponsDialog, NameLengthDialog;
        //Tooltip InGameInformation;
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
        BeamProjectile CurrentBeam;
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

        PowerupDelivery PowerupDelivery;
        StoryDialogueBox StoryDialogueBox;
        StoryDialogue StoryDialogue;

        public List<Drawable> DrawableList = new List<Drawable>();


        #region For lighting - needs to be moved once lighting all works properly


        Texture2D NormalMapTexture;

        VertexPositionColorTexture[] Vertices;
        VertexBuffer VertexBuffer;

        BasicEffect BasicEffect, BasicEffect2;
        Effect LightEffect, LightCombinedEffect, ShadowBlurEffect, DepthEffect, EmissiveBlurEffect;

        public static BlendState BlendBlack = new BlendState()
        {
            ColorBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,

            AlphaBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.One
        };

        Color AmbientLight = new Color(0.85f, 0.85f, 0.85f, 1f);
        private float specularStrength = 1.0f;

        BlendState Multiply = new BlendState() { ColorSourceBlend = Blend.DestinationColor, AlphaSourceBlend = Blend.DestinationAlpha };
        #endregion

        Texture2D LightTexture;

        bool ShowMapping = false;
        Texture2D MappingOutlines;

        Effect ParticleEffect;
        PartitionedBar PowerUnitsBar, MusicVolumeBar, SoundEffectsVolumeBar;
        CheckBox BloodEffectsToggle, FullscreenToggle;

        List<TutorialBox> TutorialBoxList = new List<TutorialBox>();

        Texture2D ShieldBoundingSphere;
        #endregion
        
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
            
            //Could allow the player to toggle this setting on or off with a special setting.
            //e.g. pressing Ctrl+i in the options menu might bring up a special box that allows them to change it
            graphics.SynchronizeWithVerticalRetrace = false;
            SetUpGameWindow();
            IsFixedTimeStep = false;
            //TargetElapsedTime = TimeSpan.FromMilliseconds(1000/60);

            Content.RootDirectory = "Content";

        }

        protected override void Initialize()
        {
            SecondaryContent = new ContentManager(Content.ServiceProvider, Content.RootDirectory);
            GameState = GameState.Menu;
            ProfileManagementState = ProfileManagementState.Loadout;
            ContainerName = "Profiles";
            Tower = new Tower("Tower", new Vector2(100, 450), 350, 120, 3, 5000, 20);
            TowerButtons = (int)Tower.Slots;
            ScreenRectangle = new Rectangle(-256, -256, 1920 + 256, 1080 + 256);

            QuadProjection = Matrix.CreateOrthographicOffCenter(0, 1920, 1080, 0, 0, 1);

            QuadEffect = new BasicEffect(graphics.GraphicsDevice);
            QuadEffect.Projection = QuadProjection;
            QuadEffect.VertexColorEnabled = true;

            BasicEffect2 = new BasicEffect(graphics.GraphicsDevice);
            BasicEffect2.Projection = QuadProjection;
            BasicEffect2.VertexColorEnabled = true;


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

            MenuBackground1 = new StaticSprite("Backgrounds/MenuBackground1", new Vector2(-192/2, -108/2));
            MenuBackground1.DrawDepth = 0.5f;

            foreach (Button button in MainMenuButtonList)
            {
                button.Initialize(OnButtonClick);
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
                PauseButtonList[i].Initialize(OnButtonClick);
            }

            PauseButtonList.Add(new Button(ButtonLeftSprite, new Vector2(0, 1080 - 50 - 32), null, null, null, PauseMenuNameList[4], RobotoRegular20_2, "Left", Color.White));
            PauseButtonList[4].Initialize(OnButtonClick);
            #endregion

            #region Initialise Options Menu
            OptionsSFXUp = new Button(RightArrowSprite, new Vector2(640 + 32, 316));
            OptionsSFXUp.Initialize(OnButtonClick);

            OptionsSFXDown = new Button(LeftArrowSprite, new Vector2(640 - 50 - 32, 316));
            OptionsSFXDown.Initialize(OnButtonClick);

            OptionsMusicUp = new Button(RightArrowSprite, new Vector2(640 + 32, 380));
            OptionsMusicUp.Initialize(OnButtonClick);

            OptionsMusicDown = new Button(LeftArrowSprite, new Vector2(640 - 50 - 32, 380));
            OptionsMusicDown.Initialize(OnButtonClick);

            OptionsBack = new Button(ButtonLeftSprite, new Vector2(0, 1080 - 32 - 50), null, null, null, "back", RobotoRegular20_2, "Left", Color.White);
            OptionsBack.Initialize(OnButtonClick);

            MusicVolumeBar = new PartitionedBar(2, 10, new Vector2(150, 16), new Vector2(100, 100)) { Texture = WhiteBlock };
            SoundEffectsVolumeBar = new PartitionedBar(2, 10, new Vector2(150, 16), new Vector2(100, 150)) { Texture = WhiteBlock };

            FullscreenToggle = new CheckBox(ButtonLeftSprite, new Vector2(50, 50), HealthIcon);
            FullscreenToggle.Font = RobotoRegular20_0;
            FullscreenToggle.Initialize(OnButtonClick);

            BloodEffectsToggle = new CheckBox(ButtonLeftSprite, new Vector2(50, 50), HealthIcon);
            BloodEffectsToggle.Font = RobotoRegular20_0;
            BloodEffectsToggle.Initialize(OnButtonClick);
            #endregion

            #region Initialise Profile Select Menu

            ProfileButtonList = new List<Button>();
            for (int i = 0; i < 4; i++)
            {
                ProfileButtonList.Add(new Button(ButtonLeftSprite, new Vector2(50, 130 + (i * 114)), null, null, null, "empty", RobotoRegular20_2, "Centre", Color.White));
                ProfileButtonList[i].Initialize(OnButtonClick);
            }

            ProfileDeleteList = new List<Button>();
            for (int i = 0; i < 4; i++)
            {
                ProfileDeleteList.Add(new Button(SmallButtonSprite, new Vector2(0, 130 + (i * 114)), null, null, null, "X", RobotoRegular20_2, "Left", Color.White));
                ProfileDeleteList[i].Initialize(OnButtonClick);
            }

            ProfileBackButton = new Button(ButtonRightSprite, new Vector2(1920 + 300, 1080 - 32 - 50), null, null, null, "back     ", RobotoRegular20_2, "Right", Color.White);
            ProfileBackButton.Initialize(OnButtonClick);

            #endregion

            #region Initialise Profile Management Menu
            ProfileManagementTabs = new Tabs(new Vector2(0, 0), WhiteBlock, RobotoRegular20_2, null, "loadout", "upgrades", "stats");

            ProfileManagementPlay = new Button(ButtonRightSprite, new Vector2(1920 - 450 + 300, 1080 - 32 - 50), null, null, null, "play     ", RobotoRegular20_2, "Right", Color.White);
            ProfileManagementPlay.Initialize(OnButtonClick);

            ProfileManagementBack = new Button(ButtonLeftSprite, new Vector2(-300, 1080 - 32 - 50), null, null, null, "     back", RobotoRegular20_2, "Left", Color.White);
            ProfileManagementBack.Initialize(OnButtonClick);

            MoveTurretsRight = new Button(RightArrowSprite, new Vector2(1681, 258));
            MoveTurretsRight.Initialize(OnButtonClick);


            MoveTurretsLeft = new Button(LeftArrowSprite, new Vector2(189, 258));
            MoveTurretsLeft.Initialize(OnButtonClick);

            MoveTrapsRight = new Button(RightArrowSprite, new Vector2(1681, 660));
            MoveTrapsRight.Initialize(OnButtonClick);

            MoveTrapsLeft = new Button(LeftArrowSprite, new Vector2(189, 660));
            MoveTrapsLeft.Initialize(OnButtonClick);

            PlaceWeaponList = new List<Button>();
            for (int i = 0; i < 8; i++)
            {
                PlaceWeaponList.Add(new Button(SelectButtonSprite, new Vector2(565 + (i * 100), 900), null, new Vector2(1f, 1f), null, "", null, "Left", null, true));
                PlaceWeaponList[i].Initialize(OnButtonClick);
            }

            MenuTower = new StaticSprite("Tower", new Vector2(1920 - 300, 150));
            MenuTower.LoadContent(SecondaryContent);

            #endregion

            #region Initialise Get Name Menu
            NameInput = new TextInput(new Vector2(1920 / 2 - 215, 1080 / 2 - 40), 350, RobotoRegular20_2, Color.White);

            GetNameBack = new Button(ButtonLeftSprite, new Vector2(0, 1080 - 32 - 50), null, null, null, "     back", RobotoRegular20_2, "Left", Color.White);
            GetNameBack.CurrentPosition.X = -300;
            GetNameBack.Initialize(OnButtonClick);

            GetNameOK = new Button(ButtonRightSprite, new Vector2(1920 - 450, 1080 - 32 - 50), null, null, null, "create     ", RobotoRegular20_2, "Right", Color.White);
            GetNameOK.Initialize(OnButtonClick);

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

            //Subscribe to events
            ExplosionHappenedEvent += OnExplosionHappened;
            TrapCollisionEvent += OnTrapCollision;
            LightProjectileFiredEvent += OnLightProjectileFired;
            TurretShootEvent += OnTurretShoot;
            RightClickEvent += OnRightClick;
            TrapPlacementEvent += OnTrapPlacement;
            HeavyProjectileCollisionEvent += OnHeavyProjectileCollision;

            LoadingAnimation = new AnimatedSprite("LoadingAnimation", new Vector2(1920 / 2 - 65, 1080 / 2 - 65), new Vector2(131, 131), 17, 30, HalfWhite, Vector2.One, true);
            LoadingAnimation.LoadContent(SecondaryContent);
            IsLoading = false;

            base.Initialize();
        }

        #region CONTENT that needs to be loaded
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            targetBatch = new SpriteBatch(GraphicsDevice);
            
            GameRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080, false, SurfaceFormat.Rgba64, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
            UIRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080, false, SurfaceFormat.Rgba64, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            MenuRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            ShaderTarget1 = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            ShaderTarget2 = new RenderTarget2D(GraphicsDevice, 1920, 1080);

            Projection = Matrix.CreateOrthographicOffCenter(0, 1920, 1080, 0, -5.0f, 5.0f);

            MenuBackground1.LoadContent(SecondaryContent);

            BlankTexture = SecondaryContent.Load<Texture2D>("Blank");
            MappingOutlines = SecondaryContent.Load<Texture2D>("MappingOutlines");

            LoadIcons();
            LoadCursorSprites();

            FPSCounter.spriteFont = DefaultFont;

            #region Menu sounds
            MenuClick = SecondaryContent.Load<SoundEffect>("Sounds/Menu/MenuPing");
            MenuWoosh = SecondaryContent.Load<SoundEffect>("Sounds/Menu/MenuWoosh");
            MenuMusic = SecondaryContent.Load<SoundEffect>("Sounds/Menu/MenuMusic1");

            MenuMusicInstance = MenuMusic.CreateInstance();
            MenuMusicInstance.IsLooped = true;
            //MenuMusicInstance.Play();
            #endregion
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
                IsLoading = true;
                AllLoaded = false;
                ReadyToPlace = false;

                NormalMapTexture = Content.Load<Texture2D>("tileNormal");
                LightTexture = Content.Load<Texture2D>("SmallLight");

                #region Just to handle lighting
                Vertices = new VertexPositionColorTexture[4];
                Vertices[0] = new VertexPositionColorTexture(new Vector3(-1, 1, 0), Color.White, new Vector2(0, 0));
                Vertices[1] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0));
                Vertices[2] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1));
                Vertices[3] = new VertexPositionColorTexture(new Vector3(1, -1, 0), Color.White, new Vector2(1, 1));
                VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColorTexture), Vertices.Length, BufferUsage.None);
                VertexBuffer.SetData(Vertices);

                BasicEffect = new BasicEffect(GraphicsDevice);
                BasicEffect.Projection = Projection;
                BasicEffect.TextureEnabled = true;
                BasicEffect.VertexColorEnabled = true;

                LightColorMap = new RenderTarget2D(GraphicsDevice, 1920, 1080, false, SurfaceFormat.Rgba64, DepthFormat.Depth24Stencil8);
                DepthMap = new RenderTarget2D(GraphicsDevice, 1920, 1080, false, SurfaceFormat.Rgba64, DepthFormat.Depth24Stencil8);
                LightNormalMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);
                LightShadowMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);
                EmissiveMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);

                ShadowBlurEffect = Content.Load<Effect>("ShadowBlur");
                ShadowBlurEffect.Parameters["Projection"].SetValue(Projection);

                DepthEffect = Content.Load<Effect>("DepthEffect");
                DepthEffect.Parameters["Projection"].SetValue(Projection);

                LightEffect = Content.Load<Effect>("LightEffect");
                LightCombinedEffect = Content.Load<Effect>("LightCombinedEffect");

                EmissiveBlurEffect = Content.Load<Effect>("EmissiveBlur");
                EmissiveBlurEffect.Parameters["Projection"].SetValue(Projection);
                #endregion
                               

                #region Loading audio

                LoadGameSounds();


                #endregion

                #region Loading tower

                Tower.LoadContent(Content);


                #endregion

                #region Loading sprites

                LoadInvaderSprites();
                LoadTurretSprites();
                LoadTrapSprites();
                LoadProjectileSprites();
                LoadWeatherSprites();

                ExplosionDecal1 = Content.Load<Texture2D>("Decals/ExplosionDecal1");
                BloodDecal1 = Content.Load<Texture2D>("Decals/BloodDecal1");

                TerrainShrub1 = Content.Load<Texture2D>("Terrain/Shrub");
                GrassBlade1 = Content.Load<Texture2D>("GrassBlade1");

                PowerupDeliveryPod = Content.Load<Texture2D>("PowerupDeliveryPod");
                PowerupDeliveryFin = Content.Load<Texture2D>("PowerupDeliveryFin");
                #endregion

                #region Loading shaders
                ParticleEffect = Content.Load<Effect>("ParticleEffect");
                ParticleEffect.Parameters["Projection"].SetValue(Projection);

                CrepuscularEffect = Content.Load<Effect>("Shaders/CrepuscularRaysEffect");
                CrepuscularEffect.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(0, 1920, 1080, 0, 0, 1));

                HealthBarEffect = Content.Load<Effect>("Shaders/HealthBarEffect");
                HealthBarEffect.Parameters["MatrixTransform"].SetValue(Projection);
                HealthBarSprite = Content.Load<Texture2D>("HealthBarSprite");

                ShockWaveEffect = Content.Load<Effect>("Shaders/ShockWaveEffect");
                ShockWaveEffect.Parameters["MatrixTransform"].SetValue(Projection);

                ShieldBubbleEffect = Content.Load<Effect>("Shaders/ShieldBubbleEffect");
                ShieldBubbleEffect.Parameters["MatrixTransform"].SetValue(Projection);
                ShieldBubbleEffect.Parameters["NormalTexture"].SetValue(Content.Load<Texture2D>("ShieldNormalMap"));
                ShieldBubbleEffect.Parameters["DistortionTexture"].SetValue(Content.Load<Texture2D>("ShieldDistortionMap"));
                #endregion

                #region Loading particles
                LightningShellCasing = Content.Load<Texture2D>("Particles/LightningTurretShell");
                ShellCasing = Content.Load<Texture2D>("Particles/MachineShell");
                MachineBullet = Content.Load<Texture2D>("Particles/MachineBullet");
                Coin = Content.Load<Texture2D>("Particles/Coin");
                Lightning.LoadContent(Content);
                Bolt.LoadContent(Content);
                SplodgeParticle = Content.Load<Texture2D>("Particles/Splodge");
                SmokeParticle = Content.Load<Texture2D>("Particles/Smoke");
                FireParticle = Content.Load<Texture2D>("Particles/FireParticle");
                FireParticle2 = Content.Load<Texture2D>("Particles/FireParticle2");
                ExplosionParticle = Content.Load<Texture2D>("Particles/ExplosionParticle");
                ExplosionParticle2 = Content.Load<Texture2D>("Particles/ExplosionParticle2");
                BallParticle = Content.Load<Texture2D>("Particles/GlowBall");
                SparkParticle = Content.Load<Texture2D>("Particles/Spark");
                RoundSparkParticle = Content.Load<Texture2D>("Particles/RoundSpark");
                HealthParticle = Content.Load<Texture2D>("Particles/HealthParticle");
                BulletTrailCap = Content.Load<Texture2D>("Particles/Cap");
                BulletTrailSegment = Content.Load<Texture2D>("Particles/Segment");
                #endregion


                #region List Creating Code
                //This code just creates the lists for the buttons and traps with the right number of possible slots
                TrapList = new List<Trap>();

                TurretList = new List<Turret>();
                for (int i = 0; i < TowerButtons; i++)
                {
                    TurretList.Add(null);
                }

                InvaderList = new List<Invader>();
                HeavyProjectileList = new List<HeavyProjectile>();
                LightningList = new List<LightningBolt>();
                TrailList = new List<BulletTrail>();
                YSortedEmitterList = new List<Emitter>();
                AlphaEmitterList = new List<Emitter>();
                AdditiveEmitterList = new List<Emitter>();
                GasEmitterList = new List<Emitter>();
                CoinList = new List<Particle>();
                TerrainSpriteList = new List<StaticSprite>();
                WeatherSpriteList = new List<StaticSprite>();
                #endregion


                #region Setting up the buttons
                TowerButtonList = new List<Button>();
                SpecialAbilitiesButtonList = new List<Button>();

                for (int i = 0; i < TowerButtons; i++)
                {
                    TowerButtonList.Add(new Button(TurretSlotButtonSprite, new Vector2(40 + Tower.DestinationRectangle.Width - 32, 500 + ((38 + 90) * i) - 32)));
                    TowerButtonList[i].Initialize(OnButtonClick);
                }

                CooldownButtonList.Clear();

                //Not clearing these lists causes a Disposed Object error to show up for some reason
                //Don't forget to clear the lists between levels or even when moving from menu back to game
                UIWeaponInfoList.Clear();

                UITrapQuickInfoList.Clear();
                UITrapOutlineList.Clear();
                UITurretOutlineList.Clear();
                UITurretInfo = null;
                ClearTurretSelect();
                ClearSelected();
                VerletShells.Clear();

                for (int i = 0; i < 8; i++)
                {
                    CooldownButton button = new CooldownButton(new Vector2(565 + (i * 100), 1080 - 80), new Vector2(90, 65), 1, PlaceWeaponList[i].IconTexture);
                    button.ButtonClickHappened += OnButtonClick;

                    if (CurrentProfile.Buttons[i] != null)
                    {
                        UIWeaponInfoTip uiWeaponInfo = new UIWeaponInfoTip(Vector2.Zero, null, null);

                        if (CurrentProfile.Buttons[i].CurrentTurret != null)
                        {
                            uiWeaponInfo = new UIWeaponInfoTip(new Vector2(button.CurrentPosition.X, button.CurrentPosition.Y - 32),
                                                            ApplyTurretUpgrades((TurretType)CurrentProfile.Buttons[i].CurrentTurret, 0), null);
                            button.ResourceCost = TurretCost(CurrentProfile.Buttons[i].CurrentTurret.Value);
                        }

                        if (CurrentProfile.Buttons[i].CurrentTrap != null)
                        {
                            uiWeaponInfo = new UIWeaponInfoTip(new Vector2(button.CurrentPosition.X, button.CurrentPosition.Y - 32),
                                                              null, ApplyTrapUpgrades((TrapType)CurrentProfile.Buttons[i].CurrentTrap, Vector2.Zero));
                            button.ResourceCost = TrapCost(CurrentProfile.Buttons[i].CurrentTrap.Value);
                        }


                        uiWeaponInfo.CurrencyIcon = CurrencyIcon;
                        uiWeaponInfo.PowerUnitIcon = PowerUnitIcon;
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

                SpecialAbilitiesButtonList.Add(new Button(DiamondButtonSprite, new Vector2(1450, 1080 - 120)));
                SpecialAbilitiesButtonList.Add(new Button(DiamondButtonSprite, new Vector2(1450 + 64 + 4, 1080 - 120)));
                SpecialAbilitiesButtonList.Add(new Button(DiamondButtonSprite, new Vector2(1450 + 32 + 2, 1080 - 120 - 32 - 2)));
                SpecialAbilitiesButtonList.Add(new Button(DiamondButtonSprite, new Vector2(1450 + 32 + 2, 1080 - 120 + 32 + 2)));

                foreach (Button button in SpecialAbilitiesButtonList)
                {
                    button.Initialize(OnButtonClick);
                }

                #endregion

                MysteryEmployerTexture = Content.Load<Texture2D>("MysteryEmployer");
                StoryDialogueBox = new StoryDialogueBox(new Vector2(-400, 64), new Vector2(0, 64), new Vector2(500, 180), MysteryEmployerTexture, RobotoRegular20_0, StoryDialogue, CurrentProfile.Name);

                MaxWaves = CurrentLevel.WaveList.Count;
                CurrentWaveIndex = 0;

                ShieldBoundingSphere = Content.Load<Texture2D>("ShieldBoundingBox");

                #region Loading UI

                StartWaveButton = new Button(ButtonRightSprite, new Vector2(1920 - (ButtonRightSprite.Width / 3), 200), null, null, null, "start waves", RobotoRegular20_2, "Right");
                StartWaveButton.Initialize(OnButtonClick);

                UITurretInfo = new UITurretInfo();
                UITurretInfo.Texture = WhiteBlock;
                UITurretInfo.OverHeatIconTexture = OverHeatIcon;
                UITurretInfo.Font = BigUIFont;

                HealthBar = new UISlopedBar(new Vector2(1920 / 2 - 810 / 2, 980), new Vector2(800 + 15, 15), Color.Lerp(Color.DarkRed, Color.LightGray, 0.2f), false, 15, 0);
                ShieldBar = new UISlopedBar(new Vector2(1920 / 2 - 810 / 2 + 5, 970), new Vector2(810 - 5 + 15, 10), Color.Lerp(Color.White, Color.LightGray, 0.2f), true, 0, 10);


                #endregion

                //Need to pre-apply lighting to everything with normal mapping so that it doesn't all look flat.
                //LightList.Add(new Light(new Vector3(1, 720, 0)) 
                //{ 
                //    Active = true, 
                //    Color = Color.Lerp(Color.White, Color.Transparent, 0.5f), 
                //    Power = 1.0f, 
                //    LightDecay = 500, 
                //    Range = 500, 
                //    Radius = 500,
                //    LightTexture = LightTexture
                //});

                AllLoaded = true;

            }
        } //This is called as a separate thread to be displayed while content is loaded

        private void UnloadGameContent()
        {
            //Clear some variables that aren't cleared as they're not lists that are reset back to new List<> when
            //the "List Creating Code" is called
            if (GameState != GameState.Playing ||
                GameState != GameState.Loading &&
                LoadingThread == null)
            {

                AllLoaded = false;

                PowerupDelivery = null;
                SelectedTrap = null;
                SelectedTurret = null;

                ClearSelected();

                if (DecalList.Count > 0)
                    DecalList.Clear();

                if (DrawableList.Count > 0)
                    DrawableList.Clear();

                if (LightList.Count > 0)
                    LightList.Clear();

                Content.Unload();

            }
        } //This is called when the player exits to the main menu. Unloads game content, not menu content


        private void LoadGameSounds()
        {
            //Load all the in-game sounds here
            //Ambience



            //Turrets
            TurretOverheat = Content.Load<SoundEffect>("Sounds/TurretOverheat");
            LightningSound = Content.Load<SoundEffect>("Sounds/LightningSound");
            CannonExplosion = Content.Load<SoundEffect>("Sounds/CannonExplosion");
            CannonFire = Content.Load<SoundEffect>("Sounds/CannonFire");
            MachineShot1 = Content.Load<SoundEffect>("Sounds/Shot11");

            //Traps
            PlaceTrap = Content.Load<SoundEffect>("Sounds/PlaceTrap");
            FireTrapStart = Content.Load<SoundEffect>("Sounds/FireTrapStart");


            //Invaders



            GroundImpact = Content.Load<SoundEffect>("Sounds/Shot12");
            Ricochet1 = Content.Load<SoundEffect>("Sounds/Ricochet1");
            Ricochet2 = Content.Load<SoundEffect>("Sounds/Ricochet2");
            Ricochet3 = Content.Load<SoundEffect>("Sounds/Ricochet3");
            Splat1 = Content.Load<SoundEffect>("Sounds/Splat1");
            Splat2 = Content.Load<SoundEffect>("Sounds/Splat2");
            Implosion = Content.Load<SoundEffect>("Sounds/Implosion2");

        }

        private void LoadInvaderSprites()
        {
            IceBlock = Content.Load<Texture2D>("IceBlock");
            Shadow = Content.Load<Texture2D>("Shadow");

            #region Solider Animations
            SoldierAnimations = new List<InvaderAnimation>()
            {
                new InvaderAnimation() 
                { 
                    CurrentInvaderState = AnimationState_Invader.Walk,
                    Texture = Content.Load<Texture2D>("Invaders/Soldier/SoldierWalk"),
                    AnimationType = AnimationType.Normal,
                    Animated = true,
                    CurrentFrame = 0,
                    FrameDelay = 150,
                    Looping = true,
                    TotalFrames = 4
                },

                new InvaderAnimation() 
                {
                    CurrentInvaderState = AnimationState_Invader.Melee,
                    Texture = Content.Load<Texture2D>("Invaders/Soldier/SoldierMelee"),
                    AnimationType = AnimationType.Normal,
                    Animated = true,
                    CurrentFrame = 0,
                    FrameDelay = 150,
                    Looping = false,
                    TotalFrames = 4
                },

                new InvaderAnimation() 
                {
                    CurrentInvaderState = AnimationState_Invader.Stand,
                    Texture = Content.Load<Texture2D>("Invaders/Soldier/SoldierStand"),
                    AnimationType = AnimationType.Normal,
                    Animated = true,
                    CurrentFrame = 0,
                    FrameDelay = 150,
                    Looping = true,
                    TotalFrames = 2                    
                }
            };

            foreach (InvaderAnimation animation in SoldierAnimations)
            {
                animation.GetFrameSize();
            }
            #endregion

            #region JumpMan Animations
            JumpManAnimations = new List<InvaderAnimation>()
            {
                new InvaderAnimation() 
                { 
                    CurrentInvaderState = AnimationState_Invader.Walk,
                    Texture = Content.Load<Texture2D>("Invaders/Soldier/SoldierWalk"),
                    AnimationType = AnimationType.Normal,
                    Animated = true,
                    CurrentFrame = 0,
                    FrameDelay = 150,
                    Looping = true,
                    TotalFrames = 4
                },

                new InvaderAnimation() 
                {
                    CurrentInvaderState = AnimationState_Invader.Melee,
                    Texture = Content.Load<Texture2D>("Invaders/Soldier/SoldierMelee"),
                    AnimationType = AnimationType.Normal,
                    Animated = true,
                    CurrentFrame = 0,
                    FrameDelay = 150,
                    Looping = false,
                    TotalFrames = 4
                },

                new InvaderAnimation() 
                {
                    CurrentInvaderState = AnimationState_Invader.Stand,
                    Texture = Content.Load<Texture2D>("Invaders/Soldier/SoldierStand"),
                    AnimationType = AnimationType.Normal,
                    Animated = true,
                    CurrentFrame = 0,
                    FrameDelay = 150,
                    Looping = true,
                    TotalFrames = 2                    
                }
            };

            foreach (InvaderAnimation animation in JumpManAnimations)
            {
                animation.GetFrameSize();
            }
            #endregion

            #region RifleMan Animations
            RifleManAnimations = new List<InvaderAnimation>()
            {
                new InvaderAnimation() 
                { 
                    CurrentInvaderState = AnimationState_Invader.Walk,
                    Texture = Content.Load<Texture2D>("Invaders/RifleMan/RifleManWalk"),
                    AnimationType = AnimationType.Normal,
                    Animated = true,
                    CurrentFrame = 0,
                    FrameDelay = 150,
                    Looping = true,
                    TotalFrames = 4
                },

                new InvaderAnimation() 
                {
                    CurrentInvaderState = AnimationState_Invader.Shoot,
                    Texture = Content.Load<Texture2D>("Invaders/RifleMan/RifleManShoot"),
                    AnimationType = AnimationType.Normal,
                    Animated = true,
                    CurrentFrame = 0,
                    FrameDelay = 150,
                    Looping = false,
                    TotalFrames = 4
                },

                new InvaderAnimation() 
                {
                    CurrentInvaderState = AnimationState_Invader.Stand,
                    Texture = Content.Load<Texture2D>("Invaders/RifleMan/RifleManStand"),
                    AnimationType = AnimationType.Normal,
                    Animated = true,
                    CurrentFrame = 0,
                    FrameDelay = 150,
                    Looping = true,
                    TotalFrames = 2                    
                }
            };

            foreach (InvaderAnimation animation in RifleManAnimations)
            {
                animation.GetFrameSize();
            }
            #endregion

            #region Stationary Cannon Animations
            StationaryCannonAnimations = new List<InvaderAnimation>()
            {
                new InvaderAnimation() 
                { 
                    CurrentInvaderState = AnimationState_Invader.Walk,
                    Texture = Content.Load<Texture2D>("Invaders/StationaryCannon/StationaryCannonBase"),
                    AnimationType = AnimationType.Regular,
                    Animated = true,
                    CurrentFrame = 0,
                    FrameDelay = 150,
                    Looping = true,
                    TotalFrames = 1
                },

                new InvaderAnimation() 
                { 
                    CurrentInvaderState = AnimationState_Invader.Stand,
                    Texture = Content.Load<Texture2D>("Invaders/StationaryCannon/StationaryCannonBase"),
                    AnimationType = AnimationType.Regular,
                    Animated = true,
                    CurrentFrame = 0,
                    FrameDelay = 150,
                    Looping = true,
                    TotalFrames = 1
                },
            };

            StationaryCannonBarrelAnimation = new InvaderAnimation()
            {
                Texture = Content.Load<Texture2D>("Invaders/StationaryCannon/StationaryCannonBarrel"),
                TotalFrames = 1,
                CurrentFrame = 0,
                AnimationType = AnimationType.Regular,
                Animated = true,
                Looping = false
            };

            StationaryCannonBarrelAnimation.GetFrameSize();

            foreach (InvaderAnimation animation in StationaryCannonAnimations)
            {
                animation.GetFrameSize();
            }
            #endregion

            #region Heal Drone Animations
            HealDroneAnimations = new List<InvaderAnimation>()
            {
                new InvaderAnimation() 
                { 
                    CurrentInvaderState = AnimationState_Invader.Walk,
                    Texture = Content.Load<Texture2D>("Invaders/HealDrone/HealDrone"),
                    AnimationType = AnimationType.Regular,
                    Animated = true,
                    CurrentFrame = 0,
                    FrameDelay = 150,
                    Looping = true,
                    TotalFrames = 1                    
                },

                new InvaderAnimation() 
                { 
                    CurrentInvaderState = AnimationState_Invader.Stand,
                    Texture = Content.Load<Texture2D>("Invaders/HealDrone/HealDrone"),
                    AnimationType = AnimationType.Regular,
                    Animated = true,
                    CurrentFrame = 0,
                    FrameDelay = 150,
                    Looping = true,
                    TotalFrames = 1                    
                },
            };

            foreach (InvaderAnimation animation in HealDroneAnimations)
            {
                animation.GetFrameSize();
            }
            #endregion

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
            #region Wall animations
            WallAnimations = new List<TrapAnimation>()
            {
                new TrapAnimation()
                {
                    CurrentTrapState = TrapAnimationState.Untriggered,
                    Texture = Content.Load<Texture2D>("Traps/WallTrap"),
                    Animated = false,
                    CurrentFrame = 0,
                    TotalFrames = 1,
                    
                    AnimationType = AnimationType.Normal
                },
            };

            foreach (TrapAnimation animation in WallAnimations)
            {
                animation.GetFrameSize();
            }
            #endregion

            #region Fire trap animations
            FireTrapAnimations = new List<TrapAnimation>()
            {
                new TrapAnimation()
                {
                    CurrentTrapState = TrapAnimationState.Untriggered,
                    Texture = Content.Load<Texture2D>("Traps/FireTrap"),
                    Animated = false,
                    CurrentFrame = 0,
                    TotalFrames = 1,
                    AnimationType = AnimationType.Regular
                },
            };
            foreach (TrapAnimation animation in FireTrapAnimations)
            {
                animation.GetFrameSize();
            }
            #endregion

            #region Catapult trap animations
            CatapultTrapAnimations = new List<TrapAnimation>()
            {
                new TrapAnimation()
                {
                    CurrentTrapState = TrapAnimationState.Untriggered,
                    Texture = Content.Load<Texture2D>("Traps/CatapultTrap"),
                    Animated = false,
                    CurrentFrame = 0,
                    TotalFrames = 1,
                    AnimationType = AnimationType.Regular
                },
            };
            foreach (TrapAnimation animation in CatapultTrapAnimations)
            {
                animation.GetFrameSize();
            }
            #endregion
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

            FreezeTurretBase = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBase");
            FreezeTurretBarrel = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineTurretBarrel");
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

            BlurryEmitter = new Emitter(BlurrySnowflake, new Vector2(0, -64), new Vector2(270, 270), new Vector2(0.95f, 1.8f),
                new Vector2(1500, 2500), 0.5f, true, new Vector2(0, 360), new Vector2(0, 5), new Vector2(0.1f, 0.25f),
                Color.White, Color.White, 0.0005f, -1, 25, 1, false, new Vector2(690, 930));

            FocusedEmitter = new Emitter(FocusedSnowflake, new Vector2(0, -64), new Vector2(-90, -90), new Vector2(0.5f, 0.75f),
                new Vector2(2000, 3000), 0.25f, true, new Vector2(0, 360), new Vector2(-1f, 1f), new Vector2(0.1f, 0.25f),
                Color.White, Color.White, 0.0005f, -1, 25, 1, true, new Vector2(690, 930), false, 0.25f, true, false, null, null, null, null, null, true, true, null);
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

        private void LoadIcons()
        {
            PowerUnitIcon = SecondaryContent.Load<Texture2D>("Icons/PowerUnitIcon");
            CurrencyIcon = SecondaryContent.Load<Texture2D>("Icons/CurrencyIcon");
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
        }

        private void LoadCursorSprites()
        {
            DefaultCursor = SecondaryContent.Load<Texture2D>("Cursors/DefaultCursor");
            CrosshairCursor = SecondaryContent.Load<Texture2D>("Cursors/Crosshair");

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
        }
        #endregion

        protected override void Draw(GameTime gameTime)
        {
            #region Draw menus
            if (GameState != GameState.Playing)
            {
                GraphicsDevice.SetRenderTarget(MenuRenderTarget);
                GraphicsDevice.Clear(Color.Transparent);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                if (GameState != GameState.Paused && GameState != GameState.Victory && GameState != GameState.Loading)
                {
                    MenuBackground1.Draw(GraphicsDevice, QuadEffect);
                }

                //Drawing the loading screen background here so that it doesn't bug out
                //when drawn as a quad with multiple threads running
                if (GameState == GameState.Loading)
                {
                    MenuBackground1.Draw(spriteBatch);
                }

                switch (GameState)
                {
                    #region Loading Screen
                    case GameState.Loading:
                        {
                            if (AllLoaded == false)
                                LoadingAnimation.Draw(spriteBatch);

                            spriteBatch.DrawString(RobotoBold40_2, AllLoaded.ToString(), new Vector2(100, 100), Color.White);
                        }
                        break;
                    #endregion

                    #region Profile Select Menu
                    case GameState.ProfileSelect:
                        {
                            foreach (Button button in ProfileButtonList)
                            {
                                button.Draw(spriteBatch);
                            }

                            foreach (Button button in ProfileDeleteList)
                            {
                                button.Draw(spriteBatch);
                            }

                            ProfileBackButton.Draw(spriteBatch);
                        }
                        break;
                    #endregion

                    #region Profile Management Menu
                    case GameState.ProfileManagement:
                        {
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
                        }
                        break;
                    #endregion

                    #region Main Menu
                    case GameState.Menu:
                        {
                            foreach (Button button in MainMenuButtonList)
                            {
                                button.Draw(spriteBatch);
                            }
                        }
                        break;
                    #endregion

                    #region Options Menu
                    case GameState.Options:
                        {
                            OptionsSFXUp.Draw(spriteBatch);
                            OptionsSFXDown.Draw(spriteBatch);

                            OptionsMusicUp.Draw(spriteBatch);
                            OptionsMusicDown.Draw(spriteBatch);

                            OptionsBack.Draw(spriteBatch);

                            FullscreenToggle.Draw(spriteBatch);
                            BloodEffectsToggle.Draw(spriteBatch);

                            string SFXVol, MusicVol;
                            SFXVol = MenuSFXVolume.ToString();
                            MusicVol = MenuMusicVolume.ToString();

                            if (SFXVol.Length == 1)
                                SFXVol = SFXVol.Insert(0, "0");

                            if (MusicVol.Length == 1)
                                MusicVol = MusicVol.Insert(0, "0");

                            SoundEffectsVolumeBar.Draw(spriteBatch);
                            MusicVolumeBar.Draw(spriteBatch);

                            spriteBatch.DrawString(DefaultFont, SFXVol, new Vector2(640 - 16, 320), Color.White);
                            spriteBatch.DrawString(DefaultFont, MusicVol, new Vector2(640 - 16, 384), Color.White);
                        }
                        break;
                    #endregion

                    #region Get Name Screen
                    case GameState.GettingName:
                        {
                            spriteBatch.DrawString(RobotoRegular20_2, "profile name",
                                                   new Vector2(TextBox.Position.X, TextBox.Position.Y - DefaultFont.MeasureString("E").Y),
                                                   Color.White);
                            TextBox.Draw(spriteBatch);

                            GetNameBack.Draw(spriteBatch);
                            GetNameOK.Draw(spriteBatch);

                            NameInput.Draw(spriteBatch);
                        }
                        break;
                    #endregion

                    #region Pause menu
                    case GameState.Paused:
                        {
                            foreach (Button button in PauseButtonList)
                            {
                                button.Draw(spriteBatch);
                            }

                            if (MainMenuDialog != null)
                            {
                                MainMenuDialog.Draw(spriteBatch);
                            }

                            if (ProfileMenuDialog != null)
                            {
                                ProfileMenuDialog.Draw(spriteBatch);
                            }
                        }
                        break;
                    #endregion

                    #region Victory Screen
                    case GameState.Victory:
                        {
                            spriteBatch.Draw(WhiteBlock, new Rectangle(665, 180, 590, 720), null, Color.Lerp(Color.Black, Color.Transparent, 0.5f), 0, new Vector2(0, 0), SpriteEffects.None, 1);
                            spriteBatch.DrawString(RobotoRegular40_2, "mission completed", new Vector2(1920 / 2, 210), Color.White, 0, RobotoBold40_2.MeasureString("mission completed") / 2, 1, SpriteEffects.None, 0);
                            VictoryContinue.Draw(spriteBatch);
                        }
                        break;
                    #endregion
                }

                #region Draw menu Dialog boxes
                if (GameState != GameState.Loading)
                {
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
                }
                #endregion
                                
                spriteBatch.End();
            }
            #endregion

            if (GameState == GameState.Playing || GameState == GameState.Paused || GameState == GameState.Victory && IsLoading == false)
            {
                #region Draw actual game
                GraphicsDevice.SetRenderTarget(LightColorMap);
                GraphicsDevice.Clear(Color.Transparent);

                #region Draw the background type stuff - ground, tower, sky, snow etc.
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

                SkyBackground.Draw(spriteBatch);
                Ground.Draw(spriteBatch);

                foreach (Decal decal in DecalList)
                {
                    decal.Draw(spriteBatch);
                }

                //FocusedEmitter.Draw(spriteBatch);

                Tower.Draw(spriteBatch);

                //BlurryEmitter.Draw(spriteBatch);

                spriteBatch.End();
                #endregion

                #region Draw things sorted according to their Y value - To create depth illusion
                //DrawableList = DrawableList.OrderBy(Drawable => Drawable.DrawDepth).ToList();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

                foreach (Drawable drawable in DrawableList)
                {
                    if (drawable.GetType().BaseType == typeof(Invader) ||
                        drawable.GetType().BaseType == typeof(LightRangedInvader) ||
                        drawable.GetType().BaseType == typeof(HeavyRangedInvader) ||
                        drawable.GetType().BaseType == typeof(Trap))
                    {
                        if (drawable.BlendState != BlendState.AlphaBlend)
                        {
                            GraphicsDevice.BlendState = drawable.BlendState;
                            drawable.Draw(GraphicsDevice, BasicEffect, ShadowBlurEffect, LightList);
                            GraphicsDevice.BlendState = BlendState.AlphaBlend;
                        }
                        else
                        {
                            drawable.Draw(GraphicsDevice, BasicEffect, ShadowBlurEffect, LightList);
                        }
                    }
                    else
                    {
                        if (drawable.GetType() == typeof(Emitter))
                        {
                            if (drawable.BlendState != BlendState.AlphaBlend)
                            {
                                GraphicsDevice.BlendState = drawable.BlendState;
                                drawable.Draw(GraphicsDevice, ParticleEffect);
                                GraphicsDevice.BlendState = BlendState.AlphaBlend;
                            }
                            else
                            {
                                drawable.Draw(GraphicsDevice, ParticleEffect);
                            }
                        }
                        else
                        {
                            drawable.Draw(spriteBatch);
                        }
                    }
                }

                //if (PowerupDelivery != null)
                //{
                //    PowerupDelivery.Draw(spriteBatch);
                //}

                //if (CurrentSpecialAbility != null)
                //    CurrentSpecialAbility.Draw(spriteBatch);

                spriteBatch.End();
                #endregion

                #region Draw with additive blending - Makes stuff look glowy
                spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive, SamplerState.PointClamp, null, null);

                foreach (Emitter emitter in AlphaEmitterList)
                {
                    emitter.Draw(spriteBatch);
                }

                foreach (LightningBolt lightningBolt in LightningList)
                {
                    lightningBolt.Draw(spriteBatch);
                }

                foreach (BulletTrail trail in TrailList)
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

                foreach (Invader invader in InvaderList)
                {
                    switch (invader.InvaderType)
                    {
                        case InvaderType.HealDrone:
                            {
                                HealDrone drone = invader as HealDrone;

                                foreach (LightningBolt bolt in drone.BoltList)
                                {
                                    bolt.Draw(spriteBatch);
                                }
                            }
                            break;
                    }
                }
                spriteBatch.End();
                #endregion



                #region Draw to the normal map
                GraphicsDevice.SetRenderTarget(LightNormalMap);
                GraphicsDevice.Clear(new Color(127, 127, 255));

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                GraphicsDevice.BlendState = BlendState.AlphaBlend;

                foreach (Drawable drawable in DrawableList)
                {
                    drawable.DrawSpriteNormal(GraphicsDevice, BasicEffect);
                }

                spriteBatch.End();
                #endregion

                #region Draw to the depth map
                GraphicsDevice.SetRenderTarget(DepthMap);
                GraphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

                foreach (Drawable drawable in DrawableList)
                {
                    drawable.DrawSpriteDepth(GraphicsDevice, DepthEffect);
                }

                spriteBatch.End();
                #endregion

                #region Draw to the emissive map
                GraphicsDevice.SetRenderTarget(EmissiveMap);
                GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                foreach (LightningBolt bolt in LightningList)
                {
                    bolt.Draw(spriteBatch);
                }
                spriteBatch.End();
                #endregion

                #region Draw to the shadow map
                GraphicsDevice.SetRenderTarget(LightShadowMap);
                GraphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                foreach (Light light in LightList)
                {
                    GraphicsDevice.SetVertexBuffer(VertexBuffer);

                    LightEffect.Parameters["lightStrength"].SetValue(light.Power);
                    LightEffect.Parameters["lightPosition"].SetValue(light.Position);
                    LightEffect.Parameters["lightColor"].SetValue(ColorToVector(light.Color));
                    LightEffect.Parameters["lightDecay"].SetValue(light.LightDecay);
                    LightEffect.Parameters["specularStrength"].SetValue(specularStrength);
                    LightEffect.Parameters["lightDepth"].SetValue(light.Depth);

                    LightEffect.CurrentTechnique = LightEffect.Techniques["DeferredPointLight"];

                    LightEffect.Parameters["screenWidth"].SetValue(1920);
                    LightEffect.Parameters["screenHeight"].SetValue(1080);

                    LightEffect.Parameters["ambientColor"].SetValue(AmbientLight.ToVector4());

                    LightEffect.Parameters["NormalMap"].SetValue(LightNormalMap);
                    LightEffect.Parameters["ColorMap"].SetValue(LightColorMap);
                    LightEffect.Parameters["DepthMap"].SetValue(DepthMap);
                    LightEffect.CurrentTechnique.Passes[0].Apply();

                    GraphicsDevice.BlendState = BlendBlack;

                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, Vertices, 0, 2);
                }
                spriteBatch.End();
                #endregion





                GraphicsDevice.SetRenderTarget(GameRenderTarget);
                GraphicsDevice.Clear(Color.CornflowerBlue);

                #region For testing various maps
                ////spriteBatch.Begin();
                ////spriteBatch.Draw(DepthMap, DepthMap.Bounds, Color.White);
                ////spriteBatch.Draw(LightNormalMap, LightNormalMap.Bounds, Color.White);
                ////spriteBatch.Draw(LightShadowMap, LightShadowMap.Bounds, Color.White);
                ////spriteBatch.Draw(EmissiveMap, EmissiveMap.Bounds, Color.White);
                ////spriteBatch.End();
                #endregion

                #region Draw the lightmap and color map combined
                //spriteBatch.Begin();
                //if (Keyboard.GetState().IsKeyDown(Keys.Tab))
                //{
                //    spriteBatch.Draw(LightNormalMap, LightNormalMap.Bounds, Color.White);
                //}

                //if (Keyboard.GetState().IsKeyDown(Keys.OemTilde))
                //{
                //    spriteBatch.Draw(DepthMap, LightNormalMap.Bounds, Color.White);
                //}

                //if (Keyboard.GetState().IsKeyDown(Keys.Q))
                //{
                //    spriteBatch.Draw(LightShadowMap, LightNormalMap.Bounds, Color.White);
                //}
                //spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, LightCombinedEffect);

                    LightCombinedEffect.CurrentTechnique = LightCombinedEffect.Techniques["DeferredCombined"];
                    LightCombinedEffect.Parameters["ambient"].SetValue(1f);
                    LightCombinedEffect.Parameters["lightAmbient"].SetValue(4f);
                    LightCombinedEffect.Parameters["ambientColor"].SetValue(AmbientLight.ToVector4());

                    LightCombinedEffect.Parameters["ColorMap"].SetValue(LightColorMap);
                    LightCombinedEffect.Parameters["ShadingMap"].SetValue(LightShadowMap);
                    LightCombinedEffect.Parameters["NormalMap"].SetValue(LightNormalMap);
                    LightCombinedEffect.Parameters["EmissiveMap"].SetValue(EmissiveMap);

                    LightCombinedEffect.CurrentTechnique.Passes[0].Apply();
                    spriteBatch.Draw(LightColorMap, Vector2.Zero, Color.White);

                spriteBatch.End();
                #endregion


                #endregion

                #region Draw UI
                GraphicsDevice.SetRenderTarget(UIRenderTarget);
                GraphicsDevice.Clear(Color.Transparent);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                
                #region Draw diagnostics
                if (Diagnostics == true)
                {
                    spriteBatch.DrawString(DefaultFont, Slow.ToString(), Vector2.Zero, Color.Red);
                    spriteBatch.DrawString(DefaultFont, CurrentProfile.LevelNumber.ToString(), new Vector2(100, 200), Color.Purple);
                    spriteBatch.DrawString(DefaultFont, "Seconds:" + Seconds.ToString(), new Vector2(0, 16), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "Resources:" + Resources.ToString(), new Vector2(0, 32), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "CurrentWaveIndex:" + (CurrentWaveIndex + 1).ToString() + "/" + MaxWaves, new Vector2(0, 48), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "ShieldOn:" + Tower.Shield.ShieldOn.ToString(), new Vector2(0, 64), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "CurrentShieldTime:" + Tower.Shield.CurrentShieldTime.ToString(), new Vector2(0, 80), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "CurrentWaveTime:" + CurrentWaveTime.ToString(), new Vector2(0, 96), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "HeavyProjectiles:" + HeavyProjectileList.Count.ToString(), new Vector2(0, 112), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "Drawables:" + DrawableList.Count.ToString(), new Vector2(0, 124), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "TrailList:" + TrailList.Count.ToString(), new Vector2(0, 136), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "Emitter1:" + YSortedEmitterList.Count.ToString(), new Vector2(0, 148), Color.Lime);
                    //spriteBatch.DrawString(DefaultFont, "Emitter2:" + EmitterList2.Count.ToString(), new Vector2(0, 160), Color.Lime);
                    //spriteBatch.DrawString(DefaultFont, "InvaderHeavy:" + InvaderHeavyProjectileList.Count.ToString(), new Vector2(0, 172), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "Invaders:" + InvaderList.Count.ToString(), new Vector2(0, 184), Color.Lime);
                    spriteBatch.DrawString(DefaultFont, "Particles:" + TotalParticles.ToString(), new Vector2(0, 200), Color.Lime);
                    //spriteBatch.DrawString(DefaultFont, "Shells:" + ShellCasingList.Count.ToString(), new Vector2(0, 216), Color.Lime);
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
                    spriteBatch.DrawString(DefaultFont, "CurrentPowerUnits:" + Tower.CurrentPowerUnits.ToString(), new Vector2(0, 248 + 64 + 32 + 32), Color.Lime);

                    foreach (Invader invader in InvaderList)
                    {
                        spriteBatch.DrawString(RobotoRegular20_0, invader.InvaderAnimationState.ToString(), invader.Position, Color.White, 0, Vector2.Zero, 0.85f, SpriteEffects.None, 0);
                        spriteBatch.DrawString(RobotoRegular20_0, "Micro:" + invader.CurrentMicroBehaviour.ToString(), invader.Position + new Vector2(0, 16), Color.White, 0, Vector2.Zero, 0.85f, SpriteEffects.None, 0);
                        spriteBatch.DrawString(RobotoRegular20_0, "Macro:" + invader.CurrentMacroBehaviour.ToString(), invader.Position + new Vector2(0, 32), Color.White, 0, Vector2.Zero, 0.85f, SpriteEffects.None, 0);
                    }

                    foreach (Turret turret in TurretList.Where(Turret => Turret != null))
                    {
                        spriteBatch.DrawString(RobotoRegular20_0, MathHelper.ToDegrees(turret.Rotation).ToString(), turret.Position, Color.White);
                    }

                    //foreach (HeavyRangedInvader invader in InvaderList.Where(Invader => Invader.InvaderType == InvaderType.StationaryCannon))
                    //{
                    //    spriteBatch.DrawString(RobotoRegular20_0, MathHelper.ToDegrees(invader.CurrentAngle).ToString(), invader.Position, Color.White);
                    //}

                    spriteBatch.DrawString(DefaultFont, (gameTime.ElapsedGameTime.TotalMilliseconds).ToString(), new Vector2(1920 - 50, 0), Color.Red);
                    FPSCounter.Draw(spriteBatch);
                }
                #endregion

                //Draw the damage numbers for when the invaders are damaged
                foreach (NumberChange numChange in NumberChangeList)
                {
                    numChange.Draw(spriteBatch);
                }

                if (WaveCountDown != null && WaveCountDown.CurrentSeconds > -1)
                    WaveCountDown.Draw(spriteBatch);
                
                spriteBatch.Draw(CurrencyIcon, new Rectangle(485, 1080 - 52, 24, 24), Color.White);
                spriteBatch.DrawString(RobotoRegular20_0, Resources.ToString(), new Vector2(552 - RobotoRegular20_0.MeasureString(Resources.ToString()).X, 1080 - 50), Color.White);

                PowerUnitsBar.Draw(spriteBatch);

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
                #endregion

                foreach (Button towerSlot in TowerButtonList)
                {
                    towerSlot.Draw(spriteBatch);
                }


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
                            if (trapOutline.Trap == trap && trap.Active == true)
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

                #region Draw outlines and health bars around invaders when moused over
                foreach (Invader invader in InvaderList)
                {
                    if (invader.InvaderOutline.Visible == true)
                    {
                        foreach (EffectPass pass in QuadEffect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            invader.HealthBar.Draw(GraphicsDevice);
                        }
                    }

                    invader.InvaderOutline.Draw(spriteBatch);
                }
                #endregion



                #region Draw Weapon Info Boxes
                foreach (UIWeaponInfoTip uiWeaponInfo in UIWeaponInfoList)
                {
                    if (uiWeaponInfo != null)
                        uiWeaponInfo.Draw(spriteBatch, GraphicsDevice, QuadEffect);
                }
                #endregion

                #region Draw Trap Quick Info
                foreach (UITrapQuickInfo uiTrapQuickInfo in UITrapQuickInfoList)
                {
                    uiTrapQuickInfo.Draw(GraphicsDevice, QuadEffect, spriteBatch);
                }
                #endregion

                if (GameState != GameState.Paused)
                    StoryDialogueBox.Draw(spriteBatch, GraphicsDevice, QuadEffect);


                #region Draw the debug bounding boxes
                if (BoundingBoxes == true)
                {
                    //Draw the shield debug box
                    spriteBatch.Draw(ShieldBoundingSphere, new Rectangle((int)Tower.DestinationRectangle.Center.X - 300, (int)Tower.DestinationRectangle.Center.Y - 300, 600, 600), Color.White);

                    BasicEffect2.TextureEnabled = true;
                    BasicEffect2.Texture = WhiteBlock;

                    foreach (Drawable drawable in DrawableList)
                    {
                        Color drawableColor = Color.White;

                        //if (drawable.GetType().BaseType == typeof(Trap))
                        //{
                        //    drawableColor = Color.Fuchsia;
                        //}

                        if (drawable.GetType().BaseType == typeof(Invader))
                        {
                            drawableColor = Color.Lime;
                        }

                        if (drawable.GetType().BaseType == typeof(LightRangedInvader))
                        {
                            drawableColor = Color.Yellow;
                        }

                        if (drawable.GetType().BaseType == typeof(HeavyRangedInvader))
                        {
                            drawableColor = Color.Violet;
                        }

                        //Draw the debug bounding boxes here
                        VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[4];
                        int[] Indices = new int[8];

                        Vertices[0] = new VertexPositionColorTexture()
                        {
                            Color = drawableColor,
                            Position = drawable.BoundingBox.Min,
                            TextureCoordinate = new Vector2(0, 0)
                        };

                        Vertices[1] = new VertexPositionColorTexture()
                        {
                            Color = drawableColor,
                            Position = new Vector3(drawable.BoundingBox.Max.X, drawable.BoundingBox.Min.Y, 0),
                            TextureCoordinate = new Vector2(1, 0)
                        };

                        Vertices[2] = new VertexPositionColorTexture()
                        {
                            Color = drawableColor,
                            Position = drawable.BoundingBox.Max,
                            TextureCoordinate = new Vector2(1, 1)
                        };

                        Vertices[3] = new VertexPositionColorTexture()
                        {
                            Color = drawableColor,
                            Position = new Vector3(drawable.BoundingBox.Min.X, drawable.BoundingBox.Max.Y, 0),
                            TextureCoordinate = new Vector2(0, 1)
                        };

                        Indices[0] = 0;
                        Indices[1] = 1;

                        Indices[2] = 2;
                        Indices[3] = 3;

                        Indices[4] = 0;

                        Indices[5] = 2;
                        Indices[6] = 0;
                        
                        foreach (EffectPass pass in BasicEffect2.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineStrip, Vertices, 0, 4, Indices, 0, 6, VertexPositionColorTexture.VertexDeclaration);
                        }
                    }
                }
                #endregion

                spriteBatch.End();
                #endregion
            }

            #region Draw the cursor detail - circular bars showing overheat etc.
            if (GameState != GameState.Loading && GameState == GameState.Playing && CurrentTurret != null)
            {
                //THIS MAY HAVE BEEN CAUSING UNEXPECTED ERROR. IT WASN'T CHECKING IF GAMESTATE != LOADING FIRST
                //MOVED IT INSIDE THE ABOVE CURSOR DRAW CHECK.
                spriteBatch.Begin(SpriteSortMode.Texture, BlendState.NonPremultiplied, SamplerState.PointClamp,
                                  DepthStencilState.None, RasterizerState.CullCounterClockwise, null);

                //HealthBarEffect.Parameters["meterValue"].SetValue(((CurrentTurret.MaxHeat / 100) * CurrentTurret.CurrentHeat) / 100);
                spriteBatch.Draw(HealthBarSprite, new Vector2(CursorPosition.X, CursorPosition.Y), null, Color.White, MathHelper.ToRadians(-90),
                           new Vector2(HealthBarSprite.Width / 2, HealthBarSprite.Height / 2), 1f, SpriteEffects.None, 0);

                spriteBatch.End();
            }
            #endregion

            #region Draw everything to the backbuffer
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            targetBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            #region DRAW ACTUAL GAME
            if (GameState == GameState.Playing || GameState == GameState.Paused || GameState == GameState.Victory && IsLoading == false)
            {
                targetBatch.Draw(GameRenderTarget, ScreenDrawRectangle, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 1f);
                targetBatch.Draw(UIRenderTarget, ScreenDrawRectangle, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);

                if (ShowMapping == true)
                {
                    targetBatch.Draw(DepthMap, new Rectangle(1920, 0, 480, 270), Color.White);
                    targetBatch.Draw(LightNormalMap, new Rectangle(1920, 270, 480, 270), Color.White);
                    targetBatch.Draw(LightShadowMap, new Rectangle(1920, 540, 480, 270), Color.White);
                    targetBatch.Draw(EmissiveMap, new Rectangle(1920, 810, 480, 270), Color.White);
                    targetBatch.Draw(MappingOutlines, new Rectangle(1920, 0, 480, 1080), Color.White);
                }
            }
            #endregion

            #region DRAW THE MENU ITEMS
            if (GameState != GameState.Playing)
            {
                targetBatch.Draw(MenuRenderTarget, ScreenDrawRectangle, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
            }
            #endregion

            CursorDraw(targetBatch);
            targetBatch.End();
            #endregion
            base.Draw(gameTime);
        }

        protected override void Update(GameTime gameTime)
        {
            CurrentGameTime = gameTime;

            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();

            CursorPosition = GetRealMousePosition();

            if (CurrentMouseState.LeftButton != PreviousMouseState.LeftButton)
            {
                LeftButtonState = CurrentMouseState.LeftButton;
            }

            if (this.IsActive == true)
                ButtonsUpdate(gameTime);

            if (CurrentMouseState.RightButton == ButtonState.Released &&
                PreviousMouseState.RightButton == ButtonState.Pressed)
            {
                CreateRightClick();
            }

            #region Turret and Trap can't be selected to place at the same time
            if (GameState == GameState.Playing || GameState == GameState.ProfileManagement)
            {
                if (SelectedTurret != null)
                    SelectedTrap = null;

                if (SelectedTrap != null)
                    SelectedTurret = null;
            }
            #endregion

            #region Open pause menu
            if (CurrentMouseState.LeftButton == ButtonState.Released &&
                CurrentKeyboardState.IsKeyUp(Keys.Escape) &&
                PreviousKeyboardState.IsKeyDown(Keys.Escape))
            {
                if (GameState == GameState.Playing || GameState == GameState.Paused)
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
            }
            #endregion
            
            switch (GameState)
            {
                #region In Game Update
                case GameState.Playing:
                    {
                        Slow = gameTime.IsRunningSlowly;
                        FPSCounter.Update(gameTime);

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

                        if (CurrentMouseState.LeftButton == ButtonState.Released &&
                           CurrentKeyboardState.IsKeyUp(Keys.F3) &&
                           PreviousKeyboardState.IsKeyDown(Keys.F3))
                        {
                            switch (BoundingBoxes)
                            {
                                case false:
                                    BoundingBoxes = true;
                                    break;

                                case true:
                                    BoundingBoxes = false;
                                    break;
                            }
                        }
                        #endregion

                        #region Show RenderTarget Maps
                        if (CurrentKeyboardState.IsKeyUp(Keys.F2) && PreviousKeyboardState.IsKeyDown(Keys.F2))
                        {
                            switch (ShowMapping)
                            {
                                case true:
                                    ShowMapping = false;
                                    graphics.PreferredBackBufferWidth = (int)ActualResolution.X;
                                    graphics.ApplyChanges();
                                    break;

                                case false:
                                    ShowMapping = true;
                                    graphics.PreferredBackBufferWidth = (int)ActualResolution.X + 480;
                                    graphics.ApplyChanges();

                                    var form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(this.Window.Handle);
                                    form.SetDesktopLocation(-3, 46);
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

                        //Seconds += gameTime.ElapsedGameTime.TotalMilliseconds;

                        //PowerUnitsBar ended up being null for some reason. Keep an eye on it.
                        PowerUnitsBar.Update(Tower.CurrentPowerUnits, Tower.MaxPowerUnits);

                        HealthBar.Update(Tower.MaxHP, Tower.CurrentHP, gameTime);
                        ShieldBar.Update(Tower.Shield.MaxShield, Tower.Shield.CurrentShield, gameTime);

                        Tower.Update(gameTime);

                        int totalPower = 0;
                        foreach (Trap trap in TrapList)
                        {
                            totalPower += trap.PowerCost;
                        }

                        Tower.CurrentPowerUnits = Tower.MaxPowerUnits - totalPower;

                        HandleWaves(gameTime);

                        if (WaveCountDown != null && WaveCountDown.CurrentSeconds > -1)
                            WaveCountDown.Update(gameTime);

                        if (StoryDialogueBox != null && StoryDialogueBox.CurrentDialogue != null)
                            StoryDialogueBox.Update(gameTime);

                        if (CurrentMouseState.LeftButton == ButtonState.Released &&
                            PreviousMouseState.LeftButton == ButtonState.Pressed)
                        {
                            if (ReadyToPlace == true &&
                                CursorPosition.X > (Tower.Position.X + Tower.Texture.Width) &&
                                (CursorPosition.Y - CursorPosition.Y % 32) <= 896 &&
                                (CursorPosition.Y - CursorPosition.Y % 32) >= 672 &&
                                SelectedTrap != null &&
                                TrapList.All(Trap => !Trap.DestinationRectangle.Contains(VectorToPoint(CursorPosition))))
                            {
                                CreateTrapPlacement(SelectedTrap.Value);
                            }
                        }
                        
                        #region Update and clean PowerupsList
                        if (PowerupsList.Count > 0)
                        {
                            for (int i = 0; i < PowerupsList.Count; i++)
                            {
                                PowerupsList[i].Update(gameTime);

                                if (PowerupsList[i].Active == false)
                                {
                                    PowerupsList.RemoveAt(i);
                                }
                            }
                        }
                        #endregion
                        
                        #region Handle Particle Emitters and Particles
                        //UpdateWeather(gameTime);

                        for (int s = 0; s < VerletShells.Count; s++)
                        {
                            VerletShells[s].Update(gameTime);

                            if (VerletShells[s].Active == false)
                            {
                                DrawableList.Remove(VerletShells[s]);
                            }
                        }

                        #region Handle YSortedEmitterList
                        for (int i = 0; i < YSortedEmitterList.Count; i++)
                        {
                            Emitter emitter = YSortedEmitterList[i];

                            emitter.Update(gameTime);

                            if (emitter.AddMore == false && emitter.ParticleList.Count == 0)
                            {
                                YSortedEmitterList.RemoveAt(i);
                                DrawableList.Remove(emitter);
                            }
                        }
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
                        #endregion

                        #region Projectile update stuff
                        if (HeavyProjectileList.Count > 0)
                            HeavyProjectileUpdate(gameTime);

                        if (CurrentBeam != null)
                            BeamProjectileUpdate(gameTime);
                        #endregion

                        #region Turret stuff
                        if (TurretList.Any(Turret => Turret != null))
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
                            foreach (Turret turret in TurretList.Where(Turret => Turret != null && Turret.Overheated == true))
                            {
                                if (TurretOverheatInstance == null)
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

                                    TurretOverheatInstance = TurretOverheat.CreateInstance();
                                    TurretOverheatInstance.IsLooped = true;
                                    TurretOverheatInstance.Play();
                                }
                            }

                            if (TurretOverheatInstance != null)
                            {
                                if (TurretList.Any(Turret => Turret != null) &&
                                    TurretOverheatInstance.State == SoundState.Playing)
                                {
                                    List<Turret> NotNullTurrets = TurretList.FindAll(Turret => Turret != null);

                                    if (NotNullTurrets.All(Turret => Turret.Overheated == false) && TurretOverheatInstance.Volume > 0)
                                    {
                                        TurretOverheatInstance.Volume -= 0.05f;
                                    }
                                }

                                if (TurretOverheatInstance.Volume <= 0.05f)
                                {
                                    TurretOverheatInstance.Stop();
                                    TurretOverheatInstance = null;
                                }
                            }
                            #endregion

                            TurretUpdate(gameTime);
                        }
                        #endregion

                        #region Trap stuff
                        if (TrapList.Count > 0)
                        {
                            TrapUpdate(gameTime);

                            for (int t = 0; t < TrapList.Count; t++)
                            {
                                if (TrapList[t].CurrentHP <= 0)
                                {
                                    DrawableList.Remove(TrapList[t]);
                                    TrapList.Remove(TrapList[t]);
                                }
                            }
                        }

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
                                                null, null, null, null, null, null, new Vector2(0.05f, 0.05f));
                                YSortedEmitterList.Add(DustEmitter);

                                Emitter DebrisEmitter = new Emitter(SplodgeParticle,
                                                new Vector2(PowerupDelivery.Position.X, PowerupDelivery.MaxY),
                                                new Vector2(40, 140),
                                                new Vector2(2, 8), new Vector2(10, 80), 1f, true, new Vector2(0, 360),
                                                new Vector2(1, 3), new Vector2(0.007f, 0.05f), Color.DarkSlateGray, Color.SaddleBrown,
                                                0.2f, 0.1f, 1, 5, true, new Vector2(PowerupDelivery.MaxY, PowerupDelivery.MaxY + 8), null, PowerupDelivery.MaxY / 1080,
                                                null, null, null, null, null, null, new Vector2(0.02f, 0.02f));
                                YSortedEmitterList.Add(DebrisEmitter);

                                Emitter DebrisEmitter2 = new Emitter(SplodgeParticle,
                                        new Vector2(PowerupDelivery.Position.X, PowerupDelivery.MaxY),
                                        new Vector2(70, 110),
                                        new Vector2(2, 8), new Vector2(20, 60), 1f, true, new Vector2(0, 360), new Vector2(1, 3),
                                        new Vector2(0.01f, 0.02f), Color.Gray, Color.SaddleBrown, 0.2f, 0.1f, 1, 5, true,
                                        new Vector2(PowerupDelivery.MaxY + 8, PowerupDelivery.MaxY + 16), null, PowerupDelivery.MaxY / 1080,
                                        null, null, null, null, null, null, new Vector2(0.02f, 0.02f));
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

                                AddDrawable(DebrisEmitter2, DustEmitter, DebrisEmitter, SmokeEmitter);

                                PowerupDelivery.Position.Y = PowerupDelivery.MaxY;

                                DecalList.Add(new Decal(ExplosionDecal1, PowerupDelivery.Position, 0, new Vector2(0, 0), PowerupDelivery.MaxY, 1));
                            }

                            //This needs to be moved
                            PowerupDelivery.Update(gameTime);
                            if (PowerupDelivery.CurrentHP <= 0)
                            {
                                PowerupsList.Add(PowerupDelivery.Powerup);
                                UIPowerupsList.Add(new UIPowerupIcon(PowerupDelivery.Powerup, GetPowerupIcon(PowerupDelivery.Powerup), new Vector2(1920 / 2, 32)));
                                UIPowerupsInfoList.Add(new UIPowerupInfo(new Vector2(1920 / 2, 32 + 32), PowerupDelivery.Powerup, RobotoRegular20_0));
                                PowerupDelivery = null;
                            }
                        }

                        #endregion

                        #region Update effects
                        #region Update and clean LightningBolts
                        for (int i = 0; i < LightningList.Count; i++)
                        {
                            LightningBolt lightningBolt = LightningList[i];

                            if (lightningBolt.Tethered == true &&
                                lightningBolt.DestinationInvader != null &&
                                lightningBolt.SourceInvader != null)
                            {
                                lightningBolt.Source = lightningBolt.SourceInvader.Center;
                                lightningBolt.Destination = lightningBolt.DestinationInvader.Center;
                            }

                            lightningBolt.Update(gameTime);

                            if (lightningBolt.Alpha <= 0)
                            {
                                LightningList.RemoveAt(i);
                            }
                        }
                        #endregion

                        #region Update and clean TrailList
                        for (int i = 0; i < TrailList.Count; i++)
                        {
                            BulletTrail trail = TrailList[i];
                            trail.Update(gameTime);

                            if (trail.Color == Color.Transparent)
                                TrailList.RemoveAt(i);
                        }
                        #endregion

                        #region Update and clean DecalList
                        for (int i = 0; i < DecalList.Count; i++)
                        {
                            DecalList[i].Update(gameTime);

                            if (DecalList[i].TransparencyPercentage <= 0)
                                DecalList.RemoveAt(i);
                        }
                        #endregion

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

                        #region Update and clean LightList
                        for (int i = 0; i < LightList.Count; i++)
                        {
                            Light light = LightList[i];
                            light.Update(gameTime);

                            if (light.Active == false)
                            {
                                LightList.RemoveAt(i);
                            }

                            //REMOVE LIGHTS WHERE THE TETHER.ACTIVE == FALSE
                            if (light.Tether != null)
                            {
                                if (light.Tether.GetType().GetMember("Active") != null)
                                {
                                    Drawable tether = light.Tether as Drawable;
                                    if (tether.Active == false)
                                    {
                                        LightList.Remove(light);
                                    }
                                }
                            }
                        }
                        #endregion


                        //Powerups have to be applied before the invaders are updated and after the explosions and projectiles are updated
                        //Create explosions/projectiles
                        //Powerup explosions/projectiles
                        //Damage invaders
                        //Remove explosions/projectiles

                        ApplyPowerups();

                        if (InvaderList.Count > 0)
                        {
                            InvaderUpdate(gameTime);
                        }
                    }
                    break;
                #endregion

                #region Options Menu Update
                case GameState.Options:
                    {
                        MusicVolumeBar.Update((int)MenuMusicVolume);
                        SoundEffectsVolumeBar.Update((int)MenuSFXVolume);
                    }
                    break;
                #endregion

                #region Update Loading Animation
                case GameState.Loading:
                    {
                        if (IsLoading == true && AllLoaded == false)
                        {
                            LoadingAnimation.Update(gameTime);
                        }
                    }
                    break;
                #endregion
            }

            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;

            base.Update(gameTime);
        }


        private void InvaderUpdate(GameTime gameTime)
        {
            for (int p = 0; p < InvaderList.Count; p++)
            {
                Invader invader = InvaderList[p];
                HeavyRangedInvader heavyRangedInvader = invader as HeavyRangedInvader;
                LightRangedInvader lightRangedInvader = invader as LightRangedInvader;

                invader.Update(gameTime, CursorPosition);

                #region Show damage and healing values
                if (invader.CurrentHP < invader.PreviousHP)
                {
                    NumberChangeList.Add(new NumberChange(RobotoRegular20_0, invader.Position, new Vector2(0, -1.5f), 0 - (int)(invader.PreviousHP - invader.CurrentHP)));
                }

                invader.PreviousHP = invader.CurrentHP;
                #endregion

                #region DIED
                if (invader.CurrentHP <= 0)
                {
                    //Airborne invaders should turn into a verlet object and crash to the ground creating an explosion effect
                    //Instead of just disappearing into thin air
                    switch (invader.InvaderType)
                    {
                        #region Soldier
                        case InvaderType.Soldier:
                            {
                                Emitter BloodEmitter = new Emitter(SplodgeParticle,
                                invader.Center,
                                new Vector2(0, 360), new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360),
                                new Vector2(1, 3), new Vector2(0.02f, 0.06f), RandomColor(Color.Red, Color.DarkRed, Color.DarkRed),
                                RandomColor(Color.Red, Color.DarkRed, Color.DarkRed), 0.1f, 0.2f, 20, 10, true,
                                new Vector2(invader.MaxY, invader.MaxY), false, 1.0f, true, false, null, null, null, null, null, true, true);

                                Emitter BloodEmitter2 = new Emitter(SmokeParticle,
                                invader.Center,
                                new Vector2(0, 360), new Vector2(0.25f, 0.75f), new Vector2(20, 60), 1f, true, new Vector2(0, 360),
                                new Vector2(0.5f, 2f), new Vector2(0.2f, 0.6f), RandomColor(Color.Red, Color.DarkRed, Color.DarkRed),
                                RandomColor(Color.Red, Color.DarkRed, Color.DarkRed), 0.01f, 0.2f, 10, 1, true,
                                new Vector2(invader.MaxY, invader.MaxY), false, 1.0f, true, false, null, null, null, null, null, true, true);

                                Decal NewDecal = new Decal(BloodDecal1, new Vector2(invader.Center.X, invader.BoundingBox.Max.Y),
                                                          (float)RandomDouble(0, 0), invader.YRange, invader.MaxY, 0.1f);

                                DecalList.Add(NewDecal);

                                YSortedEmitterList.Add(BloodEmitter);
                                YSortedEmitterList.Add(BloodEmitter2);

                                AddDrawable(BloodEmitter, BloodEmitter2);
                            }
                            break;
                        #endregion
                    }

                    #region Create the Coin visual based on the resource value of the invader
                    for (int i = 0; i < invader.ResourceValue / 10; i++)
                    {
                        CoinList.Add(new Particle(Coin, invader.Center,
                                    (float)RandomDouble(70, 110), (float)RandomDouble(6, 8), 200f, 1f, true, (float)RandomDouble(0, 360), (float)RandomDouble(-5, 5), 0.75f,
                                    Color.White, Color.White, 0.2f, true, MathHelper.Clamp(invader.MaxY + (float)RandomDouble(2, 32), 630, 960), false, null, true, true, false, false));
                    }
                    #endregion

                    Resources += invader.ResourceValue;
                    invader.Active = false;

                    InvaderList.Remove(invader);
                    DrawableList.Remove(invader);
                    return;
                }
                #endregion
                                
                #region MOVEMENT
                #region Stop the invader if it hits the tower or a trap
                if (invader.BoundingBox.Intersects(Tower.BoundingBox) || 
                    TrapList.Any(Trap => Trap.Solid == true && Trap.BoundingBox.Intersects(invader.BoundingBox)))
                {
                    invader.CurrentMicroBehaviour = MicroBehaviour.Stationary;
                }
                else
                {
                    if (invader.Frozen == false &&
                        invader.InTrapRange == false &&
                        invader.InTowerRange == false &&
                        invader.CurrentMicroBehaviour != MicroBehaviour.MovingBackwards)
                    {
                        #region RangedInvader - Don't keep moving if it's in range
                        if (heavyRangedInvader != null)
                        {
                            if (heavyRangedInvader.CurrentAngle == heavyRangedInvader.EndAngle)
                            {
                                invader.CurrentMicroBehaviour = MicroBehaviour.MovingForwards;
                            }
                        }
                        #endregion
                        else
                        {
                            invader.CurrentMicroBehaviour = MicroBehaviour.MovingForwards;
                        }
                    }
                }
                #endregion
                
                #region Let ranged invaders know how far away from the tower they are
                if (lightRangedInvader != null)
                {
                    lightRangedInvader.DistToTower = invader.Position.X - Tower.DestinationRectangle.Right;
                }

                if (heavyRangedInvader != null)
                {
                    heavyRangedInvader.DistToTower = invader.Position.X - Tower.DestinationRectangle.Right;
                }
                #endregion
                #endregion


                #region Damage the invaders if they walk into a gas cloud
                foreach (Emitter emitter in GasEmitterList)
                {
                    if (emitter.ParticleList.Any(Particle => 
                        Particle.DestinationRectangle.Intersects(invader.DestinationRectangle) &&
                        invader.DestinationRectangle.Bottom < Particle.DestinationRectangle.Bottom &&
                        Particle.CurrentTransparency > 0.05f))
                    {
                        invader.DamageOverTime(new DamageOverTimeStruct()
                        {
                            Color = Color.LawnGreen,
                            Damage = 3,
                            InitialDamage = 30,
                            MaxInterval = 50,
                            MaxDelay = 4000
                        }, Color.LawnGreen);
                    }
                }
                #endregion


                #region TRAP collision event
                if (TrapList.Any(Trap => Trap.BoundingBox.Intersects(invader.BoundingBox) && Trap.CanTrigger == true) && 
                    invader.VulnerableToTrap == true)
                {
                    Trap trap = TrapList.Find(Trap => Trap.BoundingBox.Intersects(invader.BoundingBox) && Trap.CanTrigger == true);
                    if (trap != null)
                    {
                        CreateCollision(trap, invader);
                    }
                }
                else
                {
                    invader.VulnerableToTrap = false;
                }
                #endregion


                #region MELEE attack TRAPS

                #endregion

                #region MELEE attack TOWER

                #endregion


                #region HEAVY ranged invaders fire
                if (heavyRangedInvader != null && heavyRangedInvader.CanAttack == true && heavyRangedInvader.InTowerRange == true)
                {
                    switch (heavyRangedInvader.InvaderType) 
                    {
                        #region Spider
                        case InvaderType.Spider:
                            {
                                HeavyProjectile heavyProjectile = new AcidProjectile(heavyRangedInvader, AcidSprite, RoundSparkParticle,
                                            heavyRangedInvader.Center,
                                            Random.Next((int)(heavyRangedInvader.LaunchVelocityRange.X), (int)(heavyRangedInvader.LaunchVelocityRange.Y)),
                                            -MathHelper.ToRadians(Random.Next((int)(heavyRangedInvader.AngleRange.X), (int)(heavyRangedInvader.AngleRange.Y))),
                                            0.2f, heavyRangedInvader.RangedDamage);

                                heavyProjectile.YRange = new Vector2(invader.MaxY, invader.MaxY);
                                AddDrawable(heavyProjectile);
                            }
                            break;
                        #endregion

                        #region Archer
                        case InvaderType.Archer:
                            {
                                HeavyProjectile heavyProjectile = new AcidProjectile(heavyRangedInvader, AcidSprite, RoundSparkParticle,
                                            heavyRangedInvader.Center,
                                            Random.Next((int)(heavyRangedInvader.LaunchVelocityRange.X), (int)(heavyRangedInvader.LaunchVelocityRange.Y)),
                                            -MathHelper.ToRadians(Random.Next((int)(heavyRangedInvader.AngleRange.X), (int)(heavyRangedInvader.AngleRange.Y))),
                                            0.2f, heavyRangedInvader.RangedDamage);

                                heavyProjectile.YRange = new Vector2(invader.MaxY, invader.MaxY);
                                AddDrawable(heavyProjectile);
                            }
                            break;
                        #endregion

                        #region Stationary Cannon
                        case InvaderType.StationaryCannon:
                            {
                                 HeavyProjectile heavyProjectile = new CannonBall(heavyRangedInvader, CannonBallSprite, SmokeParticle,
                                            new Vector2(heavyRangedInvader.BarrelEnd.X, heavyRangedInvader.BarrelEnd.Y),
                                            Random.Next((int)(heavyRangedInvader.LaunchVelocityRange.X), (int)(heavyRangedInvader.LaunchVelocityRange.Y)),
                                            (float)Math.PI + heavyRangedInvader.CurrentAngle, 0.2f, heavyRangedInvader.RangedDamage, 40,
                                            new Vector2(MathHelper.Clamp(heavyRangedInvader.MaxY + 32, 690, 930), 930));

                                heavyProjectile.YRange = new Vector2(invader.MaxY, invader.MaxY);
                                HeavyProjectileList.Add(heavyProjectile);
                                AddDrawable(heavyProjectile);
                            }
                            break;
                        #endregion
                    }
                }
                #endregion

                #region LIGHT ranged invaders fire
                if (lightRangedInvader != null && 
                    lightRangedInvader.CanAttack == true && 
                    lightRangedInvader.InTowerRange == true)
                {
                    switch (lightRangedInvader.InvaderType)
                    {
                        #region RifleMan
                        case InvaderType.RifleMan:
                            {                               
                                float angle = (float)RandomDouble(lightRangedInvader.AngleRange.X, lightRangedInvader.AngleRange.Y);
                                Vector2 direction = new Vector2((float)Math.Cos(MathHelper.ToRadians(angle)), (float)Math.Sin(MathHelper.ToRadians(angle)));
                                CreateLightProjectile(new MachineGunProjectile(lightRangedInvader.Center, direction, lightRangedInvader.RangedDamage), lightRangedInvader);
                            }
                            break;
                        #endregion
                    }
                }
                #endregion


                //This handles specific invader AI - not generic behaviour
                switch (invader.InvaderType)
                {
                    #region Healer Drone
                    case InvaderType.HealDrone:
                        {
                            HealDrone drone = invader as HealDrone;

                            #region Choose target
                            if (drone.HasTarget == false)
                            {
                                List<Invader> InRangeAndHurt = InvaderList.FindAll(Invader =>
                                    Invader.Position.X > Tower.DestinationRectangle.Right + 350 &&
                                    Invader.CurrentHP < Invader.MaxHP &&
                                    Invader.InvaderType != InvaderType.HealDrone &&
                                    Invader.InvaderType != InvaderType.StationaryCannon &&
                                    Invader.IsBeingHealed == false &&
                                    Invader.IsTargeted == false);

                                if (InRangeAndHurt.Count > 0)
                                {
                                    InRangeAndHurt = InRangeAndHurt.OrderByDescending(Invader => Invader.Position.X).ToList();

                                    drone.HealTarget = InRangeAndHurt[0];
                                    drone.HealTarget.CurrentHealDelay = 0;
                                    drone.HealTarget.HealDelay = 500;
                                    drone.HealTarget.IsTargeted = true;
                                    drone.IsHealing = true;
                                    drone.HasTarget = true;
                                    drone.CurrentHeight = drone.HealTarget.BoundingBox.Min.Y - 150;
                                }
                                else
                                {
                                    #region Idle behaviour
                                    if (drone.BoundingBox.Min.Y < drone.CurrentHeight - 30)
                                    {
                                        drone.Velocity.Y = LerpTime(drone.Velocity.Y, 1 * drone.Speed, 0.1f, gameTime);
                                    }

                                    if (drone.BoundingBox.Min.Y > drone.CurrentHeight + 30)
                                    {
                                        drone.Velocity.Y = LerpTime(drone.Velocity.Y, -1 * drone.Speed, 0.1f, gameTime);
                                    }
                                    #endregion
                                }
                            }
                            #endregion

                            if (drone.HasTarget == true)
                            {
                                #region The target got too close to the tower
                                if (drone.HealTarget.Position.X < Tower.DestinationRectangle.Right + 350)
                                {
                                    drone.HasTarget = false;
                                    drone.IsHealing = false;
                                    drone.HealTarget.IsBeingHealed = false;
                                    drone.HealTarget.IsTargeted = false;

                                    drone.CurrentHeight = (float)RandomDouble(drone.YRange.X, drone.YRange.Y);
                                    drone.Velocity.X = 0;
                                }
                                #endregion

                                #region The drone isn't yet close enough to start healing
                                if (Math.Abs(drone.Center.X - drone.HealTarget.Center.X) > 100)
                                {
                                    drone.Direction = drone.HealTarget.Center - drone.Center;
                                    drone.Direction.Normalize();
                                    drone.Velocity.X = LerpTime(drone.Velocity.X, drone.Direction.X * 5f, 0.05f, gameTime);
                                    drone.Speed = 2.5f;
                                }
                                #endregion

                                #region Orient the drone correctly
                                if (drone.HealTarget != null)
                                {
                                    if (drone.HealTarget.Center.X > drone.Center.X)
                                    {
                                        drone.Orientation = SpriteEffects.FlipHorizontally;
                                    }

                                    if (drone.HealTarget.Center.X < drone.Center.X)
                                    {
                                        drone.Orientation = SpriteEffects.None;
                                    }
                                }
                                #endregion

                                #region If the target reaches full health, disengage healing
                                if (drone.HealTarget.CurrentHP >= drone.HealTarget.MaxHP)
                                {
                                    drone.HealTarget.IsBeingHealed = false;
                                    drone.HealTarget.IsTargeted = false;
                                    drone.HasTarget = false;
                                    drone.IsHealing = false;

                                    drone.CurrentHeight = (float)RandomDouble(drone.YRange.X, drone.YRange.Y);
                                    drone.Velocity.X = 0;
                                }
                                #endregion

                                #region The drone is close enough to the target to start healing
                                if (Vector2.Distance(drone.HealTarget.Center, drone.Center) < 200 &&
                                    drone.HealTarget.CurrentHP < drone.HealTarget.MaxHP)
                                {
                                    drone.HealTarget.IsBeingHealed = true;

                                    #region Actually heal the target
                                    if (drone.HealTarget.CurrentHealDelay >= drone.HealTarget.HealDelay)
                                    {
                                        drone.HealTarget.HealDamage(1);
                                        drone.HealTarget.CurrentHealDelay = 0;
                                    }
                                    #endregion

                                    #region Make sure there's a beam
                                    if (drone.BoltList.Count < 5)
                                    {
                                        Lightning = new LightningBolt(drone, drone.HealTarget, new Color(0, 150, 30, 255), 0.02f, 75f, true);
                                        drone.BoltList.Add(Lightning);
                                        LightningList.AddRange(drone.BoltList);
                                    }
                                    else
                                    {
                                        foreach (LightningBolt bolt in drone.BoltList)
                                        {
                                            bolt.Source = drone.Center + new Vector2(0, 8);
                                            bolt.Destination = drone.HealTarget.Center;
                                        }
                                    }
                                    #endregion
                                }
                                #endregion

                                //THE DRONE MOVEMENT NEEDS TO BE A BIT RANDOM SO THAT TWO DRONES AREN'T MOVING EXACTLY THE SAME
                                //AT THE SAME TIME. MAYBE JUST ADD OR SUBTRACT A TINY RANDOM NUMBER FROM VELOCITIES
                                if (drone.BoundingBox.Min.Y < drone.CurrentHeight - 30)
                                {
                                    drone.Velocity.Y = LerpTime(drone.Velocity.Y, 1 * drone.Speed, 0.1f, gameTime);
                                }

                                if (drone.BoundingBox.Min.Y > drone.CurrentHeight + 30)
                                {
                                    drone.Velocity.Y = LerpTime(drone.Velocity.Y, -1 * drone.Speed, 0.1f, gameTime);
                                }
                            }

                            #region The heal target died
                            if (drone.HasTarget == true)
                            {
                                if (drone.HealTarget == null || drone.HealTarget.Active == false)
                                {
                                    drone.HasTarget = false;
                                    drone.IsHealing = false;
                                    drone.BoltList.Clear();

                                    drone.CurrentHeight = (float)RandomDouble(drone.YRange.X, drone.YRange.Y);
                                    drone.Velocity.X = 0;
                                }
                            }
                            #endregion
                        }
                        break;
                    #endregion

                    #region Jumpman
                    case InvaderType.JumpMan:
                        {
                            JumpMan jumpMan = invader as JumpMan;

                            foreach (Trap trap in TrapList)
                            {
                                //Check if the invader is close enough to the trap in the X axis
                                //Check if the invader is also in-line with the Y axis of the trap
                            }
                        }
                        break;
                    #endregion

                    #region Stationary Cannon
                    case InvaderType.StationaryCannon:
                        {
                            if (heavyRangedInvader.HitObject != null)
                            {
                                if (heavyRangedInvader.HitObject.GetType() == typeof(Shield))
                                {

                                    break;
                                }
                            }
                        }
                        break;
                    #endregion
                }


                #region Remove inactive invaders
                if (invader.Active == false)
                {
                    InvaderList.Remove(invader);
                    DrawableList.Remove(invader);
                }
                #endregion
            }
        }

        public void OnExplosionHappened(object source, ExplosionEventArgs e)
        {
            Explosion explosion = e.Explosion;

            Light light = new Light(new Vector3(explosion.Position.X, explosion.Position.Y, 30), LightTexture)
            {
                Color = LightFlashColor,
                Active = true,
                LightDecay = 80,
                Power = 0.5f,
                Radius = explosion.BlastRadius,
                Range = explosion.BlastRadius,
                MaxTime = 350
            };

            LightList.Add(light);

            #region Determine what effect to create when an invader takes damage from an explosion
            Action<Invader, double> CreateEffect = (Invader invader, double Damage) =>
            {
                Vector2 DirectionToInvader = explosion.Position - invader.Center;

                switch (invader.InvaderType)
                {
                    #region Soldier
                    case InvaderType.Soldier:
                        Emitter BloodEmitter = new Emitter(SplodgeParticle,
                                            invader.Center,
                                            new Vector2(
                                            MathHelper.ToDegrees(-(float)Math.Atan2(-DirectionToInvader.Y, -DirectionToInvader.X)) - (float)RandomDouble(0, 45),
                                            MathHelper.ToDegrees(-(float)Math.Atan2(-DirectionToInvader.Y, -DirectionToInvader.X)) + (float)RandomDouble(0, 45)),
                                            new Vector2((float)(0.1 * Damage), (float)(0.23 * Damage)), new Vector2(80, 130), 0.5f, true, new Vector2(0, 360),
                                            new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.Red, Color.DarkRed,
                                            0.1f, 0.1f, 3, 5, true, new Vector2(invader.MaxY - 8, invader.MaxY + 8), false, 0, true, false,
                                            null, null, null, null, null, true, true, 1000);
                        BloodEmitter.Grow = true;

                        YSortedEmitterList.Add(BloodEmitter);
                        AddDrawable(BloodEmitter);
                        break;
                    #endregion
                }
            };
            #endregion

            //THIS NEEDS TO BE DONE AGAIN. BETTER. "FOREACH TRAP" means that the damage is done repeatedly for each trap in the list
            #region This controls what happens when an explosion happens near the invader
            if (source.GetType().BaseType != typeof(Invader))
            {
                List<Trap> WallList = new List<Trap>();
                WallList = TrapList.FindAll(Trap => Trap.TrapType == TrapType.Wall);

                List<Invader> InRangeInvaders = InvaderList.FindAll(Invader => Vector2.Distance(Invader.Center, explosion.Position) < explosion.BlastRadius).ToList();
                InRangeInvaders = InRangeInvaders.OrderBy(Invader => Vector2.Distance(Invader.Center, explosion.Position)).ToList();

                foreach (Invader invader in InvaderList)
                {
                    if (limit < 5)
                    {
                        //Distance between the explosion and the centre of the invader
                        float InvaderToExpl = Vector2.Distance(invader.Center, explosion.Position);

                        Vector3 RayDirection = new Vector3(invader.Center, 0) - new Vector3(explosion.Position, 0);
                        RayDirection.Normalize();

                        Ray ExplosionRay = new Ray(new Vector3(explosion.Position, 0), RayDirection);

                        #region No walls exist
                        if (WallList.Count < 1 && InvaderToExpl <= explosion.BlastRadius)
                        {
                            double Damage = explosion.Damage - (InvaderToExpl / (2 * explosion.BlastRadius)) * explosion.Damage;
                            invader.CurrentHP -= (float)Damage;

                            limit++;

                            CreateEffect(invader, Damage);
                        }
                        #endregion

                        #region Check if any walls are in the way of the explosion
                        if (WallList.Count > 0 && InvaderToExpl <= explosion.BlastRadius)
                        {
                            foreach (Wall wall in WallList)
                            {
                                if (wall.BoundingBox.Intersects(ExplosionRay) != null)
                                {
                                    float WallToExpl = (float)wall.BoundingBox.Intersects(ExplosionRay);
                                    float InvaderToExplosion = (float)invader.BoundingBox.Intersects(ExplosionRay);
                                }

                                //Check if the wall is further away than the invader - i.e. not between the explosion and invader
                                if (WallList.Any(Wall => Wall.BoundingBox.Intersects(ExplosionRay) > invader.BoundingBox.Intersects(ExplosionRay)) ||
                                    WallList.Any(Wall => Wall.BoundingBox.Intersects(ExplosionRay) == null))
                                {
                                    double Damage = explosion.Damage - (InvaderToExpl / (2 * explosion.BlastRadius)) * explosion.Damage;
                                    invader.CurrentHP -= (float)Damage;
                                    limit++;
                                    CreateEffect(invader, Damage);
                                }
                            }
                        }
                        #endregion
                    }
                }

                limit = 0;
            }
            #endregion

            #region Make shell casings bounce when an explosion happens near them
            //foreach (Particle Casing in ShellCasingList)
            //{
            //    //Vector distance between invader and explosion
            //    float Dist = MathHelper.Distance(Casing.DestinationRectangle.Center.X, explosion.Position.X);

            //    //List of X values between the casing and the explosion      

            //    //[----------dist-----------]
            //    //cas--------wall--------expl
            //    var InvaderToExplosion = Enumerable.Range((int)Casing.DestinationRectangle.Center.X, (int)Dist);

            //    //[----------dist-----------]
            //    //expl-------wall---------cas
            //    var ExplosionToInvader = Enumerable.Range((int)explosion.Position.X, (int)Dist);

            //    List<Trap> TempList = new List<Trap>();
            //    TempList = TrapList.FindAll(Trap => Trap.TrapType == TrapType.Wall);

            //    if (!TempList.Any(Trap => ExplosionToInvader.Contains((int)Trap.Center.X)) &&
            //        !TempList.Any(Trap => InvaderToExplosion.Contains((int)Trap.Center.X)) &&
            //        Dist < explosion.BlastRadius)
            //    {
            //        if (Casing.BouncedOnGround == true)
            //        {
            //            Casing.BouncedOnGround = false;
            //            Casing.CanBounce = true;
            //            Casing.Gravity = 0.2f;
            //            Casing.RotationIncrement = (float)RandomDouble(-6, 6);
            //            Casing.Velocity.Y = (float)RandomDouble(-7, -2);

            //            if (Casing.CurrentPosition.Y >= Casing.MaxY)
            //                Casing.CurrentPosition.Y = Casing.MaxY - 1;

            //            float VelocityX = (explosion.Damage / 100 * (100 - (100 / explosion.BlastRadius) * Dist)) / 20;

            //            if (Casing.CurrentPosition.X < explosion.Position.X)
            //                Casing.Velocity.X = -VelocityX;

            //            if (Casing.CurrentPosition.X >= explosion.Position.X)
            //                Casing.Velocity.X = VelocityX;
            //        }
            //    }
            //}
            #endregion

            #region Make the fire react to nearby explosion
            foreach (Trap trap in TrapList)
            {
                if (trap.TrapType == TrapType.Fire)
                {
                    if (Vector2.Distance(new Vector2(trap.Center.X, trap.DestinationRectangle.Bottom),
                                         explosion.Position) <= explosion.BlastRadius)
                    {
                        float angle1, Dist;
                        Vector2 Vector;

                        Dist = Vector2.Distance(explosion.Position, trap.Center);

                        Vector = trap.Center - explosion.Position;

                        angle1 = MathHelper.ToDegrees(-(float)Math.Atan2(Vector.Y, Vector.X));
                        angle1 = MathHelper.Clamp(angle1, 0, 180);

                        foreach (Emitter emitter in YSortedEmitterList.Where(emit => emit.Tether == trap))
                        {
                            //emitter.AngleRange = new Vector2(angle1, angle1);
                            //emitter.SpeedRange = new Vector2(1, 1.5f);

                            //foreach (Particle particle in emitter.ParticleList)
                            //{
                            //    particle.Angle = (float)RandomDouble(angle1, angle1);
                            //    particle.Speed = (float)RandomDouble(1, 1.5f);
                            //}

                            //emitter.Affect(new Affector()
                            //{
                            //    AngleRange = new Vector2(angle1, angle1),
                            //    SpeedRange = new Vector2(1, 1.5f),
                            //    AffectsParticles = true,
                            //    LerpValue = 0.02f,
                            //    MaxTime = 300,
                            //    CurrentTime = 0
                            //});
                        }
                    }
                }
            }
            #endregion
        }

        #region BUTTON stuff
        private void ButtonsUpdate(GameTime gameTime)
        {
            #region In Game Buttons
            //This places the selected turret type into the right slot on the tower when the tower slot has been clicked
            if (GameState == GameState.Playing)
            {
                foreach (Button towerButton in TowerButtonList)
                {
                    towerButton.Update(CursorPosition, gameTime);
                }

                foreach (Button button in SpecialAbilitiesButtonList)
                {
                    button.Update(CursorPosition, gameTime);
                }

                #region Select Weapon Buttons
                //This makes sure that when the button at the bottom of the screen is clicked, the corresponding trap or turret is actually selected//
                //This will code will need to be added to every time that a new trap/turret is added to the game.
                #region Change the colour of the icon based on its availability
                foreach (CooldownButton button in CooldownButtonList)
                {
                    int ButtonIndex = CooldownButtonList.IndexOf(button);

                    if (CurrentProfile.Buttons[ButtonIndex] != null)
                    {
                        if (Resources >= button.ResourceCost)
                        {
                            button.IconColor = Color.White;
                        }
                        else
                        {
                            button.IconColor = Color.Gray;
                        }
                    }
                }
                #endregion

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
                }
                #endregion

               return;
            }
            #endregion

            #region Menu Buttons
            if (GameState != GameState.Playing)
            {
                if (DialogVisible == false)
                {
                    switch (GameState)
                    {
                        #region Main Menu
                        case GameState.Menu:
                            {
                                foreach (Button button in MainMenuButtonList)
                                {
                                    button.Update(CursorPosition, gameTime);

                                    if (button.CurrentButtonState == ButtonSpriteState.Hover)
                                    {
                                        button.NextPosition.X = 0;
                                    }

                                    if (button.CurrentButtonState == ButtonSpriteState.Released)
                                    {
                                        button.NextPosition.X = -50;
                                    }
                                }
                            }
                            break;
                        #endregion

                        #region Pause Menu
                        case GameState.Paused:
                            {
                                foreach (Button button in PauseButtonList)
                                {
                                    button.Update(CursorPosition, gameTime);
                                }
                            }
                            break;
                        #endregion

                        #region Profile Management
                        case GameState.ProfileManagement:
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
                                        MoveTurretsRight.Update(CursorPosition, gameTime);
                                        MoveTurretsLeft.Update(CursorPosition, gameTime);
                                        MoveTrapsRight.Update(CursorPosition, gameTime);
                                        MoveTrapsLeft.Update(CursorPosition, gameTime);

                                        #region Select turret boxes
                                        foreach (WeaponBox turretBox in SelectTurretList)
                                        {
                                            turretBox.Update(gameTime, CursorPosition);

                                            if (turretBox.DestinationRectangle.Left > MoveTurretsLeft.DestinationRectangle.Right &&
                                                turretBox.DestinationRectangle.Right < MoveTurretsRight.DestinationRectangle.Left)
                                            {
                                                turretBox.Visible = true;
                                            }
                                            else
                                            {
                                                turretBox.Visible = false;
                                            }
                                        }
                                        #endregion

                                        #region Select trap boxes
                                        foreach (WeaponBox trapBox in SelectTrapList)
                                        {
                                            trapBox.Update(gameTime, CursorPosition);

                                            if (trapBox.DestinationRectangle.Left > MoveTrapsLeft.DestinationRectangle.Right &&
                                                trapBox.DestinationRectangle.Right < MoveTrapsRight.DestinationRectangle.Left)
                                            {
                                                trapBox.Visible = true;
                                            }
                                            else
                                            {
                                                trapBox.Visible = false;
                                            }
                                        }
                                        #endregion


                                        #region Place Weapon Buttons
                                        foreach (Button button in PlaceWeaponList)
                                        {
                                            button.Update(CursorPosition, gameTime);
                                        }
                                        #endregion
                                        break;
                                    #endregion

                                    case ProfileManagementState.Stats:

                                        break;

                                    case ProfileManagementState.Upgrades:

                                        break;
                                }
                            }
                            break;
                        #endregion

                        #region Profile Select
                        case GameState.ProfileSelect:
                            {
                                ProfileBackButton.Update(CursorPosition, gameTime);

                                foreach (Button button in ProfileButtonList)
                                {
                                    button.Update(CursorPosition, gameTime);
                                }

                                foreach (Button button in ProfileDeleteList)
                                {
                                    button.Update(CursorPosition, gameTime);
                                }

                                if (ProfileBackButton.CurrentButtonState == ButtonSpriteState.Hover)
                                {
                                    ProfileBackButton.NextPosition.X = 1920 - 450;
                                }

                                if (ProfileBackButton.CurrentButtonState == ButtonSpriteState.Released)
                                {
                                    ProfileBackButton.NextPosition.X = 1920 - 400;
                                }
                            }
                            break;
                        #endregion

                        #region Options Menu
                        case GameState.Options:
                            {
                                OptionsBack.Update(CursorPosition, gameTime);

                                OptionsSFXUp.Update(CursorPosition, gameTime);
                                OptionsSFXDown.Update(CursorPosition, gameTime);

                                OptionsMusicUp.Update(CursorPosition, gameTime);
                                OptionsMusicDown.Update(CursorPosition, gameTime);

                                FullscreenToggle.Update(CursorPosition, gameTime);
                                BloodEffectsToggle.Update(CursorPosition, gameTime);

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
                            break;
                        #endregion

                        #region Get Name
                        case GameState.GettingName:
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

                                if (CurrentKeyboardState.IsKeyUp(Keys.Enter) && PreviousKeyboardState.IsKeyDown(Keys.Enter))
                                {
                                    GetNameOK.CreateButtonClick(MouseButton.Left);
                                }
                            }
                            break;
                        #endregion

                        #region Upgrades Menu

                        #endregion

                        #region Victory Screen
                        case GameState.Victory:
                            {
                                if (Victory == true)
                                    VictoryContinue.Update(CursorPosition, gameTime);
                                else
                                    VictoryRetry.Update(CursorPosition, gameTime);
                            }
                            break;
                        #endregion

                        #region Loading Screen
                        case TowerDefensePrototype.GameState.Loading:
                            {
                                if (AllLoaded == true)
                                {
                                    IsLoading = false;
                                    MenuMusicInstance.Stop();
                                    GameState = GameState.Playing;
                                }
                            }
                            break;
                        #endregion
                    }
                }
                else
                {
                    #region Dialog Boxes
                    if (ExitDialog != null)
                    {
                        ExitDialog.Update(CursorPosition, gameTime);
                    }

                    if (DeleteProfileDialog != null)
                    {
                        DeleteProfileDialog.Update(CursorPosition, gameTime);
                    }

                    if (ProfileMenuDialog != null)
                    {
                        ProfileMenuDialog.Update(CursorPosition, gameTime);
                    }

                    if (MainMenuDialog != null)
                    {
                        MainMenuDialog.Update(CursorPosition, gameTime);
                    }

                    if (NoWeaponsDialog != null)
                    {
                        NoWeaponsDialog.Update(CursorPosition, gameTime);
                    }

                    if (NameLengthDialog != null)
                    {
                        NameLengthDialog.Update(CursorPosition, gameTime);
                    }
                    #endregion
                }

                return;
            }
            #endregion
        }

        public void OnButtonClick(object source, ButtonClickEventArgs e)
        {
            if (this.IsActive == true)
            {
                int Index;
                Button button = source as Button;

                //The button was left-clicked
                if (e.ClickedButton == MouseButton.Left)
                {
                    switch (GameState)
                    {
                        case GameState.Playing:
                            {
                                #region Tower Buttons
                                if (TowerButtonList.Contains(button))
                                {
                                    Index = TowerButtonList.IndexOf(button);

                                    #region Create an effect under the turret as it's placed
                                    if (SelectedTurret != null)
                                    {
                                        Emitter Sparks1 = new Emitter(RoundSparkParticle, button.CurrentPosition,
                                            new Vector2(0, 180), new Vector2(3, 6), new Vector2(80, 480), 1f, true, new Vector2(0, 0),
                                            new Vector2(0, 0), new Vector2(0.25f, 0.25f), Color.Blue, Color.DeepSkyBlue, -0.5f, 0.1f, 1, 30,
                                            false, new Vector2(0, 1080), false, null, false, false, null, null, null, null, new Vector2(0.3f, 0.1f), null, null, null, null, null);
                                        Sparks1.Grow = true;

                                        Emitter Sparks2 = new Emitter(RoundSparkParticle, button.CurrentPosition,
                                             new Vector2(180, 360), new Vector2(3, 6), new Vector2(80, 480), 1f, true, new Vector2(0, 0),
                                             new Vector2(0, 0), new Vector2(0.25f, 0.25f), Color.Blue, Color.DeepSkyBlue, 0.5f, 0.1f, 1, 30,
                                             false, new Vector2(0, 1080), false, null, false, false, null, null, null, null, new Vector2(0.3f, 0.1f), null, null, null, null, null);
                                        Sparks2.Grow = true;

                                        Sparks1.BlendState = BlendState.Additive;
                                        Sparks2.BlendState = BlendState.Additive;
                                        
                                        YSortedEmitterList.Add(Sparks1);
                                        YSortedEmitterList.Add(Sparks2);

                                        AddDrawable(Sparks1, Sparks2);
                                    }
                                    #endregion

                                    if (SelectedTurret != null)
                                    {
                                        int resourceCost = TurretCost(SelectedTurret.Value);

                                        if (Resources >= resourceCost)
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
                                            newTurret.TurretClickHappened += OnTurretClick;
                                            TurretList[Index] = newTurret;
                                            AddDrawable(newTurret);

                                            Resources -= resourceCost;

                                            CurrentTurret = newTurret;
                                            SelectedTurret = null;
                                            TurretList[Index].Selected = true;
                                        }
                                    }
                                    return;
                                }
                                #endregion

                                #region Cooldown Buttons
                                if (CooldownButtonList.Contains(source as CooldownButton))
                                {
                                    CooldownButton cooldownButton = source as CooldownButton;
                                    if (cooldownButton.CoolingDown == false)
                                    {
                                        ClearTurretSelect();
                                        Index = CooldownButtonList.IndexOf(cooldownButton);

                                        #region Check the layout of the buttons from the player profile
                                        Action CheckLayout = new Action(() =>
                                        {
                                            if (Index <= CurrentProfile.Buttons.Count - 1)
                                            {
                                                #region Handle Trap selection
                                                if (CurrentProfile.Buttons[Index] != null &&
                                                    CurrentProfile.Buttons[Index].CurrentTurret == null)
                                                {
                                                    if (Resources >= TrapCost(CurrentProfile.Buttons[Index].CurrentTrap.Value) &&
                                                        Tower.CurrentPowerUnits > TrapPowerUsage(CurrentProfile.Buttons[Index].CurrentTrap.Value))
                                                    {
                                                        SelectedTrap = CurrentProfile.Buttons[Index].CurrentTrap.Value;
                                                    }
                                                }
                                                #endregion

                                                #region Handle Turret selection
                                                if (CurrentProfile.Buttons[Index] != null &&
                                                    CurrentProfile.Buttons[Index].CurrentTrap == null)
                                                {
                                                    if (Resources >= TurretCost(CurrentProfile.Buttons[Index].CurrentTurret.Value))
                                                    {
                                                        SelectedTurret = CurrentProfile.Buttons[Index].CurrentTurret.Value;
                                                    }
                                                }
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

                                    return;
                                }
                                #endregion

                                #region Start Wave Button
                                if (button == StartWaveButton && button != null)
                                {
                                    StartWave = true;
                                    CurrentWaveTime = 0;
                                    CurrentInvaderIndex = 0;
                                    CurrentInvaderTime = 0;

                                    if (CurrentWaveIndex > 0)
                                        CurrentWaveIndex++;

                                    //WaveCountDown = new WaveCountDown(10)
                                    //{
                                    //    Font = RobotoBold40_2,
                                    //    Color = Color.White,
                                    //    Origin = new Vector2(1920 / 2, 100)
                                    //};

                                    StartWaveButton = null;
                                    return;
                                }
                                #endregion
                            }
                            break;

                        #region Main Menu
                        case GameState.Menu:
                            {
                                Index = MainMenuButtonList.IndexOf(button);

                                switch (Index)
                                {
                                    #region Play
                                    case 0:
                                        button.ResetState();
                                        ProfileBackButton.CurrentPosition.X = 1920 + 300;
                                        GameState = GameState.ProfileSelect;
                                        SetProfileNames();
                                        break;
                                    #endregion

                                    #region Tutorial
                                    case 1:
                                        Tutorial = true;
                                        UnloadGameContent();
                                        Tower.CurrentHP = 100;
                                        Tower.MaxHP = 100;
                                        Tower.Shield.CurrentShield = 20;
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
                                        LoadingThread.IsBackground = true;
                                        LoadingThread.Start();
                                        IsLoading = false;
                                        break;
                                    #endregion

                                    #region Options
                                    case 2:
                                        button.ResetState();
                                        GameState = GameState.Options;
                                        MenuSFXVolume = CurrentSettings.SFXVolume * 10;
                                        MenuMusicVolume = CurrentSettings.MusicVolume * 10;
                                        break;
                                    #endregion

                                    #region Credits
                                    case 3:

                                        break;
                                    #endregion

                                    #region Exit
                                    case 4:
                                        button.ResetState();
                                        ExitDialog = new DialogBox(DialogBox, ShortButtonLeftSprite, ShortButtonRightSprite, RobotoRegular20_0, new Vector2(1920 / 2, 1080 / 2), "exit", "Do you want to exit?", "cancel");
                                        ExitDialog.Initialize(OnButtonClick);
                                        CurrentDialogBox = ExitDialog;
                                        DialogVisible = true;
                                        break;
                                    #endregion
                                }
                            }
                            break;
                        #endregion

                        #region Pause Menu
                        case GameState.Paused:
                            {
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
                                        MainMenuDialog.Initialize(OnButtonClick);
                                        CurrentDialogBox = MainMenuDialog;
                                        DialogVisible = true;
                                        break;

                                    case 3:
                                        button.ResetState();
                                        ProfileMenuDialog = new DialogBox(DialogBox, ShortButtonLeftSprite, ShortButtonRightSprite, RobotoRegular20_0, new Vector2(1920 / 2, 1080 / 2), "yes", "Are you sure you want to return to your profile menu? All progress will be lost.", "no");
                                        ProfileMenuDialog.Initialize(OnButtonClick);
                                        CurrentDialogBox = ProfileMenuDialog;
                                        DialogVisible = true;
                                        break;

                                    case 4:
                                        button.ResetState();
                                        ExitDialog = new DialogBox(DialogBox, ShortButtonLeftSprite, ShortButtonRightSprite, RobotoRegular20_0, new Vector2(1920 / 2, 1080 / 2), "exit", "Do you want to exit? All progress will be lost.", "cancel");
                                        ExitDialog.Initialize(OnButtonClick);
                                        CurrentDialogBox = ExitDialog;
                                        DialogVisible = true;
                                        break;
                                }
                            }
                            break;
                        #endregion

                        #region Profile Management
                        case GameState.ProfileManagement:
                            {
                                #region Play button
                                if (button == ProfileManagementPlay)
                                {
                                    ProfileManagementPlay.ResetState();
                                    MenuClick.Play();

                                    if (CurrentProfile != null)
                                    {
                                        if (CurrentProfile.Buttons.All(Weapon => Weapon == null))
                                        {
                                            NoWeaponsDialog = new DialogBox(DialogBox, ShortButtonLeftSprite, ShortButtonRightSprite, RobotoRegular20_0, new Vector2(1920 / 2, 1080 / 2), "OK", "You have no weapons to use!", "");
                                            NoWeaponsDialog.Initialize(OnButtonClick);
                                            CurrentDialogBox = NoWeaponsDialog;
                                            DialogVisible = true;
                                        }
                                        else
                                        {
                                            UnloadGameContent();
                                            Tower.CurrentHP = Tower.MaxHP;
                                            Tower.Shield.CurrentShield = Tower.Shield.MaxShield;
                                            LevelNumber = CurrentProfile.LevelNumber;
                                            LoadLevel(LevelNumber);
                                            //LoadUpgrades();
                                            StorageDevice.BeginShowSelector(this.SaveProfile, null);
                                            GameState = GameState.Loading;
                                            LoadingThread = new Thread(LoadGameContent);
                                            LoadingThread.Name = "Loading Content Thread";

                                            //Changed this to run in background now. Keep an eye on it.
                                            //Did that in an attempt to stop the loading screen giving an "Unexpected Error"
                                            //LoadingThread.IsBackground = false;
                                            LoadingThread.Start();

                                            IsLoading = false;
                                        }
                                    }
                                }
                                #endregion

                                #region Back Button
                                if (button == ProfileManagementBack)
                                {
                                    MenuClick.Play();
                                    ProfileBackButton.CurrentPosition.X = 1920 - 150;
                                    SetProfileNames();
                                    SelectedTrap = null;
                                    SelectedTurret = null;
                                    StorageDevice.BeginShowSelector(this.SaveProfile, null);
                                    GameState = GameState.ProfileSelect;
                                    ProfileManagementBack.ResetState();
                                }
                                #endregion

                                switch (ProfileManagementState)
                                {
                                    case ProfileManagementState.Loadout:
                                        {
                                            #region Select Turret Boxes
                                            if (SelectTurretList.Contains(source as WeaponBox))
                                            {
                                                Index = SelectTurretList.IndexOf(source as WeaponBox);
                                                WeaponBox turretBox = SelectTurretList[Index];

                                                if (turretBox.Visible == true)
                                                {
                                                    string WeaponName = turretBox.ContainsTurret.ToString();
                                                    var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

                                                    if ((bool)Available == true)
                                                        SelectedTurret = turretBox.ContainsTurret;
                                                }
                                            }
                                            #endregion

                                            #region Select Trap Boxes
                                            if (SelectTrapList.Contains(source as WeaponBox))
                                            {
                                                Index = SelectTrapList.IndexOf(source as WeaponBox);
                                                WeaponBox trapBox = SelectTrapList[Index];

                                                if (trapBox.Visible == true)
                                                {
                                                    string WeaponName = trapBox.ContainsTrap.ToString();
                                                    var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

                                                    if ((bool)Available == true)
                                                        SelectedTrap = trapBox.ContainsTrap;
                                                }
                                            }
                                            #endregion


                                            #region Place Weapon Buttons
                                            if (PlaceWeaponList.Contains(button))
                                            {
                                                Index = PlaceWeaponList.IndexOf(button);

                                                if (SelectedTrap == null &&
                                                    SelectedTurret == null &&
                                                    CurrentProfile.Buttons[PlaceWeaponList.IndexOf(button)] != null)
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
                                            #endregion


                                            #region Move Turrets Right
                                            if (button == MoveTurretsRight &&
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

                                            #region Move Turrets Left
                                            if (button == MoveTurretsLeft &&
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


                                            #region Move Traps Right
                                            if (button == MoveTrapsRight &&
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

                                            #region Move Traps Left
                                            if (button == MoveTrapsLeft &&
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
                                        }
                                        break;
                                }
                            }
                            break;
                        #endregion

                        #region Profile Select
                        case GameState.ProfileSelect:
                            {
                                #region Select buttons
                                if (ProfileButtonList.Contains(button))
                                {
                                    Index = ProfileButtonList.IndexOf(button);

                                    switch (Index)
                                    {
                                        #region The profiles the player can select
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
                                        #endregion
                                    }
                                }
                                #endregion

                                #region Delete buttons
                                if (ProfileDeleteList.Contains(button))
                                {
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
                                #endregion

                                #region Back Button
                                if (button == ProfileBackButton)
                                {
                                    ProfileBackButton.ResetState();

                                    foreach (Button mainMenuButton in MainMenuButtonList)
                                    {
                                        mainMenuButton.NextPosition.X = 0;
                                        mainMenuButton.CurrentPosition.X = -300;
                                    }

                                    MenuClick.Play();
                                    GameState = GameState.Menu;
                                }
                                break;
                                #endregion
                            }
                        #endregion

                        #region Options Menu
                        case GameState.Options:
                            {
                                #region Back
                                if (button == OptionsBack)
                                {
                                    OptionsBack.ResetState();
                                    MenuClick.Play();
                                    //graphics.IsFullScreen = FullscreenToggle.ValueState;
                                    //CurrentSettings.FullScreen = false;
                                    //SetUpGameWindow();
                                    //graphics.ApplyChanges();
                                    SaveSettings();
                                    GameState = GameState.Menu;
                                }
                                #endregion

                                #region Sound Effects Up
                                if (button == OptionsSFXUp)
                                {
                                    if (MenuSFXVolume < 10)
                                    {
                                        MenuSFXVolume++;
                                        CurrentSettings.SFXVolume = MenuSFXVolume / 10;
                                        SoundEffect.MasterVolume = CurrentSettings.SFXVolume;
                                        MenuClick.Play();
                                    }
                                }
                                #endregion

                                #region Sound Effects Down
                                if (button == OptionsSFXDown)
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

                                #region Music Up
                                if (button == OptionsMusicUp)
                                {
                                    if (MenuMusicVolume < 10)
                                    {
                                        MenuMusicVolume++;
                                        CurrentSettings.MusicVolume = MenuMusicVolume / 10;
                                        MediaPlayer.Volume = CurrentSettings.MusicVolume;
                                        MenuClick.Play();
                                    }
                                }
                                #endregion

                                #region Music Down
                                if (button == OptionsMusicDown)
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

                                //if (button == FullscreenToggle)
                                //{

                                //}
                            }
                            break;
                        #endregion

                        #region Victory Screen
                        case GameState.Victory:
                            {
                                if (button == VictoryContinue)
                                {
                                    CurrentProfile.LevelNumber++;
                                    StorageDevice.BeginShowSelector(this.SaveProfile, null);
                                    GameState = GameState.ProfileManagement;
                                }
                            }
                            break;
                        #endregion

                        #region Get Name
                        case GameState.GettingName:
                            {
                                #region Back Button
                                if (button == GetNameBack)
                                {
                                    ProfileBackButton.CurrentPosition.X = 1920 - 150;
                                    GetNameBack.ResetState();
                                    MenuClick.Play();
                                    NameInput.TypePosition = 0;
                                    NameInput.RealString = "";
                                    GameState = GameState.ProfileSelect;
                                }
                                #endregion

                                #region OK Button
                                if (button == GetNameOK)
                                {
                                    GetNameOK.ResetState();

                                    if (NameInput.RealString.Length < 3)
                                    {
                                        NameLengthDialog = new DialogBox(DialogBox, ShortButtonLeftSprite, ShortButtonRightSprite, RobotoRegular20_0, new Vector2(1920 / 2, 1080 / 2), "OK", "Your name is too short.", "");
                                        NameLengthDialog.Initialize(OnButtonClick);
                                        DialogVisible = true;
                                    }

                                    if (NameInput.RealString.All(Char => Char == ' '))
                                    {
                                        NameLengthDialog = new DialogBox(DialogBox, ShortButtonLeftSprite, ShortButtonRightSprite, RobotoRegular20_0, new Vector2(1920 / 2, 1080 / 2), "OK", "Your name cannot be blank.", "");
                                        NameLengthDialog.Initialize(OnButtonClick);
                                        DialogVisible = true;
                                    }

                                    if (NameInput.RealString.Length >= 3 && !NameInput.RealString.All(Char => Char == ' '))
                                    {
                                        GetNameOK.ResetState();
                                        MenuClick.Play();
                                        AddNewProfile();
                                    }
                                }
                                #endregion
                            }
                            break;
                        #endregion
                    }

                    #region Dialog Boxes
                    if (DialogVisible == true && CurrentDialogBox != null)
                    {
                        if (button == CurrentDialogBox.LeftButton)
                        {
                            if (CurrentDialogBox == ExitDialog)
                            {
                                if (GameState == GameState.Paused)
                                    StorageDevice.BeginShowSelector(this.SaveProfile, null);

                                this.Exit();
                            }

                            if (CurrentDialogBox == DeleteProfileDialog)
                            {
                                MenuClick.Play();
                                StorageDevice.BeginShowSelector(this.DeleteProfile, null);
                                SetProfileNames();
                                DialogVisible = false;
                                DeleteProfileDialog = null;
                                return;
                            }

                            if (CurrentDialogBox == ProfileMenuDialog)
                            {
                                MenuClick.Play();
                                StorageDevice.BeginShowSelector(this.SaveProfile, null);
                                DialogVisible = false;
                                ProfileMenuDialog = null;
                                ResetUpgrades();
                                UnloadGameContent();
                                GameState = GameState.ProfileManagement;
                                return;
                            }

                            if (CurrentDialogBox == MainMenuDialog)
                            {
                                MenuClick.Play();
                                StorageDevice.BeginShowSelector(this.SaveProfile, null);
                                CurrentProfile = null;
                                ResetUpgrades();
                                UnloadGameContent();
                                DialogVisible = false;
                                MainMenuDialog = null;
                                GameState = GameState.Menu;
                                return;
                            }

                            if (CurrentDialogBox == NoWeaponsDialog)
                            {
                                MenuClick.Play();
                                DialogVisible = false;
                                NoWeaponsDialog = null;
                            }

                            if (CurrentDialogBox == NameLengthDialog)
                            {
                                MenuClick.Play();
                                DialogVisible = false;
                                NameLengthDialog = null;
                            }
                        }

                        if (button == CurrentDialogBox.RightButton)
                        {
                            if (CurrentDialogBox == ExitDialog)
                            {
                                MenuClick.Play();
                                DialogVisible = false;
                                ExitDialog = null;
                            }

                            if (CurrentDialogBox == DeleteProfileDialog)
                            {
                                MenuClick.Play();
                                DialogVisible = false;
                                DeleteProfileDialog = null;
                            }

                            if (CurrentDialogBox == ProfileMenuDialog)
                            {
                                MenuClick.Play();
                                DialogVisible = false;
                                ProfileMenuDialog = null;
                            }

                            if (CurrentDialogBox == MainMenuDialog)
                            {
                                MenuClick.Play();
                                DialogVisible = false;
                                MainMenuDialog = null;
                            }
                        }
                    }
                    #endregion
                }

                //The button was right-clicked
                if (e.ClickedButton == MouseButton.Right && button.CanBeRightClicked == true)
                {
                    switch (GameState)
                    {
                        #region Profile Management
                        case GameState.ProfileManagement:
                            switch (ProfileManagementState)
                            {
                                case ProfileManagementState.Loadout:
                                    #region Place Weapon Buttons - Removal
                                    CurrentProfile.Buttons[PlaceWeaponList.IndexOf(button)] = null;
                                    button.IconTexture = null;
                                    button.Initialize(OnButtonClick);
                                    #endregion
                                    break;
                            }
                            break;
                        #endregion
                    }
                }
            }
        }
        #endregion

        #region PROJECTILE stuff
        private void HeavyProjectileUpdate(GameTime gameTime)
        {
            for (int i = 0; i < HeavyProjectileList.Count; i++)
            {
                HeavyProjectile heavyProjectile = HeavyProjectileList[i];
                TimerHeavyProjectile timedHeavyProjectile = heavyProjectile as TimerHeavyProjectile;

                heavyProjectile.Update(gameTime);

                #region Remove the projectile
                if (heavyProjectile.Active == false &&
                    heavyProjectile.EmitterList.All(Emitter => Emitter.AddMore == false && Emitter.ParticleList.Count == 0))
                {
                    DrawableList.Remove(heavyProjectile);
                    HeavyProjectileList.Remove(heavyProjectile);
                    return;
                }
                #endregion

                #region Regular Heavy Projectile
                if (heavyProjectile.Active == true && timedHeavyProjectile == null)
                {
                    //Traps should only be hit by projectiles with a similar DrawDepth
                    //Projectiles needs to have shadows drawn on the ground too, to prevent confusion
                    #region TRAP was hit
                    if (TrapList.Any(Trap => Trap.BoundingBox.Intersects(heavyProjectile.BoundingBox) &&
                        Trap.Solid == true))
                    {
                        Trap trap = TrapList.Find(Trap => Trap.BoundingBox.Intersects(heavyProjectile.BoundingBox) && Trap.Solid == true);
                        CreateHeavyProjectileCollision(heavyProjectile, heavyProjectile.SourceObject, trap);
                        return;
                    }
                    #endregion

                    #region GROUND was hit
                    if (heavyProjectile.Position.Y > heavyProjectile.MaxY && heavyProjectile.Active == true)
                    {
                        CreateHeavyProjectileCollision(heavyProjectile, heavyProjectile.SourceObject, Ground);
                        return;
                    }
                    #endregion

                    if (heavyProjectile.SourceObject.GetType().BaseType == typeof(HeavyRangedInvader))
                    {
                        #region TOWER was hit
                        if (heavyProjectile.BoundingBox.Intersects(Tower.BoundingBox))
                        {
                            CreateHeavyProjectileCollision(heavyProjectile, heavyProjectile.SourceObject, Tower);
                            return;
                        }
                        #endregion

                        #region TURRET was hit
                        if (TurretList.Any(Turret => Turret != null && Turret.BoundingBox.Intersects(heavyProjectile.BoundingBox)))
                        {
                            Turret turret = TurretList.Find(Turret => Turret.BoundingBox.Intersects(heavyProjectile.BoundingBox));
                            CreateHeavyProjectileCollision(heavyProjectile, heavyProjectile.SourceObject, turret);
                            return;
                        }
                        #endregion

                        #region SHIELD was hit
                        if (heavyProjectile.BoundingBox.Intersects(Tower.Shield.ShieldBoundingSphere) && Tower.Shield.ShieldOn == true)
                        {
                            CreateHeavyProjectileCollision(heavyProjectile, heavyProjectile.SourceObject, Tower.Shield);
                            return;
                        }
                        #endregion
                    }

                    if (heavyProjectile.SourceObject.GetType().BaseType == typeof(Turret))
                    {
                        #region INVADER was hit
                        if (InvaderList.Any(Invader => Invader.Solid == true && Invader.BoundingBox.Intersects(heavyProjectile.BoundingBox)))
                        {
                            Invader invader = InvaderList.Find(Invader => Invader.Solid == true && Invader.BoundingBox.Intersects(heavyProjectile.BoundingBox));
                            CreateHeavyProjectileCollision(heavyProjectile, heavyProjectile.SourceObject, invader);
                            return;
                        }
                        #endregion
                    }

                    if (!ScreenDrawRectangle.Contains(VectorToPoint(heavyProjectile.Center)))
                    {
                        CreateHeavyProjectileCollision(heavyProjectile, heavyProjectile.SourceObject, ScreenRectangle);
                        heavyProjectile.Active = false;
                        heavyProjectile.EmitterList.All(Emitter => Emitter.AddMore = false);
                        return;
                    }
                }
                #endregion

                #region Timed Heavy Projectile
                if (heavyProjectile.Active == true && timedHeavyProjectile != null)
                {
                    #region Deactivate the Projectile
                    Action DeactivateProjectile = new Action(() =>
                    {
                        foreach (Emitter emitter in timedHeavyProjectile.EmitterList)
                        {
                            emitter.AddMore = false;
                        }

                        timedHeavyProjectile.Active = false;
                        timedHeavyProjectile.Velocity = Vector2.Zero;
                    });
                    #endregion

                    timedHeavyProjectile.Update(gameTime);

                    switch (timedHeavyProjectile.HeavyProjectileType)
                    {
                        #region Grenade
                        case HeavyProjectileType.Grenade:
                            {
                                if (timedHeavyProjectile.Node1.Velocity == new Vector2(0, 0) &&
                                    timedHeavyProjectile.Node2.Velocity == new Vector2(0, 0) &&
                                    timedHeavyProjectile.Node1.CurrentPosition.Y >= timedHeavyProjectile.MaxY &&
                                     timedHeavyProjectile.Node2.CurrentPosition.Y >= timedHeavyProjectile.MaxY)
                                {
                                    timedHeavyProjectile.EmitterList[0].AngleRange = new Vector2(0, 180);
                                    timedHeavyProjectile.EmitterList[0].SpeedRange = new Vector2(0.25f, 0.5f);

                                    if (timedHeavyProjectile.CurrentTime >= timedHeavyProjectile.MaxTime - 500)
                                        timedHeavyProjectile.EmitterList[0].AddMore = false;
                                }
                            }
                            break;
                        #endregion

                        #region Gas Grenade
                        case HeavyProjectileType.GasGrenade:
                            if (timedHeavyProjectile.Node1.Velocity == new Vector2(0, 0) &&
                                timedHeavyProjectile.Node2.Velocity == new Vector2(0, 0) &&
                                timedHeavyProjectile.Node1.CurrentPosition.Y >= timedHeavyProjectile.MaxY &&
                                 timedHeavyProjectile.Node2.CurrentPosition.Y >= timedHeavyProjectile.MaxY)
                            {
                                timedHeavyProjectile.EmitterList[0].AngleRange = new Vector2(0, 180);
                                timedHeavyProjectile.EmitterList[0].SpeedRange = new Vector2(0.25f, 0.5f);

                                if (timedHeavyProjectile.CurrentTime >= timedHeavyProjectile.MaxTime - 500)
                                    timedHeavyProjectile.EmitterList[0].AddMore = false;
                            }
                            break;
                        #endregion
                    }

                    #region This controls what happens when a TimedProjectile intersects the ground
                    if (timedHeavyProjectile.Position.Y > timedHeavyProjectile.MaxY && timedHeavyProjectile.Active == true)
                    {
                        switch (timedHeavyProjectile.HeavyProjectileType)
                        {
                            #region Cluster Bomb Shell
                            case HeavyProjectileType.ClusterBombShell:
                                {
                                    Emitter ExplosionEmitter = new Emitter(SplodgeParticle, new Vector2(timedHeavyProjectile.Position.X, timedHeavyProjectile.MaxY),
                                        new Vector2(0, 180), new Vector2(1, 4), new Vector2(10, 30), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                        new Vector2(0.02f, 0.06f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 0.2f, 20, 10, true, new Vector2(timedHeavyProjectile.MaxY + 8, timedHeavyProjectile.MaxY + 8));                                    

                                    YSortedEmitterList.Add(ExplosionEmitter);
                                    AddDrawable(ExplosionEmitter);
                                    DeactivateProjectile();
                                }
                                break;
                            #endregion

                            #region Grenade
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
                            #endregion
                        }
                    }
                    #endregion

                    #region This controls what happens when a TimedProjectile hits a wall
                    if (TrapList.Any(Trap => Trap.BoundingBox.Intersects(timedHeavyProjectile.BoundingBox) &&
                        Trap.TrapType == TrapType.Wall))
                    {
                        int index = TrapList.IndexOf(TrapList.First(Trap => Trap.BoundingBox.Intersects(timedHeavyProjectile.BoundingBox)));

                        Trap trap = TrapList[index];

                        if (timedHeavyProjectile.Active == true)
                        {
                            switch (timedHeavyProjectile.HeavyProjectileType)
                            {
                                case HeavyProjectileType.Grenade:
                                    //Make grenade bounce off of walls
                                    break;
                            }
                        }
                    }
                    #endregion

                    #region What happens when a TimedProjectile runs out of time
                    if (timedHeavyProjectile.Detonated == true && timedHeavyProjectile.Active == true)
                    {
                        switch (timedHeavyProjectile.HeavyProjectileType)
                        {
                            #region Cluster bomb shell
                            case HeavyProjectileType.ClusterBombShell:
                                {
                                    Emitter ExplosionEmitter = new Emitter(ExplosionParticle,
                                            new Vector2(timedHeavyProjectile.Position.X, timedHeavyProjectile.Position.Y),
                                            new Vector2(0, 180), new Vector2(1, 5), new Vector2(1, 20), 0.01f, true, new Vector2(0, 360),
                                            new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 7, false,
                                            new Vector2(0, 1080), true);

                                    YSortedEmitterList.Add(ExplosionEmitter);
                                    AddDrawable(ExplosionEmitter);

                                    AddCluster(timedHeavyProjectile.SourceObject, 5, timedHeavyProjectile.Position, new Vector2(0, 360), HeavyProjectileType.ClusterBomb, timedHeavyProjectile.YRange, 2f);
                                    timedHeavyProjectile.Active = false;
                                }
                                break;
                            #endregion

                            #region Grenade
                            case HeavyProjectileType.Grenade:
                                {
                                    Explosion newExplosion = new Explosion(timedHeavyProjectile.Position, 30, 10);
                                    CreateExplosion(newExplosion, timedHeavyProjectile);

                                    Emitter ExplosionEmitter = new Emitter(ExplosionParticle,
                                            new Vector2(timedHeavyProjectile.Rod.Center.X, timedHeavyProjectile.Position.Y),
                                            new Vector2(20, 160), new Vector2(6, 14), new Vector2(5, 7), 1f, true, new Vector2(0, 360),
                                            new Vector2(0, 0), new Vector2(3, 4), Color.Lerp(ExplosionColor, Color.Transparent, 0.5f),
                                            Color.Lerp(ExplosionColor2, Color.Transparent, 0.5f), 0.0f, 0.1f, 1, 1, false,
                                            new Vector2(timedHeavyProjectile.MaxY, timedHeavyProjectile.MaxY + 8), true, timedHeavyProjectile.MaxY / 1080);
                                    YSortedEmitterList.Add(ExplosionEmitter);

                                    Emitter ExplosionEmitter2 = new Emitter(ExplosionParticle,
                                            new Vector2(timedHeavyProjectile.Rod.Center.X, timedHeavyProjectile.Position.Y),
                                            new Vector2(70, 110), new Vector2(6, 12), new Vector2(7, 18), 1f, true, new Vector2(0, 360),
                                            new Vector2(0, 0), new Vector2(3, 4), Color.Lerp(ExplosionColor, Color.Transparent, 0.5f),
                                            Color.Lerp(ExplosionColor2, Color.DarkRed, 0.5f), 0.0f, 0.15f, 10, 1, false,
                                            new Vector2(timedHeavyProjectile.MaxY, timedHeavyProjectile.MaxY + 8), true, timedHeavyProjectile.MaxY / 1080);
                                    YSortedEmitterList.Add(ExplosionEmitter2);

                                    Emitter SmokeEmitter = new Emitter(SmokeParticle,
                                            new Vector2(timedHeavyProjectile.Rod.Center.X, timedHeavyProjectile.Rod.Center.Y),
                                            new Vector2(0, 360), new Vector2(0f, 1f), new Vector2(30, 40), 0.8f, true, new Vector2(0, 360),
                                            new Vector2(0, 0), new Vector2(0.8f, 1.2f), SmokeColor1, SmokeColor2, -0.02f, 0.4f, 20, 1, false,
                                            new Vector2(timedHeavyProjectile.MaxY, timedHeavyProjectile.MaxY + 8), false, timedHeavyProjectile.MaxY / 1080,
                                            null, null, null, null, null, null, null, true, true);
                                    YSortedEmitterList.Add(SmokeEmitter);

                                    Emitter ExplosionEmitter3 = new Emitter(ExplosionParticle2,
                                            new Vector2(timedHeavyProjectile.Rod.Center.X, timedHeavyProjectile.Position.Y),
                                            new Vector2(0, 360), new Vector2(1, 2), new Vector2(25, 40), 0.35f, true, new Vector2(0, 0),
                                            new Vector2(0, 0), new Vector2(0.085f, 0.2f), FireColor,
                                            Color.Lerp(Color.Red, FireColor2, 0.5f), -0.1f, 0.05f, 10, 1, false,
                                            new Vector2(timedHeavyProjectile.MaxY, timedHeavyProjectile.MaxY + 8), true, timedHeavyProjectile.MaxY / 1080,
                                            null, null, null, null, null, null, new Vector2(0.0025f, 0.0025f), true, true, 50);
                                    YSortedEmitterList.Add(ExplosionEmitter3);

                                    AddDrawable(ExplosionEmitter, ExplosionEmitter2, ExplosionEmitter3, SmokeEmitter);

                                    if (timedHeavyProjectile.Position.Y >= timedHeavyProjectile.MaxY)
                                    {
                                        Decal NewDecal = new Decal(ExplosionDecal1, new Vector2(timedHeavyProjectile.Rod.Center.X, timedHeavyProjectile.Rod.Center.Y),
                                                                   (float)RandomDouble(0, 0), timedHeavyProjectile.YRange, timedHeavyProjectile.MaxY, 0.5f);

                                        DecalList.Add(NewDecal);

                                        Emitter DebrisEmitter = new Emitter(SplodgeParticle,
                                                new Vector2(timedHeavyProjectile.Rod.Center.X, timedHeavyProjectile.MaxY),
                                                new Vector2(40, 140), new Vector2(5, 7), new Vector2(30, 110), 1f, true, new Vector2(0, 360),
                                                new Vector2(1, 3), new Vector2(0.007f, 0.05f), Color.DarkSlateGray, Color.SaddleBrown,
                                                0.2f, 0.2f, 5, 1, true, new Vector2(timedHeavyProjectile.MaxY + 8, timedHeavyProjectile.MaxY + 16), null, timedHeavyProjectile.MaxY / 1080);
                                        AddDrawable(DebrisEmitter);

                                        Emitter DebrisEmitter2 = new Emitter(SplodgeParticle,
                                                new Vector2(timedHeavyProjectile.Rod.Center.X, timedHeavyProjectile.Position.Y), new Vector2(50, 130),
                                                new Vector2(2, 8), new Vector2(80, 150), 1f, true, new Vector2(0, 360), new Vector2(1, 3),
                                                new Vector2(0.01f, 0.02f), Color.Gray, Color.SaddleBrown, 0.2f, 0.3f, 10, 5, true,
                                                new Vector2(timedHeavyProjectile.MaxY + 8, timedHeavyProjectile.MaxY + 16), null, timedHeavyProjectile.MaxY / 1080);
                                        YSortedEmitterList.Add(DebrisEmitter2);

                                        AddDrawable(DebrisEmitter, DebrisEmitter2);
                                    }

                                    Emitter SparkEmitter = new Emitter(RoundSparkParticle,
                                            new Vector2(timedHeavyProjectile.Rod.Center.X, timedHeavyProjectile.Rod.Center.Y), new Vector2(0, 360),
                                            new Vector2(1, 4), new Vector2(120, 140), 1f, true, new Vector2(0, 360), new Vector2(1, 3),
                                            new Vector2(0.1f, 0.3f), Color.Orange, Color.OrangeRed, 0.05f, 0.1f, 2, 7, true,
                                            new Vector2(timedHeavyProjectile.MaxY + 4, timedHeavyProjectile.MaxY + 16), null, timedHeavyProjectile.MaxY / 1080);
                                    AlphaEmitterList.Add(SparkEmitter);

                                    timedHeavyProjectile.Active = false;
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

                                Emitter gasEmitter = new Emitter(SmokeParticle, timedHeavyProjectile.Rod.Center, new Vector2(-10, 190),
                                    new Vector2(0.5f, 6.0f), new Vector2(800, 1000), 0.25f, true, new Vector2(0, 360), new Vector2(-0.5f, 0.5f),
                                    new Vector2(0.5f, 1.0f), Color.LawnGreen, Color.GreenYellow, 0.003f, 1f, 2, 2, true, new Vector2(timedHeavyProjectile.MaxY, timedHeavyProjectile.MaxY + 16), false,
                                    timedHeavyProjectile.Rod.Center.Y / 1080, true, false, null, null, null, null, new Vector2(0.038f, 0.08f), true, true, 500, true, true);
                                YSortedEmitterList.Add(gasEmitter);
                                //AddDrawable(gasEmitter);

                                Emitter gasEmitter2 = new Emitter(SmokeParticle, timedHeavyProjectile.Rod.Center, new Vector2(0, 10),
                                    new Vector2(0.5f, 12.0f), new Vector2(800, 1000), 0.25f, true, new Vector2(0, 360), new Vector2(-0.5f, 0.5f),
                                    new Vector2(0.5f, 1.0f), Color.LawnGreen, Color.GreenYellow, 0.003f, 2f, 10, 2, true, new Vector2(timedHeavyProjectile.MaxY, timedHeavyProjectile.MaxY + 16), true,
                                    timedHeavyProjectile.Rod.Center.Y / 1080, true, false, null, null, null, null, new Vector2(0.05f, 0.05f), true, true, 500, true, true);
                                YSortedEmitterList.Add(gasEmitter2);
                                //AddDrawable(gasEmitter2);

                                Emitter gasEmitter3 = new Emitter(SmokeParticle, timedHeavyProjectile.Rod.Center, new Vector2(170, 180),
                                    new Vector2(0.5f, 12.0f), new Vector2(800, 1000), 0.25f, true, new Vector2(0, 360), new Vector2(-0.5f, 0.5f),
                                    new Vector2(0.5f, 1.0f), Color.LawnGreen, Color.GreenYellow, 0.003f, 1f, 10, 2, true, new Vector2(timedHeavyProjectile.MaxY, timedHeavyProjectile.MaxY + 16), true,
                                    timedHeavyProjectile.Rod.Center.Y / 1080, true, false, null, null, null, null, new Vector2(0.05f, 0.05f), true, true, 500, true, true);
                                YSortedEmitterList.Add(gasEmitter3);
                                //AddDrawable(gasEmitter3);

                                AddDrawable(gasEmitter, gasEmitter2, gasEmitter3);

                                GasEmitterList.Add(gasEmitter);
                                GasEmitterList.Add(gasEmitter2);
                                GasEmitterList.Add(gasEmitter3);
                                break;
                            #endregion
                        }

                        #region Deactivate the Projectile
                        foreach (Emitter emitter in timedHeavyProjectile.EmitterList)
                        {
                            emitter.AddMore = false;
                        }

                        timedHeavyProjectile.Active = false;
                        timedHeavyProjectile.Velocity = Vector2.Zero;
                        #endregion
                    }

                    #endregion                    

                    if (!ScreenDrawRectangle.Contains(VectorToPoint(heavyProjectile.Center)))
                    {
                        CreateHeavyProjectileCollision(heavyProjectile, heavyProjectile.SourceObject, ScreenRectangle);
                        heavyProjectile.Active = false;
                        heavyProjectile.EmitterList.All(Emitter => Emitter.AddMore = false);
                        return;
                    }
                }
                #endregion
            }
        }


        public void OnHeavyProjectileCollision(object source, HeavyProjectileEventArgs e)
        {
            HeavyProjectile heavyProjectile = e.Projectile;

            HeavyRangedInvader sourceInvader = (source as HeavyRangedInvader);

            Trap collisionTrap = e.collisionObject as Trap;
            Invader collisionInvader = e.collisionObject as Invader;
            StaticSprite collisionGround = e.collisionObject as StaticSprite;
            Tower collisionTower = e.collisionObject as Tower;
            Turret collisionTurret = e.collisionObject as Turret;
            Shield collisionShield = e.collisionObject as Shield;

            #region Deactivate Action
            //This is here so that the projectiles aren't deactivated when they collide with every invader
            //i.e. They should not collide with the soldiers, but should collide with the tanks
            Action<HeavyProjectile> DeactivateProjectile = (HeavyProjectile projectile) =>
            {
                foreach (Emitter emitter in heavyProjectile.EmitterList)
                {
                    emitter.AddMore = false;
                }

                heavyProjectile.Active = false;
                heavyProjectile.Velocity = Vector2.Zero;
            };
            #endregion

            #region TURRET spawned the projectile
            if (source.GetType().BaseType == typeof(Turret))
            {
                #region TRAP was hit
                if (collisionTrap != null)
                {
                    switch (collisionTrap.TrapType)
                    {
                        #region Wall
                        case TrapType.Wall:
                            switch (heavyProjectile.HeavyProjectileType)
                            {
                                #region Cannon Ball
                                case HeavyProjectileType.CannonBall:
                                    {
                                        //Emitter newEmitter = new Emitter(SplodgeParticle, heavyProjectile.Center,
                                        //         new Vector2(
                                        //             -(float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X) - 180 - 45,
                                        //             (float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X) - 180 + 45),
                                        //         new Vector2(2, 3), new Vector2(100, 200), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                        //         new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true,
                                        //         new Vector2(collisionTrap.DestinationRectangle.Bottom, collisionTrap.DestinationRectangle.Bottom), false, null, true, true);

                                        //EmitterList2.Add(newEmitter);

                                        //for (int i = 0; i < 15; i++)
                                        //{
                                        //    Emitter SparkEmitter2 = new Emitter(RoundSparkParticle, new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                                        //    new Vector2(0, 360), new Vector2(0.5f, 1.5f), new Vector2(30, 60), 100f, true, new Vector2(0, 360), new Vector2(1, 3),
                                        //    new Vector2(0.1f, 0.3f), Color.Orange, Color.OrangeRed, 0.05f, 0.5f, 1, 1, true, new Vector2(collisionTrap.DestinationRectangle.Bottom, collisionTrap.DestinationRectangle.Bottom),
                                        //    false, null, true, true, new Vector2(8, 10), new Vector2(
                                        //                         -(float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X) - 180 - 45,
                                        //                         (float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X) - 180 + 45), 0.6f, true, new Vector2(0, 0), null, null, null, true, null);

                                        //    AdditiveEmitterList.Add(SparkEmitter2);
                                        //}
                                    }
                                    break;
                                #endregion
                            }
                            break;
                        #endregion
                    }

                    DeactivateProjectile(heavyProjectile);
                    return;
                }
                #endregion

                #region INVADER was hit
                if (collisionInvader != null)
                {
                    switch (collisionInvader.InvaderType)
                    {
                        #region Heal Drone
                        case InvaderType.HealDrone:
                            {
                            }
                            break;
                        #endregion
                    }

                    DeactivateProjectile(heavyProjectile);
                    return;
                }
                #endregion

                #region GROUND was hit
                if (collisionGround != null)
                {
                    switch (heavyProjectile.HeavyProjectileType)
                    {
                        #region Cannon ball
                        case HeavyProjectileType.CannonBall:
                            {
                                #region Regular ground
                                Emitter ExplosionEmitter = new Emitter(ExplosionParticle2,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(20, 160), new Vector2(0.3f, 0.8f), new Vector2(500, 1000), 0.85f, true, new Vector2(-2, 2),
                                        new Vector2(-1, 1), new Vector2(0.15f, 0.25f), FireColor,
                                        Color.Black, -0.2f, 0.1f, 10, 1, false,
                                        new Vector2(heavyProjectile.MaxY, heavyProjectile.MaxY + 8), false, heavyProjectile.MaxY / 1080f,
                                        null, null, null, null, null, null, new Vector2(0.1f, 0.2f), true, true, null, null, null, true);
                                YSortedEmitterList.Add(ExplosionEmitter);

                                Emitter ExplosionEmitter3 = new Emitter(ExplosionParticle2,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(85, 95), new Vector2(2, 4), new Vector2(400, 640), 0.35f, true, new Vector2(0, 0),
                                        new Vector2(0, 0), new Vector2(0.085f, 0.2f), FireColor, ExplosionColor3, -0.1f, 0.05f, 10, 1, false,
                                        new Vector2(heavyProjectile.MaxY, heavyProjectile.MaxY + 8), true, heavyProjectile.MaxY / 1080f,
                                        null, null, null, null, null, null, new Vector2(0.0025f, 0.0025f), true, true, 50);
                                YSortedEmitterList.Add(ExplosionEmitter3);

                                Emitter DebrisEmitter = new Emitter(SplodgeParticle,
                                       new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                       new Vector2(70, 110), new Vector2(5, 7), new Vector2(480, 1760), 1f, true, new Vector2(0, 360),
                                       new Vector2(1, 3), new Vector2(0.007f, 0.05f), Color.DarkSlateGray, Color.DarkSlateGray,
                                       0.2f, 0.2f, 5, 1, true, new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 16), null, heavyProjectile.MaxY / 1080f);
                                YSortedEmitterList.Add(DebrisEmitter);

                                AddDrawable(ExplosionEmitter, ExplosionEmitter3, DebrisEmitter);
                                #endregion

                                #region Old Emitters
                                #region Regular ground
                                //Emitter SmokeEmitter = new Emitter(SmokeParticle,
                                //        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY - 30),
                                //        new Vector2(0, 0), new Vector2(0f, 1f), new Vector2(480, 640), 0.8f, true, new Vector2(0, 360),
                                //        new Vector2(0, 0), new Vector2(0.8f, 1.2f), SmokeColor2, SmokeColor2, -0.1f, 0.4f, 20, 1, false,
                                //        new Vector2(heavyProjectile.MaxY, heavyProjectile.MaxY + 8), false, heavyProjectile.MaxY / 1080f,
                                //        null, null, null, null, null, null, null, true, true);
                                //YSortedEmitterList.Add(SmokeEmitter);

                                //Emitter ExplosionEmitter3 = new Emitter(ExplosionParticle2,
                                //        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                //        new Vector2(85, 95), new Vector2(2, 4), new Vector2(400, 640), 0.35f, true, new Vector2(0, 0),
                                //        new Vector2(0, 0), new Vector2(0.085f, 0.2f), FireColor, ExplosionColor3, -0.1f, 0.05f, 10, 1, false,
                                //        new Vector2(heavyProjectile.MaxY, heavyProjectile.MaxY + 8), true, heavyProjectile.MaxY / 1080f,
                                //        null, null, null, null, null, null, new Vector2(0.0025f, 0.0025f), true, true, 50);
                                //YSortedEmitterList.Add(ExplosionEmitter3);

                                //Emitter DebrisEmitter = new Emitter(SplodgeParticle,
                                //        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                //        new Vector2(70, 110), new Vector2(5, 7), new Vector2(480, 1760), 1f, true, new Vector2(0, 360),
                                //        new Vector2(1, 3), new Vector2(0.007f, 0.05f), Color.DarkSlateGray, Color.DarkSlateGray,
                                //        0.2f, 0.2f, 5, 1, true, new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 16), null, heavyProjectile.MaxY / 1080f);
                                //YSortedEmitterList.Add(DebrisEmitter);

                                ////Emitter DebrisEmitter2 = new Emitter(SplodgeParticle,
                                ////        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY), new Vector2(80, 100),
                                ////        new Vector2(2, 8), new Vector2(80, 150), 1f, true, new Vector2(0, 360), new Vector2(1, 3),
                                ////        new Vector2(0.01f, 0.02f), Color.Gray, Color.SaddleBrown, 0.2f, 0.3f, 10, 5, true,
                                ////        new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 16), null, heavyProjectile.MaxY / 1080f);
                                ////YSortedEmitterList.Add(DebrisEmitter2);

                                //Emitter SparkEmitter = new Emitter(RoundSparkParticle,
                                //        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY - 20), new Vector2(0, 360),
                                //        new Vector2(1, 4), new Vector2(800, 1600), 1f, true, new Vector2(0, 0), new Vector2(0, 0),
                                //        new Vector2(0.1f, 0.3f), Color.Orange, Color.OrangeRed, 0.05f, 0.1f, 2, 7, true,
                                //        new Vector2(heavyProjectile.MaxY - 16, heavyProjectile.MaxY + 16), null, heavyProjectile.MaxY / 1080f,
                                //        true, true, null, null, null, null, new Vector2(0.001f, 0.002f), null, null, 1000f);
                                //SparkEmitter.BlendState = BlendState.Additive;
                                //YSortedEmitterList.Add(SparkEmitter);

                                //Emitter ExplosionEmitter = new Emitter(ExplosionParticle,
                                //        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                //        new Vector2(20, 160), new Vector2(6, 14), new Vector2(80, 112), 1f, true, new Vector2(0, 360),
                                //        new Vector2(0, 0), new Vector2(3, 4), ExplosionColor * 0.5f,
                                //        ExplosionColor2 * 0.5f, 0.0f, 0.1f, 1, 1, false,
                                //        new Vector2(heavyProjectile.MaxY, heavyProjectile.MaxY + 8), true, heavyProjectile.MaxY / 1080f);
                                //YSortedEmitterList.Add(ExplosionEmitter);

                                //Emitter ExplosionEmitter2 = new Emitter(ExplosionParticle,
                                //        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                //        new Vector2(70, 110), new Vector2(6, 12), new Vector2(126, 288), 1f, true, new Vector2(0, 360),
                                //        new Vector2(0, 0), new Vector2(3, 4), ExplosionColor * 0.5f, ExplosionColor2 * 0.5f,
                                //        0.0f, 0.15f, 10, 1, false, new Vector2(heavyProjectile.MaxY, heavyProjectile.MaxY + 8),
                                //        true, heavyProjectile.MaxY / 1080f);
                                //YSortedEmitterList.Add(ExplosionEmitter2);

                                //AddDrawable(ExplosionEmitter, ExplosionEmitter2, DebrisEmitter, ExplosionEmitter3, SmokeEmitter, SparkEmitter);
                                #endregion
                                #endregion

                                Decal NewDecal = new Decal(ExplosionDecal1, new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                                                          (float)RandomDouble(0, 0), heavyProjectile.YRange, heavyProjectile.MaxY, 0.75f);

                                DecalList.Add(NewDecal);

                                ShockWaveEffect.Parameters["CenterCoords"].SetValue(new Vector2(1 / (1920 / heavyProjectile.Position.X), 1 / (1080 / heavyProjectile.Position.Y)));
                                ShockWaveEffect.Parameters["WaveParams"].SetValue(new Vector4(1, 0.5f, 0.06f, 60));
                                ShockWaveEffect.Parameters["CurrentTime"].SetValue(0);

                                Explosion newExplosion = new Explosion(heavyProjectile.Position, heavyProjectile.BlastRadius, heavyProjectile.Damage);

                                CreateExplosion(newExplosion, heavyProjectile);

                                CannonExplosion.Play();
                            }
                            break;
                        #endregion
                    }

                    DeactivateProjectile(heavyProjectile);
                    return;
                }
                #endregion
            }
            #endregion

            #region TRAP spawned the projectile
            if (source.GetType().BaseType == typeof(Trap))
            {

            }
            #endregion

            #region INVADER spawned the projectile
            if (sourceInvader != null)
            {
                #region TRAP was hit
                if (collisionTrap != null)
                {
                    switch (collisionTrap.TrapType)
                    {
                        #region Wall
                        case TrapType.Wall:
                            {
                                switch ((source as Invader).InvaderType)
                                {
                                    #region Stationary Cannon
                                    case InvaderType.StationaryCannon:
                                        {

                                        }
                                        break;
                                    #endregion

                                    #region Archer
                                    case InvaderType.Archer:
                                        {

                                        }
                                        break;
                                    #endregion
                                }
                            }
                            break;
                        #endregion

                        #region Barrel
                        case TrapType.Barrel:
                            {
                                switch ((source as Invader).InvaderType)
                                {
                                    #region Stationary Cannon
                                    case InvaderType.StationaryCannon:
                                        {

                                        }
                                        break;
                                    #endregion

                                    #region Archer
                                    case InvaderType.Archer:
                                        {

                                        }
                                        break;
                                    #endregion
                                }
                            }
                            break;
                        #endregion
                    }

                    DeactivateProjectile(heavyProjectile);
                    return;
                }
                #endregion

                #region GROUND was hit
                if (collisionGround != null)
                {
                    switch ((source as Invader).InvaderType)
                    {
                        #region Stationary Cannon
                        case InvaderType.StationaryCannon:
                            {

                            }
                            break;
                        #endregion

                        #region Archer
                        case InvaderType.Archer:
                            {

                            }
                            break;
                        #endregion
                    }

                    sourceInvader.HitObject = Ground;
                    DeactivateProjectile(heavyProjectile);
                    return;
                }
                #endregion

                #region TOWER was hit
                if (collisionTower != null)
                {
                    switch ((source as Invader).InvaderType)
                    {
                        #region Stationary Cannon
                        case InvaderType.StationaryCannon:
                            {

                            }
                            break;
                        #endregion

                        #region Archer
                        case InvaderType.Archer:
                            {

                            }
                            break;
                        #endregion
                    }

                    DeactivateProjectile(heavyProjectile);
                    return;
                }
                #endregion

                #region TURRET was hit
                if (collisionTurret != null)
                {
                    switch ((source as Invader).InvaderType)
                    {
                        #region Stationary Cannon
                        case InvaderType.StationaryCannon:
                            {

                            }
                            break;
                        #endregion

                        #region Archer
                        case InvaderType.Archer:
                            {

                            }
                            break;
                        #endregion
                    }

                    DeactivateProjectile(heavyProjectile);
                    return;
                }
                #endregion

                #region SHIELD was hit
                if (collisionShield != null)
                {
                    sourceInvader.HitObject = Tower.Shield;

                    switch (sourceInvader.InvaderType)
                    {
                        #region Stationary Cannon
                        case InvaderType.StationaryCannon:
                            {

                            }
                            break;
                        #endregion

                        #region Archer
                        case InvaderType.Archer:
                            {

                            }
                            break;
                        #endregion
                    }

                    DeactivateProjectile(heavyProjectile);
                    return;
                }
                #endregion

                #region Projectile Outside Bounds
                if ((Rectangle)e.collisionObject == ScreenRectangle)
                {
                    sourceInvader.HitObject = ScreenRectangle;
                    DeactivateProjectile(heavyProjectile);
                }
                #endregion
            }
            #endregion
        }

        public void OnLightProjectileFired(object source, LightProjectileEventArgs e)
        {
            LightProjectile projectile = e.Projectile;
            LightRangedInvader sourceInvader = source as LightRangedInvader;
            Turret sourceTurret = source as Turret;

            CurrentProjectile = projectile;

            #region Change the ground bounding box
            if (sourceTurret != null)
            {
                Ground.BoundingBox.Min = new Vector3(0, MathHelper.Clamp(CursorPosition.Y, Math.Max(690, CurrentTurret.BaseRectangle.Bottom + 16), 960), 0);
            }

            if (sourceInvader != null)
            {
                Ground.BoundingBox.Min = new Vector3(0, MathHelper.Clamp(sourceInvader.DestinationRectangle.Bottom + 16, 690, 960), 0);
            }
            #endregion


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
                    case LightProjectileType.Laser:
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
                }
            };
            #endregion


            #region INVADER hit by a projectile
            Action<Turret, Invader, Vector2> InvaderEffect = (Turret Turret, Invader hitInvader, Vector2 collisionEnd) =>
            {
                #region The turret has a limited range
                float TurretInvaderDist = Vector2.Distance(Turret.BarrelEnd, collisionEnd);
                if (Turret.Range != 0)
                {
                    #region The invader is in range
                    if (TurretInvaderDist <= Turret.Range)
                    {
                        switch (Turret.TurretFireType)
                        {
                            default:
                                hitInvader.TurretDamage(-Turret.Damage);
                                break;

                            //case TurretFireType.Beam:
                            //    hitInvader.HitByBeam = true;
                            //    hitInvader.BeamDelay = Turret.FireDelay;

                            //    if (hitInvader.CurrentBeamDelay >= hitInvader.BeamDelay)
                            //    {
                            //        hitInvader.TurretDamage(-Turret.Damage);
                            //        hitInvader.CurrentBeamDelay = 0;
                            //    }
                            //    break;
                        }
                    }
                    #endregion
                    else
                    #region The invader is out of range - reduce the damage accordingly
                    {
                        float OverRange = TurretInvaderDist - Turret.Range;
                        float DamageReduction = Math.Min((100 / (2 * Turret.Range)) * OverRange, 50);
                        float FinalDamage = ((100 - DamageReduction) / 100) * Turret.Damage;
                        switch (Turret.TurretFireType)
                        {
                            default:
                                hitInvader.TurretDamage((int)-FinalDamage);
                                break;

                            //case TurretFireType.Beam:
                            //    hitInvader.HitByBeam = true;
                            //    hitInvader.BeamDelay = Turret.FireDelay;

                            //    if (hitInvader.CurrentBeamDelay >= hitInvader.BeamDelay)
                            //    {
                            //        hitInvader.TurretDamage((int)-FinalDamage);
                            //        hitInvader.CurrentBeamDelay = 0;
                            //    }
                            //    break;
                        }
                    }
                    #endregion
                }
                #endregion
                else
                {
                    switch (Turret.TurretFireType)
                    {
                        default:
                            hitInvader.TurretDamage(-Turret.Damage);
                            break;

                        //case TurretFireType.Beam:
                        //    hitInvader.HitByBeam = true;
                        //    hitInvader.BeamDelay = Turret.FireDelay;

                        //    if (hitInvader.CurrentBeamDelay >= hitInvader.BeamDelay)
                        //    {
                        //        hitInvader.TurretDamage(-Turret.Damage);
                        //        hitInvader.CurrentBeamDelay = 0;
                        //    }
                        //    break;
                    }
                }

                switch (Turret.TurretType)
                {
                    #region Freeze
                    case TurretType.Freeze:
                        switch (hitInvader.InvaderType)
                        {
                            #region Default
                            default:
                                hitInvader.Freeze(new FreezeStruct() { MaxDelay = 3000 }, Color.SkyBlue);

                                Emitter InvaderSparks = new Emitter(BallParticle, hitInvader.Center,
                                new Vector2(70, 110), new Vector2(1, 3), new Vector2(5, 15), 1f, true, new Vector2(0, 360),
                                new Vector2(2, 5), new Vector2(0.25f, 0.25f), Color.Blue, Color.DeepSkyBlue, 0.0f, 0.1f, 1, 10,
                                false, new Vector2(0, 1080), false, 0, false, false);

                                Emitter InvaderSmoke = new Emitter(SmokeParticle, hitInvader.Center,
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
                                        if (Vector2.Distance(invader.Center, hitInvader.Center) < 150)
                                        {
                                            for (int i = 0; i < 2; i++)
                                            {
                                                Bolt = new LightningBolt(collisionEnd, invader.Center,
                                                    Color.MediumPurple, 0.02f, 150f, true);

                                                LightningList.Add(Bolt);
                                            }

                                            if (invader != hitInvader)
                                            {
                                                invader.CurrentHP -= 5;                                                
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
                                    new Vector2(hitInvader.Center.X, collisionEnd.Y),
                                    new Vector2(
                                    MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - (float)RandomDouble(0, 45),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) + (float)RandomDouble(0, 45)),
                                    new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360),
                                    new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.DarkRed, Color.Red,
                                    0.1f, 0.1f, 10, 5, true, new Vector2(hitInvader.MaxY, hitInvader.MaxY), false, 1, true, false);

                                    Emitter BloodEmitter2 = new Emitter(SplodgeParticle,
                                    new Vector2(hitInvader.Center.X, collisionEnd.Y),
                                    new Vector2(
                                    MathHelper.ToDegrees(-(float)Math.Atan2(-CurrentProjectile.Ray.Direction.Y, -CurrentProjectile.Ray.Direction.X)) - (float)RandomDouble(0, 45),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(-CurrentProjectile.Ray.Direction.Y, -CurrentProjectile.Ray.Direction.X)) + (float)RandomDouble(0, 45)),
                                    new Vector2(1, 3), new Vector2(30, 60), 0.5f, true, new Vector2(0, 360),
                                    new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.DarkRed, Color.Red,
                                    0.1f, 0.1f, 10, 5, true, new Vector2(hitInvader.MaxY, hitInvader.MaxY), false, 1, true, false);

                                    YSortedEmitterList.Add(BloodEmitter);
                                    YSortedEmitterList.Add(BloodEmitter2);

                                    AddDrawable(BloodEmitter, BloodEmitter2);
                                    break;
                                #endregion
                            }
                        }
                        break;
                    #endregion
                }
            };
            #endregion

            #region TRAP hit by a projectile
            Action<Vector2, object, Trap> TrapEffect = (Vector2 CollisionEnd, object sourceObject, Trap Trap) =>
            {
                Turret turret = sourceObject as Turret;
                Invader invader = sourceObject as Invader;

                #region Source of projectile was a turret
                if (turret != null)
                {
                    switch (turret.TurretType)
                    {
                        default:
                            switch (Trap.TrapType)
                            {
                                #region Wall
                                case TrapType.Wall:
                                    {
                                        Emitter Emitter = new Emitter(SplodgeParticle, CollisionEnd,
                                            new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                                        MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                            new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360),
                                            new Vector2(1, 3), new Vector2(0.02f, 0.05f), Color.Gray, Color.DarkGray,
                                            0.1f, 0.1f, 10, 2, true, new Vector2(Trap.DestinationRectangle.Bottom, Trap.DestinationRectangle.Bottom), false, 1, true, false);
                                        YSortedEmitterList.Add(Emitter);

                                        //Need to calculate the reflection angle better. 
                                        for (int i = 0; i < Random.Next(2, 8); i++)
                                        {
                                            Emitter SparkEmitter2 = new Emitter(RoundSparkParticle, CollisionEnd,
                                            new Vector2(0, 360), new Vector2(0.5f, 1.5f), new Vector2(30, 60), 100f, true, new Vector2(0, 360),
                                            new Vector2(1, 3), new Vector2(0.1f, 0.3f), Color.Orange, Color.OrangeRed, 0.05f, 0.5f, 10, 1, true,
                                            new Vector2(Trap.DestinationRectangle.Bottom, Trap.DestinationRectangle.Bottom),
                                            false, null, true, true, new Vector2(2, 5),
                                            new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                                        MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                                        0.6f, false, new Vector2(0.5f, 0), null, null, null, true, null);

                                            AdditiveEmitterList.Add(SparkEmitter2);
                                        }

                                        Emitter DustEmitter = new Emitter(SmokeParticle,
                                               new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                               new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                                           MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                               new Vector2(1.8f, 3f), new Vector2(70, 100), 0.5f, true, new Vector2(0, 360),
                                               new Vector2(0.5f, 1f), new Vector2(0.2f, 0.5f), DirtColor2, DirtColor, 0.03f, 0.02f, 5, 1, false,
                                               new Vector2(0, 1080), true, CollisionEnd.Y / 1080,
                                               null, null, null, null, null, null, new Vector2(0.08f, 0.08f), true, true);
                                        YSortedEmitterList.Add(DustEmitter);

                                        AddDrawable(DustEmitter, Emitter);
                                    }
                                    break;
                                #endregion

                                #region Barrel
                                case TrapType.Barrel:
                                    Trap.CurrentHP -= turret.Damage;
                                    break;
                                #endregion
                            }
                            break;
                    }
                }
                #endregion

                #region Source of projectile was an invader
                if (invader != null)
                {
                    switch (invader.InvaderType)
                    {
                        default:
                            switch (Trap.TrapType)
                            {
                                #region Wall
                                case TrapType.Wall:
                                    {
                                        Emitter Emitter = new Emitter(SplodgeParticle, CollisionEnd,
                                            new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                                        MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                            new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360),
                                            new Vector2(1, 3), new Vector2(0.02f, 0.05f), Color.Gray, Color.DarkGray,
                                            0.1f, 0.1f, 10, 2, true, new Vector2(Trap.DestinationRectangle.Bottom, Trap.DestinationRectangle.Bottom), false, 1, true, false);
                                        YSortedEmitterList.Add(Emitter);

                                        //Need to calculate the reflection angle better. 
                                        for (int i = 0; i < Random.Next(2, 8); i++)
                                        {
                                            Emitter SparkEmitter2 = new Emitter(RoundSparkParticle, CollisionEnd,
                                            new Vector2(0, 360), new Vector2(0.5f, 1.5f), new Vector2(30, 60), 100f, true, new Vector2(0, 360),
                                            new Vector2(1, 3), new Vector2(0.1f, 0.3f), Color.Orange, Color.OrangeRed, 0.05f, 0.5f, 10, 1, true,
                                            new Vector2(Trap.DestinationRectangle.Bottom, Trap.DestinationRectangle.Bottom),
                                            false, null, true, true, new Vector2(2, 5),
                                            new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                                        MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                                        0.6f, false, new Vector2(0.5f, 0), null, null, null, true, null);

                                            AdditiveEmitterList.Add(SparkEmitter2);
                                        }

                                        Emitter DustEmitter = new Emitter(SmokeParticle,
                                               new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                               new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                                           MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                               new Vector2(1.8f, 3f), new Vector2(70, 100), 0.5f, true, new Vector2(0, 360),
                                               new Vector2(0.5f, 1f), new Vector2(0.2f, 0.5f), DirtColor2, DirtColor, 0.03f, 0.02f, 5, 1, false,
                                               new Vector2(0, 1080), true, CollisionEnd.Y / 1080,
                                               null, null, null, null, null, null, new Vector2(0.08f, 0.08f), true, true);
                                        YSortedEmitterList.Add(DustEmitter);

                                        AddDrawable(DustEmitter, Emitter);
                                    }
                                    break;
                                #endregion

                                #region Barrel
                                case TrapType.Barrel:
                                    //Trap.CurrentHP -= invader.D
                                    break;
                                #endregion
                            }
                            break;
                    }
                }
                #endregion
            };
            #endregion

            #region GROUND hit by a projectile
            Action<Vector2, Vector2, LightProjectileType> GroundEffect = (Vector2 CollisionStart, Vector2 CollisionEnd, LightProjectileType LightProjectileType) =>
            {
                switch (LightProjectileType)
                {
                    #region MachineGun
                    case LightProjectileType.MachineGun:
                        Emitter DebrisEmitter = new Emitter(SplodgeParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                                            new Vector2(60, 120), new Vector2(2, 4), new Vector2(320, 640), 0.85f, true,
                                                            new Vector2(0, 0), new Vector2(0, 0),
                                                            new Vector2(0.02f, 0.04f), Color.DarkSlateGray, Color.DarkSlateGray, 0.2f,
                                                            0.1f, 5, 1, true, new Vector2(CollisionEnd.Y + 8, CollisionEnd.Y + 16), null, CollisionEnd.Y / 1080,
                                                            true, true, null, null, null, null, null, true, true, 50f, false);
                        YSortedEmitterList.Add(DebrisEmitter);

                        Emitter DebrisEmitter2 = new Emitter(SplodgeParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                                            new Vector2(80, 100), new Vector2(3, 5), new Vector2(320,640), 0.85f, true,
                                                            new Vector2(0, 0), new Vector2(0, 0),
                                                            new Vector2(0.01f, 0.03f), Color.DarkSlateGray, Color.DarkSlateGray, 0.2f,
                                                            0.1f, 2, 3, true, new Vector2(CollisionEnd.Y + 8, CollisionEnd.Y + 16), null, CollisionEnd.Y / 1080,
                                                            true, true, null, null, null, null, null, true, true, 50f, false);
                        YSortedEmitterList.Add(DebrisEmitter2);

                        Emitter DustEmitter = new Emitter(SmokeParticle,
                                       new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                       new Vector2(80, 100), new Vector2(2f, 4f), new Vector2(1120, 1600), 0.8f, true, new Vector2(0, 360),
                                       new Vector2(0.5f, 1f), new Vector2(0.2f, 0.5f), DirtColor, DirtColor, 0.03f, 0.02f, 5, 2, false,
                                       new Vector2(0, 1080), false, CollisionEnd.Y / 1080,
                                       null, null, null, null, null, null, new Vector2(0.08f, 0.08f), true, true);
                        YSortedEmitterList.Add(DustEmitter);

                        AddDrawable(DebrisEmitter, DebrisEmitter2, DustEmitter);

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
                    case LightProjectileType.Laser:

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

                        Emitter PulseSmokeEmitter = new Emitter(SmokeParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y - 4),
                        new Vector2(90, 90), new Vector2(0.5f, 1f), new Vector2(20, 30), 1f, true, new Vector2(0, 0),
                        new Vector2(-2, 2), new Vector2(0.5f, 1f), DirtColor, DirtColor2, 0f, 0.02f, 10, 1, false, new Vector2(0, 1080), false);
                        YSortedEmitterList.Add(PulseSmokeEmitter);

                        AddDrawable(PulseSmokeEmitter, PulseDebrisEmitter);

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

            #region SHIELD hit by a projectile
            Action<Vector2, object> ShieldEffect = (Vector2 CollisionEnd, object sourceObject) =>
            {
                Tower.TakeDamage(projectile.Damage);
            };
            #endregion

            #region TURRET hit by a projectile
            Action<Vector2, object, Trap> TurretEffect = (Vector2 CollisionEnd, object sourceObject, Trap Trap) =>
            {

            };
            #endregion

            #region TOWER hit by a projectile
            Action<Vector2, object> TowerEffect = (Vector2 CollisionEnd, object sourceObject) =>
            {
                Tower.TakeDamage(projectile.Damage);
            };
            #endregion


            #region Check what the projectile actually hit - trap, invader, ground, nothing etc.
            Action<object> CheckCollision = (object projectileSource) =>
            {
                CurrentProjectile = projectile;
                Vector2 sourcePosition = projectile.Position;
                Vector2 CollisionEnd;

                List<Drawable> Intersections = DrawableList.FindAll(Drawable => Drawable.BoundingBox.Intersects(CurrentProjectile.Ray) != null);

                Intersections.RemoveAll(Drawable => Drawable.GetType().BaseType == typeof(Trap) && (Drawable as Trap).Solid == false);

                if (sourceTurret != null)
                {
                    Intersections.RemoveAll(Drawable => Drawable.GetType().BaseType == typeof(Turret));
                }
                                
                if (sourceInvader != null)
                {
                    Intersections.RemoveAll(Drawable =>
                                            Drawable.GetType().BaseType == typeof(LightRangedInvader) ||
                                            Drawable.GetType().BaseType == typeof(HeavyRangedInvader) ||
                                            Drawable.GetType().BaseType == typeof(Invader));
                }

                Drawable hitObject = Intersections.OrderBy(Collision => Collision.BoundingBox.Intersects(CurrentProjectile.Ray)).FirstOrDefault();

                Invader HitInvader = hitObject as Invader;
                Turret HitTurret = hitObject as Turret;
                Trap HitTrap = hitObject as Trap;
                PowerupDelivery HitPowerupDelivery = hitObject as PowerupDelivery;                

                if (hitObject == null)
                {
                    float? DistToGround = CurrentProjectile.Ray.Intersects(Ground.BoundingBox);

                    #region TURRET fired the projectile
                    if (sourceTurret != null)
                    {
                        if (DistToGround != null)
                        {
                            CollisionEnd = new Vector2(sourcePosition.X + (CurrentProjectile.Ray.Direction.X * (float)DistToGround),
                                                       Random.Next(-16, 16) + sourcePosition.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToGround));

                            CreateEffect(sourcePosition, CollisionEnd, CurrentProjectile.LightProjectileType);
                            GroundEffect(sourcePosition, CollisionEnd, CurrentProjectile.LightProjectileType);
                        }
                        else
                        {
                            CollisionEnd = new Vector2(sourcePosition.X + (CurrentProjectile.Ray.Direction.X * 1920),
                                                       sourcePosition.Y + (CurrentProjectile.Ray.Direction.Y * 1920));

                            CreateEffect(sourcePosition, CollisionEnd, CurrentProjectile.LightProjectileType);
                        }
                    }
                    #endregion

                    #region INVADER fired the projectile
                    if (sourceInvader != null)
                    {
                        float? DistToTower = CurrentProjectile.Ray.Intersects(Tower.BoundingBox);
                        float? DistToShield = CurrentProjectile.Ray.Intersects(Tower.Shield.ShieldBoundingSphere);

                        List<float> DistanceList = new List<float>();

                        if (DistToTower != null)
                            DistanceList.Add((float)DistToTower);

                        if (DistToShield != null && Tower.Shield.ShieldOn == true)
                            DistanceList.Add((float)DistToShield);

                        if (DistToGround != null)
                            DistanceList.Add((float)DistToGround);

                        DistanceList.Add(1920);

                        CollisionEnd = new Vector2(sourcePosition.X + (CurrentProjectile.Ray.Direction.X * (float)DistanceList.Min()),
                                                   sourcePosition.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistanceList.Min()));


                        #region GROUND was hit
                        if (DistanceList.Min() == DistToGround)
                        {
                            CreateEffect(sourcePosition, CollisionEnd, CurrentProjectile.LightProjectileType);
                            GroundEffect(sourcePosition, CollisionEnd, CurrentProjectile.LightProjectileType);
                            sourceInvader.HitObject = Ground;
                        } 
                        #endregion

                        #region SHIELD was hit
                        if (DistanceList.Min() == DistToShield)
                        {
                            CreateEffect(sourcePosition, CollisionEnd, CurrentProjectile.LightProjectileType);
                            sourceInvader.HitObject = Tower.Shield;
                        } 
                        #endregion

                        #region TOWER was hit
                        if (DistanceList.Min() == DistToTower)
                        {
                            CreateEffect(sourcePosition, CollisionEnd, CurrentProjectile.LightProjectileType);
                            sourceInvader.HitObject = Tower;
                        }
                        #endregion

                        #region NOTHING was hit
                        if (DistanceList.Count == 0)
                        {
                            CollisionEnd = new Vector2(sourcePosition.X + (CurrentProjectile.Ray.Direction.X * 1920),
                                                       sourcePosition.Y + (CurrentProjectile.Ray.Direction.Y * 1920));

                            CreateEffect(sourcePosition, CollisionEnd, CurrentProjectile.LightProjectileType);

                            sourceInvader.HitObject = null;
                        }
                        #endregion
                    }
                    #endregion
                }
                else 
                {
                    var minDist = (float)hitObject.BoundingBox.Intersects(CurrentProjectile.Ray);

                    CollisionEnd = new Vector2(sourcePosition.X + (CurrentProjectile.Ray.Direction.X * (float)minDist),
                                               sourcePosition.Y + (CurrentProjectile.Ray.Direction.Y * (float)minDist));

                    #region TRAP was hit
                    if (HitTrap != null)
                    {
                        CreateEffect(sourcePosition, CollisionEnd, CurrentProjectile.LightProjectileType);
                        TrapEffect(CollisionEnd, projectileSource, HitTrap);

                        if (sourceInvader != null)
                            sourceInvader.HitObject = HitTrap;
                    }
                    #endregion

                    #region INVADER was hit
                    if (HitInvader != null)
                    {
                        CreateEffect(sourcePosition, CollisionEnd, CurrentProjectile.LightProjectileType);
                        InvaderEffect(sourceTurret, HitInvader, CollisionEnd);
                    }
                    #endregion

                    #region TURRET was hit
                    if (HitTurret != null)
                    {
                        if (Tower.Shield.ShieldOn == true)
                        {
                            float? DistToShield = CurrentProjectile.Ray.Intersects(Tower.Shield.ShieldBoundingSphere);
                            CollisionEnd = new Vector2(sourcePosition.X + (CurrentProjectile.Ray.Direction.X * (float)DistToShield),
                                                       sourcePosition.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToShield));

                            ShieldEffect(CollisionEnd, source);
                            sourceInvader.HitObject = HitTurret;
                        }
                        else
                        {
                            //TurretEffect(CollisionEnd, 
                        }

                        CreateEffect(sourcePosition, CollisionEnd, CurrentProjectile.LightProjectileType);
                    }
                    #endregion

                    #region POWERUP DELIVERY was hit
                    if (HitPowerupDelivery != null)
                    {
                        CreateEffect(sourcePosition, CollisionEnd, CurrentProjectile.LightProjectileType);

                        sourceInvader.HitObject = HitPowerupDelivery;
                    }
                    #endregion
                }
            };
            #endregion

            CheckCollision(source);

            CurrentProjectile = null;
            projectile = null;
        }


        private void BeamProjectileUpdate(GameTime gameTime)
        {
            if (CurrentMouseState.LeftButton == ButtonState.Pressed &&
                PreviousMouseState.LeftButton == ButtonState.Pressed)
            {
                CurrentBeam.Ray.Direction = new Vector3(CurrentTurret.FireDirection.X, CurrentTurret.FireDirection.Y, 0);

                if (CurrentTurret != null)
                    Ground.BoundingBox.Min = new Vector3(0, MathHelper.Clamp(CursorPosition.Y, Math.Max(690, CurrentTurret.BaseRectangle.Bottom + 16), 960), 0);
                else
                    Ground.BoundingBox.Min = new Vector3(0, MathHelper.Clamp(CursorPosition.Y, 690, 960), 0);

                #region Determine what effect to create
                Action<Vector2, Vector2, LightProjectileType> CreateBeamEffect = (Vector2 CollisionStart, Vector2 CollisionEnd, LightProjectileType LightProjectileType) =>
                {
                    switch (LightProjectileType)
                    {
                        #region Persistent beam
                        case LightProjectileType.PersistentBeam:
                            if (!TrailList.All(Trail => Trail.SourceTurret != CurrentTurret) || TrailList.Count < 1)
                            {
                                Trail = new BulletTrail(CollisionStart, CollisionEnd);
                                Trail.Segment = BulletTrailSegment;
                                Trail.Cap = BulletTrailCap;
                                Trail.SetUp();
                                Trail.SourceTurret = CurrentTurret;
                                TrailList.Add(Trail);
                            }
                            break;
                        #endregion
                    }
                };
                #endregion

                #region Determine what the invader does when hit by a beam
                Action<Turret, Invader, Vector2> InvaderBeamEffect = (Turret Turret, Invader hitInvader, Vector2 collisionEnd) =>
                {
                    #region The turret has a limited range
                    float TurretInvaderDist = Vector2.Distance(Turret.BarrelEnd, collisionEnd);
                    if (Turret.Range != 0)
                    {
                        #region The invader is in range
                        if (TurretInvaderDist <= Turret.Range)
                        {
                            //hitInvader.HitByBeam = true;
                            //hitInvader.BeamDelay = Turret.FireDelay;

                            //if (hitInvader.CurrentBeamDelay >= hitInvader.BeamDelay)
                            //{
                            //    hitInvader.TurretDamage(-Turret.Damage);
                            //    hitInvader.CurrentBeamDelay = 0;
                            //}                         
                        }
                        #endregion
                        else
                        #region The invader is out of range - reduce the damage accordingly
                        {
                            float OverRange = TurretInvaderDist - Turret.Range;
                            float DamageReduction = Math.Min((100 / (2 * Turret.Range)) * OverRange, 50);
                            float FinalDamage = ((100 - DamageReduction) / 100) * Turret.Damage;

                            //case TurretFireType.Beam:
                            //    hitInvader.HitByBeam = true;
                            //    hitInvader.BeamDelay = Turret.FireDelay;

                            //    if (hitInvader.CurrentBeamDelay >= hitInvader.BeamDelay)
                            //    {
                            //        hitInvader.TurretDamage((int)-FinalDamage);
                            //        hitInvader.CurrentBeamDelay = 0;
                            //    }
                            //    break;                            
                        }
                        #endregion
                    }
                    #endregion
                    else
                    {
                        //case TurretFireType.Beam:
                        hitInvader.HitByBeam = true;
                        hitInvader.BeamDelay = Turret.FireDelay;

                        if (hitInvader.CurrentBeamDelay >= hitInvader.BeamDelay)
                        {
                            hitInvader.TurretDamage(-Turret.Damage);
                            hitInvader.CurrentBeamDelay = 0;
                        }
                        //    break;                        
                    }

                    //switch (Turret.TurretType)
                    //{
                    //    //Create the blood/gibb/particle effects here 
                    //    //for when an invader is hit by a beam
                    //}
                };
                #endregion

                #region Determine what the trap does when hit by a beam
                Action<Vector2, Turret, Trap> TrapBeamEffect = (Vector2 CollisionEnd, Turret turret, Trap Trap) =>
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
                                    AddDrawable(Emitter);
                                    break;

                                case TrapType.Barrel:
                                    Trap.CurrentHP -= turret.Damage;
                                    break;
                            }
                            break;
                    }
                };
                #endregion

                #region Determine what the ground does when hit by a beam
                Action<Vector2, Vector2, LightProjectileType> GroundBeamEffect = (Vector2 CollisionStart, Vector2 CollisionEnd, LightProjectileType LightProjectileType) =>
                {
                    switch (LightProjectileType)
                    {
                        #region Persistent Beam
                        case LightProjectileType.PersistentBeam:
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

                            AddDrawable(DebrisEmitter, DebrisEmitter2, DustEmitter);

                            Decal NewDecal = new Decal(ExplosionDecal1, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                                       (float)RandomDouble(0, 0), new Vector2(690, 930), CollisionEnd.Y, 0.2f);

                            DecalList.Add(NewDecal);
                            break;
                        #endregion
                    }
                };
                #endregion

                foreach (Turret turret in TurretList)
                {
                    #region Check what was hit by the projectile - Invader, Trap, PowerUp Delivery, Ground or Sky
                    if (turret != null && turret.Active == true && turret.Selected == true && CurrentBeam != null)
                    {
                        #region Variables used for checking bullet collisions
                        //List of traps that intersect with the projectile ray AND are solid
                        List<Trap> HitTraps = TrapList.FindAll(Trap => Trap.BoundingBox.Intersects(CurrentBeam.Ray) != null && Trap.Solid == true);

                        //List of invaders that intersect with the projectile ray
                        List<Invader> HitInvaders = InvaderList.FindAll(Invader => Invader.BoundingBox.Intersects(CurrentBeam.Ray) != null);

                        Vector2 CollisionEnd;

                        Trap HitTrap;
                        Invader HitInvader;

                        float? MinDistInv, MinDistTrap, MinDistDelivery;
                        float MinDist;
                        #endregion

                        MinDistTrap = HitTraps.Min(Trap => Trap.BoundingBox.Intersects(CurrentBeam.Ray));
                        HitTrap = HitTraps.Find(Trap => Trap.BoundingBox.Intersects(CurrentBeam.Ray) == MinDistTrap);

                        MinDistInv = HitInvaders.Min(Invader => Invader.BoundingBox.Intersects(CurrentBeam.Ray));
                        HitInvader = HitInvaders.Find(Invader => Invader.BoundingBox.Intersects(CurrentBeam.Ray) == MinDistInv);

                        #region Add the minimum distances to the list of distances
                        //Consider changing this to an enumerated. Might be more efficient
                        List<float> DistanceList = new List<float>();

                        if (MinDistInv != null)
                            DistanceList.Add((float)MinDistInv);

                        if (MinDistTrap != null)
                            DistanceList.Add((float)MinDistTrap);

                        if (PowerupDelivery != null)
                        {
                            if (CurrentBeam.Ray.Intersects(PowerupDelivery.BoundingBox) != null)
                            {
                                MinDistDelivery = CurrentBeam.Ray.Intersects(PowerupDelivery.BoundingBox);
                                DistanceList.Add((float)MinDistDelivery);
                            }
                        }
                        #endregion

                        //If there was a collision with an object
                        if (DistanceList.Count > 0)
                        {
                            //Find the closest collision (Trap, invader of powerup deliver)
                            MinDist = DistanceList.Min();

                            #region Trap was closest
                            if (MinDist == MinDistTrap)
                            {
                                CollisionEnd = new Vector2(turret.BarrelEnd.X + (CurrentBeam.Ray.Direction.X * (float)MinDist),
                                                           turret.BarrelEnd.Y + (CurrentBeam.Ray.Direction.Y * (float)MinDist));

                                CreateBeamEffect(turret.BarrelEnd, CollisionEnd, CurrentBeam.LightProjectileType);
                                TrapBeamEffect(CollisionEnd, turret, HitTrap);
                            }
                            #endregion

                            #region Invader was closest
                            if (MinDist == MinDistInv)
                            {
                                CollisionEnd = new Vector2(turret.BarrelEnd.X + (CurrentBeam.Ray.Direction.X * (float)MinDist),
                                                           turret.BarrelEnd.Y + (CurrentBeam.Ray.Direction.Y * (float)MinDist));

                                CreateBeamEffect(turret.BarrelEnd, CollisionEnd, CurrentBeam.LightProjectileType);
                                InvaderBeamEffect(turret, HitInvader, CollisionEnd);

                                foreach (Invader invader in InvaderList.Where(Invader => Invader.HitByBeam == true))
                                {
                                    if (invader != HitInvader)
                                    {
                                        //invader.CurrentBeamDelay = 0;
                                        invader.HitByBeam = false;
                                    }
                                }
                            }
                            else
                            {
                                foreach (Invader invader in InvaderList.Where(Invader => Invader.HitByBeam == true))
                                {
                                    if (invader != HitInvader)
                                    {
                                        //invader.CurrentBeamDelay = 0;
                                        invader.HitByBeam = false;
                                    }
                                }
                            }
                            #endregion

                            #region Powerup Delivery was closest
                            if (PowerupDelivery != null && MinDist == CurrentBeam.Ray.Intersects(PowerupDelivery.BoundingBox))
                            {
                                CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentBeam.Ray.Direction.X * (float)MinDist),
                                                           turret.BarrelRectangle.Y + (CurrentBeam.Ray.Direction.Y * (float)MinDist));

                                CreateBeamEffect(turret.BarrelEnd, CollisionEnd, CurrentBeam.LightProjectileType);
                                //TrapEffect(CollisionEnd, turret, HitTrap);
                                PowerupDelivery.CurrentHP -= turret.Damage;
                            }
                            #endregion
                        }

                        #region Ground
                        //If a projectile doesn't hit a trap or an invader but does hit the ground
                        if (HitTraps.All(Trap => Trap.BoundingBox.Intersects(CurrentBeam.Ray) == null) &&
                            InvaderList.All(Invader => Invader.BoundingBox.Intersects(CurrentBeam.Ray) == null) &&
                            CurrentBeam.Ray.Intersects(Ground.BoundingBox) != null &&
                            CurrentBeam.Active == true)
                        {
                            if (PowerupDelivery == null || CurrentBeam.Ray.Intersects(PowerupDelivery.BoundingBox) == null)
                            {
                                var DistToGround = CurrentBeam.Ray.Intersects(Ground.BoundingBox);

                                if (DistToGround != null)
                                {
                                    CollisionEnd = new Vector2(turret.BarrelCenter.X + (CurrentBeam.Ray.Direction.X * (float)DistToGround),
                                                               turret.BarrelCenter.Y + (CurrentBeam.Ray.Direction.Y * (float)DistToGround));

                                    CollisionEnd.Y += (float)RandomDouble(-turret.AngleOffset * 5, turret.AngleOffset * 5);

                                    CreateBeamEffect(turret.BarrelEnd, CollisionEnd, CurrentBeam.LightProjectileType);
                                    GroundBeamEffect(turret.BarrelEnd, CollisionEnd, CurrentBeam.LightProjectileType);
                                }
                            }

                            foreach (Invader invader in InvaderList.Where(Invader => Invader.HitByBeam == true))
                            {
                                if (invader != HitInvader)
                                {
                                    //invader.CurrentBeamDelay = 0;
                                    invader.HitByBeam = false;
                                }
                            }
                        }
                        #endregion

                        #region Sky

                        //If a projectile doesn't hit a trap, an invader or the ground
                        if (HitTraps.All(Trap => Trap.BoundingBox.Intersects(CurrentBeam.Ray) == null) &&
                            InvaderList.All(Invader => Invader.BoundingBox.Intersects(CurrentBeam.Ray) == null) &&
                            CurrentBeam.Ray.Intersects(Ground.BoundingBox) == null &&
                            CurrentBeam.Active == true)
                        {
                            if (PowerupDelivery == null || CurrentBeam.Ray.Intersects(PowerupDelivery.BoundingBox) == null)
                            {
                                CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentBeam.Ray.Direction.X * 1920),
                                                           turret.BarrelRectangle.Y + (CurrentBeam.Ray.Direction.Y * 1920));

                                CreateBeamEffect(turret.BarrelEnd, CollisionEnd, CurrentBeam.LightProjectileType);
                            }

                            foreach (Invader invader in InvaderList.Where(Invader => Invader.HitByBeam == true))
                            {
                                if (invader != HitInvader)
                                {
                                    //invader.CurrentBeamDelay = 0;
                                    invader.HitByBeam = false;
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
            }
        }

        private void AddCluster(object source, int number, Vector2 position, Vector2 angleRange, HeavyProjectileType type, Vector2 yRange, float speed)
        {
            HeavyProjectile heavyProjectile;

            for (int i = 0; i < number; i++)
            {
                //MaxY = Random.Next((int)minY, 930);
                switch (type)
                {
                    case HeavyProjectileType.ClusterBomb:
                        heavyProjectile = new ClusterBomb(source, ClusterShellSprite, SmokeParticle, position, speed, (float)RandomDouble(angleRange.X, angleRange.Y), 0.3f, 5, yRange);
                        break;

                    default:
                        heavyProjectile = null;
                        break;
                }

                if (heavyProjectile != null)
                {
                    //heavyProjectile.Initialize();
                    HeavyProjectileList.Add(heavyProjectile);
                }
            }
        }
        #endregion
        
        #region TURRET stuff
        private void TurretUpdate(GameTime gameTime)
        {
            foreach (Turret turret in TurretList.Where(Turret => Turret != null && Turret.Active == true))
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

                                    CreateTurretShoot(turret);
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

                                    CreateTurretShoot(turret);
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
                                    //turret.ElapsedTime = 0;
                                    turret.Animated = true;

                                    if (turret.CurrentAnimation.TotalFrames > 0)
                                        turret.CurrentFrame = 1;
                                    else
                                        turret.CurrentFrame = 0;

                                    CreateTurretShoot(turret);
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
                    DrawableList.Remove(turret);
                    //Remove the outline from around the turret
                    UITurretOutlineList.RemoveAll(uiOutline => uiOutline.Turret == turret);
                    return;
                }
                #endregion
            }
        }
        

        public void OnTurretShoot(object source, TurretShootEventArgs e)
        {
            Turret turret = e.Turret;

            Vector2 MousePosition, Direction;
            HeavyProjectile HeavyProjectile;

            MousePosition = CursorPosition;

            turret.ChangeFireDirection();
            Direction = turret.FireDirection;
            Direction.Normalize();

            switch (turret.TurretType)
            {
                #region Machine Gun
                case TurretType.MachineGun:
                    {
                        MachineShot1.Play();

                        CurrentProfile.ShotsFired++;

                        CreateLightProjectile(new MachineGunProjectile(new Vector2(turret.BarrelEnd.X,
                                                                                   turret.BarrelEnd.Y), Direction), turret);

                        Emitter FlashEmitter = new Emitter(ExplosionParticle,
                            new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                            new Vector2(
                            MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)),
                            MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X))),
                            new Vector2(15, 30), new Vector2(16, 48), 1f, true, new Vector2(0, 360),
                            new Vector2(0, 0), new Vector2(1, 3), ExplosionColor, ExplosionColor2, 0.0f, 0.05f, 0.5f, 1,
                            false, new Vector2(0, 1080), true);
                        YSortedEmitterList.Add(FlashEmitter);

                        ShellCasing newShell = new ShellCasing(new Vector2(turret.BarrelCenter.X, turret.BarrelCenter.Y), new Vector2(5, 5), ShellCasing);
                        VerletShells.Add(newShell);

                        AddDrawable(newShell, FlashEmitter);

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
                                 new Vector2(50, 70), new Vector2(16, 48), 0.15f, true, new Vector2(0, 360),
                                 new Vector2(-1, 1), new Vector2(2, 4), ExplosionColor, ExplosionColor2, 0.0f, 0.05f, 1f, 2,
                                 false, new Vector2(0, 1080), true, 1);
                        YSortedEmitterList.Add(FlashEmitter);

                        Emitter FlashEmitter2 = new Emitter(ExplosionParticle,
                                new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                new Vector2(
                                MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) + 80,
                                MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) + 90),
                                new Vector2(10, 15), new Vector2(16, 48), 0.15f, true, new Vector2(0, 360),
                                new Vector2(-1, 1), new Vector2(2, 4), ExplosionColor, ExplosionColor2, 0.0f, 0.05f, 1f, 2,
                                false, new Vector2(0, 1080), true, 1);
                        YSortedEmitterList.Add(FlashEmitter2);

                        Emitter FlashEmitter3 = new Emitter(ExplosionParticle,
                                new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                new Vector2(
                                MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) - 90,
                                MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) - 80),
                                new Vector2(10, 15), new Vector2(16, 48), 0.15f, true, new Vector2(0, 360),
                                new Vector2(-1, 1), new Vector2(2, 4), ExplosionColor, ExplosionColor2, 0.0f, 0.05f, 1f, 2,
                                false, new Vector2(0, 1080), true, 1);
                        YSortedEmitterList.Add(FlashEmitter3);

                        Emitter SmokeEmitter = new Emitter(SmokeParticle,
                                new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                new Vector2(
                                MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)),
                                MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X))), new Vector2(4, 8),
                                new Vector2(480, 640), 0.2f, true, new Vector2(0, 360), new Vector2(0, 0),
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

                            HeavyProjectile = new CannonBall(turret, CannonBallSprite, SmokeParticle,
                                new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), turret.LaunchVelocity,
                                turret.FireRotation, 0.35f, turret.Damage, turret.BlastRadius,
                                new Vector2(Math.Max(AvgY - 32, 690), Math.Max(AvgY + 32, 930)));
                        }
                        else
                        {
                            HeavyProjectile = new CannonBall(turret, CannonBallSprite, SmokeParticle,
                                new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), turret.LaunchVelocity,
                                turret.FireRotation, 0.35f, turret.Damage, turret.BlastRadius,
                                new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));
                        }

                        //HeavyProjectile.Initialize();
                        HeavyProjectileList.Add(HeavyProjectile);

                        AddDrawable(HeavyProjectile, FlashEmitter, FlashEmitter2, FlashEmitter3, SmokeEmitter);

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
                        HeavyProjectile = new FlameProjectile(turret, FlameSprite, FireParticle2, new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                            (float)RandomDouble(7, 9), turret.Rotation, 0.3f, 5,
                            new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));

                        foreach (Emitter emitter in HeavyProjectile.EmitterList)
                        {
                            emitter.AngleRange = new Vector2(
                           -(MathHelper.ToDegrees((float)Math.Atan2(-HeavyProjectile.Velocity.Y, -HeavyProjectile.Velocity.X))) - 20,
                           -(MathHelper.ToDegrees((float)Math.Atan2(-HeavyProjectile.Velocity.Y, -HeavyProjectile.Velocity.X))) + 20);
                        }

                        //HeavyProjectile.Initialize();
                        HeavyProjectileList.Add(HeavyProjectile);
                    }
                    break;
                #endregion

                #region Lightning turret
                case TurretType.Lightning:
                    {
                        CreateLightProjectile(new LightningProjectile(new Vector2(turret.BarrelEnd.X,
                                                                                  turret.BarrelEnd.Y), Direction), turret);

                        Vector2 BarrelStart = new Vector2((float)Math.Cos(turret.Rotation) * (45),
                                                          (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height / 2));

                        Particle NewShellCasing = new Particle(LightningShellCasing,
                            new Vector2(turret.BarrelRectangle.X - BarrelStart.X, turret.BarrelRectangle.Y - BarrelStart.Y),
                            turret.Rotation - MathHelper.ToRadians((float)RandomDouble(175, 185)),
                            (float)RandomDouble(3, 5), 500, 1f, true, (float)RandomDouble(turret.Rotation, turret.Rotation),
                            (float)RandomDouble(-6, 6), 1f, Color.White, Color.Lerp(Color.White, Color.Transparent, 0.25f), 0.35f, true, Random.Next(Tower.DestinationRectangle.Bottom, Tower.DestinationRectangle.Bottom + 32),
                            false, null, true, true, true, false, new Vector2(0, 0), SpriteEffects.None, 4000
                            );

                        //ShellCasingList.Add(NewShellCasing);

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

                        heavyProjectile = new ClusterBombShell(turret, 1000, ClusterBombSprite, SmokeParticle,
                                            new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 12, turret.Rotation, 0.2f, 5, 0,
                                            new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));
                        //heavyProjectile.Initialize();

                        AddDrawable(heavyProjectile, FlashEmitter, FlashEmitter2, FlashEmitter3, SmokeEmitter);

                        //Particle NewShellCasing = new Particle(ShellCasing,
                        //    new Vector2(turret.BarrelRectangle.X, turret.BarrelRectangle.Y),
                        //    turret.Rotation - MathHelper.ToRadians((float)RandomDouble(175, 185)),
                        //    (float)RandomDouble(3, 6), 500, 1f, true, (float)RandomDouble(-10, 10),
                        //    (float)RandomDouble(-6, 6), 1.5f, Color.White, Color.White, 0.2f, true,
                        //    Random.Next(Tower.DestinationRectangle.Bottom, Tower.DestinationRectangle.Bottom + 32),
                        //    false, null, true);
                        //ShellCasingList.Add(NewShellCasing);
                    }
                    break;
                #endregion

                #region Fel Cannon
                case TurretType.FelCannon:
                    {
                        HeavyProjectile = new FelProjectile(turret, BlankTexture, RoundSparkParticle, SmokeParticle,
                                new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 5, turret.Rotation, 0.01f, turret.Damage, 100,
                                new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));

                        //HeavyProjectile.Initialize();
                        HeavyProjectileList.Add(HeavyProjectile);
                    }
                    break;
                #endregion

                #region Beam turret
                case TurretType.Beam:
                    {
                        CreateLightProjectile(new LaserProjectile(new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), Direction), turret);
                    }
                    break;
                #endregion

                #region Freeze turret
                case TurretType.Freeze:
                    {
                        CreateLightProjectile(new FreezeProjectile(new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), Direction), turret);
                    }
                    break;
                #endregion

                #region Boomerang turret
                case TurretType.Boomerang:
                    {
                        HeavyProjectile = new BoomerangProjectile(turret, BoomerangSprite, SmokeParticle, new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 12, turret.Rotation, 0.2f, turret.Damage, 100,
                            new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));

                        //HeavyProjectile.Initialize();
                        HeavyProjectileList.Add(HeavyProjectile);
                        //AddDrawable(HeavyProjectile);
                    }
                    break;
                #endregion

                #region Grenade launcher turret
                case TurretType.Grenade:
                    {
                        TimerHeavyProjectile heavyProjectile;

                        heavyProjectile = new GrenadeProjectile(turret, 2500, GrenadeSprite, SmokeParticle,
                            new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 8, turret.Rotation, 0.1f, 5, 0,
                            new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930), true);

                        //heavyProjectile.Initialize();
                        HeavyProjectileList.Add(heavyProjectile);
                        AddDrawable(heavyProjectile);

                        turret.CurrentHeat += turret.ShotHeat;
                    }
                    break;
                #endregion

                #region Gas Grenade Launcher Turret
                case TurretType.GasGrenade:
                    {
                        TimerHeavyProjectile heavyProjectile;

                        heavyProjectile = new GasGrenadeProjectile(turret, 2500, GrenadeSprite, SmokeParticle,
                            new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 16, turret.Rotation, 0.3f, 5, 0,
                            new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930), true);

                        //heavyProjectile./Initialize();

                        HeavyProjectileList.Add(heavyProjectile);
                        AddDrawable(heavyProjectile);

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
                            CreateLightProjectile(new ShotgunProjectile(new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), Direction), turret);

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
                        AddDrawable(FlashEmitter);

                        //Particle NewShellCasing = new Particle(ShellCasing,
                        //    new Vector2(turret.BarrelCenter.X, turret.BarrelCenter.Y),
                        //    turret.Rotation - MathHelper.ToRadians((float)RandomDouble(175, 185)),
                        //    (float)RandomDouble(4, 6), 500, 1f, true, (float)RandomDouble(-10, 10),
                        //    (float)RandomDouble(-6, 6), 0.7f, Color.Orange, Color.Lerp(Color.White, Color.Transparent, 0.25f), 0.2f, true, Random.Next(600, 630),
                        //    false, null, true);
                        //ShellCasingList.Add(NewShellCasing);                          

                        turret.CurrentHeat += turret.ShotHeat;
                    }
                    break;
                #endregion

                #region Persistent beam turret
                case TurretType.PersistentBeam:
                    {
                        if (CurrentBeam == null)
                        {
                            CurrentBeam = new PersistentBeamProjectile(CurrentTurret.BarrelEnd, CurrentTurret.FireDirection);
                        }
                    }
                    break;
                #endregion
            }
        }

        public void OnTurretClick(object source, EventArgs e)
        {
            Turret turret = source as Turret;

            ClearSelected();
            turret.Selected = true;
            CurrentTurret = turret;

            //Makes sure two turrets cannot be selected at the same time
            foreach (Turret turret2 in TurretList.Where(Turret => Turret != null && Turret != turret))
            {
                turret2.Selected = false;
            }
        }

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
        #endregion

        #region TRAP stuff
        private void TrapUpdate(GameTime gameTime)
        {
            //foreach (Trap trap in TrapList)
            UITrapOutlineList.RemoveAll(trapOutline => trapOutline.Trap == null || trapOutline.Trap.Active == false);
            UITrapQuickInfoList.RemoveAll(trapQuickInfo => trapQuickInfo.Trap == null || trapQuickInfo.Trap.Active == false);

            for (int t = 0; t < TrapList.Count; t++)
            {
                Trap trap = TrapList[t];

                trap.Update(gameTime);

                if (trap.Active == false)
                {
                    TrapList.Remove(trap);
                    DrawableList.Remove(trap);
                }

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
                    uiTrapQuickInfo.Detontations.Texture = WhiteBlock;
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

                        foreach (Emitter emitter in YSortedEmitterList.Where(emit => emit.Tether == trap))
                        {
                            emitter.AddMore = false;
                        }

                        TrapList.Remove(trap);
                        DrawableList.Remove(trap);

                        //Remove the outline from around the trap
                        UITrapOutlineList.RemoveAll(uiOutline => uiOutline.Trap == trap);
                        UITrapQuickInfoList.RemoveAll(trapQuickInfo => trapQuickInfo.Trap == trap);
                    }
                }
                #endregion

                #region Specific behaviour based on the trap type
                switch (trap.TrapType)
                {
                    #region Fire trap behaviour
                    case TrapType.Fire:
                        //#region Make less fire if the trap isn't ready to detonate yet
                        //if (trap.CurrentDetonateDelay < trap.DetonateDelay)
                        //{
                        //    foreach (Emitter emitter in YSortedEmitterList.Where(emit => emit.Tether == trap))
                        //    {
                        //        if (emitter.TextureName.Contains("Fire"))
                        //        {
                        //            emitter.HPRange = new Vector2(25, 50);
                        //            emitter.ScaleRange = new Vector2(0.01f, 0.05f);
                        //        }

                        //        if (emitter.TextureName.Contains("Smoke"))
                        //        {
                        //            emitter.HPRange = new Vector2(25, 50);
                        //            emitter.ScaleRange = new Vector2(0.1f, 0.3f);
                        //        }

                        //        if (emitter.TextureName.Contains("Spark"))
                        //        {
                        //            emitter.HPRange = new Vector2(25, 50);
                        //        }
                        //    }
                        //}
                        //#endregion

                        #region Reset the trap back to it's original state if it's ready to detonate again
                        //if (trap.CurrentDetonateDelay >= trap.DetonateDelay)
                        //{
                        //    foreach (Emitter emitter in YSortedEmitterList.Where(emit => emit.Tether == trap))
                        //    {
                        //        if (emitter.TextureName.Contains("Fire"))
                        //        {
                        //            emitter.HPRange = Vector2.Lerp(emitter.HPRange, new Vector2(90, 140), 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                        //            emitter.ScaleRange = Vector2.Lerp(emitter.ScaleRange, new Vector2(0.075f, 0.15f), 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                        //        }

                        //        if (emitter.TextureName.Contains("Smoke"))
                        //        {
                        //            emitter.HPRange = Vector2.Lerp(emitter.HPRange, new Vector2(250, 350), 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                        //            emitter.ScaleRange = Vector2.Lerp(emitter.ScaleRange, new Vector2(0.2f, 0.5f), 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                        //        }

                        //        if (emitter.TextureName.Contains("Spark"))
                        //        {
                        //            emitter.HPRange = Vector2.Lerp(emitter.HPRange, new Vector2(60, 180), 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                        //        }
                        //    }
                        //}
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
                            AddDrawable(ExplosionEmitter2);

                            Emitter ExplosionEmitter = new Emitter(FireParticle,
                                    new Vector2(trap.Position.X, trap.DestinationRectangle.Bottom),
                                    new Vector2(0, 180), new Vector2(1, 5), new Vector2(1, 20), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 7, false,
                                    new Vector2(0, 1080), true);

                            YSortedEmitterList.Add(ExplosionEmitter);
                            AddDrawable(ExplosionEmitter);

                            //Emitter newEmitter2 = new Emitter(SmokeParticle,
                            //        new Vector2(trap.Position.X, trap.DestinationRectangle.Bottom),
                            //        new Vector2(80, 100), new Vector2(0.5f, 1f), new Vector2(20, 40), 0.01f, true, new Vector2(0, 360),
                            //        new Vector2(0, 0), new Vector2(1, 2), SmokeColor1, SmokeColor2, 0.0f, 0.3f, 1, 1, false,
                            //        new Vector2(0, 1080), false);

                            //EmitterList2.Add(newEmitter2);

                            Explosion newExplosion = new Explosion(trap.Center, 150, 100);
                            CreateExplosion(newExplosion, trap);
                        }
                        break;

                    #endregion
                }
                #endregion
            }
        }

        public void OnTrapCollision(object source, TrapCollisionEventArgs e)
        {
            Trap HitTrap = e.Trap;
            Invader invader = e.Invader;

            if (HitTrap.Active == true)
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
                                foreach (Emitter emitter in YSortedEmitterList.Where(emit => emit.Tether == HitTrap && emit.TextureName.Contains("Fire")))
                                {
                                    emitter.EndColor = Color.Lerp(emitter.EndColor, Color.Red, 0.1f);
                                    emitter.StartColor = Color.Lerp(emitter.EndColor, Color.Red, 0.1f);
                                }
                                break;
                        }
                        break;
                    #endregion

                    #region Catapult trap
                    case TrapType.Catapult:
                        invader.Trajectory(new Vector2(4, -8));
                        break;
                    #endregion
                }

                #region Deactivate trap if it's reached the detonation limit
                if (HitTrap.CurrentDetonateLimit == 0)
                {
                    HitTrap.Active = false;

                    foreach (Emitter emitter in YSortedEmitterList.Where(emit => emit.Tether == HitTrap))
                    {
                        emitter.AddMore = false;
                    }
                }
                #endregion
            }
        }

        public void OnTrapPlacement(object source, EventArgs e)
        {
            #region Place Upgraded Trap
            Action<Trap, Vector2> PlaceUpgradedTrap = (trap, trapPosition) =>
            {
                /* The following is called separately inside each case so that the Destination Rectangle can be known
                        
                        NewTrap.Position = trapPosition;
                        TrapList.Add(NewTrap); AddDrawable(NewTrap);
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
                            NewTrap.AnimationList = WallAnimations;
                            NewTrap.TrapState = NewTrap.TrapState;

                            NewTrap.Position = trapPosition;
                            TrapList.Add(NewTrap);
                            AddDrawable(NewTrap);
                            NewTrap.Initialize();
                        }
                        break;
                    #endregion

                    #region Fire
                    case TrapType.Fire:
                        {
                            FireTrapStart.Play();

                            NewTrap = trap;
                            NewTrap.AnimationList = FireTrapAnimations;
                            NewTrap.TrapState = NewTrap.TrapState;
                            NewTrap.Position = trapPosition;
                            NewTrap.Initialize();
                            TrapList.Add(NewTrap);


                            Emitter FireEmitter = new Emitter(FireParticle,
                                new Vector2(NewTrap.Position.X + NewTrap.CurrentAnimation.FrameSize.X / 2, 
                                            NewTrap.Position.Y + NewTrap.CurrentAnimation.FrameSize.Y - 12), 
                                new Vector2(65, 105), new Vector2(0.5f, 0.75f), new Vector2(1500, 2000), 0.65f, true,
                                new Vector2(-0, 0), new Vector2(-0.25f, 0.25f), new Vector2(0.075f, 0.15f), FireColor, Color.Black,
                                0.0f, -1, 75, 1, false, new Vector2(0, 1080), true, (NewTrap.DestinationRectangle.Bottom + 1.0f) / 1080f, 
                                null, null, null, null, null, null, null, true, true, 1000);
                            FireEmitter.TextureName = "FireParticle";

                            //Emitter SmokeEmitter = new Emitter(SmokeParticle,
                            //    new Vector2(NewTrap.Position.X + NewTrap.CurrentAnimation.FrameSize.X / 2, NewTrap.Position.Y + NewTrap.CurrentAnimation.FrameSize.Y - 16),
                            //    new Vector2(85, 95), new Vector2(0.2f, 0.5f), new Vector2(250, 350), 0.9f, true, new Vector2(-20, 20),
                            //    new Vector2(-2, 2), new Vector2(0.6f, 1f), SmokeColor, SmokeColor2, 0.0f, -1, 150, 1, false,
                            //    new Vector2(0, 1080), true, (NewTrap.DestinationRectangle.Bottom) / 1080f, null, null, null, null, null, null, null, true, true, 250);
                            //SmokeEmitter.TextureName = "SmokeParticle";

                            Emitter SparkEmitter = new Emitter(RoundSparkParticle,
                                   new Vector2(NewTrap.Position.X + NewTrap.CurrentAnimation.FrameSize.X / 2, 
                                               NewTrap.Position.Y + NewTrap.CurrentAnimation.FrameSize.Y - 16),
                                   new Vector2(80, 100), new Vector2(1, 4), new Vector2(800, 1600), 1f, true, new Vector2(0, 0), 
                                   new Vector2(0, 0), new Vector2(0.1f, 0.3f), Color.LightYellow, Color.White, -0.001f, -1f, 100, 1, false,
                                   new Vector2(0, 1080), null, (NewTrap.DestinationRectangle.Bottom - 2) / 1080f);
                            SparkEmitter.TextureName = "SparkParticle";

                            YSortedEmitterList.Add(FireEmitter);
                            //YSortedEmitterList.Add(SmokeEmitter);
                            YSortedEmitterList.Add(SparkEmitter);

                            AddDrawable(NewTrap, FireEmitter, SparkEmitter);

                            FireEmitter.Tether = NewTrap;
                            //SmokeEmitter.Tether = NewTrap;
                            SparkEmitter.Tether = NewTrap;

                            Light light = new Light(new Vector3(NewTrap.Center.X, NewTrap.DestinationRectangle.Bottom - 8, 15), LightTexture)
                            {
                                Color = FireLightColor,
                                Active = true,
                                LightDecay = 100,
                                Power = 0.25f,
                                Radius = 100,
                                Range = 100,
                                Tether = NewTrap,
                                MaxTime = -1,
                                Oscillate = true,
                                CurrentOscillationTime = 0,
                                MaxOscillationTime = 500
                            };

                            //light.LightTexture = LightTexture;

                            LightList.Add(light);
                        }
                        break;
                    #endregion

                    #region Spikes
                    //case TrapType.Spikes:
                    //    {
                    //        NewTrap = trap;
                    //        NewTrap.TextureList = SpikeTrapSprite;
                    //        NewTrap.CurrentTexture = NewTrap.TextureList[0];

                    //        NewTrap.Position = trapPosition;
                    //        TrapList.Add(NewTrap); 
                    //        AddDrawable(NewTrap);
                    //        NewTrap.Initialize();
                    //    }
                    //    break;
                    #endregion

                    #region Catapult
                    case TrapType.Catapult:
                        {
                            NewTrap = trap;
                            NewTrap.AnimationList = CatapultTrapAnimations;
                            NewTrap.TrapState = NewTrap.TrapState;

                            NewTrap.Position = trapPosition;
                            TrapList.Add(NewTrap);
                            AddDrawable(NewTrap);
                            NewTrap.Initialize();
                        }
                        break;
                    #endregion

                    #region Ice
                    //case TrapType.Ice:
                    //    {
                    //        //NewTrap = trap;
                    //        //NewTrap.TextureList = IceTrapSprite;

                    //        //NewTrap.Position = trapPosition;
                    //        //TrapList.Add(NewTrap); AddDrawable(NewTrap);
                    //        //NewTrap.Initialize();

                    //        NewTrap = trap;
                    //        NewTrap.TextureList = IceTrapSprite;
                    //        NewTrap.CurrentTexture = NewTrap.TextureList[0];

                    //        NewTrap.Position = trapPosition;
                    //        TrapList.Add(NewTrap); 
                    //        AddDrawable(NewTrap);
                    //        NewTrap.Initialize();
                    //    }
                    //    break;
                    #endregion

                    #region Barrel
                    //case TrapType.Barrel:
                    //    {
                    //        NewTrap = trap;
                    //        NewTrap.TextureList = BarrelTrapSprite;
                    //        NewTrap.CurrentTexture = NewTrap.TextureList[0];

                    //        NewTrap.Position = trapPosition;
                    //        TrapList.Add(NewTrap); 
                    //        AddDrawable(NewTrap);
                    //        NewTrap.Initialize();
                    //    }
                    //    break;
                    #endregion

                    #region Sawblade
                    //case TrapType.SawBlade:
                    //    {
                    //        NewTrap = trap;
                    //        NewTrap.TextureList = SawBladeTrapSprite;
                    //        NewTrap.CurrentTexture = NewTrap.TextureList[0];

                    //        NewTrap.Position = trapPosition;
                    //        TrapList.Add(NewTrap); 
                    //        AddDrawable(NewTrap);
                    //        NewTrap.Initialize();
                    //    }
                    //    break;
                    #endregion

                    #region Line
                    //case TrapType.Line:
                    //    {
                    //        NewTrap = trap;
                    //        NewTrap.TextureList = LineTrapSprite;
                    //        NewTrap.CurrentTexture = NewTrap.TextureList[0];

                    //        NewTrap.Position = trapPosition;
                    //        TrapList.Add(NewTrap);
                    //        AddDrawable(NewTrap);
                    //        //DrawableList.
                    //        NewTrap.Initialize();

                    //        Emitter FireEmitter = new Emitter(FireParticle, new Vector2(NewTrap.Position.X, NewTrap.Position.Y),
                    //          new Vector2(0, 0), new Vector2(1.5f, 2.0f), new Vector2(40 * 1.5f, 60 * 1.5f), 0.01f, true, new Vector2(-4, 4),
                    //          new Vector2(-4, 4), new Vector2(1 * 1.5f, 2 * 1.5f), FireColor, FireColor2, 0.0f, -1, 25 * 1.5f, 3, false, new Vector2(0, 1080),
                    //          false, CursorPosition.Y / 1080);

                    //        NewTrap.TrapEmitterList.Add(FireEmitter);
                    //    }
                    //    break;
                    #endregion
                }

                ReadyToPlace = false;
                PlaceTrap.Play();
                ClearSelected();
            };
            #endregion

            //Place the trap fully upgraded onto the terrain when the mouse is clicked
            //Still need to do a better check if there is a trap under where a new trap is going to be placed
            if (CursorPosition.Y < 672 || CursorPosition.Y > 928)
            {
                return;
            }

            Vector2 NewTrapPosition = new Vector2(CursorPosition.X - CursorPosition.X % 32, CursorPosition.Y - CursorPosition.Y % 32);
            int resourceCost = TrapCost(SelectedTrap.Value);

            if (Resources >= resourceCost)
            {
                Resources -= resourceCost;

                Trap newTrap = ApplyTrapUpgrades(SelectedTrap.Value, NewTrapPosition);
                PlaceUpgradedTrap(newTrap, NewTrapPosition);

                //Make sure that the cooldowns are applied only to trap buttons as they're placed
                //i.e. not when the button is clicked
                for (int i = 0; i < CooldownButtonList.Count; i++)
                {
                    if (CurrentProfile.Buttons[i] != null && CurrentProfile.Buttons[i].CurrentTrap == newTrap.TrapType && StartWave == true)
                    {
                        CooldownButtonList[i].CoolingDown = true;
                    }
                }

                #region Effects to create when a trap is placed, e.g. A dust puff when a wall is placed
                switch (newTrap.TrapType)
                {
                    #region Wall
                    case TrapType.Wall:
                        Emitter dustEmitter = new Emitter(SmokeParticle,
                            new Vector2(newTrap.Center.X, newTrap.DestinationRectangle.Bottom),
                            new Vector2(160, 200), new Vector2(1, 4), new Vector2(20, 40), 0.25f, true, new Vector2(0, 360),
                            new Vector2(0, 1.5f), new Vector2(0.1f, 0.5f), DirtColor, DirtColor2, -0.05f, 0.2f, 3, 1, false,
                            new Vector2(0, 1080), false, null, null, null, null, null, null, null, new Vector2(0.05f, 0.05f));

                        Emitter dustEmitter2 = new Emitter(SmokeParticle,
                            new Vector2(newTrap.Center.X, newTrap.DestinationRectangle.Bottom),
                            new Vector2(-20, 20), new Vector2(1, 4), new Vector2(20, 40), 0.25f, true, new Vector2(0, 360),
                            new Vector2(0, 1.5f), new Vector2(0.1f, 0.5f), DirtColor, DirtColor2, -0.05f, 0.1f, 3, 1, false,
                            new Vector2(0, 1080), false, null, null, null, null, null, null, null, new Vector2(0.05f, 0.05f));

                        YSortedEmitterList.Add(dustEmitter);
                        YSortedEmitterList.Add(dustEmitter2);
                        AddDrawable(dustEmitter, dustEmitter2);
                        break;
                    #endregion

                    #region Fire
                    case TrapType.Fire:
                        AdditiveEmitterList.Add(new Emitter(RoundSparkParticle,
                            new Vector2(newTrap.Center.X, newTrap.DestinationRectangle.Bottom),
                            new Vector2(160, 200), new Vector2(1, 4), new Vector2(20, 40), 1f, true, new Vector2(0, 0),
                            new Vector2(0, 0), new Vector2(0.25f, 0.25f), FireColor, FireColor2, -0.05f, 0.2f, 3, 1, false,
                            new Vector2(0, 1080), false, null, null, null, null, null, null, null, new Vector2(0.05f, 0.05f)));

                        AdditiveEmitterList.Add(new Emitter(RoundSparkParticle,
                            new Vector2(newTrap.Center.X, newTrap.DestinationRectangle.Bottom),
                            new Vector2(-20, 20), new Vector2(1, 4), new Vector2(20, 40), 1f, true, new Vector2(0, 0),
                            new Vector2(0, 0), new Vector2(0.25f, 0.25f), FireColor, FireColor2, -0.05f, 0.1f, 3, 1, false,
                            new Vector2(0, 1080), false, null, null, null, null, null, null, null, new Vector2(0.05f, 0.05f)));
                        break;


                    #endregion
                }
                #endregion
            }
        }

        //Apply upgrades, return upgraded trap
        public Trap ApplyTrapUpgrades(TrapType trapType, Vector2 trapPosition)
        {
            switch (trapType)
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

        private int TrapPowerUsage(TrapType trapType)
        {
            int cost = 0;

            switch (trapType)
            {
                case TrapType.Fire:
                    cost = new FireTrap(Vector2.Zero).PowerCost;
                    break;

                case TrapType.Catapult:
                    cost = new CatapultTrap(Vector2.Zero).PowerCost;
                    break;

                case TrapType.Ice:
                    cost = new IceTrap(Vector2.Zero).PowerCost;
                    break;

                case TrapType.SawBlade:
                    cost = new SawBladeTrap(Vector2.Zero).PowerCost;
                    break;

                case TrapType.Spikes:
                    cost = new SpikeTrap(Vector2.Zero).PowerCost;
                    break;

                case TrapType.Wall:
                    cost = new Wall(Vector2.Zero).PowerCost;
                    break;

                case TrapType.Barrel:
                    cost = new BarrelTrap(Vector2.Zero).PowerCost;
                    break;
            }

            return cost;
        }
        #endregion

        #region SPECIAL stuff
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
                            TimerHeavyProjectile AirStrikeProjectile = new ClusterBombShell(CurrentSpecialAbility, 1000, ClusterBombSprite, SmokeParticle, airStrike.CurrentPosition, 10, 0, 0.2f, 5, 0, new Vector2(690, 930));
                            //AirStrikeProjectile.Initialize();
                            AddDrawable(AirStrikeProjectile);
                            airStrike.CurrentTime = 0;
                        }
                        break;
                }
            }
        }
        #endregion


        #region PROFILE handling
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
                    button.Initialize(OnButtonClick);
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
                DeleteProfileDialog.Initialize(OnButtonClick);
                CurrentDialogBox = DeleteProfileDialog;
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

        #region SETTINGS handling
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

        #region UPGRADES handling
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


        private void UpdateWeather(GameTime gameTime)
        {
            //Update the weather effects such as snow, sun, rain etc.
            CurrentWeatherTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            switch (CurrentWeather)
            {
                #region Snow
                case Weather.Snow:
                    {
                        if (CurrentWeatherTime > 250)
                        {
                            CurrentWeatherTime = 0;
                            WeatherSpriteList.Add(
                                new StaticSprite(RandomTexture(SnowDust1, SnowDust2, SnowDust3, SnowDust4, SnowDust5),
                                    new Vector2(Random.Next(250, 1920), Random.Next(690, 930)),
                                    new Vector2(0.5f, 0.5f), HalfWhite,
                                    new Vector2((float)RandomDouble(0.01f, 0.07f), 0), false, false, null, null, 2500, true));
                        }

                        foreach (StaticSprite sprite in WeatherSpriteList)
                        {
                            sprite.Update(gameTime);
                        }

                        BlurryEmitter.Update(gameTime);
                        FocusedEmitter.Update(gameTime);

                        BlurryEmitter.ParticleList.RemoveAll(Particle => Particle.CurrentPosition.Y > 1080);
                        FocusedEmitter.ParticleList.RemoveAll(Particle => Particle.CurrentPosition.Y > 1080);

                        BlurryEmitter.Position = new Vector2(Random.Next(0, 1920), -64);
                        FocusedEmitter.Position = new Vector2(Random.Next(0, 1920), -64);
                    }
                    break;
                #endregion
            }

            for (int i = 0; i < WeatherSpriteList.Count; i++)
            {
                if (WeatherSpriteList[i].Active == false)
                    WeatherSpriteList.RemoveAt(i);
            }

            WeatherSpriteList.RemoveAll(Sprite => Sprite.Active == false);
        }

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

            //if (GameState == GameState.Playing && IsLoading == false)
            //{
            CurrentWave = CurrentLevel.WaveList[CurrentWaveIndex];

            #region Use the "Start Waves" button to begin the next wave
            if (StartWaveButton != null)
            {
                StartWaveButton.Update(CursorPosition, gameTime);
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
                            float Multiplier = (float)RandomDouble(0.75f, 1.5f);

                            switch (nextInvader.InvaderType)
                            {
                                default:
                                    string invaderName = nextInvader.InvaderType.ToString();
                                    var invaderAnimationList = (List<InvaderAnimation>)this.GetType().GetField(invaderName + "Animations").GetValue(this);

                                    //Have to do it this way to make sure all the invaders aren't referencing the same animation
                                    nextInvader.AnimationList = new List<InvaderAnimation>(invaderAnimationList.Count);

                                    HeavyRangedInvader heavyRangedInvader = nextInvader as HeavyRangedInvader;
                                    LightRangedInvader lightRangedInvader = nextInvader as LightRangedInvader;

                                    //This is here to load the barrel sprite for the heavy ranged invaders
                                    if (heavyRangedInvader != null)
                                    {
                                        switch (heavyRangedInvader.InvaderType)
                                        {
                                            case InvaderType.StationaryCannon:
                                                {
                                                    heavyRangedInvader.BarrelAnimation = StationaryCannonBarrelAnimation;
                                                }
                                                break;
                                        }
                                    }

                                    for (int i = 0; i < invaderAnimationList.Count; i++)
                                    {
                                        nextInvader.AnimationList.Add(new InvaderAnimation()
                                        {
                                            Texture = invaderAnimationList[i].Texture,
                                            Animated = invaderAnimationList[i].Animated,
                                            Looping = invaderAnimationList[i].Looping,
                                            FrameDelay = invaderAnimationList[i].FrameDelay / Multiplier,
                                            TotalFrames = invaderAnimationList[i].TotalFrames,
                                            FrameSize = invaderAnimationList[i].FrameSize,
                                            NormalizedFrameSize = invaderAnimationList[i].NormalizedFrameSize,
                                            CurrentInvaderState = invaderAnimationList[i].CurrentInvaderState,
                                            AnimationType = invaderAnimationList[i].AnimationType,
                                            CurrentFrameDelay = 0
                                        });
                                    }
                                    break;
                            }

                            //This makes sure that the property change triggers
                            nextInvader.InvaderAnimationState = nextInvader.InvaderAnimationState;

                            nextInvader.IceBlock = IceBlock;
                            nextInvader.Shadow = Shadow;

                            //This gives the invaders a speed variation
                            //nextInvader.Direction.X *= Multiplier;
                            nextInvader.Speed *= Multiplier;

                            if (InvaderList.Find(Invader => Invader.MaxY == nextInvader.MaxY) != null)
                            {
                                nextInvader.MaxY += 2;
                            }

                            nextInvader.Initialize();
                            nextInvader.Update(gameTime, CursorPosition);
                            nextInvader.InvaderOutline = new UIOutline(nextInvader.Position,
                                                            new Vector2(nextInvader.CurrentAnimation.GetFrameSize().X,
                                                                        nextInvader.CurrentAnimation.GetFrameSize().Y),
                                                                       null, null, nextInvader);
                            nextInvader.InvaderOutline.OutlineTexture = TurretSelectBox;
                            InvaderList.Add(nextInvader);
                            AddDrawable(nextInvader);
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
                                StartWaveButton.Initialize(OnButtonClick);
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
                VictoryContinue = new Button(ButtonLeftSprite, new Vector2(-300, 834), null, null, null, "continue", RobotoRegular20_2, "Left", null);
                VictoryContinue.NextPosition = new Vector2(665, 834);
                VictoryContinue.Initialize(OnButtonClick);
                GameState = GameState.Victory;
            }
            #endregion
            //}
        }


        public void LoadLevel(int number)
        {
            //CurrentLevel = Content.Load<Level>("Levels/Level" + number);
            //var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

            Assembly assembly = Assembly.Load("TowerDefensePrototype");
            Type t = assembly.GetType("TowerDefensePrototype.Level" + number);
            CurrentLevel = (Level)Activator.CreateInstance(t);

            //Load the story dialogue used for this level here
            try
            {
                StoryDialogue = Content.Load<StoryDialogue>("StoryDialogue/StoryDialogue" + number);
            }
            catch (ContentLoadException)
            {                
                //throw;
            }

            PowerUnitsBar = new PartitionedBar(2, 20, new Vector2(850, 10), new Vector2(540, 958)); 
            PowerUnitsBar.Texture = WhiteBlock;

            //Load the resources used for this specific level (Can't be generalised) i.e. backgroung textures
            CurrentLevel.LoadContent(Content);
            CurrentWeather = CurrentLevel.StartWeather;

            #region Use those resources to create the background and foreground sprites
            Ground = new StaticSprite(CurrentLevel.GroundTexture, new Vector2(0, 1080 - 500 + 70));
            Ground.DrawDepth = 1.0f;
            Ground.BoundingBox = new BoundingBox(new Vector3(0, MathHelper.Clamp(CursorPosition.Y, 690, 1080), 0), new Vector3(1920, MathHelper.Clamp(CursorPosition.Y + 1, 800, 1080), 0));

            ForeGround = new StaticSprite(CurrentLevel.ForegroundTexture, new Vector2(0, 1080 - 400));
            ForeGround.DrawDepth = 1.0f;

            SkyBackground = new StaticSprite(CurrentLevel.SkyBackgroundTexture, new Vector2(0, 0));
            SkyBackground.DrawDepth = 1.0f;
            #endregion

            //CurrentLevel.AmbienceList[0].Play();
            //Handle this level's ambient sound effects
            //SoundEffectInstance Ambience0 = CurrentLevel.AmbienceList[0].CreateInstance();
            //Ambience0.IsLooped = true;
            //Ambience0.Volume = 0.2f;
            //LevelAmbience.Add(Ambience0);

            //LevelAmbience[0].Play();

            CurrentWaveIndex = 0;
            CurrentInvaderIndex = 0;

            CurrentWaveTime = 0;
            CurrentInvaderTime = 0;

            CurrentWave = CurrentLevel.WaveList[0];

            StartWave = false;

            Resources = CurrentLevel.Resources;
        }


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
                                    CurrentCursorTexture.Height), null, CursorColor, 0, Vector2.Zero,
                                    SpriteEffects.None, 1);

                    spriteBatch.Draw(PrimaryCursorTexture, new Rectangle((int)CursorPosition.X, (int)CursorPosition.Y,
                                     PrimaryCursorTexture.Width, PrimaryCursorTexture.Height), null,
                                     CursorColor, 0, Vector2.Zero, SpriteEffects.None, 1);
                }
                else
                {
                    if (SelectedTrap != null)
                    {
                        spriteBatch.Draw(CurrentCursorTexture, new Rectangle((int)(CursorPosition.X - CursorPosition.X % 32),
                                        (int)(CursorPosition.Y - CursorPosition.Y % 32), CurrentCursorTexture.Width, CurrentCursorTexture.Height),
                                        null, CursorColor, 0, Vector2.Zero, SpriteEffects.None, 1);
                    }
                    else
                    {
                        spriteBatch.Draw(CurrentCursorTexture, new Rectangle((int)(CursorPosition.X), (int)(CursorPosition.Y),
                                    CurrentCursorTexture.Width, CurrentCursorTexture.Height),
                                    null, CursorColor, 0, new Vector2(CurrentCursorTexture.Width / 2, CurrentCursorTexture.Height), SpriteEffects.None, 1);
                    }


                    if (PrimaryCursorTexture != CrosshairCursor)
                    {
                        spriteBatch.Draw(PrimaryCursorTexture, new Rectangle((int)CursorPosition.X, (int)CursorPosition.Y,
                                         PrimaryCursorTexture.Width, PrimaryCursorTexture.Height), null, Color.White,
                                         0, Vector2.Zero, SpriteEffects.None, 1);
                    }
                    else
                    {
                        spriteBatch.Draw(PrimaryCursorTexture, new Rectangle((int)CursorPosition.X - PrimaryCursorTexture.Width / 2,
                                         (int)CursorPosition.Y - PrimaryCursorTexture.Height / 2, PrimaryCursorTexture.Width,
                                         PrimaryCursorTexture.Height), null, Color.White, 0, Vector2.Zero,
                                         SpriteEffects.None, 1);
                    }
                }
            }
        }

        public Vector2 GetRealMousePosition()
        {
            if (CurrentSettings.FullScreen == false)
            {
                MouseTransform = Matrix.CreateTranslation(new Vector3(CurrentMouseState.X, CurrentMouseState.Y, 0)) *
                                 Matrix.CreateTranslation(new Vector3(0, 0, 0)) *
                                 Matrix.CreateScale(new Vector3((float)(ResolutionOffsetRatio / 2), (float)(ResolutionOffsetRatio / 2), 1));
            }
            else
            {
                MouseTransform = Matrix.CreateTranslation(new Vector3(0, -(ActualResolution.Y - CurrentSettings.ResHeight) / 2, 0)) *
                                 Matrix.CreateScale(new Vector3((float)(ResolutionOffsetRatio), (float)(ResolutionOffsetRatio), 1));

            }

            return Vector2.Transform(new Vector2(CurrentMouseState.X, CurrentMouseState.Y), MouseTransform);

            //CursorPosition.X = worldMouse.X;
            //CursorPosition.Y = worldMouse.Y;
        }


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
                turretBox.PowerUnitIcon = PowerUnitIcon;
                turretBox.RobotoBold40_2 = RobotoBold40_2;
                turretBox.RobotoRegular20_0 = RobotoRegular20_0;
                turretBox.RobotoRegular20_2 = RobotoRegular20_2;
                turretBox.RobotoItalic20_0 = RobotoItalic20_0;
                turretBox.LoadContent(SecondaryContent);

                turretBox.Visible = false;
                SelectTurretList.Add(turretBox);
                turretBox.ButtonClickHappened += OnButtonClick;
            }
            #endregion

            #region Trap boxes
            SelectTrapList = new List<WeaponBox>();
            var trapTypes = Enum.GetValues(typeof(TrapType));

            for (int i = 0; i < trapTypes.Length; i++)
            {
                WeaponBox trapBox = new WeaponBox(new Vector2(158 + 113 + (i * 282), 902 - 32), null, ApplyTrapUpgrades((TrapType)(i), Vector2.Zero));

                trapBox.ContainsTrap = (TrapType)trapTypes.GetValue(i);
                trapBox.PowerUnitIcon = PowerUnitIcon;
                trapBox.CurrencyIcon = CurrencyIcon;
                trapBox.RobotoBold40_2 = RobotoBold40_2;
                trapBox.RobotoRegular20_0 = RobotoRegular20_0;
                trapBox.RobotoRegular20_2 = RobotoRegular20_2;
                trapBox.RobotoItalic20_0 = RobotoItalic20_0;
                trapBox.LoadContent(SecondaryContent);

                trapBox.Visible = false;
                SelectTrapList.Add(trapBox);
                trapBox.ButtonClickHappened += OnButtonClick;
            }
            #endregion
            #endregion
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
                catch (Exception)
                {

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
                //switch (powerup.CurrentEffect.GeneralPowerup)
                //{
                //    //case GeneralPowerup.ExplosivePower:
                //    //    foreach (Explosion explosion in ExplosionList)
                //    //    {
                //    //        explosion.Damage = (float)PercentageChange(explosion.Damage, powerup.CurrentEffect.PowerupValue);
                //    //    }
                //    //    break;

                //    //case GeneralPowerup.BlastRadii:
                //    //    foreach (Explosion explosion in ExplosionList)
                //    //    {
                //    //        explosion.BlastRadius = (float)PercentageChange(explosion.BlastRadius, powerup.CurrentEffect.PowerupValue);
                //    //    }
                //    //    break;
                //}
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


        public void AddDrawable(params Drawable[] newDrawable)
        {
            //AddDrawable(newDrawable);
            DrawableList.AddRange(newDrawable);

            DrawableList.Sort((x, y) => x.DrawDepth.CompareTo(y.DrawDepth));
            //DrawableList = DrawableList.OrderBy(Drawable => Drawable.DrawDepth).ToList();

            //COULD ALSO TRY THIS - INSERT ITEMS IN CORRECT ORDER INSTEAD OF SORTING AFTERWARDS
            //MIGHT BE BETTER FOR A FEW REASONS. FASTER, NO RE-ORDERING ETC.
            //DrawableList.Insert(DrawableList.FindLastIndex(Drawable => Drawable.DrawDepth < newDrawable.DrawDepth), newDrawable);
            //DrawableList.Insert(DrawableList.FindIndex(Drawable => Drawable.DrawDepth < (newDrawable[0].DrawDepth)), newDrawable[0]);



            //DrawableList = DrawableList.OrderBy(Drawable => Drawable.DrawDepth).ToList();
            //Add a drawable item to the drawable list
            //Sort the list
            //
            //This means that the list is only sorted when it needs to be instead of every frame
        }

        public void CleanDrawableList()
        {
            //Remove all the inactive drawable items and corresponding items in InvaderList, TrapList etc.

            /*CONSIDER FINDING OBJECTS IN T_LIST THEN REMOVING THE SAME OBJECT FROM DRAWABLE LIST
             * Like this:
             * T_Object = ProjectileList.Find(Projectile.Active = false && Projectile.AddMore = false)
             * DrawableList.Remove(T_Object);
             * HeavyProjectileList.Remove(T_Object);
             * 
             * This is so that I can remove objects that don't have parameters present in Drawable
             * Such as checking if (Drawable)Projectile.AddMore == false;
            */

            ////Remove inactive invaders
            //InvaderList.RemoveAll(Invader => Invader.Active == false);
            //DrawableList.RemoveAll(Drawable => Drawable.GetType().BaseType == typeof(Invader) && 
            //                                   Drawable.Active == false);

            ////Remove inactive traps
            //TrapList.RemoveAll(Trap => Trap.Active == false);
            //DrawableList.RemoveAll(Drawable => Drawable.GetType().BaseType == typeof(Trap) && 
            //                                   Drawable.Active == false);

            ////Remove inactive projectiles
            //HeavyProjectileList.RemoveAll(HeavyProjectile => HeavyProjectile.Active == false &&
            //                                                 HeavyProjectile.EmitterList.All(Emitter => Emitter.ParticleList.Count == 0));
            //DrawableList.RemoveAll(Drawable => Drawable.GetType().BaseType == typeof(HeavyProjectile) && 
            //                                   Drawable.Active == false);


            //HeavyProjectileList.RemoveAll(HeavyProjectile => HeavyProjectile.Active == false &&
            //                                                 HeavyProjectile.EmitterList.All(Emitter => Emitter.ParticleList.Count == 0));
            //DrawableList.RemoveAll(Drawable => Drawable.GetType().BaseType == typeof(HeavyProjectile) && Drawable.Active == false);
        }

        private void SetUpGameWindow()
        {
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

            ScreenDrawRectangle = new Rectangle(0, (int)(ActualResolution.Y - CurrentSettings.ResHeight) / 2,
                                                CurrentSettings.ResWidth, CurrentSettings.ResHeight);
            #endregion
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

        protected Vector4 ColorToVector(Color color)
        {
            return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

        private Color RandomColor(params Color[] colors)
        {
            //List<Color> ColorList = colors.ToList();

            return colors[Random.Next(0, colors.Length)];
        }

        private Texture2D RandomTexture(params Texture2D[] textures)
        {
            //List<Texture2D> TextureList = textures.ToList();

            return textures[Random.Next(0, textures.Length)];
        }

        private SpriteEffects RandomOrientation(params SpriteEffects[] Orientations)
        {
            //List<SpriteEffects> OrientationList = new List<SpriteEffects>();

            //foreach (SpriteEffects orientation in Orientations)
            //{
            //    OrientationList.Add(orientation);
            //}

            return Orientations[Random.Next(0, Orientations.Length)];
        }

        private float LerpTime(float value1, float value2, float lerp, GameTime gameTime)
        {
            //Auomatically corrects for the gametime
            return MathHelper.Lerp(value1, value2, lerp * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60));
        }

        private Vector2 LerpVectorTime(Vector2 value1, Vector2 value2, float lerp, GameTime gameTime)
        {
            //Auomatically corrects for the gametime
            return Vector2.Lerp(value1, value2, lerp * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60));
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

        private void ClearSelected()
        {
            CurrentTurret = null;
            SelectedTurret = null;
            SelectedTrap = null;
            ReadyToPlace = false;
        }
        #endregion
    }
}