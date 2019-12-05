using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using System.Windows;
using GameDataTypes;
using System.Configuration;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Reflection;

namespace TowerDefensePrototype
{
    public enum TrapType { Wall, Spikes, Catapult, Fire, Ice, Tar, Barrel, SawBlade, Line, Trigger };
    public enum TurretType { MachineGun, Cannon, FlameThrower, Lightning, Cluster, FelCannon, Beam, Freeze, Boomerang, Grenade, Shotgun, PersistentBeam };
    public enum InvaderType { Soldier, BatteringRam, Airship, Archer, Tank, Spider, Slime, SuicideBomber, FireElemental, TestInvader };
    public enum HeavyProjectileType { CannonBall, FlameThrower, Arrow, Acid, Torpedo, ClusterBomb, ClusterBombShell, FelProjectile, Boomerang, Grenade };
    public enum LightProjectileType { MachineGun, Freeze, Lightning, Beam, Pulse, Shotgun, PersistentBeam };
    public enum GameState { Menu, Loading, Playing, Paused, ProfileSelect, Options, ProfileManagement, GettingName, Victory, Upgrades };
    public enum ProfileState { Standard, Upgrades, Stats };

    public struct StackedUpgrade
    {
        public float GatlingSpeed, GatlingDamage, GatlingAccuracy;
        public float CannonSpeed, CannonDamage, CannonBlastRadius;
    };

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Variable declarations

        GraphicsDeviceManager graphics;
        ContentManager SecondaryContent;
        SpriteBatch spriteBatch, targetBatch;
        RenderTarget2D GameRenderTarget, MenuRenderTarget, UIRenderTarget, BackgroundRenderTarget;
        RenderTarget2D ShaderTarget1, ShaderTarget2;
        Texture2D ScreenTex, MenuTex;

        string Error = "";

        #region XNA Declarations
        static Random Random = new Random();

        //Sprites
        #region Particle sprites
        Texture2D BlankTexture, ShellCasing, Coin, RoundSparkParticle,
                  HealthBarSprite, SplodgeParticle, SmokeParticle, FireParticle, BallParticle, SparkParticle,
                  BulletTrailCap, BulletTrailSegment;
        #endregion

        #region Icon sprites
        Texture2D LockIcon;

        //Turret icons
        public Texture2D BeamTurretIcon, BoomerangTurretIcon, CannonTurretIcon, ClusterTurretIcon, FelCannonTurretIcon, FlameThrowerTurretIcon,
                  FreezeTurretIcon, GrenadeTurretIcon, LightningTurretIcon, MachineGunTurretIcon, PersistentBeamTurretIcon, ShotgunTurretIcon;

        //Trap Icons
        public Texture2D CatapultTrapIcon, IceTrapIcon, TarTrapIcon, WallTrapIcon, BarrelTrapIcon, FireTrapIcon, LineTrapIcon, SawBladeTrapIcon,
                  SpikesTrapIcon;

        #endregion

        #region Cursor sprites
        Texture2D CurrentCursorTexture, PrimaryCursorTexture, DefaultCursor, CrosshairCursor;

        //Turret Cursors        
        public Texture2D MachineGunTurretCursor, CannonTurretCursor, FlameThrowerTurretCursor, LightningTurretCursor, ClusterTurretCursor,
                         FelCannonTurretCursor, BeamTurretCursor, FreezeTurretCursor, BoomerangTurretCursor, GrenadeTurretCursor, ShotgunTurretCursor,
                         PersistentBeamTurretCursor;

        //Trap Cursors
        public Texture2D WallTrapCursor, SpikesTrapCursor, CatapultTrapCursor, FireTrapCursor, IceTrapCursor, TarTrapCursor, BarrelTrapCursor,
                         SawBladeTrapCursor, LineTrapCursor, TriggerTrapCursor;
        #endregion

        #region Trap sprites
        List<Texture2D> BarrelTrapSprite, CatapultTrapSprite, IceTrapSprite, TarTrapSprite, WallSprite,
                        LineTrapSprite, SawBladeTrapSprite, SpikeTrapSprite, FireTrapSprite;
        #endregion

        #region Turret sprites
        Texture2D TurretSelectBox;
        public Texture2D MachineGunTurretBase, MachineGunTurretBarrel;
        public Texture2D CannonTurretBase, CannonTurretBarrel;
        public Texture2D LightningTurretBase, LightningTurretBarrel;
        #endregion

        #region Enemy sprites
        public Texture2D IceBlock, Shadow;
        public List<Texture2D> SoldierSprite, BatteringRamSprite, AirshipSprite, ArcherSprite, TankSprite, SpiderSprite,
                               SlimeSprite, SuicideBomberSprite, FireElementalSprite, TestInvaderSprite;

        #endregion

        #region Button sprites
        Texture2D SelectButtonSprite, ButtonLeftSprite, ButtonRightSprite, LeftArrowSprite, RightArrowSprite, SmallButtonSprite,
                  TextBoxSprite, WeaponBoxSprite, TurretSlotButtonSprite, DialogBox;
        #endregion

        #region This is for the horizontal health bars
        Texture2D WhiteBlock;
        #endregion

        Texture2D TerrainShrub1, TerrainRock1;
        Texture2D TooltipBox;

        #region Colour declarations
        public Color HalfWhite = Color.Lerp(Color.White, Color.Transparent, 0.5f);
        public Color FireColor = new Color(Color.White.R, Color.White.G, Color.White.B, 255);
        //public Color FireColor = new Color(Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 200);
        public Color FireColor2 = new Color(Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 90);
        public Color DirtColor = new Color(51, 31, 0, 100);
        public Color DirtColor2 = new Color(51, 31, 0, 125);
        public Color SmokeColor1 = Color.Lerp(Color.DarkGray, Color.Transparent, 0.25f);
        public Color SmokeColor2 = Color.Lerp(Color.Gray, Color.Transparent, 0.25f);
        public Color MenuColor = Color.White;
        #endregion

        

        Vector2 CursorPosition, ActualResolution;
        Rectangle ScreenRectangle;

        SpriteFont DefaultFont, TooltipFont, ResourceFont;

        MouseState CurrentMouseState, PreviousMouseState;
        KeyboardState CurrentKeyboardState, PreviousKeyboardState;

        int Resources, ResourceCost, TrapLimit, TowerButtons, ProfileNumber, LevelNumber, limit, TotalParticles;
        int CurrentWaveIndex = 0;
        int CurrentInvaderIndex = 0;
        int MaxWaves = 0;

        string FileName, ContainerName, ProfileName;

        bool ReadyToPlace, IsLoading, Slow;
        bool DialogVisible = false;
        bool Diagnostics = false;
        bool Tutorial = false;
        bool StartWave = false;

        float MenuSFXVolume, MenuMusicVolume, VictoryTime, CurrentInvaderTime, CurrentWaveTime, CurrentWavePauseTime, HealthValue;

        double Seconds, ResolutionOffsetRatio;

        

        BinaryFormatter formatter = new BinaryFormatter();

        Effect HealthBarEffect, ShockWaveEffect, ShieldBubbleEffect, BackgroundEffect, ButtonBlurEffect;
        Color CursorColor = Color.White;
        Matrix Projection, MouseTransform;
        #endregion

        #region Sound effects
        SoundEffect MenuClick, FireTrapStart, LightningSound, CannonExplosion, CannonFire,
                    MachineShot1, GroundImpact, Ricochet1, Ricochet2, Ricochet3, MenuWoosh,
                    PlaceTrap, Splat1, Splat2, MenuMusic, Implosion, TurretOverheat;

        SoundEffectInstance MenuMusicInstance, TurretOverheatInstance;
        #endregion

        #region List declarations
        List<Button> SelectButtonList, TowerButtonList, MainMenuButtonList, PauseButtonList,
                     ProfileButtonList, ProfileDeleteList, PlaceWeaponList, UpgradeButtonList;

        List<WeaponBox> TurretBoxes, TrapBoxes;

        List<Trap> TrapList;
        List<Turret> TurretList;
        List<Invader> InvaderList;

        List<HeavyProjectile> HeavyProjectileList, InvaderHeavyProjectileList;
        List<LightProjectile> InvaderLightProjectileList, LightProjectileList;
        List<TimerHeavyProjectile> TimedProjectileList;

        List<Emitter> EmitterList, EmitterList2, AlphaEmitterList, AdditiveEmitterList;
        List<Particle> ShellCasingList, CoinList;

        List<string> PauseMenuNameList;

        List<Explosion> ExplosionList, EnemyExplosionList;

        List<LightningBolt> LightningList;
        List<BulletTrail> TrailList, InvaderTrailList;

        List<NumberChange> NumberChangeList = new List<NumberChange>();

        List<StaticSprite> TerrainSpriteList;

        List<Decal> DecalList = new List<Decal>();
        List<Light> LightList = new List<Light>();
        #endregion

        #region Custom class declarations
        Button ProfileBackButton, ProfileManagementPlay, ProfileManagementBack,
               OptionsBack, OptionsSFXUp, OptionsSFXDown, OptionsMusicUp, OptionsMusicDown,
               GetNameOK, GetNameBack, SelectTrapRight, SelectTrapLeft, SelectTurretRight, SelectTurretLeft,
               VictoryReturn, VictoryRetry, VictoryComplete, ProfileManagementUpgrades, UpgradesBack;
        Tower Tower;
        StaticSprite Ground, ForeGround, ProfileMenuTitle, MainMenuBackground, SkyBackground, TextBox, MenuTower;
        AnimatedSprite LoadingAnimation;
        Nullable<TrapType> SelectedTrap;
        Nullable<TurretType> SelectedTurret;
        LightProjectile CurrentProjectile, CurrentInvaderProjectile;
        GameState GameState;
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
        #endregion

        #region Upgrade values
        float MachineGunTurretSpeed = 0;
        float MachineGunTurretAccuracy = 0;
        float MachineGunTurretDamage = 0;

        float CannonTurretSpeed = 0;
        float CannonTurretDamage = 0;
        float CannonTurretBlastRadius = 0;
        #endregion

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

            graphics = new GraphicsDeviceManager(this);
            graphics.SynchronizeWithVerticalRetrace = false;

            //this.TargetElapsedTime = TimeSpan.FromSeconds(1.0f / 60.0f);
            IsFixedTimeStep = false;
            //this.IsMouseVisible = true;

            Content.RootDirectory = "Content";

            #region Set up game window

            //Use ratio to calculate backbuffer size depending on the players selected resolution and the actual screen ratio
            //e.g. Selected resolution = 1280x720, screen size = 1920x1200, screen ratio = 16:10, game ratio = 16:9
            //Full screen resolution = 1280x800, render size = 1280x720, difference = 80/2
            //This makes sure that if the screen isn't 16:9 the player gets a letterboxed view instead of a distorted game.

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
            ContainerName = "Profiles";
            TrapLimit = 8;
            Camera = new Camera();
            Tower = new Tower("Tower", new Vector2(100, 500), 350, 20, 3, 5000);
            TowerButtons = (int)Tower.Slots;
            ScreenRectangle = new Rectangle(-256, -256, 1920 + 256, 1080 + 256);

            DefaultFont = SecondaryContent.Load<SpriteFont>("Fonts/DefaultFont");
            TooltipFont = SecondaryContent.Load<SpriteFont>("Fonts/TooltipFont");

            #region Load button sprites
            DialogBox = SecondaryContent.Load<Texture2D>("DialogBox");
            ButtonLeftSprite = SecondaryContent.Load<Texture2D>("Buttons/ButtonLeft");
            ButtonRightSprite = SecondaryContent.Load<Texture2D>("Buttons/ButtonRight");
            SelectButtonSprite = SecondaryContent.Load<Texture2D>("Buttons/Button");
            LeftArrowSprite = SecondaryContent.Load<Texture2D>("Buttons/LeftArrow");
            RightArrowSprite = SecondaryContent.Load<Texture2D>("Buttons/RightArrow");
            SmallButtonSprite = SecondaryContent.Load<Texture2D>("Buttons/SmallButton");
            TextBoxSprite = SecondaryContent.Load<Texture2D>("Buttons/TextBox");
            WeaponBoxSprite = SecondaryContent.Load<Texture2D>("Buttons/WeaponBox");
            TurretSlotButtonSprite = SecondaryContent.Load<Texture2D>("Buttons/TurretSlotButton");
            #endregion

            #region Initialise Main Menu

            MainMenuButtonList = new List<Button>();
            MainMenuButtonList.Add(new Button(ButtonLeftSprite, new Vector2(-300, 130), null, null, null, "          PLAY", DefaultFont, "Left", Color.White));
            MainMenuButtonList.Add(new Button(ButtonLeftSprite, new Vector2(-300, 130 + ((64 + 50) * 1)), null, null, null, "          TUTORIAL", DefaultFont, "Left", Color.White));
            MainMenuButtonList.Add(new Button(ButtonLeftSprite, new Vector2(-300, 130 + ((64 + 50) * 2)), null, null, null, "          OPTIONS", DefaultFont, "Left", Color.White));
            MainMenuButtonList.Add(new Button(ButtonLeftSprite, new Vector2(-300, 130 + ((64 + 50) * 3)), null, null, null, "          CREDITS", DefaultFont, "Left", Color.White));
            MainMenuButtonList.Add(new Button(ButtonLeftSprite, new Vector2(-300, 1080 - 50 - 32), null, null, null, "          EXIT", DefaultFont, "Left", Color.White));

            MainMenuBackground = new StaticSprite("Backgrounds/MenuBackground", new Vector2(0, 0));

            foreach (Button button in MainMenuButtonList)
            {
                button.LoadContent();
            }

            #endregion

            #region Initialise Pause Menu
            PauseMenuNameList = new List<string>();
            PauseMenuNameList.Add("Resume Game");
            PauseMenuNameList.Add("Options");
            PauseMenuNameList.Add("Main Menu");
            PauseMenuNameList.Add("Profile Menu");
            PauseMenuNameList.Add("Exit");

            PauseButtonList = new List<Button>();
            for (int i = 0; i < 4; i++)
            {
                PauseButtonList.Add(new Button(ButtonLeftSprite, new Vector2(0, 130 + ((64 + 50) * i)), null, null, null, PauseMenuNameList[i], DefaultFont, "Left", Color.White));
                PauseButtonList[i].LoadContent();
            }

            PauseButtonList.Add(new Button(ButtonLeftSprite, new Vector2(0, 1080 - 50 - 32), null, null, null, PauseMenuNameList[4], DefaultFont, "Left", Color.White));
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

            OptionsBack = new Button(ButtonLeftSprite, new Vector2(0, 1080 - 32 - 50), null, null, null, "Back", DefaultFont, "Left", Color.White);
            OptionsBack.LoadContent();

            #endregion

            #region Initialise Profile Select Menu

            ProfileButtonList = new List<Button>();
            for (int i = 0; i < 4; i++)
            {
                ProfileButtonList.Add(new Button(ButtonLeftSprite, new Vector2(50, 130 + (i * 114)), null, null, null, "empty", DefaultFont, "Centre", Color.White));
                ProfileButtonList[i].LoadContent();
            }

            ProfileDeleteList = new List<Button>();
            for (int i = 0; i < 4; i++)
            {
                ProfileDeleteList.Add(new Button(SmallButtonSprite, new Vector2(0, 130 + (i * 114)), null, null, null, "X", DefaultFont, "Left", Color.White));
                ProfileDeleteList[i].LoadContent();
            }

            ProfileBackButton = new Button(ButtonRightSprite, new Vector2(1920 + 300, 1080 - 32 - 50), null, null, null, "Back     ", DefaultFont, "Right", Color.White);
            ProfileBackButton.LoadContent();

            #endregion

            #region Initialise Profile Management Menu

            ProfileManagementPlay = new Button(ButtonRightSprite, new Vector2(1920 - 450 + 300, 1080 - 32 - 50), null, null, null, "Play     ", DefaultFont, "Right", Color.White);
            ProfileManagementPlay.LoadContent();

            ProfileManagementBack = new Button(ButtonLeftSprite, new Vector2(-300, 1080 - 32 - 50), null, null, null, "     Back", DefaultFont, "Left", Color.White);
            ProfileManagementBack.LoadContent();

            ProfileMenuTitle = new StaticSprite("ProfileMenuTitle", new Vector2(0, 32));
            ProfileMenuTitle.LoadContent(SecondaryContent);

            SelectTrapLeft = new Button(LeftArrowSprite, new Vector2(80 + 196, 314 + 96));
            SelectTrapLeft.LoadContent();

            SelectTrapRight = new Button(RightArrowSprite, new Vector2(1920 - 80 - 50, 314 + 96));
            SelectTrapRight.LoadContent();

            SelectTurretLeft = new Button(LeftArrowSprite, new Vector2(80 + 196, 170 + 32));
            SelectTurretLeft.LoadContent();

            SelectTurretRight = new Button(RightArrowSprite, new Vector2(1920 - 80 - 50, 170 + 32));
            SelectTurretRight.LoadContent();

            ProfileManagementUpgrades = new Button(ButtonLeftSprite, new Vector2(100, 100), null, null, null, "Upgrades", DefaultFont);
            ProfileManagementUpgrades.LoadContent();

            PlaceWeaponList = new List<Button>();
            for (int i = 0; i < 8; i++)
            {
                PlaceWeaponList.Add(new Button(SelectButtonSprite, new Vector2(118 + (i * 100), 1080 - 180), null, new Vector2(1f, 1f), null, "", null, "Left", null, true));
                //PlaceWeaponList[i].NextScale = new Vector2(0.35f, 0.35f);
                PlaceWeaponList[i].LoadContent();
            }

            TurretBoxes = new List<WeaponBox>();
            var turretTypes = Enum.GetValues(typeof(TurretType));

            for (int i = 0; i < turretTypes.Length; i++)
            {
                TurretBoxes.Add(new WeaponBox(WeaponBoxSprite, new Vector2(158 + 196 + (i * 196), 128 + 32), Vector2.One));
                TurretBoxes[i].LoadContent();
                TurretBoxes[i].ContainsTurret = (TurretType)turretTypes.GetValue(i);
            }

            TrapBoxes = new List<WeaponBox>();
            var trapTypes = Enum.GetValues(typeof(TrapType));

            for (int i = 0; i < trapTypes.Length; i++)
            {
                TrapBoxes.Add(new WeaponBox(WeaponBoxSprite, new Vector2(158 + 196 + (i * 196), 128 + 130 + 96), Vector2.One));
                TrapBoxes[i].LoadContent();
                TrapBoxes[i].ContainsTrap = (TrapType)trapTypes.GetValue(i);
            }

            MenuTower = new StaticSprite("Tower", new Vector2(1920 - 300, 150));
            MenuTower.LoadContent(SecondaryContent);

            #endregion

            #region Initialise Get Name Menu

            NameInput = new TextInput(new Vector2(1920 / 2 - 215, 1080 / 2 - 40), 350, "Fonts/DefaultFont", Color.White);
            NameInput.LoadContent(SecondaryContent);

            GetNameBack = new Button(ButtonLeftSprite, new Vector2(0, 1080 - 32 - 50), null, null, null, "     Back", DefaultFont, "Left", Color.White);
            GetNameBack.CurrentPosition.X = -300;
            GetNameBack.LoadContent();

            GetNameOK = new Button(ButtonRightSprite, new Vector2(1920 - 450, 1080 - 32 - 50), null, null, null, "Create     ", DefaultFont, "Right", Color.White);
            GetNameOK.LoadContent();

            TextBox = new StaticSprite("Buttons/TextBox", new Vector2((1920 / 2) - 225, (1080 / 2) - 50));
            TextBox.LoadContent(SecondaryContent);

            #endregion

            #region Initialise Upgrades Menu
            UpgradesBack = new Button(ButtonLeftSprite, new Vector2(0, 1080 - 32 - 50), null, null, null, "Back", DefaultFont, "Left");
            UpgradesBack.LoadContent();

            UpgradeButtonList = new List<Button>();
            for (int i = 0; i < 10; i++)
            {
                UpgradeButtonList.Add(new Button(WeaponBoxSprite, new Vector2(100 + (i * 92), 100), null, new Vector2(0.5f, 0.5f), Color.White, "Poop", DefaultFont, "Left", Color.White, false));
                UpgradeButtonList[i].NextScale = new Vector2(0.5f, 0.5f);
                UpgradeButtonList[i].LoadContent();
            }
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

            GraphicsDevice.SetRenderTarget(GameRenderTarget);
            GraphicsDevice.Clear(Color.White);
            //STUFF DRAWN BETWEEN TARGET1 AND TARGET2 CAN HAVE LIGHTS APPLIED TO IT

            #region Draw things in game that SHOULD be shaken - Non-diegetic elements
            if (GameState == GameState.Playing || GameState == GameState.Paused || GameState == GameState.Victory && IsLoading == false)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred,
                            BlendState.AlphaBlend,
                            null,
                            null,
                            null,
                            null,
                            Camera.Transformation(GraphicsDevice));

                SkyBackground.Draw(spriteBatch);

                Ground.Draw(spriteBatch);

                Tower.Draw(spriteBatch);

                #region Draw Traps

                foreach (Trap Trap in TrapList)
                {
                    if (Trap.DetonateDelay > 0)
                    {
                        Trap.TimingBar.Draw(spriteBatch);
                    }

                    if (Trap.MaxHP > 0)
                    {
                        Trap.HealthBar.Draw(spriteBatch);
                    }
                }

                #endregion

                foreach (Emitter emitter in EmitterList2)
                {
                    emitter.Draw(spriteBatch);
                }

                foreach (Decal decal in DecalList)
                {
                    decal.Draw(spriteBatch);
                }

                spriteBatch.End();
            }
            #endregion

            #region Draw stuff that SHOULD be shaken with ADDITIVE blending
            if (GameState == GameState.Playing || GameState == GameState.Paused || GameState == GameState.Victory && IsLoading == false)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate,
                BlendState.Additive,
                null,
                null,
                null,
                null,
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
                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, null, null, null, null, Camera.Transformation(GraphicsDevice));

                //This draws the timing bar for the turrets, but also makes 
                //sure that it doesn't draw for the blank turrets, which
                //would make the timing bar appear in the top right corner
                foreach (Turret turret in TurretList)
                {
                    if (turret != null)
                        if (turret.Active == true)
                            turret.Draw(spriteBatch);
                }


                foreach (StaticSprite terrainSprite in TerrainSpriteList)
                {
                    terrainSprite.Draw(spriteBatch);
                }


                foreach (Invader invader in InvaderList)
                {
                    invader.Draw(spriteBatch);
                }

                foreach (Particle shellCasing in ShellCasingList)
                {
                    shellCasing.Draw(spriteBatch);
                    shellCasing.DrawShadow(spriteBatch);
                }

                foreach (Emitter emitter in EmitterList)
                {
                    emitter.Draw(spriteBatch);
                }

                foreach (Trap trap in TrapList)
                {
                    trap.Draw(spriteBatch);
                }

                foreach (HeavyProjectile heavyProjectile in HeavyProjectileList)
                {
                    if (heavyProjectile.HeavyProjectileType != HeavyProjectileType.FelProjectile)
                    {
                        heavyProjectile.Draw(spriteBatch);
                        heavyProjectile.DrawShadow(spriteBatch);
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

                spriteBatch.End();
            }
            #endregion

            #region Draw with additive blending - Makes stuff look glowy
            if (GameState == GameState.Playing || GameState == GameState.Paused || GameState == GameState.Victory && IsLoading == false)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null);
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


            GraphicsDevice.SetRenderTarget(BackgroundRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);

            //Draw the menu background on a separate render target
            #region Draw background
            if (MainMenuBackground != null && GameState != GameState.Playing && GameState != GameState.Paused)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null);
                //BackgroundEffect.CurrentTechnique.Passes[0].Apply();
                MainMenuBackground.Draw(spriteBatch);
                spriteBatch.End();
            }
            #endregion

            GraphicsDevice.SetRenderTarget(MenuRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            //STUFF DRAWN AFTER HERE CANNOT HAVE LIGHT APPLIED TO IT
            #region Draw menus
            spriteBatch.Begin();
            if (GameState != GameState.Playing)
            {
                #region Draw Profile Select Menu
                if (GameState == GameState.ProfileSelect)
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
                #endregion

                #region Draw Profile Managment Screen
                if (GameState == GameState.ProfileManagement)
                {
                    ProfileMenuTitle.Draw(spriteBatch);
                    spriteBatch.DrawString(DefaultFont, CurrentProfile.Name, new Vector2(16, 37), HalfWhite);
                    spriteBatch.DrawString(DefaultFont, "Turrets", new Vector2(158 + 196, 116), HalfWhite);
                    spriteBatch.DrawString(DefaultFont, "Traps", new Vector2(158 + 196, 120 + 130 + 60), HalfWhite);
                    spriteBatch.DrawString(DefaultFont, "Selected Weapons", new Vector2(118, 1080 - 224), HalfWhite);
                    //spriteBatch.DrawString(ProfileFont, CurrentProfile.Name, new Vector2(32, 16), Color.White);
                    //spriteBatch.DrawString(ProfileFont, "Level: " + CurrentProfile.LevelNumber.ToString(), new Vector2(500, 16), Color.White);
                    //spriteBatch.DrawString(DefaultFont, "Points: " + CurrentProfile.Points.ToString(), new Vector2(640, 64), Color.White);
                    //spriteBatch.DrawString(DefaultFont, "Shots: " + CurrentProfile.ShotsFired.ToString(), new Vector2(640, 128), Color.White);
                    //spriteBatch.DrawString(DefaultFont, "Level: " + CurrentProfile.LevelNumber.ToString(), new Vector2(640, 192), Color.White);
                    //spriteBatch.DrawString(DefaultFont, SelectedTurret.ToString(), new Vector2(0, 0), Color.Red);
                    //spriteBatch.DrawString(DefaultFont, SelectedTrap.ToString(), new Vector2(0, 0), Color.Red);

                    ProfileManagementPlay.Draw(spriteBatch);
                    ProfileManagementBack.Draw(spriteBatch);
                    ProfileManagementUpgrades.Draw(spriteBatch);

                    foreach (WeaponBox turretBox in TurretBoxes)
                    {
                        if (turretBox.DestinationRectangle.Right > SelectTurretRight.DestinationRectangle.Center.X)
                        {
                            turretBox.Color = Color.Lerp(turretBox.Color, Color.Transparent, 0.4f);
                            turretBox.CurrentIconColor = Color.Lerp(turretBox.CurrentIconColor, Color.Transparent, 0.4f);
                        }

                        if (turretBox.DestinationRectangle.Left < SelectTurretLeft.DestinationRectangle.Center.X)
                        {
                            turretBox.Color = Color.Lerp(turretBox.Color, Color.Transparent, 0.4f);
                            turretBox.CurrentIconColor = Color.Lerp(turretBox.CurrentIconColor, Color.Transparent, 0.4f);
                        }


                        if (turretBox.DestinationRectangle.Right < SelectTurretRight.DestinationRectangle.Center.X &&
                            turretBox.DestinationRectangle.Center.X > SelectTurretLeft.DestinationRectangle.Center.X)
                        {
                            turretBox.Color = Color.Lerp(turretBox.Color, Color.White, 0.3f);
                            turretBox.CurrentIconColor = Color.Lerp(turretBox.CurrentIconColor, Color.White, 0.3f);
                        }

                        if (turretBox.DestinationRectangle.Left > SelectTurretLeft.DestinationRectangle.Center.X &&
                            turretBox.DestinationRectangle.Center.X < SelectTurretRight.DestinationRectangle.Center.X)
                        {
                            turretBox.Color = Color.Lerp(turretBox.Color, Color.White, 0.3f);
                            turretBox.CurrentIconColor = Color.Lerp(turretBox.CurrentIconColor, Color.White, 0.3f);
                        }
                    }

                    foreach (WeaponBox trapBox in TrapBoxes)
                    {
                        if (trapBox.DestinationRectangle.Right > SelectTurretRight.DestinationRectangle.Center.X)
                        {
                            trapBox.Color = Color.Lerp(trapBox.Color, Color.Transparent, 0.4f);
                            trapBox.CurrentIconColor = Color.Lerp(trapBox.CurrentIconColor, Color.Transparent, 0.4f);
                        }

                        if (trapBox.DestinationRectangle.Left < SelectTurretLeft.DestinationRectangle.Center.X)
                        {
                            trapBox.Color = Color.Lerp(trapBox.Color, Color.Transparent, 0.4f);
                            trapBox.CurrentIconColor = Color.Lerp(trapBox.CurrentIconColor, Color.Transparent, 0.4f);
                        }


                        if (trapBox.DestinationRectangle.Right < SelectTurretRight.DestinationRectangle.Center.X &&
                            trapBox.DestinationRectangle.Center.X > SelectTurretLeft.DestinationRectangle.Center.X)
                        {
                            trapBox.Color = Color.Lerp(trapBox.Color, Color.White, 0.3f);
                            trapBox.CurrentIconColor = Color.Lerp(trapBox.CurrentIconColor, Color.White, 0.3f);
                        }

                        if (trapBox.DestinationRectangle.Left > SelectTurretLeft.DestinationRectangle.Center.X &&
                            trapBox.DestinationRectangle.Center.X < SelectTurretRight.DestinationRectangle.Center.X)
                        {
                            trapBox.Color = Color.Lerp(trapBox.Color, Color.White, 0.3f);
                            trapBox.CurrentIconColor = Color.Lerp(trapBox.CurrentIconColor, Color.White, 0.3f);
                        }
                    }

                    foreach (Button button in PlaceWeaponList)
                    {
                        button.Draw(spriteBatch);
                    }

                    foreach (WeaponBox turretBox in TurretBoxes)
                    {
                        turretBox.Draw(spriteBatch);
                    }

                    foreach (WeaponBox trapBox in TrapBoxes)
                    {
                        trapBox.Draw(spriteBatch);
                    }

                    SelectTrapLeft.Draw(spriteBatch);
                    SelectTrapRight.Draw(spriteBatch);

                    SelectTurretLeft.Draw(spriteBatch);
                    SelectTurretRight.Draw(spriteBatch);

                    //MenuTower.Draw(spriteBatch);
                }
                #endregion

                #region Draw Loading Screen
                if (GameState == GameState.Loading)
                {
                    LoadingAnimation.Draw(spriteBatch);
                }
                #endregion

                #region Draw Main Menu
                if (GameState == GameState.Menu)
                {
                    foreach (Button button in MainMenuButtonList)
                    {
                        button.Draw(spriteBatch);
                    }
                }
                #endregion

                #region Draw Options Menu
                if (GameState == GameState.Options)
                {
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
                }
                #endregion

                #region Draw GetName Menu
                if (GameState == GameState.GettingName)
                {
                    spriteBatch.DrawString(DefaultFont, "Enter profile name:", new Vector2(TextBox.Position.X, TextBox.Position.Y - DefaultFont.MeasureString("E").Y), Color.White);
                    TextBox.Draw(spriteBatch);

                    GetNameBack.Draw(spriteBatch);
                    GetNameOK.Draw(spriteBatch);

                    NameInput.Draw(spriteBatch);
                }
                #endregion

                #region Draw Upgrades Menu
                if (GameState == GameState.Upgrades)
                {
                    spriteBatch.DrawString(DefaultFont, "Upgrades", new Vector2(0, 0), Color.White);

                    UpgradesBack.Draw(spriteBatch);

                    foreach (Button button in UpgradeButtonList)
                    {
                        button.Draw(spriteBatch);
                    }
                }
                #endregion

                #region Draw victory elements
                if (GameState == GameState.Victory)
                {
                    if (VictoryComplete != null)
                        VictoryComplete.Draw(spriteBatch);

                    if (VictoryRetry != null)
                        VictoryRetry.Draw(spriteBatch);

                    if (VictoryReturn != null)
                        VictoryReturn.Draw(spriteBatch);
                }
                #endregion

                #region Draw menu Dialog boxes
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
                #endregion

                CursorDraw(spriteBatch);

                //spriteBatch.Draw(MenuFade, ScreenRectangle, MenuColor);
            }

            spriteBatch.End();
            #endregion

            GraphicsDevice.SetRenderTarget(UIRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            //Draw in-game UI elements on a separate render target
            #region Draw the UI elements
            if (GameState == GameState.Playing || GameState == GameState.Paused && IsLoading == false)
            {
                #region Draw round health bar
                HealthValue = (float)((double)Tower.CurrentHP / (double)Tower.MaxHP);
                HealthBarEffect.Parameters["meterValue"].SetValue(HealthValue);

                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.LinearWrap,
                                  DepthStencilState.None, RasterizerState.CullCounterClockwise, HealthBarEffect);
                spriteBatch.Draw(HealthBarSprite, new Vector2(80, 80), null, Color.White, MathHelper.ToRadians(-90),
                                 new Vector2(HealthBarSprite.Width / 2, HealthBarSprite.Height / 2), 1f, SpriteEffects.None, 0);
                spriteBatch.End();
                #endregion

                spriteBatch.Begin();
                #region Draw UI numbers - health, sheield, resources etc.
                int PercentageHP = (int)MathHelper.Clamp((float)((100d / (double)Tower.MaxHP) * (double)Tower.CurrentHP), 0, 100);
                int PercentageShield = (int)MathHelper.Clamp((float)((100d / (double)Tower.MaxShield) * (double)Tower.CurrentShield), 0, 100);

                spriteBatch.DrawString(DefaultFont, PercentageHP.ToString() + "%", new Vector2(1920 / 2 + 1, 12), Color.Black);
                spriteBatch.DrawString(DefaultFont, PercentageHP.ToString() + "%", new Vector2(1920 / 2, 11), Color.LightGray);

                spriteBatch.DrawString(DefaultFont, PercentageShield.ToString() + "%", new Vector2(1920 / 2 + 1, 34), Color.Black);
                spriteBatch.DrawString(DefaultFont, PercentageShield.ToString() + "%", new Vector2(1920 / 2, 33), Color.LightGray);

                spriteBatch.DrawString(DefaultFont, Resources.ToString(), new Vector2(48, 48), Color.Black);

                foreach (NumberChange numChange in NumberChangeList)
                {
                    numChange.Draw(spriteBatch);
                }

                if (WaveCountDown != null && WaveCountDown.CurrentSeconds > -1 && WaveCountDown.Text != null)
                    WaveCountDown.Draw(spriteBatch);

                if (ResourceCost > 0)
                    spriteBatch.DrawString(DefaultFont, ResourceCost.ToString(), new Vector2(550, 950), Color.White);
                #endregion

                #region Drawing buttons
                if (StartWaveButton != null)
                    StartWaveButton.Draw(spriteBatch);

                foreach (Button button in SelectButtonList)
                {
                    button.Draw(spriteBatch);
                }

                //Draw tooltips for the buttons
                if (InGameInformation != null)
                    InGameInformation.Draw(spriteBatch);

                #endregion

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
                    spriteBatch.DrawString(DefaultFont, "Emitter1:" + EmitterList.Count.ToString(), new Vector2(0, 148), Color.Lime);
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
                }
                #endregion

                #region Draw turret UI stuff
                foreach (Turret turret in TurretList)
                {
                    if (turret != null)
                    {
                        turret.TimingBar.Draw(spriteBatch);
                        turret.HealthBar.Draw(spriteBatch);
                        turret.HeatBar.Draw(spriteBatch);

                        if (turret.SelectBox.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y))
                            && turret.Selected == false
                            && GameState == GameState.Playing)
                        {
                            spriteBatch.Draw(turret.Rect, turret.SelectBox, Color.White);
                        }
                    }
                }
                #endregion

                #region Draw pause menu buttons
                if (GameState == GameState.Paused)
                {
                    foreach (Button button in PauseButtonList)
                    {
                        button.Draw(spriteBatch);
                    }
                }
                #endregion
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

            spriteBatch.Begin();
            #region Draw pause menu Dialog boxes
            //if (GameState != GameState.Playing)
            {
                if (ExitDialog != null)
                {
                    ExitDialog.Draw(spriteBatch);
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
            #endregion
            spriteBatch.End();


            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            //#region Draw Victory Menu
            //if (GameState == GameState.Victory && IsLoading == false)
            //{
            //    if (VictoryRetry != null)
            //        VictoryRetry.Draw(spriteBatch);

            //    if (VictoryComplete != null)
            //        VictoryComplete.Draw(spriteBatch);

            //    if (VictoryReturn != null)
            //        VictoryReturn.Draw(spriteBatch);

            //    spriteBatch.DrawString(DefaultFont, "VICTORY", new Vector2(1920 / 2 - DefaultFont.MeasureString("VICTORY").X / 2, 16), Color.White);
            //}
            //#endregion

            //#region Draw Pause Menu
            //if (GameState == GameState.Paused)
            //{
            //    foreach (Button button in PauseButtonList)
            //    {
            //        button.Draw(spriteBatch);
            //    }
            //}
            //#endregion

            //#region Draw Dialog Boxes

            //#endregion
            //spriteBatch.End();


            #region Draw the cursor on top of EVERYTHING
            spriteBatch.Begin();
            CursorDraw(spriteBatch);
            spriteBatch.End();
            #endregion
            #endregion

            #region Handle render target stuff
            #region Layer up all the shaders that need to be applied to the game world - Not including the interface
            if (GameState == GameState.Playing || GameState == GameState.Paused)
            {
                ShieldBubbleEffect.Parameters["ScreenTexture"].SetValue(GameRenderTarget);
                ShockWaveEffect.Parameters["ScreenTexture"].SetValue(GameRenderTarget);
                ScreenTex = HandleShaders(GameRenderTarget,
                                          ShockWaveEffect.CurrentTechnique.Passes[0],
                                          ShieldBubbleEffect.CurrentTechnique.Passes[0]
                                          );
            }
            #endregion

            #region Layer up the h and v blurs for the menu
            if (GameState != GameState.Playing && GameState != GameState.Paused)
            {
                ButtonBlurEffect.Parameters["ScreenTexture"].SetValue(BackgroundRenderTarget);
                MenuTex = HandleShaders(MenuRenderTarget);
            }
            #endregion

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            #region DRAW ACTUAL GAME
            if (GameState == GameState.Playing || GameState == GameState.Paused)
            {
                if (ScreenTex != null)
                {
                    targetBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null);
                    targetBatch.Draw(ScreenTex, new Rectangle(0, (int)(ActualResolution.Y - CurrentSettings.ResHeight) / 2,
                                     CurrentSettings.ResWidth, CurrentSettings.ResHeight), null, Color.White, 0, Vector2.Zero,
                                     SpriteEffects.None, 0);
                    targetBatch.DrawString(DefaultFont, Error, new Vector2(100, 100), Color.Red);
                    targetBatch.End();
                }
            }
            #endregion

            #region DRAW THE MENU BACKGROUND AND MENU ITEMS
            if (GameState != GameState.Playing)
            {
                if (MainMenuBackground != null &&
                    BackgroundRenderTarget != null)
                {
                    targetBatch.Begin();
                    targetBatch.Draw(BackgroundRenderTarget, new Rectangle(0, (int)(ActualResolution.Y - CurrentSettings.ResHeight) / 2,
                                     CurrentSettings.ResWidth, CurrentSettings.ResHeight), null, Color.White, 0, Vector2.Zero,
                                     SpriteEffects.None, 0);
                    targetBatch.DrawString(DefaultFont, Error, new Vector2(100, 100), Color.Red);
                    targetBatch.End();
                }

                if (MenuRenderTarget != null)
                {
                    targetBatch.Begin();
                    targetBatch.Draw(MenuTex, new Rectangle(0, (int)(ActualResolution.Y - CurrentSettings.ResHeight) / 2,
                                     CurrentSettings.ResWidth, CurrentSettings.ResHeight), null, Color.White, 0, Vector2.Zero,
                                     SpriteEffects.None, 0);
                    targetBatch.DrawString(DefaultFont, Error, new Vector2(100, 100), Color.Red);
                    targetBatch.End();
                }
            }
            #endregion

            #region DRAW THE UI ITEMS, MOUSE, ETC.
            if (GameState == GameState.Playing ||
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

            if (SelectedTurret != null)
                SelectedTrap = null;

            if (SelectedTrap != null)
                SelectedTurret = null;

            if (GameState == GameState.Playing && IsLoading == false)
            {
                if (CurrentMouseState.LeftButton == ButtonState.Released &&
                    CurrentKeyboardState.IsKeyUp(Keys.Escape) &&
                    PreviousKeyboardState.IsKeyDown(Keys.Escape))
                {
                    GameState = GameState.Paused;
                }

                if (CurrentMouseState.LeftButton == ButtonState.Released &&
                   CurrentKeyboardState.IsKeyUp(Keys.Space) &&
                   PreviousKeyboardState.IsKeyDown(Keys.Space))
                {
                    Diagnostics = true;
                }

                Seconds += gameTime.ElapsedGameTime.TotalMilliseconds;

                Tower.Update(gameTime);

                InvaderUpdate(gameTime);

                RightClickClearSelected();

                SelectButtonsUpdate(gameTime);
                TowerButtonUpdate(gameTime);

                AttackTower();
                AttackTraps();

                RangedAttackTower(gameTime);
                RangedAttackTraps();
                HandleWaves(gameTime);

                if (WaveCountDown != null && WaveCountDown.CurrentSeconds > -1)
                    WaveCountDown.Update(gameTime);

                if (GameState != GameState.Playing)
                    return;

                #region Handle Particle Emitters and Particles
                for (int i = 0; i < ShellCasingList.Count; i++)
                {
                    if (ShellCasingList[i].Active == false)
                        ShellCasingList.RemoveAt(i);
                }

                for (int i = 0; i < CoinList.Count; i++)
                {
                    if (CoinList[i].Active == false)
                        CoinList.RemoveAt(i);
                }

                //foreach (Particle coin in CoinList)
                //{
                //    if (coin.BouncedOnGround == true && coin.Velocity.X == 0)
                //    {
                //        coin.Velocity = Vector2.Zero;
                //        coin.CurrentPosition = Vector2.SmoothStep(coin.CurrentPosition, new Vector2(100, 100), 0.1f);
                //    }
                //}

                #region Handle EmitterList2
                foreach (Emitter emitter in EmitterList2)
                {
                    emitter.Update(gameTime);
                }

                for (int i = 0; i < EmitterList2.Count; i++)
                {
                    if (EmitterList2[i].Active == false)
                        EmitterList2.RemoveAt(i);
                }

                for (int i = 0; i < EmitterList2.Count; i++)
                {
                    if (EmitterList2[i].AddMore == false && EmitterList2[i].ParticleList.Count == 0)
                        EmitterList2.RemoveAt(i);
                }
                #endregion

                #region Handle EmitterList
                foreach (Emitter emitter in EmitterList)
                {
                    emitter.Update(gameTime);
                }

                for (int i = 0; i < EmitterList.Count; i++)
                {
                    if (EmitterList[i].Active == false)
                        EmitterList.RemoveAt(i);
                }

                for (int i = 0; i < EmitterList.Count; i++)
                {
                    if (EmitterList[i].AddMore == false && EmitterList[i].ParticleList.Count == 0)
                        EmitterList.RemoveAt(i);
                }
                #endregion

                #region Handle AlphaEmitterList
                foreach (Emitter emitter in AlphaEmitterList)
                {
                    emitter.Update(gameTime);
                }

                for (int i = 0; i < AlphaEmitterList.Count; i++)
                {
                    if (AlphaEmitterList[i].Active == false)
                        AlphaEmitterList.RemoveAt(i);
                }

                for (int i = 0; i < AlphaEmitterList.Count; i++)
                {
                    if (AlphaEmitterList[i].AddMore == false && AlphaEmitterList[i].ParticleList.Count == 0)
                        AlphaEmitterList.RemoveAt(i);
                }
                #endregion

                #region Handle AdditiveEmitterList
                foreach (Emitter emitter in AdditiveEmitterList)
                {
                    emitter.Update(gameTime);
                }

                for (int i = 0; i < AdditiveEmitterList.Count; i++)
                {
                    if (AdditiveEmitterList[i].Active == false)
                        AdditiveEmitterList.RemoveAt(i);
                }

                for (int i = 0; i < AdditiveEmitterList.Count; i++)
                {
                    if (AdditiveEmitterList[i].AddMore == false && AdditiveEmitterList[i].ParticleList.Count == 0)
                        AdditiveEmitterList.RemoveAt(i);
                }
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

                foreach (Particle coin in CoinList)
                {
                    coin.Update(gameTime);
                }

                TotalParticles = 0;
                foreach (Emitter emitter in EmitterList)
                {
                    TotalParticles += emitter.ParticleList.Count;
                }

                foreach (Emitter emitter2 in EmitterList2)
                {
                    TotalParticles += emitter2.ParticleList.Count;
                }
                #endregion

                #region Projectile update stuff
                HeavyProjectileUpdate(gameTime);
                TimedProjectileUpdate(gameTime);
                LightProjectileUpdate();

                InvaderHeavyProjectileUpdate(gameTime);
                InvaderLightProjectileUpdate(gameTime);

                for (int i = 0; i < TimedProjectileList.Count; i++)
                {
                    if (TimedProjectileList[i].Active == false)
                        TimedProjectileList.RemoveAt(i);
                }

                for (int i = 0; i < HeavyProjectileList.Count; i++)
                {
                    if (HeavyProjectileList[i].Active == false && HeavyProjectileList[i].EmitterList.All(Emitter => Emitter.ParticleList.Count == 0 && Emitter.AddMore == false))
                        HeavyProjectileList.RemoveAt(i);
                }


                for (int i = 0; i < InvaderHeavyProjectileList.Count; i++)
                {
                    if (InvaderHeavyProjectileList[i].Active == false && InvaderHeavyProjectileList[i].EmitterList.All(Emitter => Emitter.ParticleList.Count == 0))
                        InvaderHeavyProjectileList.RemoveAt(i);
                }

                for (int i = 0; i < HeavyProjectileList.Count; i++)
                {
                    if (!ScreenRectangle.Contains(new Point((int)HeavyProjectileList[i].Position.X, (int)HeavyProjectileList[i].Position.Y)))
                        HeavyProjectileList.RemoveAt(i);
                }

                for (int i = 0; i < InvaderHeavyProjectileList.Count; i++)
                {
                    if (!ScreenRectangle.Contains(new Point((int)InvaderHeavyProjectileList[i].Position.X, (int)InvaderHeavyProjectileList[i].Position.Y)))
                        InvaderHeavyProjectileList.RemoveAt(i);
                }
                #endregion

                #region Turret stuff.
                TurretUpdate();

                foreach (Turret turret in TurretList)
                {
                    if (turret != null)
                        if (turret.Active == true)
                            turret.Update(gameTime, CursorPosition);
                }
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

                for (int i = 0; i < TrapList.Count; i++)
                {
                    if (TrapList[i].Active == false)
                        TrapList.RemoveAt(i);
                }
                #endregion

                #region Update effects
                for (int i = 0; i < InvaderList.Count; i++)
                {
                    if (InvaderList[i].Active == false)
                        InvaderList.RemoveAt(i);
                }

                foreach (LightningBolt lightningBolt in LightningList)
                {
                    lightningBolt.Update(gameTime);
                }

                foreach (BulletTrail trail in TrailList)
                {
                    trail.Update(gameTime);
                }

                foreach (BulletTrail trail in InvaderTrailList)
                {
                    trail.Update(gameTime);
                }

                for (int i = 0; i < LightningList.Count; i++)
                {
                    if (LightningList[i].Alpha <= 0)
                        LightningList.Remove(LightningList[i]);
                }

                for (int i = 0; i < TrailList.Count; i++)
                {
                    if (TrailList[i].Alpha <= 0)
                        TrailList.Remove(TrailList[i]);
                }

                for (int i = 0; i < InvaderTrailList.Count; i++)
                {
                    if (InvaderTrailList[i].Alpha <= 0)
                        InvaderTrailList.Remove(InvaderTrailList[i]);
                }

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

                for (int i = 0; i < NumberChangeList.Count; i++)
                {
                    if (NumberChangeList[i].Active == false)
                    {
                        NumberChangeList.RemoveAt(i);
                    }
                }
                #endregion

                if (InGameInformation != null)
                    InGameInformation.Update(CursorPosition, gameTime);


                for (int i = 0; i < ExplosionList.Count; i++)
                {
                    if (ExplosionList[i].Active == false)
                        ExplosionList.RemoveAt(i);
                }

                if (ExplosionList.Count > 0)
                {
                    foreach (Explosion explosion in ExplosionList)
                    {
                        explosion.Active = false;
                    }
                }

                EnemyExplosionsUpdate(gameTime);
            }

            MenuButtonsUpdate(gameTime);

            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;

            if (IsLoading == true)
                LoadingAnimation.Update(gameTime);

            base.Update(gameTime);
        }


        #region CONTENT that needs to be loaded
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            targetBatch = new SpriteBatch(GraphicsDevice);
            GameRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            MenuRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            UIRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            BackgroundRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);

            ShaderTarget1 = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            ShaderTarget2 = new RenderTarget2D(GraphicsDevice, 1920, 1080);

            MainMenuBackground.LoadContent(SecondaryContent);

            BlankTexture = SecondaryContent.Load<Texture2D>("Blank");

            DefaultCursor = SecondaryContent.Load<Texture2D>("Cursors/DefaultCursor");
            CrosshairCursor = SecondaryContent.Load<Texture2D>("Cursors/Crosshair");

            TooltipBox = SecondaryContent.Load<Texture2D>("InformationBox");

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
            ShotgunTurretCursor = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/MachineGunTurretIcon");
            PersistentBeamTurretCursor = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/MachineGunTurretIcon");

            //Trap Cursors
            WallTrapCursor = SecondaryContent.Load<Texture2D>("Icons/TrapIcons/FireTrapIcon");
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

            #region Fonts

            #endregion

            #region Load icon sprites

            LockIcon = SecondaryContent.Load<Texture2D>("Icons/LockIcon");

            //Turret icons
            BeamTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/BeamTurretIcon");
            BoomerangTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/BoomerangTurretIcon");
            CannonTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/CannonTurretIcon");
            ClusterTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/ClusterTurretIcon");
            FelCannonTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/FelCannonTurretIcon");
            FlameThrowerTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/FlameThrowerTurretIcon");
            FreezeTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/FreezeTurretIcon");
            GrenadeTurretIcon = SecondaryContent.Load<Texture2D>("Icons/TurretIcons/GrenadeTurretIcon");
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

            #endregion

            #region Menu sounds
            MenuClick = SecondaryContent.Load<SoundEffect>("Sounds/MenuPing");
            MenuWoosh = SecondaryContent.Load<SoundEffect>("Sounds/MenuWoosh");

            MenuMusic = SecondaryContent.Load<SoundEffect>("Sounds/MenuMusic1");
            MenuMusicInstance = MenuMusic.CreateInstance();
            MenuMusicInstance.IsLooped = true;
            //MenuMusicInstance.Play();
            #endregion

            Projection = Matrix.CreateOrthographicOffCenter(0, 1920, 1080, 0, 0, 1);

            BackgroundEffect = SecondaryContent.Load<Effect>("Shaders/BackgroundEffect");
            BackgroundEffect.Parameters["MatrixTransform"].SetValue(Projection);

            ButtonBlurEffect = SecondaryContent.Load<Effect>("Shaders/ButtonBlurEffect");
            ButtonBlurEffect.Parameters["MatrixTransform"].SetValue(Projection);
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

                IsLoading = true;

                ReadyToPlace = false;

                Ground = new StaticSprite("Backgrounds/Ground", new Vector2(0, 1080 - 500 + 70));
                Ground.Depth = 1.0f;
                Ground.LoadContent(Content);

                ForeGround = new StaticSprite("Backgrounds/Foreground", new Vector2(0, 1080 - 400));
                ForeGround.Depth = 1;
                ForeGround.LoadContent(Content);

                SkyBackground = new StaticSprite("Backgrounds/Sky", new Vector2(0, 0));
                SkyBackground.LoadContent(Content);

                LoadGameSounds();

                //Resources = 1800;

                ShellCasing = Content.Load<Texture2D>("Particles/MachineShell");
                Coin = Content.Load<Texture2D>("Particles/Coin");
                ResourceFont = Content.Load<SpriteFont>("Fonts/DefaultFont");

                Tower.LoadContent(Content);

                #region Setting up the buttons
                SelectButtonList = new List<Button>();
                TowerButtonList = new List<Button>();

                for (int i = 0; i < TowerButtons; i++)
                {
                    TowerButtonList.Add(new Button(TurretSlotButtonSprite, new Vector2(40 + Tower.DestinationRectangle.Width - 32, 500 + ((38 + 90) * i) - 32)));
                    TowerButtonList[i].LoadContent();
                }

                for (int i = 0; i < 8; i++)
                {
                    //a ? b : (c ? d : e)
                    //string cost = (CurrentProfile.Buttons[i].CurrentTrap.HasValue) ? TrapCost(CurrentProfile.Buttons[i].CurrentTrap.Value).ToString() : TurretCost(CurrentProfile.Buttons[i].CurrentTurret.Value).ToString();
                    //trap.hasvalue ? return trapcost : (turret.hasvalue ? return turretcost : return ""

                    //string cost = "";

                    //if (CurrentProfile.Buttons[i] == null)
                    //    cost = "";
                    //else
                    //{
                    //    if (CurrentProfile.Buttons[i].CurrentTrap.HasValue)
                    //        cost = TrapCost(CurrentProfile.Buttons[i].CurrentTrap.Value).ToString();

                    //    if (CurrentProfile.Buttons[i].CurrentTurret.HasValue)
                    //        cost = TurretCost(CurrentProfile.Buttons[i].CurrentTurret.Value).ToString();
                    //}
                    Button button = new Button(SelectButtonSprite, new Vector2(550 + (i * 100), 1080 - 80),
                                               PlaceWeaponList[i].IconTexture, null, null, "",
                                               null, "Left", Color.White, false);
                    button.LoadContent();
                    SelectButtonList.Add(button);
                }

                if (Tutorial == true)
                {
                    SelectButtonList[0].IconTexture = MachineGunTurretIcon;
                    SelectButtonList[0].LoadContent();

                    SelectButtonList[1].IconTexture = FireTrapIcon;
                    SelectButtonList[1].LoadContent();
                }
                #endregion

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
                EmitterList = new List<Emitter>();
                EmitterList2 = new List<Emitter>();
                AlphaEmitterList = new List<Emitter>();
                AdditiveEmitterList = new List<Emitter>();
                ExplosionList = new List<Explosion>();
                EnemyExplosionList = new List<Explosion>();
                ShellCasingList = new List<Particle>();
                CoinList = new List<Particle>();
                LightProjectileList = new List<LightProjectile>();
                TerrainSpriteList = new List<StaticSprite>();
                //NumberChangeList = new List<NumberChange>();
                #endregion

                HealthBarEffect = Content.Load<Effect>("Shaders/HealthBarEffect");
                HealthBarEffect.Parameters["MatrixTransform"].SetValue(Projection);
                HealthBarSprite = Content.Load<Texture2D>("HealthBarSprite");

                ShockWaveEffect = Content.Load<Effect>("Shaders/ShockWaveEffect");
                ShockWaveEffect.Parameters["MatrixTransform"].SetValue(Projection);

                ShieldBubbleEffect = Content.Load<Effect>("Shaders/ShieldBubbleEffect");
                ShieldBubbleEffect.Parameters["MatrixTransform"].SetValue(Projection);
                ShieldBubbleEffect.Parameters["NormalTexture"].SetValue(Content.Load<Texture2D>("ShieldNormalMap"));
                ShieldBubbleEffect.Parameters["DistortionTexture"].SetValue(Content.Load<Texture2D>("ShieldDistortionMap"));

                MaxWaves = CurrentLevel.WaveList.Count;
                CurrentWaveIndex = 0;

                StartWaveButton = new Button(ButtonLeftSprite, new Vector2(1000, 200));
                StartWaveButton.LoadContent();

                Lightning.LoadContent(Content);
                Bolt.LoadContent(Content);

                //Texture2D SplodgeParticle, SmokeParticle, FireParticle, BallParticle;
                SplodgeParticle = Content.Load<Texture2D>("Particles/Splodge");
                SmokeParticle = Content.Load<Texture2D>("Particles/Smoke");
                FireParticle = Content.Load<Texture2D>("Particles/FireParticle");
                BallParticle = Content.Load<Texture2D>("Particles/GlowBall");
                SparkParticle = Content.Load<Texture2D>("Particles/Spark");
                RoundSparkParticle = Content.Load<Texture2D>("Particles/RoundSpark");

                BulletTrailCap = Content.Load<Texture2D>("Particles/Cap");
                BulletTrailSegment = Content.Load<Texture2D>("Particles/Segment");

                TerrainShrub1 = Content.Load<Texture2D>("Terrain/Shrub");

                for (int i = 0; i < 15; i++)
                {
                    Vector2 newPos = new Vector2(Random.Next(0, 1920), Random.Next(690, 930));

                    if (TerrainSpriteList.Any(Sprite => Vector2.Distance(Sprite.Position, newPos) < 150))
                    {
                        newPos = new Vector2(Random.Next(0, 1920), Random.Next(690, 930));
                    }

                    StaticSprite terrainSprite = new StaticSprite(TerrainShrub1, newPos);
                    terrainSprite.Depth = (float)(terrainSprite.DestinationRectangle.Bottom / 1080.0);
                    TerrainSpriteList.Add(terrainSprite);
                }

                WhiteBlock = Content.Load<Texture2D>("WhiteBlock");

                LoadInvaderSprites();
                LoadTurretSprites();
                LoadTrapSprites();

                IsLoading = false;

                GameState = GameState.Playing;
            }
        } //This is called as a separate thread to be displayed while content is loaded

        private void UnloadGameContent()
        {
            Content.Unload();
        } //This is called when the player exits to the main menu. Unloads game content, not menu content


        private void LoadGameSounds()
        {
            //Load all the in-game sounds here
            FireTrapStart = Content.Load<SoundEffect>("Sounds/FireTrapStart");
            LightningSound = Content.Load<SoundEffect>("Sounds/LightningSound");
            CannonExplosion = Content.Load<SoundEffect>("Sounds/CannonExplosion");
            CannonFire = Content.Load<SoundEffect>("Sounds/CannonFire");
            MachineShot1 = Content.Load<SoundEffect>("Sounds/Shot3");
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
                Content.Load<Texture2D>("Traps/Trap")
            };

            //BarrelTrapSprite = Content.Load<Texture2D>("Traps/Trap");
            //CatapultTrapSprite = Content.Load<Texture2D>("Traps/Trap");
            //IceTrapSprite = Content.Load<Texture2D>("Traps/Trap");
            //TarTrapSprite = Content.Load<Texture2D>("Traps/TarTrap");
            //WallSprite = Content.Load<Texture2D>("Traps/Trap");
            //LineTrapSprite = Content.Load<Texture2D>("Traps/Trap");
            //SawBladeTrapSprite = Content.Load<Texture2D>("Traps/Trap");
            //SpikeTrapSprite = Content.Load<Texture2D>("Traps/SpikeTrap");
            //FireTrapSprite = Content.Load<Texture2D>("Traps/Trap");
        }

        private void LoadTurretSprites()
        {
            TurretSelectBox = Content.Load<Texture2D>("SelectBox");

            MachineGunTurretBase = Content.Load<Texture2D>("Turrets/MachineTurretBase");
            MachineGunTurretBarrel = Content.Load<Texture2D>("Turrets/MachineTurretBarrel");

            CannonTurretBase = Content.Load<Texture2D>("Turrets/MachineTurretBase");
            CannonTurretBarrel = Content.Load<Texture2D>("Turrets/MachineTurretBarrel");

            LightningTurretBase = Content.Load<Texture2D>("Turrets/MachineTurretBase");
            LightningTurretBarrel = Content.Load<Texture2D>("Turrets/MachineTurretBarrel");
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

                            Type turretType = Type.GetType("TowerDefensePrototype." + SelectedTurret.ToString() + "Turret");
                            object turret = Activator.CreateInstance(turretType, TowerButtonList[Index].CurrentPosition);
                            Turret newTurret = (Turret)turret;

                            newTurret = ApplyUpgrades(newTurret);
                            newTurret.Font = DefaultFont;
                            newTurret.Rect = TurretSelectBox;

                            switch (newTurret.TurretType)
                            {
                                default:
                                    var TurretBase = this.GetType().GetField(newTurret.TurretType.ToString() + "TurretBase").GetValue(this);
                                    var TurretBarrel = this.GetType().GetField(newTurret.TurretType.ToString() + "TurretBarrel").GetValue(this);
                                    newTurret.TurretBase = (Texture2D)TurretBase;
                                    newTurret.TurretBarrel = (Texture2D)TurretBarrel;
                                    break;
                                //case TurretType.MachineGun:
                                //    newTurret.TurretBase = MachineGunTurretBase;
                                //    newTurret.TurretBarrel = MachineGunTurretBarrel;
                                //    break;

                                //case TurretType.Cannon:
                                //    newTurret.TurretBase = MachineGunTurretBase;
                                //    newTurret.TurretBarrel = MachineGunTurretBarrel;
                                //    break;
                            }

                            newTurret.Initialize(Content);

                            newTurret.TimingBar.Box = WhiteBlock;
                            newTurret.HealthBar.Box = WhiteBlock;
                            newTurret.HeatBar.Box = WhiteBlock;

                            TurretList[Index] = newTurret;

                            Resources -= TurretCost(SelectedTurret.Value);
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

            foreach (Button button in SelectButtonList)
            {
                if (this.IsActive == true)
                {
                    button.Update(CursorPosition, gameTime);

                    if (button.CurrentButtonState == ButtonSpriteState.Hover &&
                        button.DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y))
                        && InGameInformation == null)
                    {
                        int i = SelectButtonList.IndexOf(button);
                        if (CurrentProfile.Buttons[i] != null)
                        {
                            if (CurrentProfile.Buttons[i].CurrentTrap != null)
                            {
                                InGameInformation = new Tooltip(CurrentProfile.Buttons[i].CurrentTrap.ToString(), TooltipBox, TooltipFont);
                                ResourceCost = TrapCost(CurrentProfile.Buttons[i].CurrentTrap.Value);
                            }
                            else
                            {
                                InGameInformation = new Tooltip(CurrentProfile.Buttons[i].CurrentTurret.ToString(), TooltipBox, TooltipFont);
                                ResourceCost = TurretCost(CurrentProfile.Buttons[i].CurrentTurret.Value);
                            }
                        }

                        if (InGameInformation != null)
                            InGameInformation.LoadContent(Content);
                    }

                    #region Check if a select button has been clicked
                    if (button.JustClicked == true)
                    {
                        ClearTurretSelect();
                        Index = SelectButtonList.IndexOf(button);

                        Action CheckLayout = new Action(() =>
                        {
                            if (Index <= CurrentProfile.Buttons.Count - 1)
                            {
                                #region Handle Trap Selection
                                if (CurrentProfile.Buttons[Index] != null && CurrentProfile.Buttons[Index].CurrentTurret == null)
                                    if (Resources >= TrapCost(CurrentProfile.Buttons[Index].CurrentTrap.Value) && TrapLimit > TrapList.Count)
                                        SelectedTrap = CurrentProfile.Buttons[Index].CurrentTrap.Value;
                                #endregion

                                #region Handle Turret Selection
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

                        switch (Index)
                        {
                            default:
                                CheckLayout();

                                if (SelectedTrap != null || SelectedTurret != null)
                                    ReadyToPlace = true;
                                break;
                        }
                    }
                    #endregion
                }
            }

            foreach (Button button in SelectButtonList)
            {
                int ButtonIndex = SelectButtonList.IndexOf(button);

                if (CurrentProfile.Buttons[ButtonIndex] != null)
                {
                    TrapType? trap = CurrentProfile.Buttons[ButtonIndex].CurrentTrap as TrapType?;
                    TurretType? turret = CurrentProfile.Buttons[ButtonIndex].CurrentTurret as TurretType?;

                    if (trap != null)
                        if (Resources >= TrapCost(trap.Value) && TrapLimit > TrapList.Count)
                            button.CurrentIconColor = Color.White;
                        else
                            button.CurrentIconColor = Color.Gray;

                    if (turret != null && TurretList.Count < 3)
                        if (Resources >= TurretCost(turret.Value))
                            button.CurrentIconColor = Color.White;
                        else
                            button.CurrentIconColor = Color.Gray;
                }
            }

            if (SelectButtonList.All(Button => Button.CurrentButtonState == ButtonSpriteState.Released &&
                !Button.DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)))
                && InGameInformation != null)
            {
                InGameInformation = null;
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
                                    DecalList.Clear();
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
                                    ExitDialog = new DialogBox(DialogBox, ButtonLeftSprite, ButtonRightSprite, DefaultFont, new Vector2(1920 / 2, 1080 / 2), "Exit", "Do you want to exit?", "Cancel");
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
                                    MainMenuDialog = new DialogBox(DialogBox, ButtonLeftSprite, ButtonRightSprite, DefaultFont, new Vector2(1920 / 2, 1080 / 2), "Yes", "Are you sure you want to return to the main menu? All progress will be lost.", "No");
                                    MainMenuDialog.LoadContent();
                                    DialogVisible = true;
                                    break;

                                case 3:
                                    button.ResetState();
                                    ProfileMenuDialog = new DialogBox(DialogBox, ButtonLeftSprite, ButtonRightSprite, DefaultFont, new Vector2(1920 / 2, 1080 / 2), "Yes", "Are you sure you want to return to your profile menu? All progress will be lost.", "No");
                                    ProfileMenuDialog.LoadContent();
                                    DialogVisible = true;
                                    break;

                                case 4:
                                    button.ResetState();
                                    ExitDialog = new DialogBox(DialogBox, ButtonLeftSprite, ButtonRightSprite, DefaultFont, new Vector2(1920 / 2, 1080 / 2), "Exit", "Do you want to exit? All progress will be lost.", "Cancel");
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
                    ProfileManagementPlay.Update(CursorPosition, gameTime);
                    ProfileManagementBack.Update(CursorPosition, gameTime);
                    ProfileManagementUpgrades.Update(CursorPosition, gameTime);
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
                                NoWeaponsDialog = new DialogBox(DialogBox, ButtonLeftSprite, ButtonRightSprite, DefaultFont, new Vector2(1920 / 2, 1080 / 2), "OK", "You have no weapons to use!", "");
                                NoWeaponsDialog.LoadContent();
                                DialogVisible = true;
                            }
                            else
                            {
                                UnloadGameContent();
                                Tower.CurrentHP = Tower.MaxHP;
                                Tower.CurrentShield = Tower.MaxShield;
                                MenuColor = Color.White;
                                SelectedTrap = null;
                                SelectedTurret = null;
                                LevelNumber = CurrentProfile.LevelNumber;
                                DecalList.Clear();
                                LoadLevel(LevelNumber);
                                LoadUpgrades();
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

                    #region Upgrades Button
                    if (ProfileManagementUpgrades.JustClicked == true)
                    {
                        GameState = GameState.Upgrades;
                    }
                    #endregion

                    #region Select Weapon Buttons
                    foreach (WeaponBox trapBox in TrapBoxes)
                    {
                        trapBox.Update(CursorPosition, gameTime);

                        if (trapBox.CurrentPosition.X > SelectTrapLeft.DestinationRectangle.Right &&
                            trapBox.DestinationRectangle.Right < SelectTrapRight.DestinationRectangle.Left)
                        {
                            if (trapBox.JustClicked == true)
                            {
                                string WeaponName = trapBox.ContainsTrap.ToString();
                                var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

                                if ((bool)Available == true)
                                    SelectedTrap = trapBox.ContainsTrap;
                            }
                        }

                        //if (trapBox.CurrentButtonState == ButtonSpriteState.Hover &&
                        //    trapBox.CurrentPosition.X > SelectTrapLeft.DestinationRectangle.Right &&
                        //    trapBox.DestinationRectangle.Right < SelectTrapRight.DestinationRectangle.Left)
                        //{
                        //    string WeaponName = trapBox.ContainsTrap.ToString();
                        //    var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

                        //    if ((bool)Available == true)
                        //    {

                        //    }
                        //}
                    }

                    foreach (WeaponBox turretBox in TurretBoxes)
                    {
                        turretBox.Update(CursorPosition, gameTime);

                        if (turretBox.CurrentPosition.X > SelectTurretLeft.DestinationRectangle.Right &&
                            turretBox.DestinationRectangle.Right < SelectTurretRight.DestinationRectangle.Left)
                        {
                            if (turretBox.JustClicked == true)
                            {
                                string WeaponName = turretBox.ContainsTurret.ToString();
                                var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

                                if ((bool)Available == true)
                                    SelectedTurret = turretBox.ContainsTurret;
                            }
                        }

                        //if (turretBox.CurrentButtonState == ButtonSpriteState.Hover &&
                        //    turretBox.CurrentPosition.X > SelectTurretLeft.DestinationRectangle.Right && 
                        //    turretBox.DestinationRectangle.Right < SelectTurretRight.DestinationRectangle.Left)
                        //{
                        //    string WeaponName = turretBox.ContainsTurret.ToString();
                        //    var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

                        //    if ((bool)Available == true)
                        //    {
                        //    }
                        //}
                    }

                    //if (TurretBoxes.All(WeaponBox => WeaponBox.CurrentButtonState == ButtonSpriteState.Released) &&
                    //    TrapBoxes.All(WeaponBox => WeaponBox.CurrentButtonState == ButtonSpriteState.Released))
                    //{

                    //}
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

                    #region Move weapons right
                    SelectTrapLeft.Update(CursorPosition, gameTime);
                    SelectTurretLeft.Update(CursorPosition, gameTime);

                    if (TrapBoxes[TrapBoxes.Count - 1].NextPosition.X + TrapBoxes[TrapBoxes.Count - 1].SourceRectangle.Width > SelectTrapRight.DestinationRectangle.Left)
                        if (SelectTrapRight.JustClicked == true)
                        {
                            foreach (WeaponBox trapBox in TrapBoxes)
                            {
                                trapBox.NextPosition.X -= 196;
                                trapBox.IconNextPosition.X -= 196;
                            }
                        }

                    if (TurretBoxes[TurretBoxes.Count - 1].NextPosition.X + TurretBoxes[TurretBoxes.Count - 1].SourceRectangle.Width > SelectTurretRight.DestinationRectangle.Left)
                        if (SelectTurretRight.JustClicked == true)
                        {
                            foreach (WeaponBox turretBox in TurretBoxes)
                            {
                                turretBox.NextPosition.X -= 196;
                                turretBox.IconNextPosition.X -= 196;
                            }
                        }
                    #endregion

                    #region Move weapons left
                    SelectTrapRight.Update(CursorPosition, gameTime);
                    SelectTurretRight.Update(CursorPosition, gameTime);

                    if (TrapBoxes[0].NextPosition.X < SelectTrapLeft.DestinationRectangle.Right)
                        if (SelectTrapLeft.JustClicked == true)
                        {
                            foreach (WeaponBox trapBox in TrapBoxes)
                            {
                                trapBox.NextPosition.X += 196;
                                trapBox.IconNextPosition.X += 196;
                            }
                        }

                    if (TurretBoxes[0].NextPosition.X < SelectTurretLeft.DestinationRectangle.Right)
                        if (SelectTurretLeft.JustClicked == true)
                        {
                            foreach (WeaponBox turretBox in TurretBoxes)
                            {
                                turretBox.NextPosition.X += 196;
                                turretBox.IconNextPosition.X += 196;
                            }
                        }
                    #endregion

                    #region Move weapons using scroll wheel
                    if (SelectTrapRight.CurrentButtonState == ButtonSpriteState.Hover ||
                        SelectTrapLeft.CurrentButtonState == ButtonSpriteState.Hover ||
                        TrapBoxes.Any(WeaponBox => WeaponBox.CurrentButtonState == ButtonSpriteState.Hover))
                    {
                        if (TrapBoxes[TrapBoxes.Count - 1].NextPosition.X + TrapBoxes[TrapBoxes.Count - 1].SourceRectangle.Width > SelectTrapRight.DestinationRectangle.Left)
                            if (CurrentMouseState.ScrollWheelValue > PreviousMouseState.ScrollWheelValue)
                            {
                                foreach (WeaponBox trapBox in TrapBoxes)
                                {
                                    trapBox.NextPosition.X -= 196;
                                    trapBox.IconNextPosition.X -= 196;
                                }
                            }

                        if (TrapBoxes[0].NextPosition.X < SelectTrapLeft.DestinationRectangle.Right)
                            if (CurrentMouseState.ScrollWheelValue < PreviousMouseState.ScrollWheelValue)
                            {
                                foreach (WeaponBox trapBox in TrapBoxes)
                                {
                                    trapBox.NextPosition.X += 196;
                                    trapBox.IconNextPosition.X += 196;
                                }
                            }
                    }

                    if (SelectTurretRight.CurrentButtonState == ButtonSpriteState.Hover ||
                        SelectTurretLeft.CurrentButtonState == ButtonSpriteState.Hover ||
                        TurretBoxes.Any(WeaponBox => WeaponBox.CurrentButtonState == ButtonSpriteState.Hover))
                    {
                        if (TurretBoxes[TurretBoxes.Count - 1].NextPosition.X + TurretBoxes[TurretBoxes.Count - 1].SourceRectangle.Width > SelectTurretRight.DestinationRectangle.Left)
                            if (CurrentMouseState.ScrollWheelValue > PreviousMouseState.ScrollWheelValue)
                            {
                                foreach (WeaponBox TurretBox in TurretBoxes)
                                {
                                    TurretBox.NextPosition.X -= 196;
                                    TurretBox.IconNextPosition.X -= 196;
                                }
                            }

                        if (TurretBoxes[0].NextPosition.X < SelectTurretLeft.DestinationRectangle.Right)
                            if (CurrentMouseState.ScrollWheelValue < PreviousMouseState.ScrollWheelValue)
                            {
                                foreach (WeaponBox TurretBox in TurretBoxes)
                                {
                                    TurretBox.NextPosition.X += 196;
                                    TurretBox.IconNextPosition.X += 196;
                                }
                            }
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

                                case 4:
                                    ProfileNumber = 5;
                                    FileName = "Profile5.sav";
                                    StorageDevice.BeginShowSelector(this.HandleProfile, null);
                                    break;

                                case 5:
                                    ProfileNumber = 6;
                                    FileName = "Profile6.sav";
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
                            NameLengthDialog = new DialogBox(DialogBox, ButtonLeftSprite, ButtonRightSprite, DefaultFont, new Vector2(1920 / 2, 1080 / 2), "OK", "Your name is too short.", "");
                            NameLengthDialog.LoadContent();
                            DialogVisible = true;
                        }

                        if (NameInput.RealString.All(Char => Char == ' '))
                        {
                            NameLengthDialog = new DialogBox(DialogBox, ButtonLeftSprite, ButtonRightSprite, DefaultFont, new Vector2(1920 / 2, 1080 / 2), "OK", "Your name cannot be blank.", "");
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

                #region Handling Victory Menu Button Presses
                if (GameState == GameState.Victory && DialogVisible == false)
                {
                    if (VictoryRetry != null)
                        VictoryRetry.Update(CursorPosition, gameTime);

                    if (VictoryComplete != null)
                        VictoryComplete.Update(CursorPosition, gameTime);

                    if (VictoryReturn != null)
                        VictoryReturn.Update(CursorPosition, gameTime);

                    if (VictoryReturn != null)
                        if (VictoryReturn.JustClicked == true)
                        {
                            IsLoading = false;
                            StorageDevice.BeginShowSelector(this.SaveProfile, null);
                            ResetUpgrades();
                            UnloadGameContent();
                            MaxWaves = CurrentLevel.WaveList.Count;
                            CurrentWaveIndex = 0;
                            GameState = GameState.ProfileManagement;
                            VictoryTime = 0;
                            CurrentWaveTime = 0;
                            CurrentInvaderTime = 0;
                            VictoryRetry = null;
                            VictoryReturn = null;
                            VictoryComplete = null;
                        }

                    if (VictoryRetry != null)
                        if (VictoryRetry.JustClicked == true)
                        {
                            MenuColor = Color.White;
                            SelectedTrap = null;
                            SelectedTurret = null;
                            LevelNumber = CurrentProfile.LevelNumber;
                            LoadLevel(LevelNumber);
                            LoadUpgrades();
                            //StorageDevice.BeginShowSelector(this.SaveProfile, null);
                            GameState = GameState.Loading;
                            LoadingThread = new Thread(LoadGameContent);
                            LoadingThread.Start();
                            IsLoading = false;
                            VictoryTime = 0;
                            CurrentWaveTime = 0;
                            CurrentInvaderTime = 0;
                            Tower.CurrentHP = Tower.MaxHP;
                            Tower.CurrentShield = Tower.MaxShield;
                            VictoryRetry = null;
                            VictoryReturn = null;
                            VictoryComplete = null;
                            return;
                        }

                    if (VictoryComplete != null)
                        if (VictoryComplete.JustClicked == true)
                        {
                            IsLoading = false;
                            CurrentProfile.LevelNumber++;
                            UnlockWeapons();
                            StorageDevice.BeginShowSelector(this.SaveProfile, null);
                            ResetUpgrades();
                            UnloadGameContent();
                            MaxWaves = CurrentLevel.WaveList.Count;
                            CurrentWaveIndex = 0;
                            GameState = GameState.ProfileManagement;
                            VictoryTime = 0;
                            CurrentWaveTime = 0;
                            CurrentInvaderTime = 0;
                            VictoryRetry = null;
                            VictoryReturn = null;
                            VictoryComplete = null;
                        }
                }
                #endregion

                #region Handling Upgrades Menu Button Presses
                if (GameState == GameState.Upgrades && DialogVisible == false)
                {
                    UpgradesBack.Update(CursorPosition, gameTime);

                    foreach (Button button in UpgradeButtonList)
                    {
                        button.Update(CursorPosition, gameTime);

                        int index = UpgradeButtonList.IndexOf(button);

                        if (button.JustClicked == true)
                        {
                            switch (index)
                            {
                                case 0:
                                    CurrentProfile.UpgradesList.Add(new Upgrade2());
                                    break;
                            }
                        }
                    }

                    if (UpgradesBack.JustClicked == true)
                    {
                        GameState = GameState.ProfileManagement;
                    }
                }
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
                if (invader.CurrentHP < invader.PreviousHP)
                {
                    NumberChangeList.Add(new NumberChange(DefaultFont, invader.Position, new Vector2(0, -1.5f), (int)(invader.PreviousHP - invader.CurrentHP)));
                }

                invader.PreviousHP = invader.CurrentHP;
                invader.Update(gameTime);

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
                                new Vector2(invader.MaxY, invader.MaxY), false, null, true, false, null, null, null, null, null, true, true));

                                EmitterList2.Add(new Emitter(SmokeParticle, 
                                new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                                new Vector2(0, 360), new Vector2(0.25f, 0.75f), new Vector2(20, 60), 1f, true, new Vector2(0, 360), 
                                new Vector2(0.5f, 2f), new Vector2(0.2f, 0.6f), RandomColor(Color.Red, Color.DarkRed, Color.DarkRed), 
                                RandomColor(Color.Red, Color.DarkRed, Color.DarkRed), 0.01f, 0.2f, 10, 1, true, 
                                new Vector2(invader.MaxY, invader.MaxY), false, null, true, false, null, null, null, null, null, true, true));

                                Decal NewDecal = new Decal("BloodDecal", new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Bottom),
                                                          (float)RandomDouble(0, 0), invader.YRange, invader.MaxY, 0.1f);

                                NewDecal.LoadContent(Content);
                                DecalList.Add(NewDecal);

                            }
                            break;

                        case InvaderType.SuicideBomber:
                            EmitterList2.Add(new Emitter(SplodgeParticle, 
                            new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                            new Vector2(0, 360), new Vector2(0.25f, 0.5f), new Vector2(25, 50), 0.5f, true, 
                            new Vector2(0, 360), new Vector2(1, 3), new Vector2(0.02f, 0.06f), Color.DarkRed, Color.Red, 
                            0.2f, 0.2f, 20, 10, true, new Vector2(invader.MaxY, invader.MaxY)));

                            EnemyExplosionList.Add(new Explosion(new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y), 100, invader.AttackPower));
                            break;

                        case InvaderType.Spider:
                            EmitterList2.Add(new Emitter(SplodgeParticle, 
                            new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                            new Vector2(0, 360), new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), 
                            new Vector2(1, 3), new Vector2(0.02f, 0.06f), Color.Green, Color.Lime, 0.1f, 0.2f, 20, 10, true, 
                            new Vector2(invader.MaxY, invader.MaxY)));
                            break;

                        case InvaderType.Tank:
                            EnemyExplosionList.Add(new Explosion(new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y), 200, invader.AttackPower));

                            Emitter ExplosionEmitter2 = new Emitter(SplodgeParticle, 
                                    new Vector2(invader.DestinationRectangle.Center.X, invader.MaxY), new Vector2(0, 180), new Vector2(1, 4), 
                                    new Vector2(20, 50), 2f, true, new Vector2(0, 360), new Vector2(1, 3), new Vector2(0.02f, 0.06f), 
                                    Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 0.2f, 20, 10, true, 
                                    new Vector2(invader.MaxY + 8, invader.MaxY + 8));

                            EmitterList.Add(ExplosionEmitter2);

                            Emitter ExplosionEmitter = new Emitter(FireParticle,
                                    new Vector2(invader.DestinationRectangle.Center.X, invader.MaxY),
                                    new Vector2(0, 180), new Vector2(3, 8), new Vector2(1, 15), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 20, false,
                                    new Vector2(0, 1080), true);

                            EmitterList.Add(ExplosionEmitter);

                            //Color SmokeColor1 = Color.Lerp(Color.DarkGray, Color.Transparent, 0.5f);
                            //Color SmokeColor2 = Color.Lerp(Color.Gray, Color.Transparent, 0.5f);

                            Emitter newEmitter2 = new Emitter(SmokeParticle,
                                    new Vector2(invader.DestinationRectangle.Center.X, invader.MaxY),
                                    new Vector2(80, 100), new Vector2(0.5f, 1f), new Vector2(20, 40), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(0, 0), new Vector2(1, 2), SmokeColor1, SmokeColor2, 0.0f, 0.3f, 1, 1, false,
                                    new Vector2(0, 1080), false);

                            EmitterList2.Add(newEmitter2);
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
                            var value = Vector2.Distance(rangedInvader.Position, new Vector2(Tower.DestinationRectangle.Left, Tower.DestinationRectangle.Bottom));

                            if (value < rangedInvader.Range.Y + Random.Next(0, 30))
                            {
                                invader.Velocity.X = 0;
                            }
                            break;
                    }
                }
            }

            #region This controls what happens when an explosion happens near the invader
            if (ExplosionList.Count > 0)
                foreach (Explosion explosion in ExplosionList)
                {
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
                                        0.1f, 0.1f, 3, 5, true, new Vector2(invader.MaxY, invader.MaxY), false, 1, true, false);

                                        EmitterList.Add(BloodEmitter);
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
                        switch (trap.TrapType)
                        {
                            case TrapType.Wall:
                                {
                                    Wall wallTrap = trap as Wall;
                                    invader.Velocity.X = 0;
                                }
                                break;
                        }
                    }
                }

                //If an invader is not colliding with a wall trap or the tower, move as normal
                if (!TrapList.Any(Trap => Trap.DestinationRectangle.Intersects(invader.DestinationRectangle) && 
                                  Trap.TrapType == TrapType.Wall) &&
                    Vector2.Distance(
                    PointToVector(invader.DestinationRectangle.Center),
                    new Vector2(Tower.DestinationRectangle.Right, invader.DestinationRectangle.Center.Y)) > 5)
                {
                    invader.Velocity.X = invader.Direction.X * invader.Speed;
                }
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
                        Tower.TakeDamage(invader.AttackPower);

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
                   (rangedInvader.DestinationRectangle.Left - Tower.DestinationRectangle.Right) > rangedInvader.Range.X &&
                   (rangedInvader.DestinationRectangle.Left - Tower.DestinationRectangle.Right) < rangedInvader.Range.Y)
                {
                    switch (rangedInvader.InvaderType)
                    {
                        case InvaderType.Spider:
                            {
                                HeavyProjectile heavyProjectile = new AcidProjectile(
                                new Vector2(rangedInvader.DestinationRectangle.Center.X, rangedInvader.DestinationRectangle.Center.Y),
                                Random.Next((int)(rangedInvader.PowerRange.X), (int)(rangedInvader.PowerRange.Y)),
                                -MathHelper.ToRadians(Random.Next((int)(rangedInvader.AngleRange.X), (int)(rangedInvader.AngleRange.Y))),
                                0.2f, rangedInvader.RangedAttackPower);

                                heavyProjectile.YRange = new Vector2(invader.Bottom, invader.Bottom);
                                heavyProjectile.LoadContent(Content);
                                InvaderHeavyProjectileList.Add(heavyProjectile);
                            }
                            break;

                        case InvaderType.Archer:
                            {
                                HeavyProjectile heavyProjectile = new AcidProjectile(
                                new Vector2(rangedInvader.DestinationRectangle.Center.X, rangedInvader.DestinationRectangle.Center.Y),
                                Random.Next((int)(rangedInvader.PowerRange.X), (int)(rangedInvader.PowerRange.Y)),
                                -MathHelper.ToRadians(Random.Next((int)(rangedInvader.AngleRange.X), (int)(rangedInvader.AngleRange.Y))),
                                0.2f, rangedInvader.RangedAttackPower);

                                heavyProjectile.YRange = new Vector2(invader.Bottom, invader.Bottom);
                                heavyProjectile.LoadContent(Content);
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

            for (int i = 0; i < InvaderLightProjectileList.Count; i++)
            {
                if (InvaderLightProjectileList[i].Active == false)
                    InvaderLightProjectileList.RemoveAt(i);
            }
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
        private void TurretUpdate()
        {
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
                    if (turret.Selected == true &&
                        CurrentMouseState.LeftButton == ButtonState.Pressed &&
                        CursorPosition.Y < SelectButtonList[0].CurrentPosition.Y)
                    {
                        if (turret.CanShoot == true && this.IsActive == true)
                        {
                            turret.ElapsedTime = 0;
                            turret.Animated = true;
                            turret.CurrentFrame = 1;
                            TurretShoot();
                        }
                    }

                    if (turret.JustClicked == true)
                    {
                        ClearSelected();
                        turret.Selected = true;

                        //Makes sure two turrets cannot be selected at the same time
                        foreach (Turret turret2 in TurretList)
                        {
                            if (turret2 != null && turret2 != turret)
                            {
                                turret2.Selected = false;
                            }
                        }
                    }

                    //Remove turret when middle clicked and refund resources
                    if (turret.Active == true &&
                        turret.SelectBox.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)) &&
                        CurrentMouseState.MiddleButton == ButtonState.Released &&
                        PreviousMouseState.MiddleButton == ButtonState.Pressed &&
                        turret.CurrentHealth == turret.Health)
                    {
                        Resources += turret.ResourceCost;
                        turret.Active = false;
                        TowerButtonList[TurretList.IndexOf(turret)].ButtonActive = true;
                        TurretList[TurretList.IndexOf(turret)] = null;
                        return;
                    }
                }
            }
        }

        private void TurretShoot()
        {
            foreach (Turret turret in TurretList)
            {
                if (turret != null &&
                    turret.Active == true &&
                    CursorPosition.Y < (SelectButtonList[0].CurrentPosition.Y) &&
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
                                //MachineShot1.Play();

                                CurrentProfile.ShotsFired++;

                                LightProjectileList.Add(new MachineGunProjectile(new Vector2(turret.BarrelCenter.X,
                                                                                             turret.BarrelCenter.Y), Direction));
                                Emitter FlashEmitter = new Emitter(FireParticle,
                                    new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                    new Vector2(
                                    MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X))),
                                    new Vector2(15, 30), new Vector2(1, 3), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(-1, 1), new Vector2(1, 3), FireColor, FireColor2, 0.0f, 0.05f, 0.5f, 1,
                                    false, new Vector2(0, 1080), true, 1);
                                EmitterList.Add(FlashEmitter);

                                Particle NewShellCasing = new Particle(ShellCasing,
                                    new Vector2(turret.BarrelCenter.X, turret.BarrelCenter.Y),
                                    turret.Rotation - MathHelper.ToRadians((float)RandomDouble(175, 185)),
                                    (float)RandomDouble(2, 4), 500, 1f, true, (float)RandomDouble(-10, 10),
                                    (float)RandomDouble(-6, 6), 0.7f, Color.Orange, Color.Lerp(Color.White, Color.Transparent, 0.25f), 0.35f, true, Random.Next(Tower.DestinationRectangle.Bottom, Tower.DestinationRectangle.Bottom + 32),
                                    false, 0.998f, true, true, true);

                                ShellCasingList.Add(NewShellCasing);

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

                                Emitter FlashEmitter = new Emitter(FireParticle,
                                        new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                        new Vector2(
                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)),
                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X))),
                                        new Vector2(20, 35), new Vector2(1, 3), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(-1, 1), new Vector2(2, 4), FireColor, FireColor2, 0.0f, 0.05f, 0.5f, 1,
                                        false, new Vector2(0, 1080), true, 1);
                                EmitterList.Add(FlashEmitter);

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

                                    HeavyProjectile = new CannonBall(new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 16,
                                        turret.FireRotation, 0.2f, turret.Damage, turret.BlastRadius,
                                        new Vector2(AvgY - 32, AvgY+32));
                                }
                                else
                                {
                                    HeavyProjectile = new CannonBall(new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 16, 
                                        turret.FireRotation, 0.2f, turret.Damage, turret.BlastRadius, 
                                        new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));
                                }

                                HeavyProjectile.LoadContent(Content);
                                HeavyProjectileList.Add(HeavyProjectile);

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

                                ShellCasingList.Add(new Particle(ShellCasing,
                                    new Vector2(turret.BarrelRectangle.X - BarrelStart.X, turret.BarrelRectangle.Y - BarrelStart.Y),
                                    turret.Rotation - MathHelper.ToRadians((float)RandomDouble(175, 185)),
                                    (float)RandomDouble(1, 3), 500, 1f, true, (float)RandomDouble(-10, 10),
                                    (float)RandomDouble(-6, 6), 1f, Color.Orange, Color.Lerp(Color.White, Color.Transparent, 0.25f), 0.35f, true, Random.Next(Tower.DestinationRectangle.Bottom, Tower.DestinationRectangle.Bottom + 32),
                                    false, null, true, true, true));
                            }
                            break;
                        #endregion

                        #region Flamethrower turret
                        case TurretType.FlameThrower:
                            {
                                HeavyProjectile = new FlameProjectile(new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                    (float)RandomDouble(7, 9), turret.Rotation, 0.3f, 5,
                                    new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));

                                foreach (Emitter emitter in HeavyProjectile.EmitterList)
                                {
                                    emitter.AngleRange = new Vector2(
                                   -(MathHelper.ToDegrees((float)Math.Atan2(-HeavyProjectile.Velocity.Y, -HeavyProjectile.Velocity.X))) - 20,
                                   -(MathHelper.ToDegrees((float)Math.Atan2(-HeavyProjectile.Velocity.Y, -HeavyProjectile.Velocity.X))) + 20);
                                }

                                HeavyProjectile.LoadContent(Content);
                                HeavyProjectileList.Add(HeavyProjectile);
                            }
                            break;
                        #endregion

                        #region Lightning turret
                        case TurretType.Lightning:
                            {
                                LightProjectileList.Add(new LightningProjectile(new Vector2(turret.BarrelCenter.X, turret.BarrelCenter.Y), Direction));
                                LightningSound.Play();
                            }
                            break;
                        #endregion

                        #region Cluster turret
                        case TurretType.Cluster:
                            {
                                TimerHeavyProjectile heavyProjectile;

                                Emitter FlashEmitter = new Emitter(FireParticle,
                                    new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                    new Vector2(
                                    MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X))),
                                    new Vector2(10, 20), new Vector2(1, 6), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(-2, 2), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.2f, 1, 5, false,
                                    new Vector2(0, 1080), true);

                                EmitterList.Add(FlashEmitter);
                                heavyProjectile = new ClusterBombShell(1000, new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 12, turret.Rotation, 0.2f, 5,
                                    new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));

                                TimedProjectileList.Add(heavyProjectile);
                                heavyProjectile.LoadContent(Content);

                                ShellCasingList.Add(new Particle(ShellCasing,
                                    new Vector2(turret.BarrelRectangle.X, turret.BarrelRectangle.Y),
                                    turret.Rotation - MathHelper.ToRadians((float)RandomDouble(175, 185)),
                                    (float)RandomDouble(3, 6), 500, 1f, true, (float)RandomDouble(-10, 10),
                                    (float)RandomDouble(-6, 6), 1.5f, Color.White, Color.White, 0.2f, true,
                                    Random.Next(Tower.DestinationRectangle.Bottom, Tower.DestinationRectangle.Bottom + 32), false, null, true));
                            }
                            break;
                        #endregion

                        #region Fel Cannon
                        case TurretType.FelCannon:
                            {
                                HeavyProjectile = new FelProjectile(
                                        new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 5, turret.Rotation, 0.01f, turret.Damage, 100,
                                        new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));

                                HeavyProjectile.LoadContent(Content);
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
                                HeavyProjectile = new BoomerangProjectile(new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 12, turret.Rotation, 0.2f, turret.Damage, 100,
                                    new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));

                                HeavyProjectile.LoadContent(Content);
                                HeavyProjectileList.Add(HeavyProjectile);
                            }
                            break;
                        #endregion

                        #region Grenade launcher turret
                        case TurretType.Grenade:
                            {
                                TimerHeavyProjectile heavyProjectile;

                                heavyProjectile = new GrenadeProjectile(1000, new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 16, turret.Rotation, 0.3f, 5,
                                    new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));

                                heavyProjectile.LoadContent(Content);
                                TimedProjectileList.Add(heavyProjectile);

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

                                Emitter FlashEmitter = new Emitter(FireParticle,
                                        new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(turret.Direction.Y, turret.Direction.X)),
                                            MathHelper.ToDegrees(-(float)Math.Atan2(turret.Direction.Y, turret.Direction.X))),
                                new Vector2(20, 30), new Vector2(1, 4), 1f, true, new Vector2(0, 360),
                                new Vector2(-10, 10), new Vector2(2, 3), FireColor, FireColor2, 0.0f, 0.05f, 1, 10,
                                false, new Vector2(0, 1080), true, 1);
                                EmitterList.Add(FlashEmitter);

                                ShellCasingList.Add(new Particle(ShellCasing,
                                    new Vector2(turret.BarrelCenter.X, turret.BarrelCenter.Y),
                                    turret.Rotation - MathHelper.ToRadians((float)RandomDouble(175, 185)),
                                    (float)RandomDouble(4, 6), 500, 1f, true, (float)RandomDouble(-10, 10),
                                    (float)RandomDouble(-6, 6), 0.7f, Color.Orange, Color.Lerp(Color.White, Color.Transparent, 0.25f), 0.2f, true, Random.Next(600, 630),
                                    false, null, true));

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
                    //default:
                    //    //This makes sure that the trails from the projectiles appear correctly oriented while falling
                    //    foreach (Emitter emitter in heavyProjectile.EmitterList)
                    //    {
                    //        emitter.AngleRange = new Vector2(
                    //            -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) - 20,
                    //            -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) + 20);
                    //    }
                    //    break;

                    case HeavyProjectileType.Boomerang:
                        {
                            heavyProjectile.Velocity.X -= 0.2f;
                        }
                        break;

                    case HeavyProjectileType.FelProjectile:
                        for (int i = 0; i < 5; i++)
                        {
                            Bolt = new LightningBolt(heavyProjectile.Position, new Vector2(heavyProjectile.Position.X + (float)RandomDouble(-30, 30f), heavyProjectile.Position.Y + (float)RandomDouble(-30f, 30f)), Color.LimeGreen, 0.25f);
                            LightningList.Add(Bolt);
                        }
                        break;
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
                            case HeavyProjectileType.CannonBall:
                                Emitter newEmitter = new Emitter(SplodgeParticle, new Vector2(heavyProjectile.DestinationRectangle.Center.X, heavyProjectile.DestinationRectangle.Center.Y),
                                                     new Vector2(
                                                         -(float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X) - 180 - 45,
                                                         (float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X) - 180 + 45),
                                                     new Vector2(2, 3), new Vector2(100, 200), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                                     new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true,
                                                     new Vector2(TrapList[index].DestinationRectangle.Bottom, TrapList[index].DestinationRectangle.Bottom), false, null, true, true);

                                EmitterList2.Add(newEmitter);
                                break;
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
                if (heavyProjectile.Position.Y > heavyProjectile.MaxY && heavyProjectile.Active == true)
                {
                    switch (heavyProjectile.HeavyProjectileType)
                    {
                        #region CannonBall projectile
                        case HeavyProjectileType.CannonBall:
                            {
                                Emitter ExplosionEmitter = new Emitter(FireParticle,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(20, 160), new Vector2(3, 8), new Vector2(5, 40), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(3, 4), Color.Lerp(FireColor, Color.Transparent, 0.5f),
                                        Color.Lerp(FireColor2, Color.Transparent, 0.5f), 0.0f, 0.1f, 1, 1, false,
                                        new Vector2(0, 1080), true);
                                EmitterList.Add(ExplosionEmitter);

                                Emitter ExplosionEmitter2 = new Emitter(FireParticle,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(70, 110), new Vector2(4, 10), new Vector2(20, 60), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(3, 4), Color.Lerp(FireColor, Color.Transparent, 0.5f),
                                        Color.Lerp(FireColor2, Color.Transparent, 0.5f), 0.0f, 0.15f, 10, 1, false,
                                        new Vector2(0, 1080), true);
                                EmitterList.Add(ExplosionEmitter2);

                                Emitter SmokeEmitter = new Emitter(SmokeParticle,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY - 30),
                                        new Vector2(0, 0), new Vector2(0f, 1f), new Vector2(30, 40), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(0.8f, 1.2f), SmokeColor1, SmokeColor2, -0.1f, 0.4f, 20, 1, false,
                                        new Vector2(0, 1080), false, null, null, null, null, null, null, null, null, true, true);
                                EmitterList.Add(SmokeEmitter);

                                Emitter DebrisEmitter = new Emitter(SplodgeParticle, 
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(70, 110), new Vector2(5, 7), new Vector2(30, 110), 2f, true, new Vector2(0, 360), 
                                        new Vector2(1, 3), new Vector2(0.007f, 0.05f), Color.DarkSlateGray, Color.SaddleBrown, 
                                        0.2f, 0.2f, 5, 1, true, new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 8));
                                EmitterList.Add(DebrisEmitter);

                                Emitter DebrisEmitter2 = new Emitter(SplodgeParticle, 
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY), new Vector2(80, 100), 
                                        new Vector2(2, 8), new Vector2(80, 150), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                        new Vector2(0.01f, 0.02f), Color.Gray, Color.SaddleBrown, 0.2f, 0.3f, 10, 5, true, 
                                        new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 8));
                                EmitterList.Add(DebrisEmitter2);

                                Emitter SparkEmitter = new Emitter(RoundSparkParticle, 
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY - 20), new Vector2(0, 360), 
                                        new Vector2(1, 4), new Vector2(120, 140), 100f, true, new Vector2(0, 360), new Vector2(1, 3),
                                        new Vector2(0.1f, 0.3f), Color.Orange, Color.OrangeRed, 0.05f, 0.1f, 1, 10, true, 
                                        new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 8));
                                AlphaEmitterList.Add(SparkEmitter);

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

                                float YDist = heavyProjectile.YRange.Y - heavyProjectile.MaxY;
                                float YDist2 = heavyProjectile.YRange.Y - heavyProjectile.YRange.X;

                                float PercentY = (100 / YDist2) * YDist;
                                float thing = (100 - PercentY) / 100;

                                thing = MathHelper.Clamp(thing, 0.5f, 1f);

                                Decal NewDecal = new Decal("Decal", new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                                                          (float)RandomDouble(0, 0), heavyProjectile.YRange, heavyProjectile.MaxY, 1f);

                                NewDecal.LoadContent(Content);
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

                                EmitterList.Add(ExplosionEmitter2);

                                Emitter ExplosionEmitter = new Emitter(FireParticle,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(0, 180), new Vector2(1, 5), new Vector2(1, 20), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 7, false,
                                        new Vector2(0, 1080), true);

                                EmitterList.Add(ExplosionEmitter);

                                Color SmokeColor1 = Color.Lerp(Color.DarkGray, Color.Transparent, 0.5f);
                                Color SmokeColor2 = Color.Lerp(Color.Gray, Color.Transparent, 0.5f);

                                Emitter newEmitter2 = new Emitter(SmokeParticle,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(80, 100), new Vector2(0.5f, 1f), new Vector2(20, 40), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(1, 2), SmokeColor1, SmokeColor2, 0.0f, 0.3f, 1, 1, false,
                                        new Vector2(0, 1080), false);

                                EmitterList2.Add(newEmitter2);

                                ExplosionList.Add(new Explosion(heavyProjectile.Position, 300, heavyProjectile.Damage));
                            }
                            break;
                        #endregion

                        #region Flame projectiles from FlameThrower
                        case HeavyProjectileType.FlameThrower:
                            {
                                Emitter newEmitter = new Emitter(SmokeParticle, new Vector2(heavyProjectile.Position.X,
                                    heavyProjectile.DestinationRectangle.Bottom), new Vector2(20, 160), new Vector2(1f, 1.6f), new Vector2(10, 12), 0.25f, false,
                                    new Vector2(-20, 20), new Vector2(-4, 4), new Vector2(0.6f, 0.8f), FireColor, FireColor2, -0.2f, 0.5f, 3, 1, false, new Vector2(0, 1080));
                                EmitterList.Add(newEmitter);

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

                                        invader.DamageOverTime(800, 1, 0.1f, LightningColor);
                                        invader.Freeze(1000, LightningColor);
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
                                case InvaderType.Airship:
                                    InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                                    DeactivateProjectile.Invoke();
                                    break;

                                case InvaderType.Tank:
                                    InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                                    DeactivateProjectile.Invoke();

                                    Emitter newEmitter = new Emitter(SplodgeParticle, new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                                    new Vector2(-(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) - 45,
                                    -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) + 90),
                                    new Vector2(2, 3), new Vector2(100, 200), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                    new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true,
                                    new Vector2(InvaderList[Index].MaxY, InvaderList[Index].MaxY), false, 1f);

                                    EmitterList.Add(newEmitter);
                                    break;
                            }
                            break;
                        #endregion

                        #region Flame projectile from FlameThrower
                        case HeavyProjectileType.FlameThrower:
                            switch (InvaderList[Index].InvaderType)
                            {
                                case InvaderType.Airship:
                                    if (InvaderList[Index].Burning == false)
                                        InvaderList[Index].DamageOverTime(1000, 1, 100, Color.Red);

                                    DeactivateProjectile.Invoke();
                                    break;

                                case InvaderType.Soldier:
                                    if (InvaderList[Index].Burning == false)
                                        InvaderList[Index].DamageOverTime(1000, 1, 100, Color.Red);

                                    DeactivateProjectile.Invoke();
                                    break;

                                case InvaderType.Spider:
                                    if (InvaderList[Index].Burning == false)
                                        InvaderList[Index].DamageOverTime(1000, 1, 100, Color.Red);

                                    DeactivateProjectile.Invoke();
                                    break;

                                case InvaderType.Slime:
                                    if (InvaderList[Index].Burning == false)
                                        InvaderList[Index].DamageOverTime(1000, 1, 100, Color.Red);

                                    DeactivateProjectile.Invoke();
                                    break;

                                case InvaderType.Tank:
                                    if (InvaderList[Index].Burning == false)
                                        InvaderList[Index].DamageOverTime(1000, 1, 100, Color.Red);

                                    DeactivateProjectile.Invoke();
                                    break;
                            }
                            break;
                        #endregion

                        #region Arrow projectile
                        case HeavyProjectileType.Arrow:
                            if (heavyProjectile.Velocity != Vector2.Zero)
                            {
                                switch (InvaderList[Index].InvaderType)
                                {

                                    case InvaderType.Airship:
                                        InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                                        DeactivateProjectile.Invoke();
                                        break;

                                    case InvaderType.Soldier:
                                        InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                                        DeactivateProjectile.Invoke();
                                        break;

                                    case InvaderType.Slime:
                                        InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                                        DeactivateProjectile.Invoke();
                                        break;

                                    case InvaderType.Spider:
                                        InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                                        DeactivateProjectile.Invoke();
                                        break;

                                    case InvaderType.Tank:
                                        InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                                        DeactivateProjectile.Invoke();
                                        break;
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

                                EmitterList.Add(ExplosionEmitter2);

                                DeactivateProjectile();
                            }
                            break;

                        case HeavyProjectileType.Grenade:
                            {

                            }
                            break;
                    }
                }
                #endregion

                #region What happens when a TimedProjectile runs out of time
                if (timedProjectile.Detonated == true)
                    switch (timedProjectile.HeavyProjectileType)
                    {
                        #region Cluster bomb shell
                        case HeavyProjectileType.ClusterBombShell:
                            {
                                Emitter ExplosionEmitter = new Emitter(FireParticle,
                                        new Vector2(timedProjectile.Position.X, timedProjectile.Position.Y),
                                        new Vector2(0, 180), new Vector2(1, 5), new Vector2(1, 20), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 7, false,
                                        new Vector2(0, 1080), true);

                                EmitterList.Add(ExplosionEmitter);

                                AddCluster(5, timedProjectile.Position, new Vector2(0, 360), HeavyProjectileType.ClusterBomb, timedProjectile.YRange, 2f);
                                timedProjectile.Active = false;
                            }
                            break;
                        #endregion

                        #region Grenade
                        case HeavyProjectileType.Grenade:
                            {
                                ExplosionList.Add(new Explosion(timedProjectile.Position, 30, 10));

                                Emitter ExplosionEmitter = new Emitter(FireParticle,
                                            new Vector2(timedProjectile.Position.X, timedProjectile.Position.Y),
                                            new Vector2(0, 180), new Vector2(1, 5), new Vector2(1, 20), 0.01f, true, new Vector2(0, 360),
                                            new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 7, false,
                                            new Vector2(0, 1080), true);

                                EmitterList.Add(ExplosionEmitter);

                                timedProjectile.Active = false;
                            }
                            break;
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
                        heavyProjectile = new ClusterBomb(position, speed, (float)RandomDouble(angleRange.X, angleRange.Y), 0.3f, 5, yRange);
                        break;

                    default:
                        heavyProjectile = null;
                        break;
                }

                if (heavyProjectile != null)
                {
                    heavyProjectile.LoadContent(Content);
                    HeavyProjectileList.Add(heavyProjectile);
                }
            }
        }

        private void LightProjectileUpdate()
        {
            Ground.BoundingBox = new BoundingBox(new Vector3(0, MathHelper.Clamp(CursorPosition.Y, 690, 1080), 0), new Vector3(1920, MathHelper.Clamp(CursorPosition.Y + 1, 800, 1080), 0));

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
                hitInvader.TurretDamage(-Turret.Damage);
                if (hitInvader.CurrentHP <= 0)
                    Resources += hitInvader.ResourceValue;

                switch (Turret.TurretType)
                {
                    #region Freeze
                    case TurretType.Freeze:
                        switch (hitInvader.InvaderType)
                        {
                            #region Default
                            default:
                                hitInvader.Freeze(3000, Color.SkyBlue);

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

                                    EmitterList.Add(BloodEmitter);
                                    EmitterList.Add(BloodEmitter2);
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

                                EmitterList.Add(Emitter);
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
                                                            new Vector2(60, 120), new Vector2(2, 4), new Vector2(20, 40), 2f, true, 
                                                            new Vector2(0, 360), new Vector2(1, 3),
                                                            new Vector2(0.02f, 0.04f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 
                                                            0.1f, 5, 1, true, new Vector2(CollisionEnd.Y + 8, CollisionEnd.Y + 8));
                        EmitterList.Add(DebrisEmitter);

                        Emitter DebrisEmitter2 = new Emitter(SplodgeParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                                            new Vector2(80, 100), new Vector2(3, 5), new Vector2(20, 40), 2f, true, 
                                                            new Vector2(0, 360), new Vector2(1, 3),
                                                            new Vector2(0.01f, 0.03f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 
                                                            0.1f, 2, 3, true, new Vector2(CollisionEnd.Y + 8, CollisionEnd.Y + 8));
                        EmitterList.Add(DebrisEmitter2);

                        Emitter SmokeEmitter = new Emitter(SmokeParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y - 4),
                                                           new Vector2(90, 90), new Vector2(0.5f, 1f), new Vector2(20, 30), 1f, true, 
                                                           new Vector2(0, 0), new Vector2(-2, 2), new Vector2(0.5f, 1f), DirtColor, DirtColor2, 
                                                           0f, 0.02f, 5, 1, false, new Vector2(0, 1080), false);
                        EmitterList.Add(SmokeEmitter);

                        //Emitter SparkEmitter = new Emitter(RoundSparkParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                        //                                   new Vector2(0, 180), new Vector2(8,10), new Vector2(2, 15), 0.5f, true, 
                        //                                   new Vector2(0, 360), new Vector2(1, 1), new Vector2(0.01f, 0.5f), 
                        //                                   Color.Orange, Color.OrangeRed, -0.0f, 0.1f, 10, 1, true, 
                        //                                   new Vector2(CollisionEnd.Y + 8, CollisionEnd.Y + 8), false, 
                        //                                   1, true, true, null, null, null, true);
                        //AlphaEmitterList.Add(SparkEmitter);

                        Decal NewDecal = new Decal("Decal", new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                                   (float)RandomDouble(0, 0), new Vector2(690, 930), CollisionEnd.Y, 0.2f);

                        NewDecal.LoadContent(Content);
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
                        EmitterList.Add(PulseDebrisEmitter);

                        Emitter PulseSmokeEmitter = new Emitter(SmokeParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y - 4),
                        new Vector2(90, 90), new Vector2(0.5f, 1f), new Vector2(20, 30), 1f, true, new Vector2(0, 0),
                        new Vector2(-2, 2), new Vector2(0.5f, 1f), DirtColor, DirtColor2, 0f, 0.02f, 10, 1, false, new Vector2(0, 1080), false);
                        EmitterList.Add(PulseSmokeEmitter);

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

                    if (turret != null && turret.Active == true && turret.Selected == true && CurrentProjectile != null)
                    {
                        List<Trap> HitTraps = TrapList.FindAll(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) != null && Trap.Solid == true);
                        List<Invader> HitInvaders = InvaderList.FindAll(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) != null);
                        Vector2 CollisionEnd;

                        #region Invader AND Trap
                        //If a projectile hit an invader AND a solid trap
                        if (TrapList.Any(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) != null && Trap.Solid == true &&
                            InvaderList.Any(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) != null &&
                            CurrentProjectile.Active == true)))
                        {
                            //First Invader to be hit
                            float MinDistInv = (float)HitInvaders.Min(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray));
                            Invader HitInvader = HitInvaders.Find(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) == MinDistInv);

                            //First solid trap to be hit
                            float MinDistTrap = (float)HitTraps.Min(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray));
                            Trap HitTrap = HitTraps.Find(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) == MinDistTrap);

                            //

                            #region A trap was hit first
                            if (HitTraps.Any(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) < MinDistInv))
                            {
                                CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)MinDistTrap),
                                                           turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)MinDistTrap));

                                CreateEffect(turret.BarrelEnd, CollisionEnd, CurrentProjectile.LightProjectileType);
                                TrapEffect(CollisionEnd, turret, HitTrap);
                            }
                            #endregion

                            #region An invader was hit first
                            if (HitInvaders.Any(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) < MinDistTrap))
                            {
                                CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)MinDistInv),
                                                           turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)MinDistInv));

                                CreateEffect(turret.BarrelEnd, CollisionEnd, CurrentProjectile.LightProjectileType);
                                InvaderEffect(turret, HitInvader, CollisionEnd);
                            }
                            #endregion
                        }
                        #endregion

                        #region Trap
                        //If a projectile hit just a solid trap
                        if (HitTraps.Any(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) != null && Trap.Solid == true &&
                            InvaderList.All(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) == null &&
                            CurrentProjectile.Active == true)))
                        {
                            float MinDistTrap = (float)HitTraps.Min(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray));
                            Trap HitTrap = HitTraps.Find(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) == MinDistTrap);

                            //Vector2 CollisionEnd;
                            CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)MinDistTrap),
                                                           turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)MinDistTrap));

                            CreateEffect(turret.BarrelEnd, CollisionEnd, CurrentProjectile.LightProjectileType);
                            TrapEffect(CollisionEnd, turret, HitTrap);
                        }
                        #endregion

                        #region Invader
                        //If a projectile hit just an invader or hit an invader and a non-solid trap
                        if (HitTraps.All(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                            InvaderList.Any(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) != null) &&
                            CurrentProjectile.Active == true)
                        {
                            float MinDistInv = (float)HitInvaders.Min(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray));
                            Invader HitInvader = HitInvaders.Find(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) == MinDistInv);

                            //Vector2 CollisionEnd;

                            CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)MinDistInv),
                                                       turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)MinDistInv));

                            CreateEffect(turret.BarrelEnd, CollisionEnd, CurrentProjectile.LightProjectileType);
                            InvaderEffect(turret, HitInvader, CollisionEnd);
                        }
                        #endregion

                        #region Ground
                        //If a projectile doesn't hit a trap or an invader but does hit the ground
                        if (HitTraps.All(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                            InvaderList.All(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                            CurrentProjectile.Ray.Intersects(Ground.BoundingBox) != null &&
                            CurrentProjectile.Active == true)
                        {
                            var DistToGround = CurrentProjectile.Ray.Intersects(Ground.BoundingBox);
                            //Vector2 CollisionEnd;

                            if (DistToGround != null)
                            {
                                CollisionEnd = new Vector2(turret.BarrelCenter.X + (CurrentProjectile.Ray.Direction.X * (float)DistToGround),
                                                           turret.BarrelCenter.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToGround));

                                CollisionEnd.Y += (float)RandomDouble(-turret.AngleOffset * 5, turret.AngleOffset * 5);

                                CreateEffect(turret.BarrelEnd, CollisionEnd, CurrentProjectile.LightProjectileType);
                                GroundEffect(turret.BarrelEnd, CollisionEnd, CurrentProjectile.LightProjectileType);
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
                            //Vector2 CollisionEnd;
                            CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * 1920),
                                                       turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * 1920));

                            CreateEffect(turret.BarrelEnd, CollisionEnd, CurrentProjectile.LightProjectileType);
                        }
                        #endregion

                        if (CurrentProjectile != null)
                            if (CurrentProjectile.LightProjectileType != LightProjectileType.PersistentBeam &&
                                projectile.LightProjectileType != LightProjectileType.PersistentBeam)
                            {
                                CurrentProjectile = null;
                                projectile.Active = false;
                            }
                            else
                            {
                                CurrentProjectile.Ray.Direction = new Vector3(turret.FireDirection, 0);
                            }

                        if (CurrentProjectile != null)
                            if (PreviousMouseState.LeftButton == ButtonState.Pressed &&
                                CurrentMouseState.LeftButton == ButtonState.Released &&
                                CurrentProjectile.LightProjectileType == LightProjectileType.PersistentBeam)
                            {
                                CurrentProjectile = null;
                                projectile.Active = false;
                            }

                        if (CurrentProjectile != null)
                            if (turret.TurretType == TurretType.PersistentBeam &&
                                CurrentProjectile.LightProjectileType == LightProjectileType.PersistentBeam &&
                                turret.CurrentHeat >= turret.MaxHeat)
                            {
                                CurrentProjectile = null;
                                projectile.Active = false;
                            }

                    }
                }
            }

            for (int i = 0; i < LightProjectileList.Count; i++)
            {
                if (LightProjectileList[i].Active == false)
                    LightProjectileList.RemoveAt(i);
            }
        }
        #endregion

        #region TRAP stuff that needs to be called every step
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
                SelectedTrap != null)
            {
                if (CursorPosition.Y < 672 || CursorPosition.Y > 928)
                {
                    return;
                }

                Vector2 NewTrapPosition = new Vector2(CursorPosition.X - CursorPosition.X % 32 + CurrentCursorTexture.Width / 2, CursorPosition.Y - CursorPosition.Y % 32 + CurrentCursorTexture.Height);

                if (Resources >= TrapCost(SelectedTrap.Value))
                {
                    Resources -= TrapCost(SelectedTrap.Value);

                    switch (SelectedTrap)
                    {
                        case TrapType.Wall:
                            {
                                NewTrap = new Wall(NewTrapPosition);
                                NewTrap.TextureList = WallSprite;
                                TrapList.Add(NewTrap);

                                ReadyToPlace = false;
                                PlaceTrap.Play();
                                ClearSelected();
                            }
                            break;

                        case TrapType.Fire:
                            {
                                FireTrapStart.Play();

                                NewTrap = new FireTrap(NewTrapPosition);
                                NewTrap.TextureList = FireTrapSprite;
                                TrapList.Add(NewTrap);

                                Color SmokeColor = Color.DarkGray;
                                SmokeColor.A = 200;

                                Color SmokeColor2 = Color.Gray;
                                SmokeColor.A = 175;

                                Emitter FireEmitter = new Emitter(FireParticle, new Vector2(NewTrap.Position.X, NewTrap.Position.Y),
                                  new Vector2(70, 110), new Vector2(0.5f * 1.5f, 0.75f * 1.5f), new Vector2(40 * 1.5f, 60 * 1.5f), 0.01f, true, new Vector2(-4, 4),
                                  new Vector2(-4, 4), new Vector2(1 * 1.5f, 2 * 1.5f), FireColor, FireColor2, 0.0f, -1, 25 * 1.5f, 1, false, new Vector2(0, 1080),
                                  false, CursorPosition.Y / 1080);

                                Emitter SmokeEmitter = new Emitter(SmokeParticle, new Vector2(NewTrap.Position.X, NewTrap.Position.Y - 4),
                                    new Vector2(80, 100), new Vector2(0.2f * 1.5f, 0.5f * 1.5f), new Vector2(5000 * 1.5f, 8000 * 1.5f), 0.1f, true, new Vector2(-20, 20),
                                    new Vector2(-2, 2), new Vector2(0.5f * 1.5f, 0.8f * 1.5f), SmokeColor, SmokeColor2, 0.0f, -1, 150, 1, false,
                                    new Vector2(0, 1080), true, (CursorPosition.Y - 1) / 1080);

                                Emitter SparkEmitter = new Emitter(SplodgeParticle, new Vector2(NewTrap.Position.X, NewTrap.Position.Y - 4),
                                  new Vector2(80, 100), new Vector2(1 * 1.5f, 1 * 1.5f), new Vector2(60 * 1.5f, 80 * 1.5f), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                  new Vector2(0.01f * 1.5f, 0.02f * 1.5f), Color.Yellow, Color.Orange, -0.005f * 1.5f, -1, 25, 1, false, new Vector2(0, NewTrap.Position.Y + 16));

                                NewTrap.TrapEmitterList.Add(SmokeEmitter);
                                NewTrap.TrapEmitterList.Add(FireEmitter);
                                NewTrap.TrapEmitterList.Add(SparkEmitter);

                                ReadyToPlace = false;
                                ClearSelected();
                            }
                            break;

                        case TrapType.Spikes:
                            {
                                NewTrap = new SpikeTrap(NewTrapPosition);
                                NewTrap.TextureList = SpikeTrapSprite;
                                TrapList.Add(NewTrap);

                                ReadyToPlace = false;
                                ClearSelected();
                            }
                            break;

                        case TrapType.Catapult:
                            {
                                NewTrap = new CatapultTrap(NewTrapPosition);
                                NewTrap.TextureList = CatapultTrapSprite;
                                TrapList.Add(NewTrap);

                                ReadyToPlace = false;
                                ClearSelected();
                            }
                            break;

                        case TrapType.Ice:
                            {
                                NewTrap = new IceTrap(NewTrapPosition);
                                NewTrap.TextureList = IceTrapSprite;
                                TrapList.Add(NewTrap);

                                ReadyToPlace = false;
                                ClearSelected();
                            }
                            break;

                        case TrapType.Tar:
                            {
                                NewTrap = new TarTrap(NewTrapPosition);
                                NewTrap.TextureList = TarTrapSprite;
                                TrapList.Add(NewTrap);

                                ReadyToPlace = false;
                                ClearSelected();
                            }
                            break;

                        case TrapType.Barrel:
                            {
                                NewTrap = new BarrelTrap(NewTrapPosition);
                                NewTrap.TextureList = BarrelTrapSprite;
                                TrapList.Add(NewTrap);

                                ReadyToPlace = false;
                                PlaceTrap.Play();
                                ClearSelected();
                            }
                            break;

                        case TrapType.SawBlade:
                            {
                                NewTrap = new SawBladeTrap(NewTrapPosition);
                                NewTrap.TextureList = SawBladeTrapSprite;
                                TrapList.Add(NewTrap);

                                ReadyToPlace = false;
                                ClearSelected();
                            }
                            break;

                        case TrapType.Line:
                            {
                                NewTrap = new LineTrap(NewTrapPosition);
                                NewTrap.TextureList = LineTrapSprite;
                                TrapList.Add(NewTrap);

                                Emitter FireEmitter = new Emitter(FireParticle, new Vector2(NewTrap.Position.X, NewTrap.Position.Y),
                                  new Vector2(0, 0), new Vector2(1.5f, 2.0f), new Vector2(40 * 1.5f, 60 * 1.5f), 0.01f, true, new Vector2(-4, 4),
                                  new Vector2(-4, 4), new Vector2(1 * 1.5f, 2 * 1.5f), FireColor, FireColor2, 0.0f, -1, 25 * 1.5f, 3, false, new Vector2(0, 1080),
                                  false, CursorPosition.Y / 1080);

                                NewTrap.TrapEmitterList.Add(FireEmitter);

                                ReadyToPlace = false;
                                ClearSelected();
                            }
                            break;
                    }

                    NewTrap.Initialize();
                    NewTrap.HealthBar.Box = WhiteBlock;
                    NewTrap.TimingBar.Box = WhiteBlock;
                }
            }

        }

        private void TrapUpdate(GameTime gameTime)
        {
            foreach (Trap trap in TrapList)
            {
                trap.Update(gameTime);

                #region Remove trap if middle-clicked
                if (trap.DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)) &&
                    CurrentMouseState.MiddleButton == ButtonState.Released &&
                    PreviousMouseState.MiddleButton == ButtonState.Pressed)
                {
                    if (trap.Active == true && trap.CurrentHP == trap.MaxHP && trap.CurrentDetonateLimit == trap.DetonateLimit)
                    {
                        trap.Active = false;
                        Resources += trap.ResourceCost;
                    }
                }
                #endregion

                #region Specific behaviour based on the trap type
                switch (trap.TrapType)
                {
                    case TrapType.Fire:
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

                        if (trap.Affected == false)
                        {
                            foreach (Emitter emitter in trap.TrapEmitterList)
                            {
                                emitter.AngleRange = Vector2.Lerp(emitter.AngleRange, new Vector2(70, 110), 0.02f);
                                emitter.SpeedRange = Vector2.Lerp(emitter.SpeedRange, new Vector2(0.5f, 0.75f), 0.02f);
                            }

                        }
                        break;

                    case TrapType.Barrel:
                        if (trap.CurrentHP <= 0)
                        {
                            trap.Active = false;
                            Emitter ExplosionEmitter2 = new Emitter(SplodgeParticle, new Vector2(trap.Position.X, trap.DestinationRectangle.Bottom),
                                       new Vector2(0, 180), new Vector2(1, 4), new Vector2(10, 30), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                       new Vector2(0.02f, 0.06f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 0.2f, 20, 10, true, new Vector2(trap.DestinationRectangle.Bottom + 8, trap.DestinationRectangle.Bottom + 8));

                            EmitterList.Add(ExplosionEmitter2);

                            Emitter ExplosionEmitter = new Emitter(FireParticle,
                                    new Vector2(trap.Position.X, trap.DestinationRectangle.Bottom),
                                    new Vector2(0, 180), new Vector2(1, 5), new Vector2(1, 20), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 7, false,
                                    new Vector2(0, 1080), true);

                            EmitterList.Add(ExplosionEmitter);

                            Color SmokeColor1 = Color.Lerp(Color.DarkGray, Color.Transparent, 0.5f);
                            Color SmokeColor2 = Color.Lerp(Color.Gray, Color.Transparent, 0.5f);

                            Emitter newEmitter2 = new Emitter(SmokeParticle,
                                    new Vector2(trap.Position.X, trap.DestinationRectangle.Bottom),
                                    new Vector2(80, 100), new Vector2(0.5f, 1f), new Vector2(20, 40), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(0, 0), new Vector2(1, 2), SmokeColor1, SmokeColor2, 0.0f, 0.3f, 1, 1, false,
                                    new Vector2(0, 1080), false);

                            EmitterList2.Add(newEmitter2);

                            ExplosionList.Add(new Explosion(PointToVector(trap.DestinationRectangle.Center), 150, 100));
                        }
                        break;
                }
                #endregion

                #region Update all of the particle emitters for the traps
                foreach (Emitter emitter in trap.TrapEmitterList)
                {
                    if (emitter.AddMore == false && emitter.ParticleList.Count == 0)
                    {
                        trap.Active = false;
                    }
                }
                #endregion
            }

            for (int i = 0; i < TrapList.Count; i++)
            {
                if (TrapList[i].Active == false)
                    TrapList.RemoveAt(i);
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
                        invader.TrapDamage(HitTrap.TrapType);

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
                                        invader.FireEmitter = new Emitter(FireParticle, invader.Position,
                                        new Vector2(0, 90), new Vector2(0.25f * 1.5f, 0.5f * 1.5f), new Vector2(40 * 0.75f, 60 * 0.75f), 0.01f, true, new Vector2(-4, 4),
                                        new Vector2(-4, 4), new Vector2(1 * 1.5f, 2 * 1.5f), FireColor, FireColor2, 0.0f, -1, 25 * 0.25f, 1, false, new Vector2(0, 1080),
                                        false, invader.DrawDepth + 0.1f);

                                        HitTrap.TrapEmitterList[1].EndColor =
                                            Color.Lerp(HitTrap.TrapEmitterList[1].EndColor, Color.Red, 0.1f);

                                        HitTrap.TrapEmitterList[1].StartColor =
                                                Color.Lerp(HitTrap.TrapEmitterList[1].EndColor, Color.Red, 0.1f);
                                        break;
                                }
                                break;
                            #endregion

                            #region Sawblade trap
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
                ProfileName = "empty";
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

                DeleteProfileDialog = new DialogBox(DialogBox, ButtonLeftSprite, ButtonRightSprite, DefaultFont, new Vector2(1920 / 2, 1080 / 2), "Delete", "Do you want to delete " + ThisProfile.Name + "?", "Cancel");
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
                FlameThrower = false,
                Cluster = false,
                Lightning = true,
                FelCannon = false,
                Beam = false,
                Freeze = false,
                Grenade = false,
                Shotgun = false,
                PulseGun = false,
                PersistentBeam = false,
                Boomerang = false,

                //Traps available on profile creation
                Fire = true,
                Spikes = true,                
                SawBlade = false,
                Wall = true,
                Ice = false,                
                Barrel = false,
                Line = false,
                Trigger = false,                
                Tar = true,
                Catapult = false,

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

                //ADD TO THE WAVE TIME
                if (CurrentWaveTime < CurrentWave.TimeToNextWave)
                {
                    if (StartWave == true)
                    {
                        CurrentWaveTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    }
                }

                //IF IT'S TIME TO START ADDING INVADERS
                if (CurrentWaveTime >= CurrentWave.TimeToNextWave)
                {                    
                    //ADD TO THE INVADER TIME
                    if (CurrentInvaderTime < CurrentWave.TimeToNextInvader)
                        CurrentInvaderTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                    //START ADDING INVADERS IF IT'S TIME
                    if (CurrentInvaderTime >= CurrentWave.TimeToNextInvader)
                    {
                        if (CurrentInvaderIndex < CurrentWave.InvaderList.Count)
                        {
                            //IF IT'S AN ACTUAL INVADER
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

                                nextInvader.Initialize();                                
                                InvaderList.Add(nextInvader);
                                CurrentInvaderTime = 0;
                                CurrentWave.InvaderList[CurrentInvaderIndex] = null;
                                CurrentInvaderIndex++;
                                return;
                            }
                            //IF IT'S A PAUSE
                            

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

                            if (CurrentWave.InvaderList[CurrentInvaderIndex] is float)
                            {
                                CurrentWave.TimeToNextInvader = (float)CurrentWave.InvaderList[CurrentInvaderIndex];
                                CurrentInvaderIndex++;
                                CurrentInvaderTime = 0;
                                CurrentWavePauseTime = 0;
                                return;
                            }
                        }
                        //RESET THE WAVE AND INVADER TIMERS BECAUSE THE INVADERS FOR THIS LIST HAVE RUN OUT
                        else
                        {
                            if (CurrentWave.Overflow == true)
                            {
                                if (InvaderList.Count == 0)
                                {
                                    CurrentWaveTime = 0;
                                    CurrentInvaderIndex = 0;
                                    CurrentInvaderTime = 0;
                                    CurrentWaveIndex++;
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
                    }
                }
            }
        }

        //Handle levels
        public void LoadLevel(int number)
        {
            //CurrentLevel = Content.Load<Level>("Levels/Level" + number);
            //var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

            Assembly assembly = Assembly.Load("TowerDefensePrototype");
            Type t = assembly.GetType("TowerDefensePrototype.Level" + number);
            CurrentLevel = (Level)Activator.CreateInstance(t);

            CurrentWaveIndex = 0;
            CurrentInvaderIndex = 0;

            CurrentWaveTime = 0;
            CurrentInvaderTime = 0;

            CurrentWave = CurrentLevel.WaveList[0];

            StartWave = false;

            Resources = CurrentLevel.Resources;
        }


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

        //Handle upgrades
        public Turret ApplyUpgrades(Turret turret)
        {
            Turret UpgradedTurret = turret;

            switch (turret.TurretType)
            {
                case TurretType.MachineGun:
                    UpgradedTurret.FireDelay = (int)PercentageChange(turret.FireDelay, -MachineGunTurretSpeed);
                    UpgradedTurret.Damage = (int)PercentageChange(turret.Damage, MachineGunTurretDamage);
                    UpgradedTurret.AngleOffset = (int)PercentageChange(turret.AngleOffset, -MachineGunTurretAccuracy);
                    break;

                case TurretType.Cannon:
                    UpgradedTurret.FireDelay = (int)PercentageChange(turret.FireDelay, -CannonTurretSpeed);
                    UpgradedTurret.Damage = (int)PercentageChange(turret.Damage, CannonTurretDamage);
                    UpgradedTurret.BlastRadius = (int)PercentageChange(turret.BlastRadius, CannonTurretBlastRadius);
                    break;
            }

            return UpgradedTurret;
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


        //Some math functions
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
                        spriteBatch.Draw(CurrentCursorTexture, new Rectangle((int)(CursorPosition.X - CursorPosition.X % 32),
                                        (int)(CursorPosition.Y - CursorPosition.Y % 32), CurrentCursorTexture.Width, CurrentCursorTexture.Height),
                                        null, CursorColor, MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, 1);
                    else
                        spriteBatch.Draw(CurrentCursorTexture, new Rectangle((int)(CursorPosition.X), (int)(CursorPosition.Y),
                                    CurrentCursorTexture.Width, CurrentCursorTexture.Height),
                                    null, CursorColor, MathHelper.ToRadians(0), new Vector2(CurrentCursorTexture.Width / 2, CurrentCursorTexture.Height), SpriteEffects.None, 1);


                    if (PrimaryCursorTexture != CrosshairCursor)
                        spriteBatch.Draw(PrimaryCursorTexture, new Rectangle((int)CursorPosition.X, (int)CursorPosition.Y,
                                         PrimaryCursorTexture.Width, PrimaryCursorTexture.Height), null, Color.White,
                                         MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, 1);
                    else
                        spriteBatch.Draw(PrimaryCursorTexture, new Rectangle((int)CursorPosition.X - PrimaryCursorTexture.Width / 2,
                                         (int)CursorPosition.Y - PrimaryCursorTexture.Height / 2, PrimaryCursorTexture.Width,
                                         PrimaryCursorTexture.Height), null, Color.White, MathHelper.ToRadians(0), Vector2.Zero,
                                         SpriteEffects.None, 1);
                }
            }
        }


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
            if (CurrentMouseState.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed)
            {
                SelectedTurret = null;
                SelectedTrap = null;

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
            SelectedTurret = null;
            SelectedTrap = null;
            ReadyToPlace = false;
        }
        #endregion

        private bool RandomBool()
        {
            if ((float)Random.NextDouble() > 0.5f)
                return true;
            else
                return false;
        }

        private Color RandomColor(params Color[] colors)
        {
            List<Color> ColorList = new List<Color>();

            foreach (Color color in colors)
            {
                ColorList.Add(color);
            }

            return ColorList[Random.Next(0, ColorList.Count)];
        }


        private void HandlePlacedIcons()
        {
            foreach (Button button in PlaceWeaponList)
            {
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
            foreach (WeaponBox turretBox in TurretBoxes)
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
                            var icon = this.GetType().GetField(WeaponName + "TurretIcon").GetValue(this);
                            turretBox.IconTexture = (Texture2D)icon;
                        }
                        else
                        {
                            turretBox.IconTexture = LockIcon;
                        }
                        break;
                }
            }

            foreach (WeaponBox trapBox in TrapBoxes)
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
                            var icon = this.GetType().GetField(WeaponName + "TrapIcon").GetValue(this);
                            trapBox.IconTexture = (Texture2D)icon;
                        }
                        else
                        {
                            trapBox.IconTexture = LockIcon;
                        }
                        break;
                }
            }
            //foreach (WeaponBox turretBox in TurretBoxes)
            //{
            //    switch (turretBox.ContainsTurret)
            //    {
            //        case null:

            //            break;

            //        default:
            //            {
            //                string WeaponName = turretBox.ContainsTurret.ToString();
            //                var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

            //                if ((bool)Available == true)
            //                    turretBox.IconName = "Icons/TurretIcons/" + WeaponName + "TurretIcon";
            //                else
            //                    turretBox.IconName = "Icons/LockIcon";
            //            }
            //            break;
            //    }

            //    turretBox.LoadContent(SecondaryContent);
            //}

            //foreach (WeaponBox trapBox in TrapBoxes)
            //{
            //    switch (trapBox.ContainsTrap)
            //    {
            //        case null:

            //            break;

            //        default:
            //            {
            //                string WeaponName = trapBox.ContainsTrap.ToString();
            //                var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

            //                if ((bool)Available == true)
            //                    trapBox.IconName = "Icons/TrapIcons/" + WeaponName + "TrapIcon";
            //                else
            //                    trapBox.IconName = "Icons/LockIcon";
            //            }
            //            break;                        
            //    }

            //    trapBox.LoadContent(SecondaryContent);
            //}
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

                case TrapType.Tar:
                    cost = new TarTrap(Vector2.Zero).ResourceCost;
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
        }
    }
}