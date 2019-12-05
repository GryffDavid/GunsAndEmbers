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

namespace TowerDefensePrototype
{
    public enum TrapType { Wall, Spikes, Catapult, Fire, Ice, Tar, Barrel, SawBlade };
    public enum TurretType { MachineGun, Cannon, FlameThrower, Lightning, Cluster };
    public enum InvaderType { Soldier, BatteringRam, Airship, Archer, Tank, Spider, Slime, SuicideBomber };
    public enum HeavyProjectileType { CannonBall, FlameThrower, Arrow, Acid, Torpedo, ClusterBomb, ClusterBombShell };
    public enum LightProjectileType { MachineGun, Freeze, Lightning };
    public enum GameState { Menu, Loading, Playing, Paused, ProfileSelect, Options, ProfileManagement, Tutorial, LoadingGame, GettingName };

    public struct StackedUpgrade 
    { 
        public float GatlingSpeed, GatlingDamage;
    };

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Variable declarations
        GraphicsDeviceManager graphics;
        ContentManager SecondaryContent;
        SpriteBatch spriteBatch;

        //XNA Declarations
        Texture2D BlankTexture, HUDBarTexture, ShellCasing, Coin, BigShellCasing, PauseMenuBackground, HealthBarSprite, ShieldBarSprite;

        Texture2D DefaultCursor, CrosshairCursor, FireCursor, WallCursor, SpikesCursor, BasicTurretCursor,
                  CannonTurretCursor, FlameThrowerCursor, CurrentCursorTexture, PrimaryCursorTexture,
                  SawbladeCursor, IceCursor;
        Vector2 CursorPosition, GroundCollisionPoint;
        Rectangle ScreenRectangle;
        SpriteFont ResourceFont;
        MouseState CurrentMouseState, PreviousMouseState;
        KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        int Resources, SelectedTurretIndex, TrapLimit, TowerButtons, ProfileNumber;
        string SelectButtonAssetName, TowerSlotAssetName, FileName, ContainerName, ProfileName;
        bool ReadyToPlace, IsLoading, Slow, WavesStarted;
        float MenuSFXVolume, MenuMusicVolume;
        float GatlingSpeed = 0;
        float CannonSpeed = 0;

        //Sound effects
        SoundEffect MenuClick, FireTrapStart, LightningSound;

        public Color HalfWhite = Color.Lerp(Color.White, Color.Transparent, 0.5f);

        Effect HealthBarEffect;

        public Color FireColor = new Color(Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 200);
        public Color FireColor2 = new Color(Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 90);

        static Random Random = new Random();

        int LevelNumber;
        Level CurrentLevel;
        Wave CurrentWave = null;
        int CurrentWaveIndex = 0;
        int CurrentInvaderIndex = 0;
        float WaveTime, CurrentWaveTime;

        #region List declarations
        List<Button> SelectButtonList, TowerButtonList, MainMenuButtonList, PauseButtonList,
                     ProfileButtonList, ProfileDeleteList, UpgradesButtonList;

        List<List<Button>> ChooseWeaponList;
        List<Button> ChooseWeaponSlots;

        List<Trap> TrapList;
        List<Turret> TurretList;
        List<Invader> InvaderList;

        List<HeavyProjectile> HeavyProjectileList, InvaderHeavyProjectileList;
        List<LightProjectile> LightProjectileList, InvaderLightProjectileList;
        List<TimerHeavyProjectile> TimedProjectileList;

        List<Emitter> EmitterList, EmitterList2;
        List<Particle> ShellCasingList, CoinList;

        List<string> MainMenuNameList, PauseMenuNameList, IconNameList, ProfileWeaponList;

        List<Explosion> ExplosionList, EnemyExplosionList;

        List<LightningBolt> LightningList;
        List<BulletTrail> TrailList;
        #endregion

        #region Custom class declarations
        Button ProfileBackButton, ProfileManagementPlay, ProfileManagementBack,
               OptionsBack, OptionsSFXUp, OptionsSFXDown, OptionsMusicUp, OptionsMusicDown,
               GetNameOK, GetNameBack, WaveStartButton;
        Tower Tower;
        StaticSprite Ground, TestBackground, ProfileMenuTitle, MainMenuBackground, SkyBackground, TextBox;
        AnimatedSprite LoadingAnimation;
        Nullable<TrapType> SelectedTrap;
        Nullable<TurretType> SelectedTurret;
        HorizontalBar TowerHealthBar;
        LightProjectile CurrentProjectile;
        Emitter DirtEmitter;
        GameState GameState;
        Thread LoadingThread;
        Profile CurrentProfile;
        StorageDevice Device;
        Stream OpenFile;
        SpriteFont ButtonFont, HUDFont;       
        Settings CurrentSettings, DefaultSettings;
        TextInput NameInput;
        DialogBox ExitDialog, DeleteProfileDialog, MainMenuDialog, ProfileMenuDialog;
        InformationBox WeaponInformation, UpgradeInformation;
        public StackedUpgrade StackedUpgrade = new StackedUpgrade();
        bool DialogVisible = false;
        #endregion
        #endregion

        public Game1()
        {
            DefaultSettings = new Settings
            {
                FullScreen = false,
                SFXVolume = 1.0f,
                MusicVolume = 1.0f
            };

            LoadSettings();

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.IsFullScreen = CurrentSettings.FullScreen;
            //graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";

            if (graphics.IsFullScreen == false)
            {
                IntPtr hWnd = this.Window.Handle;
                var control = System.Windows.Forms.Control.FromHandle(hWnd);
                var form = control.FindForm();
                form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
                //form.Text = "";
                form.ControlBox = false;
            }
        }

        protected override void Initialize()
        {
            SecondaryContent = new ContentManager(Content.ServiceProvider, Content.RootDirectory);
            GameState = GameState.Menu;
            ContainerName = "Profiles";
            TrapLimit = 8;            

            Tower = new Tower("Tower", new Vector2(32, 350), 2000, 1000, 10, 3);
            TowerButtons = (int)Tower.Slots;

            MainMenuNameList = new List<string>();
            MainMenuNameList.Add("Play");
            MainMenuNameList.Add("Options");
            MainMenuNameList.Add("Exit");

            MainMenuButtonList = new List<Button>();
            MainMenuButtonList.Add(new Button("Buttons/ButtonLeft", new Vector2(0, 130), null, null, null, "PLAY", "Fonts/ButtonFont", "Left", Color.White));
            MainMenuButtonList.Add(new Button("Buttons/ButtonLeft", new Vector2(0, 130 + ((64 + 50) * 1)), null, null, null, "TUTORIAL", "Fonts/ButtonFont", "Left", Color.White));
            MainMenuButtonList.Add(new Button("Buttons/ButtonLeft", new Vector2(0, 130 + ((64 + 50) * 2)), null, null, null, "OPTIONS", "Fonts/ButtonFont", "Left", Color.White));
            MainMenuButtonList.Add(new Button("Buttons/ButtonLeft", new Vector2(0, 130 + ((64 + 50) * 3)), null, null, null, "CREDITS", "Fonts/ButtonFont", "Left", Color.White));
            MainMenuButtonList.Add(new Button("Buttons/ButtonLeft", new Vector2(0, 720 - 50 - 32), null, null, null, "EXIT", "Fonts/ButtonFont", "Left", Color.White));

            MainMenuBackground = new StaticSprite("Backgrounds/MainMenuBackground", new Vector2(0, 0));

            OptionsSFXUp = new Button("Buttons/RightArrow", new Vector2(640 + 32, 316));
            OptionsSFXUp.LoadContent(SecondaryContent);

            OptionsSFXDown = new Button("Buttons/LeftArrow", new Vector2(640 - 50 - 32, 316));
            OptionsSFXDown.LoadContent(SecondaryContent);

            OptionsMusicUp = new Button("Buttons/RightArrow", new Vector2(640 + 32, 380));
            OptionsMusicUp.LoadContent(SecondaryContent);

            OptionsMusicDown = new Button("Buttons/LeftArrow", new Vector2(640 - 50 - 32, 380));
            OptionsMusicDown.LoadContent(SecondaryContent);

            foreach (Button button in MainMenuButtonList)
            {
                button.LoadContent(SecondaryContent);
            }

            ProfileButtonList = new List<Button>();
            for (int i = 0; i < 4; i++)
            {
                ProfileButtonList.Add(new Button("Buttons/ButtonLeft", new Vector2(50, 130 + (i * 114)), null, null, null, "empty", "Fonts/ButtonFont", "Centre", Color.White));
                ProfileButtonList[i].LoadContent(SecondaryContent);
            }

            ProfileDeleteList = new List<Button>();
            for (int i = 0; i < 4; i++)
            {
                ProfileDeleteList.Add(new Button("Buttons/SmallButton", new Vector2(0, 130 + (i * 114)), null, null, null, "X", "Fonts/ButtonFont", "Left", Color.White));
                ProfileDeleteList[i].LoadContent(SecondaryContent);
            }

            UpgradesButtonList = new List<Button>();
            for (int i = 0; i < 6; i++)
            {
                UpgradesButtonList.Add(new Button("Buttons/NewButton", new Vector2(100 + (73 * i), 100)));
                UpgradesButtonList[i].LoadContent(SecondaryContent);
            }

            ChooseWeaponList = new List<List<Button>>();

            for (int i = 0; i < 2; i++)//Rows
            {
                List<Button> SubList = new List<Button>();
                for (int k = 0; k < 8; k++) //Columns
                {
                    Button button = new Button("Buttons/NewButton", new Vector2(32 + (80 * k), 720 - 272 + (70 * i)), "Icons/LockIcon");
                    button.LoadContent(SecondaryContent);
                    SubList.Add(button);
                }

                ChooseWeaponList.Add(SubList);
            }

            ChooseWeaponSlots = new List<Button>();
            for (int i = 0; i < 10; i++)
            {
                Button button = new Button("Buttons/NewButton", new Vector2(64 + (80 * i), 360), null, null, null, "", "", null, null, true);
                button.LoadContent(SecondaryContent);
                ChooseWeaponSlots.Add(button);
            }

            TextBox = new StaticSprite("Buttons/TextBox", new Vector2((1280 / 2) - 225, (720 / 2) - 50));
            TextBox.LoadContent(SecondaryContent);

            ProfileBackButton = new Button("Buttons/ButtonRight", new Vector2(1280 - 450, 720 - 32 - 50), null, null, null, "Back", "Fonts/ButtonFont", "Right", Color.White);
            ProfileBackButton.LoadContent(SecondaryContent);

            NameInput = new TextInput(new Vector2(415 + 4, 310 + 4), 350, "Fonts/ButtonFont", Color.White);
            NameInput.LoadContent(SecondaryContent);

            GetNameBack = new Button("Buttons/ButtonLeft", new Vector2(0, 720 - 32 - 50), null, null, null, "Back", "Fonts/ButtonFont", "Left", Color.White);
            GetNameBack.LoadContent(SecondaryContent);

            GetNameOK = new Button("Buttons/ButtonRight", new Vector2(1280 - 450, 720 - 32 - 50), null, null, null, "Create", "Fonts/ButtonFont", "Right", Color.White);
            GetNameOK.LoadContent(SecondaryContent);

            ProfileManagementPlay = new Button("Buttons/ButtonRight", new Vector2(1280 - 450, 720 - 32 - 50), null, null, null, "Play", "Fonts/ButtonFont", "Right", Color.White);
            ProfileManagementPlay.LoadContent(SecondaryContent);

            ProfileManagementBack = new Button("Buttons/ButtonLeft", new Vector2(0, 720 - 32 - 50), null, null, null, "Back", "Fonts/ButtonFont", "Left", Color.White);
            ProfileManagementBack.LoadContent(SecondaryContent);

            OptionsBack = new Button("Buttons/ButtonLeft", new Vector2(0, 720 - 32 - 50), null, null, null, "Back", "Fonts/ButtonFont", "Left", Color.White);
            OptionsBack.LoadContent(SecondaryContent);

            ProfileMenuTitle = new StaticSprite("ProfileMenuTitle", new Vector2(0, 32));
            ProfileMenuTitle.LoadContent(SecondaryContent);

            ScreenRectangle = new Rectangle(-128, -128, 1408, 848);

            LoadingAnimation = new AnimatedSprite("LoadingAnimation", new Vector2(640 - 65, 320 - 65), new Vector2(131, 131), 17, 30, HalfWhite, Vector2.One, true);
            LoadingAnimation.LoadContent(SecondaryContent);

            ProfileWeaponList = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                ProfileWeaponList.Add(null);
            }

            IsLoading = false;

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            //This is just the stuff that needs to be updated every step
            //This is where I call all the smaller procedures that I broke the update into             
            CurrentMouseState = Mouse.GetState();
            CursorPosition = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);
            CurrentKeyboardState = Keyboard.GetState();

            if (WeaponInformation != null)
                WeaponInformation.Update();

            Slow = gameTime.IsRunningSlowly;

            if (GameState == GameState.Playing && IsLoading == false)
            {
                if (CurrentMouseState.LeftButton == ButtonState.Released &&
                    CurrentKeyboardState.IsKeyUp(Keys.Escape) &&
                    PreviousKeyboardState.IsKeyDown(Keys.Escape))
                {
                    GameState = GameState.Paused;
                }

                InvaderUpdate(gameTime);

                RightClickClearSelected();

                SelectButtonsUpdate();

                TowerButtonUpdate();                

                AttackTower();
                AttackTraps();

                RangedAttackTower();
                RangedAttackTraps();

                if (WavesStarted == false)
                WaveStartButton.Update();

                if (WaveStartButton.JustClicked == true)
                    WavesStarted = true;

                HandleWaves(gameTime);

                TowerHealthBar.Update(new Vector2(90, 19), (int)Tower.CurrentHP);

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

                foreach (Emitter emitter in EmitterList2)
                {
                    emitter.Update(gameTime);
                }

                for (int i = 0; i < EmitterList2.Count; i++)
                {
                    if (EmitterList2[i].Active == false)
                        EmitterList2.RemoveAt(i);
                }

                foreach (Emitter emitter in EmitterList)
                {
                    emitter.Update(gameTime);
                }

                for (int i = 0; i < EmitterList.Count; i++)
                {
                    if (EmitterList[i].Active == false)
                        EmitterList.RemoveAt(i);
                }

                //This just handles the shell casings which aren't created by an emitter, they are just in a list
                //Because they have to only be emitted every time the Machine Gun is fired
                foreach (Particle shellCasing in ShellCasingList)
                {
                    shellCasing.Update();
                }

                foreach (Particle coin in CoinList)
                {
                    coin.Update();
                }

                for (int i = 0; i < ExplosionList.Count; i++)
                {
                    if (ExplosionList[i].Active == false)
                        ExplosionList.RemoveAt(i);
                }

                if (ExplosionList.Count > 0)
                    foreach (Explosion explosion in ExplosionList)
                    {
                        explosion.Active = false;
                    }
                #endregion

                #region Projectile update stuff
                HeavyProjectileUpdate(gameTime);
                TimedProjectileUpdate(gameTime);
                LightProjectileUpdate();

                InvaderHeavyProjectileUpdate(gameTime);

                for (int i = 0; i < TimedProjectileList.Count; i++)
                {
                    if (TimedProjectileList[i].Active == false)
                        TimedProjectileList.RemoveAt(i);
                }

                for (int i = 0; i < LightProjectileList.Count; i++)
                {
                    if (LightProjectileList[i].Active == false)
                        LightProjectileList.RemoveAt(i);
                }

                for (int i = 0; i < HeavyProjectileList.Count; i++)
                {
                    if (HeavyProjectileList[i].Active == false && HeavyProjectileList[i].Emitter.ParticleList.Count == 0)
                        HeavyProjectileList.RemoveAt(i);
                }

                for (int i = 0; i < InvaderHeavyProjectileList.Count; i++)
                {
                    if (InvaderHeavyProjectileList[i].Active == false && InvaderHeavyProjectileList[i].Emitter.ParticleList.Count == 0)
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
                            turret.Update(gameTime);
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
                    lightningBolt.Update();
                }

                foreach (BulletTrail trail in TrailList)
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
                #endregion

                EnemyExplosionsUpdate(gameTime);
            }

            MenuButtonsUpdate();

            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;

            LoadingAnimation.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            #region Spritebatch
            spriteBatch.Begin();

            Color backgroundColor = new Color(22, 60, 90);
            GraphicsDevice.Clear(backgroundColor);

            #region Draw Profile Select Menu
            if (GameState == GameState.ProfileSelect)
            {
                MainMenuBackground.Draw(spriteBatch);

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
                MainMenuBackground.Draw(spriteBatch);
                ProfileMenuTitle.Draw(spriteBatch);
                spriteBatch.DrawString(ButtonFont, CurrentProfile.Name, new Vector2(16, 37), HalfWhite);
                //spriteBatch.DrawString(ProfileFont, CurrentProfile.Name, new Vector2(32, 16), Color.White);
                //spriteBatch.DrawString(ProfileFont, "Level: " + CurrentProfile.LevelNumber.ToString(), new Vector2(500, 16), Color.White);
                spriteBatch.DrawString(ButtonFont, "Points: " + CurrentProfile.Points.ToString(), new Vector2(640, 64), Color.White);
                spriteBatch.DrawString(ButtonFont, "Shots: " + CurrentProfile.ShotsFired.ToString(), new Vector2(640, 128), Color.White);
                spriteBatch.DrawString(ButtonFont, SelectedTurret.ToString(), new Vector2(0, 0), Color.Red);
                spriteBatch.DrawString(ButtonFont, SelectedTrap.ToString(), new Vector2(0, 0), Color.Red);

                foreach (Button button in UpgradesButtonList)
                {
                    button.Draw(spriteBatch);
                }

                foreach (List<Button> list in ChooseWeaponList)
                {
                    foreach (Button button in list)
                    {
                        button.Draw(spriteBatch);
                    }
                }

                foreach (Button button in ChooseWeaponSlots)
                {
                    button.Draw(spriteBatch);
                }

                ProfileManagementPlay.Draw(spriteBatch);
                ProfileManagementBack.Draw(spriteBatch);

                if (UpgradeInformation != null)
                    UpgradeInformation.Draw(spriteBatch);
            }
            #endregion

            #region Draw Game
            if (GameState == GameState.Playing || GameState == GameState.Paused && IsLoading == false)
            {
                SkyBackground.Draw(spriteBatch);
                TestBackground.Draw(spriteBatch);
                Ground.Draw(spriteBatch);
                Tower.Draw(spriteBatch);

                if (WavesStarted == false)
                WaveStartButton.Draw(spriteBatch);

                #region Draw Traps, Invaders and Turrets

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

                //float TextSize = ResourceFont.MeasureString(CurrentProfile.Name).X;
                //spriteBatch.DrawString(ResourceFont, CurrentProfile.Name, new Vector2(1280 - (TextSize + 4), 0), Color.White);                

                #region Drawing buttons
                spriteBatch.Draw(HUDBarTexture, new Rectangle(graphics.PreferredBackBufferWidth / 2, 720, HUDBarTexture.Width, HUDBarTexture.Height), null, Color.White, 0, new Vector2(HUDBarTexture.Width / 2, HUDBarTexture.Height), SpriteEffects.None, 0);

                foreach (Button button in SelectButtonList)
                {
                    button.Draw(spriteBatch);
                }

                foreach (Button towerSlot in TowerButtonList)
                {
                    towerSlot.Draw(spriteBatch);
                }
                #endregion

                foreach (Emitter emitter in EmitterList2)
                {
                    emitter.Draw(spriteBatch);
                }

                spriteBatch.DrawString(HUDFont, HeavyProjectileList.Count.ToString(), new Vector2(10, 720 - 30), Color.White);

                int PercentageHP = (int)MathHelper.Clamp((float)((100d / (double)Tower.MaxHP) * (double)Tower.CurrentHP), 0, 100);
                int PercentageShield = (int)MathHelper.Clamp((float)((100d / (double)Tower.MaxShield) * (double)Tower.CurrentShield), 0, 100);

                Vector2 HPSize, ShieldSize;
                HPSize = HUDFont.MeasureString(PercentageHP.ToString());
                ShieldSize = HUDFont.MeasureString(PercentageShield.ToString());

                spriteBatch.DrawString(HUDFont, PercentageHP.ToString(), new Vector2(80 - (HPSize.X / 2), 64), Color.White);
                spriteBatch.DrawString(HUDFont, PercentageShield.ToString(), new Vector2(80 - (ShieldSize.X / 2), 80), Color.White);
                spriteBatch.DrawString(HUDFont, Resources.ToString(), new Vector2(0,100), Color.White);
            }
            #endregion

            #region Draw Loading Screen
            if (GameState == GameState.Loading)
            {
                MainMenuBackground.Draw(spriteBatch);
                LoadingAnimation.Draw(spriteBatch);
            }
            #endregion

            #region Draw Main Menu
            if (GameState == GameState.Menu)
            {
                MainMenuBackground.Draw(spriteBatch);
                foreach (Button button in MainMenuButtonList)
                {
                    button.Draw(spriteBatch);
                }
            }
            #endregion

            #region Draw Options Menu
            if (GameState == GameState.Options)
            {
                MainMenuBackground.Draw(spriteBatch);

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

                spriteBatch.DrawString(ButtonFont, SFXVol, new Vector2(640 - 16, 320), Color.White);
                spriteBatch.DrawString(ButtonFont, MusicVol, new Vector2(640 - 16, 384), Color.White);
            }
            #endregion

            #region Draw GetName Menu
            if (GameState == GameState.GettingName)
            {
                MainMenuBackground.Draw(spriteBatch);
                spriteBatch.DrawString(ButtonFont, "Enter profile name:", new Vector2(TextBox.Position.X, TextBox.Position.Y - ButtonFont.MeasureString("E").Y), Color.White);
                TextBox.Draw(spriteBatch);

                GetNameBack.Draw(spriteBatch);
                GetNameOK.Draw(spriteBatch);

                NameInput.Draw(spriteBatch);
            }
            #endregion

            if (WeaponInformation != null)
                WeaponInformation.Draw(spriteBatch);

            spriteBatch.End();
            #endregion

            #region Draw the lightning with additive blending
            spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive);
            if (GameState == GameState.Playing || GameState == GameState.Paused && IsLoading == false)
            {
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
            }
            spriteBatch.End();
            #endregion

            #region Draw things sorted according to their Y value - To create depth illusion
            //This second spritebatch sorts everthing Back to Front, 
            //to make sure that the invaders are drawn correctly according to their Y value
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

            if (GameState == GameState.Playing || GameState == GameState.Paused && IsLoading == false)
            {

                //This draws the timing bar for the turrets, but also makes 
                //sure that it doesn't draw for the blank turrets, which
                //would make the timing bar appear in the top right corner
                foreach (Turret turret in TurretList)
                {
                    if (turret != null)
                        turret.TimingBar.Draw(spriteBatch);
                }

                foreach (Turret turret in TurretList)
                {
                    if (turret != null)
                        if (turret.Active == true)
                            turret.Draw(spriteBatch);
                }

                foreach (Invader invader in InvaderList)
                {
                    invader.Draw(spriteBatch);
                }

                foreach (Particle shellCasing in ShellCasingList)
                {
                    shellCasing.Draw(spriteBatch);
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
                    heavyProjectile.Draw(spriteBatch);
                }

                foreach (HeavyProjectile heavyProjectile in InvaderHeavyProjectileList)
                {
                    heavyProjectile.Draw(spriteBatch);
                }

                foreach (TimerHeavyProjectile timedProjectile in TimedProjectileList)
                {
                    timedProjectile.Draw(spriteBatch);
                }
            }

            CursorDraw(spriteBatch);

            spriteBatch.End();
            #endregion

            double HealthValue;
            #region Health SpriteBatch
            if (GameState == GameState.Playing || GameState == GameState.Paused && IsLoading == false)
            {
                HealthValue = (double)Tower.CurrentHP / (double)Tower.MaxHP;

                HealthBarEffect.Parameters["meterValue"].SetValue((float)HealthValue);

                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive, SamplerState.LinearWrap,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, HealthBarEffect);

                spriteBatch.Draw(HealthBarSprite, new Vector2(80, 80), null, new Color(255, 255, 255, 522), MathHelper.ToRadians(-90), new Vector2(HealthBarSprite.Width / 2, HealthBarSprite.Height / 2), 1f, SpriteEffects.None, 1);

                spriteBatch.End();
            }
            #endregion

            double ShieldValue;
            #region Shield SpriteBatch
            if (GameState == GameState.Playing || GameState == GameState.Paused && IsLoading == false)
            {
                ShieldValue = (double)Tower.CurrentShield / (double)Tower.MaxShield;

                HealthBarEffect.Parameters["meterValue"].SetValue((float)ShieldValue);

                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive, SamplerState.LinearWrap,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, HealthBarEffect);

                spriteBatch.Draw(ShieldBarSprite, new Vector2(80, 80), null, new Color(255, 255, 255, 255), MathHelper.ToRadians(90), new Vector2(HealthBarSprite.Width / 2, HealthBarSprite.Height / 2), 1f, SpriteEffects.None, 1);

                spriteBatch.End();
            }
            #endregion

            spriteBatch.Begin();
            #region Draw Pause Menu
            if (GameState == GameState.Paused)
            {
                spriteBatch.Draw(PauseMenuBackground, new Rectangle(0, 0, PauseMenuBackground.Width, PauseMenuBackground.Height), Color.White);

                foreach (Button button in PauseButtonList)
                {
                    button.Draw(spriteBatch);
                }
            }
            #endregion

            #region Draw Dialog Boxes
            if (ExitDialog != null)
            {
                if (GameState == GameState.Menu)
                {
                    spriteBatch.Draw(PauseMenuBackground, new Rectangle(0, 0, PauseMenuBackground.Width, PauseMenuBackground.Height), Color.White);
                    ExitDialog.Draw(spriteBatch);
                }
                else
                {
                    ExitDialog.Draw(spriteBatch);
                }
            }

            if (DeleteProfileDialog != null)
            {
                spriteBatch.Draw(PauseMenuBackground, new Rectangle(0, 0, PauseMenuBackground.Width, PauseMenuBackground.Height), Color.White);
                DeleteProfileDialog.Draw(spriteBatch);
            }

            if (MainMenuDialog != null)
            {
                MainMenuDialog.Draw(spriteBatch);
            }

            if (ProfileMenuDialog != null)
            {
                ProfileMenuDialog.Draw(spriteBatch);
            }
            #endregion

            CursorDraw(spriteBatch);

            spriteBatch.DrawString(HUDFont, Slow.ToString(), Vector2.Zero, Color.Red);
            spriteBatch.End();
        }

        #region Handle Game Content
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            MainMenuBackground.LoadContent(SecondaryContent);

            BlankTexture = SecondaryContent.Load<Texture2D>("Blank");

            DefaultCursor = SecondaryContent.Load<Texture2D>("Cursors/DefaultCursor");
            CrosshairCursor = SecondaryContent.Load<Texture2D>("Cursors/Crosshair");
            FireCursor = SecondaryContent.Load<Texture2D>("Icons/FireIcon");
            WallCursor = SecondaryContent.Load<Texture2D>("Icons/WallIcon");
            SpikesCursor = SecondaryContent.Load<Texture2D>("Icons/SpikesIcon");
            SawbladeCursor = SecondaryContent.Load<Texture2D>("Icons/SawbladeIcon");
            IceCursor = SecondaryContent.Load<Texture2D>("Icons/SnowFlakeIcon");

            BasicTurretCursor = SecondaryContent.Load<Texture2D>("Icons/BasicTurretIcon2");
            CannonTurretCursor = SecondaryContent.Load<Texture2D>("Icons/CannonTurretIcon");
            FlameThrowerCursor = SecondaryContent.Load<Texture2D>("Icons/FlameThrowerTurretIcon");

            ButtonFont = SecondaryContent.Load<SpriteFont>("Fonts/ButtonFont");
            HUDFont = SecondaryContent.Load<SpriteFont>("Fonts/HUDFont");

            MenuClick = SecondaryContent.Load<SoundEffect>("Sounds/MenuClick");

            PauseMenuBackground = SecondaryContent.Load<Texture2D>("Backgrounds/PauseMenuBackground");
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
                HUDBarTexture = Content.Load<Texture2D>("Backgrounds/NewInterface");

                HealthBarEffect = Content.Load<Effect>("HealthBarEffect");
                HealthBarSprite = Content.Load<Texture2D>("HealthBar");
                ShieldBarSprite = Content.Load<Texture2D>("ShieldBar");

                IsLoading = true;

                ReadyToPlace = false;

                Ground = new StaticSprite("Ground7", new Vector2(0, 0));
                Ground.LoadContent(Content);
                Ground.Position = new Vector2(0, 720 - 200);

                TestBackground = new StaticSprite("Backgrounds/TestBackground", new Vector2(0, 335));
                TestBackground.Scale = new Vector2(1, 0.8f);
                TestBackground.LoadContent(Content);

                SelectButtonAssetName = "Buttons/NewButton";
                TowerSlotAssetName = "Buttons/TurretSlotButton";

                SkyBackground = new StaticSprite("Backgrounds/GradientBackground", Vector2.Zero);
                SkyBackground.LoadContent(Content);

                #region IconNameList, PauseMenuNameList;
                //This gets the names of the icons that are to appear on the
                //buttons that allow the player to select traps/turrets they want to place
                IconNameList = new List<string>();

                for (int i = 0; i < 10; i++)
                {
                    IconNameList.Add(null);
                }

                for (int i = 0; i < CurrentProfile.Buttons.Count; i++)
                {
                    int Index = i;

                    switch (CurrentProfile.Buttons[Index])
                    {
                        case null:
                            IconNameList[Index] = "Blank";
                            break;

                        case "FireTrap":
                            IconNameList[Index] = "Icons/FireIcon";
                            break;

                        case "Wall":
                            IconNameList[Index] = "Icons/WallIcon";
                            break;

                        case "Cannon":
                            IconNameList[Index] = "Icons/CannonTurretIcon";
                            break;

                        case "MachineGun":
                            IconNameList[Index] = "Icons/MachineGunIcon";
                            break;

                        case "IceTrap":
                            IconNameList[Index] = "Icons/SnowFlakeIcon";
                            break;
                    }

                }

                PauseMenuNameList = new List<string>();
                PauseMenuNameList.Add("Resume Game");
                PauseMenuNameList.Add("Options");
                PauseMenuNameList.Add("Main Menu");
                PauseMenuNameList.Add("Profile Menu");
                PauseMenuNameList.Add("Exit");
                #endregion

                LoadGameSounds();

                Resources = 1000;

                TowerHealthBar = new HorizontalBar(Content, new Vector2(220, 20), (int)Tower.MaxHP, (int)Tower.CurrentHP);
                ShellCasing = Content.Load<Texture2D>("Particles/Shell");
                Coin = Content.Load<Texture2D>("Particles/Coin");
                BigShellCasing = Content.Load<Texture2D>("Particles/BigShell");
                ResourceFont = Content.Load<SpriteFont>("Fonts/ButtonFont");

                Tower.LoadContent(Content);

                #region Setting up the buttons
                SelectButtonList = new List<Button>();
                TowerButtonList = new List<Button>();
                PauseButtonList = new List<Button>();

                for (int i = 0; i < TowerButtons; i++)
                {
                    TowerButtonList.Add(new Button(TowerSlotAssetName, new Vector2(48 + 64 + 32 + 8, 350 + ((38 + 90) * i) - 32)));
                    TowerButtonList[i].LoadContent(Content);
                }

                for (int i = 0; i < 10; i++)
                {
                    Button button = new Button(SelectButtonAssetName, new Vector2(270 + (i * 73), 720 - HUDBarTexture.Height + 15), IconNameList[i], null, null, "", "", "Left", null, false);
                    button.LoadContent(Content);
                    SelectButtonList.Add(button);
                }

                for (int i = 0; i < 4; i++)
                {
                    PauseButtonList.Add(new Button("Buttons/ButtonLeft", new Vector2(0, 130 + ((64 + 50) * i)), null, null, null, PauseMenuNameList[i], "Fonts/ButtonFont", "Left", Color.White));
                    PauseButtonList[i].LoadContent(Content);
                }

                PauseButtonList.Add(new Button("Buttons/ButtonLeft", new Vector2(0, 720 - 50 - 32), null, null, null, PauseMenuNameList[4], "Fonts/ButtonFont", "Left", Color.White));
                PauseButtonList[4].LoadContent(Content);

                WaveStartButton = new Button("Buttons/ButtonRight", new Vector2(1280 - 225, 0), null, new Vector2(0.5f, 1), null, "Ready", "Fonts/ButtonFont", "Right", null, false);
                WaveStartButton.LoadContent(Content);
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

                foreach (Invader invader in InvaderList)
                {
                    invader.LoadContent(Content);
                }

                HeavyProjectileList = new List<HeavyProjectile>();
                LightProjectileList = new List<LightProjectile>();
                TimedProjectileList = new List<TimerHeavyProjectile>();
                InvaderHeavyProjectileList = new List<HeavyProjectile>();
                InvaderLightProjectileList = new List<LightProjectile>();
                LightningList = new List<LightningBolt>();
                TrailList = new List<BulletTrail>();                
                EmitterList = new List<Emitter>();
                EmitterList2 = new List<Emitter>();
                ExplosionList = new List<Explosion>();
                EnemyExplosionList = new List<Explosion>();
                ShellCasingList = new List<Particle>();
                CoinList = new List<Particle>();
                #endregion

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
        }
        #endregion


        #region BUTTON stuff that needs to be done every step
        private void TowerButtonUpdate()
        {
            //This places the selected turret type into the right slot on the tower when the tower slot has been clicked
            int Index;

            foreach (Button towerButton in TowerButtonList)
            {
                towerButton.Update();

                if (towerButton.JustClicked == true)
                {
                    Index = TowerButtonList.IndexOf(towerButton);
                    switch (SelectedTurret)
                    {
                        //The upgrades for the turrets are applied before the LoadContent method is called to ensure that the correct values
                        //are used. Otherwise strange things happen like the timing bars not matching the actual timing.
                        case TurretType.MachineGun:

                            if (Resources >= new GatlingTurret(Vector2.Zero).ResourceCost)
                            {
                                TowerButtonList[Index].ButtonActive = false;
                                TurretList[Index] = new GatlingTurret(TowerButtonList[Index].Position); //Fix this to make sure that the BasicTurret has the resource names built in.

                                //Apply the upgrades for the gatling turret in here
                                TurretList[Index].FireDelay = PercentageChange(TurretList[Index].FireDelay, GatlingSpeed);
                                TurretList[Index].Damage = (int)PercentageChange(TurretList[Index].Damage, 0);

                                TurretList[Index].LoadContent(Content);
                                Resources -= new GatlingTurret(Vector2.Zero).ResourceCost;
                                SelectedTurret = null;
                                TurretList[Index].Selected = false;
                            }
                            break;

                        case TurretType.Cannon:
                            if (Resources >= new CannonTurret(Vector2.Zero).ResourceCost)
                            {
                                TowerButtonList[Index].ButtonActive = false;
                                TurretList[Index] = new CannonTurret(TowerButtonList[Index].Position);

                                //Apply the upgrades for the cannon turret in here
                                TurretList[Index].FireDelay = PercentageChange(TurretList[Index].FireDelay, CannonSpeed);
                                TurretList[Index].Damage = (int)PercentageChange(TurretList[Index].Damage, 0);

                                TurretList[Index].LoadContent(Content);
                                Resources -= new CannonTurret(Vector2.Zero).ResourceCost;
                                SelectedTurret = null;
                                TurretList[Index].Selected = false;


                            }
                            break;

                        case TurretType.FlameThrower:
                            if (Resources >= new FlameThrowerTurret(Vector2.Zero).ResourceCost)
                            {
                                TowerButtonList[Index].ButtonActive = false;
                                TurretList[Index] = new FlameThrowerTurret(TowerButtonList[Index].Position);

                                //Apply the upgrades for the flamethrower turret in here

                                TurretList[Index].LoadContent(Content);
                                Resources -= new FlameThrowerTurret(Vector2.Zero).ResourceCost;
                                SelectedTurret = null;
                                TurretList[Index].Selected = false;

                            }
                            break;

                        case TurretType.Lightning:
                            if (Resources >= new LightningTurret(Vector2.Zero).ResourceCost)
                            {
                                TowerButtonList[Index].ButtonActive = false;
                                TurretList[Index] = new LightningTurret(TowerButtonList[Index].Position);

                                //Apply the upgrades for the flamethrower turret in here

                                TurretList[Index].LoadContent(Content);
                                Resources -= new LightningTurret(Vector2.Zero).ResourceCost;
                                SelectedTurret = null;
                                TurretList[Index].Selected = false;
                            }
                            break;

                        case TurretType.Cluster:
                            if (Resources >= new ClusterTurret(Vector2.Zero).ResourceCost)
                            {
                                TowerButtonList[Index].ButtonActive = false;
                                TurretList[Index] = new ClusterTurret(TowerButtonList[Index].Position);

                                //Apply the upgrades for the flamethrower turret in here

                                TurretList[Index].LoadContent(Content);
                                Resources -= new ClusterTurret(Vector2.Zero).ResourceCost;
                                SelectedTurret = null;
                                TurretList[Index].Selected = false;
                            }
                            break;
                    }
                }
            }

        }

        private void SelectButtonsUpdate()
        {
            //This makes sure that when the button at the bottom of the screen is clicked, the corresponding trap or turret is actually selected//
            //This will code will need to be added to every time that a new trap/turret is added to the game.

            int Index;

            foreach (Button button in SelectButtonList)
            {
                button.Update();

                if (button.JustClicked == true)
                {
                    ClearTurretSelect();
                    Index = SelectButtonList.IndexOf(button);

                    Action CheckLayout = new Action(() =>
                    {
                        if (Index <= CurrentProfile.Buttons.Count - 1)
                        {
                            switch (CurrentProfile.Buttons[Index])
                            {
                                case null:
                                    {
                                        SelectedTrap = null;
                                        SelectedTurret = null;
                                    }
                                    break;

                                case "":
                                    {
                                        SelectedTrap = null;
                                        SelectedTurret = null;
                                    }
                                    break;

                                case "FireTrap":
                                    {
                                        if (CurrentProfile.Fire == true && Resources >= new FireTrap(Vector2.Zero).ResourceCost)
                                        {
                                            SelectedTrap = TrapType.Barrel;
                                            SelectedTurret = null;
                                        }
                                    }
                                    break;

                                case "IceTrap":
                                    {
                                        if (CurrentProfile.Ice == true && Resources >= new IceTrap(Vector2.Zero).ResourceCost)
                                        {
                                            SelectedTrap = TrapType.Ice;
                                        }
                                    }
                                    break;

                                case "Wall":
                                    {
                                        if (CurrentProfile.Wall == true && Resources >= new Wall(Vector2.Zero).ResourceCost)
                                        {
                                            SelectedTrap = TrapType.Wall;
                                            SelectedTurret = null;
                                        }
                                    }
                                    break;

                                case "MachineGunTurret":
                                    {
                                        if (CurrentProfile.Cannon == true && Resources >= new GatlingTurret(Vector2.Zero).ResourceCost)
                                        {
                                            SelectedTrap = null;
                                            SelectedTurret = TurretType.MachineGun;
                                        }
                                    }
                                    break;

                                case "CannonTurret":
                                    {
                                        if (CurrentProfile.Cannon == true && Resources >= new CannonTurret(Vector2.Zero).ResourceCost)
                                        {
                                            SelectedTrap = null;
                                            SelectedTurret = TurretType.Cluster;
                                        }
                                    }
                                    break;

                                case "SawBlade":
                                    {
                                        if (CurrentProfile.SawBlade == true && Resources >= new SawBladeTrap(Vector2.Zero).ResourceCost)
                                        {
                                            SelectedTrap = TrapType.SawBlade;
                                            SelectedTurret = null;
                                        }
                                    }
                                    break;

                                case "FlameThrowerTurret":
                                    {
                                        if (CurrentProfile.FlameThrower == true && Resources >= new FlameThrowerTurret(Vector2.Zero).ResourceCost)
                                        {
                                            SelectedTrap = null;
                                            SelectedTurret = TurretType.FlameThrower;
                                        }
                                    }
                                    break;

                                case "LightningTurret":
                                    {
                                        if (CurrentProfile.LightningTurret == true && Resources >= new LightningTurret(Vector2.Zero).ResourceCost)
                                        {
                                            SelectedTrap = null;
                                            SelectedTurret = TurretType.Lightning;
                                        }
                                    }
                                    break;

                                case "ClusterTurret":
                                    {
                                        if (CurrentProfile.ClusterTurret == true && Resources >= new ClusterTurret(Vector2.Zero).ResourceCost)
                                        {
                                            SelectedTrap = null;
                                            SelectedTurret = TurretType.Cluster;
                                        }
                                    }
                                    break;
                            }
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
                            ReadyToPlace = true;
                            break;
                    }
                }
            }
        }

        private void MenuButtonsUpdate()
        {
            #region Handling Main Menu Button Presses
            if (GameState == GameState.Menu && DialogVisible == false)
            {
                int Index;

                foreach (Button button in MainMenuButtonList)
                {
                    button.Update();

                    if (button.JustClicked == true)
                    {
                        Index = MainMenuButtonList.IndexOf(button);
                        MenuClick.Play();

                        switch (Index)
                        {
                            case 0:
                                GameState = GameState.ProfileSelect;
                                SetProfileNames();
                                break;

                            case 1:

                                break;

                            case 2:
                                GameState = GameState.Options;
                                MenuSFXVolume = CurrentSettings.SFXVolume * 10;
                                MenuMusicVolume = CurrentSettings.MusicVolume * 10;
                                break;

                            case 3:

                                break;

                            case 4:
                                ExitDialog = new DialogBox(new Vector2(1280 / 2, 720 / 2), "Yes", "Do you want to exit?", "No");
                                ExitDialog.LoadContent(SecondaryContent);
                                DialogVisible = true;
                                break;
                        }
                    }
                }
            }
            else
            {
                foreach (Button button in MainMenuButtonList)
                {
                    button.CurrentButtonState = ButtonSpriteState.Released;
                }
            }
            #endregion

            #region Handling Pause Menu Button Presses
            if (GameState == GameState.Paused && DialogVisible == false)
            {
                int Index;

                foreach (Button button in PauseButtonList)
                {
                    button.Update();

                    if (button.JustClicked == true)
                    {
                        MenuClick.Play();

                        Index = PauseButtonList.IndexOf(button);

                        switch (Index)
                        {
                            case 0:
                                GameState = GameState.Playing;
                                break;

                            case 1:

                                break;

                            case 2:
                                MainMenuDialog = new DialogBox(new Vector2(1280 / 2, 720 / 2), "Yes", "Are you sure you want to return to the main menu? All progress will be lost.", "No");
                                MainMenuDialog.LoadContent(SecondaryContent);
                                DialogVisible = true;
                                break;

                            case 3:
                                ProfileMenuDialog = new DialogBox(new Vector2(1280 / 2, 720 / 2), "Yes", "Are you sure you want to return to your profile menu? All progress will be lost.", "No");
                                ProfileMenuDialog.LoadContent(SecondaryContent);
                                DialogVisible = true;
                                break;

                            case 4:
                                ExitDialog = new DialogBox(new Vector2(1280 / 2, 720 / 2), "Yes", "Do you want to exit?", "No");
                                ExitDialog.LoadContent(SecondaryContent);
                                DialogVisible = true;
                                break;
                        }
                    }
                }
            }
            #endregion

            #region Handling Profile Select Menu Button Presses
            if (GameState == GameState.ProfileSelect && DialogVisible == false)
            {
                int Index;

                foreach (Button button in ProfileButtonList)
                {
                    button.Update();

                    if (button.JustClicked == true)
                    {

                        MenuClick.Play();

                        Index = ProfileButtonList.IndexOf(button);

                        foreach (Button button2 in ChooseWeaponSlots)
                        {
                            button2.IconName = null;
                        }
                        
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
                    button.Update();

                    if (button.JustClicked == true)
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
                }

                ProfileBackButton.Update();

                if (ProfileBackButton.JustClicked == true)
                {
                    MenuClick.Play();

                    GameState = GameState.Menu;
                }
            }
            #endregion

            #region Handling Profile Management Button Presses
            if (GameState == GameState.ProfileManagement)
            {
                string text;

                RightClickClearSelected();

                ProfileManagementPlay.Update();
                ProfileManagementBack.Update();

                foreach (Button button in ChooseWeaponSlots)
                {
                    int index = ChooseWeaponSlots.IndexOf(button);

                    button.Update();

                    switch (ProfileWeaponList[index])
                    {
                        case "MachineGunTurret":
                            ChooseWeaponSlots[index].IconName = "Icons/BasicTurretIcon2";
                            ChooseWeaponSlots[index].LoadContent(SecondaryContent);
                            break;

                        case "CannonTurret":
                            ChooseWeaponSlots[index].IconName = "Icons/CannonTurretIcon";
                            ChooseWeaponSlots[index].LoadContent(SecondaryContent);
                            break;

                        case "IceTrap":
                            ChooseWeaponSlots[index].IconName = "Icons/SnowFlakeIcon";
                            ChooseWeaponSlots[index].LoadContent(SecondaryContent);
                            break;

                        case "FireTrap":
                            ChooseWeaponSlots[index].IconName = "Icons/FireIcon";
                            ChooseWeaponSlots[index].LoadContent(SecondaryContent);
                            break;
                    }

                    if (button.JustClicked == true)
                    {
                        switch (SelectedTurret)
                        {
                            case TurretType.MachineGun:
                                ChooseWeaponSlots[index].IconName = "Icons/BasicTurretIcon2";
                                ChooseWeaponSlots[index].LoadContent(SecondaryContent);
                                ProfileWeaponList[index] = "MachineGunTurret";
                                break;

                            case TurretType.Cannon:
                                ChooseWeaponSlots[index].IconName = "Icons/CannonTurretIcon";
                                ChooseWeaponSlots[index].LoadContent(SecondaryContent);
                                ProfileWeaponList[index] = "CannonTurret";
                                break;
                        }

                        switch (SelectedTrap)
                        {
                            case TrapType.Fire:
                                ChooseWeaponSlots[index].IconName = "Icons/FireIcon";
                                ChooseWeaponSlots[index].LoadContent(SecondaryContent);
                                ProfileWeaponList[index] = "FireTrap";
                                break;
                        }

                        SelectedTrap = null;
                        SelectedTurret = null;
                    }

                    if (button.JustRightClicked == true)
                    {
                        ProfileWeaponList[index] = "";
                        ChooseWeaponSlots[index].IconName = "Blank";
                        ChooseWeaponSlots[index].LoadContent(SecondaryContent);
                    }
                }

                foreach (List<Button> list in ChooseWeaponList)
                {
                    switch (ChooseWeaponList.IndexOf(list))
                    {
                        case 0:
                            foreach (Button button in list)
                            {
                                if (button.JustClicked == true)
                                    switch (list.IndexOf(button))
                                    {
                                        case 0:
                                            if (CurrentProfile.MachineGun == true)
                                            {
                                                SelectedTrap = null;
                                                SelectedTurret = TurretType.MachineGun;
                                            }
                                            break;

                                        case 1:
                                            if (CurrentProfile.Cannon == true)
                                            {
                                                SelectedTrap = null;
                                                SelectedTurret = TurretType.Cannon;
                                            }
                                            break;

                                        case 7:
                                            if (CurrentProfile.Fire == true)
                                            {
                                                SelectedTrap = TrapType.Fire;
                                                SelectedTurret = null;
                                            }
                                            break;
                                    }
                            }
                            break;

                        case 1:
                            foreach (Button button in list)
                            {
                                if (button.JustClicked == true)
                                    switch (list.IndexOf(button))
                                    {
                                        case 0: if (CurrentProfile.FlameThrower == true)
                                            {
                                                SelectedTrap = null;
                                                SelectedTurret = TurretType.FlameThrower;
                                            }
                                            break;

                                        case 1:
                                            if (CurrentProfile.Ice == true)
                                            {
                                                SelectedTurret = null;
                                                SelectedTrap = TrapType.Ice;
                                            }
                                            break;
                                    }
                            }
                            break;
                    }
                }

                foreach (List<Button> list in ChooseWeaponList)
                {
                    foreach (Button button in list)
                    {
                        if (button.JustClicked == true)
                            MenuClick.Play();

                        button.Update();                        

                        switch (button.IconName)
                        {
                            default:
                                text = "Unlock this weapon by completing new levels.";
                                break;
                        }

                        if (button.CurrentButtonState == ButtonSpriteState.Hover && WeaponInformation == null)
                        {                            
                            WeaponInformation = new InformationBox(text);
                            WeaponInformation.LoadContent(SecondaryContent);
                        }
                    }
                }

                if (ChooseWeaponList.All(TestList => TestList.All(button => button.CurrentButtonState == ButtonSpriteState.Released)))
                {
                    WeaponInformation = null;
                }

                foreach (List<Button> list in ChooseWeaponList)
                {
                    foreach (Button button in list)
                    {
                        button.IconName = "Icons/LockIcon";
                        button.LoadContent(SecondaryContent);
                    }
                }

                #region The list of weapons to choose from
                foreach (List<Button> list in ChooseWeaponList)
                {
                    switch (ChooseWeaponList.IndexOf(list))
                    {
                        case 0:
                            foreach (Button button in list)
                            {
                                switch (list.IndexOf(button))
                                {
                                    case 0:
                                        if (CurrentProfile.MachineGun == true)
                                        {
                                            if (button.IconName == "Icons/LockIcon")
                                            {
                                                button.IconName = "Icons/BasicTurretIcon2";
                                                button.LoadContent(SecondaryContent);
                                            }
                                        }
                                        break;

                                    case 1:
                                        if (CurrentProfile.Cannon == true)
                                        {
                                            if (button.IconName == "Icons/LockIcon")
                                            {
                                                button.IconName = "Icons/CannonTurretIcon";
                                                button.LoadContent(SecondaryContent);
                                            }
                                        }
                                        break;

                                    case 2:
                                        if (CurrentProfile.SawBlade == true)
                                        {
                                            if (button.IconName == "Icons/LockIcon")
                                            {
                                                button.IconName = "Icons/SawbladeIcon";
                                                button.LoadContent(SecondaryContent);
                                            }
                                        }
                                        break;

                                    case 3:
                                        if (CurrentProfile.Spikes == true)
                                        {
                                            if (button.IconName == "Icons/LockIcon")
                                            {
                                                button.IconName = "Icons/SpikesIcon";
                                                button.LoadContent(SecondaryContent);
                                            }
                                        }
                                        break;

                                    case 4:
                                        if (CurrentProfile.Wall == true)
                                        {
                                            if (button.IconName == "Icons/LockIcon")
                                            {
                                                button.IconName = "Icons/WallIcon";
                                                button.LoadContent(SecondaryContent);
                                            }
                                        }
                                        break;

                                    case 5:
                                        if (CurrentProfile.LightningTurret == true)
                                        {
                                            if (button.IconName == "Icons/LockIcon")
                                            {
                                                button.IconName = "blank";
                                                button.LoadContent(SecondaryContent);
                                            }
                                        }
                                        break;

                                    case 6:
                                        if (CurrentProfile.Catapult == true)
                                        {
                                            if (button.IconName == "Icons/LockIcon")
                                            {
                                                button.IconName = "blank";
                                                button.LoadContent(SecondaryContent);
                                            }
                                        }
                                        break;

                                    case 7:
                                        if (CurrentProfile.Fire == true)
                                        {
                                            if (button.IconName == "Icons/LockIcon")
                                            {
                                                button.IconName = "Icons/FireIcon";
                                                button.LoadContent(SecondaryContent);
                                            }
                                        }
                                        break;
                                }

                            }
                            break;

                        case 1:
                            foreach (Button button in list)
                            {
                                switch (list.IndexOf(button))
                                {
                                    case 0:
                                        if (CurrentProfile.FlameThrower == true)
                                        {
                                            if (button.IconName == "Icons/LockIcon")
                                            {
                                                button.IconName = "blank";
                                                button.LoadContent(SecondaryContent);
                                            }
                                        }
                                        break;

                                    case 1:
                                        if (CurrentProfile.Ice == true)
                                        {
                                            if (button.IconName == "Icons/LockIcon")
                                            {
                                                button.IconName = "Icons/SnowFlakeIcon";
                                                button.LoadContent(SecondaryContent);
                                            }
                                        }
                                        break;

                                    case 2:

                                        break;

                                    case 3:

                                        break;

                                    case 4:

                                        break;

                                    case 5:

                                        break;

                                    case 6:

                                        break;

                                    case 7:

                                        break;
                                }
                            }
                            break;
                    }
                }
                #endregion

                foreach (Button button in UpgradesButtonList)
                {
                    int Index;
                    Index = UpgradesButtonList.IndexOf(button);

                    button.Update();

                    if (UpgradeInformation != null)
                    UpgradeInformation.Update();

                    if (button.JustClicked == true)
                    {
                        MenuClick.Play();

                        

                        switch (Index)
                        {
                            case 0:
                                if (CurrentProfile.Points >= 10)
                                {
                                    CurrentProfile.Points -= 10;
                                    CurrentProfile.UpgradesList.Add(new Upgrade1());
                                    StorageDevice.BeginShowSelector(this.SaveProfile, null);
                                }
                                break;

                            case 1:
                                if (CurrentProfile.Points >= 10)
                                {
                                    CurrentProfile.UpgradesList.Add(new Upgrade2());
                                    StorageDevice.BeginShowSelector(this.SaveProfile, null);
                                }
                                break;
                        }
                    }

                    if (button.CurrentButtonState == ButtonSpriteState.Hover && UpgradeInformation == null)
                    {
                        switch (Index)
                        {
                            case 0:
                                UpgradeInformation = new InformationBox(new Upgrade1().Text);
                                break;

                            case 1:
                                UpgradeInformation = new InformationBox(new Upgrade2().Text);
                                break;

                            default:
                                UpgradeInformation = null;
                                return;
                        }                        
                        
                        UpgradeInformation.LoadContent(Content);
                    }
                }

                if (UpgradesButtonList.All(button => button.CurrentButtonState == ButtonSpriteState.Released))
                {
                    UpgradeInformation = null;
                }

                if (ProfileManagementBack.JustClicked == true)
                {
                    MenuClick.Play();
                    SetProfileNames();
                    SelectedTrap = null;
                    SelectedTurret = null;
                    CurrentProfile.Buttons = ProfileWeaponList;
                    StorageDevice.BeginShowSelector(this.SaveProfile, null);
                    GameState = GameState.ProfileSelect;
                }

                if (ProfileManagementPlay.JustClicked == true)
                {
                    MenuClick.Play();
                    if (CurrentProfile != null)
                    {
                        SelectedTrap = null;
                        SelectedTurret = null;
                        LevelNumber = CurrentProfile.LevelNumber;
                        LoadLevel(LevelNumber);
                        LoadUpgrades();
                        CurrentProfile.Buttons = ProfileWeaponList;
                        StorageDevice.BeginShowSelector(this.SaveProfile, null);
                        GameState = GameState.Loading;
                        LoadingThread = new Thread(LoadGameContent);
                        LoadingThread.Start();
                        IsLoading = false;
                    }
                }
            }
            #endregion

            #region Handling Options Button Presses
            if (GameState == GameState.Options)
            {
                OptionsBack.Update();

                OptionsSFXUp.Update();
                OptionsSFXDown.Update();

                OptionsMusicUp.Update();
                OptionsMusicDown.Update();

                if (OptionsBack.JustClicked == true)
                {
                    MenuClick.Play();
                    SaveSettings();
                    GameState = GameState.Menu;
                }

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
            }
            #endregion

            #region Handling GetName Button Presses
            if (GameState == GameState.GettingName)
            {
                GetNameBack.Update();
                GetNameOK.Update();
                NameInput.Update();
                NameInput.Active = true;

                if (GetNameBack.JustClicked == true)
                {
                    MenuClick.Play();
                    NameInput.TypePosition = 0;
                    NameInput.RealString = "";
                    GameState = GameState.ProfileSelect;
                }

                if ((GetNameOK.JustClicked == true) ||
                    (CurrentKeyboardState.IsKeyUp(Keys.Enter) && PreviousKeyboardState.IsKeyDown(Keys.Enter)) &&
                    (NameInput.RealString.Length >= 3))
                {
                    MenuClick.Play();
                    AddNewProfile();
                }
            }
            #endregion

            #region Handling DialogBox Button Presses
            if (ExitDialog != null)
            {
                ExitDialog.Update();

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
                DeleteProfileDialog.Update();

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
                ProfileMenuDialog.Update();

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
                MainMenuDialog.Update();

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
            #endregion
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
                invader.Update(gameTime);
                invader.Move();

                #region This controls what happens when the invaders die, depending on their type
                if (invader.CurrentHP <= 0)
                {
                    for (int i = 0; i < invader.ResourceValue; i++)
                    {
                        CoinList.Add(new Particle(Coin, new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                            (float)RandomDouble(90, 270), (float)RandomDouble(6, 8), 200f, 1f, true, (float)RandomDouble(0, 360), (float)RandomDouble(-5, 5), 0.75f,
                            Color.White, Color.White, 0.2f, true, MathHelper.Clamp(invader.MaxY + (float)RandomDouble(-32, 32), 525, 630), false, null, true));
                    }

                    switch (invader.InvaderType)
                    {
                        case InvaderType.Soldier:
                            EmitterList2.Add(new Emitter("Particles/Splodge", new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                          new Vector2(0, 360), new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), new Vector2(1, 3),
                          new Vector2(0.02f, 0.06f), Color.DarkRed, Color.Red, 0.1f, 0.2f, 20, 10, true, new Vector2(invader.MaxY, invader.MaxY)));
                            EmitterList2[EmitterList2.Count - 1].LoadContent(Content);
                            break;

                        case InvaderType.SuicideBomber:
                            EmitterList2.Add(new Emitter("Particles/Splodge", new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                            new Vector2(0, 360), new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), new Vector2(1, 3),
                            new Vector2(0.02f, 0.06f), Color.DarkRed, Color.Red, 0.1f, 0.2f, 20, 10, true, new Vector2(invader.MaxY, invader.MaxY)));
                            EmitterList2[EmitterList2.Count - 1].LoadContent(Content);

                            EnemyExplosionList.Add(new Explosion(new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y), 100, invader.AttackPower));
                            break;

                        case InvaderType.Spider:
                            EmitterList2.Add(new Emitter("Particles/Splodge", new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                            new Vector2(0, 360), new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), new Vector2(1, 3),
                            new Vector2(0.02f, 0.06f), Color.Green, Color.Lime, 0.1f, 0.2f, 20, 10, true, new Vector2(invader.MaxY, invader.MaxY)));
                            EmitterList2[EmitterList2.Count - 1].LoadContent(Content);
                            break;

                        case InvaderType.Tank:
                            EnemyExplosionList.Add(new Explosion(new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y), 200, invader.AttackPower));

                            Emitter ExplosionEmitter2 = new Emitter("Particles/Splodge", new Vector2(invader.DestinationRectangle.Center.X, invader.MaxY),
                            new Vector2(0, 180), new Vector2(1, 4), new Vector2(20, 50), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                            new Vector2(0.02f, 0.06f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 0.2f, 20, 10, true, new Vector2(invader.MaxY + 8, invader.MaxY + 8));

                            EmitterList.Add(ExplosionEmitter2);
                            EmitterList[EmitterList.IndexOf(ExplosionEmitter2)].LoadContent(Content);

                            Emitter ExplosionEmitter = new Emitter("Particles/FireParticle",
                                    new Vector2(invader.DestinationRectangle.Center.X, invader.MaxY),
                                    new Vector2(0, 180), new Vector2(3, 8), new Vector2(1, 15), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 20, false,
                                    new Vector2(0, 720), true);

                            EmitterList.Add(ExplosionEmitter);
                            EmitterList[EmitterList.IndexOf(ExplosionEmitter)].LoadContent(Content);

                            Color SmokeColor1 = Color.Lerp(Color.DarkGray, Color.Transparent, 0.5f);
                            Color SmokeColor2 = Color.Lerp(Color.Gray, Color.Transparent, 0.5f);

                            Emitter newEmitter2 = new Emitter("Particles/Smoke",
                                    new Vector2(invader.DestinationRectangle.Center.X, invader.MaxY),
                                    new Vector2(80, 100), new Vector2(0.5f, 1f), new Vector2(20, 40), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(0, 0), new Vector2(1, 2), SmokeColor1, SmokeColor2, 0.0f, 0.3f, 1, 1, false,
                                    new Vector2(0, 720), false);

                            EmitterList2.Add(newEmitter2);
                            EmitterList2[EmitterList2.IndexOf(newEmitter2)].LoadContent(Content);
                            break;

                        case InvaderType.Airship:
                            EmitterList2.Add(new Emitter("Particles/Splodge", new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                            new Vector2(0, 360), new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), new Vector2(1, 3),
                            new Vector2(0.02f, 0.06f), Color.DarkRed, Color.Red, 0.1f, 0.2f, 20, 10, true, new Vector2(525, 630)));
                            EmitterList2[EmitterList2.Count - 1].LoadContent(Content);
                            break;

                        case InvaderType.Slime:
                            EmitterList2.Add(new Emitter("Particles/Splodge", new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                            new Vector2(0, 360), new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), new Vector2(1, 3),
                            new Vector2(0.02f, 0.06f), Color.HotPink, Color.LightPink, 0.1f, 0.2f, 20, 10, true, new Vector2(invader.MaxY, invader.MaxY)));
                            EmitterList2[EmitterList2.Count - 1].LoadContent(Content);
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
                        invader.Trajectory(Vector2.Zero);
                        invader.Position = new Vector2(invader.Position.X, invader.MaxY - invader.DestinationRectangle.Height);
                        invader.Gravity = 0;
                    }

                    if (invader.DestinationRectangle.Bottom < invader.MaxY)
                    {
                        invader.Gravity = 0.2f;
                    }
                }
                #endregion
            }

            #region This controls what happens when an explosion happens near the invader
            if (ExplosionList.Count > 0)
            foreach (Explosion explosion in ExplosionList)
            {
                foreach (Invader invader in InvaderList)
                {                       
                    //Vector distance between invader and explosion
                    float Dist = MathHelper.Distance(invader.DestinationRectangle.Center.X, explosion.Position.X);

                    //List of X values between the invader and the explosion      
                    
                    //[----------dist-----------]
                    //inv--------wall--------expl
                    var InvaderToExplosion = Enumerable.Range((int)invader.DestinationRectangle.Center.X, (int)Dist);

                    //[----------dist-----------]
                    //expl-------wall---------inv
                    var ExplosionToInvader = Enumerable.Range((int)explosion.Position.X, (int)Dist);

                    List<Trap> TempList = new List<Trap>();
                    TempList = TrapList.FindAll(Trap => Trap.TrapType == TrapType.Wall);

                    if (!TempList.Any(Trap => ExplosionToInvader.Contains(Trap.DestinationRectangle.Center.X)) &&
                        !TempList.Any(Trap => InvaderToExplosion.Contains(Trap.DestinationRectangle.Center.X)) &&
                        Dist < explosion.BlastRadius)
                    {
                        invader.CurrentHP -= explosion.Damage / 100 * (100 - (100 / explosion.BlastRadius) * Dist);
                    }                   
                }

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
            #endregion
        }


        private void AttackTower()
        {
            //Melee attack
            foreach (Invader invader in InvaderList)
            {
                if (!invader.DestinationRectangle.Intersects(Tower.DestinationRectangle))
                {
                    invader.CurrentMoveVector = invader.MoveVector;
                }

                if (invader.DestinationRectangle.Intersects(Tower.DestinationRectangle))
                {
                    invader.CurrentMoveVector = Vector2.Zero;

                    if (invader.CanAttack == true)
                    {
                        switch (invader.InvaderType)
                        {
                            case InvaderType.SuicideBomber:
                                Emitter ExplosionEmitter = new Emitter("Particles/FireParticle",
                                        new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                                        new Vector2(0, 360), new Vector2(1, 5), new Vector2(1, 10), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 7, false,
                                        new Vector2(0, 720), true);

                                EmitterList.Add(ExplosionEmitter);
                                EmitterList[EmitterList.IndexOf(ExplosionEmitter)].LoadContent(Content);

                                Emitter newEmitter2 = new Emitter("Particles/Smoke",
                                        new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                                        new Vector2(0, 360), new Vector2(1, 2), new Vector2(1, 15), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(1, 2), Color.Gray, Color.DarkGray, 0.0f, 0.3f, 1, 2, false,
                                        new Vector2(0, 720), false);

                                EmitterList2.Add(newEmitter2);
                                EmitterList2[EmitterList2.IndexOf(newEmitter2)].LoadContent(Content);

                                Emitter ExplosionEmitter2 = new Emitter("Particles/Splodge",
                                        new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                                        new Vector2(0, 360), new Vector2(1, 7), new Vector2(15, 40), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(0.1f, 0.3f), Color.DarkSlateGray, Color.SandyBrown, 0.0f, 0.1f, 1, 3, false,
                                        new Vector2(0, 720), true);

                                EmitterList.Add(ExplosionEmitter2);
                                EmitterList[EmitterList.IndexOf(ExplosionEmitter2)].LoadContent(Content);

                                invader.CurrentHP = 0;
                                break;
                        }
                    }
                }
            }

            //Ranged attack
            foreach (Invader invader in InvaderList)
            {
                RangedInvader rangedInvader = invader as RangedInvader;

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
                                0.2f);

                                heavyProjectile.YRange = new Vector2(invader.Bottom, invader.Bottom);
                                heavyProjectile.LoadContent(Content);
                                InvaderHeavyProjectileList.Add(heavyProjectile);
                            }
                            break;

                        case InvaderType.Archer:
                            {
                                HeavyProjectile heavyProjectile = new Arrow(
                                new Vector2(rangedInvader.DestinationRectangle.Center.X, rangedInvader.DestinationRectangle.Center.Y),
                                Random.Next((int)(rangedInvader.PowerRange.X), (int)(rangedInvader.PowerRange.Y)),
                                -MathHelper.ToRadians(Random.Next((int)(rangedInvader.AngleRange.X), (int)(rangedInvader.AngleRange.Y))),
                                0.2f);

                                heavyProjectile.YRange = new Vector2(invader.Bottom, invader.Bottom);
                                heavyProjectile.LoadContent(Content);
                                InvaderHeavyProjectileList.Add(heavyProjectile);
                            }
                            break;

                        case InvaderType.Airship:
                            {
                                HeavyProjectile heavyProjectile = new CannonBall(
                                new Vector2(rangedInvader.DestinationRectangle.Center.X, rangedInvader.DestinationRectangle.Center.Y),
                                Random.Next((int)(rangedInvader.PowerRange.X), (int)(rangedInvader.PowerRange.Y)),
                                -MathHelper.ToRadians(Random.Next((int)(rangedInvader.AngleRange.X), (int)(rangedInvader.AngleRange.Y))),
                                0.2f, rangedInvader.RangedAttackPower, 200);

                                heavyProjectile.YRange = new Vector2(519, 630);
                                heavyProjectile.LoadContent(Content);
                                InvaderHeavyProjectileList.Add(heavyProjectile);
                            }
                            break;
                    }
                }
            }
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
                                    invader.CurrentMoveVector = Vector2.Zero;
                                }
                                break;
                        }
                    }
                }
            }
        }


        private void RangedAttackTower()
        {

        }

        private void RangedAttackTraps()
        {
            //Handle the Enemy Projectiles when they hit a trap, based on the traptype and the projectiletype
            foreach (HeavyProjectile heavyProjectile in InvaderHeavyProjectileList)
            {
                if (TrapList.Any(Trap => Trap.DestinationRectangle.Intersects(heavyProjectile.CollisionRectangle))
                    && heavyProjectile.Active == true)
                {
                    int index = TrapList.IndexOf(TrapList.First(Trap => Trap.DestinationRectangle.Intersects(heavyProjectile.CollisionRectangle)));

                    switch (TrapList[index].TrapType)
                    {
                        #region Wall
                        case TrapType.Wall:
                            {                                
                                switch (heavyProjectile.HeavyProjectileType)
                                {
                                    case HeavyProjectileType.Acid:
                                        {
                                            Emitter newEmitter = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                                                new Vector2(-(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) - 45,
                                                    -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) + 45),
                                                    new Vector2(2, 3), new Vector2(100, 200), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                                new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true,
                                                new Vector2(TrapList[index].DestinationRectangle.Bottom, TrapList[index].DestinationRectangle.Bottom));
                                            EmitterList2.Add(newEmitter);
                                            EmitterList2[EmitterList2.IndexOf(newEmitter)].LoadContent(Content);
                                            heavyProjectile.Active = false;
                                            TrapList[index].CurrentHP -= heavyProjectile.Damage;
                                            heavyProjectile.Emitter.AddMore = false;
                                        }
                                        break;

                                    case HeavyProjectileType.Arrow:

                                        break;

                                    case HeavyProjectileType.CannonBall:

                                        break;

                                    case HeavyProjectileType.FlameThrower:

                                        break;
                                }
                                
                            }
                            break;
                            #endregion

                        case TrapType.Barrel:
                            {

                            }
                            break;

                        case TrapType.Catapult:
                            {

                            }
                            break;

                        case TrapType.Fire:
                            {

                            }
                            break;

                        case TrapType.Ice:
                            {

                            }
                            break;

                        case TrapType.Spikes:
                            {

                            }
                            break;

                        case TrapType.Tar:
                            {

                            }
                            break;
                    }
                }
            }
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
                                Emitter newEmitter = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                new Vector2(0, 180), new Vector2(2, 3), new Vector2(10, 25), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                new Vector2(0.02f, 0.06f), Color.Lime, Color.LimeGreen, 0.2f, 0.2f, 20, 10, true, new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 8));
                                EmitterList2.Add(newEmitter);
                                EmitterList2[EmitterList2.IndexOf(newEmitter)].LoadContent(Content);
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

                                Emitter newEmitter = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                    new Vector2(0, 180), new Vector2(2, 3), new Vector2(10, 25), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                    new Vector2(0.02f, 0.06f), DirtColor, DirtColor2, 0.2f, 0.2f, 20, 10, true, new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 8));
                                EmitterList2.Add(newEmitter);
                                EmitterList2[EmitterList2.IndexOf(newEmitter)].LoadContent(Content);
                            }
                            break;
                    }

                    #region Deactivate the enemy projectile
                    heavyProjectile.Emitter.AddMore = false;
                    heavyProjectile.Active = false;
                    heavyProjectile.Velocity = Vector2.Zero;
                    #endregion
                }
                #endregion
                
                if (TurretList.Any(turret => turret != null && turret.SelectBox.Intersects(heavyProjectile.DestinationRectangle)))
                {
                    Turret hitTurret = TurretList[TurretList.FindIndex(turret => turret.SelectBox.Intersects(heavyProjectile.DestinationRectangle))];

                    hitTurret.Active = false;
                    
                    TowerButtonList[TurretList.IndexOf(hitTurret)].ButtonActive = true;
                    TurretList[TurretList.IndexOf(hitTurret)] = null;
                }

                #region What happens when an enemy projectiles hits the tower
                if (heavyProjectile.DestinationRectangle.Intersects(Tower.DestinationRectangle) 
                    && heavyProjectile.Active == true)
                {
                    switch (heavyProjectile.HeavyProjectileType)
                    {
                        case HeavyProjectileType.Acid:
                            {
                                Tower.TakeDamage(heavyProjectile.Damage);

                                Emitter newEmitter = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                                new Vector2(-(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) - 45,
                                -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) + 45),
                                new Vector2(2, 3), new Vector2(100, 200), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true,
                                new Vector2(Tower.DestinationRectangle.Bottom, Tower.DestinationRectangle.Bottom));

                                EmitterList2.Add(newEmitter);
                                EmitterList2[EmitterList2.IndexOf(newEmitter)].LoadContent(Content);
                            }
                            break;

                        case HeavyProjectileType.Arrow:
                            {

                            }
                            break;

                        case HeavyProjectileType.CannonBall:
                            {
                                Tower.TakeDamage(heavyProjectile.Damage);

                                Emitter newEmitter = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                                new Vector2(-(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) - 45,
                                -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) + 45),
                                new Vector2(2, 3), new Vector2(100, 200), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true,
                                new Vector2(Tower.DestinationRectangle.Bottom, Tower.DestinationRectangle.Bottom));

                                EmitterList2.Add(newEmitter);
                                EmitterList2[EmitterList2.IndexOf(newEmitter)].LoadContent(Content);
                            }
                            break;

                        case HeavyProjectileType.FlameThrower:
                            {

                            }
                            break;
                    }

                    #region Deactivate the enemy projectile
                    heavyProjectile.Active = false;
                    heavyProjectile.Emitter.AddMore = false;
                    heavyProjectile.Velocity = Vector2.Zero;
                    #endregion
                }
                #endregion
            }
        }

        private void InvaderLightProjectileUpdate(GameTime gameTime)
        {
            foreach (LightProjectile lightProjectile in InvaderLightProjectileList)
            {
                //Check if the light projectile hits a trap or the tower or a turret here.
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
            //Makes sure two turrets cannot be selected at the same time
            foreach (Turret turret in TurretList)
            {
                if (turret != null && turret.Active == true)
                {
                    if (turret.Selected == true && 
                        CurrentMouseState.LeftButton == ButtonState.Pressed && 
                        PreviousMouseState.LeftButton == ButtonState.Pressed && 
                        CurrentMouseState.Y < (720 - HUDBarTexture.Height))
                    {
                        if (turret.CanShoot == true)
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
                        SelectedTurretIndex = TurretList.IndexOf(turret);

                        foreach (Turret turret2 in TurretList)
                        {
                            if (turret2 != null && turret2 != turret)
                            {
                                turret2.Selected = false;
                            }
                        }
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
                    CursorPosition.Y < (720 - HUDBarTexture.Height) &&
                    turret.Selected == true)
                {
                    Vector2 MousePosition, Direction, BarrelEnd;
                    HeavyProjectile HeavyProjectile;

                    CurrentMouseState = Mouse.GetState();
                    MousePosition = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);

                    Direction = turret.FireDirection;
                    Direction.Normalize();

                    switch (turret.TurretType)
                    {
                        #region Machine Gun
                        case TurretType.MachineGun:
                            {
                                CurrentProfile.ShotsFired++;

                                BarrelEnd = new Vector2((float)Math.Cos(turret.Rotation) * (turret.BarrelRectangle.Width - 25),
                                                        (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height + 25));

                                CurrentProjectile = new MachineGunProjectile(new Vector2(turret.BarrelRectangle.X,
                                                                                         turret.BarrelRectangle.Y), Direction);
                                LightProjectileList.Add(CurrentProjectile);

                                Emitter FlashEmitter = new Emitter("Particles/FireParticle",
                                        new Vector2(turret.BarrelRectangle.X + BarrelEnd.X, turret.BarrelRectangle.Y + BarrelEnd.Y),
                                new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(turret.Direction.Y, turret.Direction.X)),
                                            MathHelper.ToDegrees(-(float)Math.Atan2(turret.Direction.Y, turret.Direction.X))),
                                new Vector2(20, 30), new Vector2(1, 4), 1f, true, new Vector2(0, 360),
                                new Vector2(-10, 10), new Vector2(2, 3), FireColor, FireColor2, 0.0f, 0.05f, 1, 10,
                                false, new Vector2(0, 720), true, 1);
                                EmitterList.Add(FlashEmitter);
                                EmitterList[EmitterList.IndexOf(FlashEmitter)].LoadContent(Content);

                                ShellCasingList.Add(new Particle(ShellCasing,
                                    new Vector2(turret.BarrelRectangle.X, turret.BarrelRectangle.Y),
                                    turret.Rotation - MathHelper.ToRadians((float)RandomDouble(175, 185)),
                                    (float)RandomDouble(2, 4), 500, 1f, true, (float)RandomDouble(-10, 10),
                                    (float)RandomDouble(-3, 6), 1, Color.White, Color.White, 0.2f, true,
                                    (float)RandomDouble(608, 640), false, 1, true));
                            }
                            break;
                        #endregion

                        #region Cannon turret
                        case TurretType.Cannon:
                            {
                                BarrelEnd = new Vector2((float)Math.Cos(turret.Rotation) * (turret.BarrelRectangle.Width - 20),
                                                        (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height + 20));

                                Emitter FlashEmitter = new Emitter("Particles/FireParticle",
                                    new Vector2(turret.BarrelRectangle.X + BarrelEnd.X, turret.BarrelRectangle.Y + BarrelEnd.Y),
                                    new Vector2(
                                    MathHelper.ToDegrees(-(float)Math.Atan2(turret.Direction.Y, turret.Direction.X)),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(turret.Direction.Y, turret.Direction.X))),
                                    new Vector2(10, 20), new Vector2(1, 6), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(-2, 2), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.2f, 1, 5,
                                    false, new Vector2(0, 720), true, 1);

                                EmitterList.Add(FlashEmitter);
                                EmitterList[EmitterList.IndexOf(FlashEmitter)].LoadContent(Content);

                                HeavyProjectile = new CannonBall(new Vector2(turret.BarrelRectangle.X + BarrelEnd.X,
                                    turret.BarrelRectangle.Y + BarrelEnd.Y), 12, turret.Rotation, 0.2f, turret.Damage, 100,
                                    new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 520, 630), 630));

                                HeavyProjectile.LoadContent(Content);
                                HeavyProjectileList.Add(HeavyProjectile);

                                Vector2 BarrelStart = new Vector2((float)Math.Cos(turret.Rotation) * (45),
                                                                  (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height / 2));

                                ShellCasingList.Add(new Particle(BigShellCasing,
                                    new Vector2(turret.BarrelRectangle.X - BarrelStart.X, turret.BarrelRectangle.Y - BarrelStart.Y),
                                    turret.Rotation - MathHelper.ToRadians((float)RandomDouble(175, 185)),
                                    (float)RandomDouble(4, 6), 500, 1f, true, (float)RandomDouble(-10, 10),
                                    (float)RandomDouble(-6, 6), 1f, Color.White, Color.White, 0.2f, true, Random.Next(600, 630),
                                    false, null, true));
                            }
                            break;
                        #endregion

                        #region Flamethrower turret
                        case TurretType.FlameThrower:
                            {
                                BarrelEnd = new Vector2((float)Math.Cos(turret.Rotation) * (turret.BarrelRectangle.Width - 25),
                                                        (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height + 25));

                                HeavyProjectile = new FlameProjectile(new Vector2(turret.BarrelRectangle.X + BarrelEnd.X - 2,
                                    turret.BarrelRectangle.Y + BarrelEnd.Y - 2), (float)RandomDouble(7, 9), turret.Rotation, 0.3f,
                                    new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 520, 630), 630));

                                HeavyProjectile.Emitter.AngleRange = new Vector2(
                                    -(MathHelper.ToDegrees((float)Math.Atan2(-HeavyProjectile.Velocity.Y, -HeavyProjectile.Velocity.X))) - 20,
                                    -(MathHelper.ToDegrees((float)Math.Atan2(-HeavyProjectile.Velocity.Y, -HeavyProjectile.Velocity.X))) + 20);

                                HeavyProjectile.LoadContent(Content);
                                HeavyProjectileList.Add(HeavyProjectile);
                            }
                            break;
                        #endregion

                        #region Lightning turret
                        case TurretType.Lightning:
                            {
                                BarrelEnd = new Vector2((float)Math.Cos(turret.Rotation) * (turret.BarrelRectangle.Width - 20),
                                                        (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height + 20));

                                CurrentProjectile = new LightningProjectile(new Vector2(turret.BarrelRectangle.X,
                                                                                        turret.BarrelRectangle.Y), Direction);
                                LightProjectileList.Add(CurrentProjectile);


                                //for (int i = 0; i < 5; i++)
                                //{
                                //    LightningList.Add(new LightningBolt(new Vector2(turret.BarrelRectangle.X + BarrelEnd.X,
                                //                                        turret.BarrelRectangle.Y + BarrelEnd.Y), MousePosition,
                                //                                        Color.MediumPurple));
                                //    LightningList[i].LoadContent(Content);
                                //}

                                LightningSound.Play();
                            }
                            break;
                        #endregion

                        #region Cluster turret
                        case TurretType.Cluster:
                            {
                                TimerHeavyProjectile heavyProjectile;

                                BarrelEnd = new Vector2((float)Math.Cos(turret.Rotation) * (turret.BarrelRectangle.Width - 20),
                                                        (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height + 20));

                                Emitter FlashEmitter = new Emitter("Particles/FireParticle",
                                    new Vector2(turret.BarrelRectangle.X + BarrelEnd.X, turret.BarrelRectangle.Y + BarrelEnd.Y),
                                    new Vector2(
                                    MathHelper.ToDegrees(-(float)Math.Atan2(turret.Direction.Y, turret.Direction.X)),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(turret.Direction.Y, turret.Direction.X))),
                                    new Vector2(10, 20), new Vector2(1, 6), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(-2, 2), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.2f, 1, 5, false,
                                    new Vector2(0, 720), true);

                                EmitterList.Add(FlashEmitter);
                                EmitterList[EmitterList.IndexOf(FlashEmitter)].LoadContent(Content);

                                heavyProjectile = new ClusterBombShell(1000, new Vector2(turret.BarrelRectangle.X + BarrelEnd.X,
                                    turret.BarrelRectangle.Y + BarrelEnd.Y), 12, turret.Rotation, 0.2f,
                                    new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 520, 630), 630));

                                heavyProjectile.LoadContent(Content);
                                TimedProjectileList.Add(heavyProjectile);

                                ShellCasingList.Add(new Particle(BigShellCasing,
                                    new Vector2(turret.BarrelRectangle.X, turret.BarrelRectangle.Y),
                                    turret.Rotation - MathHelper.ToRadians((float)RandomDouble(175, 185)),
                                    (float)RandomDouble(3, 6), 500, 1f, true, (float)RandomDouble(-10, 10),
                                    (float)RandomDouble(-6, 6), 1f, Color.White, Color.White, 0.2f, true,
                                    Random.Next(600, 630), false, null, true));
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

                //This makes sure that the trails from the projectiles appear correctly oriented while falling
                if (heavyProjectile.Emitter != null)
                    heavyProjectile.Emitter.AngleRange = new Vector2(
                        -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) - 20,
                        -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) + 20);

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
                                Emitter newEmitter = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.DestinationRectangle.Center.X, heavyProjectile.DestinationRectangle.Center.Y),
                                                     new Vector2(
                                                         -(float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X) -180 - 45,
                                                         (float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X) - 180 + 45),
                                                     new Vector2(2, 3), new Vector2(100, 200), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                                     new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true,
                                                     new Vector2(TrapList[index].DestinationRectangle.Bottom, TrapList[index].DestinationRectangle.Bottom), false, null, true, true);

                                EmitterList2.Add(newEmitter);

                                EmitterList2[EmitterList2.IndexOf(newEmitter)].LoadContent(Content);
                                break;
                        }

                        heavyProjectile.Active = false;

                        heavyProjectile.Emitter.AddMore = false;
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
                                Emitter ExplosionEmitter2 = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                new Vector2(0, 180), new Vector2(1, 4), new Vector2(10, 30), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                new Vector2(0.02f, 0.06f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 0.2f, 20, 10, true, new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 8));

                                EmitterList.Add(ExplosionEmitter2);
                                EmitterList[EmitterList.IndexOf(ExplosionEmitter2)].LoadContent(Content);

                                Emitter ExplosionEmitter = new Emitter("Particles/FireParticle",
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(0, 180), new Vector2(1, 5), new Vector2(1, 20), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 7, false,
                                        new Vector2(0, 720), true);

                                EmitterList.Add(ExplosionEmitter);
                                EmitterList[EmitterList.IndexOf(ExplosionEmitter)].LoadContent(Content);

                                Color SmokeColor1 = Color.Lerp(Color.DarkGray, Color.Transparent, 0.5f);
                                Color SmokeColor2 = Color.Lerp(Color.Gray, Color.Transparent, 0.5f);

                                Emitter newEmitter2 = new Emitter("Particles/Smoke",
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(80, 100), new Vector2(0.5f, 1f), new Vector2(20, 40), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(1, 2), SmokeColor1, SmokeColor2, 0.0f, 0.3f, 1, 1, false,
                                        new Vector2(0, 720), false);

                                EmitterList2.Add(newEmitter2);
                                EmitterList2[EmitterList2.IndexOf(newEmitter2)].LoadContent(Content);

                                ExplosionList.Add(new Explosion(heavyProjectile.Position, heavyProjectile.BlastRadius, heavyProjectile.Damage));
                            }
                            break;
                        #endregion

                        #region ClusterBomb (Child of the ClusterBombShell) projectile
                        case HeavyProjectileType.ClusterBomb:
                            {
                                Emitter ExplosionEmitter2 = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                new Vector2(0, 180), new Vector2(1, 4), new Vector2(10, 30), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                new Vector2(0.02f, 0.06f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 0.2f, 20, 10, true, new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 8));

                                EmitterList.Add(ExplosionEmitter2);
                                EmitterList[EmitterList.IndexOf(ExplosionEmitter2)].LoadContent(Content);

                                Emitter ExplosionEmitter = new Emitter("Particles/FireParticle",
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(0, 180), new Vector2(1, 5), new Vector2(1, 20), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 7, false,
                                        new Vector2(0, 720), true);

                                EmitterList.Add(ExplosionEmitter);
                                EmitterList[EmitterList.IndexOf(ExplosionEmitter)].LoadContent(Content);

                                Color SmokeColor1 = Color.Lerp(Color.DarkGray, Color.Transparent, 0.5f);
                                Color SmokeColor2 = Color.Lerp(Color.Gray, Color.Transparent, 0.5f);

                                Emitter newEmitter2 = new Emitter("Particles/Smoke",
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(80, 100), new Vector2(0.5f, 1f), new Vector2(20, 40), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(1, 2), SmokeColor1, SmokeColor2, 0.0f, 0.3f, 1, 1, false,
                                        new Vector2(0, 720), false);

                                EmitterList2.Add(newEmitter2);
                                EmitterList2[EmitterList2.IndexOf(newEmitter2)].LoadContent(Content);

                                ExplosionList.Add(new Explosion(heavyProjectile.Position, 300, heavyProjectile.Damage));
                            }
                            break;
                        #endregion

                        #region Flame projectiles from FlameThrower
                        case HeavyProjectileType.FlameThrower:
                            {
                                Emitter newEmitter = new Emitter("Particles/Smoke", new Vector2(heavyProjectile.Position.X,
                                    heavyProjectile.DestinationRectangle.Bottom), new Vector2(20, 160), new Vector2(1f, 1.6f), new Vector2(10, 12), 0.25f, false,
                                    new Vector2(-20, 20), new Vector2(-4, 4), new Vector2(0.6f, 0.8f), FireColor, FireColor2, -0.2f, 0.5f, 3, 1, false, new Vector2(0, 720));
                                EmitterList.Add(newEmitter);
                                EmitterList[EmitterList.IndexOf(newEmitter)].LoadContent(Content);
                                heavyProjectile.Emitter.AddMore = false;
                                heavyProjectile.Active = false;
                            }
                            break;

                        case HeavyProjectileType.Arrow:
                            {
                                heavyProjectile.Emitter.AddMore = false;
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
                    }
                    #region Deactivate the Projectile
                    if (heavyProjectile.HeavyProjectileType != HeavyProjectileType.Arrow)
                    {
                        heavyProjectile.Emitter.AddMore = false;
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
                            heavyProjectile.Emitter.AddMore = false;
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

                                    Emitter newEmitter = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                                    new Vector2(-(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) - 45,
                                    -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) + 90),
                                    new Vector2(2, 3), new Vector2(100, 200), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                    new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true,
                                    new Vector2(InvaderList[Index].MaxY, InvaderList[Index].MaxY), false, 1f);

                                    EmitterList.Add(newEmitter);

                                    EmitterList[EmitterList.IndexOf(newEmitter)].LoadContent(Content);
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

                                        Emitter newEmitter = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                                        new Vector2(-(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) - 45,
                                            -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) + 90),
                                            new Vector2(2, 3), new Vector2(100, 200), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                        new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true,
                                        new Vector2(InvaderList[Index].MaxY, InvaderList[Index].MaxY));

                                        EmitterList2.Add(newEmitter);

                                        EmitterList2[EmitterList2.IndexOf(newEmitter)].LoadContent(Content);
                                        break;
                                }
                            }
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
                timedProjectile.Update(gameTime);

                #region This controls what happens when a TimedProjectile intersects the ground
                if (timedProjectile.Position.Y > timedProjectile.MaxY && timedProjectile.Active == true)
                {
                    switch (timedProjectile.HeavyProjectileType)
                    {
                        case HeavyProjectileType.ClusterBombShell:

                            Emitter ExplosionEmitter2 = new Emitter("Particles/Splodge", new Vector2(timedProjectile.Position.X, timedProjectile.MaxY),
                                new Vector2(0, 180), new Vector2(1, 4), new Vector2(10, 30), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                new Vector2(0.02f, 0.06f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 0.2f, 20, 10, true, new Vector2(timedProjectile.MaxY + 8, timedProjectile.MaxY + 8));

                            EmitterList.Add(ExplosionEmitter2);
                            EmitterList[EmitterList.IndexOf(ExplosionEmitter2)].LoadContent(Content);
                            break;
                    }

                    #region Deactivate the Projectile
                    timedProjectile.Emitter.AddMore = false;
                    timedProjectile.Active = false;
                    timedProjectile.Velocity = Vector2.Zero;
                    #endregion
                }
                #endregion

                #region What happens when a TimedProjectile runs out of time
                if (timedProjectile.Detonated == true)
                    switch (timedProjectile.HeavyProjectileType)
                    {
                        case HeavyProjectileType.ClusterBombShell:
                            if (timedProjectile.Position.Y >= 520)
                            {
                                Emitter ExplosionEmitter = new Emitter("Particles/FireParticle",
                                        new Vector2(timedProjectile.Position.X, timedProjectile.Position.Y),
                                        new Vector2(0, 180), new Vector2(1, 5), new Vector2(1, 20), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 7, false,
                                        new Vector2(0, 720), true);

                                EmitterList.Add(ExplosionEmitter);
                                EmitterList[EmitterList.IndexOf(ExplosionEmitter)].LoadContent(Content);

                                AddCluster(5, timedProjectile.Position, new Vector2(0, 360), HeavyProjectileType.ClusterBomb, timedProjectile.MaxY, 2f);
                            }
                            else
                            {
                                Emitter ExplosionEmitter = new Emitter("Particles/FireParticle",
                                        new Vector2(timedProjectile.Position.X, timedProjectile.Position.Y),
                                        new Vector2(0, 180), new Vector2(1, 5), new Vector2(1, 20), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 7, false,
                                        new Vector2(0, 720), true);

                                EmitterList.Add(ExplosionEmitter);
                                EmitterList[EmitterList.IndexOf(ExplosionEmitter)].LoadContent(Content);

                                AddCluster(5, timedProjectile.Position, new Vector2(0, 360), HeavyProjectileType.ClusterBomb, 520, 2f);
                            }

                            timedProjectile.Active = false;
                            break;
                    }
                #endregion
            }
        }

        private void AddCluster(int number, Vector2 position, Vector2 angleRange, HeavyProjectileType type, float minY, float speed)
        {
            HeavyProjectile heavyProjectile;
            float MaxY;

            for (int i = 0; i < number; i++)
            {
                MaxY = Random.Next((int)minY, 630);
                switch (type)
                {
                    case HeavyProjectileType.ClusterBomb:
                        heavyProjectile = new ClusterBomb(position, speed, (float)RandomDouble(angleRange.X, angleRange.Y), 0.3f, new Vector2(MaxY, MaxY));
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
            Ground.BoundingBox = new BoundingBox(new Vector3(0, MathHelper.Clamp(CursorPosition.Y, 525, 720), 0), new Vector3(1280, MathHelper.Clamp(CursorPosition.Y + 1, 525, 720), 0));

            foreach (Turret turret in TurretList)
            {
                if (turret != null && turret.Active == true && turret.Selected == true && CurrentProjectile != null)
                {
                    #region If a projectile hit an invader AND a trap
                    if (TrapList.Any(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) != null &&
                        InvaderList.Any(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) != null &&
                        CurrentProjectile.Active == true)))
                    {
                        float MinDistToTrap = (float)TrapList.Min(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray));
                        Trap HitTrap = TrapList.Find(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) == MinDistToTrap);
                        var DistToTrap = HitTrap.BoundingBox.Intersects(CurrentProjectile.Ray);

                        float MinDistToInv = (float)InvaderList.Min(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray));
                        Invader HitInvader = InvaderList.Find(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) == MinDistToInv);
                        var DistToInvader = HitInvader.BoundingBox.Intersects(CurrentProjectile.Ray);

                        Vector2 BarrelEnd, CollisionEnd;
                        BulletTrail Trail;

                        BarrelEnd = new Vector2(turret.BarrelRectangle.X + (float)Math.Cos(turret.Rotation) * (turret.BarrelRectangle.Width - 20),
                                                turret.BarrelRectangle.Y + (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height + 20));

                        #region if the invader is closer to the turret than the trap
                        if (DistToInvader < DistToTrap)
                        {
                            switch (HitInvader.InvaderType)
                            {
                                default:
                                    CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)DistToInvader),
                                                              turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToInvader));

                                    switch (CurrentProjectile.LightProjectileType)
                                    {
                                        case LightProjectileType.Lightning:
                                            for (int i = 0; i < 5; i++)
                                            {
                                                LightningList.Add(new LightningBolt(BarrelEnd, CollisionEnd, Color.MediumPurple));
                                                LightningList[i].LoadContent(Content);
                                            }
                                            break;

                                        case LightProjectileType.MachineGun:
                                            Trail = new BulletTrail(BarrelEnd, CollisionEnd);
                                            Trail.LoadContent(Content);
                                            TrailList.Add(Trail);
                                            break;
                                    }
                                    break;

                                case InvaderType.Soldier:
                                    CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)DistToInvader),
                                                              turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToInvader));

                                    switch (CurrentProjectile.LightProjectileType)
                                    {
                                        case LightProjectileType.Lightning:
                                            for (int i = 0; i < 5; i++)
                                            {
                                                LightningList.Add(new LightningBolt(BarrelEnd, CollisionEnd, Color.MediumPurple));
                                                LightningList[i].LoadContent(Content);
                                            }
                                            break;

                                        case LightProjectileType.MachineGun:
                                            Trail = new BulletTrail(BarrelEnd, CollisionEnd);
                                            Trail.LoadContent(Content);
                                            TrailList.Add(Trail);
                                            break;
                                    }

                                    EmitterList.Add(new Emitter("Particles/Splodge", new Vector2(HitInvader.DestinationRectangle.Center.X, CollisionEnd.Y),
                                    new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - (float)RandomDouble(0, 45),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) + (float)RandomDouble(0, 45)),
                                    new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360),
                                    new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.DarkRed, Color.Red,
                                    0.1f, 0.1f, 10, 5, true, new Vector2(HitInvader.MaxY, HitInvader.MaxY), false, 1, true, false));

                                    EmitterList[EmitterList.Count - 1].LoadContent(Content);
                                    break;
                            }

                            HitInvader.TurretDamage(-turret.Damage);
                            if (HitInvader.CurrentHP <= 0)
                                Resources += HitInvader.ResourceValue;
                        }
                        #endregion

                        #region If the trap is closer to the turret than the invader
                        if (DistToTrap < DistToInvader)
                        {
                            switch (HitTrap.TrapType)
                            {
                                default:
                                    CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * 1280),
                                                               turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * 1280));

                                    switch (CurrentProjectile.LightProjectileType)
                                    {
                                        case LightProjectileType.Lightning:
                                            for (int i = 0; i < 5; i++)
                                            {
                                                LightningList.Add(new LightningBolt(BarrelEnd, CollisionEnd, Color.MediumPurple));
                                                LightningList[i].LoadContent(Content);
                                            }
                                            break;

                                        case LightProjectileType.MachineGun:
                                            Trail = new BulletTrail(BarrelEnd, CollisionEnd);
                                            Trail.LoadContent(Content);
                                            TrailList.Add(Trail);
                                            break;
                                    }
                                   
                                    HitInvader.TurretDamage(-turret.Damage);
                                    if (HitInvader.CurrentHP <= 0)
                                        Resources += HitInvader.ResourceValue;                                    
                                    break;

                                case TrapType.Wall:
                                    CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)DistToTrap),
                                                               turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToTrap));

                                    if (CurrentProjectile.LightProjectileType == LightProjectileType.MachineGun)
                                    {
                                        Trail = new BulletTrail(BarrelEnd, CollisionEnd);
                                        Trail.LoadContent(Content);
                                        TrailList.Add(Trail);
                                    }

                                    if (CurrentProjectile.LightProjectileType == LightProjectileType.Lightning)
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            LightningList.Add(new LightningBolt(BarrelEnd, CollisionEnd,
                                                                                Color.MediumPurple));
                                            LightningList[i].LoadContent(Content);
                                        }
                                    }


                                    EmitterList.Add(new Emitter("Particles/Splodge", new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                    new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                    new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360),
                                    new Vector2(1, 3), new Vector2(0.02f, 0.05f), Color.Gray, Color.DarkGray,
                                    0.1f, 0.1f, 10, 2, true, new Vector2(HitTrap.DestinationRectangle.Bottom, HitTrap.DestinationRectangle.Bottom), false, 1, true, false));
                                    EmitterList[EmitterList.Count - 1].LoadContent(Content);
                                    break;

                                case TrapType.Barrel:
                                     CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)DistToTrap),
                                                               turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToTrap));

                                    if (CurrentProjectile.LightProjectileType == LightProjectileType.MachineGun)
                                    {
                                        Trail = new BulletTrail(BarrelEnd, CollisionEnd);
                                        Trail.LoadContent(Content);
                                        TrailList.Add(Trail);
                                    }

                                    if (CurrentProjectile.LightProjectileType == LightProjectileType.Lightning)
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            LightningList.Add(new LightningBolt(BarrelEnd, CollisionEnd,
                                                                                Color.MediumPurple));
                                            LightningList[i].LoadContent(Content);
                                        }
                                    }

                                    if (HitTrap.CanTrigger == false)
                                    {
                                        HitTrap.CurrentHP -= turret.Damage;
                                    }
                                    break;
                            }
                        }
                        #endregion
                    }
                    #endregion

                    #region If a projectile hit just a trap
                    if (TrapList.Any(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) != null &&
                        InvaderList.All(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) == null &&
                        CurrentProjectile.Active == true)))
                    {
                        float MinDist = (float)TrapList.Min(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray));
                        Trap HitTrap = TrapList.Find(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) == MinDist);
                        var DistToTrap = HitTrap.BoundingBox.Intersects(CurrentProjectile.Ray);

                        Vector2 BarrelEnd, CollisionEnd;
                        BulletTrail Trail;

                        BarrelEnd = new Vector2(turret.BarrelRectangle.X + (float)Math.Cos(turret.Rotation) * (turret.BarrelRectangle.Width - 20),
                                                        turret.BarrelRectangle.Y + (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height + 20));

                        switch (HitTrap.TrapType)
                        {
                            default:
                                var DistToGround = CurrentProjectile.Ray.Intersects(Ground.BoundingBox);

                                CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)DistToGround),
                                                           turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToGround));

                                if (CurrentProjectile.LightProjectileType == LightProjectileType.MachineGun)
                                    {
                                        Trail = new BulletTrail(BarrelEnd, CollisionEnd);
                                        Trail.LoadContent(Content);
                                        TrailList.Add(Trail);
                                    }

                                if (CurrentProjectile.LightProjectileType == LightProjectileType.Lightning)
                                {
                                    for (int i = 0; i < 5; i++)
                                    {
                                        LightningList.Add(new LightningBolt(BarrelEnd, CollisionEnd,
                                                                            Color.MediumPurple));
                                        LightningList[i].LoadContent(Content);
                                    }
                                }
                             
                                break;

                            case TrapType.Wall:
                                CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)DistToTrap),
                                                           turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToTrap));

                                if (CurrentProjectile.LightProjectileType == LightProjectileType.MachineGun)
                                    {
                                        Trail = new BulletTrail(BarrelEnd, CollisionEnd);
                                        Trail.LoadContent(Content);
                                        TrailList.Add(Trail);
                                    }

                                if (CurrentProjectile.LightProjectileType == LightProjectileType.Lightning)
                                {
                                    for (int i = 0; i < 5; i++)
                                    {
                                        LightningList.Add(new LightningBolt(BarrelEnd, CollisionEnd,
                                                                            Color.MediumPurple));
                                        LightningList[i].LoadContent(Content);
                                    }
                                }

                                EmitterList.Add(new Emitter("Particles/Splodge", new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                    new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 - (float)RandomDouble(0, 45),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - 180 + (float)RandomDouble(0, 45)),
                                    new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360),
                                    new Vector2(1, 3), new Vector2(0.02f, 0.05f), Color.Gray, Color.DarkGray,
                                    0.1f, 0.1f, 10, 2, true, new Vector2(HitTrap.DestinationRectangle.Bottom, HitTrap.DestinationRectangle.Bottom), false, 1, true, false));
                                EmitterList[EmitterList.Count - 1].LoadContent(Content);                                
                                break;

                            case TrapType.Barrel:
                                CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)DistToTrap),
                                                                turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToTrap));

                                    if (CurrentProjectile.LightProjectileType == LightProjectileType.MachineGun)
                                    {
                                        Trail = new BulletTrail(BarrelEnd, CollisionEnd);
                                        Trail.LoadContent(Content);
                                        TrailList.Add(Trail);
                                    }

                                    if (CurrentProjectile.LightProjectileType == LightProjectileType.Lightning)
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            LightningList.Add(new LightningBolt(BarrelEnd, CollisionEnd,
                                                                                Color.MediumPurple));
                                            LightningList[i].LoadContent(Content);
                                        }
                                    }

                                if (HitTrap.CanTrigger == false)
                                {    
                                    HitTrap.CurrentHP -= turret.Damage;
                                }
                                break;
                        }                        
                    }
                    #endregion

                    #region If a projectile hit just an invader
                    if (TrapList.All(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                        InvaderList.Any(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) != null) &&
                        CurrentProjectile.Active == true)
                    {
                        float MinDistToInv = (float)InvaderList.Min(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray));
                        Invader HitInvader = InvaderList.Find(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) == MinDistToInv);
                        var DistToInvader = HitInvader.BoundingBox.Intersects(CurrentProjectile.Ray);

                        Vector2 BarrelEnd, CollisionEnd;
                        BulletTrail Trail;

                        BarrelEnd = new Vector2(turret.BarrelRectangle.X + (float)Math.Cos(turret.Rotation) * (turret.BarrelRectangle.Width - 20),
                                                turret.BarrelRectangle.Y + (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height + 20));

                        CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)DistToInvader),
                                                   turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToInvader));

                        if (CurrentProjectile.LightProjectileType == LightProjectileType.MachineGun)
                        {
                            Trail = new BulletTrail(BarrelEnd, CollisionEnd);
                            Trail.LoadContent(Content);
                            TrailList.Add(Trail);
                        }

                        if (CurrentProjectile.LightProjectileType == LightProjectileType.Lightning)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                LightningList.Add(new LightningBolt(BarrelEnd, CollisionEnd,
                                                                    Color.MediumPurple));
                                LightningList[i].LoadContent(Content);
                            }
                        }

                        HitInvader.TurretDamage(-turret.Damage);
                        if (HitInvader.CurrentHP <= 0)
                            Resources += HitInvader.ResourceValue;

                        switch (HitInvader.InvaderType)
                        {                                
                            case InvaderType.Soldier:
                                EmitterList.Add(new Emitter("Particles/Splodge", new Vector2(HitInvader.DestinationRectangle.Center.X, CollisionEnd.Y),
                                new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - (float)RandomDouble(0, 45),
                                MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) + (float)RandomDouble(0, 45)),
                                new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360),
                                new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.DarkRed, Color.Red,
                                0.2f, 0.1f, 10, 5, true, new Vector2(HitInvader.MaxY, HitInvader.MaxY), false, 1, true, false));

                                EmitterList[EmitterList.Count - 1].LoadContent(Content);                                
                                break;

                            case InvaderType.Spider:
                                EmitterList.Add(new Emitter("Particles/Splodge", new Vector2(HitInvader.DestinationRectangle.Center.X, CollisionEnd.Y),
                                new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - (float)RandomDouble(0, 45),
                                MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) + (float)RandomDouble(0, 45)),
                                new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360),
                                new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.Green, Color.LimeGreen,
                                0.2f, 0.1f, 10, 5, true, new Vector2(HitInvader.MaxY, HitInvader.MaxY), false, 1, true, false));

                                EmitterList[EmitterList.Count - 1].LoadContent(Content);        
                                break;
                        }
                    }
                    #endregion

                    #region If a projectile doesn't hit a trap or an invader but does hit the ground
                    if (TrapList.All(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                        InvaderList.All(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                        CurrentProjectile.Ray.Intersects(Ground.BoundingBox) != null &&
                        CurrentProjectile.Active == true)
                    {
                        var DistToGround = CurrentProjectile.Ray.Intersects(Ground.BoundingBox);
                        double Hypot, Height;

                        Vector2 BarrelEnd, CollisionEnd;
                        BulletTrail Trail;

                        BarrelEnd = new Vector2(turret.BarrelRectangle.X + (float)Math.Cos(turret.Rotation) * (turret.BarrelRectangle.Width - 20),
                                                turret.BarrelRectangle.Y + (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height + 20));
                        if (DistToGround != null)
                        {
                            GroundCollisionPoint.Y = Ground.BoundingBox.Min.Y;

                            Hypot = Math.Pow(DistToGround.Value, 2);
                            Height = Math.Pow(Ground.BoundingBox.Min.Y - turret.BarrelRectangle.Y, 2);

                            if (CurrentMouseState.X > turret.BarrelRectangle.X)
                                GroundCollisionPoint.X = turret.BarrelRectangle.X + (float)Math.Sqrt(Hypot - Height);
                            else
                                GroundCollisionPoint.X = turret.BarrelRectangle.X - (float)Math.Sqrt(Hypot - Height);

                            if (turret.CanShoot == false &&
                                CurrentMouseState.LeftButton == ButtonState.Pressed)
                            {
                                Color DirtColor = new Color();
                                DirtColor.A = 100;
                                DirtColor.R = 51;
                                DirtColor.G = 31;
                                DirtColor.B = 0;

                                Color DirtColor2 = DirtColor;
                                DirtColor2.A = 125;

                                switch (turret.TurretType)
                                {
                                    case TurretType.MachineGun:
                                        CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)DistToGround),
                                                                   turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToGround));

                                        if (CurrentProjectile.LightProjectileType == LightProjectileType.MachineGun)
                                        {
                                            Trail = new BulletTrail(BarrelEnd, CollisionEnd);
                                            Trail.LoadContent(Content);
                                            TrailList.Add(Trail);
                                        }



                                        DirtEmitter = new Emitter("Particles/Smoke", new Vector2(GroundCollisionPoint.X, GroundCollisionPoint.Y),
                                            new Vector2(90, 90), new Vector2(0.5f, 1f), new Vector2(20, 30), 1f, true, new Vector2(0, 0),
                                            new Vector2(-2, 2), new Vector2(0.5f, 1f), DirtColor, DirtColor2, 0f, 0.02f, 10, 1, false, new Vector2(0, 720), false);
                                        EmitterList.Add(DirtEmitter);
                                        EmitterList[EmitterList.IndexOf(DirtEmitter)].LoadContent(Content);
                                        break;

                                    case TurretType.Lightning:
                                        CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)DistToGround),
                                                                   turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToGround));

                                        if (CurrentProjectile.LightProjectileType == LightProjectileType.Lightning)
                                        {
                                            for (int i = 0; i < 5; i++)
                                            {
                                                LightningList.Add(new LightningBolt(BarrelEnd, CollisionEnd,
                                                                                    Color.MediumPurple));
                                                LightningList[i].LoadContent(Content);
                                            }
                                        }

                                        DirtEmitter = new Emitter("Particles/Smoke", new Vector2(GroundCollisionPoint.X, GroundCollisionPoint.Y),
                                            new Vector2(90, 90), new Vector2(0.5f, 1f), new Vector2(20, 30), 1f, true, new Vector2(0, 0),
                                            new Vector2(-2, 2), new Vector2(0.5f, 1f), DirtColor, DirtColor2, 0f, 0.02f, 10, 1, false, new Vector2(0, 720), false);
                                        EmitterList.Add(DirtEmitter);
                                        EmitterList[EmitterList.IndexOf(DirtEmitter)].LoadContent(Content);

                                        break;
                                }
                            }
                        }                        
                    }
                    #endregion

                    #region If a projectile doesn't hit a trap, an invader or the ground
                    if (TrapList.All(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                        InvaderList.All(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                        CurrentProjectile.Ray.Intersects(Ground.BoundingBox) == null &&
                        CurrentProjectile.Active == true)
                    {
                        Vector2 BarrelEnd, CollisionEnd;
                        BulletTrail Trail;

                        BarrelEnd = new Vector2(turret.BarrelRectangle.X + (float)Math.Cos(turret.Rotation) * (turret.BarrelRectangle.Width - 20),
                                                turret.BarrelRectangle.Y + (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height + 20));

                        CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * 1280),
                                                   turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * 1280));

                        if (CurrentProjectile.LightProjectileType == LightProjectileType.MachineGun)
                        {
                            Trail = new BulletTrail(BarrelEnd, CollisionEnd);
                            Trail.LoadContent(Content);
                            TrailList.Add(Trail);
                        }

                        if (CurrentProjectile.LightProjectileType == LightProjectileType.Lightning)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                LightningList.Add(new LightningBolt(BarrelEnd, CollisionEnd,
                                                                    Color.MediumPurple));
                                LightningList[i].LoadContent(Content);
                            }
                        }

                    }
                    #endregion 

                    CurrentProjectile = null;
                }
            }
        }
        #endregion

        #region TRAP stuff that needs to be called every step
        private void TrapPlacement()
        {
            if (CurrentMouseState.LeftButton == ButtonState.Pressed &&
                PreviousMouseState.RightButton == ButtonState.Released &&
                ReadyToPlace == true &&
                TrapList.Count < TrapLimit &&
                CursorPosition.X > (Tower.Position.X + Tower.Texture.Width) &&
                CursorPosition.Y < (630) && CursorPosition.Y > (525))
            {
                foreach (Trap trap in TrapList)
                {
                    if (CursorPosition.X > (trap.DestinationRectangle.X - 32) &&
                        CursorPosition.X < (trap.DestinationRectangle.Right + 32))
                    {
                        return;
                    }
                }

                switch (SelectedTrap)
                {
                    case TrapType.Wall:
                        if (Resources >= new Wall(Vector2.Zero).ResourceCost)
                        {
                            Trap NewTrap = new Wall(new Vector2(CursorPosition.X - 16, CursorPosition.Y));
                            TrapList.Add(NewTrap);
                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);
                            ReadyToPlace = false;
                            Resources -= new Wall(Vector2.Zero).ResourceCost;
                            ClearSelected();
                        }
                        break;

                    case TrapType.Fire:
                        if (Resources >= new FireTrap(Vector2.Zero).ResourceCost)
                        {
                            FireTrapStart.Play();

                            Trap NewTrap = new FireTrap(new Vector2(CursorPosition.X - 16, CursorPosition.Y));
                            TrapList.Add(NewTrap);

                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);

                            Color SmokeColor = Color.Gray;
                            SmokeColor.A = 200;

                            Color SmokeColor2 = Color.WhiteSmoke;
                            SmokeColor.A = 175;

                            Emitter FireEmitter = new Emitter("Particles/FireParticle", new Vector2(NewTrap.Position.X + 16, NewTrap.Position.Y),
                                new Vector2(70, 110), new Vector2(0.5f, 0.75f), new Vector2(40, 60), 0.01f, true, new Vector2(-20, 20),
                                new Vector2(-4, 4), new Vector2(1, 2f), FireColor, FireColor2, 0.0f, -1, 10, 1, false, new Vector2(0, 720),
                                false, CursorPosition.Y / 720);
                            NewTrap.TrapEmitterList.Add(FireEmitter);

                            Emitter SmokeEmitter = new Emitter("Particles/Smoke", new Vector2(NewTrap.Position.X + 16, NewTrap.Position.Y - 4),
                                new Vector2(70, 110), new Vector2(0.2f, 0.5f), new Vector2(250, 350), 1f, true, new Vector2(-20, 20),
                                new Vector2(-4, 4), new Vector2(0.5f, 0.5f), SmokeColor, SmokeColor2, 0.0f, -1, 300, 1, false,
                                new Vector2(0, 720), false, CursorPosition.Y / 720);

                            NewTrap.TrapEmitterList.Add(SmokeEmitter);

                            NewTrap.TrapEmitterList[NewTrap.TrapEmitterList.IndexOf(SmokeEmitter)].LoadContent(Content);

                            NewTrap.TrapEmitterList.Add(FireEmitter);

                            NewTrap.TrapEmitterList[NewTrap.TrapEmitterList.IndexOf(FireEmitter)].LoadContent(Content);

                            ReadyToPlace = false;
                            Resources -= new FireTrap(Vector2.Zero).ResourceCost;
                            ClearSelected();
                        }
                        break;

                    case TrapType.Spikes:
                        if (Resources >= new SpikeTrap(Vector2.Zero).ResourceCost)
                        {
                            Trap NewTrap = new SpikeTrap(new Vector2(CursorPosition.X - 16, CursorPosition.Y));
                            TrapList.Add(NewTrap);
                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);
                            ReadyToPlace = false;
                            Resources -= new SpikeTrap(Vector2.Zero).ResourceCost;
                            ClearSelected();
                        }
                        break;

                    case TrapType.Catapult:
                        if (Resources >= new CatapultTrap(Vector2.Zero).ResourceCost)
                        {
                            Trap NewTrap = new CatapultTrap(new Vector2(CursorPosition.X - 16, CursorPosition.Y));
                            TrapList.Add(NewTrap);
                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);
                            ReadyToPlace = false;
                            Resources -= new CatapultTrap(Vector2.Zero).ResourceCost;
                            ClearSelected();
                        }
                        break;

                    case TrapType.Ice:
                        if (Resources >= new IceTrap(Vector2.Zero).ResourceCost)
                        {
                            Trap NewTrap = new IceTrap(new Vector2(CursorPosition.X - 16, CursorPosition.Y));
                            TrapList.Add(NewTrap);
                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);
                            ReadyToPlace = false;
                            Resources -= new IceTrap(Vector2.Zero).ResourceCost;
                            ClearSelected();
                        }
                        break;

                    case TrapType.Tar:
                        if (Resources >= new TarTrap(Vector2.Zero).ResourceCost)
                        {
                            Trap NewTrap = new TarTrap(new Vector2(CursorPosition.X - 16, CursorPosition.Y));
                            TrapList.Add(NewTrap);
                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);
                            ReadyToPlace = false;
                            Resources -= new TarTrap(Vector2.Zero).ResourceCost;
                            ClearSelected();
                        }
                        break;

                    case TrapType.Barrel:
                        if (Resources >= new FireTrap(Vector2.Zero).ResourceCost)
                        {
                            Trap NewTrap = new BarrelTrap(new Vector2(CursorPosition.X - 16, CursorPosition.Y));
                            TrapList.Add(NewTrap);
                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);
                            ReadyToPlace = false;
                            Resources -= new FireTrap(Vector2.Zero).ResourceCost;
                            ClearSelected();
                        }
                        break;

                    case TrapType.SawBlade:
                        if (Resources >= new SawBladeTrap(Vector2.Zero).ResourceCost)
                        {
                            Trap NewTrap = new SawBladeTrap(new Vector2(CursorPosition.X - 16, CursorPosition.Y));
                            TrapList.Add(NewTrap);
                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);
                            ReadyToPlace = false;
                            Resources -= new SawBladeTrap(Vector2.Zero).ResourceCost;
                            ClearSelected();
                        }
                        break;
                }
            }
        }

        private void TrapUpdate(GameTime gameTime)
        {
            foreach (Trap trap in TrapList)
            {
                trap.Update(gameTime);

                switch (trap.TrapType)
                {
                    case TrapType.Fire:
                         foreach (Explosion explosion in ExplosionList)
                    {
                        if (Vector2.Distance(new Vector2(trap.DestinationRectangle.Center.X, trap.DestinationRectangle.Bottom),
                            explosion.Position) <= explosion.BlastRadius && explosion.Active == true)
                        {
                            if (trap.DestinationRectangle.Center.X > explosion.Position.X)
                            {
                                trap.CurrentAffectedTime = 0;

                                foreach (Emitter emitter in trap.TrapEmitterList)
                                    emitter.AngleRange = new Vector2(0, 45);
                            }

                            if (trap.DestinationRectangle.Center.X <= explosion.Position.X)
                            {
                                trap.CurrentAffectedTime = 0;

                                foreach (Emitter emitter in trap.TrapEmitterList)
                                    emitter.AngleRange = new Vector2(135, 180);
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
                                    emitter.AngleRange = new Vector2(0, 45);
                            }

                            if (trap.DestinationRectangle.Center.X <= enemyExplosion.Position.X)
                            {
                                trap.CurrentAffectedTime = 0;

                                foreach (Emitter emitter in trap.TrapEmitterList)
                                    emitter.AngleRange = new Vector2(135, 180);
                            }
                        }
                    }

                    if (trap.Affected == false)
                    {
                        foreach (Emitter emitter in trap.TrapEmitterList)
                            emitter.AngleRange = Vector2.Lerp(emitter.AngleRange, new Vector2(70, 110), 0.02f);
                    }
                        break;

                    case TrapType.Barrel:
                        if (trap.CurrentHP <= 0)
                        {
                            trap.Active = false;
                            Emitter ExplosionEmitter2 = new Emitter("Particles/Splodge", new Vector2(trap.Position.X, trap.DestinationRectangle.Bottom),
                                       new Vector2(0, 180), new Vector2(1, 4), new Vector2(10, 30), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                       new Vector2(0.02f, 0.06f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 0.2f, 20, 10, true, new Vector2(trap.DestinationRectangle.Bottom + 8, trap.DestinationRectangle.Bottom + 8));

                            EmitterList.Add(ExplosionEmitter2);
                            EmitterList[EmitterList.IndexOf(ExplosionEmitter2)].LoadContent(Content);

                            Emitter ExplosionEmitter = new Emitter("Particles/FireParticle",
                                    new Vector2(trap.Position.X, trap.DestinationRectangle.Bottom),
                                    new Vector2(0, 180), new Vector2(1, 5), new Vector2(1, 20), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 7, false,
                                    new Vector2(0, 720), true);

                            EmitterList.Add(ExplosionEmitter);
                            EmitterList[EmitterList.IndexOf(ExplosionEmitter)].LoadContent(Content);

                            Color SmokeColor1 = Color.Lerp(Color.DarkGray, Color.Transparent, 0.5f);
                            Color SmokeColor2 = Color.Lerp(Color.Gray, Color.Transparent, 0.5f);

                            Emitter newEmitter2 = new Emitter("Particles/Smoke",
                                    new Vector2(trap.Position.X, trap.DestinationRectangle.Bottom),
                                    new Vector2(80, 100), new Vector2(0.5f, 1f), new Vector2(20, 40), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(0, 0), new Vector2(1, 2), SmokeColor1, SmokeColor2, 0.0f, 0.3f, 1, 1, false,
                                    new Vector2(0, 720), false);

                            EmitterList2.Add(newEmitter2);
                            EmitterList2[EmitterList2.IndexOf(newEmitter2)].LoadContent(Content);

                            ExplosionList.Add(new Explosion(PointToVector(trap.DestinationRectangle.Center), 400, 200));
                        }
                        break;
                }

                foreach (Emitter emitter in trap.TrapEmitterList)
                {
                    if (emitter.AddMore == false && emitter.ParticleList.Count == 0)
                    {
                        trap.Active = false;
                    }
                }
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
                        HitTrap.CurrentDetonateDelay = 0;

                        if (HitTrap.CurrentDetonateLimit > 0)
                            HitTrap.CurrentDetonateLimit -= 1;

                        switch (HitTrap.TrapType)
                        {
                            case TrapType.SawBlade:
                                switch (invader.InvaderType)
                                {
                                    case InvaderType.Soldier:
                                    case InvaderType.SuicideBomber:
                                        EmitterList2.Add(new Emitter("Particles/Splodge", new Vector2(invader.DestinationRectangle.Center.X, HitTrap.DestinationRectangle.Center.Y),
                                        new Vector2(0, 65), new Vector2(2, 4), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), new Vector2(1, 3),
                                        new Vector2(0.02f, 0.06f), Color.DarkRed, Color.Red, 0.1f, 0.3f, 20, 20, true, new Vector2(invader.MaxY, invader.MaxY)));
                                        EmitterList2[EmitterList2.Count - 1].LoadContent(Content);
                                        break;

                                    case InvaderType.Slime:
                                        EmitterList2.Add(new Emitter("Particles/Splodge", new Vector2(invader.DestinationRectangle.Center.X, HitTrap.DestinationRectangle.Center.Y),
                                        new Vector2(0, 65), new Vector2(2, 4), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), new Vector2(1, 3),
                                        new Vector2(0.02f, 0.06f), Color.HotPink, Color.LightPink, 0.1f, 0.3f, 20, 20, true, new Vector2(invader.MaxY, invader.MaxY)));
                                        EmitterList2[EmitterList2.Count - 1].LoadContent(Content);
                                        break;
                                }
                                break;
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
                }
                else
                {
                    GameState = TowerDefensePrototype.GameState.GettingName;
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

                Stream stream = container.CreateFile(FileName);

                XmlSerializer serializer = new XmlSerializer(typeof(Profile));

                serializer.Serialize(stream, CurrentProfile);

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

            XmlSerializer serializer = new XmlSerializer(typeof(Profile));

            CurrentProfile = (Profile)serializer.Deserialize(OpenFile);

            //ProfileWeaponList = CurrentProfile.Buttons;
            for (int i = 0; i < CurrentProfile.Buttons.Count; i++)
            {
                ProfileWeaponList[i] = CurrentProfile.Buttons[i];
            }            

            OpenFile.Close();  

            container.Dispose();
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

                XmlSerializer serializer = new XmlSerializer(typeof(Profile));

                serializer.Serialize(stream, CurrentProfile);

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

                XmlSerializer serializer = new XmlSerializer(typeof(Profile));

                ThisProfile = (Profile)serializer.Deserialize(OpenFile);
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

                XmlSerializer serializer = new XmlSerializer(typeof(Profile));

                ThisProfile = (Profile)serializer.Deserialize(OpenFile);

                OpenFile.Close();

                container.Dispose();

                ProfileName = ThisProfile.Name;

                DeleteProfileDialog = new DialogBox(new Vector2(1280 / 2, 720 / 2), "Delete", "Do you want to delete " + ThisProfile.Name + "?", "Cancel");
                DeleteProfileDialog.LoadContent(SecondaryContent);
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
            //TempList2.Add(new Upgrade1());

            CurrentProfile = new Profile()
            {
                Name = NameInput.RealString,
                LevelNumber = 1,

                Points = 10,

                Cannon = true,
                MachineGun = true,
                Catapult = false,
                FlameThrower = false,
                ClusterTurret = false,

                Fire = true,
                Spikes = false,
                LightningTurret = false,
                SawBlade = false,
                Wall = false,
                Ice = false,

                Buttons = TempList,               

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
            CurrentWaveTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentWaveTime >= CurrentWave.WaveTime)
            {
                if (CurrentInvaderIndex < CurrentWave.InvaderList.Count && WavesStarted == true)
                {
                    Invader invader = CurrentWave.InvaderList[CurrentInvaderIndex];
                    invader.LoadContent(Content);
                    InvaderList.Add(invader);
                    CurrentInvaderIndex++;
                }                

                CurrentWaveTime = 0;
            }

            if (CurrentInvaderIndex >= CurrentWave.InvaderList.Count)
            {                
                CurrentWaveIndex++;

                if (CurrentWaveIndex < CurrentLevel.WaveList.Count)
                {                    
                    CurrentInvaderIndex = 0;
                    CurrentWave = CurrentLevel.WaveList[CurrentWaveIndex];
                    WavesStarted = CurrentWave.Overflow;
                }
            }
        }

        //Handle levels
        public void LoadLevel(int number)
        {
            //CurrentLevel = Content.Load<Level>("Levels/Level" + number);
            switch (LevelNumber)
            {
                case 1:
                    CurrentLevel = new Level1();
                    WavesStarted = false;
                    CurrentWaveIndex = 0;
                    CurrentInvaderIndex = 0;
                    CurrentWave = CurrentLevel.WaveList[0];
                    break;
            }
        }


        //Handle Settings
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


        //Handle upgrades
        public void LoadUpgrades()
        {
            foreach (Upgrade upgrade in CurrentProfile.UpgradesList)
            {
                StackedUpgrade.GatlingSpeed += upgrade.GatlingSpeed;
            }

            GatlingSpeed += StackedUpgrade.GatlingSpeed; 
        }

        public void ResetUpgrades()
        {
            StackedUpgrade = new StackedUpgrade();
            GatlingSpeed = 0;
            CannonSpeed = 0;
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
                            CurrentCursorTexture = BlankTexture;
                        break;

                    case TrapType.Fire:
                        CurrentCursorTexture = FireCursor;
                        break;

                    case TrapType.Spikes:
                        CurrentCursorTexture = SpikesCursor;
                        break;

                    case TrapType.Wall:
                        CurrentCursorTexture = WallCursor;
                        break;

                    case TrapType.Ice:
                        CurrentCursorTexture = IceCursor;
                        break;

                    case TrapType.SawBlade:
                        CurrentCursorTexture = SawbladeCursor;
                        break;
                }

                switch (SelectedTurret)
                {
                    case null:
                        if (SelectedTrap == null)
                            CurrentCursorTexture = BlankTexture;
                        break;

                    case TurretType.MachineGun:
                        CurrentCursorTexture = BasicTurretCursor;
                        break;

                    case TurretType.Cannon:
                        CurrentCursorTexture = CannonTurretCursor;
                        break;

                    case TurretType.FlameThrower:
                        CurrentCursorTexture = FlameThrowerCursor;
                        break;
                }
            }


            if (GameState != GameState.Loading)
            {
                if (GameState != GameState.Playing)
                {
                    spriteBatch.Draw(CurrentCursorTexture, new Rectangle((int)CursorPosition.X - (CurrentCursorTexture.Width / 2),
                        (int)CursorPosition.Y - CurrentCursorTexture.Height, CurrentCursorTexture.Width, CurrentCursorTexture.Height), null,
                        Color.White, MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, 1);

                    spriteBatch.Draw(PrimaryCursorTexture, new Rectangle((int)CursorPosition.X, (int)CursorPosition.Y,
                                                    PrimaryCursorTexture.Width, PrimaryCursorTexture.Height), null,
                                            Color.White, MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, 1);
                }
                else
                {
                    spriteBatch.Draw(CurrentCursorTexture, new Rectangle((int)CursorPosition.X - (CurrentCursorTexture.Width / 2),
                        (int)CursorPosition.Y - CurrentCursorTexture.Height, CurrentCursorTexture.Width, CurrentCursorTexture.Height), null,
                        Color.White, MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, 1);

                    if (PrimaryCursorTexture != CrosshairCursor)
                        spriteBatch.Draw(PrimaryCursorTexture, new Rectangle((int)CursorPosition.X, (int)CursorPosition.Y,
                                PrimaryCursorTexture.Width, PrimaryCursorTexture.Height), null,
                        Color.White, MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, 1);
                    else
                        spriteBatch.Draw(PrimaryCursorTexture, new Rectangle((int)CursorPosition.X - PrimaryCursorTexture.Width / 2, (int)CursorPosition.Y - PrimaryCursorTexture.Height / 2,
                            PrimaryCursorTexture.Width, PrimaryCursorTexture.Height), null,
                        Color.White, MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, 1);
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
    }
}
