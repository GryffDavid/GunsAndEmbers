using System;
using System.Collections.ObjectModel;
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

        
    public enum Weather { Snow };
    public enum WorldType { Snowy };


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
        GraphicsDeviceManager graphics;
        ContentManager SecondaryContent;
        SpriteBatch spriteBatch;
        RenderTarget2D GameRenderTarget, MenuRenderTarget, UIRenderTarget;

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
        Texture2D BloodDecal1, ExplosionDecal1, GlueDecal1;
        #endregion

        #region Particle sprites
        Texture2D BlankTexture, BigShellCasing, SmallShellingCasing, LightningShellCasing, Coin, RoundSparkParticle, HealthParticle,
                  HealthBarSprite, SplodgeParticle, SmokeParticle, FireParticle, FireParticle2, ExplosionParticle,
                  ExplosionParticle2, BallParticle, SparkParticle, BulletTrailCap, BulletTrailSegment, BlurrySnowflake,
                  FocusedSnowflake, MachineBullet, HitEffectParticle, BOOMParticle, PINGParticle, SNAPParticle,
                  WHAMParticle, BAMParticle, FWOOMParticle, SPLATParticle,
                  ToonBloodDrip1, ToonGlueDrip1;

        Texture2D ToonSmoke1, ToonSmoke2, ToonSmoke3, ToonSmoke4, ToonSmoke5, ToonSmoke6, ToonSmoke7;
        Texture2D ToonDust1, ToonDust2, ToonDust3, ToonDust4, ToonDust5, ToonDust6, ToonDust7;
        #endregion

        #region Icon sprites
        Texture2D LockIcon, HealthIcon, OverHeatIcon, CurrencyIcon, PowerUnitIcon, RightClickIcon;

        Dictionary<string, Texture2D> TurretIconDictionary = new Dictionary<string, Texture2D>();
        Dictionary<string, Texture2D> TrapIconDictionary = new Dictionary<string, Texture2D>();

        public Texture2D FearEmotionIcon;
        #endregion

        #region Cursor sprites
        //NEW_TURRET G **turret cursor declarations**
        Texture2D CurrentCursorTexture, PrimaryCursorTexture, DefaultCursor, CrosshairCursor;

        //Turret Cursors        
        public Texture2D MachineGunTurretCursor, CannonTurretCursor, FlameThrowerTurretCursor, LightningTurretCursor, ClusterTurretCursor,
                         FelCannonTurretCursor, BeamTurretCursor, FreezeTurretCursor, BoomerangTurretCursor, GrenadeTurretCursor, GasGrenadeTurretCursor,
                         ShotgunTurretCursor,
                         PersistentBeamTurretCursor, HarpoonTurretCursor, StickyMineTurretCursor, GrappleTurretCursor;

        //Trap Cursors
        public Texture2D WallTrapCursor, SpikesTrapCursor, CatapultTrapCursor, FireTrapCursor, IceTrapCursor, TarTrapCursor, BarrelTrapCursor,
                         SawBladeTrapCursor, LineTrapCursor, TriggerTrapCursor, LandMineTrapCursor, FlameThrowerTrapCursor, GlueTrapCursor;
        #endregion

        #region Trap sprites
        //NEW_TRAP B **trap animations list declarations here**
        Dictionary<string, List<TrapAnimation>> TrapAnimationsDictionary = new Dictionary<string, List<TrapAnimation>>();

        #endregion

        #region Turret sprites
        Texture2D TurretSelectBox;
        Dictionary<string, Texture2D> TurretSpriteDictionary = new Dictionary<string, Texture2D>();

        public Texture2D MachineGunTurretBarrelGib;
        #endregion

        #region Enemy sprites
        Texture2D IceBlock, Shadow, DropShipDoorTexture;
        Dictionary<string, List<InvaderAnimation>> InvaderAnimationsDictionary = new Dictionary<string, List<InvaderAnimation>>();

        public InvaderAnimation StationaryCannonBarrelAnimation, HarpoonCannonBarrelAnimation;
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

        public Color FireColor = new Color(Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 200);
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
                   RobotoBold40_2, RobotoRegular40_2, RobotoRegular20_2, RobotoRegular20_0, RobotoItalic20_0,
                   DialogueFont, RobotoBold20_0_Outline, RobotoBold20_2_Outline;
        #endregion

        #region Projectile sprites
        //NEW_HEAVYPROJECTILE B **heavy projectile sprite declarations here**
        //Dictionary<string, Texture2D> HeavySpriteDictionary = new Dictionary<string, Texture2D>();

        public Texture2D CannonBallProjectileSprite,
                         FlameThrowerProjectileSprite,
                         ArrowProjectileSprite,
                         AcidProjectileSprite,
                         TorpedoProjectileSprite,
                         ClusterBombProjectileSprite,
                         ClusterBombShellProjectileSprite,
                         FelProjectileSprite,
                         BoomerangProjectileSprite,
                         GrenadeProjectileSprite,
                         GasGrenadeProjectileSprite,
                         FireGrenadeProjectileSprite,
                         HarpoonProjectileSprite,
                         StickyMineProjectileSprite,
                         DropMissileProjectileSprite;

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
            }
        }

        private ButtonState _RightButtonState;
        public ButtonState RightButtonState
        {
            get { return _RightButtonState; }
            set { _RightButtonState = value; }
        }


        int TowerButtons, ProfileNumber, LevelNumber, TotalParticles;

        private int _Resources;
        public int Resources 
        {
            get { return _Resources; }
            set 
            {
                if (ResourceCounter != null)
                    ResourceCounter.AddChange(value - _Resources);

                _Resources = value;
            }
        }

        public int CurrentWaveIndex = 0;
        int CurrentInvaderIndex = 0;
        int MaxWaves = 0;

        string FileName, ContainerName, ProfileName;

        bool ReadyToPlace, IsLoading, AllLoaded, Slow;
        bool DialogVisible = false;
        bool Diagnostics = false;
        bool BoundingBoxes = false;
        public bool StartWave = false;
        bool Victory = false;

        float MenuSFXVolume, MenuMusicVolume, CurrentInvaderTime, CurrentWaveTime, CurrentWavePauseTime;

        double ResolutionOffsetRatio;

        Effect HealthBarEffect;
        Color CursorColor = Color.White;
        Matrix Projection, MouseTransform, QuadProjection;
        #endregion

        #region Sound effects
        SoundEffect MenuClick, FireTrapStart, LightningSound, CannonExplosion, CannonFire,
                    MachineShot1, GroundImpact, Ricochet1, Ricochet2, Ricochet3, MenuWoosh,
                    PlaceTrap, Splat1, Splat2, MenuMusic, Implosion, TurretOverheat;

        SoundEffect BatteringRamDrop1;

        //Ambient sounds
        //SoundEffect PolarWindAmbience;

        SoundEffectInstance MenuMusicInstance, TurretOverheatInstance;
        #endregion

        #region List declarations
        public List<Button> TowerButtonList;
        List<Button> MainMenuButtonList, PauseButtonList,
                     ProfileButtonList, ProfileDeleteList, PlaceWeaponList,
                     SpecialAbilitiesButtonList;

        public ObservableCollection<Trap> TrapList;
        //List<Trap> TrapList;
        public List<Turret> TurretList;
        public List<Invader> InvaderList;

        List<HeavyProjectile> HeavyProjectileList;

        List<SmokeTrail> SmokeTrailList = new List<SmokeTrail>();

        List<Emitter> YSortedEmitterList, AlphaEmitterList, AdditiveEmitterList, GasEmitterList;
        List<Particle> CoinList;

        List<string> PauseMenuNameList;
        
        List<LightningBolt> LightningList;
        List<BulletTrail> TrailList;

        List<NumberChange> NumberChangeList = new List<NumberChange>();

        List<StaticSprite> TerrainSpriteList, WeatherSpriteList;

        List<Decal> DecalList = new List<Decal>();

        List<WeaponBox> SelectTurretList, SelectTrapList;

        public List<CooldownButton> CooldownButtonList = new List<CooldownButton>();

        List<UIWeaponInfoTip> UIWeaponInfoList = new List<UIWeaponInfoTip>();

        List<ShellCasing> VerletShells = new List<ShellCasing>();

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
               VictoryContinue;
        Tower Tower;
        StaticSprite Ground, ForeGround, SkyBackground, TextBox, MenuTower;
        StaticSprite MenuBackground1;
        AnimatedSprite LoadingAnimation;
        public static Nullable<TrapType> SelectedTrap;
        public static Nullable<TurretType> SelectedTurret;
        //Nullable<SpecialType> SelectedSpecial;
        LightProjectile CurrentProjectile;
        ProfileManagementState ProfileManagementState;
        Thread LoadingThread;
        public Profile CurrentProfile;
        StorageDevice Device;
        Stream OpenFile;
        Settings CurrentSettings, DefaultSettings;
        TextInput NameInput;
        DialogBox ExitDialog, DeleteProfileDialog, MainMenuDialog, ProfileMenuDialog, NoWeaponsDialog, NameLengthDialog;
        Level CurrentLevel;
        Wave CurrentWave = null;
        BulletTrail Trail;
        LightningBolt Lightning = new LightningBolt(Vector2.One, Vector2.Zero, Color.White, 1);
        LightningBolt Bolt = new LightningBolt(Vector2.One, Vector2.Zero, Color.White, 1);
        Trap NewTrap;
        Button StartWaveButton;
        //WaveCountDown WaveCountDown;
        public Turret CurrentTurret;
        Tabs ProfileManagementTabs;
        //BeamProjectile CurrentBeam;
        //UIInvaderInfo UIInvaderInfo;
        #endregion

        _2DCamera Camera = new _2DCamera();

        UISlopedBar HealthBar, ShieldBar;

        BasicEffect QuadEffect;
        BasicEffect SmokeBasicEffect;

        FrameRateCounter FPSCounter = new FrameRateCounter();

        SpecialAbility CurrentSpecialAbility;

        //float CurrentWeatherTime;
        Nullable<Weather> CurrentWeather;

        PowerupDelivery PowerupDelivery;

        public static List<Drawable> DrawableList = new List<Drawable>();


        #region For lighting - needs to be moved once lighting all works properly


        BasicEffect BasicEffect, BasicEffect2, ProjectileBasicEffect, JetBasicEffect;
        Effect ShadowBlurEffect;

        #endregion

        Texture2D JetEngineSprite, ExplosionRingSprite, ShieldSprite;
        
        Effect ParticleEffect;
        Effect DepthEffect, BlurEffect, CrepEffect, LightCombined, LightEffect;
        PartitionedBar PowerUnitsBar, MusicVolumeBar, SoundEffectsVolumeBar;
        CheckBox BloodEffectsToggle, FullscreenToggle;

        List<StoryDialogueBox> TutorialBoxList = new List<StoryDialogueBox>();

        Texture2D ShieldBoundingSphere, RopeTexture1;

        AnimatedSprite ThinkingAnimation;
        ResourceCounter ResourceCounter;

        List<AmmoBelt> AmmoBeltList = new List<AmmoBelt>();
        List<ExplosionEffect> ExplosionEffectList = new List<ExplosionEffect>();
        List<Shield> ShieldList = new List<Shield>();
        List<ToonLightning> ToonLightningList = new List<ToonLightning>();
        List<StickyMine> StickyMineList = new List<StickyMine>();

        Vector2 GroundRange = new Vector2(672, 896);
        //List<SmokeTrail> SmokeTrailList = new List<SmokeTrail>();

        //^:b*[^:b#/]+.*$//
        //Regular expression to count KLOC

        List<myRay> ExplosionRays = new List<myRay>();

        List<Rope> RopeList = new List<Rope>();
        List<JetEngine> JetEngineList = new List<JetEngine>();

        public LevelDialogue LoadoutMenuDialogue;

        LightProjectile CurrentBeam;
        
        //StoryDialogueItems Level1Dialogue;

        #endregion


        #region Commonly re-used emitters. Use ShallowCopy to get.

        #endregion
        
        RenderTarget2D EmissiveMap, BlurMap, ColorMap, NormalMap, LightMap, FinalMap, SpecMap, DepthMap, ShadowMap;
        RenderTarget2D CrepLightMap, CrepColorMap, OcclusionMap;
        RenderTarget2D Buffer1, Buffer2;

        VertexPositionColorTexture[] LightVertices;
        VertexPositionColorTexture[] EmissiveVertices;
        VertexPositionColorTexture[] CrepVertices;

        List<Light> LightList = new List<Light>();
        List<CrepuscularLight> CrepLightList = new List<CrepuscularLight>();
        List<Solid> SolidList = new List<Solid>();
        List<PolygonShadow> ShadowList = new List<PolygonShadow>();
        List<myRay> RayList = new List<myRay>();
        Texture2D CrepuscularLightTexture;

        Color AmbientLight = new Color(0.75f, 0.75f, 0.75f, 1f);
        //Color AmbientLight = new Color(0.15f, 0.15f, 0.15f, 1f);


        public static BlendState BlendBlack = new BlendState()
        {
            ColorBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,

            AlphaBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.One
        };

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
            //graphics.SynchronizeWithVerticalRetrace = false;
            SetUpGameWindow();
            //IsFixedTimeStep = false;
            //TargetElapsedTime = TimeSpan.FromMilliseconds(1000f / 60f);

            Content.RootDirectory = "Content";

        }

        protected override void Initialize()
        {
            SecondaryContent = new ContentManager(Content.ServiceProvider, Content.RootDirectory);
            GameState = GameState.Menu;
            ProfileManagementState = ProfileManagementState.Loadout;
            ContainerName = "Profiles";

            string thing = Directory.GetParent(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\..\\..\\")).Name;
            Window.Title = thing;

            Tower = new Tower("Tower", new Vector2(100, 450), 1500, 400, 3, 5000, 20);
            TowerButtons = (int)Tower.Slots;
            ScreenRectangle = new Rectangle(-256, -256, 1920 + 256, 1080 + 256);

            QuadProjection = Matrix.CreateOrthographicOffCenter(0, 1920, 1080, 0, 0, 1);

            QuadEffect = new BasicEffect(graphics.GraphicsDevice);
            QuadEffect.Projection = QuadProjection;
            QuadEffect.VertexColorEnabled = true;

            BasicEffect2 = new BasicEffect(graphics.GraphicsDevice);
            BasicEffect2.Projection = QuadProjection;
            BasicEffect2.VertexColorEnabled = true;


            SmokeBasicEffect = new BasicEffect(GraphicsDevice);
            Projection = Matrix.CreateOrthographicOffCenter(0, 1920, 1080, 0, 0, 1);

            SmokeBasicEffect.Projection = Projection;
            SmokeBasicEffect.VertexColorEnabled = true;

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
            
            //Subscribe to events
            ExplosionHappenedEvent += OnExplosionHappened;
            TrapCollisionEvent += OnTrapCollision;
            LightProjectileFiredEvent += OnLightProjectileFired;
            TurretShootEvent += OnTurretShoot;
            RightClickEvent += OnRightClick;
            TrapPlacementEvent += OnTrapPlacement;
            HeavyProjectileCollisionEvent += OnHeavyProjectileCollision;

            LoadingAnimation = new AnimatedSprite("LoadingAnimation", new Vector2(1920 / 2 - 65, 1080 / 2 - 65), new Vector2(131, 131), 17, 30, new Color(255,255,255,10), Vector2.One, true);
            LoadingAnimation.LoadContent(SecondaryContent);
            IsLoading = false;

            base.Initialize();
        }

        #region CONTENT that needs to be loaded
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            GameRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080, false, SurfaceFormat.Rgba64, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
            UIRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080, false, SurfaceFormat.Rgba64, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            MenuRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);

            Projection = Matrix.CreateOrthographicOffCenter(0, 1920, 1080, 0, -5.0f, 5.0f);

            MenuBackground1.LoadContent(SecondaryContent);

            BlankTexture = SecondaryContent.Load<Texture2D>("Blank");

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

                ExplosionRingSprite = Content.Load<Texture2D>("ExplosionRingSprite");
                ShieldSprite = Content.Load<Texture2D>("ShieldSprite");
                
                #region Just to handle lighting
                BasicEffect = new BasicEffect(GraphicsDevice);
                BasicEffect.Projection = Projection;
                BasicEffect.TextureEnabled = true;
                BasicEffect.VertexColorEnabled = true;
                
                ProjectileBasicEffect = new BasicEffect(GraphicsDevice);
                ProjectileBasicEffect.Projection = Projection;
                ProjectileBasicEffect.TextureEnabled = true;
                ProjectileBasicEffect.VertexColorEnabled = true;

                JetBasicEffect = new BasicEffect(GraphicsDevice);
                JetBasicEffect.Projection = Projection;
                JetBasicEffect.TextureEnabled = true;
                JetBasicEffect.VertexColorEnabled = true;

                ShadowBlurEffect = Content.Load<Effect>("Shaders/ShadowBlur");
                ShadowBlurEffect.Parameters["Projection"].SetValue(Projection);                
                #endregion
                             
                ThinkingAnimation = new AnimatedSprite("ThinkingAnimation", Vector2.Zero, new Vector2(24, 24), 6, 100, Color.White, new Vector2(1, 1), true);
                ThinkingAnimation.LoadContent(Content);

                CrepuscularLightTexture = Content.Load<Texture2D>("Flare1");

                //CrepLightList.Add(new CrepuscularLight()
                //{
                //    Position = new Vector2(1280 / 2, 720 / 2),
                //    Decay = 0.9999f,
                //    Exposure = 0.23f,
                //    Density = 0.826f,
                //    Weight = 0.358767f
                //});

                CrepLightList.Add(new CrepuscularLight()
                {
                    Position = new Vector2(800, 150),
                    Decay = 0.9999f,
                    Exposure = 0.53f,
                    Density = 0.826f,
                    Weight = 0.958767f,
                    Depth = 0f
                });

                LightList.Add(new Light()
                {
                    Color = Color.White,
                    Active = true,
                    Power = 1.9f,
                    Position = new Vector3(800, 150, 100),
                    Size = 1920,
                    Depth = 0f
                });


                //LightList.Add(new Light()
                //{
                //    Color = Color.Plum,
                //    Active = true,
                //    Power = 0.7f,
                //    Position = new Vector3(100, 100, 100),
                //    Size = 800
                //});

                //LightList.Add(new Light()
                //{
                //    Color = Color.Silver,
                //    Active = true,
                //    Power = 0.8f,
                //    Position = new Vector3(500, 500, 100),
                //    Size = 1600
                //});

                #region Prepare Render Targets
                Buffer2 = new RenderTarget2D(GraphicsDevice, 1920, 1080, false, SurfaceFormat.Rgba64, DepthFormat.None, 1, RenderTargetUsage.PreserveContents);
                Buffer1 = new RenderTarget2D(GraphicsDevice, 1920, 1080);

                OcclusionMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);

                EmissiveMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);
                BlurMap = new RenderTarget2D(GraphicsDevice, 1920, 1080); 
                ColorMap = new RenderTarget2D(GraphicsDevice, 1920, 1080); 
                NormalMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);
                LightMap = new RenderTarget2D(GraphicsDevice, 1920, 1080, false, SurfaceFormat.Rgba64, DepthFormat.None, 8, RenderTargetUsage.PreserveContents);

                FinalMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);
                SpecMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);
                CrepLightMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);
                CrepColorMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);
                DepthMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);

                ShadowMap = new RenderTarget2D(GraphicsDevice, 1920, 1080);

                LightVertices = new VertexPositionColorTexture[4];
                LightVertices[0] = new VertexPositionColorTexture(new Vector3(-1, 1, 0), Color.White, new Vector2(0, 0));
                LightVertices[1] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0));
                LightVertices[2] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1));
                LightVertices[3] = new VertexPositionColorTexture(new Vector3(1, -1, 0), Color.White, new Vector2(1, 1));

                CrepVertices = new VertexPositionColorTexture[4];
                CrepVertices[0] = new VertexPositionColorTexture(new Vector3(-1, 1, 0), Color.White, new Vector2(0, 0));
                CrepVertices[1] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0));
                CrepVertices[2] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1));
                CrepVertices[3] = new VertexPositionColorTexture(new Vector3(1, -1, 0), Color.White, new Vector2(1, 1));

                EmissiveVertices = new VertexPositionColorTexture[6];
                EmissiveVertices[0] = new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0));
                EmissiveVertices[1] = new VertexPositionColorTexture(new Vector3(1920, 0, 0), Color.White, new Vector2(1, 0));
                EmissiveVertices[2] = new VertexPositionColorTexture(new Vector3(1920, 1080, 0), Color.White, new Vector2(1, 1));
                EmissiveVertices[3] = new VertexPositionColorTexture(new Vector3(1920, 1080, 0), Color.White, new Vector2(1, 1));
                EmissiveVertices[4] = new VertexPositionColorTexture(new Vector3(0, 1080, 0), Color.White, new Vector2(0, 1));
                EmissiveVertices[5] = new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0));
                #endregion

                #region Loading audio

                LoadGameSounds();


                #endregion

                #region Loading tower

                Tower.LoadContent(Content);
                Invader.Tower = Tower;

                //Tower.DrawDepth = Tower.DestinationRectangle.Bottom / 1080f;
                //AddDrawable(Tower);

                #endregion                

                #region Loading sprites
                LoadInvaderSprites();
                LoadTurretSprites();
                LoadTrapSprites();
                LoadProjectileSprites();
                LoadWeatherSprites();

                ExplosionDecal1 = Content.Load<Texture2D>("Decals/ExplosionDecal1");
                BloodDecal1 = Content.Load<Texture2D>("Decals/BloodDecal1");
                GlueDecal1 = Content.Load<Texture2D>("Decals/GlueDecal1");

                TerrainShrub1 = Content.Load<Texture2D>("Terrain/Shrub");
                GrassBlade1 = Content.Load<Texture2D>("GrassBlade1");

                PowerupDeliveryPod = Content.Load<Texture2D>("PowerupDeliveryPod");
                PowerupDeliveryFin = Content.Load<Texture2D>("PowerupDeliveryFin");

                JetEngineSprite = Content.Load<Texture2D>("JetSprite");
                #endregion

                #region Loading shaders
                ParticleEffect = Content.Load<Effect>("Shaders/ParticleEffect");
                ParticleEffect.Parameters["Projection"].SetValue(Projection);

                HealthBarEffect = Content.Load<Effect>("Shaders/HealthBarEffect");
                HealthBarEffect.Parameters["MatrixTransform"].SetValue(Projection);
                HealthBarSprite = Content.Load<Texture2D>("CircularBar");

                DepthEffect = Content.Load<Effect>("Shaders/Depth");
                BlurEffect = Content.Load<Effect>("Shaders/Blur");
                CrepEffect = Content.Load<Effect>("Shaders/Crepuscular");
                LightCombined = Content.Load<Effect>("Shaders/LightCombined");
                LightEffect = Content.Load<Effect>("Shaders/LightEffect");

                DepthEffect.Parameters["Projection"].SetValue(Projection);
                DepthEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(0, 0, 0)));

                CrepEffect.Parameters["Projection"].SetValue(Projection);
                BlurEffect.Parameters["Projection"].SetValue(Projection);
                #endregion

                #region Loading particles
                LightningShellCasing = Content.Load<Texture2D>("Particles/LightningTurretShell");
                BigShellCasing = Content.Load<Texture2D>("Particles/MachineShell");
                SmallShellingCasing = Content.Load<Texture2D>("Particles/SmallShellCasing");
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
                HitEffectParticle = Content.Load<Texture2D>("Particles/HitEffectParticle");

                BOOMParticle = Content.Load<Texture2D>("Particles/BOOM");
                SNAPParticle = Content.Load<Texture2D>("Particles/SNAP");
                PINGParticle = Content.Load<Texture2D>("Particles/PING");
                WHAMParticle = Content.Load<Texture2D>("Particles/WHAM");
                BAMParticle = Content.Load<Texture2D>("Particles/BAM");
                FWOOMParticle = Content.Load<Texture2D>("Particles/FWOOM");
                ToonBloodDrip1 = Content.Load<Texture2D>("Particles/ToonBloodDrip1");
                ToonGlueDrip1 = Content.Load<Texture2D>("Particles/ToonGlueDrip1");
                SPLATParticle = Content.Load<Texture2D>("Particles/SPLAT");

                ToonSmoke1 = Content.Load<Texture2D>("Particles/ToonSmoke/ToonSmoke1");
                ToonSmoke2 = Content.Load<Texture2D>("Particles/ToonSmoke/ToonSmoke2");
                ToonSmoke3 = Content.Load<Texture2D>("Particles/ToonSmoke/ToonSmoke3");
                ToonSmoke4 = Content.Load<Texture2D>("Particles/ToonSmoke/ToonSmoke4");
                ToonSmoke5 = Content.Load<Texture2D>("Particles/ToonSmoke/ToonSmoke5");
                ToonSmoke6 = Content.Load<Texture2D>("Particles/ToonSmoke/ToonSmoke6");
                ToonSmoke7 = Content.Load<Texture2D>("Particles/ToonSmoke/ToonSmoke7");

                ToonDust1 = Content.Load<Texture2D>("Particles/ToonDust/ToonDust1");
                ToonDust2 = Content.Load<Texture2D>("Particles/ToonDust/ToonDust2");
                ToonDust3 = Content.Load<Texture2D>("Particles/ToonDust/ToonDust3");
                ToonDust4 = Content.Load<Texture2D>("Particles/ToonDust/ToonDust4");
                ToonDust5 = Content.Load<Texture2D>("Particles/ToonDust/ToonDust5");
                ToonDust6 = Content.Load<Texture2D>("Particles/ToonDust/ToonDust6");
                ToonDust7 = Content.Load<Texture2D>("Particles/ToonDust/ToonDust7");
                #endregion
                
                #region List Creating Code
                //This code just creates the lists for the buttons and traps with the right number of possible slots
                //TrapList = new List<Trap>();
                TrapList = new ObservableCollection<Trap>();

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

                Invader.TrapList = TrapList;
                Invader.InvaderList = InvaderList;
                Invader.EmitterList = YSortedEmitterList;
                Invader.DrawableList = DrawableList;
                Invader.LightList = LightList;
                #endregion


                #region Setting up the buttons
                TowerButtonList = new List<Button>();
                SpecialAbilitiesButtonList = new List<Button>();

                for (int i = 0; i < TowerButtons; i++)
                {
                    TowerButtonList.Add(new Button(TurretSlotButtonSprite, new Vector2(40 + Tower.DestinationRectangle.Width - 32, 500 + ((38 + 90) * i) - 32)));
                    TowerButtonList[i].Initialize(OnButtonClick);
                }


                //Not clearing these lists causes a Disposed Object error to show up for some reason
                //Don't forget to clear the lists between levels or even when moving from menu back to game
                ClearTurretSelect();
                ClearSelected();
                VerletShells.Clear();
                UIWeaponInfoList.Clear();
                CooldownButtonList.Clear();

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
                                                            GetNewTurret((TurretType)CurrentProfile.Buttons[i].CurrentTurret, 0), null);
                            button.ResourceCost = TurretCost(CurrentProfile.Buttons[i].CurrentTurret.Value);
                        }

                        if (CurrentProfile.Buttons[i].CurrentTrap != null)
                        {
                            uiWeaponInfo = new UIWeaponInfoTip(new Vector2(button.CurrentPosition.X, button.CurrentPosition.Y - 32),
                                                              null, GetNewTrap((TrapType)CurrentProfile.Buttons[i].CurrentTrap, Vector2.Zero));
                            button.ResourceCost = TrapCost(CurrentProfile.Buttons[i].CurrentTrap.Value);
                        }


                        uiWeaponInfo.CurrencyIcon = CurrencyIcon;
                        uiWeaponInfo.PowerUnitIcon = PowerUnitIcon;
                        uiWeaponInfo.RobotoBold40_2 = RobotoBold40_2;
                        uiWeaponInfo.RobotoRegular20_0 = RobotoRegular20_0;
                        uiWeaponInfo.RobotoRegular20_2 = RobotoRegular20_2;
                        uiWeaponInfo.RobotoItalic20_0 = RobotoItalic20_0;

                        //uiWeaponInfo.LoadContent(Content);
                        UIWeaponInfoList.Add(uiWeaponInfo);
                    }
                    else
                    {
                        UIWeaponInfoList.Add(null);
                    }

                    CooldownButtonList.Add(button);
                }

                SpecialAbilitiesButtonList.Add(new Button(DiamondButtonSprite, new Vector2(1450, 1080 - 120)));
                SpecialAbilitiesButtonList.Add(new Button(DiamondButtonSprite, new Vector2(1450 + 64 + 4, 1080 - 120)));
                SpecialAbilitiesButtonList.Add(new Button(DiamondButtonSprite, new Vector2(1450 + 32 + 2, 1080 - 120 - 32 - 2)));
                SpecialAbilitiesButtonList.Add(new Button(DiamondButtonSprite, new Vector2(1450 + 32 + 2, 1080 - 120 + 32 + 2)));

                foreach (Button button in SpecialAbilitiesButtonList)
                {
                    button.Initialize(OnButtonClick);
                }

                #endregion

                RopeTexture1 = Content.Load<Texture2D>("RopeTexture");

                MysteryEmployerTexture = Content.Load<Texture2D>("MysteryEmployer");

                MaxWaves = CurrentLevel.WaveList.Count;
                CurrentWaveIndex = 0;

                ShieldBoundingSphere = Content.Load<Texture2D>("ShieldBoundingBox");

                #region Loading UI

                //StartWaveButton = new Button(ButtonRightSprite, new Vector2(1920 - (ButtonRightSprite.Width / 3), 200), null, null, null, "start waves", RobotoRegular20_2, "Right");
                //StartWaveButton.Initialize(OnButtonClick);
                
                HealthBar = new UISlopedBar(new Vector2(1920 / 2 - 810 / 2, 980), new Vector2(800 + 15, 15), Color.Lerp(Color.DarkRed, Color.LightGray, 0.2f), false, 15, 0);
                ShieldBar = new UISlopedBar(new Vector2(1920 / 2 - 810 / 2 + 5, 970), new Vector2(810 - 5 + 15, 10), Color.Lerp(Color.White, Color.LightGray, 0.2f), true, 0, 10);


                #endregion

                SolidList.Add(new Solid(WhiteBlock, new Vector2(Tower.DestinationRectangle.Left, Tower.DestinationRectangle.Bottom - 86), new Vector2(168, 86)));

                ResourceCounter = new ResourceCounter();
                ResourceCounter.Font = DefaultFont;
                ResourceCounter.ResourceIcon = CurrencyIcon;
                
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

                if (NumberChangeList.Count > 0)
                    NumberChangeList.Clear();

                if (DecalList.Count > 0)
                    DecalList.Clear();

                if (DrawableList.Count > 0)
                    DrawableList.Clear();

                if (RopeList.Count > 0)
                    RopeList.Clear();

                TurretSpriteDictionary.Clear();
                TrapAnimationsDictionary.Clear();
                InvaderAnimationsDictionary.Clear();
                LightList.Clear();
                CrepLightList.Clear();

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

            BatteringRamDrop1 = Content.Load<SoundEffect>("Sounds/BatteringRamDrop1");

        }

        private void LoadInvaderSprites()
        {
            IceBlock = Content.Load<Texture2D>("IceBlock");
            Shadow = Content.Load<Texture2D>("Shadow");
            FearEmotionIcon = Content.Load<Texture2D>("Invaders/EmotionIcons/FearEmotionIcon");

            foreach (InvaderType invaderType in Enum.GetValues(typeof(InvaderType)))
            {
                string entryName = invaderType.ToString() + "InvaderAnimations";
                List<InvaderAnimation> AnimationsList = new List<InvaderAnimation>();

                InvaderAnimationsDictionary.Add(entryName, null);

                switch (invaderType)
                {
                    #region Soldier
                    case InvaderType.Soldier:
                        {
                            AnimationsList = new List<InvaderAnimation>()
                            {
                                new InvaderAnimation() 
                                { 
                                    CurrentInvaderState = AnimationState_Invader.Walk,
                                    Texture = Content.Load<Texture2D>("Invaders/Soldier/SoldierWalk"),
                                    AnimationType = AnimationType.Normal,
                                    Animated = true,
                                    CurrentFrame = 0,
                                    FrameDelay = 30,
                                    Looping = true,
                                    TotalFrames = 24
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
                                    FrameDelay = 25,
                                    Looping = true,
                                    TotalFrames = 30                    
                                }
                            };
                        }
                        break;
                    #endregion

                    #region JumpMan
                    case InvaderType.JumpMan:
                        {
                            AnimationsList = new List<InvaderAnimation>()
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
                        }
                        break; 
                    #endregion

                    #region RifleMan
                    case InvaderType.RifleMan:
                        {
                            AnimationsList = new List<InvaderAnimation>()
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
                        }
                        break;
                    #endregion

                    #region StationaryCannon
                    case InvaderType.StationaryCannon:
                        {
                            AnimationsList = new List<InvaderAnimation>()
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
                        }
                        break;
                    #endregion

                    #region HarpoonCannon
                    case InvaderType.HarpoonCannon:
                        {
                            AnimationsList = new List<InvaderAnimation>()
                            {
                                new InvaderAnimation() 
                                { 
                                    CurrentInvaderState = AnimationState_Invader.Walk,
                                    Texture = Content.Load<Texture2D>("Invaders/HarpoonCannon/HarpoonCannonBase"),
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
                                    Texture = Content.Load<Texture2D>("Invaders/HarpoonCannon/HarpoonCannonBase"),
                                    AnimationType = AnimationType.Regular,
                                    Animated = true,
                                    CurrentFrame = 0,
                                    FrameDelay = 150,
                                    Looping = true,
                                    TotalFrames = 1
                                },
                            };

                            HarpoonCannonBarrelAnimation = new InvaderAnimation()
                            {
                                Texture = Content.Load<Texture2D>("Invaders/HarpoonCannon/HarpoonCannonBarrel"),
                                TotalFrames = 1,
                                CurrentFrame = 0,
                                AnimationType = AnimationType.Regular,
                                Animated = true,
                                Looping = false
                            };

                            HarpoonCannonBarrelAnimation.GetFrameSize();
                        }
                        break;
                    #endregion

                    #region HealDrone
                    case InvaderType.HealDrone:
                        {
                            AnimationsList = new List<InvaderAnimation>()
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
                        }
                        break;
                    #endregion

                    #region ShieldGenerator
                    case InvaderType.ShieldGenerator:
                        {
                            AnimationsList = new List<InvaderAnimation>()
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
                        }
                        break;
                    #endregion

                    #region BatteringRam
                    case InvaderType.BatteringRam:
                        {
                            AnimationsList = new List<InvaderAnimation>()
                            {
                                new InvaderAnimation() 
                                { 
                                    CurrentInvaderState = AnimationState_Invader.Walk,
                                    Texture = Content.Load<Texture2D>("Invaders/BatteringRam/BatteringRam"),
                                    AnimationType = AnimationType.Regular,
                                    Animated = false,
                                    CurrentFrame = 0,
                                    FrameDelay = 150,
                                    Looping = false,
                                    TotalFrames = 1                    
                                },

                                new InvaderAnimation() 
                                { 
                                    CurrentInvaderState = AnimationState_Invader.Stand,
                                    Texture = Content.Load<Texture2D>("Invaders/BatteringRam/BatteringRam"),
                                    AnimationType = AnimationType.Regular,
                                    Animated = false,
                                    CurrentFrame = 0,
                                    FrameDelay = 150,
                                    Looping = false,
                                    TotalFrames = 1                    
                                },
                            };
                        }
                        break;
                    #endregion

                    #region DropShip
                    case InvaderType.DropShip:
                        {
                            AnimationsList = new List<InvaderAnimation>()
                            {
                                new InvaderAnimation()
                                {
                                    CurrentInvaderState = AnimationState_Invader.Walk,
                                    Texture = Content.Load<Texture2D>("Invaders/DropShip/DropShipTexture"),
                                    Animated = false,
                                    CurrentFrame = 0,
                                    FrameDelay = 150,
                                    Looping = false,
                                    TotalFrames = 1
                                },

                                new InvaderAnimation()
                                {
                                    CurrentInvaderState = AnimationState_Invader.Stand,
                                    Texture = Content.Load<Texture2D>("Invaders/DropShip/DropShipTexture"),
                                    Animated = false,
                                    CurrentFrame = 0,
                                    FrameDelay = 150,
                                    Looping = false,
                                    TotalFrames = 1
                                }
                            };

                            DropShipDoorTexture = Content.Load<Texture2D>("Invaders/Dropship/DropShipDoor");   
                        }
                        break;
                    #endregion

                    #region Crate
                    case InvaderType.Crate:
                        {
                            AnimationsList = new List<InvaderAnimation>()
                            {
                                new InvaderAnimation()
                                {
                                    CurrentInvaderState = AnimationState_Invader.Stand,
                                    Texture = WhiteBlock,
                                    AnimationType = AnimationType.Regular,
                                    Animated = false,
                                    CurrentFrame = 0,
                                    FrameDelay = 100,
                                    Looping = false,
                                    TotalFrames = 1
                                }
                            };
                        }
                        break;
                    #endregion

                    #region GunShip
                    case InvaderType.GunShip:
                        {
                            AnimationsList = new List<InvaderAnimation>()
                            {
                                new InvaderAnimation()
                                {
                                    CurrentInvaderState = AnimationState_Invader.Walk,
                                    Texture = Content.Load<Texture2D>("Invaders/GunShip/GunShipTexture"),
                                    Animated = false,
                                    CurrentFrame = 0,
                                    FrameDelay = 150,
                                    Looping = false,
                                    TotalFrames = 1
                                },

                                new InvaderAnimation()
                                {
                                    CurrentInvaderState = AnimationState_Invader.Stand,
                                    Texture = Content.Load<Texture2D>("Invaders/GunShip/GunShipTexture"),
                                    Animated = false,
                                    CurrentFrame = 0,
                                    FrameDelay = 150,
                                    Looping = false,
                                    TotalFrames = 1
                                }
                            };
                        }
                        break;
                    #endregion
                }

                InvaderAnimationsDictionary[entryName] = AnimationsList;

                if (InvaderAnimationsDictionary[entryName] != null)
                {
                    foreach (InvaderAnimation invaderAnimation in InvaderAnimationsDictionary[entryName])
                    {
                        invaderAnimation.GetFrameSize();
                    }
                }
            }
        }

        private void LoadTrapSprites()
        {
            //NEW_TRAP C **trap animations loaded here**
            foreach (TrapType trapType in Enum.GetValues(typeof(TrapType)))
            {
                string entryName = trapType.ToString() + "TrapAnimations";
                List<TrapAnimation> AnimationsList = new List<TrapAnimation>();
                
                TrapAnimationsDictionary.Add(entryName, null);
                
                switch (trapType)
                {
                    #region Wall
                    case TrapType.Wall:
                        {
                            AnimationsList = new List<TrapAnimation>()
                            {
                                new TrapAnimation()
                                {
                                    CurrentTrapState = TrapAnimationState.Untriggered,
                                    Texture = Content.Load<Texture2D>("Traps/Wall/WallTrap"),
                                    Animated = false,
                                    CurrentFrame = 0,
                                    TotalFrames = 1,
                    
                                    AnimationType = AnimationType.Normal
                                }
                            };
                        }
                        break;
                    #endregion
                        
                    #region FlameThrower
                    case TrapType.FlameThrower:
                        {
                            AnimationsList = new List<TrapAnimation>()
                            {
                                new TrapAnimation()
                                {
                                    CurrentTrapState = TrapAnimationState.Untriggered,
                                    Texture = Content.Load<Texture2D>("Traps/FlameThrower/FlameThrowerTrapUntriggered"),
                                    Animated = false,
                                    CurrentFrame = 0,
                                    TotalFrames = 1,
                    
                                    AnimationType = AnimationType.Regular
                                },

                                new TrapAnimation()
                                {
                                    CurrentTrapState = TrapAnimationState.Active,
                                    Texture = Content.Load<Texture2D>("Traps/FlameThrower/FlameThrowerTrapActive"),
                                    Animated = false,
                                    CurrentFrame = 0,
                                    TotalFrames = 1,
                    
                                    AnimationType = AnimationType.Regular
                                },

                                new TrapAnimation()
                                {
                                    CurrentTrapState = TrapAnimationState.Triggering,
                                    Texture = Content.Load<Texture2D>("Traps/FlameThrower/FlameThrowerTrapTriggering"),
                                    Animated = true,
                                    CurrentFrame = 0,
                                    TotalFrames = 17,
                    
                                    AnimationType = AnimationType.Regular
                                },

                                new TrapAnimation()
                                {
                                    CurrentTrapState = TrapAnimationState.Resetting,
                                    Texture = Content.Load<Texture2D>("Traps/FlameThrower/FlameThrowerTrapResetting"),
                                    Animated = true,
                                    CurrentFrame = 0,
                                    TotalFrames = 17,

                                    AnimationType = AnimationType.Regular
                                }
                            };
                        }
                        break; 
                    #endregion

                    #region Spikes
                    case TrapType.Spikes:
                        {
                            AnimationsList = new List<TrapAnimation>()
                            {
                                new TrapAnimation()
                                {
                                    CurrentTrapState = TrapAnimationState.Untriggered,
                                    Texture = Content.Load<Texture2D>("Traps/Spikes/SpikesTrapUntriggered"),
                                    Animated = false,
                                    CurrentFrame = 0, 
                                    TotalFrames = 1,

                                    AnimationType = AnimationType.Regular
                                },

                                new TrapAnimation()
                                {
                                    CurrentTrapState = TrapAnimationState.Active,
                                    Texture = Content.Load<Texture2D>("Traps/Spikes/SpikesTrapActive"),
                                    Animated = false,
                                    CurrentFrame = 0,
                                    TotalFrames = 1,
                    
                                    AnimationType = AnimationType.Regular
                                },

                                new TrapAnimation()
                                {
                                    CurrentTrapState = TrapAnimationState.Triggering,
                                    Texture = Content.Load<Texture2D>("Traps/Spikes/SpikesTrapTriggering"),
                                    Animated = true,
                                    CurrentFrame = 0,
                                    TotalFrames = 6,
                    
                                    AnimationType = AnimationType.Regular
                                },

                                new TrapAnimation()
                                {
                                    CurrentTrapState = TrapAnimationState.Resetting,
                                    Texture = Content.Load<Texture2D>("Traps/Spikes/SpikesTrapResetting"),
                                    Animated = true,
                                    CurrentFrame = 0,
                                    TotalFrames = 6,

                                    AnimationType = AnimationType.Regular
                                }
                            };
                        }
                        break;
                    #endregion

                    #region Fire
                    case TrapType.Fire:
                        {
                            AnimationsList = new List<TrapAnimation>()
                            {
                                new TrapAnimation()
                                {
                                    CurrentTrapState = TrapAnimationState.Untriggered,
                                    Texture = Content.Load<Texture2D>("Traps/FireTrap"),
                                    Animated = false,
                                    CurrentFrame = 0,
                                    TotalFrames = 1,
                                    AnimationType = AnimationType.Regular
                                }
                            };
                        }
                        break; 
                    #endregion

                    #region Catapult
                    case TrapType.Catapult:
                        {
                            AnimationsList = new List<TrapAnimation>()
                            {
                                new TrapAnimation()
                                {
                                    CurrentTrapState = TrapAnimationState.Untriggered,
                                    Texture = Content.Load<Texture2D>("Traps/CatapultTrap"),
                                    Animated = false,
                                    CurrentFrame = 0,
                                    TotalFrames = 1,
                                    AnimationType = AnimationType.Regular
                                }
                            };
                        }
                        break; 
                    #endregion

                    #region LandMine
                    case TrapType.LandMine:
                        {
                            AnimationsList = new List<TrapAnimation>()
                            {
                                new TrapAnimation()
                                {
                                    CurrentTrapState = TrapAnimationState.Untriggered,
                                    Texture = Content.Load<Texture2D>("Traps/LandMine"),
                                    Animated = false,
                                    CurrentFrame = 0,
                                    TotalFrames = 1,
                                    AnimationType = AnimationType.Regular
                                }
                            };
                        }
                        break; 
                    #endregion

                    #region Barrel
                    case TrapType.Barrel:
                        {
                            AnimationsList = new List<TrapAnimation>()
                            {
                                new TrapAnimation()
                                {
                                    CurrentTrapState = TrapAnimationState.Untriggered,
                                    Texture = Content.Load<Texture2D>("Traps/Barrel"),
                                    Animated = false,
                                    CurrentFrame = 0,
                                    TotalFrames = 1,
                                    AnimationType = AnimationType.Regular
                                }
                            };
                        }
                        break; 
                    #endregion

                    #region Glue
                    case TrapType.Glue:
                        {
                            AnimationsList = new List<TrapAnimation>()
                            {
                                new TrapAnimation()
                                {
                                    CurrentTrapState = TrapAnimationState.Untriggered,
                                    Texture = Content.Load<Texture2D>("Traps/GlueTrap"),
                                    Animated = false,
                                    CurrentFrame = 0,
                                    TotalFrames = 1,
                                    AnimationType = AnimationType.Regular
                                }
                            };
                        }
                        break;
                    #endregion
                }

                TrapAnimationsDictionary[entryName] = AnimationsList;

                if (TrapAnimationsDictionary[entryName] != null)
                {
                    foreach (TrapAnimation trapAnimation in TrapAnimationsDictionary[entryName])
                    {
                        trapAnimation.GetFrameSize();
                    }
                }
            }
        }

        private void LoadProjectileSprites()
        {
            //NEW_HEAVYPROJECTILE C **heavy projectile sprites loaded here**
            foreach (HeavyProjectileType heavyProjectileType in Enum.GetValues(typeof(HeavyProjectileType)))
            {
                string projectileTextureName = heavyProjectileType.ToString()+"ProjectileSprite";
                var thing = this.GetType().GetField(projectileTextureName);
                thing.SetValue(this, Content.Load<Texture2D>("Projectiles/" + heavyProjectileType.ToString() + "Projectile"));
            }
        }

        private void LoadTurretSprites()
        {
            //NEW_TURRET C **turret animations loaded here**
            
            TurretSelectBox = Content.Load<Texture2D>("SelectBox");

            MachineGunTurretBarrelGib = Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineGunTurretBarrelGib");

            foreach (TurretType turretType in Enum.GetValues(typeof(TurretType)))
            {
                string TurretBaseName = "Turrets\\" + turretType.ToString() + "Turret\\" + turretType.ToString() + "TurretBase";
                string TurretBarrelName = "Turrets\\" + turretType.ToString() + "Turret\\" + turretType.ToString() + "TurretBarrel"; ;

                string TurretBaseTextureName = turretType.ToString() + "TurretBase";
                string TurretBarrelTextureName = turretType.ToString() + "TurretBarrel";
                
                string pat = Directory.GetCurrentDirectory() + "\\Content\\" + TurretBaseName + ".xnb";

                if (File.Exists(pat))
                {
                    TurretSpriteDictionary.Add(TurretBaseTextureName, Content.Load<Texture2D>(TurretBaseName));
                    TurretSpriteDictionary.Add(TurretBarrelTextureName, Content.Load<Texture2D>(TurretBarrelName));
                }
                else
                {
                    TurretSpriteDictionary.Add(TurretBaseTextureName, Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineGunTurretBase"));
                    TurretSpriteDictionary.Add(TurretBarrelTextureName, Content.Load<Texture2D>("Turrets/MachineGunTurret/MachineGunTurretBarrel"));
                }
            }
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
        }

        private void LoadFonts()
        {
            DefaultFont = SecondaryContent.Load<SpriteFont>("Fonts/RobotoRegular20_2");
            DialogueFont = SecondaryContent.Load<SpriteFont>("Fonts/RobotoRegular20_2");
            //ButtonFont = SecondaryContent.Load<SpriteFont>("Fonts/RobotoRegular20_2");

            RobotoBold20_0_Outline = SecondaryContent.Load<SpriteFont>("Fonts/RobotoBold20_0_Outline");
            RobotoBold20_2_Outline = SecondaryContent.Load<SpriteFont>("Fonts/RobotoBold20_2_Outline");

            TooltipFont = SecondaryContent.Load<SpriteFont>("Fonts/SpriteFont1");
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
            #region Turret Icons
            foreach (TurretType turretType in Enum.GetValues(typeof(TurretType)))
            {
                string TurretIconName = "Icons\\TurretIcons\\" + turretType.ToString() + "TurretIcon";

                string TurretIconTextureName = turretType.ToString() + "TurretIcon";

                string pat = Directory.GetCurrentDirectory() + "\\Content\\" + TurretIconName + ".xnb";

                if (File.Exists(pat))
                {
                    TurretIconDictionary.Add(TurretIconTextureName, SecondaryContent.Load<Texture2D>(TurretIconName));
                }
                else
                {
                    TurretIconDictionary.Add(TurretIconTextureName, SecondaryContent.Load<Texture2D>("Icons/TurretIcons/MachineGunTurretIcon"));                    
                }
            }
            #endregion

            #region Trap Icons
            foreach (TrapType TrapType in Enum.GetValues(typeof(TrapType)))
            {
                string TrapIconName = "Icons\\TrapIcons\\" + TrapType.ToString() + "TrapIcon";

                string TrapIconTextureName = TrapType.ToString() + "TrapIcon";

                string pat = Directory.GetCurrentDirectory() + "\\Content\\" + TrapIconName + ".xnb";

                if (File.Exists(pat))
                {
                    TrapIconDictionary.Add(TrapIconTextureName, SecondaryContent.Load<Texture2D>(TrapIconName));
                }
                else
                {
                    TrapIconDictionary.Add(TrapIconTextureName, SecondaryContent.Load<Texture2D>("Icons/TrapIcons/FireTrapIcon"));
                }
            }
            #endregion     

            PowerUnitIcon = SecondaryContent.Load<Texture2D>("Icons/PowerUnitIcon");
            CurrencyIcon = SecondaryContent.Load<Texture2D>("Icons/CurrencyIcon");
            LockIcon = SecondaryContent.Load<Texture2D>("Icons/LockIcon");
            HealthIcon = SecondaryContent.Load<Texture2D>("Icons/HealthIcon");
            OverHeatIcon = SecondaryContent.Load<Texture2D>("Icons/OverHeatIcon");
            RightClickIcon = SecondaryContent.Load<Texture2D>("Icons/MouseRightClickIcon");
        }

        private void LoadCursorSprites()
        {
            DefaultCursor = SecondaryContent.Load<Texture2D>("Cursors/DefaultCursor");
            CrosshairCursor = SecondaryContent.Load<Texture2D>("Cursors/Crosshair");

            #region Turret Cursors
            foreach (TurretType turretType in Enum.GetValues(typeof(TurretType)))
            {
                string TurretIconName = "Icons/TurretIcons/" + turretType.ToString() + "TurretIcon";
                string CursorTextureName = turretType.ToString() + "TurretCursor";

                var thing = this.GetType().GetField(CursorTextureName);
                thing.SetValue(this, SecondaryContent.Load<Texture2D>(TurretIconName));
            }
            #endregion

            #region Trap Cursors
            foreach (TrapType trapType in Enum.GetValues(typeof(TrapType)))
            {
                string TrapIconName = "Icons/TrapIcons/" + trapType.ToString() + "TrapIcon";
                string CursorTextureName = trapType.ToString() + "TrapCursor";

                var thing = this.GetType().GetField(CursorTextureName);
                thing.SetValue(this, SecondaryContent.Load<Texture2D>(TrapIconName));
            }
            #endregion
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
                //if (GameState == GameState.Loading)
                //{
                //    MenuBackground1.Draw(spriteBatch);
                //}

                switch (GameState)
                {
                    #region Loading Screen
                    case GameState.Loading:
                        {
                            if (AllLoaded == false)
                                LoadingAnimation.Draw(spriteBatch);

                            //spriteBatch.DrawString(RobotoBold40_2, AllLoaded.ToString(), new Vector2(100, 100), Color.White);
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

                            if (LoadoutMenuDialogue != null)
                            {
                                LoadoutMenuDialogue.DialogueBox.Draw(spriteBatch);
                            }
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
                            spriteBatch.DrawString(RobotoRegular40_2, "unlocked", new Vector2(1920 / 2, 280), Color.White, 0, RobotoBold40_2.MeasureString("unlocked") / 2, 1, SpriteEffects.None, 0);
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
                
                #region Emissive
                #region Draw to Emissive Map
                GraphicsDevice.SetRenderTarget(EmissiveMap);
                GraphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, RasterizerState.CullNone);

                #region Draw number changes
                //Draw the damage numbers for when the invaders are damaged
                foreach (NumberChange numChange in NumberChangeList)
                {
                    numChange.Draw(spriteBatch);
                }
                #endregion


                foreach (ExplosionEffect effect in ExplosionEffectList)
                {
                    effect.Draw(spriteBatch);
                }

                //foreach (BulletTrail trail in TrailList)
                //{
                //    trail.Draw(spriteBatch);
                //}

                foreach (Particle coin in CoinList)
                {
                    coin.Draw(spriteBatch);
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

                foreach (Drawable drawable in DrawableList)
                {
                    if (drawable.GetType() == typeof(ToonLightning))
                    {
                        drawable.Draw(GraphicsDevice, BasicEffect2);
                    }

                    if (drawable.GetType() == typeof(JetEngine))
                    {
                        drawable.Draw(GraphicsDevice, BasicEffect);
                        (drawable as JetEngine).JetEmitter.Draw(GraphicsDevice, ParticleEffect);
                    }


                    if (drawable.GetType() == typeof(Emitter) && drawable.Emissive == true)
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
                }

                //spriteBatch.Draw(UIRenderTarget, UIRenderTarget.Bounds, Color.White);
                spriteBatch.End();
                #endregion

                #region Blur
                GraphicsDevice.SetRenderTarget(BlurMap);
                GraphicsDevice.Clear(Color.Transparent);

                BlurEffect.Parameters["InputTexture"].SetValue(EmissiveMap);
                BlurEffect.CurrentTechnique = BlurEffect.Techniques["Technique1"];

                foreach (EffectPass pass in BlurEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, EmissiveVertices, 0, 2);
                }
                #endregion
                #endregion

                #region Draw to ColorMap
                GraphicsDevice.SetRenderTarget(ColorMap);
                GraphicsDevice.Clear(Color.Transparent);

                #region Draw the background type stuff - ground, tower, sky, snow etc.
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

                SkyBackground.Draw(spriteBatch);
                Ground.Draw(spriteBatch);


                foreach (Decal decal in DecalList)
                {
                    decal.Draw(spriteBatch);
                }

                foreach (ExplosionEffect effect in ExplosionEffectList)
                {
                    effect.Draw(spriteBatch);
                }

                foreach (Trap trap in TrapList.Where(Trap => Trap.AmbientShadowTexture != null))
                {
                    spriteBatch.Draw(trap.AmbientShadowTexture, new Vector2(trap.DestinationRectangle.Left, trap.DestinationRectangle.Bottom - trap.ZDepth), Color.White);

                    if (trap.DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)))
                    {
                        spriteBatch.Draw(WhiteBlock, BoundingBoxToRect(trap.CollisionBox), Color.DarkGray);
                    }
                }

                Tower.Draw(spriteBatch);

                //foreach (Rope rope in RopeList)
                //{
                //    rope.Draw(spriteBatch);
                //}

                spriteBatch.End();
                #endregion

                foreach (SmokeTrail trail in SmokeTrailList)
                {
                    trail.DrawVector(GraphicsDevice, SmokeBasicEffect);
                }

                #region Draw things sorted according to their Y value - To create depth illusion
                //DrawableList = DrawableList.OrderBy(Drawable => Drawable.DrawDepth).ToList();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, null, null);

                foreach (Drawable drawable in DrawableList)
                {
                    if (drawable.GetType().BaseType == typeof(HeavyProjectile))
                    {
                        drawable.Draw(GraphicsDevice, ProjectileBasicEffect, ShadowBlurEffect, ParticleEffect);
                    }

                    if (drawable.GetType() == typeof(JetEngine))
                    {
                        drawable.Draw(GraphicsDevice, BasicEffect);
                        (drawable as JetEngine).JetEmitter.Draw(GraphicsDevice, ParticleEffect);
                    }

                    if (drawable.GetType() == typeof(Shield))
                    {
                        drawable.Draw(GraphicsDevice, BasicEffect);
                    }

                    if (drawable.GetType() == typeof(Rope))
                    {
                        drawable.Draw(GraphicsDevice, BasicEffect);
                    }

                    if (drawable.GetType() == typeof(ShellCasing))
                    {
                        drawable.Draw(GraphicsDevice, BasicEffect);
                    }

                    if (drawable.GetType() == typeof(ToonLightning))
                    {
                        drawable.Draw(GraphicsDevice, BasicEffect2);
                    }

                    if (drawable.GetType() == typeof(StickyMine))
                    {
                        drawable.Draw(GraphicsDevice, BasicEffect);
                    }

                    if (drawable.GetType().BaseType == typeof(Invader) ||
                        drawable.GetType().BaseType == typeof(LightRangedInvader) ||
                        drawable.GetType().BaseType == typeof(HeavyRangedInvader) ||
                        drawable.GetType().BaseType == typeof(Trap))
                    {
                        if (drawable.BlendState != BlendState.AlphaBlend)
                        {
                            GraphicsDevice.BlendState = drawable.BlendState;
                            drawable.Draw(GraphicsDevice, BasicEffect, ShadowBlurEffect);
                            GraphicsDevice.BlendState = BlendState.AlphaBlend;
                        }
                        else
                        {
                            drawable.Draw(GraphicsDevice, BasicEffect, ShadowBlurEffect);
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

                    if (drawable.GetType() == typeof(Rope))
                    {
                        drawable.Draw(spriteBatch);
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

                #region Draw with additive blending - Should actually be drawn with bloom
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
                    if (heavyProjectile.HeavyProjectileType == HeavyProjectileType.Fel)
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
                //spriteBatch.End();
                #endregion

                #region Draw to NormalMap
                GraphicsDevice.SetRenderTarget(NormalMap);
                GraphicsDevice.Clear(new Color(128, 128, 255));
                spriteBatch.Begin();

                spriteBatch.End();
                #endregion

                #region Draw to SpecMap
                GraphicsDevice.SetRenderTarget(SpecMap);
                GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin();

                spriteBatch.End();
                #endregion

                #region Draw to DepthMap
                GraphicsDevice.SetRenderTarget(DepthMap);
                GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null);
                foreach (Drawable drawable in DrawableList)
                {
                    if (drawable.GetType() != typeof(Emitter))
                        drawable.DrawSpriteDepth(GraphicsDevice, DepthEffect);
                }
                spriteBatch.End();
                #endregion

                #region Draw to LightMap
                GraphicsDevice.SetRenderTarget(LightMap);
                GraphicsDevice.Clear(Color.Transparent);

                foreach (Light light in LightList)
                {
                    if (light.Active == true)
                    {
                        MyShadow(light);

                        GraphicsDevice.SetRenderTarget(LightMap);

                        LightEffect.Parameters["ShadowMap"].SetValue(ShadowMap);

                        LightEffect.Parameters["LightPosition"].SetValue(light.Position);
                        LightEffect.Parameters["LightColor"].SetValue(ColorToVector(light.Color));
                        LightEffect.Parameters["LightPower"].SetValue(light.Power);
                        LightEffect.Parameters["LightSize"].SetValue(light.Size);
                        LightEffect.Parameters["NormalMap"].SetValue(NormalMap);
                        LightEffect.Parameters["ColorMap"].SetValue(ColorMap);
                        LightEffect.Parameters["DepthMap"].SetValue(DepthMap);
                        LightEffect.Parameters["lightDepth"].SetValue(light.Depth);

                        LightEffect.CurrentTechnique = LightEffect.Techniques["DeferredPointLight"];
                        LightEffect.CurrentTechnique.Passes[0].Apply();

                        GraphicsDevice.BlendState = BlendBlack;
                        GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, LightVertices, 0, 2);
                    }
                }

                spriteBatch.Begin();
                spriteBatch.Draw(BlurMap, BlurMap.Bounds, Color.White);
                spriteBatch.End();
                #endregion

                #region Combine Normals, Lighting and Color
                GraphicsDevice.SetRenderTarget(FinalMap);
                GraphicsDevice.Clear(Color.DeepSkyBlue);

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, LightCombined);
                #region Draw the lightmap and color map combined
                LightCombined.CurrentTechnique = LightCombined.Techniques["DeferredCombined2"];
                LightCombined.Parameters["ambient"].SetValue(1f);
                LightCombined.Parameters["lightAmbient"].SetValue(4f);
                LightCombined.Parameters["ambientColor"].SetValue(AmbientLight.ToVector4());

                LightCombined.Parameters["ColorMap"].SetValue(ColorMap);
                LightCombined.Parameters["ShadingMap"].SetValue(LightMap);
                LightCombined.Parameters["NormalMap"].SetValue(NormalMap);

                LightCombined.CurrentTechnique.Passes[0].Apply();

                spriteBatch.Draw(ColorMap, Vector2.Zero, Color.White);
                #endregion
                spriteBatch.End();

                //spriteBatch.Begin();
                //spriteBatch.Draw(EmissiveMap, ColorMap.Bounds, Color.White);
                //spriteBatch.Draw(BlurMap, BlurMap.Bounds, Color.White);
                //foreach (Solid solid in SolidList)
                //{
                //    solid.Draw(spriteBatch, Color.Black);
                //}
                //spriteBatch.End();
                #endregion

                #region Occlusion Map
                GraphicsDevice.SetRenderTarget(OcclusionMap);
                GraphicsDevice.Clear(Color.White);
                spriteBatch.Begin();
                //spriteBatch.Draw(WhiteBlock, CursorPosition, Color.Black);

                foreach (Drawable drawable in DrawableList)
                {
                    if (drawable.GetType() != typeof(Emitter))
                        drawable.DrawSpriteOcclusion(GraphicsDevice, BasicEffect);
                }
                spriteBatch.End();
                #endregion

                #region Crepuscular ColorMap
                
                #endregion

                #region Buffers for Multiple Crepuscular Lights
                GraphicsDevice.SetRenderTarget(Buffer2);
                GraphicsDevice.Clear(Color.Transparent);

                CrepEffect.Parameters["ColorMap"].SetValue(CrepColorMap);
                CrepEffect.Parameters["OccMap"].SetValue(OcclusionMap);

                foreach (CrepuscularLight light in CrepLightList)
                {
                    GraphicsDevice.SetRenderTarget(CrepLightMap);
                    GraphicsDevice.Clear(Color.Transparent);

                    spriteBatch.Begin();
                    //spriteBatch.Draw(CrepuscularLightTexture, new Rectangle((int)(light.Position.X), (int)(light.Position.Y), CrepuscularLightTexture.Width / 3, CrepuscularLightTexture.Height / 3), null,
                    //                 LightList[CrepLightList.IndexOf(light)].Color, 0, new Vector2(CrepuscularLightTexture.Width / 2, CrepuscularLightTexture.Height / 2), SpriteEffects.None, 0);

                    Color colp = new Color(Color.OrangeRed.R, Color.OrangeRed.G, Color.OrangeRed.B, 80);

                    spriteBatch.Draw(CrepuscularLightTexture, new Rectangle((int)(light.Position.X), (int)(light.Position.Y), CrepuscularLightTexture.Width / 3, CrepuscularLightTexture.Height / 3), null,
                                     colp, 0, new Vector2(CrepuscularLightTexture.Width / 2, CrepuscularLightTexture.Height / 2), SpriteEffects.None, 0);


                    spriteBatch.End();

                    #region Buffer1
                    GraphicsDevice.SetRenderTarget(Buffer1);
                    GraphicsDevice.Clear(Color.Transparent);
                    {
                        CrepEffect.Parameters["LightPosition"].SetValue(light.Position / new Vector2(1920, 1080));
                        CrepEffect.Parameters["decay"].SetValue(light.Decay);
                        CrepEffect.Parameters["exposure"].SetValue(light.Exposure);
                        CrepEffect.Parameters["density"].SetValue(light.Density);
                        CrepEffect.Parameters["weight"].SetValue(light.Weight);
                        CrepEffect.Parameters["lightDepth"].SetValue(light.Depth);
                        CrepEffect.Parameters["DepthMap"].SetValue(DepthMap);                            

                        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                        CrepEffect.CurrentTechnique.Passes[0].Apply();

                        spriteBatch.Draw(CrepLightMap, CrepLightMap.Bounds, Color.White);
                        spriteBatch.End();
                    }

                    GraphicsDevice.SetRenderTarget(Buffer2);

                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                    spriteBatch.Draw(Buffer1, Buffer1.Bounds, Color.White);
                    spriteBatch.End();
                    #endregion
                }
                #endregion
                
                #endregion                

                #region Draw UI
                GraphicsDevice.SetRenderTarget(UIRenderTarget);
                GraphicsDevice.Clear(Color.Transparent);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                //spriteBatch.DrawString(DialogueFont, CurrentLevel.LevelDialogue.CurrentText, new Vector2(100, 100), Color.White);
                if (CurrentLevel.LevelDialogue != null)
                {
                    CurrentLevel.LevelDialogue.DialogueBox.Draw(spriteBatch);
                    CurrentLevel.LevelDialogue.TutorialMarker.Draw(spriteBatch);
                }

                #region Draw diagnostics
                if (Diagnostics == true)
                {
                    int yPos = 0;
                    int yInc = 10;

                    foreach (HeavyProjectile projectile in HeavyProjectileList)
                    {
                        spriteBatch.Draw(WhiteBlock, new Rectangle((int)projectile.Ballistics.EndPosition.X, (int)projectile.Ballistics.EndPosition.Y, 8, 8), null, Color.Red, 0, new Vector2(WhiteBlock.Width / 2, WhiteBlock.Height / 2), SpriteEffects.None, 0);
                    }

                    //spriteBatch.DrawString(TooltipFont, CurrentProfile.LevelNumber.ToString(), new Vector2(100, 200), Color.Purple);
                    //spriteBatch.DrawString(TooltipFont, gameTime.ElapsedGameTime.ToString(), new Vector2(1100, 0), Color.Red);
                    //spriteBatch.DrawString(TooltipFont, CurrentSettings.TimesPlayed.ToString(), new Vector2(0, yPos), Tower.Color);

                    spriteBatch.DrawString(TooltipFont, Slow.ToString(), Vector2.Zero, Color.Red);
                    yPos += yInc;

                    //spriteBatch.DrawString(TooltipFont, "Seconds:" + Seconds.ToString(), new Vector2(0, yPos), Color.Lime);
                    //yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "Resources:" + Resources.ToString(), new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "CurrentWaveIndex:" + (CurrentWaveIndex + 1).ToString() + "/" + MaxWaves, new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "ShieldOn:" + Tower.Shield.ShieldOn.ToString(), new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "CurrentShieldTime:" + Tower.Shield.CurrentShieldTime.ToString(), new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "CurrentWaveTime:" + CurrentWaveTime.ToString(), new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "HeavyProjectiles:" + HeavyProjectileList.Count.ToString(), new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "Drawables:" + DrawableList.Count.ToString(), new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "TrailList:" + TrailList.Count.ToString(), new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "Emitter1:" + YSortedEmitterList.Count.ToString(), new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "Invaders:" + InvaderList.Count.ToString(), new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "Particles:" + TotalParticles.ToString(), new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "Lightning:" + LightningList.Count.ToString(), new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "InvaderTime:" + CurrentInvaderTime.ToString(), new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "WaveTime:" + CurrentWaveTime.ToString(), new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "PauseTime:" + CurrentWavePauseTime, new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "WeatherSprites:" + WeatherSpriteList.Count.ToString(), new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "CurrentPowerUnits:" + Tower.CurrentPowerUnits.ToString(), new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    spriteBatch.DrawString(TooltipFont, "Ropes:" + RopeList.Count.ToString(), new Vector2(0, yPos), Color.Lime);
                    yPos += yInc;

                    foreach (Invader invader in InvaderList)
                    {
                        int yPos2 = 0;
                        int yInc2 = 10;

                        spriteBatch.DrawString(TooltipFont, invader.InvaderAnimationState.ToString(), invader.Position, Color.White, 0, Vector2.Zero, 0.85f, SpriteEffects.None, 0);
                        yPos2 += yInc2;

                        spriteBatch.DrawString(TooltipFont, "Micro:" + invader.CurrentMicroBehaviour.ToString(), invader.Position + new Vector2(0, yPos2), Color.White, 0, Vector2.Zero, 0.85f, SpriteEffects.None, 0);
                        yPos2 += yInc2;

                        spriteBatch.DrawString(TooltipFont, "Macro:" + invader.CurrentMacroBehaviour.ToString(), invader.Position + new Vector2(0, yPos2), Color.White, 0, Vector2.Zero, 0.85f, SpriteEffects.None, 0);
                        yPos2 += yInc2;

                        spriteBatch.DrawString(TooltipFont, "Int:" + invader.Intelligence.ToString(), invader.Position + new Vector2(0, yPos2), Color.White);
                        yPos2 += yInc2;

                        spriteBatch.DrawString(TooltipFont, "MaxY:" + invader.DrawDepth.ToString(), invader.Position + new Vector2(0, yPos2), Color.White);
                        yPos2 += yInc2;
                    }

                    foreach (Emitter emitter in YSortedEmitterList)
                    {
                        spriteBatch.DrawString(TooltipFont, "DrawDepth: " + emitter.DrawDepth.ToString(), emitter.Position, Color.White);
                    }

                    foreach (HeavyProjectile heavyProjectile in HeavyProjectileList)
                    {
                        spriteBatch.DrawString(TooltipFont, "Rotation: " + heavyProjectile.CurrentRotation, heavyProjectile.Position, Color.White);
                    }

                    foreach (Turret turret in TurretList.Where(Turret => Turret != null))
                    {
                        spriteBatch.DrawString(TooltipFont, "Rotation: " + MathHelper.ToDegrees(turret.Rotation).ToString(), turret.Position, Color.White);
                        spriteBatch.DrawString(TooltipFont, "TotalDamage: " + turret.TotalDamageDone.ToString(), turret.Position + new Vector2(0, 16), Color.White);

                    }


                    foreach (Rope rope in RopeList)
                    {
                        spriteBatch.DrawString(TooltipFont, "Rope DrawDepth: " + rope.DrawDepth.ToString(), rope.StartPoint, Color.Yellow);
                    }
                    //foreach (HeavyRangedInvader invader in InvaderList.Where(Invader => Invader.InvaderType == InvaderType.StationaryCannon))
                    //{
                    //    spriteBatch.DrawString(RobotoRegular20_0, MathHelper.ToDegrees(invader.CurrentAngle).ToString(), invader.Position, Color.White);
                    //}

                    spriteBatch.DrawString(DefaultFont, (gameTime.ElapsedGameTime.TotalMilliseconds).ToString(), new Vector2(1920 - 50, 0), Color.Red);
                    FPSCounter.Draw(spriteBatch);
                }
                else
                {
                    foreach (Invader invader in InvaderList)
                    {
                        if (invader.ShowDiagnostics == true)
                        {
                            int yPos = 0;
                            int yInc = 10;

                            spriteBatch.DrawString(TooltipFont, invader.InvaderAnimationState.ToString(), invader.Position, Color.White, 0, Vector2.Zero, 0.85f, SpriteEffects.None, 0);
                            yPos += yInc;

                            spriteBatch.DrawString(TooltipFont, "Micro:" + invader.CurrentMicroBehaviour.ToString(), invader.Position + new Vector2(0, yPos), Color.White, 0, Vector2.Zero, 0.85f, SpriteEffects.None, 0);
                            yPos += yInc;

                            spriteBatch.DrawString(TooltipFont, "Macro:" + invader.CurrentMacroBehaviour.ToString(), invader.Position + new Vector2(0, yPos), Color.White, 0, Vector2.Zero, 0.85f, SpriteEffects.None, 0);
                            yPos += yInc;

                            spriteBatch.DrawString(TooltipFont, "Int:" + invader.Intelligence.ToString(), invader.Position + new Vector2(0, yPos), Color.White);
                            yPos += yInc;

                            spriteBatch.DrawString(TooltipFont, "DrawDepth:" + invader.DrawDepth.ToString(), invader.Position + new Vector2(0, yPos), Color.White);
                            yPos += yInc;

                            if (invader.OperatingVehicle != null)
                            {
                                spriteBatch.DrawString(TooltipFont, "OperatingVehicle:" + invader.OperatingVehicle.InvaderType.ToString(), invader.Position + new Vector2(0, yPos), Color.White);
                                yPos += yInc;
                            }

                            if (invader.Waypoints.Count > 0)
                            {
                                spriteBatch.DrawString(TooltipFont, "Waypoints:" + invader.Waypoints.Count.ToString(), invader.Position + new Vector2(0, yPos), Color.White);
                                yPos += yInc;
                            }

                            if (invader.Pathfinder != null)
                            {
                                spriteBatch.DrawString(TooltipFont, "Findng Path", invader.Position + new Vector2(0, yPos), Color.White);
                                yPos += yInc;
                            }

                            if (invader.OperatorList.Count > 0)
                            {
                                spriteBatch.DrawString(TooltipFont, "Operators:" + invader.OperatorList.Count.ToString(), invader.Position + new Vector2(0, yPos), Color.White);
                                yPos += yInc;
                            }
                        }
                    }
                }

                if (Diagnostics == true)
                {
                    foreach (Trap trap in TrapList)
                    {
                        spriteBatch.DrawString(TooltipFont, "State: " + trap.TrapState, trap.Position, Color.White);
                        spriteBatch.DrawString(TooltipFont, "CurrentFrame: " + trap.CurrentAnimation.CurrentFrame, trap.Position + new Vector2(0, 10), Color.White);
                        spriteBatch.DrawString(TooltipFont, "FrameDelay: " + trap.CurrentAnimation.CurrentFrameDelay, trap.Position + new Vector2(0, 20), Color.White);
                        spriteBatch.DrawString(TooltipFont, "DrawDepth: " + trap.DrawDepth.ToString(), trap.Position + new Vector2(0, 30), Color.White);
                    }
                }
                #endregion

                #region Draw number changes
                //Draw the damage numbers for when the invaders are damaged
                foreach (NumberChange numChange in NumberChangeList)
                {
                    numChange.Draw(spriteBatch);
                }
                #endregion

                //if (WaveCountDown != null && WaveCountDown.CurrentSeconds > -1)
                //    WaveCountDown.Draw(spriteBatch);

                ResourceCounter.Draw(spriteBatch);

                #region Draw currency indicators
                spriteBatch.Draw(CurrencyIcon, new Rectangle(485, 1080 - 52, 24, 24), Color.White);
                spriteBatch.DrawString(RobotoRegular20_0, Resources.ToString(), new Vector2(552 - RobotoRegular20_0.MeasureString(Resources.ToString()).X, 1080 - 50), Color.White);
                #endregion

                PowerUnitsBar.Draw(spriteBatch);

                #region Draw the player status info - health/shield bars, turret and invader info
                spriteBatch.Draw(HealthIcon, new Rectangle((int)HealthBar.Position.X - 4, (int)HealthBar.Position.Y, HealthIcon.Width, HealthIcon.Height), null, Color.White, 0, new Vector2(HealthIcon.Width / 2, HealthIcon.Height / 2), SpriteEffects.None, 0);

                foreach (EffectPass pass in QuadEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    HealthBar.Draw(GraphicsDevice);
                    ShieldBar.Draw(GraphicsDevice);
                }
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

                #region Tower buttons
                foreach (Button towerSlot in TowerButtonList)
                {
                    towerSlot.Draw(spriteBatch);
                }
                #endregion

                #region Draw turret bars and outlines
                foreach (Turret turret in TurretList)
                {
                    if (turret != null)
                    {
                        if (turret.Active == true)
                        {
                            turret.DrawBars(GraphicsDevice, QuadEffect);
                            turret.TurretOutline.Draw(spriteBatch);
                        }
                    }
                }
                #endregion

                #region Draw trap bars, outline and quickinfo
                foreach (Trap trap in TrapList)
                {
                    if (trap.DestinationRectangle.Contains(VectorToPoint(CursorPosition)))
                        trap.DrawBars(GraphicsDevice, QuadEffect);

                    trap.TrapOutline.Draw(spriteBatch);
                    trap.TrapQuickInfo.Draw(GraphicsDevice, QuadEffect, spriteBatch);
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

                    if (invader.InvaderType == InvaderType.ShieldGenerator)
                    {
                        if (invader.Shield.Active == true)
                        {
                            foreach (EffectPass pass in QuadEffect.CurrentTechnique.Passes)
                            {
                                pass.Apply();
                                (invader as ShieldGenerator).ShieldBar.Draw(GraphicsDevice);
                            }
                        }
                    }

                    if (invader.CurrentBehaviourDelay < invader.MaxBehaviourDelay)
                    {
                        invader.ThinkingAnimation.Draw(spriteBatch);
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

                #region Draw the debug bounding boxes
                if (BoundingBoxes == true)
                {
                    //Draw the shield debug box
                    spriteBatch.Draw(ShieldBoundingSphere, new Rectangle((int)Tower.DestinationRectangle.Center.X - 300, (int)Tower.DestinationRectangle.Center.Y - 300, 600, 600), Color.White);

                    foreach (Invader invader in InvaderList)
                    {
                        spriteBatch.Draw(WhiteBlock, new Rectangle((int)invader.Position.X, (int)invader.Position.Y, 4, 4), Color.Red);
                        spriteBatch.Draw(WhiteBlock, new Rectangle((int)invader.Center.X, (int)invader.Center.Y, 4, 4), Color.Yellow);
                    }

                    foreach (Emitter emitter in YSortedEmitterList)
                    {
                        spriteBatch.Draw(WhiteBlock, new Rectangle((int)emitter.Position.X, (int)emitter.MaxY, 4, 4), Color.Red);
                    }

                    foreach (HeavyProjectile heavyProjectile in HeavyProjectileList)
                    {
                        spriteBatch.Draw(WhiteBlock, new Rectangle((int)heavyProjectile.Position.X, (int)heavyProjectile.Position.Y, 4, 4), Color.Red);
                        spriteBatch.Draw(WhiteBlock, new Rectangle((int)heavyProjectile.TipPosition.X, (int)heavyProjectile.TipPosition.Y, 4, 4), Color.Yellow);
                        spriteBatch.Draw(WhiteBlock, new Rectangle((int)heavyProjectile.BasePosition.X, (int)heavyProjectile.BasePosition.Y, 4, 4), Color.Purple);
                    }

                    foreach (Turret turret in TurretList.Where(Turret => Turret != null))
                    {
                        spriteBatch.Draw(WhiteBlock, new Rectangle((int)turret.Position.X, (int)turret.Position.Y, 4, 4), Color.Red);
                        spriteBatch.Draw(WhiteBlock, new Rectangle((int)turret.BarrelCenter.X, (int)turret.BarrelCenter.Y, 4, 4), Color.Yellow);
                        spriteBatch.Draw(WhiteBlock, new Rectangle((int)turret.BasePivot.X, (int)turret.BasePivot.Y, 4, 4), Color.Purple);
                    }

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

                            if (drawable.GetType() == typeof(ShieldGenerator))
                            {
                                spriteBatch.Draw(ShieldBoundingSphere,
                                    new Rectangle(
                                        (int)(drawable as ShieldGenerator).Center.X - (int)(drawable as ShieldGenerator).ShieldRadius,
                                        (int)(drawable as ShieldGenerator).Center.Y - (int)(drawable as ShieldGenerator).ShieldRadius,
                                        (int)(drawable as ShieldGenerator).ShieldRadius * 2,
                                        (int)(drawable as ShieldGenerator).ShieldRadius * 2), Color.White);
                            }
                        }

                        //Draw the debug bounding boxes here
                        VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[4];
                        int[] Indices = new int[8];

                        Vertices[0] = new VertexPositionColorTexture()
                        {
                            Color = drawableColor,
                            Position = drawable.CollisionBox.Min,
                            TextureCoordinate = new Vector2(0, 0)
                        };

                        Vertices[1] = new VertexPositionColorTexture()
                        {
                            Color = drawableColor,
                            Position = new Vector3(drawable.CollisionBox.Max.X, drawable.CollisionBox.Min.Y, 0),
                            TextureCoordinate = new Vector2(1, 0)
                        };

                        Vertices[2] = new VertexPositionColorTexture()
                        {
                            Color = drawableColor,
                            Position = drawable.CollisionBox.Max,
                            TextureCoordinate = new Vector2(1, 1)
                        };

                        Vertices[3] = new VertexPositionColorTexture()
                        {
                            Color = drawableColor,
                            Position = new Vector3(drawable.CollisionBox.Min.X, drawable.CollisionBox.Max.Y, 0),
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

                    foreach (Drawable drawable in DrawableList)
                    {
                        Color drawableColor = Color.White;

                        //if (drawable.GetType().BaseType == typeof(Trap))
                        //{
                        //    drawableColor = Color.Fuchsia;
                        //}

                        if (drawable.GetType().BaseType == typeof(Invader))
                        {
                            drawableColor = Color.Red;
                        }

                        if (drawable.GetType().BaseType == typeof(LightRangedInvader))
                        {
                            drawableColor = Color.Yellow;
                        }

                        if (drawable.GetType().BaseType == typeof(HeavyRangedInvader))
                        {
                            drawableColor = Color.Violet;

                            if (drawable.GetType() == typeof(ShieldGenerator))
                            {
                                spriteBatch.Draw(ShieldBoundingSphere,
                                    new Rectangle(
                                        (int)(drawable as ShieldGenerator).Center.X - (int)(drawable as ShieldGenerator).ShieldRadius,
                                        (int)(drawable as ShieldGenerator).Center.Y - (int)(drawable as ShieldGenerator).ShieldRadius,
                                        (int)(drawable as ShieldGenerator).ShieldRadius * 2,
                                        (int)(drawable as ShieldGenerator).ShieldRadius * 2), Color.White);
                            }
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

                    foreach (Drawable drawable in DrawableList.Where(Drawable => Drawable.GetType().BaseType == typeof(Invader)))
                    {
                        Color drawableColor = Color.White;
                        Invader inv = drawable as Invader;
                        //if (drawable.GetType().BaseType == typeof(Trap))
                        //{
                        //    drawableColor = Color.Fuchsia;
                        //}

                        if (drawable.GetType().BaseType == typeof(Invader))
                        {
                            drawableColor = Color.White;
                        }

                        //Draw the debug bounding boxes here
                        VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[4];
                        int[] Indices = new int[8];

                        Vertices[0] = new VertexPositionColorTexture()
                        {
                            Color = drawableColor,
                            Position = new Vector3(inv.DestinationRectangle.X, inv.DestinationRectangle.Y, 0),
                            TextureCoordinate = new Vector2(0, 0)
                        };

                        Vertices[1] = new VertexPositionColorTexture()
                        {
                            Color = drawableColor,
                            Position = new Vector3(inv.DestinationRectangle.Right, inv.DestinationRectangle.Y, 0),
                            TextureCoordinate = new Vector2(1, 0)
                        };

                        Vertices[2] = new VertexPositionColorTexture()
                        {
                            Color = drawableColor,
                            Position = new Vector3(inv.DestinationRectangle.Right, inv.DestinationRectangle.Bottom, 0),
                            TextureCoordinate = new Vector2(1, 1)
                        };

                        Vertices[3] = new VertexPositionColorTexture()
                        {
                            Color = drawableColor,
                            Position = new Vector3(inv.DestinationRectangle.Left, inv.DestinationRectangle.Bottom, 0),
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

                    foreach (Drawable drawable in DrawableList.Where(Drawable => Drawable.GetType().BaseType == typeof(Invader)))
                    {
                        Color drawableColor = Color.White;
                        Invader inv = drawable as Invader;
                        //if (drawable.GetType().BaseType == typeof(Trap))
                        //{
                        //    drawableColor = Color.Fuchsia;
                        //}

                        if (drawable.GetType().BaseType == typeof(Invader))
                        {
                            drawableColor = Color.Purple * 0.5f;
                        }

                        //Draw the debug bounding boxes here
                        VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[4];
                        int[] Indices = new int[8];

                        Vertices[0] = new VertexPositionColorTexture()
                        {
                            Color = drawableColor,
                            Position = inv.vertices[0].Position,
                            TextureCoordinate = new Vector2(0, 0)
                        };

                        Vertices[1] = new VertexPositionColorTexture()
                        {
                            Color = drawableColor,
                            Position = inv.vertices[1].Position,
                            TextureCoordinate = new Vector2(1, 0)
                        };

                        Vertices[2] = new VertexPositionColorTexture()
                        {
                            Color = drawableColor,
                            Position = inv.vertices[2].Position,
                            TextureCoordinate = new Vector2(1, 1)
                        };

                        Vertices[3] = new VertexPositionColorTexture()
                        {
                            Color = drawableColor,
                            Position = inv.vertices[3].Position,
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

                #region Draw Explosion Rays
                if (BoundingBoxes == true)
                {
                    foreach (myRay ray in ExplosionRays)
                    {
                        //Draw the debug bounding boxes here
                        VertexPositionColor[] Vertices = new VertexPositionColor[2];
                        int[] Indices = new int[2];

                        Vertices[0] = new VertexPositionColor()
                        {
                            Color = Color.Orange,
                            Position = ray.position
                        };

                        Vertices[1] = new VertexPositionColor()
                        {
                            Color = Color.Orange,
                            Position = ray.position + (ray.direction * ray.length)
                        };

                        Indices[0] = 0;
                        Indices[1] = 1;

                        foreach (EffectPass pass in BasicEffect2.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineStrip, Vertices, 0, 2, Indices, 0, 1, VertexPositionColorTexture.VertexDeclaration);
                        }
                    }
                }
                #endregion

                spriteBatch.End();
                #endregion

                #region Draw the circular bars
                //THIS MAY HAVE BEEN CAUSING UNEXPECTED ERROR. IT WASN'T CHECKING IF GAMESTATE != LOADING FIRST
                //MOVED IT INSIDE THE ABOVE CURSOR DRAW CHECK.
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, null, null, HealthBarEffect);

                foreach (Trap trap in TrapList)
                {
                    float val = (float)trap.CurrentRemovalTime / 1000;
                    HealthBarEffect.Parameters["meterValue"].SetValue(val);
                    CircularBar bar = trap.TrapQuickInfo.RemovalBar;

                    spriteBatch.Draw(bar.Texture,
                                 new Rectangle((int)bar.Position.X, (int)bar.Position.Y,
                                               (int)bar.Size.X, (int)bar.Size.Y), null, Color.White,
                                               MathHelper.ToRadians(-90), new Vector2(256, 256), SpriteEffects.None, 0);
                }

                //foreach (UITrapQuickInfo trapInfo in UITrapQuickInfoList)
                //{
                //    float val = (1 / trapInfo.RemovalBar.MaxValue) * trapInfo.RemovalBar.CurrentValue;
                //    HealthBarEffect.Parameters["meterValue"].SetValue(val);
                //    spriteBatch.Draw(HealthBarSprite, new Rectangle((int)trapInfo.RemovalBar.Position.X, (int)trapInfo.RemovalBar.Position.Y, (int)trapInfo.RemovalBar.Size.X, (int)trapInfo.RemovalBar.Size.Y), null, trapInfo.RemovalBar.FrontColor, 0,
                //                     new Vector2(32, 32), SpriteEffects.None, 0);
                //}

                //spriteBatch.Draw(HealthBarSprite, new Vector2(CursorPosition.X, CursorPosition.Y), null, Color.White, MathHelper.ToRadians(-90),
                //           new Vector2(HealthBarSprite.Width / 2, HealthBarSprite.Height / 2), 1f, SpriteEffects.None, 0);

                spriteBatch.End();
                #endregion
            }

            #region Draw everything to the backbuffer
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            
            #region DRAW ACTUAL GAME
            if (GameState == GameState.Playing || GameState == GameState.Paused || GameState == GameState.Victory && IsLoading == false)
            {
                #region Draw Maps while holding S key                
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                    spriteBatch.Draw(EmissiveMap, new Rectangle(0, 0, 480, 270), Color.White);
                    spriteBatch.DrawString(DefaultFont, "Emissive", new Vector2(0, 0), Color.White);

                    spriteBatch.Draw(BlurMap, new Rectangle(0, 270, 480, 270), Color.White);
                    spriteBatch.DrawString(DefaultFont, "Blur", new Vector2(0, 270), Color.White);

                    spriteBatch.Draw(NormalMap, new Rectangle(0, 540, 480, 270), Color.White);
                    spriteBatch.DrawString(DefaultFont, "Normal", new Vector2(0, 540), Color.White);

                    spriteBatch.Draw(SpecMap, new Rectangle(0, 810, 480, 270), Color.White);
                    spriteBatch.DrawString(DefaultFont, "Specular", new Vector2(0, 810), Color.White);

                    spriteBatch.Draw(DepthMap, new Rectangle(480, 0, 480, 270), Color.White);
                    spriteBatch.DrawString(DefaultFont, "Depth", new Vector2(480, 0), Color.White);

                    spriteBatch.Draw(LightMap, new Rectangle(480, 270, 480, 270), Color.White);
                    spriteBatch.DrawString(DefaultFont, "Light", new Vector2(480, 270), Color.White);

                    spriteBatch.Draw(Buffer1, new Rectangle(480, 540, 480, 270), Color.White);
                    spriteBatch.DrawString(DefaultFont, "Buffer1", new Vector2(480, 540), Color.White);

                    spriteBatch.Draw(Buffer2, new Rectangle(480, 810, 480, 270), Color.White);
                    spriteBatch.DrawString(DefaultFont, "Buffer2", new Vector2(480, 810), Color.White);


                    spriteBatch.Draw(CrepColorMap, new Rectangle(960, 0, 480, 270), Color.White);
                    spriteBatch.DrawString(DefaultFont, "CrepColor", new Vector2(960, 0), Color.White);

                    spriteBatch.Draw(ColorMap, new Rectangle(960, 270, 480, 270), Color.White);
                    spriteBatch.DrawString(DefaultFont, "Color", new Vector2(960, 270), Color.White);

                    spriteBatch.Draw(FinalMap, new Rectangle(960, 540, 480, 270), Color.White);
                    spriteBatch.DrawString(DefaultFont, "Final", new Vector2(960, 540), Color.White);

                    spriteBatch.Draw(ShadowMap, new Rectangle(960, 810, 480, 270), Color.White);
                    spriteBatch.DrawString(DefaultFont, "Shadow", new Vector2(960, 810), Color.Black);

                    spriteBatch.Draw(OcclusionMap, new Rectangle(960 + 480, 0, 480, 270), Color.White);
                    spriteBatch.DrawString(DefaultFont, "OccMap", new Vector2(960 + 480, 0), Color.Black);

                    spriteBatch.End();


                    CrepEffect.Parameters["ColorMap"].SetValue(DepthMap);
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                    CrepEffect.CurrentTechnique.Passes[0].Apply();
                    spriteBatch.Draw(CrepLightMap, new Rectangle(1440, 0, 320, 180), Color.White);
                    spriteBatch.End();
                }
                else
                    if (Keyboard.GetState().IsKeyDown(Keys.D))
                    {
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                        spriteBatch.Draw(DepthMap, new Rectangle(0, 0, 1920, 1080), Color.White);
                        spriteBatch.DrawString(DefaultFont, "Depth", new Vector2(0, 0), Color.White);

                        spriteBatch.End();
                    }
                        else
                        if (Keyboard.GetState().IsKeyDown(Keys.L))
                        {
                            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                            spriteBatch.Draw(LightMap, new Rectangle(0, 0, 1920, 1080), Color.White);
                            spriteBatch.DrawString(DefaultFont, "Light", new Vector2(0, 0), Color.White);

                            spriteBatch.End();
                        }
                        else
                            if (Keyboard.GetState().IsKeyDown(Keys.O))
                            {
                                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                                spriteBatch.Draw(OcclusionMap, new Rectangle(0, 0, 1920, 1080), Color.White);
                                spriteBatch.DrawString(DefaultFont, "Occlusion", new Vector2(0, 0), Color.White);

                                spriteBatch.End();
                            }
                #endregion
                else
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, null, null, null, Camera.Transform);
                    spriteBatch.Draw(FinalMap, FinalMap.Bounds, Color.White);
                    spriteBatch.Draw(Buffer2, FinalMap.Bounds, Color.White);
                    spriteBatch.End();

                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
                    spriteBatch.Draw(UIRenderTarget, ScreenDrawRectangle, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.End();
                }

            }
            #endregion

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
            #region DRAW THE MENU ITEMS
            if (GameState != GameState.Playing)
            {
                spriteBatch.Draw(MenuRenderTarget, ScreenDrawRectangle, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
            }
            #endregion

            spriteBatch.DrawString(RobotoRegular20_0, "Version: " + Window.Title, new Vector2(0 + 5, 1080 - 24), HalfWhite);
            CursorDraw(spriteBatch);
            spriteBatch.End();
            #endregion

            base.Draw(gameTime);
        }

        protected override void Update(GameTime gameTime)
        {
            FPSCounter.Update(gameTime);
            Slow = gameTime.IsRunningSlowly;

            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();

            CursorPosition = GetRealMousePosition();
            
            if (CurrentKeyboardState.IsKeyDown(Keys.F5))
            {
                Thread.Sleep(32);
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.F6))
            {
                return;
            }
            
            if (CurrentMouseState.LeftButton != PreviousMouseState.LeftButton)
            {
                LeftButtonState = CurrentMouseState.LeftButton;
            }

            if (this.IsActive == true)
                MenuButtonsUpdate(gameTime);

            if (CurrentMouseState.RightButton == ButtonState.Released &&
                PreviousMouseState.RightButton == ButtonState.Pressed)
            {
                CreateRightClick();
            }

            //Press Ctrl+Shift+V for instant victory
            //TODO: URGENT. REMOVE BEFORE RELEASE
            if (CurrentKeyboardState.IsKeyDown(Keys.LeftShift) && 
                CurrentKeyboardState.IsKeyDown(Keys.LeftControl) &&
                PreviousKeyboardState.IsKeyUp(Keys.V) && 
                CurrentKeyboardState.IsKeyDown(Keys.V))
            {
                PlayerVictory();
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


            if (CurrentKeyboardState.IsKeyDown(Keys.LeftControl) &&
                CurrentKeyboardState.IsKeyDown(Keys.F) &&
                PreviousKeyboardState.IsKeyUp(Keys.F))
            {
                int stop = 0;
                stop++;
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.LeftControl) &&
             CurrentKeyboardState.IsKeyDown(Keys.R) &&
             PreviousKeyboardState.IsKeyUp(Keys.R))
            {
                StartWaves();
            }


            if (CurrentKeyboardState.IsKeyDown(Keys.LeftControl) &&
                CurrentKeyboardState.IsKeyDown(Keys.G) &&
                PreviousKeyboardState.IsKeyUp(Keys.G))
            {
                Crate crate = new Crate(new Vector2(1000, 100), new Vector2(1000, 100));
                AddInvader(crate, gameTime);
                crate.Position = new Vector2(1000, 100);
            }


            #region Show diagnostics
            if (CurrentMouseState.LeftButton == ButtonState.Released &&
               CurrentKeyboardState.IsKeyUp(Keys.F1) &&
               PreviousKeyboardState.IsKeyDown(Keys.F1))
            {
                Diagnostics = !Diagnostics;
            }

            if (CurrentMouseState.LeftButton == ButtonState.Released &&
               CurrentKeyboardState.IsKeyUp(Keys.F3) &&
               PreviousKeyboardState.IsKeyDown(Keys.F3))
            {
                BoundingBoxes = !BoundingBoxes;
            }

            if (CurrentMouseState.RightButton == ButtonState.Released &&
                PreviousMouseState.RightButton == ButtonState.Pressed &&
                CurrentKeyboardState.IsKeyDown(Keys.LeftShift))
            {
                CreateExplosion(new Explosion(CursorPosition, 400, 5), new CannonTurret(Vector2.Zero));
            }

            //Hold left control and click on an invader to show its diagnostics
            if (InvaderList != null && InvaderList.Count > 0)
            foreach (Invader invader in InvaderList)
            {
                if (CurrentMouseState.LeftButton == ButtonState.Released &&
                    PreviousMouseState.LeftButton == ButtonState.Pressed &&
                    CurrentKeyboardState.IsKeyDown(Keys.LeftControl) &&
                    invader.DestinationRectangle.Contains(VectorToPoint(CursorPosition)))
                {
                    invader.ShowDiagnostics = !invader.ShowDiagnostics;
                }

                if (CurrentMouseState.RightButton == ButtonState.Released &&
                    PreviousMouseState.RightButton == ButtonState.Pressed &&
                    CurrentKeyboardState.IsKeyDown(Keys.LeftControl) &&
                    invader.DestinationRectangle.Contains(VectorToPoint(CursorPosition)))
                {
                    invader.CurrentHP = 0;
                }
            }
            #endregion

            //if (GameState == GameState.Paused || GameState == GameState.Playing)
            //if (LightList[0] != null)
            //{
            //    LightList[0].Position.X = CursorPosition.X;
            //    LightList[0].Position.Y = CursorPosition.Y;
            //    LightList[0].Depth = CursorPosition.Y / 1080f;
            //}


            switch (GameState)
            {
                #region In Game Update
                case GameState.Playing:
                    {
                        GameButtonsUpdate(gameTime);

                        if (CurrentLevel.LevelDialogue != null)
                            CurrentLevel.LevelDialogue.Update(gameTime);

                        Camera.Update(gameTime);

                        foreach (Light light in LightList)
                        {
                            light.Update(gameTime);
                        }

                        foreach (Solid solid in SolidList)
                        {
                            solid.Update(gameTime);
                        }

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

                        #region Ammo belts from destroyed turrets
                        foreach (AmmoBelt belt in AmmoBeltList)
                        {
                            belt.Update(gameTime);

                            if (belt.Active == false)
                                DrawableList.Remove(belt);
                        }

                        AmmoBeltList.RemoveAll(Belt => Belt.Active == false); 
                        #endregion

                        //if (PreviousResources != Resources)
                        //{
                        //    int diff = Resources - PreviousResources;

                        //    ResourceCounter.AddChange(diff);
                        //}

                        //foreach (JetEngine engine in JetEngineList)
                        //{
                        //    engine.Update(gameTime);
                        //}

                    

                        if (CurrentBeam != null)
                        {
                            UpdateBeams();
                        }

                        foreach (Rope rope in RopeList)
                        {
                            rope.Update(gameTime);

                            if (rope.Sticks.Count <= 1)
                            {
                                DrawableList.Remove(rope);
                            }
                        }
                        
                        foreach (JetEngine engine in JetEngineList)
                        {
                            engine.Update(gameTime);
                        }
                        
                        //ToonLightningList.RemoveAll(Lightning => Lightning.Active == false);

                        if (ToonLightningList.Count > 0)
                        {
                            for (int t = 0; t < ToonLightningList.Count; t++)
                            {
                                ToonLightningList[t].Update(gameTime);

                                if (ToonLightningList[t].Active == false)
                                {
                                    DrawableList.Remove(ToonLightningList[t]);
                                    ToonLightningList.Remove(ToonLightningList[t]);
                                }                                
                            }
                        }


                        RopeList.RemoveAll(Rope => Rope.Sticks.Count <= 1);

                        foreach (ExplosionEffect effect in ExplosionEffectList)
                        {
                            effect.Update(gameTime);
                        }

                        //if (TurretList[0] != null && SmokeTrailList.Count > 0)
                        //{
                        //    SmokeTrailList[0].StartPosition = TurretList[0].BarrelEnd;

                        //    //if (TurretList[0].Overheated == false)
                        //    //{
                        //    //    SmokeTrailList[0].AddMore = false;
                        //    //}
                        //}

                        foreach (SmokeTrail trail in SmokeTrailList)
                        {
                            trail.Update(gameTime);
                        }

                        SmokeTrailList.RemoveAll(SmokeTrail => SmokeTrail.Active == false);

                      

                        //Seconds += gameTime.ElapsedGameTime.TotalMilliseconds;
                        ResourceCounter.Update(gameTime);

                        //PowerUnitsBar ended up being null for some reason. Keep an eye on it.
                        if (PowerUnitsBar != null)
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

                        if (CurrentWaveIndex <= CurrentLevel.WaveList.Count -1)
                            HandleWaves(gameTime);

                        //if (WaveCountDown != null && WaveCountDown.CurrentSeconds > -1)
                        //    WaveCountDown.Update(gameTime);
                        
                        if (CurrentMouseState.LeftButton == ButtonState.Released &&
                            PreviousMouseState.LeftButton == ButtonState.Pressed)
                        {
                            if (ReadyToPlace == true &&
                                CursorPosition.X > (Tower.Position.X + Tower.Texture.Width) &&
                                (CursorPosition.Y - CursorPosition.Y % 32) >= GroundRange.X &&
                                (CursorPosition.Y - CursorPosition.Y % 32) <= GroundRange.Y &&                                
                                SelectedTrap != null &&
                                TrapList.All(Trap => !Trap.DestinationRectangle.Contains(VectorToPoint(CursorPosition))))
                            {
                                CreateTrapPlacement(SelectedTrap.Value);
                            }
                        }

                        //if (InvaderList.Any(Invader => Invader.InvaderType == InvaderType.DropShip && Invader.Velocity != Vector2.Zero))
                        //{
                        //    Camera.Shake(2, 1f);
                        //}

                        
                        #region Update and clean PowerupsList
                        if (PowerupsList.Count > 0)
                        {
                            PowerupsList.ForEach(Powerup => Powerup.Update(gameTime));
                            PowerupsList.RemoveAll(Powerup => Powerup.Active == false);

                            //for (int i = 0; i < PowerupsList.Count; i++)
                            //{
                            //    PowerupsList[i].Update(gameTime);

                            //    if (PowerupsList[i].Active == false)
                            //    {
                            //        PowerupsList.RemoveAt(i);
                            //    }
                            //}
                        }
                        #endregion
                        
                        #region Handle Particle Emitters and Particles
                        VerletShells.ForEach(Shell => 
                            {
                                Shell.Update(gameTime);

                                if (Shell.Active == false)
                                {
                                    DrawableList.Remove(Shell);
                                }
                            });

                        VerletShells.RemoveAll(Shell => Shell.Active == false);

                        //VerletShells.ForEach(x => { x.Update(gameTime); if (x.Active == false) VerletShells.Remove(x); });

                        //for (int s = 0; s < VerletShells.Count; s++)
                        //{
                        //    VerletShells[s].Update(gameTime);

                        //    if (VerletShells[s].Active == false)
                        //    {
                        //        DrawableList.Remove(VerletShells[s]);
                        //    }
                        //}

                        #region Handle YSortedEmitterList
                        YSortedEmitterList.ForEach(Emitter => 
                        {
                            Emitter.Update(gameTime);

                            if (Emitter.Active == false || (Emitter.AddMore == false && Emitter.ParticleList.Count == 0))
                            {
                                DrawableList.Remove(Emitter);
                            }
                        });

                        YSortedEmitterList.RemoveAll(Emitter => Emitter.Active == false || (Emitter.AddMore == false && Emitter.ParticleList.Count == 0));
                        
                        //for (int i = 0; i < YSortedEmitterList.Count; i++)
                        //{
                        //    Emitter emitter = YSortedEmitterList[i];

                        //    emitter.Update(gameTime);

                        //    if (emitter.AddMore == false && emitter.ParticleList.Count == 0)
                        //    {
                        //        YSortedEmitterList.RemoveAt(i);
                        //        DrawableList.Remove(emitter);
                        //    }
                        //}
                        #endregion

                        #region Handle AlphaEmitterList
                        AlphaEmitterList.ForEach(Emitter => Emitter.Update(gameTime));
                        AlphaEmitterList.RemoveAll(Emitter => Emitter.Active == false || (Emitter.AddMore == false && Emitter.ParticleList.Count == 0));

                        //foreach (Emitter emitter in AlphaEmitterList)
                        //{
                        //    emitter.Update(gameTime);
                        //}

                        //AlphaEmitterList.RemoveAll(Emitter => Emitter.Active == false);
                        //AlphaEmitterList.RemoveAll(Emitter => Emitter.AddMore == false && Emitter.ParticleList.Count == 0);
                        #endregion

                        #region Handle AdditiveEmitterList
                        AlphaEmitterList.ForEach(Emitter => Emitter.Update(gameTime));
                        AlphaEmitterList.RemoveAll(Emitter => Emitter.Active == false || (Emitter.AddMore == false && Emitter.ParticleList.Count == 0));
                        //foreach (Emitter emitter in AdditiveEmitterList)
                        //{
                        //    emitter.Update(gameTime);
                        //}

                        //AdditiveEmitterList.RemoveAll(Emitter => Emitter.Active == false);
                        //AdditiveEmitterList.RemoveAll(Emitter => Emitter.AddMore == false && Emitter.ParticleList.Count == 0);
                        #endregion

                        #region Handle GasEmitterList
                        GasEmitterList.ForEach(Emitter => Emitter.Update(gameTime));
                        GasEmitterList.RemoveAll(Emitter => Emitter.Active == false || (Emitter.AddMore == false && Emitter.ParticleList.Count == 0));

                        //foreach (Emitter emitter in GasEmitterList)
                        //{
                        //    emitter.Update(gameTime);
                        //}

                        //GasEmitterList.RemoveAll(Emitter => Emitter.Active == false);
                        //GasEmitterList.RemoveAll(Emitter => Emitter.AddMore == false && Emitter.ParticleList.Count == 0);
                        #endregion

                        CoinList.ForEach(Coin => Coin.Update(gameTime));
                        CoinList.RemoveAll(Coin => Coin.Active == false);

                        //foreach (Particle coin in CoinList)
                        //{
                        //    coin.Update(gameTime);
                        //}
                        //CoinList.RemoveAll(Coin => Coin.Active == false);

                        TotalParticles = 0;
                        foreach (Emitter emitter in YSortedEmitterList)
                        {
                            TotalParticles += emitter.ParticleList.Count;
                        }
                        #endregion

                        #region Projectile update stuff
                        if (HeavyProjectileList.Count > 0)
                            HeavyProjectileUpdate(gameTime);
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

                        TurretList.ForEach(Turret =>
                        {
                            if (Turret != null && Turret.Active == false)
                            {
                                DrawableList.Remove(Turret);
                                TowerButtonList[TurretList.IndexOf(Turret)].Active = true;
                                TowerButtonList[TurretList.IndexOf(Turret)].ButtonActive = true;
                                TurretList[TurretList.IndexOf(Turret)] = null;
                            }
                        });

                        //TurretList.RemoveAll(Turret => Turret != null && Turret.Active == false);    
                        #endregion

                        #region Trap stuff
                        if (TrapList.Count > 0)
                        {
                            TrapUpdate(gameTime);

                            for (int t = 0; t < TrapList.Count; t++)
                            {
                                if (TrapList[t].CurrentHP <= 0 || TrapList[t].CurrentDetonateLimit == 0)
                                {
                                    InvaderList.ForEach(Invader =>
                                    {
                                        if (Invader.TargetTrap == TrapList[t])
                                        {
                                            Invader.TargetTrap = null;
                                        }
                                    });

                                    DrawableList.Remove(TrapList[t]);
                                    TrapList.Remove(TrapList[t]);
                                }
                            }
                        }

                        //trap.Update(gameTime);

                        //if (trap.Active == false)
                        //{
                        //    InvaderList.ForEach(Invader =>
                        //    {
                        //        if (Invader.TargetTrap == trap)
                        //        {
                        //            Invader.TargetTrap = null;
                        //        }
                        //    });

                        //    TrapList.Remove(trap);
                        //    DrawableList.Remove(trap);
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

                                DecalList.Add(new Decal(ExplosionDecal1, PowerupDelivery.Position, 0, 1));
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
                        TrailList.ForEach(Trail => Trail.Update(gameTime));
                        //TrailList.RemoveAll(Trail => Trail.MiddleScale.X <= 0);

                        TrailList.RemoveAll(Trail => Trail.Color == Color.Transparent);

                        
                        //for (int i = 0; i < TrailList.Count; i++)
                        //{
                        //    BulletTrail trail = TrailList[i];
                        //    trail.Update(gameTime);

                        //    if (trail.Color == Color.Transparent)
                        //        TrailList.RemoveAt(i);
                        //}
                        #endregion

                        #region Update and clean DecalList
                        DecalList.ForEach(Decal => Decal.Update(gameTime));
                        DecalList.RemoveAll(Decal => Decal.TransparencyPercentage <= 0);

                        //for (int i = 0; i < DecalList.Count; i++)
                        //{
                        //    DecalList[i].Update(gameTime);

                        //    if (DecalList[i].TransparencyPercentage <= 0)
                        //        DecalList.RemoveAt(i);
                        //}
                        #endregion
                        #endregion

                        #region Handle number changes for damage etc.
                        NumberChangeList.ForEach(Change => Change.Update(gameTime));
                        NumberChangeList.RemoveAll(NumChange => NumChange.Active == false);
                        #endregion

                        #region Special Ability stuff
                        UpdateSpecialAbilities(gameTime);
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
                        
                        if (InvaderList.Count > 0)
                        {
                            InvaderUpdate(gameTime);
                        }
                        
                        #region Clean InvaderList
                        InvaderList.ForEach(Invader =>
                        {
                            if (Invader.Active == false)
                            {
                                DrawableList.Remove(Invader);
                            }
                        });
                        InvaderList.RemoveAll(Invader => Invader.Active == false);                        

                        ShieldList.ForEach(Shield =>
                            {
                                if ((Shield.Tether as Invader).Active == false)
                                {
                                    DrawableList.Remove(Shield);
                                }
                            });

                        ShieldList.RemoveAll(Shield => (Shield.Tether as Invader).Active == false);
                        #endregion

                        //Powerups have to be applied before the invaders are updated and after the explosions and projectiles are updated
                        //Create explosions/projectiles
                        //Powerup explosions/projectiles
                        //Damage invaders
                        //Remove explosions/projectiles

                        ApplyPowerups();

                        #region Sort Drawables if there is a change and overlap
                        foreach (Drawable drawable in DrawableList)
                        {
                            foreach (Drawable drawable2 in DrawableList)
                            {
                                if (drawable != drawable2 &&
                                    drawable.PreviousMaxY != drawable.MaxY &&
                                    Approx(drawable.PreviousMaxY, drawable2.MaxY, 0.5f))
                                {
                                    SortDrawables();
                                }
                            }
                        }
                        #endregion
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

                case TowerDefensePrototype.GameState.ProfileManagement:
                    {
                        if (LoadoutMenuDialogue != null)
                        {
                            LoadoutMenuDialogue.Update(gameTime);
                            //LoadoutMenuDialogue.Update(gameTime);                            
                        }
                    }
                    break;
            }
                       

            //PreviousResources = Resources;
            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;

            base.Update(gameTime);
        }
        
        public void OnExplosionHappened(object source, ExplosionEventArgs e)
        {
            Explosion explosion = e.Explosion;

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
                                            new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.Red, Color.Red,
                                            0.1f, 0.1f, 3, 5, true, new Vector2(invader.MaxY - 8, invader.MaxY + 8), false, (invader.DestinationRectangle.Bottom - 1) / 1080f, true, false,
                                            null, null, null, null, null, false, false, 1000);
                        BloodEmitter.Grow = true;

                        YSortedEmitterList.Add(BloodEmitter);
                        AddDrawable(BloodEmitter);
                        break;
                    #endregion
                }
            };
            #endregion

            if (source.GetType().BaseType != typeof(Invader))
            {
                ExplosionRays.Clear();

                foreach (Invader invader in InvaderList.Where(Invader => Vector2.Distance(Invader.Center, explosion.Position) < explosion.BlastRadius))
                {
                    Vector3 direction = new Vector3(invader.Center, 0) - new Vector3(explosion.Position, 0);

                    Ray explosionRay = new Ray(new Vector3(explosion.Position, 0), direction);

                    myRay myRay = new myRay()
                    {
                        position = explosionRay.Position,
                        direction = explosionRay.Direction
                    };

                    float invDist = float.PositiveInfinity;
                    float trapDist = float.PositiveInfinity;

                    invDist = explosionRay.Intersects(invader.BoundingBox).GetValueOrDefault(float.PositiveInfinity);

                    foreach (Trap trap in TrapList.Where(Trap => Trap.Solid == true && Vector2.Distance(Trap.Center, explosion.Position) < explosion.BlastRadius))
                    {
                        trapDist = explosionRay.Intersects(trap.BoundingBox).GetValueOrDefault(trapDist);
                    }
                    
                    if (trapDist > invDist)
                    {                        
                        invader.CurrentHP -= explosion.Damage;
                        ((source as HeavyProjectile).SourceObject as Turret).TotalDamageDone += explosion.Damage;
                        myRay.length = invDist;
                    }
                    else
                    {
                        myRay.length = trapDist;
                    }

                    ExplosionRays.Add(myRay);
                }
            }
        }

        #region BUTTON stuff
        private void MenuButtonsUpdate(GameTime gameTime)
        {
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
                                //else
                                //    VictoryRetry.Update(CursorPosition, gameTime);
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

        public void GameButtonsUpdate(GameTime gameTime)
        {
            #region In Game Buttons
            //This places the selected turret type into the right slot on the tower when the tower slot has been clicked

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

                                        if (Resources >= resourceCost && TurretList[Index] == null)
                                        {
                                            TowerButtonList[Index].ButtonActive = false;
                                            Turret newTurret = GetNewTurret(SelectedTurret.Value, Index);

                                            switch (newTurret.TurretType)
                                            {
                                                default:
                                                    TurretSpriteDictionary.TryGetValue(newTurret.TurretType.ToString() + "TurretBase", out newTurret.TurretBase);
                                                    TurretSpriteDictionary.TryGetValue(newTurret.TurretType.ToString() + "TurretBarrel", out newTurret.TurretBarrel);
                                                    break;
                                            }

                                            switch (newTurret.TurretType)
                                            {
                                                case TurretType.MachineGun:
                                                    newTurret.AmmoBelt = new AmmoBelt(newTurret.Position, MachineBullet);
                                                    break;
                                            }

                                            newTurret.Initialize(Content);

                                            newTurret.TurretOutline = new UIOutline(new Vector2(newTurret.SelectBox.X, newTurret.SelectBox.Y), new Vector2(newTurret.SelectBox.Width, newTurret.SelectBox.Height), null, newTurret);
                                            newTurret.TurretOutline.OutlineTexture = TurretSelectBox;

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
                                    StartWaves();
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

                                            //Fire = true,
                                            //MachineGun = true,

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
                                        if (CurrentProfile.Buttons.All(Weapon => Weapon == null) && CurrentProfile.LevelNumber > 1)
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
                                                    //var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

                                                    bool Available;
                                                    CurrentProfile.TurretDictionary.TryGetValue(turretBox.ContainsTurret.Value, out Available);

                                                    if (Available == true)
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
                                                    //var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);
                                                    bool Available;
                                                    CurrentProfile.TrapDictionary.TryGetValue(trapBox.ContainsTrap.Value, out Available);


                                                    if (Available == true)
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
                                    if (CurrentProfile.LevelNumber == 1)
                                    {
                                        LoadoutMenuDialogue = new LoadoutDialogue(this);
                                        StoryDialogueItems DialogueItems = Content.Load<StoryDialogueItems>("StoryDialogue/LoadoutMenuDialogue");
                                        LoadoutMenuDialogue.ItemsList = DialogueItems.DialogueItems;

                                        LoadoutMenuDialogue.TutorialMarker = new ButtonMarker(new Vector2(100, 100), Content.Load<Texture2D>("WhiteBlock"));

                                        LoadoutMenuDialogue.DialogueBox = new StoryDialogueBox();
                                        LoadoutMenuDialogue.DialogueBox.BoxTexture = Content.Load<Texture2D>("WhiteBlock");
                                        LoadoutMenuDialogue.DialogueBox.DialogueFont = Content.Load<SpriteFont>("Fonts/RobotoBold20_0_Outline");
                                        LoadoutMenuDialogue.DialogueBox.TipFont = Content.Load<SpriteFont>("Fonts/RobotoLight20_0");

                                    }

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
                                UnloadGameContent();
                                GameState = GameState.ProfileManagement;
                                return;
                            }

                            if (CurrentDialogBox == MainMenuDialog)
                            {
                                MenuClick.Play();
                                StorageDevice.BeginShowSelector(this.SaveProfile, null);
                                CurrentProfile = null;
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

        #region INVADER stuff
        private void InvaderUpdate(GameTime gameTime)
        {
            for (int p = 0; p < InvaderList.Count; p++)
            {
                Invader invader = InvaderList[p];
                HeavyRangedInvader heavyRangedInvader = invader as HeavyRangedInvader;
                LightRangedInvader lightRangedInvader = invader as LightRangedInvader;

                invader.Update(gameTime, CursorPosition);

                if (invader.InvaderType == InvaderType.DropShip &&
                    invader.Position.X < 0 - invader.DestinationRectangle.Width)
                {
                    invader.Active = false;
                }

                if (invader.InvaderType == InvaderType.DropShip)
                {
                    invader.DrawDepth = 400f / 1080f;
                }

                //STOP VERTICAL VELOCITY WHEN HITTING THE GROUND
                if (invader.CurrentMicroBehaviour != MicroBehaviour.FollowWaypoints)
                    if (invader.Airborne == false)
                {
                    if ((invader.BoundingBox.Max.Y + invader.Velocity.Y) > invader.MaxY)
                    {
                        invader.Velocity.Y = 0;
                        invader.Gravity = 0;
                        invader.Position = new Vector2(invader.Position.X, invader.MaxY - invader.DestinationRectangle.Height);
                        invader.InAir = false;

                        switch (invader.InvaderType)
                        {
                            default:

                                break;

                            case InvaderType.BatteringRam:
                                {
                                    Emitter DustEmitter = new Emitter(ToonDust3,
                                      new Vector2(invader.Center.X, invader.DestinationRectangle.Bottom + 2),
                                      new Vector2(50, 130), new Vector2(1f, 4f), new Vector2(1500, 2000), 1f, false, new Vector2(-10, 10),
                                      new Vector2(0f, 0f), new Vector2(0.03f, 0.04f), Color.White, Color.White, 0.03f, 0.02f, 5, 5, false,
                                      new Vector2(invader.DestinationRectangle.Bottom + 2, invader.DestinationRectangle.Bottom + 2), true, invader.DestinationRectangle.Bottom + 2 / 1080.0f,
                                      null, null, null, null, null, null, new Vector2(0.08f, 0.08f), false, false,
                                      null, null, null, null);

                                    YSortedEmitterList.Add(DustEmitter);

                                    AddDrawable(DustEmitter);
                                    BatteringRamDrop1.Play();
                                    Camera.Shake(10, 1f);
                                }
                                break;
                        }
                    }

                    if (invader.BoundingBox.Max.Y < invader.MaxY)
                    {
                        invader.Gravity = 0.2f;
                    }
                }

                #region Show damage and healing values
                if (invader.CurrentHP < invader.PreviousHP)
                {
                    NumberChangeList.Add(new NumberChange(DialogueFont, invader.Position, new Vector2(0, -1.5f), 0 - (int)(invader.PreviousHP - invader.CurrentHP), Color.Red));
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
                                Emitter BloodEmitter = new Emitter(ToonBloodDrip1, invader.Center,
                                new Vector2(0, 180), new Vector2(1, 2), new Vector2(800, 1600), 1f, false, new Vector2(0, 360),
                                new Vector2(1, 3), new Vector2(0.02f, 0.06f), Color.White, Color.White, 0.1f, 0.2f, 20, 10, true,
                                new Vector2(invader.MaxY, invader.MaxY), true, (invader.MaxY + 1f) / 1080f, true, false, null, null, null, true, null, true, true);
                                YSortedEmitterList.Add(BloodEmitter);
                                
                                AddDrawable(BloodEmitter);
                            }
                            break;
                        #endregion

                        case InvaderType.HealDrone:
                            {
                                Texture2D tex = InvaderAnimationsDictionary["HealDroneInvaderAnimations"][0].Texture;
                                ShellCasing turretBase = new ShellCasing(invader.Position, new Vector2(invader.Direction.X * 3, 0), tex);
                                VerletShells.Add(turretBase);
                                
                                AddDrawable(turretBase);
                            }
                            break;
                    }
                    
                    #region Create the Coin visual based on the resource value of the invader
                    for (int i = 0; i < invader.ResourceValue / 10; i++)
                    {
                        CoinList.Add(new Particle(Coin, invader.Center,
                                    (float)RandomDouble(70, 110), (float)RandomDouble(6, 8), 200f, 1f, true, (float)RandomDouble(0, 360), 
                                    (float)RandomDouble(-5, 5), 0.75f,
                                    Color.White, Color.White, 0.2f, true, MathHelper.Clamp(invader.MaxY + (float)RandomDouble(2, 32), 630, 960), false, null, true, true, false, false));
                    }
                    #endregion

                    Resources += invader.ResourceValue;

                    invader.Active = false;

                    //INVADERS ARE NOW REMOVED IN THE MAIN UPDATE METHOD AFTER InvaderUpdate(gameTime) HAS BEEN RUN
                    //SAFER THAT WAY
                    //InvaderList.Remove(invader);
                    //DrawableList.Remove(invader);

                    //ShieldList.RemoveAll(Shield => Shield.Tether == invader);
                    //DrawableList.RemoveAll(drawable => (drawable as Shield) != null && (drawable as Shield).Tether == invader);
                    return;
                }
                #endregion
                
                #region Damage the invaders if they walk into a gas cloud
                foreach (Emitter emitter in GasEmitterList)
                {
                    if (emitter.ParticleList.Any(Particle =>
                        Particle.DestinationRectangle.Intersects(BoundingBoxToRect(invader.CollisionBox)) &&
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
                Trap trap = TrapList.FirstOrDefault(Trap => Trap.BoundingBox.Intersects(invader.BoundingBox) && Trap.CanTrigger == true);
                if (invader.VulnerableToTrap == true)
                {
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
                if (invader.MeleeDamageStruct != null &&
                    invader.CanAttack == true &&
                    invader.TargetTrap != null &&
                    invader.TrapCollision == true)
                {
                    invader.TargetTrap.CurrentHP -= invader.MeleeDamageStruct.Damage;

                    //Create the melee damage effect based on the invader type and the trap type
                    switch (invader.InvaderType)
                    {
                        #region Soldier
                        case InvaderType.Soldier:
                            {
                                switch (invader.TargetTrap.TrapType)
                                {
                                    #region Wall
                                    case TrapType.Wall:
                                        {
                                            //Create hit effect
                                            Emitter BOOMEmitter;

                                            if (Random.NextDouble() >= 0.5f)
                                            {
                                                BOOMEmitter = new Emitter(WHAMParticle, new Vector2(invader.Position.X, invader.Center.Y),
                                                              new Vector2(0, 0), new Vector2(0.001f, 0.001f), new Vector2(250, 250), 1f, false,
                                                              new Vector2(-25, 25), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                                              Color.White, Color.White, 0f, 0.05f, 50, 1, false, new Vector2(0, 1080), true,
                                                              invader.TargetTrap.DrawDepth + (4 / 1080f), null, null, null, null, null, false, new Vector2(0.11f, 0.11f), false, false,
                                                              null, false, false, true);
                                            }
                                            else
                                            {
                                                BOOMEmitter = new Emitter(BAMParticle, new Vector2(invader.Position.X, invader.Center.Y),
                                                              new Vector2(0, 0), new Vector2(0.001f, 0.001f), new Vector2(250, 250), 1f, false,
                                                              new Vector2(-25, 25), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                                              Color.White, Color.White, 0f, 0.05f, 50, 1, false, new Vector2(0, 1080), true,
                                                              invader.TargetTrap.DrawDepth + (4 / 1080f), null, null, null, null, null, false, new Vector2(0.11f, 0.11f), false, false,
                                                              null, false, false, true);
                                            }

                                            YSortedEmitterList.Add(BOOMEmitter);

                                            Emitter hitEmitter = new Emitter(HitEffectParticle, new Vector2(invader.Position.X, invader.Center.Y),
                                                new Vector2(60, -60), new Vector2(5, 8), new Vector2(250, 500), 1f, false,
                                                new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                                Color.White, Color.Yellow, 0f, 0.05f, 50, 10, false, new Vector2(0, 1080), true,
                                                1.0f, null, null, null, null, null, true, new Vector2(0.2f, 0.2f), false, false,
                                                null, false, false, false);
                                            hitEmitter.Emissive = true;

                                            YSortedEmitterList.Add(hitEmitter);
                                            AddDrawable(hitEmitter, BOOMEmitter);
                                        }
                                        break;
                                    #endregion
                                }
                            }
                            break;
                        #endregion
                    }
                }
                #endregion
                
                #region MELEE attack TOWER
                if (invader.MeleeDamageStruct != null &&
                    invader.CanAttack == true &&
                    invader.TowerCollision == true)
                {
                    Tower.CurrentHP -= invader.MeleeDamageStruct.Damage;

                    switch (invader.InvaderType)
                    {
                        #region Soldier
                        case InvaderType.Soldier:
                            {
                                Emitter BOOMEmitter;

                                if (Random.NextDouble() >= 0.5f)
                                {
                                    BOOMEmitter = new Emitter(WHAMParticle, new Vector2(invader.Position.X, invader.Center.Y),
                                                  new Vector2(0, 0), new Vector2(0.001f, 0.001f), new Vector2(250, 250), 1f, false,
                                                  new Vector2(-25, 25), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                                  Color.White, Color.White, 0f, 0.05f, 50, 1, false, new Vector2(0, 1080), true,
                                                  invader.DrawDepth + (4 / 1080f), null, null, null, null, null, false, new Vector2(0.11f, 0.11f), false, false,
                                                  null, false, false, true);
                                }
                                else
                                {
                                    BOOMEmitter = new Emitter(BAMParticle, new Vector2(invader.Position.X, invader.Center.Y),
                                                  new Vector2(0, 0), new Vector2(0.001f, 0.001f), new Vector2(250, 250), 1f, false,
                                                  new Vector2(-25, 25), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                                  Color.White, Color.White, 0f, 0.05f, 50, 1, false, new Vector2(0, 1080), true,
                                                  invader.DrawDepth + (4 / 1080f), null, null, null, null, null, false, new Vector2(0.11f, 0.11f), false, false,
                                                  null, false, false, true);
                                }

                                YSortedEmitterList.Add(BOOMEmitter);

                                Emitter hitEmitter = new Emitter(HitEffectParticle, new Vector2(invader.Position.X, invader.Center.Y),
                                    new Vector2(60, -60), new Vector2(5, 8), new Vector2(250, 500), 1f, false,
                                    new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                    Color.White, Color.Yellow, 0f, 0.05f, 50, 10, false, new Vector2(0, 1080), true,
                                    1.0f, null, null, null, null, null, true, new Vector2(0.2f, 0.2f), false, false,
                                    null, false, false, false);
                                hitEmitter.Emissive = true;

                                YSortedEmitterList.Add(hitEmitter);
                                AddDrawable(hitEmitter, BOOMEmitter);
                            }
                            break; 
                        #endregion
                    }
                }
                #endregion


                #region HEAVY ranged invaders fire
                if (heavyRangedInvader != null &&
                    heavyRangedInvader.CanAttack == true &&
                    (heavyRangedInvader.InTowerRange == true || heavyRangedInvader.InTrapRange == true))
                {
                    switch (heavyRangedInvader.InvaderType)
                    {
                        #region Spider
                        case InvaderType.Spider:
                            {
                                HeavyProjectile heavyProjectile = new AcidProjectile(heavyRangedInvader, AcidProjectileSprite, RoundSparkParticle,
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
                                HeavyProjectile heavyProjectile = new AcidProjectile(heavyRangedInvader, AcidProjectileSprite, RoundSparkParticle,
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
                                HeavyProjectile heavyProjectile = new CannonBall(heavyRangedInvader, CannonBallProjectileSprite, ToonSmoke3,
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

                        #region Harpoon Cannon
                        case InvaderType.HarpoonCannon:
                            {
                                HeavyProjectile heavyProjectile = new HarpoonProjectile(heavyRangedInvader, HarpoonProjectileSprite, ToonSmoke3,
                                           new Vector2(heavyRangedInvader.BarrelEnd.X, heavyRangedInvader.BarrelEnd.Y),
                                           Random.Next((int)(heavyRangedInvader.LaunchVelocityRange.X), (int)(heavyRangedInvader.LaunchVelocityRange.Y)),
                                           (float)Math.PI + heavyRangedInvader.CurrentAngle, 0.2f, heavyRangedInvader.RangedDamage, 40,
                                           new Vector2(MathHelper.Clamp(heavyRangedInvader.MaxY + 32, 690, 930), 930));
                                heavyProjectile.Update(gameTime);
                                heavyProjectile.YRange = new Vector2(invader.MaxY, invader.MaxY);
                                HeavyProjectileList.Add(heavyProjectile);
                                

                                Rope rope = new Rope(heavyRangedInvader.BarrelEnd, heavyProjectile, heavyRangedInvader.BoundingBox.Max.Y);
                                rope.DrawDepth = invader.DrawDepth - (1f / 1080f);
                                rope.LoadContent(Content);
                                RopeList.Add(rope);
                                (invader as HarpoonCannon).Rope = rope;

                                AddDrawable(heavyProjectile, rope);
                            }
                            break;
                        #endregion

                        #region GunShip
                        case InvaderType.GunShip:
                            {
                                HeavyProjectile heavyProjectile = new DropMissile(heavyRangedInvader, CannonBallProjectileSprite,
                                    BallParticle, new Vector2(heavyRangedInvader.Center.X, heavyRangedInvader.BoundingBox.Max.Y - 80),
                                    0, 0, 0.52f, 80, 100, 180f, 600f, new Vector2(690, 930));
                                heavyProjectile.Update(gameTime);
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
                                float angle = (float)RandomDouble(lightRangedInvader.CurrentAngle - (MathHelper.ToRadians(3)), lightRangedInvader.CurrentAngle + (MathHelper.ToRadians(3)));
                                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                                CreateLightProjectile(new MachineGunProjectile(lightRangedInvader.Center, direction, lightRangedInvader.RangedDamage), lightRangedInvader);

                                //ShellCasing newShell = new ShellCasing(
                                //    new Vector2(lightRangedInvader.Position.X, lightRangedInvader.Position.Y), 
                                //    new Vector2(Random.Next(1, 3), -Random.Next(2, 5)), 
                                //    SmallShellingCasing, new Vector2(invader.MaxY, invader.MaxY + 12));
                                //newShell.Initialize();
                                //VerletShells.Add(newShell);

                                //AddDrawable(newShell);
                            }
                            break;
                        #endregion
                    }
                }
                #endregion
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
                    Trap trap = TrapList.FirstOrDefault(Trap => 
                        Trap.BoundingBox.Intersects(heavyProjectile.BoundingBox) && 
                        Trap.Solid == true &&
                        //ONLY COLLIDES WITH TRAP IF THEY HAVE A SIMILAR DEPTH
                        (heavyProjectile.MaxY > Trap.CollisionBox.Min.Y - 6 &&
                         heavyProjectile.MaxY < Trap.CollisionBox.Max.Y + 6));

                    if (trap != null)
                    {
                        CreateHeavyProjectileCollision(heavyProjectile, heavyProjectile.SourceObject, trap);
                        return;
                    }

                    //if (TrapList.Any(Trap => Trap.BoundingBox.Intersects(heavyProjectile.BoundingBox) &&
                    //    Trap.Solid == true))
                    //{
                    //    Trap trap = TrapList.Where(Trap => Trap.BoundingBox.Intersects(heavyProjectile.BoundingBox) && Trap.Solid == true).ToList().First();
                    //    CreateHeavyProjectileCollision(heavyProjectile, heavyProjectile.SourceObject, trap);
                    //    return;
                    //}
                    #endregion

                    #region GROUND was hit
                    if (heavyProjectile.BoundingBox.Max.Y > heavyProjectile.MaxY && heavyProjectile.Active == true)
                    {
                        CreateHeavyProjectileCollision(heavyProjectile, heavyProjectile.SourceObject, Ground);
                        return;
                    }
                    #endregion

                    if (heavyProjectile.SourceObject.GetType().BaseType == typeof(HeavyRangedInvader))
                    {
                        #region SHIELD was hit
                        if (heavyProjectile.BoundingBox.Intersects(Tower.Shield.BoundingSphere) && 
                            Tower.Shield.ShieldOn == true && 
                            heavyProjectile.ShieldSolid == true)
                        {
                            CreateHeavyProjectileCollision(heavyProjectile, heavyProjectile.SourceObject, Tower.Shield);
                            return;
                        }
                        #endregion

                        //#region TOWER was hit
                        //if (heavyProjectile.BoundingBox.Intersects(Tower.BoundingBox))
                        //{
                        //    CreateHeavyProjectileCollision(heavyProjectile, heavyProjectile.SourceObject, Tower);
                        //    return;
                        //}
                        //#endregion

                        #region TURRET was hit
                        if (TurretList.Any(Turret => Turret != null && 
                            Turret.BoundingBox.Intersects(heavyProjectile.BoundingBox)))
                        {
                            //Got an intersection with null turret here. Keep an eye on it.
                            //Turret turret = TurretList.Find(Turret => Turret.BoundingBox.Intersects(heavyProjectile.BoundingBox));
                            Turret turret = TurretList.FirstOrDefault(Turret => Turret != null && Turret.BoundingBox.Intersects(heavyProjectile.BoundingBox));
                            CreateHeavyProjectileCollision(heavyProjectile, heavyProjectile.SourceObject, turret);
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

                        #region INVADER SHIELD was hit
                        //if (ShieldList.Any(Shield => Shield != null && Shield.BoundingSphere.Intersects(heavyProjectile.BoundingBox)))
                        //{
                        //    Shield shield = ShieldList.Find(Shield => Shield.BoundingSphere.Intersects(heavyProjectile.BoundingBox));
                        //    CreateHeavyProjectileCollision(heavyProjectile, heavyProjectile.SourceObject, shield);
                        //    return;
                        //}
                        if (InvaderList.Any(Invader => Invader.Shield != null && 
                            Invader.Shield.BoundingSphere.Intersects(heavyProjectile.BoundingBox) && 
                            Invader.Shield.ShieldOn == true))
                        {
                            //Shield shield = ShieldList.Find(Shield => Shield.BoundingSphere.Intersects(heavyProjectile.BoundingBox));
                            Shield shield = InvaderList.Find(Invader => Invader.Shield.BoundingSphere.Intersects(heavyProjectile.BoundingBox)).Shield;
                            CreateHeavyProjectileCollision(heavyProjectile, heavyProjectile.SourceObject, shield);
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
                    Trap trap = TrapList.FirstOrDefault(Trap => Trap.BoundingBox.Intersects(timedHeavyProjectile.BoundingBox) && Trap.TrapType == TrapType.Wall);

                    if (timedHeavyProjectile.Active == true)
                    {
                        switch (timedHeavyProjectile.HeavyProjectileType)
                        {
                            case HeavyProjectileType.Grenade:
                                //Make grenade bounce off of walls
                                break;
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
                                        Decal NewDecal = new Decal(ExplosionDecal1, new Vector2(timedHeavyProjectile.Rod.Center.X, timedHeavyProjectile.Rod.Center.Y), timedHeavyProjectile.MaxY, 0.5f);

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

                heavyProjectile.Update(gameTime);
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
                                        float angle = MathHelper.ToDegrees((float)Math.Atan2(heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X));
                                        
                                        #region Smoke
                                        Emitter Emitter2 = new Emitter(ToonSmoke2, new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y), new Vector2(angle - 30, angle + 30), new Vector2(1, 1),
                                        new Vector2(500, 1000), 1f, false, new Vector2(-10, 10), new Vector2(-1, 1), new Vector2(0.05f, 0.06f), new Color(255, 128, 0, 6), Color.Black,
                                        -0.005f, 0.4f, 50, 10, false, new Vector2(0, 720), true, (collisionTrap.BoundingBox.Max.Y + 1f) / 1080f,
                                        null, null, null, null, null, false, null, null, null,
                                        null, null, null, true, null);

                                        Emitter Emitter = new Emitter(ToonSmoke3,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y), new Vector2(angle - 30, angle + 30), new Vector2(1, 1),
                                        new Vector2(500, 1000), 1f, false, new Vector2(-10, 10), new Vector2(-1, 1), new Vector2(0.05f, 0.06f), new Color(255, 128, 0, 6), Color.Black,
                                        -0.005f, 0.4f, 50, 10, false, new Vector2(0, 720), true, (collisionTrap.BoundingBox.Max.Y + 1f) / 1080f,
                                        null, null, null, null, null, false, null, null, null,
                                        null, null, null, true, null);

                                        YSortedEmitterList.Add(Emitter);
                                        YSortedEmitterList.Add(Emitter2);

                                        AddDrawable(Emitter, Emitter2);
                                        #endregion

                                        Emitter ExplosionEmitter = new Emitter(ExplosionParticle2,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y),
                                        new Vector2(angle - 70, angle + 70), new Vector2(0.3f, 0.8f), new Vector2(500, 1000), 1f, true, new Vector2(-2, 2),
                                        new Vector2(-1, 1), new Vector2(0.15f, 0.25f), new Color(255, 128, 0, 6), new Color(0, 0, 0, 255), -0.05f, 0.1f, 10, 1, false,
                                        new Vector2(heavyProjectile.BoundingBox.Max.Y, heavyProjectile.BoundingBox.Max.Y + 8), false, (collisionTrap.BoundingBox.Max.Y + 2f) / 1080f,
                                        null, null, null, null, null, null, new Vector2(0.1f, 0.2f), true, true, null, null, null, true);
                                        ExplosionEmitter.Emissive = true;
                                        YSortedEmitterList.Add(ExplosionEmitter);

                                        Emitter ExplosionEmitter3 = new Emitter(ExplosionParticle2,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y),
                                        new Vector2(angle - 5, angle + 5), new Vector2(2, 4), new Vector2(400, 640), 1f, false, new Vector2(0, 0),
                                        new Vector2(0, 0), new Vector2(0.085f, 0.2f), new Color(255, 128, 0, 6), new Color(0, 0, 0, 255), -0.1f, 0.05f, 10, 1, false,
                                        new Vector2(heavyProjectile.BoundingBox.Max.Y, heavyProjectile.BoundingBox.Max.Y + 8), true, (collisionTrap.BoundingBox.Max.Y + 1f) / 1080f,
                                        null, null, null, null, null, null, new Vector2(0.0025f, 0.0025f), true, true, 50);
                                        ExplosionEmitter.Emissive = true;
                                        YSortedEmitterList.Add(ExplosionEmitter3);

                                        Emitter HitEffect1 = new Emitter(HitEffectParticle, new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y), 
                                        new Vector2(angle - 70f, angle + 70f), new Vector2(5f, 8f),
                                        new Vector2(250f, 500f), 1f, false, new Vector2(0f, 360f), new Vector2(-2f, 2f),
                                        new Vector2(0.35f, 0.35f), new Color(255, 255, 191, 255),
                                        new Color(255, 255, 255, 255), 0f, 0.05f, 50f, 7, false, new Vector2(0f, 1080), true,
                                        (collisionTrap.BoundingBox.Max.Y + 3f) / 1080f, false, false, null, null, 0f, true, new Vector2(0.11f, 0.11f), false, false, 0f,
                                        false, false, false, null);
                                        ExplosionEmitter.Emissive = true;
                                        YSortedEmitterList.Add(HitEffect1);

                                        AddDrawable(HitEffect1, ExplosionEmitter, ExplosionEmitter3);
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
                                ////This was made with the particle editor
                                //for (int i = 0; i < 10; i++)
                                //{
                                //    //Emitter MultiFire_Explosion5 = new Emitter(ExplosionParticle2, new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y),
                                //    //    new Vector2(-155f, -115f), new Vector2(1f, 1f),
                                //    //    new Vector2(500f, 1500f), 1f, true, new Vector2(-2f, 2f), new Vector2(-1f, 1f), new Vector2(0.0825f, 0.0960f),
                                //    //    new Color(255, 128, 0, 6), new Color(0, 0, 0, 255), 0f, 0.78f, 30f, 3, true, new Vector2(heavyProjectile.BoundingBox.Max.Y, heavyProjectile.BoundingBox.Max.Y + 24), false,
                                //    //    (heavyProjectile.BoundingBox.Max.Y - 4) / 1080f, true, true, new Vector2(4f, 5f), new Vector2(42f, 120f), 0.2f, false, new Vector2(0.1f, 0.2f),
                                //    //    false, false, 0f, false, false, true, null);

                                //    Emitter MultiFire_Explosion5 = new Emitter(ExplosionParticle2, new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y),
                                //        new Vector2(-81f, -41f), new Vector2(1f, 1f),
                                //        new Vector2(500f, 1500f), 1f, false, new Vector2(-2f, 2f),
                                //        new Vector2(-1f, 1f), new Vector2(0.03f, 0.0585f), new Color(255, 128, 0, 6),
                                //        new Color(0, 0, 0, 255), 0f, 0.78f, 16f, 3, true, new Vector2(heavyProjectile.BoundingBox.Max.Y + 2, heavyProjectile.BoundingBox.Max.Y + 24), true,
                                //        (heavyProjectile.BoundingBox.Max.Y - 4) / 1080f, true, true, new Vector2(4f, 5f), new Vector2(42f, 120f), 0.2f, false,
                                //        new Vector2(0.1f, 0.2f), false, false, 0f, true, false, true, null);
                                //    MultiFire_Explosion5.Emissive = true;

                                //    YSortedEmitterList.Add(MultiFire_Explosion5);
                                //    AddDrawable(MultiFire_Explosion5);
                                //}

                                #region Smoke
                                Emitter Emitter2 = new Emitter(ToonSmoke2,
                                                                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y), new Vector2(60, 120), new Vector2(1, 1),
                                                                           new Vector2(500, 1000), 1f, false, new Vector2(-10, 10), new Vector2(-1, 1), new Vector2(0.05f, 0.06f), new Color(255, 128, 0, 6), Color.Black,
                                                                           -0.005f, 0.4f, 50, 10, false, new Vector2(0, 720), true, (heavyProjectile.BoundingBox.Max.Y - 4) / 1080f,
                                                                           null, null, null, null, null, false, null, null, null,
                                                                           null, null, null, true, null);

                                Emitter Emitter = new Emitter(ToonSmoke3,
                                                             new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y), new Vector2(60, 120), new Vector2(1, 1),
                                                              new Vector2(500, 1000), 1f, false, new Vector2(-10, 10), new Vector2(-1, 1), new Vector2(0.05f, 0.06f), new Color(255, 128, 0, 6), Color.Black,
                                                              -0.005f, 0.4f, 50, 10, false, new Vector2(0, 720), true, (heavyProjectile.BoundingBox.Max.Y - 4) / 1080f,
                                                              null, null, null, null, null, false, null, null, null,
                                                              null, null, null, true, null);

                                YSortedEmitterList.Add(Emitter);
                                YSortedEmitterList.Add(Emitter2);

                                AddDrawable(Emitter, Emitter2); 
                                #endregion

                                Emitter ExplosionEmitter = new Emitter(ExplosionParticle2,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y),
                                        new Vector2(20, 160), new Vector2(0.3f, 0.8f), new Vector2(500, 1000), 1f, true, new Vector2(-2, 2),
                                        new Vector2(-1, 1), new Vector2(0.15f, 0.25f), new Color(255, 128, 0, 6), new Color(0, 0, 0, 255), -0.2f, 0.1f, 10, 1, false,
                                        new Vector2(heavyProjectile.BoundingBox.Max.Y, heavyProjectile.BoundingBox.Max.Y + 8), false, heavyProjectile.BoundingBox.Max.Y / 1080f,
                                        null, null, null, null, null, null, new Vector2(0.1f, 0.2f), true, true, null, null, null, true);
                                ExplosionEmitter.Emissive = true;
                                YSortedEmitterList.Add(ExplosionEmitter);

                                Emitter ExplosionEmitter3 = new Emitter(ExplosionParticle2,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y),
                                        new Vector2(85, 95), new Vector2(2, 4), new Vector2(400, 640), 1f, false, new Vector2(0, 0),
                                        new Vector2(0, 0), new Vector2(0.085f, 0.2f), new Color(255, 128, 0, 6), new Color(0, 0, 0, 255), -0.1f, 0.05f, 10, 1, false,
                                        new Vector2(heavyProjectile.BoundingBox.Max.Y, heavyProjectile.BoundingBox.Max.Y + 8), true, heavyProjectile.BoundingBox.Max.Y / 1080f,
                                        null, null, null, null, null, null, new Vector2(0.0025f, 0.0025f), true, true, 50);
                                ExplosionEmitter3.Emissive = true;
                                YSortedEmitterList.Add(ExplosionEmitter3);

                                Emitter BOOMEmitter = new Emitter(BOOMParticle, new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y - 12),
                                                       new Vector2(0, 0), new Vector2(0.001f, 0.001f), new Vector2(400, 400), 1f, false,
                                                       new Vector2(-25, 25), new Vector2(0, 0), new Vector2(0.35f, 0.35f),
                                                       Color.White, Color.White, 0f, 0.05f, 50, 1, false, new Vector2(0, 1080), true,
                                                       heavyProjectile.BoundingBox.Max.Y + 4 / 1080f, null, null, null, null, null, false, new Vector2(0.11f, 0.11f), false, false,
                                                       null, false, false, true);
                                YSortedEmitterList.Add(BOOMEmitter);

                                Emitter DebrisEmitter = new Emitter(SplodgeParticle,
                                       new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y - 24),
                                       new Vector2(70, 110), new Vector2(3, 7), new Vector2(1200, 2400), 1f, false, new Vector2(0, 360),
                                       new Vector2(1, 3), new Vector2(0.05f, 0.1f), Color.DimGray, Color.DimGray,
                                       0.2f, 0.2f, 5, 1, true, new Vector2(heavyProjectile.BoundingBox.Max.Y - 16, heavyProjectile.BoundingBox.Max.Y + 16), true,
                                       heavyProjectile.BoundingBox.Max.Y / 1080f, true, true);
                                YSortedEmitterList.Add(DebrisEmitter);
                                
                                //This was generated with the particle editor
                                Emitter HitEffect1 = new Emitter(HitEffectParticle, new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y), new Vector2(20f, 160f), new Vector2(5f, 8f), 
                                    new Vector2(250f, 500f), 1f, false, new Vector2(0f, 360f), new Vector2(-2f, 2f), 
                                    new Vector2(0.35f, 0.35f), new Color(255, 255, 191, 255),
                                    new Color(255, 255, 255, 255), 0f, 0.05f, 50f, 7, false, new Vector2(0f, 1080), true, 
                                    (heavyProjectile.BoundingBox.Max.Y + 8) / 1080f, 
                                    false, false, null, null, 0f, true, new Vector2(0.11f, 0.11f), false, false, 0f, 
                                    false, false, false, null);
                                HitEffect1.Emissive = true;

                                YSortedEmitterList.Add(HitEffect1);

                                AddDrawable(DebrisEmitter, HitEffect1, ExplosionEmitter3, ExplosionEmitter, BOOMEmitter);
                                #endregion
                                
                                ExplosionEffect explosionEffect = new ExplosionEffect(new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y));
                                explosionEffect.Texture = ExplosionRingSprite;
                                ExplosionEffectList.Add(explosionEffect);

                                Decal NewDecal = new Decal(ExplosionDecal1, new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y),
                                                          (float)RandomDouble(0, 0),  0.75f);

                                DecalList.Add(NewDecal);

                                //ShockWaveEffect.Parameters["CenterCoords"].SetValue(new Vector2(1 / (1920 / heavyProjectile.Position.X), 1 / (1080 / heavyProjectile.Position.Y)));
                                //ShockWaveEffect.Parameters["WaveParams"].SetValue(new Vector4(1, 0.5f, 0.06f, 60));
                                //ShockWaveEffect.Parameters["CurrentTime"].SetValue(0);

                                Explosion newExplosion = new Explosion(new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y), heavyProjectile.BlastRadius, heavyProjectile.Damage);

                                CreateExplosion(newExplosion, heavyProjectile);

                                Camera.Shake(15, 1.5f);
                                CannonExplosion.Play();

                                LightList.Add(new Light()
                                {
                                    Active = true,
                                    Color = Color.OrangeRed,
                                    CurTime = 0,
                                    MaxTime = 150f,
                                    Depth = heavyProjectile.BoundingBox.Max.Y / 1080f,
                                    Position = new Vector3(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y, 250),
                                    Power = 0.8f,
                                    Size = 600f
                                });
                            }
                            break;
                        #endregion

                        #region Sticky Mine
                        case HeavyProjectileType.StickyMine:
                            {
                                StickyMine newMine = new StickyMine(GrenadeProjectileSprite, heavyProjectile.Position, StickyMineList.Count);
                                StickyMineList.Add(newMine);
                                newMine.Initialize();
                                newMine.DrawDepth = heavyProjectile.Position.Y / 1080f;
                                AddDrawable(newMine);
                            }
                            break; 
                        #endregion
                    }

                    DeactivateProjectile(heavyProjectile);
                    return;
                }
                #endregion

                #region INVADER SHIELD was hit
                if (collisionShield != null)
                {
                    switch (heavyProjectile.HeavyProjectileType)
                    {
                        #region Cannon Ball
                        case HeavyProjectileType.CannonBall:
                            {
                                for (int i = 0; i < Random.Next(4, 10); i++)
                                {
                                    Emitter SparkEmitter2 = new Emitter(RoundSparkParticle, heavyProjectile.Position,
                                    new Vector2(0, 360), new Vector2(0.5f, 1.5f), new Vector2(480, 960), 100f, true, new Vector2(0, 360),
                                    new Vector2(1, 3), new Vector2(0.1f, 0.3f), Color.DarkBlue, Color.Aquamarine, 0.05f, 0.5f, 1, 1, true,
                                    new Vector2(collisionShield.DestinationRectangle.Bottom, collisionShield.DestinationRectangle.Bottom),
                                    false, null, true, true, new Vector2(4, 8),
                                    new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(heavyProjectile.Velocity.Y, heavyProjectile.Velocity.X)) - 180 - (float)RandomDouble(0, 90),
                                                MathHelper.ToDegrees(-(float)Math.Atan2(heavyProjectile.Velocity.Y, heavyProjectile.Velocity.X)) - 180 + (float)RandomDouble(0, 45)),
                                                0.2f, false, new Vector2(0.25f, 0), null, null, null, true, null);

                                    AdditiveEmitterList.Add(SparkEmitter2);
                                }

                                collisionShield.TakeDamage(heavyProjectile.Damage);
                                //Tower.TakeDamage(sourceInvader.RangedDamage);
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
                #region INVADER was hit
                if (collisionInvader != null)
                {

                }
                #endregion

                #region GROUND was hit
                if (collisionGround != null)
                {

                    DeactivateProjectile(heavyProjectile);
                }
                #endregion
            }
            #endregion

            #region INVADER spawned the projectile
            if (sourceInvader != null)
            {
                #region TRAP was hit
                if (collisionTrap != null)
                {
                    sourceInvader.HitObject = collisionTrap;

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
                                            collisionTrap.CurrentHP -= sourceInvader.RangedDamage;
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
                    sourceInvader.HitObject = Ground;

                    switch ((source as Invader).InvaderType)
                    {
                        #region Stationary Cannon
                        case InvaderType.StationaryCannon:
                            {
                                #region Regular ground
                                Emitter ExplosionEmitter = new Emitter(ExplosionParticle2,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y),
                                        new Vector2(20, 160), new Vector2(0.3f, 0.8f), new Vector2(500, 1000), 0.85f, true, new Vector2(-2, 2),
                                        new Vector2(-1, 1), new Vector2(0.15f, 0.25f), FireColor,
                                        Color.Black, -0.2f, 0.1f, 10, 1, false,
                                        new Vector2(heavyProjectile.MaxY, heavyProjectile.MaxY + 8), false, heavyProjectile.MaxY / 1080f,
                                        null, null, null, null, null, null, new Vector2(0.1f, 0.2f), true, true, null, null, null, true);
                                YSortedEmitterList.Add(ExplosionEmitter);

                                Emitter ExplosionEmitter3 = new Emitter(ExplosionParticle2,
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y),
                                        new Vector2(85, 95), new Vector2(2, 4), new Vector2(400, 640), 0.35f, true, new Vector2(0, 0),
                                        new Vector2(0, 0), new Vector2(0.085f, 0.2f), FireColor, ExplosionColor3, -0.1f, 0.05f, 10, 1, false,
                                        new Vector2(heavyProjectile.MaxY, heavyProjectile.MaxY + 8), true, heavyProjectile.MaxY / 1080f,
                                        null, null, null, null, null, null, new Vector2(0.0025f, 0.0025f), true, true, 50);
                                YSortedEmitterList.Add(ExplosionEmitter3);

                                Emitter DebrisEmitter = new Emitter(SplodgeParticle,
                                       new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y),
                                       new Vector2(70, 110), new Vector2(5, 7), new Vector2(480, 1760), 1f, true, new Vector2(0, 360),
                                       new Vector2(1, 3), new Vector2(0.007f, 0.05f), Color.DarkSlateGray, Color.DarkSlateGray,
                                       0.2f, 0.2f, 5, 1, true, new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 16), null, heavyProjectile.MaxY / 1080f);
                                YSortedEmitterList.Add(DebrisEmitter);

                                AddDrawable(ExplosionEmitter, ExplosionEmitter3, DebrisEmitter);
                                #endregion                              

                                Decal NewDecal = new Decal(ExplosionDecal1, new Vector2(heavyProjectile.Position.X, heavyProjectile.BoundingBox.Max.Y),
                                                          (float)RandomDouble(0, 0), 0.75f);

                                DecalList.Add(NewDecal);


                                Explosion newExplosion = new Explosion(heavyProjectile.Position, heavyProjectile.BlastRadius, heavyProjectile.Damage);

                                CreateExplosion(newExplosion, source);
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

                #region TOWER was hit
                if (collisionTower != null)
                {
                    sourceInvader.HitObject = collisionTower;

                    switch ((source as Invader).InvaderType)
                    {
                        #region Stationary Cannon
                        case InvaderType.StationaryCannon:
                            {
                                DeactivateProjectile(heavyProjectile);
                            }
                            break;
                        #endregion

                        #region Archer
                        case InvaderType.Archer:
                            {
                                DeactivateProjectile(heavyProjectile);
                            }
                            break;
                        #endregion
                    }
                    return;
                }
                #endregion

                #region TURRET was hit
                if (collisionTurret != null)
                {
                    sourceInvader.HitObject = collisionTurret;

                    switch ((source as Invader).InvaderType)
                    {
                        #region Harpoon Cannon
                        case InvaderType.HarpoonCannon:
                            {
                                DeactivateProjectile(heavyProjectile);
                                (sourceInvader as HarpoonCannon).Rope.TetherProjectile = null;
                                (sourceInvader as HarpoonCannon).HarpoonedTurret = collisionTurret;
                                (sourceInvader as HarpoonCannon).Rope.Sticks[0].Point1.CurrentPosition = collisionTurret.Position;

                            }
                            break;
                        #endregion

                        #region Stationary Cannon
                        case InvaderType.StationaryCannon:
                            {
                                DeactivateProjectile(heavyProjectile);
                            }
                            break;
                        #endregion

                        #region Archer
                        case InvaderType.Archer:
                            {

                            }
                            break;
                        #endregion

                        case InvaderType.GunShip:
                            {
                                DeactivateProjectile(heavyProjectile);
                            }
                            break;
                    }

                    //DeactivateProjectile(heavyProjectile);
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
                                for (int i = 0; i < Random.Next(4, 10); i++)
                                {
                                    Emitter SparkEmitter2 = new Emitter(RoundSparkParticle, heavyProjectile.Position,
                                    new Vector2(0, 360), new Vector2(0.5f, 1.5f), new Vector2(480, 960), 100f, true, new Vector2(0, 360),
                                    new Vector2(1, 3), new Vector2(0.1f, 0.3f), Color.DarkBlue, Color.Aquamarine, 0.05f, 0.5f, 1, 1, true,
                                    new Vector2(Tower.DestinationRectangle.Bottom, Tower.DestinationRectangle.Bottom),
                                    false, null, true, true, new Vector2(4, 8),
                                    new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(heavyProjectile.Velocity.Y, heavyProjectile.Velocity.X)) - 180 - (float)RandomDouble(0, 90),
                                                MathHelper.ToDegrees(-(float)Math.Atan2(heavyProjectile.Velocity.Y, heavyProjectile.Velocity.X)) - 180 + (float)RandomDouble(0, 45)),
                                                0.2f, false, new Vector2(0.25f, 0), null, null, null, true, null);

                                    AdditiveEmitterList.Add(SparkEmitter2);
                                }

                                Tower.TakeDamage(sourceInvader.RangedDamage);
                                DeactivateProjectile(heavyProjectile);

                            }
                            break;
                        #endregion

                        #region Archer
                        case InvaderType.Archer:
                            {
                                DeactivateProjectile(heavyProjectile);

                            }
                            break;
                        #endregion
                    }

                    return;
                }
                #endregion

                #region Projectile Outside Bounds
                if ((Rectangle)e.collisionObject == ScreenRectangle)
                {
                    //switch ((source as Invader).InvaderType)
                    //{
                    //    case InvaderType.HarpoonCannon:
                    //        {

                    //        }
                    //        break;
                    //}

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
                        for (int i = 0; i < 1; i++)
                        {
                            Vector2 thing = CollisionStart - CollisionEnd;

                            int numSeg = (int)MathHelper.Clamp(GetEven((int)thing.Length() / 10), 4, (float)double.PositiveInfinity);
                            LightningList.Clear();
                            ToonLightning newLightning = new ToonLightning(numSeg, 15, CollisionStart, CollisionEnd, new Vector2(80, 100), new Color(Color.LimeGreen.R, Color.LimeGreen.G, Color.LimeGreen.B, 80));
                            newLightning.DrawDepth = (CollisionEnd.Y / 1080f);
                            newLightning.Color = Color.LimeGreen;
                            ToonLightningList.Add(newLightning);
                            AddDrawable(newLightning);
                            //LightningList.Add(newLightning);   

                            //ToonLightning lightning = new ToonLightning(
                            //Lightning = new LightningBolt(CollisionStart, CollisionEnd, Color.MediumPurple, 0.02f, 150f, false);
                            //LightningList.Add(Lightning);
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
                                Turret.TotalDamageDone += Turret.Damage;
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
                                Turret.TotalDamageDone += FinalDamage;
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
                            Turret.TotalDamageDone += Turret.Damage;
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
                                hitInvader.Freeze(new FreezeStruct() { MaxDelay = 6000 }, Color.SkyBlue);

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

                            case InvaderType.DropShip:

                                break;                                
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
                                    Emitter BloodEmitter = new Emitter(ToonBloodDrip1,
                                    new Vector2(hitInvader.Center.X, collisionEnd.Y),
                                    new Vector2(
                                    MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - (float)RandomDouble(0, 45),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) + (float)RandomDouble(0, 45)),
                                    new Vector2(1, 2), new Vector2(3000, 6000), 1f, false, new Vector2(0, 360),
                                    new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.White, Color.White,
                                    0.1f, 0.1f, 10, 5, true, new Vector2(hitInvader.MaxY - 5, hitInvader.MaxY + 5), true, (hitInvader.DestinationRectangle.Bottom + 1)/1080f, true, false,
                                    null, null, null, true, null, null, null, 500);

                                    Emitter BloodEmitter2 = new Emitter(ToonBloodDrip1,
                                    new Vector2(hitInvader.Center.X, collisionEnd.Y),
                                    new Vector2(
                                    MathHelper.ToDegrees(-(float)Math.Atan2(-CurrentProjectile.Ray.Direction.Y, -CurrentProjectile.Ray.Direction.X)) - (float)RandomDouble(0, 45),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(-CurrentProjectile.Ray.Direction.Y, -CurrentProjectile.Ray.Direction.X)) + (float)RandomDouble(0, 45)),
                                    new Vector2(1, 3), new Vector2(1800, 3600), 1f, false, new Vector2(0, 360),
                                    new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.White, Color.White,
                                    0.1f, 0.1f, 10, 5, true, new Vector2(hitInvader.MaxY - 5, hitInvader.MaxY + 5), true, (hitInvader.DestinationRectangle.Bottom + 1) / 1080f, true, false,
                                    null, null, null, true, null, null, null, 500);

                                    YSortedEmitterList.Add(BloodEmitter);
                                    YSortedEmitterList.Add(BloodEmitter2);

                                    AddDrawable(BloodEmitter, BloodEmitter2);
                                    break;
                                #endregion

                                #region HarpoonCannon
                                case InvaderType.HarpoonCannon:
                                    {
                                        Emitter BOOMEmitter = SnapPing(collisionEnd);

                                        YSortedEmitterList.Add(BOOMEmitter);

                                        Emitter hitEmitter = new Emitter(HitEffectParticle, collisionEnd,
                                                new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - 45,
                                                            MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + 45), new Vector2(5, 8), new Vector2(250, 500), 1f, false,
                                                new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                                Color.White, Color.Yellow, 0f, 0.05f, 50, 10, false, new Vector2(0, 1080), true,
                                                1.0f, null, null, null, null, null, true, new Vector2(0.2f, 0.2f), false, false,
                                                null, false, false, false);

                                        YSortedEmitterList.Add(hitEmitter);

                                        AddDrawable(hitEmitter, BOOMEmitter);
                                    }
                                    break; 
                                #endregion

                                #region BatteringRam
                                case InvaderType.BatteringRam:
                                    {
                                        Emitter BOOMEmitter = SnapPing(collisionEnd);

                                        YSortedEmitterList.Add(BOOMEmitter);

                                        Emitter hitEmitter = new Emitter(HitEffectParticle, collisionEnd,
                                                new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - 45,
                                                            MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + 45), new Vector2(5, 8), new Vector2(250, 500), 1f, false,
                                                new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                                Color.White, Color.Yellow, 0f, 0.05f, 50, 10, false, new Vector2(0, 1080), true,
                                                1.0f, null, null, null, null, null, true, new Vector2(0.2f, 0.2f), false, false,
                                                null, false, false, false);
                                        hitEmitter.Emissive = true;

                                        Emitter Sparks2 = new Emitter(RoundSparkParticle, collisionEnd,
                                            new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - 25,
                                                        MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + 25), new Vector2(0.7f, 4.05f),
                                            new Vector2(1016f, 2484f), 1f, true, new Vector2(-2f, 2f), new Vector2(-1f, 1f),
                                            new Vector2(0.15f, 0.15f), new Color(254, 197, 1, 78), new Color(255, 128, 0, 9),
                                            0.102f, 0.09f, 18f, 12, true, new Vector2(hitInvader.DestinationRectangle.Bottom + 4, hitInvader.DestinationRectangle.Bottom + 16), true, hitInvader.DrawDepth + (1f / 1080f), true, true,
                                            new Vector2(0f, 0f), new Vector2(0f, 0f), 0f, true, new Vector2(0f, 0f),
                                            false, false, 0f, false, false, false, null);
                                        Sparks2.Emissive = true;

                                        YSortedEmitterList.Add(hitEmitter);
                                        YSortedEmitterList.Add(Sparks2);

                                        AddDrawable(hitEmitter, BOOMEmitter, Sparks2);
                                    }
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
            Action<Vector2, object, Trap> TrapEffect = (Vector2 collisionEnd, object sourceObject, Trap Trap) =>
            {
                Turret turret = sourceObject as Turret;
                Invader invader = sourceObject as Invader;

                #region Source of projectile was a TURRET
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
                                        Emitter BOOMEmitter = SnapPing(collisionEnd);
                                        //BOOMEmitter.Emissive = true;
                                        YSortedEmitterList.Add(BOOMEmitter);

                                        Emitter DustEmitter = new Emitter(ToonSmoke3,
                                               new Vector2(collisionEnd.X, collisionEnd.Y),
                                               new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                                           MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                               new Vector2(1.8f, 3f), new Vector2(1120, 1600), 1f, false, new Vector2(0, 360),
                                               new Vector2(0.5f, 1f), new Vector2(0.02f, 0.05f), Color.Gray, Color.Gray, 0.03f, 0.02f, 5, 1, true,
                                               new Vector2(0, 1080), true, (Trap.DestinationRectangle.Bottom + 8)/1080f,
                                               null, null, null, null, null, null, new Vector2(0.08f, 0.08f), true, true);
                                        YSortedEmitterList.Add(DustEmitter);

                                        Emitter hitEmitter = new Emitter(HitEffectParticle, collisionEnd,
                                                    new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - 45,
                                                                MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + 45),
                                                    new Vector2(5, 8), new Vector2(250, 500), 1f, false,
                                                    new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                                    Color.White, Color.LightYellow, 0f, 0.05f, 50, 5, false, new Vector2(0, 1080), true,
                                                    Trap.DrawDepth + (4f / 1080f), null, null, null, null, null, true, new Vector2(0.2f, 0.2f), false, false,
                                                    null, false, false, false);
                                        hitEmitter.Emissive = true;

                                        YSortedEmitterList.Add(hitEmitter);
                                        AddDrawable(DustEmitter, hitEmitter, BOOMEmitter);
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

                #region Source of projectile was an INVADER
                if (invader != null)
                {
                    switch (invader.InvaderType)
                    {
                        default:
                            Trap.CurrentHP -= projectile.Damage;

                            switch (Trap.TrapType)
                            {
                                #region Wall
                                case TrapType.Wall:
                                    {
                                        Emitter Emitter = new Emitter(SplodgeParticle, collisionEnd,
                                            new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                                        MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                            new Vector2(1, 2), new Vector2(800, 1600), 0.5f, true, new Vector2(0, 360),
                                            new Vector2(1, 3), new Vector2(0.02f, 0.05f), Color.Gray, Color.DarkGray,
                                            0.1f, 0.1f, 10, 2, true, new Vector2(Trap.DestinationRectangle.Bottom, Trap.DestinationRectangle.Bottom), false, 1, true, false);
                                        YSortedEmitterList.Add(Emitter);

                                        //Need to calculate the reflection angle better. 
                                        for (int i = 0; i < Random.Next(2, 8); i++)
                                        {
                                            Emitter SparkEmitter2 = new Emitter(RoundSparkParticle, collisionEnd,
                                            new Vector2(0, 360), new Vector2(0.5f, 1.5f), new Vector2(480, 960), 100f, true, new Vector2(0, 360),
                                            new Vector2(1, 3), new Vector2(0.1f, 0.3f), Color.Orange, Color.OrangeRed, 0.05f, 0.5f, 10, 1, true,
                                            new Vector2(Trap.DestinationRectangle.Bottom, Trap.DestinationRectangle.Bottom),
                                            false, null, true, true, new Vector2(2, 5),
                                            new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                                        MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                                        0.6f, false, new Vector2(0.5f, 0), null, null, null, true, null);

                                            AdditiveEmitterList.Add(SparkEmitter2);
                                        }

                                        Emitter DustEmitter = new Emitter(SmokeParticle,
                                               new Vector2(collisionEnd.X, collisionEnd.Y),
                                               new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                                           MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                               new Vector2(1.8f, 3f), new Vector2(1120, 1600), 0.5f, true, new Vector2(0, 360),
                                               new Vector2(0.5f, 1f), new Vector2(0.2f, 0.5f), DirtColor2, DirtColor, 0.03f, 0.02f, 5, 1, false,
                                               new Vector2(0, 1080), true, collisionEnd.Y / 1080,
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
                        //Emitter DebrisEmitter = new Emitter(SplodgeParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                        //                                    new Vector2(60, 120), new Vector2(2, 4), new Vector2(320, 640), 1f, false,
                        //                                    new Vector2(0, 0), new Vector2(0, 0),
                        //                                    new Vector2(0.02f, 0.04f), Color.DarkSlateGray, Color.DarkSlateGray, 0.2f,
                        //                                    0.1f, 5, 1, true, new Vector2(CollisionEnd.Y + 8, CollisionEnd.Y + 16), false, (CollisionEnd.Y-4) / 1080,
                        //                                    true, true, null, null, null, null, null, true, true, 50f, false);
                        //YSortedEmitterList.Add(DebrisEmitter);

                        //Emitter DebrisEmitter2 = new Emitter(SplodgeParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                        //                                    new Vector2(80, 100), new Vector2(3, 5), new Vector2(320, 640), 1f, false,
                        //                                    new Vector2(0, 0), new Vector2(0, 0),
                        //                                    new Vector2(0.01f, 0.03f), Color.DarkSlateGray, Color.DarkSlateGray, 0.2f,
                        //                                    0.1f, 2, 3, true, new Vector2(CollisionEnd.Y + 8, CollisionEnd.Y + 16), false, (CollisionEnd.Y-4) / 1080,
                        //                                    true, true, null, null, null, null, null, true, true, 50f, false);
                        //YSortedEmitterList.Add(DebrisEmitter2);

                        Emitter DustEmitter = new Emitter(ToonDust3,
                                       new Vector2(CollisionEnd.X, CollisionEnd.Y + 2),
                                       new Vector2(70, 110), new Vector2(1f, 4f), new Vector2(1120, 1600), 1f, false, new Vector2(-10, 10),
                                       new Vector2(0f, 0f), new Vector2(0.03f, 0.04f), Color.White, Color.White, 0.03f, 0.02f, 5, 3, false,
                                       new Vector2(CollisionEnd.Y + 2, CollisionEnd.Y + 2), true, CollisionEnd.Y / 1080.0f,
                                       null, null, null, null, null, null, new Vector2(0.08f, 0.08f), false, false,
                                       null, null, null, null);

                        //if ((CollisionEnd.Y / 1080.0f) < 0.5f)
                        //{
                        //    int stop = 0;
                        //}

                        YSortedEmitterList.Add(DustEmitter);

                        AddDrawable(DustEmitter);//, DebrisEmitter, DebrisEmitter2);

                        Decal NewDecal = new Decal(ExplosionDecal1, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                                   (float)RandomDouble(0, 0), 0.2f);

                        DecalList.Add(NewDecal);
                        break;
                    #endregion

                    #region Lightning
                    case LightProjectileType.Lightning:
                        Emitter hitEmitter = new Emitter(HitEffectParticle, CollisionEnd, new Vector2(0, 180),
                                                    new Vector2(5, 8), new Vector2(250, 500), 1f, false,
                                                    new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                                    Color.White, Color.LightYellow, 0f, 0.05f, 50, 15, false, new Vector2(0, 1080), true,
                                                    (CollisionEnd.Y + 4)/ 1080f, null, null, null, null, null, true, new Vector2(0.2f, 0.2f), false, false,
                                                    null, false, false, false);
                        hitEmitter.Emissive = true;

                                        YSortedEmitterList.Add(hitEmitter);
                                        AddDrawable(hitEmitter);
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
            Action<Vector2, object, Shield> ShieldEffect = (Vector2 CollisionEnd, object sourceObject, Shield shield) =>
            {
                #region INVADER fired
                if (sourceObject.GetType().BaseType == typeof(LightRangedInvader))
                {
                    Tower.TakeDamage(projectile.Damage);

                    Emitter hitEmitter = new Emitter(HitEffectParticle, CollisionEnd,
                                                new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - 45,
                                                            MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + 45), new Vector2(5, 8), new Vector2(250, 500), 1f, false,
                                                new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                                Color.White, Color.White, 0f, 0.05f, 50, 10, false, new Vector2(0, 1080), true,
                                                1.0f, null, null, null, null, null, true, new Vector2(0.2f, 0.2f), false, false,
                                                null, false, false, false);
                    hitEmitter.Emissive = true;

                    Emitter Sparks2 = new Emitter(RoundSparkParticle, CollisionEnd,
                        new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - 25,
                                    MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + 25), new Vector2(0.7f, 4.05f),
                        new Vector2(1016f, 2484f), 1f, true, new Vector2(-2f, 2f), new Vector2(-1f, 1f),
                        new Vector2(0.15f, 0.15f), new Color(254, 197, 1, 78), new Color(255, 128, 0, 9),
                        0.102f, 0.09f, 18f, 12, true, new Vector2(Tower.DestinationRectangle.Bottom + 4, Tower.DestinationRectangle.Bottom + 16), true, Tower.DestinationRectangle.Bottom / 1080f, true, true,
                        new Vector2(0f, 0f), new Vector2(0f, 0f), 0f, true, new Vector2(0f, 0f),
                        false, false, 0f, false, false, false, null);
                    Sparks2.Emissive = true;

                    YSortedEmitterList.Add(hitEmitter);
                    YSortedEmitterList.Add(Sparks2);

                    AddDrawable(hitEmitter, Sparks2);

                    //for (int i = 0; i < Random.Next(2, 8); i++)
                    //{
                    //    Emitter SparkEmitter2 = new Emitter(RoundSparkParticle, CollisionEnd,
                    //    new Vector2(0, 360), new Vector2(0.5f, 1.5f), new Vector2(480, 960), 100f, true, new Vector2(0, 360),
                    //    new Vector2(1, 3), new Vector2(0.1f, 0.3f), Color.AliceBlue, Color.Aquamarine, 0.05f, 0.5f, 10, 1, true,
                    //    new Vector2(Tower.DestinationRectangle.Bottom, Tower.DestinationRectangle.Bottom),
                    //    false, null, true, true, new Vector2(2, 5),
                    //    new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                    //                MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                    //                0.6f, false, new Vector2(0.5f, 0), null, null, null, true, null);

                    //    AdditiveEmitterList.Add(SparkEmitter2);
                    //}
                } 
                #endregion

                #region TURRET fired
                if (sourceObject.GetType().BaseType == typeof(Turret))
                {
                    if ((shield.Tether as Invader) != null)
                    {
                        Invader shieldSource = (shield.Tether as Invader);

                        switch (shieldSource.InvaderType)
                        {
                            #region ShieldGenerator
                            case InvaderType.ShieldGenerator:
                                {
                                    shield.TakeDamage(sourceTurret.Damage);
                                }
                                break;
                            #endregion
                        }

                        for (int i = 0; i < Random.Next(2, 8); i++)
                        {
                            Emitter SparkEmitter2 = new Emitter(RoundSparkParticle, CollisionEnd,
                            new Vector2(0, 360), new Vector2(0.5f, 1.5f), new Vector2(480, 960), 100f, true, new Vector2(0, 360),
                            new Vector2(1, 3), new Vector2(0.1f, 0.3f), Color.AliceBlue, Color.Aquamarine, 0.05f, 0.5f, 10, 1, true,
                            new Vector2(shield.DestinationRectangle.Bottom, shield.DestinationRectangle.Bottom + 4),
                            false, null, true, true, new Vector2(2, 5),
                            new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                        MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                        0.6f, false, new Vector2(0.5f, 0), null, null, null, true, null);

                            AdditiveEmitterList.Add(SparkEmitter2);
                        }
                    }
                } 
                #endregion
            };
            #endregion

            #region TURRET hit by a projectile
            Action<Vector2, object, Trap> TurretEffect = (Vector2 CollisionEnd, object sourceObject, Trap Trap) =>
            {

            };
            #endregion

            #region TOWER hit by a projectile
            Action<Vector2, object> TowerEffect = (Vector2 collisionEnd, object sourceObject) =>
            {
                Tower.TakeDamage(projectile.Damage);

                switch (projectile.LightProjectileType)
                {
                    case LightProjectileType.MachineGun:
                        {
                            Emitter BOOMEmitter = SnapPing(collisionEnd);
                            //BOOMEmitter.Emissive = true;
                            YSortedEmitterList.Add(BOOMEmitter);

                            Emitter DustEmitter = new Emitter(ToonSmoke3,
                                   new Vector2(collisionEnd.X, collisionEnd.Y),
                                   new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                               MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                   new Vector2(1.8f, 3f), new Vector2(1120, 1600), 1f, false, new Vector2(0, 360),
                                   new Vector2(0.5f, 1f), new Vector2(0.02f, 0.05f), Color.Gray, Color.Gray, 0.03f, 0.02f, 5, 1, true,
                                   new Vector2(0, 1080), true, (Tower.DestinationRectangle.Bottom + 8) / 1080f,
                                   null, null, null, null, null, null, new Vector2(0.08f, 0.08f), true, true);
                            YSortedEmitterList.Add(DustEmitter);

                            Emitter hitEmitter = new Emitter(HitEffectParticle, collisionEnd,
                                        new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - 45,
                                                    MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + 45),
                                        new Vector2(5, 8), new Vector2(250, 500), 1f, false,
                                        new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                        Color.White, Color.LightYellow, 0f, 0.05f, 50, 5, false, new Vector2(0, 1080), true,
                                        (Tower.DestinationRectangle.Bottom + 4f) / 1080f, null, null, null, null, null, true, new Vector2(0.2f, 0.2f), false, false,
                                        null, false, false, false);
                            hitEmitter.Emissive = true;

                            YSortedEmitterList.Add(hitEmitter);
                            AddDrawable(DustEmitter, hitEmitter, BOOMEmitter);
                        }
                        break;
                }
            };
            #endregion

    

            #region Check what the projectile actually hit - trap, invader, ground, nothing etc.
            Action<object> CheckCollision = (object projectileSource) =>
            {
                CurrentProjectile = projectile;
                Vector2 sourcePosition = projectile.Position;
                Vector2 CollisionEnd;

                List<Drawable> Intersections = DrawableList.FindAll(Drawable => Drawable.BoundingBox.Intersects(CurrentProjectile.Ray) != null);
                List<Drawable> SphereIntersections = DrawableList.FindAll(Drawable => Drawable.BoundingSphere.Intersects(CurrentProjectile.Ray) != null);

                SphereIntersections.RemoveAll(Drawable => Drawable.GetType() == typeof(Shield) && (Drawable as Shield).ShieldOn == false);
                Intersections.RemoveAll(Drawable => Drawable.GetType().BaseType == typeof(Trap) && (Drawable as Trap).Solid == false);

                Intersections.RemoveAll(Drawable => 
                                        Drawable.GetType().BaseType == typeof(Trap) && 
                                        (Drawable as Trap).TrapType == TrapType.Wall &&
                                        (CursorPosition.Y < (Drawable as Trap).CollisionBox.Min.Y ||
                                         CursorPosition.Y > (Drawable as Trap).CollisionBox.Max.Y));

                #region Make sure TURRETS can't shoot themselves or other TURRETS
                if (sourceTurret != null)
                {
                    Intersections.RemoveAll(Drawable => Drawable.GetType().BaseType == typeof(Turret));
                }                
                #endregion

                #region Make sure INVADERS can't shoot themselves or other INVADERS
                if (sourceInvader != null)
                {
                    Intersections.RemoveAll(Drawable =>
                                            Drawable.GetType().BaseType == typeof(LightRangedInvader) ||
                                            Drawable.GetType().BaseType == typeof(HeavyRangedInvader) ||
                                            Drawable.GetType().BaseType == typeof(Invader));

                    SphereIntersections.RemoveAll(Drawable =>
                                            Drawable.GetType().BaseType == typeof(LightRangedInvader) ||
                                            Drawable.GetType().BaseType == typeof(HeavyRangedInvader) ||
                                            Drawable.GetType().BaseType == typeof(Invader));
                } 
                #endregion

                #region What object was hit first
                Drawable HitBox = Intersections.OrderBy(Collision => Collision.BoundingBox.Intersects(CurrentProjectile.Ray)).FirstOrDefault();
                Drawable HitSphere = SphereIntersections.OrderBy(Collision => Collision.BoundingSphere.Intersects(CurrentProjectile.Ray)).FirstOrDefault();
                float? boxDist = 5000;
                float? sphereDist = 5000;

                if (HitSphere != null)
                    sphereDist = HitSphere.BoundingSphere.Intersects(CurrentProjectile.Ray);

                if (HitBox != null)
                    boxDist = HitBox.BoundingBox.Intersects(CurrentProjectile.Ray);

                Drawable hitObject;

                if (sphereDist < boxDist)
                {
                    hitObject = HitSphere;
                }
                else
                {
                    hitObject = HitBox;
                }
                #endregion

                Invader HitInvader = hitObject as Invader;
                Turret HitTurret = hitObject as Turret;
                Trap HitTrap = hitObject as Trap;
                PowerupDelivery HitPowerupDelivery = hitObject as PowerupDelivery;
                Shield HitShield = hitObject as Shield;

                //if (HitTrap != null && HitTrap.TrapType == TrapType.Wall)
                //{
                //    if (CursorPosition.Y < HitTrap.CollisionBox.Min.Y ||
                //        CursorPosition.Y > HitTrap.CollisionBox.Max.Y)
                //    {
                //        HitTrap = null;
                //    }
                //}

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
                        float? DistToShield = CurrentProjectile.Ray.Intersects(Tower.Shield.BoundingSphere);

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
                            ShieldEffect(CollisionEnd, source, Tower.Shield);
                            sourceInvader.HitObject = Tower.Shield;
                        } 
                        #endregion

                        #region TOWER was hit
                        if (DistanceList.Min() == DistToTower)
                        {
                            CreateEffect(sourcePosition, CollisionEnd, CurrentProjectile.LightProjectileType);
                            TowerEffect(CollisionEnd, source);
                            sourceInvader.HitObject = Tower;
                            //Tower.TakeDamage(CurrentProjectile.Damage);
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
                    #region Distance to collision intersection
                    float minDist;

                    if (hitObject.BoundingBox != new BoundingBox())
                    {
                        minDist = (float)hitObject.BoundingBox.Intersects(CurrentProjectile.Ray);
                    }
                    else
                    {
                        minDist = (float)hitObject.BoundingSphere.Intersects(CurrentProjectile.Ray);
                    }
                    #endregion

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
                            //BUG: DistToShield ENDED UP NULL HERE. CAUSED A CRASH. KEEP AN EYE ON IT
                            float? DistToShield = CurrentProjectile.Ray.Intersects(Tower.Shield.BoundingSphere);

                            if (DistToShield != null)
                            {
                                CollisionEnd = new Vector2(sourcePosition.X + (CurrentProjectile.Ray.Direction.X * (float)DistToShield),
                                                           sourcePosition.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToShield));

                                ShieldEffect(CollisionEnd, source, Tower.Shield);
                                sourceInvader.HitObject = HitTurret;
                            }
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

                    #region INVADER SHIELD was hit
                    if (HitShield != null)
                    {
                        CreateEffect(sourcePosition, CollisionEnd, CurrentProjectile.LightProjectileType);
                        ShieldEffect(CollisionEnd, source, HitShield);
                    }
                    #endregion
                }
            };
            #endregion
             
            CheckCollision(source);

            CurrentProjectile = null;
            projectile = null;
        }
        
        public void UpdateBeams()
        {
            //THIS IS FOR PERSISTENT BEAMS
            if (CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                LightProjectile projectile = CurrentBeam;
                LightRangedInvader sourceInvader = CurrentBeam.Source as LightRangedInvader;
                Turret sourceTurret = CurrentBeam.Source as Turret;
                sourceTurret.ChangeFireDirection();

                CurrentProjectile = projectile;

                CurrentBeam.Ray.Direction = new Vector3(sourceTurret.FireDirection.X, sourceTurret.FireDirection.Y, 0);
                CurrentBeam.Ray.Position = new Vector3(sourceTurret.BarrelEnd.X, sourceTurret.BarrelEnd.Y, 0);
                CurrentBeam.Position = new Vector2(sourceTurret.BarrelEnd.X, sourceTurret.BarrelEnd.Y);

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
                            for (int i = 0; i < 1; i++)
                            {
                                Vector2 thing = CollisionStart - CollisionEnd;

                                int numSeg = (int)MathHelper.Clamp(GetEven((int)thing.Length() / 10), 4, (float)double.PositiveInfinity);
                                LightningList.Clear();
                                ToonLightning newLightning = new ToonLightning(numSeg, 15, CollisionStart, CollisionEnd, new Vector2(80, 100), new Color(Color.LimeGreen.R, Color.LimeGreen.G, Color.LimeGreen.B, 80));
                                newLightning.DrawDepth = (CollisionEnd.Y / 1080f);
                                ToonLightningList.Add(newLightning);
                                AddDrawable(newLightning);
                                //LightningList.Add(newLightning);   

                                //ToonLightning lightning = new ToonLightning(
                                //Lightning = new LightningBolt(CollisionStart, CollisionEnd, Color.MediumPurple, 0.02f, 150f, false);
                                //LightningList.Add(Lightning);
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
                                        Emitter BloodEmitter = new Emitter(ToonBloodDrip1,
                                        new Vector2(hitInvader.Center.X, collisionEnd.Y),
                                        new Vector2(
                                        MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - (float)RandomDouble(0, 45),
                                        MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) + (float)RandomDouble(0, 45)),
                                        new Vector2(1, 2), new Vector2(3000, 6000), 1f, false, new Vector2(0, 360),
                                        new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.White, Color.White,
                                        0.1f, 0.1f, 10, 5, true, new Vector2(hitInvader.MaxY - 5, hitInvader.MaxY + 5), true, (hitInvader.DestinationRectangle.Bottom + 1) / 1080f, true, false,
                                        null, null, null, true, null, null, null, 500);

                                        Emitter BloodEmitter2 = new Emitter(ToonBloodDrip1,
                                        new Vector2(hitInvader.Center.X, collisionEnd.Y),
                                        new Vector2(
                                        MathHelper.ToDegrees(-(float)Math.Atan2(-CurrentProjectile.Ray.Direction.Y, -CurrentProjectile.Ray.Direction.X)) - (float)RandomDouble(0, 45),
                                        MathHelper.ToDegrees(-(float)Math.Atan2(-CurrentProjectile.Ray.Direction.Y, -CurrentProjectile.Ray.Direction.X)) + (float)RandomDouble(0, 45)),
                                        new Vector2(1, 3), new Vector2(1800, 3600), 1f, false, new Vector2(0, 360),
                                        new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.White, Color.White,
                                        0.1f, 0.1f, 10, 5, true, new Vector2(hitInvader.MaxY - 5, hitInvader.MaxY + 5), true, (hitInvader.DestinationRectangle.Bottom + 1) / 1080f, true, false,
                                        null, null, null, true, null, null, null, 500);

                                        YSortedEmitterList.Add(BloodEmitter);
                                        YSortedEmitterList.Add(BloodEmitter2);

                                        AddDrawable(BloodEmitter, BloodEmitter2);
                                        break;
                                    #endregion

                                    case InvaderType.HarpoonCannon:
                                        {
                                            Emitter BOOMEmitter = SnapPing(collisionEnd);

                                            YSortedEmitterList.Add(BOOMEmitter);

                                            Emitter hitEmitter = new Emitter(HitEffectParticle, collisionEnd,
                                                    new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - 45,
                                                                MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + 45), new Vector2(5, 8), new Vector2(250, 500), 1f, false,
                                                    new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                                    Color.White, Color.Yellow, 0f, 0.05f, 50, 10, false, new Vector2(0, 1080), true,
                                                    1.0f, null, null, null, null, null, true, new Vector2(0.2f, 0.2f), false, false,
                                                    null, false, false, false);

                                            YSortedEmitterList.Add(hitEmitter);

                                            AddDrawable(hitEmitter, BOOMEmitter);
                                        }
                                        break;
                                }
                            }
                            break;
                        #endregion
                    }
                };
                #endregion

                #region TRAP hit by a projectile
                Action<Vector2, object, Trap> TrapEffect = (Vector2 collisionEnd, object sourceObject, Trap Trap) =>
                {
                    Turret turret = sourceObject as Turret;
                    Invader invader = sourceObject as Invader;

                    #region Source of projectile was a TURRET
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
                                            Emitter BOOMEmitter = SnapPing(collisionEnd);
                                            YSortedEmitterList.Add(BOOMEmitter);

                                            Emitter DustEmitter = new Emitter(ToonSmoke3,
                                                   new Vector2(collisionEnd.X, collisionEnd.Y),
                                                   new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                                               MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                                   new Vector2(1.8f, 3f), new Vector2(1120, 1600), 1f, false, new Vector2(0, 360),
                                                   new Vector2(0.5f, 1f), new Vector2(0.02f, 0.05f), Color.Gray, Color.Gray, 0.03f, 0.02f, 5, 1, true,
                                                   new Vector2(0, 1080), true, (Trap.DestinationRectangle.Bottom + 8) / 1080f,
                                                   null, null, null, null, null, null, new Vector2(0.08f, 0.08f), true, true);
                                            YSortedEmitterList.Add(DustEmitter);

                                            Emitter hitEmitter = new Emitter(HitEffectParticle, collisionEnd,
                                                        new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - 45,
                                                                    MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + 45),
                                                        new Vector2(5, 8), new Vector2(250, 500), 1f, false,
                                                        new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                                        Color.White, Color.LightYellow, 0f, 0.05f, 50, 5, false, new Vector2(0, 1080), true,
                                                        Trap.DrawDepth + (4f / 1080f), null, null, null, null, null, true, new Vector2(0.2f, 0.2f), false, false,
                                                        null, false, false, false);
                                            YSortedEmitterList.Add(hitEmitter);
                                            AddDrawable(DustEmitter, hitEmitter, BOOMEmitter);
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

                    #region Source of projectile was an INVADER
                    if (invader != null)
                    {
                        switch (invader.InvaderType)
                        {
                            default:
                                Trap.CurrentHP -= projectile.Damage;

                                switch (Trap.TrapType)
                                {
                                    #region Wall
                                    case TrapType.Wall:
                                        {
                                            Emitter Emitter = new Emitter(SplodgeParticle, collisionEnd,
                                                new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                                            MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                                new Vector2(1, 2), new Vector2(800, 1600), 0.5f, true, new Vector2(0, 360),
                                                new Vector2(1, 3), new Vector2(0.02f, 0.05f), Color.Gray, Color.DarkGray,
                                                0.1f, 0.1f, 10, 2, true, new Vector2(Trap.DestinationRectangle.Bottom, Trap.DestinationRectangle.Bottom), false, 1, true, false);
                                            YSortedEmitterList.Add(Emitter);

                                            //Need to calculate the reflection angle better. 
                                            for (int i = 0; i < Random.Next(2, 8); i++)
                                            {
                                                Emitter SparkEmitter2 = new Emitter(RoundSparkParticle, collisionEnd,
                                                new Vector2(0, 360), new Vector2(0.5f, 1.5f), new Vector2(480, 960), 100f, true, new Vector2(0, 360),
                                                new Vector2(1, 3), new Vector2(0.1f, 0.3f), Color.Orange, Color.OrangeRed, 0.05f, 0.5f, 10, 1, true,
                                                new Vector2(Trap.DestinationRectangle.Bottom, Trap.DestinationRectangle.Bottom),
                                                false, null, true, true, new Vector2(2, 5),
                                                new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                                            MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                                            0.6f, false, new Vector2(0.5f, 0), null, null, null, true, null);

                                                AdditiveEmitterList.Add(SparkEmitter2);
                                            }

                                            Emitter DustEmitter = new Emitter(SmokeParticle,
                                                   new Vector2(collisionEnd.X, collisionEnd.Y),
                                                   new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                                               MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                                   new Vector2(1.8f, 3f), new Vector2(1120, 1600), 0.5f, true, new Vector2(0, 360),
                                                   new Vector2(0.5f, 1f), new Vector2(0.2f, 0.5f), DirtColor2, DirtColor, 0.03f, 0.02f, 5, 1, false,
                                                   new Vector2(0, 1080), true, collisionEnd.Y / 1080,
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
                            //Emitter DebrisEmitter = new Emitter(SplodgeParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                            //                                    new Vector2(60, 120), new Vector2(2, 4), new Vector2(320, 640), 1f, false,
                            //                                    new Vector2(0, 0), new Vector2(0, 0),
                            //                                    new Vector2(0.02f, 0.04f), Color.DarkSlateGray, Color.DarkSlateGray, 0.2f,
                            //                                    0.1f, 5, 1, true, new Vector2(CollisionEnd.Y + 8, CollisionEnd.Y + 16), false, (CollisionEnd.Y-4) / 1080,
                            //                                    true, true, null, null, null, null, null, true, true, 50f, false);
                            //YSortedEmitterList.Add(DebrisEmitter);

                            //Emitter DebrisEmitter2 = new Emitter(SplodgeParticle, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                            //                                    new Vector2(80, 100), new Vector2(3, 5), new Vector2(320, 640), 1f, false,
                            //                                    new Vector2(0, 0), new Vector2(0, 0),
                            //                                    new Vector2(0.01f, 0.03f), Color.DarkSlateGray, Color.DarkSlateGray, 0.2f,
                            //                                    0.1f, 2, 3, true, new Vector2(CollisionEnd.Y + 8, CollisionEnd.Y + 16), false, (CollisionEnd.Y-4) / 1080,
                            //                                    true, true, null, null, null, null, null, true, true, 50f, false);
                            //YSortedEmitterList.Add(DebrisEmitter2);

                            Emitter DustEmitter = new Emitter(ToonDust3,
                                           new Vector2(CollisionEnd.X, CollisionEnd.Y + 2),
                                           new Vector2(70, 110), new Vector2(1f, 4f), new Vector2(1120, 1600), 1f, false, new Vector2(-10, 10),
                                           new Vector2(0f, 0f), new Vector2(0.03f, 0.04f), Color.White, Color.White, 0.03f, 0.02f, 5, 3, false,
                                           new Vector2(CollisionEnd.Y + 2, CollisionEnd.Y + 2), true, CollisionEnd.Y / 1080.0f,
                                           null, null, null, null, null, null, new Vector2(0.08f, 0.08f), false, false,
                                           null, null, null, null);

                            //if ((CollisionEnd.Y / 1080.0f) < 0.5f)
                            //{
                            //    int stop = 0;
                            //}

                            YSortedEmitterList.Add(DustEmitter);

                            AddDrawable(DustEmitter);//, DebrisEmitter, DebrisEmitter2);

                            Decal NewDecal = new Decal(ExplosionDecal1, new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                                       (float)RandomDouble(0, 0), 0.2f);

                            DecalList.Add(NewDecal);
                            break;
                        #endregion

                        #region Lightning
                        case LightProjectileType.Lightning:
                            Emitter hitEmitter = new Emitter(HitEffectParticle, CollisionEnd, new Vector2(0, 180),
                                                        new Vector2(5, 8), new Vector2(250, 500), 1f, false,
                                                        new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                                                        Color.White, Color.LightYellow, 0f, 0.05f, 50, 15, false, new Vector2(0, 1080), true,
                                                        (CollisionEnd.Y + 4) / 1080f, null, null, null, null, null, true, new Vector2(0.2f, 0.2f), false, false,
                                                        null, false, false, false);
                            YSortedEmitterList.Add(hitEmitter);
                            AddDrawable(hitEmitter);
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
                Action<Vector2, object, Shield> ShieldEffect = (Vector2 CollisionEnd, object sourceObject, Shield shield) =>
                {
                    #region INVADER fired
                    if (sourceObject.GetType().BaseType == typeof(LightRangedInvader))
                    {
                        Tower.TakeDamage(projectile.Damage);

                        for (int i = 0; i < Random.Next(2, 8); i++)
                        {
                            Emitter SparkEmitter2 = new Emitter(RoundSparkParticle, CollisionEnd,
                            new Vector2(0, 360), new Vector2(0.5f, 1.5f), new Vector2(480, 960), 100f, true, new Vector2(0, 360),
                            new Vector2(1, 3), new Vector2(0.1f, 0.3f), Color.AliceBlue, Color.Aquamarine, 0.05f, 0.5f, 10, 1, true,
                            new Vector2(Tower.DestinationRectangle.Bottom, Tower.DestinationRectangle.Bottom),
                            false, null, true, true, new Vector2(2, 5),
                            new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                        MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                        0.6f, false, new Vector2(0.5f, 0), null, null, null, true, null);

                            AdditiveEmitterList.Add(SparkEmitter2);
                        }
                    }
                    #endregion

                    #region TURRET fired
                    if (sourceObject.GetType().BaseType == typeof(Turret))
                    {
                        if ((shield.Tether as Invader) != null)
                        {
                            Invader shieldSource = (shield.Tether as Invader);

                            switch (shieldSource.InvaderType)
                            {
                                #region ShieldGenerator
                                case InvaderType.ShieldGenerator:
                                    {
                                        shield.TakeDamage(sourceTurret.Damage);
                                    }
                                    break;
                                #endregion
                            }

                            for (int i = 0; i < Random.Next(2, 8); i++)
                            {
                                Emitter SparkEmitter2 = new Emitter(RoundSparkParticle, CollisionEnd,
                                new Vector2(0, 360), new Vector2(0.5f, 1.5f), new Vector2(480, 960), 100f, true, new Vector2(0, 360),
                                new Vector2(1, 3), new Vector2(0.1f, 0.3f), Color.AliceBlue, Color.Aquamarine, 0.05f, 0.5f, 10, 1, true,
                                new Vector2(shield.DestinationRectangle.Bottom, shield.DestinationRectangle.Bottom + 4),
                                false, null, true, true, new Vector2(2, 5),
                                new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                            MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                            0.6f, false, new Vector2(0.5f, 0), null, null, null, true, null);

                                AdditiveEmitterList.Add(SparkEmitter2);
                            }
                        }
                    }
                    #endregion
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
                    List<Drawable> SphereIntersections = DrawableList.FindAll(Drawable => Drawable.BoundingSphere.Intersects(CurrentProjectile.Ray) != null);

                    SphereIntersections.RemoveAll(Drawable => Drawable.GetType() == typeof(Shield) && (Drawable as Shield).ShieldOn == false);
                    Intersections.RemoveAll(Drawable => Drawable.GetType().BaseType == typeof(Trap) && (Drawable as Trap).Solid == false);

                    #region Make sure TURRETS can't shoot themselves or other TURRETS
                    if (sourceTurret != null)
                    {
                        Intersections.RemoveAll(Drawable => Drawable.GetType().BaseType == typeof(Turret));
                    }
                    #endregion

                    #region Make sure INVADERS can't shoot themselves or other INVADERS
                    if (sourceInvader != null)
                    {
                        Intersections.RemoveAll(Drawable =>
                                                Drawable.GetType().BaseType == typeof(LightRangedInvader) ||
                                                Drawable.GetType().BaseType == typeof(HeavyRangedInvader) ||
                                                Drawable.GetType().BaseType == typeof(Invader));

                        SphereIntersections.RemoveAll(Drawable =>
                                                Drawable.GetType().BaseType == typeof(LightRangedInvader) ||
                                                Drawable.GetType().BaseType == typeof(HeavyRangedInvader) ||
                                                Drawable.GetType().BaseType == typeof(Invader));
                    }
                    #endregion

                    #region What object was hit first
                    Drawable HitBox = Intersections.OrderBy(Collision => Collision.BoundingBox.Intersects(CurrentProjectile.Ray)).FirstOrDefault();
                    Drawable HitSphere = SphereIntersections.OrderBy(Collision => Collision.BoundingSphere.Intersects(CurrentProjectile.Ray)).FirstOrDefault();
                    float? boxDist = 5000;
                    float? sphereDist = 5000;

                    if (HitSphere != null)
                        sphereDist = HitSphere.BoundingSphere.Intersects(CurrentProjectile.Ray);

                    if (HitBox != null)
                        boxDist = HitBox.BoundingBox.Intersects(CurrentProjectile.Ray);

                    Drawable hitObject;

                    if (sphereDist < boxDist)
                    {
                        hitObject = HitSphere;
                    }
                    else
                    {
                        hitObject = HitBox;
                    }
                    #endregion

                    Invader HitInvader = hitObject as Invader;
                    Turret HitTurret = hitObject as Turret;
                    Trap HitTrap = hitObject as Trap;
                    PowerupDelivery HitPowerupDelivery = hitObject as PowerupDelivery;
                    Shield HitShield = hitObject as Shield;

                    if (hitObject == null)
                    {
                        float? DistToGround = CurrentProjectile.Ray.Intersects(Ground.BoundingBox);

                        #region TURRET fired the projectile
                        if (sourceTurret != null)
                        {
                            if (DistToGround != null)
                            {
                                CollisionEnd = new Vector2(sourcePosition.X + (CurrentProjectile.Ray.Direction.X * (float)DistToGround),
                                                           Random.Next(0, 0) + sourcePosition.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToGround));

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
                            float? DistToShield = CurrentProjectile.Ray.Intersects(Tower.Shield.BoundingSphere);

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
                                ShieldEffect(CollisionEnd, CurrentBeam.Source, Tower.Shield);
                                sourceInvader.HitObject = Tower.Shield;
                            }
                            #endregion

                            #region TOWER was hit
                            if (DistanceList.Min() == DistToTower)
                            {
                                CreateEffect(sourcePosition, CollisionEnd, CurrentProjectile.LightProjectileType);
                                sourceInvader.HitObject = Tower;
                                Tower.TakeDamage(CurrentProjectile.Damage);
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
                        #region Distance to collision intersection
                        float minDist;

                        if (hitObject.BoundingBox != new BoundingBox())
                        {
                            minDist = (float)hitObject.BoundingBox.Intersects(CurrentProjectile.Ray);
                        }
                        else
                        {
                            minDist = (float)hitObject.BoundingSphere.Intersects(CurrentProjectile.Ray);
                        }
                        #endregion

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
                                float? DistToShield = CurrentProjectile.Ray.Intersects(Tower.Shield.BoundingSphere);
                                CollisionEnd = new Vector2(sourcePosition.X + (CurrentProjectile.Ray.Direction.X * (float)DistToShield),
                                                           sourcePosition.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToShield));

                                ShieldEffect(CollisionEnd, CurrentBeam.Source, Tower.Shield);
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

                        #region INVADER SHIELD was hit
                        if (HitShield != null)
                        {
                            CreateEffect(sourcePosition, CollisionEnd, CurrentProjectile.LightProjectileType);
                            ShieldEffect(CollisionEnd, CurrentBeam.Source, HitShield);
                        }
                        #endregion
                    }
                };
                #endregion

                CheckCollision(CurrentBeam.Source);
            }
            else
            {
                CurrentBeam = null;
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
                        heavyProjectile = new ClusterBomb(source, ClusterBombShellProjectileSprite, SmokeParticle, position, speed, (float)RandomDouble(angleRange.X, angleRange.Y), 0.3f, 5, yRange);
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

                if (turret.Overheated == true && SmokeTrailList.All(Trail => Trail.TurretTether != turret))
                {
                    SmokeTrail trail = new SmokeTrail(turret.BarrelEnd);
                    trail.TurretTether = turret;
                    SmokeTrailList.Add(trail);
                }

                if (turret.Overheated == false)
                {
                    SmokeTrailList.ForEach(SmokeTrail =>
                        {
                            if (SmokeTrail.TurretTether == turret)
                            {
                                SmokeTrail.AddMore = false;
                            }
                        });
                }

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

                                //SHAKE CAMERA WHEN FIRING MACHINE GUN
                                //if (turret.Overheated == false)
                                //{
                                //    if (turret.TurretType == TurretType.MachineGun)
                                //    {
                                //        Camera.Shake(2, 1);
                                //    }
                                //}
                                
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

                if (turret.SelectBox.Contains(VectorToPoint(CursorPosition)))
                {
                    turret.TurretOutline.Visible = true;
                }
                else
                {
                    turret.TurretOutline.Visible = false;
                }

                #region What to do when the turret is destroyed
                //This should create an explosion effect and create verlet objects with
                //the barrel and base sprites
                if (turret.CurrentHealth <= 0)
                {
                    for (int i = 0; i < 15; i++)
                    {
                        Emitter MultiFire_Explosion5 = new Emitter(ExplosionParticle2, turret.Position,
                            new Vector2(-147f, -107f), new Vector2(1f, 1f), new Vector2(500f, 1500f), 1f, false,
                            new Vector2(-2f, 2f), new Vector2(-1f, 1f), new Vector2(0.0405f, 0.0525f),
                            new Color(255, 128, 0, 6), new Color(0, 0, 0, 255), 0f, 1f, 16f, 3, true,
                            new Vector2(1080f, 1080f), true, turret.BoundingBox.Max.Y / 1080f, true,
                            true, new Vector2(5f, 8f), new Vector2(20f, 160f), 0.2f, false,
                            new Vector2(0.1f, 0.2f), false, false, 0f, true, false, true, null);
                        MultiFire_Explosion5.Emissive = true;
                        YSortedEmitterList.Add(MultiFire_Explosion5);
                        AddDrawable(MultiFire_Explosion5);
                    }

                    Emitter HitEffect1 = new Emitter(HitEffectParticle, turret.Position, new Vector2(0f, 360f),
                        new Vector2(5f, 8f), new Vector2(250f, 500f), 1f, false, new Vector2(0f, 360f),
                        new Vector2(-2f, 2f), new Vector2(0.278f, 0.2915f), new Color(255, 255, 255, 255),
                        new Color(255, 255, 255, 255), 0f, 0.05f, 50f, 10, false, new Vector2(0f, 720f),
                        true, turret.BoundingBox.Max.Y / 1080f, false, false, new Vector2(0f, 0f),
                        new Vector2(0f, 0f), 0f, true, new Vector2(0.11f, 0.11f), false, false, 0f,
                        false, false, false, null);
                    HitEffect1.Emissive = true;
                    YSortedEmitterList.Add(HitEffect1);

                    Emitter Sparks1 = new Emitter(SparkParticle, turret.Position, new Vector2(0f, 360f),
                        new Vector2(2f, 4f), new Vector2(983f, 1500f), 1f, true, new Vector2(-2f, 2f),
                        new Vector2(-1f, 1f), new Vector2(1f, 1f), new Color(255, 255, 0, 0),
                        new Color(255, 128, 64, 48), 0.102f, 0.06f, 18f, 50, false, new Vector2(0f, 720f),
                        true, turret.BoundingBox.Max.Y / 1080f, false, false, new Vector2(0f, 0f),
                        new Vector2(0f, 0f), 0f, true, new Vector2(0f, 0f), false, false, 0f, false, false, false, null);
                    Sparks1.Emissive = true;
                    YSortedEmitterList.Add(Sparks1);

                    Emitter ExplosionSmoke1 = new Emitter(ToonSmoke3, turret.Position, new Vector2(60f, 120f),
                        new Vector2(1f, 1f), new Vector2(500f, 1000f), 1f, false, new Vector2(-10f, 10f),
                        new Vector2(-1f, 1f), new Vector2(0.05f, 0.06f), new Color(192, 192, 192, 255),
                        new Color(128, 128, 128, 255), -0.005f, 0.4f, 53f, 10, false, new Vector2(0f, 720f),
                        true, (turret.BoundingBox.Max.Y + 1) / 1080f, false, false, new Vector2(0f, 0f),
                        new Vector2(0f, 0f), 0f, false, new Vector2(0f, 0f), false, false, 0f,
                        false, false, true, null);
                    YSortedEmitterList.Add(ExplosionSmoke1);

                    Emitter ExplosionEmitter1 = new Emitter(ExplosionParticle2, turret.Position, new Vector2(20f, 160f),
                        new Vector2(0f, 0f), new Vector2(500f, 1500f), 1f, true, new Vector2(-2f, 2f),
                        new Vector2(-1f, 1f), new Vector2(0.15f, 0.15f), new Color(255, 128, 0, 3),
                        new Color(0, 0, 0, 255), -0.071f, 0.1f, 50f, 7, false, new Vector2(0f, 720f), false,
                        (turret.BoundingBox.Max.Y + 2) / 1080f, false, false, new Vector2(0f, 0f), new Vector2(0f, 0f),
                        0f, false, new Vector2(0.1f, 0.2f), false, false, 0f, false, false, true, null);
                    ExplosionEmitter1.Emissive = true;
                    YSortedEmitterList.Add(ExplosionEmitter1);

                    AddDrawable(HitEffect1, Sparks1, ExplosionSmoke1, ExplosionEmitter1);

                    switch (turret.TurretType)
                    {
                        #region Cannon
                        case TurretType.Cannon:
                            {


                            }
                            break;
                        #endregion

                        #region Machine Gun
                        case TurretType.MachineGun:
                            {
                                //#region Explosion Particle Effects
                                //Emitter ExplosionEmitter = new Emitter(ExplosionParticle2,
                                //                               turret.Position,
                                //                               new Vector2(20, 160), new Vector2(0.3f, 0.8f), new Vector2(500, 1000), 0.85f, true, new Vector2(-2, 2),
                                //                               new Vector2(-1, 1), new Vector2(0.2f, 0.4f), FireColor, Color.Black, -0.2f, 0.1f, 10, 1, false,
                                //                               new Vector2(turret.BoundingBox.Max.Y, turret.BoundingBox.Max.Y), false, turret.BoundingBox.Max.Y / 1080f,
                                //                               null, null, null, null, null, null, new Vector2(0.1f, 0.2f), true, true, null, null, null, true);
                                //YSortedEmitterList.Add(ExplosionEmitter);

                                //Emitter ExplosionEmitter3 = new Emitter(ExplosionParticle2,
                                //        turret.Position,
                                //        new Vector2(0, 360), new Vector2(1, 5), new Vector2(400, 640), 0.35f, true, new Vector2(0, 0),
                                //        new Vector2(0, 0), new Vector2(0.1f, 0.3f), FireColor, ExplosionColor3, -0.1f, 0.05f, 1, 1, false,
                                //        new Vector2(turret.BoundingBox.Max.Y, turret.BoundingBox.Max.Y), true, turret.BoundingBox.Max.Y / 1080f,
                                //        null, null, null, null, null, null, new Vector2(0.0025f, 0.0025f), true, true, 50);
                                //YSortedEmitterList.Add(ExplosionEmitter3);

                                //Emitter SparkEmitter = new Emitter(RoundSparkParticle,
                                //   turret.Position,
                                //   new Vector2(0, 360), new Vector2(1, 4), new Vector2(800, 1600), 1f, true, new Vector2(0, 0),
                                //   new Vector2(0, 0), new Vector2(0.1f, 0.3f), Color.LightYellow, Color.White, 0.1f, 0.1f, 1, 1, false,
                                //   new Vector2(0, 1080), null, (turret.BoundingBox.Max.Y) / 1080f);
                                //YSortedEmitterList.Add(SparkEmitter);

                                //#endregion

                                Explosion newExplosion = new Explosion(turret.Position, 200, 0);
                                CreateExplosion(newExplosion, turret);

                                ShellCasing turretBase = new ShellCasing(turret.Position, new Vector2(Random.Next(15, 25), Random.Next(-15, 15)), turret.TurretBase);
                                ShellCasing turretBarrel = new ShellCasing(turret.Position, new Vector2(Random.Next(15, 25), Random.Next(-15, 15)), MachineGunTurretBarrelGib);
                                VerletShells.Add(turretBase);
                                VerletShells.Add(turretBarrel);

                                AmmoBelt belt = turret.AmmoBelt;
                                belt.CurrentTime = 0;
                                belt.MaxTime = 6000f;
                                belt.Nodes[0].Pinned = false;
                                belt.Nodes2[0].Pinned = false;
                                AmmoBeltList.Add(belt);

                                AddDrawable(turretBase, turretBarrel, belt);
                            }
                            break;
                        #endregion
                    }

                    turret.Active = false;
                }
                #endregion

                #region Remove turret when middle clicked and refund resources
                if (turret.Active == true &&
                    turret.SelectBox.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)) &&
                    CurrentMouseState.MiddleButton == ButtonState.Released &&
                    PreviousMouseState.MiddleButton == ButtonState.Pressed &&
                    turret.CurrentHealth == turret.MaxHealth)
                {
                    //turret.CurrentHealth = 0;
                    Resources += TurretCost(turret.TurretType);
                    turret.Active = false;
                    TowerButtonList[TurretList.IndexOf(turret)].ButtonActive = true;
                    TurretList[TurretList.IndexOf(turret)] = null;
                    DrawableList.Remove(turret);
                    //Remove the outline from around the turret
                    //UITurretOutlineList.RemoveAll(uiOutline => uiOutline.Turret == turret);
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

            turret.ShotsFired++;

            //NEW_TURRET D **turret behaviour here**
            switch (turret.TurretType)
            {
                #region Machine Gun
                case TurretType.MachineGun:
                    {
                        MachineShot1.Play();

                        CurrentProfile.ShotsFired++;

                        CreateLightProjectile(new MachineGunProjectile(new Vector2(turret.BarrelEnd.X,
                                                                                   turret.BarrelEnd.Y), Direction), turret);

                        Emitter FlashEmitter = new Emitter(HitEffectParticle, new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                                        new Vector2(
                                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) - 30,
                                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) + 30),
                                                        new Vector2(8, 12), new Vector2(100, 200), 1f, false,
                                                        new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.25f, 0.25f),
                                                        new Color(Color.Yellow.R, Color.Yellow.G, Color.Yellow.B, 20), 
                                                        new Color(Color.Orange.R, Color.Orange.G, Color.Orange.B, 255), 0f, 0.05f, 100, 7, false, new Vector2(0, 1080), true,
                                                        1.0f, null, null, null, null, null, true, new Vector2(0.25f, 0.25f), false, false,
                                                        null, false, false, false);
                        FlashEmitter.Emissive = true;
                        YSortedEmitterList.Add(FlashEmitter);

                        Emitter FlashEmitter2 = new Emitter(HitEffectParticle, new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                                        new Vector2(
                                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) - 5,
                                                        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) + 5),
                                                        new Vector2(12, 15), new Vector2(80, 150), 1f, false,
                                                        new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.35f, 0.35f),
                                                        Color.LightYellow,
                                                        new Color(Color.Orange.R, Color.Orange.G, Color.Orange.B, 50), 0f, 0.05f, 100, 7, false, new Vector2(0, 1080), true,
                                                        1.0f, null, null, null, null, null, true, new Vector2(0.18f, 0.18f), false, false,
                                                        null, false, false, false);
                        FlashEmitter2.Emissive = true;
                        YSortedEmitterList.Add(FlashEmitter2);
                        
                        //Emitter puffEmitter = new Emitter(SmokeParticle, turret.BarrelEnd, new Vector2(
                        //                                MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) - 30,
                        //                                MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) + 30),
                        //                                  new Vector2(-2, -1), new Vector2(200, 400), 1f, false, new Vector2(0, 180), new Vector2(-0.5f, 0.5f),
                        //                                  new Vector2(0.25f, 0.5f), Color.DarkGray, Color.DarkGray, -0.00f, 0.05f, 1, 1, false, new Vector2(0, 1080), true, 2.0f,
                        //                                  null, null, null, null, null, null, null, true, true, 150f);
                        //YSortedEmitterList.Add(puffEmitter);

                        ShellCasing newShell = new ShellCasing(
                            new Vector2(turret.BarrelCenter.X, turret.BarrelCenter.Y), 
                            new Vector2(-Random.Next(10, 15), -Random.Next(20, 30)), 
                            SmallShellingCasing);
                        newShell.Initialize();
                        VerletShells.Add(newShell);

                        AddDrawable(newShell, FlashEmitter, FlashEmitter2);//, puffEmitter);

                        //turret.AmmoBelt.Sticks2.RemoveAt(turret.AmmoBelt.Sticks2.Count - 1);

                        for (int i = 0; i < (int)turret.AmmoBelt.Nodes2.Count / 2; i++)
                        {
                            turret.AmmoBelt.Nodes2[i].PreviousPosition.X += 8;
                            turret.AmmoBelt.Nodes[i].PreviousPosition.X += 8;

                            turret.AmmoBelt.Nodes2[i].PreviousPosition.Y -= 8;
                            turret.AmmoBelt.Nodes[i].PreviousPosition.Y -= 8;
                        }

                        turret.CurrentHeat += turret.ShotHeat;

                        //Camera.Shake(2, 2f);
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

                        //Emitter SmokeEmitter = new Emitter(ToonSmoke2,
                        //        new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                        //        new Vector2(
                        //        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)),
                        //        MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X))), new Vector2(4, 8),
                        //        new Vector2(480, 640), 1f, false, new Vector2(-45, 45), new Vector2(0, 0),
                        //        new Vector4(0.08f, 0.2f, 0.08f, 0.2f), SmokeColor1, SmokeColor2, 0f, 0.05f, 1, 1, false,
                        //        new Vector2(0, 1080), true, 1f, null, null, null, null, null, null, new Vector2(0.05f, 0.05f), true, true);
                        //YSortedEmitterList.Add(SmokeEmitter);

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

                            HeavyProjectile = new CannonBall(turret, CannonBallProjectileSprite, ToonSmoke3,
                                new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), turret.LaunchVelocity,
                                turret.FireRotation, 0.35f, turret.Damage, turret.BlastRadius,
                                new Vector2(Math.Max(AvgY - 32, 690), Math.Max(AvgY + 32, 930)));
                        }
                        else
                        {
                            HeavyProjectile = new CannonBall(turret, CannonBallProjectileSprite, ToonSmoke3,
                                new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), turret.LaunchVelocity,
                                turret.FireRotation, 0.35f, turret.Damage, turret.BlastRadius,
                                new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 690, 930), 930));
                        }

                        //SmokeTrail trail = new SmokeTrail(turret.BarrelEnd);
                        //trail.ProjectileTether = HeavyProjectile;
                        //SmokeTrailList.Add(trail);
                       

                        //HeavyProjectile.Initialize();
                        HeavyProjectileList.Add(HeavyProjectile);

                        AddDrawable(HeavyProjectile, FlashEmitter, FlashEmitter2, FlashEmitter3);

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
                        HeavyProjectile = new FlameProjectile(turret, FlameThrowerProjectileSprite, FireParticle2, new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
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

                case TurretType.StickyMine:
                    {
                        HeavyProjectile = new StickyMineProjectile(turret, GasGrenadeProjectileSprite, SparkParticle, turret.BarrelEnd, 30,
                            turret.FireRotation, 0.2f, 5, 80, new Vector2(690, 960));


                        //SmokeTrail trail = new SmokeTrail(turret.BarrelEnd);
                        //trail.ProjectileTether = HeavyProjectile;
                        //SmokeTrailList.Add(trail);
                       

                        //HeavyProjectile.Initialize();
                        HeavyProjectileList.Add(HeavyProjectile);

                        AddDrawable(HeavyProjectile);
                    }
                    break;

                case TurretType.Grapple:
                    {

                    }
                    break;

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

                        heavyProjectile = new ClusterBombShell(turret, 1000, ClusterBombProjectileSprite, SmokeParticle,
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
                        HeavyProjectile = new BoomerangProjectile(turret, BoomerangProjectileSprite, SmokeParticle, new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 12, turret.Rotation, 0.2f, turret.Damage, 100,
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

                        heavyProjectile = new GrenadeProjectile(turret, 2500, GrenadeProjectileSprite, SmokeParticle,
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

                        heavyProjectile = new GasGrenadeProjectile(turret, 2500, GrenadeProjectileSprite, SmokeParticle,
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
                        //LightProjectile beam = new MachineGunProjectile(new Vector2(turret.BarrelEnd.X,
                        //                                                           turret.BarrelEnd.Y), Direction), turret
                        if (CurrentBeam == null)
                        {
                            LightProjectile beam = new MachineGunProjectile(new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), Direction, turret.Damage);
                            beam.Source = turret;
                            CurrentBeam = beam;
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

        public Turret GetNewTurret(TurretType turretType, int index)
        {
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

            return (Turret)newTurret;
        }

        public static int TurretCost(TurretType turretType)
        {
            int cost = 0;

            switch (turretType)
            {
                default:
                    {
                        PropertyInfo propertyInfo;
                        propertyInfo = Type.GetType("TowerDefensePrototype." + turretType.ToString() + "Turret").GetProperty("ResourceCost", BindingFlags.Public | BindingFlags.Static);
                        object value = propertyInfo.GetValue(null, null);
                        cost = (int)value;
                    }
                    break;
            }

            return cost;
        }
        #endregion

        #region TRAP stuff
        private void TrapUpdate(GameTime gameTime)
        {
            for (int t = 0; t < TrapList.Count; t++)
            {
                Trap trap = TrapList[t];
                trap.Update(gameTime);
                                
                if (trap.DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)))
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) && 
                        CurrentMouseState.LeftButton == ButtonState.Released && 
                        PreviousMouseState.LeftButton == ButtonState.Pressed)
                    {
                        switch (trap.TrapState)
                        {
                            case TrapAnimationState.Untriggered:
                                trap.TrapState = TrapAnimationState.Triggering;
                                trap.CurrentAnimation.CurrentFrame = 0;
                                trap.CurrentAnimation.Update(gameTime);
                                break;

                            case TrapAnimationState.Active:
                                trap.TrapState = TrapAnimationState.Resetting;
                                trap.CurrentAnimation.CurrentFrame = 0;
                                trap.CurrentAnimation.Update(gameTime);
                                break;
                        }
                    }

                    #region Remove trap if right-clicked for long enough
                    if (CurrentMouseState.RightButton == ButtonState.Pressed &&
                        PreviousMouseState.RightButton == ButtonState.Pressed)
                    {
                        trap.CurrentRemovalTime += gameTime.ElapsedGameTime.TotalMilliseconds;

                        if (trap.CurrentRemovalTime > trap.MaxRemovalTime &&
                            trap.Active == true &&
                            trap.CurrentHP == trap.MaxHP &&
                            trap.CurrentDetonateLimit == trap.DetonateLimit)
                        {
                            trap.Active = false;

                            Resources += TrapCost(trap.TrapType);

                            foreach (Emitter emitter in YSortedEmitterList.Where(emit => emit.Tether == trap))
                            {
                                emitter.AddMore = false;
                            }

                            TrapList.Remove(trap);
                            DrawableList.Remove(trap); ;
                        }
                    }
                    else
                    {
                        trap.CurrentRemovalTime = 0;
                    }
                    #endregion
                    
                    trap.TrapQuickInfo.Update(gameTime);
                    trap.TrapOutline.Visible = true;
                    trap.Color = Color.White * 0.5f;
                }
                else
                {
                    trap.TrapQuickInfo.Visible = false;
                    trap.TrapOutline.Visible = false;
                    trap.TrapQuickInfo.CurrentVisibilityDelay = 0;
                    trap.CurrentRemovalTime = 0;
                    trap.Color = Color.White;
                }
                
                #region Specific behaviour based on the trap type
                switch (trap.TrapType)
                {
                    #region FlameThrower trap behaviour
                    case TrapType.FlameThrower:
                        {
                            if (trap.TrapState == TrapAnimationState.Active)
                            {
                                #region Create fire emitter
                                if (!YSortedEmitterList.Exists(Emitter => Emitter.Tether == trap))
                                {
                                    Emitter FireEmitter = new Emitter(FireParticle,
                                                                      new Vector2(trap.Position.X + trap.CurrentAnimation.FrameSize.X - 10,
                                                                                  trap.Position.Y + trap.CurrentAnimation.FrameSize.Y / 2),
                                                                      new Vector2(-5, 5), new Vector2(1.2f, 1.95f), new Vector2(2500, 3000), 1f, false,
                                                                      new Vector2(90, 90), new Vector2(-0.25f, 0.25f), new Vector2(0.05f, 0.1f), FireColor2, Color.Black * 0.75f,
                                                                      0.0f, -1, 20, 2, false, new Vector2(trap.DestinationRectangle.Bottom - 1.0f, trap.DestinationRectangle.Bottom - 1.0f), true, (trap.DestinationRectangle.Bottom - 1.0f) / 1080f,
                                                                      null, null, null, null, null, null, new Vector2(0.0f, 0.001f), true, true, null, false, false, true, null);

                                    FireEmitter.Emissive = true;

                                    YSortedEmitterList.Add(FireEmitter);
                                    AddDrawable(FireEmitter);

                                    FireEmitter.Tether = trap;
                                }
                                #endregion

                                (trap as FlameThrowerTrap).CurrentActiveTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                                if ((trap as FlameThrowerTrap).CurrentActiveTime > (trap as FlameThrowerTrap).ActiveTime)
                                {
                                    (trap as FlameThrowerTrap).CurrentActiveTime = 0;

                                    foreach (Emitter emitter in YSortedEmitterList.Where(emit => emit.Tether == trap))
                                    {
                                        emitter.AddMore = false;
                                    }

                                    trap.TrapState = TrapAnimationState.Resetting;
                                    trap.CurrentAnimation.CurrentFrame = 0;
                                    trap.CurrentAnimation.Update(gameTime);
                                }
                            }

                            if (trap.TrapState == TrapAnimationState.Triggering &&
                                trap.CurrentAnimation.CurrentFrame >= 16)
                            {
                                trap.CurrentAnimation.CurrentFrame = 0;
                                trap.TrapState = TrapAnimationState.Active;
                                trap.CurrentAnimation.CurrentFrame = 0;
                            }

                            if (trap.TrapState == TrapAnimationState.Resetting &&
                              trap.CurrentAnimation.CurrentFrame >= 16)
                            {
                                trap.CurrentAnimation.CurrentFrame = 0;
                                trap.TrapState = TrapAnimationState.Untriggered;
                                trap.CurrentAnimation.CurrentFrame = 0;
                            }
                        }
                        break;
                    #endregion

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

                    #region Spikes trap behaviour
                    case TrapType.Spikes:
                        {
                            if (trap.TrapState == TrapAnimationState.Active)
                            {
                                (trap as SpikesTrap).CurrentActiveTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                                if ((trap as SpikesTrap).CurrentActiveTime > (trap as SpikesTrap).ActiveTime)
                                {
                                    (trap as SpikesTrap).CurrentActiveTime = 0;

                                    trap.TrapState = TrapAnimationState.Resetting;
                                    trap.CurrentAnimation.CurrentFrame = 0;
                                    trap.CurrentAnimation.Update(gameTime);
                                }
                            }

                            if (trap.TrapState == TrapAnimationState.Triggering &&
                                trap.CurrentAnimation.CurrentFrame >= 5)
                            {
                                trap.CurrentAnimation.CurrentFrame = 0;
                                trap.TrapState = TrapAnimationState.Active;
                                trap.CurrentAnimation.CurrentFrame = 0;
                            }

                            if (trap.TrapState == TrapAnimationState.Resetting &&
                              trap.CurrentAnimation.CurrentFrame >= 5)
                            {
                                trap.CurrentAnimation.CurrentFrame = 0;
                                trap.TrapState = TrapAnimationState.Untriggered;
                                trap.CurrentAnimation.CurrentFrame = 0;
                            }
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
                //invader.TrapDamage(HitTrap);

                if (HitTrap.CurrentDetonateLimit > 0)
                    HitTrap.CurrentDetonateLimit -= 1;

                HitTrap.CurrentDetonateDelay = 0;

                //NEW_TRAP E **trap behaviour here**
                switch (HitTrap.TrapType)
                {
                    #region Fire Trap
                    case TrapType.Fire:
                        switch (HitTrap.CurrentDetonateLimit)
                        {
                            default:
                                foreach (Emitter emitter in YSortedEmitterList.Where(emit => emit.Tether == HitTrap && emit.TextureName.Contains("Fire")))
                                {
                                    //emitter.EndColor = Color.Lerp(emitter.EndColor, Color.Red, 0.1f);
                                    emitter.StartColor = Color.Lerp(emitter.StartColor, Color.Red, 0.1f);
                                }
                                break;
                        }
                        break;
                    #endregion

                    #region Glue Trap
                    case TrapType.Glue:
                        {
                            Decal decal = new Decal(GlueDecal1, HitTrap.Center, 0, 0.2f);
                            DecalList.Add(decal);
                            //Emitter glueEmitter = new Emitter(ToonGlueDrip1, HitTrap.Center, new Vector2(0, 360), new Vector2(4, 5),
                            //    new Vector2(2000, 2500), 1f, true, new Vector2(0, 0), new Vector2(0, 0),
                            //    new Vector2(0.04f, 0.08f), Color.Purple, Color.MediumPurple, 0.2f, 0.1f, 10, 40, true,
                            //    new Vector2(HitTrap.Position.Y, HitTrap.Position.Y + 8), false, null, true, null, null, null, null, true,
                            //    new Vector2(0.04f, 0.00f));
                           
                            //YSortedEmitterList.Add(glueEmitter);
                            //AddDrawable(glueEmitter);

                            invader.Freeze(new FreezeStruct()  { MaxDelay = 2500 }, Color.Purple);
                        }
                        break;
                    #endregion

                    #region Catapult trap
                    case TrapType.Catapult:
                        invader.Trajectory(new Vector2(4, -8));
                        break;
                    #endregion

                    #region LandMine Trap
                    case TrapType.LandMine:
                        {
                            Emitter ExplosionEmitter = new Emitter(ExplosionParticle2,
                                       new Vector2(HitTrap.Position.X, HitTrap.BoundingBox.Max.Y),
                                       new Vector2(20, 160), new Vector2(0.3f, 0.8f), new Vector2(500, 1000), 0.85f, true, new Vector2(-2, 2),
                                       new Vector2(-1, 1), new Vector2(0.15f, 0.25f), FireColor,
                                       Color.Black, -0.2f, 0.1f, 10, 1, false,
                                       new Vector2(HitTrap.DestinationRectangle.Bottom, HitTrap.DestinationRectangle.Bottom + 8), false, HitTrap.DestinationRectangle.Bottom / 1080f,
                                       null, null, null, null, null, null, new Vector2(0.1f, 0.2f), true, true, null, null, null, true);
                            YSortedEmitterList.Add(ExplosionEmitter);

                            Emitter ExplosionEmitter3 = new Emitter(ExplosionParticle2,
                                    new Vector2(HitTrap.Position.X, HitTrap.BoundingBox.Max.Y),
                                    new Vector2(85, 95), new Vector2(2, 4), new Vector2(400, 640), 0.35f, true, new Vector2(0, 0),
                                    new Vector2(0, 0), new Vector2(0.085f, 0.2f), FireColor, ExplosionColor3, -0.1f, 0.05f, 10, 1, false,
                                    new Vector2(HitTrap.DestinationRectangle.Bottom, HitTrap.DestinationRectangle.Bottom + 8), true, HitTrap.DestinationRectangle.Bottom / 1080f,
                                    null, null, null, null, null, null, new Vector2(0.0025f, 0.0025f), true, true, 50);
                            YSortedEmitterList.Add(ExplosionEmitter3);

                            Emitter DebrisEmitter = new Emitter(SplodgeParticle,
                                   new Vector2(HitTrap.Position.X, HitTrap.BoundingBox.Max.Y),
                                   new Vector2(70, 110), new Vector2(5, 7), new Vector2(480, 1760), 1f, true, new Vector2(0, 360),
                                   new Vector2(1, 3), new Vector2(0.007f, 0.05f), Color.DarkSlateGray, Color.DarkSlateGray,
                                   0.2f, 0.2f, 5, 1, true, new Vector2(HitTrap.DestinationRectangle.Bottom + 8, HitTrap.DestinationRectangle.Bottom + 16), null, HitTrap.DestinationRectangle.Bottom / 1080f);
                            YSortedEmitterList.Add(DebrisEmitter);

                            AddDrawable(ExplosionEmitter, ExplosionEmitter3, DebrisEmitter);
                                
                                                            

                             //   ExplosionEffect explosionEffect = new ExplosionEffect(new Vector2(HitTrap.Position.X, HitTrap.BoundingBox.Max.Y));
                             //   explosionEffect.Texture = ExplosionRingSprite;
                             //   ExplosionEffectList.Add(explosionEffect);

                             //   Decal NewDecal = new Decal(ExplosionDecal1, new Vector2(HitTrap.Position.X, HitTrap.Position.Y),
                             //                             (float)RandomDouble(0, 0), HitTrap.Position, HitTrap.DestinationRectangle.Bottom, 0.75f);

                             //   DecalList.Add(NewDecal);

                             //   Explosion newExplosion = new Explosion(new Vector2(HitTrap.Position.X, HitTrap.BoundingBox.Max.Y), 150, HitTrap.NormalDamage);

                             //   CreateExplosion(newExplosion, HitTrap);
                        }
                        break; 
                    #endregion

                    #region Barrel Trap
                    case TrapType.Barrel:
                        {
                            //for (int i = 0; i < 10; i++)
                            //{
                            //    HeavyProjectile heavyProjectile = new FlameProjectile(HitTrap, FlameProjectileSprite, FireParticle,
                            //               new Vector2(HitTrap.Position.X, HitTrap.Position.Y),
                            //               Random.Next(2, 5),
                            //               MathHelper.ToRadians(Random.Next(-105, -85)), 0.2f, HitTrap.NormalDamage, new Vector2(HitTrap.DestinationRectangle.Bottom, HitTrap.DestinationRectangle.Bottom + 16));

                            //    heavyProjectile.YRange = new Vector2(invader.MaxY, invader.MaxY);
                            //    HeavyProjectileList.Add(heavyProjectile);
                            //    AddDrawable(heavyProjectile);

                            //    //Light light = new Light(new Vector3(NewTrap.Center.X, NewTrap.DestinationRectangle.Bottom - 8, 15), LightTexture)
                            //    //{
                            //    //    Color = FireLightColor,
                            //    //    Active = true,
                            //    //    LightDecay = 25,
                            //    //    Power = 0.25f,
                            //    //    Radius = 12,
                            //    //    Range = 12,
                            //    //    Tether = heavyProjectile,
                            //    //    MaxTime = -1,
                            //    //    Oscillate = true,
                            //    //    CurrentOscillationTime = 0,
                            //    //    MaxOscillationTime = 500                                    
                            //    //};

                            //    //LightList.Add(light);
                            //}
                        }
                        break; 
                    #endregion

                    #region Spikes Trap
                    case TrapType.Spikes:
                        {
                            if (HitTrap.TrapState == TrapAnimationState.Untriggered)
                            {
                                HitTrap.TrapState = TrapAnimationState.Triggering;
                                HitTrap.CurrentAnimation.CurrentFrame = 0;
                                //HitTrap.CurrentAnimation.Update(time);

                                invader.TrapDamage(HitTrap);

                                //Create effect based on the type of invader being hurt by the spikes
                                switch (invader.InvaderType)
                                {
                                    #region Soldier
                                    case InvaderType.Soldier:
                                        {
                                            Emitter BloodEmitter = new Emitter(ToonBloodDrip1,
                                                    new Vector2(invader.Center.X, invader.Center.Y),
                                                    new Vector2(70, 110),
                                                    new Vector2(1, 2), new Vector2(3000, 6000), 1f, false, new Vector2(0, 360),
                                                    new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.White, Color.White,
                                                    0.1f, 0.1f, 10, 5, true, new Vector2(invader.MaxY - 5, invader.MaxY + 5), true, (invader.DestinationRectangle.Bottom + 1) / 1080f, true, false,
                                                    null, null, null, true, null, null, null, 500);

                                            Emitter BloodEmitter2 = new Emitter(ToonBloodDrip1,
                                                    new Vector2(invader.Center.X, invader.Center.Y),
                                                    new Vector2(70, 110),
                                                    new Vector2(1, 3), new Vector2(1800, 3600), 1f, false, new Vector2(0, 360),
                                                    new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.White, Color.White,
                                                    0.1f, 0.1f, 10, 5, true, new Vector2(invader.MaxY - 5, invader.MaxY + 5), true, (invader.DestinationRectangle.Bottom + 1) / 1080f, true, false,
                                                    null, null, null, true, null, null, null, 500);

                                            YSortedEmitterList.Add(BloodEmitter);
                                            YSortedEmitterList.Add(BloodEmitter2);

                                            AddDrawable(BloodEmitter, BloodEmitter2);
                                        }
                                        break;
                                    #endregion
                                }
                            }
                        }
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

                NewTrap = trap;
                string EntryName = trap.TrapType.ToString() + "TrapAnimations";

                switch (trap.TrapType)
                {
                    default:
                        {
                            NewTrap.AnimationList = new List<TrapAnimation>();

                            for (int i = 0; i < TrapAnimationsDictionary[EntryName].Count; i++)
                            {
                                TrapAnimation animation = new TrapAnimation();
                                animation = TrapAnimationsDictionary[EntryName][i].ShallowCopy();
                                NewTrap.AnimationList.Add(animation);
                            }

                            NewTrap.TrapState = NewTrap.TrapState;
                            NewTrap.Position = trapPosition;
                            TrapList.Add(NewTrap);
                            AddDrawable(NewTrap);
                            NewTrap.Initialize();
                        }
                        break;
                }

                switch (trap.TrapType)
                {
                    #region Wall
                    case TrapType.Wall:
                        {
                            //NewTrap.AmbientShadowTexture = WallAmbShadow;
                            SolidList.Add(new Solid(WhiteBlock, new Vector2(NewTrap.DestinationRectangle.Left, NewTrap.DestinationRectangle.Bottom - NewTrap.ZDepth), new Vector2(NewTrap.DestinationRectangle.Width, NewTrap.ZDepth)));
                        }
                        break;
                    #endregion

                    #region FlameThrower
                    case TrapType.FlameThrower:
                        {

                        }
                        break;
                    #endregion

                    #region Glue
                    case TrapType.Glue:
                        {

                        }
                        break;
                    #endregion

                    #region Fire
                    case TrapType.Fire:
                        {
                            FireTrapStart.Play();

                            Emitter FireEmitter = new Emitter(ExplosionParticle2, new Vector2(NewTrap.Position.X + NewTrap.CurrentAnimation.FrameSize.X / 2,
                                            NewTrap.Position.Y + NewTrap.CurrentAnimation.FrameSize.Y - 8), new Vector2(150f, 30f), new Vector2(0f, 0.5f),
                                new Vector2(899f, 1490f), 0.94f, false, new Vector2(-6f, 6f), new Vector2(-0.5f, 1f),
                                new Vector2(0.1025f, 0.1355f), new Color(255, 128, 0, 0), new Color(0, 0, 0, 255), -0.015f,
                                -1f, 93f, 3, false, new Vector2(0f, 720f), true, (NewTrap.DestinationRectangle.Bottom + 1.0f) / 1080f, false, false, new Vector2(0f, 0f),
                                new Vector2(0f, 0f), 0f, false, new Vector2(0.069f, 0f), false, false, 0f, false, false, true, null);
                            FireEmitter.TextureName = "FireParticle";
                            
                            Emitter SparkEmitter = new Emitter(RoundSparkParticle,
                                   new Vector2(NewTrap.Position.X + NewTrap.CurrentAnimation.FrameSize.X / 2,
                                               NewTrap.Position.Y + NewTrap.CurrentAnimation.FrameSize.Y - 16),
                                   new Vector2(80, 100), new Vector2(1, 4), new Vector2(800, 1600), 1f, true, new Vector2(0, 0),
                                   new Vector2(0, 0), new Vector2(0.1f, 0.3f), Color.LightYellow, Color.White, -0.001f, -1f, 100, 1, false,
                                   new Vector2(0, 1080), null, (NewTrap.DestinationRectangle.Bottom - 2) / 1080f);
                            SparkEmitter.TextureName = "SparkParticle";

                            YSortedEmitterList.Add(FireEmitter);
                            YSortedEmitterList.Add(SparkEmitter);

                            AddDrawable(NewTrap, FireEmitter, SparkEmitter);

                            FireEmitter.Tether = NewTrap;
                            SparkEmitter.Tether = NewTrap;

                            LightList.Add(new Light()
                            {
                                Active = true,
                                Color = Color.OrangeRed,
                                Position = new Vector3(NewTrap.Center.X, NewTrap.Position.Y + NewTrap.CurrentAnimation.FrameSize.Y - 8, 20),
                                Size = 400,
                                Power = 0.28f,
                                Depth = NewTrap.DrawDepth
                            });

                            //CrepLightList.Add(new CrepuscularLight()
                            //{
                            //    Position = new Vector2(NewTrap.Center.X, NewTrap.Position.Y + NewTrap.CurrentAnimation.FrameSize.Y - 8),
                            //    Decay = 0.9999f,
                            //    Exposure = 0.23f,
                            //    Density = 0.826f,
                            //    Weight = 0.358767f,
                            //    Depth = NewTrap.DrawDepth
                            //});
                        }
                        break;
                    #endregion

                    #region Spikes
                    case TrapType.Spikes:
                        {

                        }
                        break;
                    #endregion

                    #region Catapult
                    case TrapType.Catapult:
                        {
                        
                        }
                        break;
                    #endregion

                    #region Ice
                    case TrapType.Ice:
                        {

                        }
                        break;
                    #endregion

                    #region Barrel
                    case TrapType.Barrel:
                        {

                        }
                        break;
                    #endregion

                    #region Sawblade
                    case TrapType.SawBlade:
                        {

                        }
                        break;
                    #endregion

                    #region LandMine
                    case TrapType.LandMine:
                        {

                        }
                        break;
                    #endregion
                }

                ReadyToPlace = false;
                PlaceTrap.Play();

                if (CurrentKeyboardState.IsKeyDown(Keys.LeftShift) == false)
                    ClearSelected();
            };
            #endregion

            //Place the trap fully upgraded onto the terrain when the mouse is clicked
            //Still need to do a better check if there is a trap under where a new trap is going to be placed
            //if (CursorPosition.Y < 672 || CursorPosition.Y > 928)
            //{
            //    return;
            //}

            //IDEA: As a trap is placed, make sure it shows up as partially transparent with a box showing the actual collision
            //bounds of the trap. Mousing over the trap should do the same thing.
            //
            //Should also consider making lanes visible when moused-over with a trap selected

            Vector2 NewTrapPosition = new Vector2(CursorPosition.X - CursorPosition.X % 32, CursorPosition.Y - CursorPosition.Y % 32);
            int resourceCost = TrapCost(SelectedTrap.Value);

            if (Resources >= resourceCost)
            {
                Resources -= resourceCost;

                Trap newTrap = GetNewTrap(SelectedTrap.Value, NewTrapPosition);                              
                PlaceUpgradedTrap(newTrap, NewTrapPosition);

                //Trap outline
                newTrap.TrapOutline = new UIOutline(newTrap.Position, new Vector2(newTrap.DestinationRectangle.Width, newTrap.DestinationRectangle.Height), newTrap);  
                newTrap.TrapOutline.OutlineTexture = TurretSelectBox;

                //The trap quick info
                newTrap.TrapQuickInfo = new UITrapQuickInfo(newTrap.Position, newTrap);
                newTrap.TrapQuickInfo.RightClickIcon = RightClickIcon;
                newTrap.TrapQuickInfo.Detontations.Texture = WhiteBlock;
                newTrap.TrapQuickInfo.BoldFont = RobotoBold40_2;
                newTrap.TrapQuickInfo.Font = RobotoRegular20_2;
                newTrap.TrapQuickInfo.Italics = RobotoItalic20_0;

                newTrap.TrapQuickInfo.RemovalBar = new CircularBar(newTrap.Position + new Vector2(25, -25), new Vector2(41, 41), 0,
                                        (float)newTrap.MaxRemovalTime, Color.Transparent, Color.White, HealthBarSprite);


                #region Start button cooldown when trap is placed
                for (int i = 0; i < CooldownButtonList.Count; i++)
                {
                    if (CurrentProfile.Buttons[i] != null && CurrentProfile.Buttons[i].CurrentTrap == newTrap.TrapType && StartWave == true)
                    {
                        CooldownButtonList[i].CoolingDown = true;
                    }
                }
                #endregion

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
                        {
                            Emitter BOOMEmitter;

                            BOOMEmitter = new Emitter(FWOOMParticle, new Vector2(newTrap.Center.X, newTrap.DestinationRectangle.Bottom),
                                          new Vector2(0, 0), new Vector2(0.001f, 0.001f), new Vector2(500, 500), 1f, false,
                                          new Vector2(-25, 25), new Vector2(0, 0), new Vector2(0.2f, 0.2f),
                                          Color.White, Color.White, -0.05f, 0.05f, 50, 1, false, new Vector2(0, 1080), true,
                                          1.0f, null, null, null, null, null, false, new Vector2(0.11f, 0.11f), false, false,
                                          null, false, false, true);

                            YSortedEmitterList.Add(BOOMEmitter);
                            DrawableList.Add(BOOMEmitter);
                                                   
                            //AdditiveEmitterList.Add(new Emitter(RoundSparkParticle,
                            //    new Vector2(newTrap.Center.X, newTrap.DestinationRectangle.Bottom),
                            //    new Vector2(160, 200), new Vector2(1, 4), new Vector2(20, 40), 1f, true, new Vector2(0, 0),
                            //    new Vector2(0, 0), new Vector2(0.25f, 0.25f), FireColor, FireColor2, -0.05f, 0.2f, 3, 1, false,
                            //    new Vector2(0, 1080), false, null, null, null, null, null, null, null, new Vector2(0.05f, 0.05f)));

                            //AdditiveEmitterList.Add(new Emitter(RoundSparkParticle,
                            //    new Vector2(newTrap.Center.X, newTrap.DestinationRectangle.Bottom),
                            //    new Vector2(-20, 20), new Vector2(1, 4), new Vector2(20, 40), 1f, true, new Vector2(0, 0),
                            //    new Vector2(0, 0), new Vector2(0.25f, 0.25f), FireColor, FireColor2, -0.05f, 0.1f, 3, 1, false,
                            //    new Vector2(0, 1080), false, null, null, null, null, null, null, null, new Vector2(0.05f, 0.05f)));
                        }
                        break;


                    #endregion
                }
                #endregion
            }
        }

        public Trap GetNewTrap(TrapType trapType, Vector2 trapPosition)
        {
            Type trapObject = Type.GetType("TowerDefensePrototype." + trapType.ToString() + "Trap");
            object newTrap = Activator.CreateInstance(trapObject, trapPosition);
            return (Trap)newTrap;
        }

        public static int TrapCost(TrapType trapType)
        {
            int cost = 0;

            switch (trapType)
            {
                default:
                    {
                        PropertyInfo propertyInfo;
                        propertyInfo = Type.GetType("TowerDefensePrototype." + trapType.ToString() + "Trap").GetProperty("ResourceCost", BindingFlags.Public | BindingFlags.Static);                        
                        object value = propertyInfo.GetValue(null, null);
                        cost = (int)value;
                    }
                    break;
            }

            return cost;
        }

        private int TrapPowerUsage(TrapType trapType)
        {
            int cost = 0;

            //NEW_TRAP G **trap power usage added here**
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
                    cost = new SpikesTrap(Vector2.Zero).PowerCost;
                    break;

                case TrapType.Wall:
                    cost = new WallTrap(Vector2.Zero).PowerCost;
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
                            TimerHeavyProjectile AirStrikeProjectile = new ClusterBombShell(CurrentSpecialAbility, 1000, ClusterBombProjectileSprite, SmokeParticle, airStrike.CurrentPosition, 10, 0, 0.2f, 5, 0, new Vector2(690, 930));
                            //AirStrikeProjectile.Initialize();
                            AddDrawable(AirStrikeProjectile);
                            airStrike.CurrentTime = 0;
                        }
                        break;
                }
            }
        }
        #endregion

        public void AddInvader(Invader nextInvader, GameTime gameTime, Vector2? posOverride = null)
        {
            float Multiplier = (float)RandomDouble(0.75f, 1.5f);

            if (posOverride != null)
            {
                nextInvader.Position = posOverride.Value;
            }
            

            string EntryName = nextInvader.InvaderType.ToString() + "InvaderAnimations";

            switch (nextInvader.InvaderType)
            {
                default:
                    //Have to do it this way to make sure all the invaders aren't referencing the same animation
                    nextInvader.AnimationList = new List<InvaderAnimation>(InvaderAnimationsDictionary[EntryName].Count);

                    for (int i = 0; i < InvaderAnimationsDictionary[EntryName].Count; i++)
                    {
                        InvaderAnimation animation = new InvaderAnimation();
                        animation = InvaderAnimationsDictionary[EntryName][i].ShallowCopy();
                        animation.FrameDelay /= Multiplier;
                        nextInvader.AnimationList.Add(animation);
                    }

                    HeavyRangedInvader heavyRangedInvader = nextInvader as HeavyRangedInvader;
                    LightRangedInvader lightRangedInvader = nextInvader as LightRangedInvader;

                    switch (nextInvader.InvaderType)
                    {
                        #region HealDrone
                        case InvaderType.HealDrone:
                            {
                                Emitter jetEmitter = new Emitter(BallParticle, nextInvader.Position, new Vector2(90f, 90f),
                                    new Vector2(6.8f, 6.8f), new Vector2(150, 150), 1f, true,
                                    new Vector2(0f, 360f), new Vector2(-2f, 2f), new Vector2(1f, 1f),
                                    new Color(255, 128, 0, 63), new Color(0, 0, 255, 255), 0f, 0f, 16f,
                                    1, false, new Vector2(0f, 720f), true, 0, false, false,
                                    new Vector2(0f, 0f), new Vector2(0f, 0f), 0f, false, new Vector2(0.045f, 0.045f),
                                    false, false, 0f, false, false, false, null);

                                JetEngine newEngine = new JetEngine(new Vector2(100, 100), new Vector2(JetEngineSprite.Width, JetEngineSprite.Height), JetEngineSprite, Color.White)
                                {
                                    JetEmitter = jetEmitter,
                                    Tether = nextInvader,
                                    TetherOffset = new Vector2(42, 30),
                                    Origin = new Vector2(JetEngineSprite.Width / 2, 0)
                                };

                                JetEngineList.Add(newEngine);
                                AddDrawable(newEngine);

                                //JetEngine newEngine = new JetEngine(JetEngineSprite)
                                //{
                                //    Tether = nextInvader
                                //};

                                //JetEngineList.Add(newEngine);
                                //AddDrawable(newEngine);
                            }
                            break;
                        #endregion

                        #region ShieldGenerator
                        case InvaderType.ShieldGenerator:
                            {
                                Shield shield = nextInvader.Shield;
                                shield.ShieldTexture = ShieldSprite;
                                shield.Tether = nextInvader;
                                shield.MaxRadius = (nextInvader as ShieldGenerator).ShieldRadius;
                                shield.Active = false;
                                ShieldList.Add(shield);
                                AddDrawable(shield);
                                //ShieldList.Add((ShieldGenerator)nextInvader.Shi
                            }
                            break;
                        #endregion

                        #region DropShip
                        case InvaderType.DropShip:
                            {
                                (nextInvader as DropShip).DoorTexture = DropShipDoorTexture;
                                (nextInvader as DropShip).VapourTexture = ToonSmoke3;
                                (nextInvader as DropShip).RopeTexture = RopeTexture1;
                                (nextInvader as DropShip).Game1 = this;
                            }
                            break;
                        #endregion
                    }

                    #region Load the barrel sprites for the heavy ranged invaders
                    if (heavyRangedInvader != null)
                    {
                        //HEAVYRANGED_INVADER B **if the invader has a barrel animation that needs to be loaded**
                        switch (heavyRangedInvader.InvaderType)
                        {
                            #region StationaryCannon
                            case InvaderType.StationaryCannon:
                                {
                                    heavyRangedInvader.BarrelAnimation = StationaryCannonBarrelAnimation;
                                }
                                break;
                            #endregion

                            #region HarpoonCannon
                            case InvaderType.HarpoonCannon:
                                {
                                    heavyRangedInvader.BarrelAnimation = HarpoonCannonBarrelAnimation;
                                }
                                break;
                            #endregion
                        }
                    }
                    #endregion
                    break;
            }

            //This makes sure that the property change triggers
            nextInvader.InvaderAnimationState = nextInvader.InvaderAnimationState;

            nextInvader.IceBlock = IceBlock;
            nextInvader.Shadow = Shadow;
            nextInvader.FearEmotionIcon = FearEmotionIcon;
            nextInvader.ThinkingAnimation = ThinkingAnimation.ShallowCopy();

            //This gives the invaders a speed variation
            //nextInvader.Direction.X *= Multiplier;
            nextInvader.Speed = nextInvader.OriginalSpeed * Multiplier;

            if (InvaderList.Find(Invader => Invader.MaxY == nextInvader.MaxY) != null)
            {
                nextInvader.MaxY += 2;
            }

            nextInvader.MaxY = Random.Next((int)nextInvader.YRange.X, (int)nextInvader.YRange.Y);

            //if (nextInvader.Airborne == false)
            //    nextInvader.Position.Y = nextInvader.MaxY - nextInvader.CurrentAnimation.FrameSize.Y;

            nextInvader.Velocity = Vector2.Zero;
            nextInvader.Initialize();

            nextInvader.Update(gameTime, CursorPosition);

            nextInvader.InvaderOutline = new UIOutline(nextInvader.Position,
                                            new Vector2(nextInvader.CurrentAnimation.GetFrameSize().X,
                                                        nextInvader.CurrentAnimation.GetFrameSize().Y),
                                                       null, null, nextInvader);
            nextInvader.InvaderOutline.OutlineTexture = TurretSelectBox;

            InvaderList.Add(nextInvader);
            AddDrawable(nextInvader);
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
                    #region If there are still elements to be added
                    if (CurrentInvaderIndex < CurrentWave.InvaderList.Count && StartWave == true)
                    {
                        #region If the element is an invader
                        if (CurrentWave.InvaderList[CurrentInvaderIndex] is Invader)
                        {
                            Invader nextInvader = (Invader)CurrentWave.InvaderList[CurrentInvaderIndex];

                            AddInvader(nextInvader, gameTime);
                                                        
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
                                //StartWaveButton = new Button(ButtonLeftSprite, new Vector2(1000, 200));
                                //StartWaveButton.Initialize(OnButtonClick);
                                CurrentWaveIndex++;
                                StartWave = false;
                            }
                        }
                    }
                    #endregion
                }
            }

            //#region Handle victory conditions
            //if (InvaderList.Count == 0 &&
            //    CurrentWaveIndex > CurrentLevel.WaveList.Count - 1)
            //{
            //    Victory = true;
            //    VictoryContinue = new Button(ButtonLeftSprite, new Vector2(-300, 834), null, null, null, "continue", RobotoRegular20_2, "Left", null);
            //    VictoryContinue.NextPosition = new Vector2(665, 834);
            //    VictoryContinue.Initialize(OnButtonClick);
            //    GameState = GameState.Victory;
            //}
            //#endregion
            //}
        }

        public void PlayerVictory()
        {
            Victory = true;
            VictoryContinue = new Button(ButtonLeftSprite, new Vector2(-300, 834), null, null, null, "continue", RobotoRegular20_2, "Left", null);
            VictoryContinue.NextPosition = new Vector2(665, 834);
            VictoryContinue.Initialize(OnButtonClick);
            GameState = GameState.Victory;
        }

        public void StartWaves()
        {
            StartWave = true;
            CurrentWaveTime = 0;
            CurrentInvaderIndex = 0;
            CurrentInvaderTime = 0;

            //if (CurrentWaveIndex > 0)
                //CurrentWaveIndex++;

            //WaveCountDown = new WaveCountDown(10)
            //{
            //    Font = RobotoBold40_2,
            //    Color = Color.White,
            //    Origin = new Vector2(1920 / 2, 100)
            //};

            StartWaveButton = null;
        }
         
        public void LoadLevel(int number)
        {
            //CurrentLevel = Content.Load<Level>("Levels/Level" + number);
            //var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

            Assembly assembly = Assembly.Load("TowerDefensePrototype");
            Type t = assembly.GetType("TowerDefensePrototype.Level" + number);
            CurrentLevel = (Level)Activator.CreateInstance(t, this);
            
            ////Load the story dialogue used for this level here
            //try
            //{
            //    StoryDialogue = Content.Load<StoryDialogue>("StoryDialogue/StoryDialogue" + number);
            //}
            //catch (ContentLoadException)
            //{                
            //    //throw;
            //}

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


        public void DynamicAddWeapon(int slot, TurretType? turret, TrapType? trap)
        {
            Texture2D Icon = null;

            if (turret != null)
            {
                CurrentProfile.Buttons[slot] = new Weapon(turret, null);

                switch (turret)
                {
                    case null:

                        break;

                    default:
                        {
                            //string WeaponName = turret.ToString();
                            //var icon = this.GetType().GetField(WeaponName + "TurretIcon").GetValue(this);
                            //Icon = (Texture2D)icon;

                            TurretIconDictionary.TryGetValue(CurrentProfile.Buttons[slot].CurrentTurret.ToString() + "TurretIcon", out Icon);
                        }
                        break;
                }
            }

            if (trap != null)
            {
                CurrentProfile.Buttons[slot] = new Weapon(null, trap);

                switch (trap)
                {
                    case null:

                        break;

                    default:
                        {
                            //string WeaponName = trap.ToString();
                            //var icon = this.GetType().GetField(WeaponName + "TrapIcon").GetValue(this);
                            //Icon = (Texture2D)icon;

                            TrapIconDictionary.TryGetValue(CurrentProfile.Buttons[slot].CurrentTrap.ToString() + "TrapIcon", out Icon);
                        }
                        break;
                }
            }

            //UIWeaponInfoList.Clear();
            //CooldownButtonList.Clear();

            int i = slot;

            //for (int i = 0; i < 8; i++)
            //{
                CooldownButton button = new CooldownButton(new Vector2(565 + (i * 100), 1080 - 80), new Vector2(90, 65), 1, Icon);
                button.ButtonClickHappened += OnButtonClick;

                if (CurrentProfile.Buttons[i] != null)
                {
                    UIWeaponInfoTip uiWeaponInfo = new UIWeaponInfoTip(Vector2.Zero, null, null);

                    if (CurrentProfile.Buttons[i].CurrentTurret != null)
                    {
                        uiWeaponInfo = new UIWeaponInfoTip(new Vector2(button.CurrentPosition.X, button.CurrentPosition.Y - 32),
                                                        GetNewTurret((TurretType)CurrentProfile.Buttons[i].CurrentTurret, 0), null);
                        button.ResourceCost = TurretCost(CurrentProfile.Buttons[i].CurrentTurret.Value);
                    }

                    if (CurrentProfile.Buttons[i].CurrentTrap != null)
                    {
                        uiWeaponInfo = new UIWeaponInfoTip(new Vector2(button.CurrentPosition.X, button.CurrentPosition.Y - 32),
                                                          null, GetNewTrap((TrapType)CurrentProfile.Buttons[i].CurrentTrap, Vector2.Zero));
                        button.ResourceCost = TrapCost(CurrentProfile.Buttons[i].CurrentTrap.Value);
                    }


                    uiWeaponInfo.CurrencyIcon = CurrencyIcon;
                    uiWeaponInfo.PowerUnitIcon = PowerUnitIcon;
                    uiWeaponInfo.RobotoBold40_2 = RobotoBold40_2;
                    uiWeaponInfo.RobotoRegular20_0 = RobotoRegular20_0;
                    uiWeaponInfo.RobotoRegular20_2 = RobotoRegular20_2;
                    uiWeaponInfo.RobotoItalic20_0 = RobotoItalic20_0;

                    //uiWeaponInfo.LoadContent(Content);
                    UIWeaponInfoList[i] = uiWeaponInfo;
                }
                //else
                //{
                //    UIWeaponInfoList.Add(null);
                //}

                CooldownButtonList[i] = button;
                //CooldownButtonList.RemoveAt(CooldownButtonList.Count - 1);
                //CooldownButtonList.Add(button);
            //}
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
            //if (powerup.CurrentEffect.GeneralPowerup != null)
            //    switch (powerup.CurrentEffect.GeneralPowerup)
            //    {
            //        case GeneralPowerup.BlastRadii:
            //            return KineticDamageIcon;

            //        case GeneralPowerup.ExplosivePower:
            //            return KineticDamageIcon;

            //        case GeneralPowerup.RepairTower:
            //            return KineticDamageIcon;
            //    }

            //if (powerup.CurrentEffect.TowerPowerup != null)
            //    switch (powerup.CurrentEffect.TowerPowerup)
            //    {
            //        case null:
            //            break;
            //    }

            //if (powerup.CurrentEffect.TrapPowerup != null)
            //    switch (powerup.CurrentEffect.TrapPowerup)
            //    {
            //        case null:
            //            break;
            //    }

            //if (powerup.CurrentEffect.TurretPowerup != null)
            //    switch (powerup.CurrentEffect.TurretPowerup)
            //    {
            //        case null:
            //            break;
            //    }


            return WhiteBlock;
        }

        private void HandlePlacedIcons()
        {
            foreach (Button button in PlaceWeaponList)
            {
                button.IconTexture = null;

                int Index = PlaceWeaponList.IndexOf(button);
                Button PressedButton = PlaceWeaponList[Index];
                                
                if (CurrentProfile.Buttons[Index] != null)
                {
                    //Change the icon of the placed weapon button when a weapon is placed into a slot - TURRETS
                    switch (CurrentProfile.Buttons[Index].CurrentTurret)
                    {
                        case null:

                            break;

                        default:
                            {
                                TurretIconDictionary.TryGetValue(CurrentProfile.Buttons[Index].CurrentTurret.ToString() + "TurretIcon", out PressedButton.IconTexture);
                            }
                            break;
                    }

                    //Change the icon of the placed weapon button when a weapon is placed into a slot - TRAPS
                    switch (CurrentProfile.Buttons[Index].CurrentTrap)
                    {
                        case null:

                            break;

                        default:
                            {
                                TrapIconDictionary.TryGetValue(CurrentProfile.Buttons[Index].CurrentTrap.ToString() + "TrapIcon", out PressedButton.IconTexture);
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
                        //var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

                        bool Available;
                        CurrentProfile.TurretDictionary.TryGetValue(turretBox.ContainsTurret.Value, out Available);

                        if (Available == true)
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
                        //var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

                        bool Available;
                        CurrentProfile.TrapDictionary.TryGetValue(trapBox.ContainsTrap.Value, out Available);

                        if (Available == true)
                        {
                            trapBox.Locked = false;
                        }
                        else
                        {
                            trapBox.Locked = true;
                            trapBox.WeaponName = "Locked";
                        }

                        //string WeaponName = trapBox.ContainsTrap.ToString();
                        //var Available = CurrentProfile.GetType().GetField(WeaponName).GetValue(CurrentProfile);

                        //if ((bool)Available == true)
                        //{
                        //    trapBox.Locked = false;
                        //}
                        //else
                        //{
                        //    trapBox.Locked = true;
                        //    trapBox.WeaponName = "Locked";
                        //}
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
                WeaponBox turretBox = new WeaponBox(new Vector2(158 + 113 + (i * 282), 500 - 32), GetNewTurret((TurretType)(i), -1), null);

                turretBox.ContainsTurret = (TurretType)turretTypes.GetValue(i);

                turretBox.CurrencyIcon = CurrencyIcon;
                turretBox.PowerUnitIcon = PowerUnitIcon;
                turretBox.RobotoBold40_2 = RobotoBold40_2;
                turretBox.RobotoRegular20_0 = RobotoRegular20_0;
                turretBox.RobotoRegular20_2 = RobotoRegular20_2;
                turretBox.RobotoItalic20_0 = RobotoItalic20_0;
                //turretBox.LoadContent(SecondaryContent);

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
                WeaponBox trapBox = new WeaponBox(new Vector2(158 + 113 + (i * 282), 902 - 32), null, GetNewTrap((TrapType)(i), Vector2.Zero));

                trapBox.ContainsTrap = (TrapType)trapTypes.GetValue(i);
                trapBox.PowerUnitIcon = PowerUnitIcon;
                trapBox.CurrencyIcon = CurrencyIcon;
                trapBox.RobotoBold40_2 = RobotoBold40_2;
                trapBox.RobotoRegular20_0 = RobotoRegular20_0;
                trapBox.RobotoRegular20_2 = RobotoRegular20_2;
                trapBox.RobotoItalic20_0 = RobotoItalic20_0;
                //trapBox.LoadContent(SecondaryContent);

                trapBox.Visible = false;
                SelectTrapList.Add(trapBox);
                trapBox.ButtonClickHappened += OnButtonClick;
            }
            #endregion
            #endregion
        }


        public void UnlockWeapons()
        {
            switch (CurrentProfile.LevelNumber)
            {
                case 1:

                    break;

                case 2:
                    //CurrentProfile.Cannon = true;
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


        public static void AddDrawable(params Drawable[] newDrawable)
        {
            //AddDrawable(newDrawable)

            //THIS SHOWS PROMISE!! WORKS BUT THE FIRST ELEMENT INSERTED DETERMINES HOW OTHER ELEMENTS INSERTED WILL SHOW UP
            //CAN TOTALLY WORK WITH MORE WORK
            //--*****************************************************--
            //if (DrawableList.Count > 0)
            //{
            //    for (int i = 0; i < newDrawable.Count(); i++)
            //    {
            //        int thing = DrawableList.IndexOf(DrawableList.FirstOrDefault(Drawable => Drawable.DrawDepth > newDrawable[i].DrawDepth));

            //        if (thing != -1)
            //        {
            //            DrawableList.Insert(thing, newDrawable[i]);
            //        }
            //    }
            //}
            //else
            //{
            //    DrawableList.AddRange(newDrawable);
            //}
            //--*********************************************************************--

            
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

        public void SortDrawables()
        {
            DrawableList.Sort((x, y) => x.DrawDepth.CompareTo(y.DrawDepth));
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

            //CurrentProfile = new Profile();
            CurrentProfile = new Profile()
            {
                Name = NameInput.RealString,
                LevelNumber = 1,

                Points = 0,
                Credits = 0,
                ShotsFired = 0
            };

            //foreach (TurretType turretType in Enum.GetValues(typeof(TurretType)))
            //{
            //    bool myThing = true;
            //    CurrentProfile.TurretDictionary.TryGetValue(turretType, out myThing);
            //}

            //foreach (TrapType trapType in Enum.GetValues(typeof(TurretType)))
            //{
            //    bool myThing = true;
            //    CurrentProfile.TrapDictionary.TryGetValue(trapType, out myThing);
            //}


            #region Make all TRAPS available
            //foreach (TrapType trapType in Enum.GetValues(typeof(TrapType)))
            //{
            //    string TrapName = trapType.ToString();

            //    var thing = CurrentProfile.GetType().GetField(TrapName);
            //    thing.SetValue(CurrentProfile, true);
            //}
            #endregion

            #region Make all TURRETS available
            //foreach (TurretType turretType in Enum.GetValues(typeof(TurretType)))
            //{
            //    string TurretName = turretType.ToString();

            //    var thing = CurrentProfile.GetType().GetField(TurretName);
            //    thing.SetValue(CurrentProfile, true);
            //}
            #endregion

            StorageDevice.BeginShowSelector(this.NewProfile, null);

            NameInput.RealString = "";
            NameInput.TypePosition = 0;

            //GameState = GameState.ProfileManagement;
            //GameState = GameState.Playing;

            if (CurrentProfile != null)
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
        

        #region Some cool functions
        public static void PlayRandomSound(params SoundEffect[] soundEffect)
        {
            soundEffect[Random.Next(0, soundEffect.Length)].Play(0.5f, 0, 0);
        }

        public static double RandomDouble(double a, double b)
        {
            return a + Random.NextDouble() * (b - a);
        }

        public static double PercentageChange(double number, double percentage) //Changes a number by a percentage. i.e. PercentageChange(100, 25) returns 125
        {
            double newNumber = number;
            newNumber = newNumber + ((newNumber / 100) * percentage);
            return newNumber;
        }

        /// <summary>
        /// Checks if two values are approximately the same.
        /// </summary>
        /// <param name="value1">The first value to compare</param>
        /// <param name="value2">The second value to compare</param>
        /// <param name="delta">The maximum difference between the values</param>
        /// <returns></returns>
        public static bool Approx(float value1, float value2, float delta)
        {
            if (Math.Abs(value1 - value2) < delta)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Point VectorToPoint(Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }

        public static Rectangle BoundingBoxToRect(BoundingBox boundingBox)
        {
            Rectangle rect = new Rectangle((int)boundingBox.Min.X, (int)boundingBox.Min.Y,
                                           (int)(boundingBox.Max.X - boundingBox.Min.X),
                                           (int)(boundingBox.Max.Y - boundingBox.Min.Y));
            return rect;
        }

        public static BoundingBox RectToBoundingBox(Rectangle rectangle)
        {
            BoundingBox box = new BoundingBox(new Vector3(rectangle.Left, rectangle.Top, 0), new Vector3(rectangle.Right, rectangle.Bottom, 0));
            return box;
        }

        public static bool RandomBool()
        {
            if ((float)Random.NextDouble() > 0.5f)
                return true;
            else
                return false;
        }

        public static Vector4 ColorToVector(Color color)
        {
            return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

        private static Color RandomColor(params Color[] colors)
        {
            //List<Color> ColorList = colors.ToList();

            return colors[Random.Next(0, colors.Length)];
        }

        private static Texture2D RandomTexture(params Texture2D[] textures)
        {
            //List<Texture2D> TextureList = textures.ToList();

            return textures[Random.Next(0, textures.Length)];
        }

        public static SpriteEffects RandomOrientation(params SpriteEffects[] Orientations)
        {
            //List<SpriteEffects> OrientationList = new List<SpriteEffects>();

            //foreach (SpriteEffects orientation in Orientations)
            //{
            //    OrientationList.Add(orientation);
            //}

            return Orientations[Random.Next(0, Orientations.Length)];
        }

        public static float LerpTime(float value1, float value2, float lerp, GameTime gameTime)
        {
            //Auomatically corrects for the gametime
            return MathHelper.Lerp(value1, value2, lerp * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60));
        }

        public static Vector2 LerpVectorTime(Vector2 value1, Vector2 value2, float lerp, GameTime gameTime)
        {
            //Auomatically corrects for the gametime
            return Vector2.Lerp(value1, value2, lerp * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60));
        }

        //********REALLY REALLY COOL!!*****
        //Can keep incrementing the value and it'll just loop around to the first value again. Really neat
        //public void NextSearchType()
        //{
        //    searchMethod = (SearchMethod)(((int)searchMethod + 1) % (int)SearchMethod.Max);
        //}
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

        public class myRay
        {
            public Vector3 position, direction;
            public float length;
        }

        public Emitter SnapPing(Vector2 collisionEnd)
        {
            Emitter BOOMEmitter;

            if (Random.NextDouble() >= 0.5f)
            {
                BOOMEmitter = new Emitter(SNAPParticle, collisionEnd,
                              new Vector2(0, 0), new Vector2(0.001f, 0.001f), new Vector2(250, 250), 1f, false,
                              new Vector2(-25, 25), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                              Color.White, Color.White, 0f, 0.05f, 50, 1, false, new Vector2(0, 1080), true,
                              collisionEnd.Y + 4 / 1080f, null, null, null, null, null, false, new Vector2(0.11f, 0.11f), false, false,
                              null, false, false, true);
            }
            else
            {
                BOOMEmitter = new Emitter(PINGParticle, collisionEnd,
                              new Vector2(0, 0), new Vector2(0.001f, 0.001f), new Vector2(250, 250), 1f, false,
                              new Vector2(-25, 25), new Vector2(0, 0), new Vector2(0.15f, 0.15f),
                              Color.White, Color.White, 0f, 0.05f, 50, 1, false, new Vector2(0, 1080), true,
                              collisionEnd.Y + 4 / 1080f, null, null, null, null, null, false, new Vector2(0.11f, 0.11f), false, false,
                              null, false, false, true);
            }

            return BOOMEmitter;
        }

        private int GetEven(int num)
        {
            if (num % 2 == 0)
            {
                return num;
            }
            else
            {
                return num + 1;
            }
        }


        public static class PSBlendState
        {
            public static BlendState Multiply = new BlendState
            {
                ColorSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.Zero,
                ColorBlendFunction = BlendFunction.Add
            };
            public static BlendState Screen = new BlendState
            {
                ColorSourceBlend = Blend.InverseDestinationColor,
                ColorDestinationBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Add
            };
            public static BlendState Darken = new BlendState
            {
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Min
            };
            public static BlendState Lighten = new BlendState
            {
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Max
            };
        }

        public void DrawShadows(Light light)
        {
            //Vector3 LightPos;

            //LightPos = new Vector3(Mouse.GetState().X, Mouse.GetState().Y, 250);
            //LightList[0].Position = new Vector3(SpritePos.X, SpritePos.Y, 0) + new Vector3(16, 16, 0);
            //LightList[0].Position = LightPos;

            //LightList[LightList.Count - 1].Position = LightPos;

            Vector2 SourcePosition = new Vector2(light.Position.X, light.Position.Y);

            RayList.Clear();
            ShadowList.Clear();

            foreach (Solid solid in SolidList)
            {
                Vector3 lightVector, check1, check2, thing, thing2;

                for (int i = 0; i < solid.vertices.Count(); i++)
                {
                    if (CurrentKeyboardState.IsKeyDown(Keys.P) &&
                        PreviousKeyboardState.IsKeyUp(Keys.P))
                    {
                        int stop = 10;
                    }

                    lightVector = solid.vertices[i].Position - new Vector3(SourcePosition, 0);
                    lightVector.Normalize();

                    //lightVector *= light.Size;

                    int nextIndex, prevIndex;

                    nextIndex = Wrap(i + 1, 4);
                    prevIndex = Wrap(i - 1, 4);

                    check1 = solid.vertices[nextIndex].Position - new Vector3(SourcePosition, 0);
                    check2 = solid.vertices[prevIndex].Position - new Vector3(SourcePosition, 0);

                    thing = Vector3.Cross(lightVector, check1);
                    thing2 = Vector3.Cross(lightVector, check2);

                    //NOTE: THIS LINE SEEMS TO FIX THE 0 VALUE CHECK VARIABLE RESULTING IN A DISAPPEARING SHADOW
                    thing.Normalize();

                    //SHADOWS DON'T SHOW UP IF THE Y OR X VALUES FOR THE THING AND CHECK ARE THE SAME.
                    //i.e. check1.y = 158 AND thing1.y = 158. Then the next if evaluates to false and a ray isn't added.
                    //meaning that there's a blank side for the polygon
                    //The Check variables use the previous and next vertex positions to calculate a vector
                    //This can end up with the vector having a 0 in it if the light lines up with a side
                    //This makes the cross product values messed up

                    if ((thing.Z <= 0 && thing2.Z <= 0) ||
                        (thing.Z >= 0 && thing2.Z >= 0))
                    {
                        RayList.Add(new myRay() { direction = lightVector, position = solid.vertices[i].Position, length = 1f });
                    }
                }

                if (RayList.Count > 1)
                {
                    int p = RayList.Count() - 2;

                    VertexPositionColor[] vertices = new VertexPositionColor[6];

                    vertices[0].Position = RayList[p].position;
                    vertices[1].Position = RayList[p].position + (RayList[p].direction * 1920f);
                    vertices[2].Position = RayList[p + 1].position + (RayList[p + 1].direction * 1920f);

                    vertices[3].Position = RayList[p + 1].position + (RayList[p + 1].direction * 1920f);
                    vertices[4].Position = RayList[p + 1].position;
                    vertices[5].Position = RayList[p].position;

                    vertices[0].Color = Color.Black;
                    vertices[1].Color = Color.Black;
                    vertices[2].Color = Color.Black;
                    vertices[3].Color = Color.Black;
                    vertices[4].Color = Color.Black;
                    vertices[5].Color = Color.Black;

                    ShadowList.Add(new PolygonShadow() { Vertices = vertices });
                }
            }
        }

        public Texture2D MyShadow(Light light)
        {
            GraphicsDevice.SetRenderTarget(ShadowMap);
            GraphicsDevice.Clear(Color.White);

            DrawShadows(light);

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.BlendState = PSBlendState.Multiply;
            BasicEffect.TextureEnabled = false;
            BasicEffect.Techniques[0].Passes[0].Apply();

            foreach (PolygonShadow shadow in ShadowList)
            {
                shadow.Draw(GraphicsDevice);
            }

            BasicEffect.TextureEnabled = true;
            return ShadowMap;
        }

        public static int Wrap(int index, int n)
        {
            return ((index % n) + n) % n;
        }
    }
}