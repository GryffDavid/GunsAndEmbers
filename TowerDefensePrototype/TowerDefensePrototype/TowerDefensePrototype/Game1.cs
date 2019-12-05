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

namespace TowerDefensePrototype
{
    public enum TrapType { Wall, Spikes, Catapult, Fire, Ice, Tar, Barrel, SawBlade };
    public enum TurretType { MachineGun, Cannon, FlameThrower, Lightning, Cluster };
    public enum InvaderType { Soldier, BatteringRam, Airship, Archer, Tank, Spider, Slime, SuicideBomber, FireElemental };
    public enum HeavyProjectileType { CannonBall, FlameThrower, Arrow, Acid, Torpedo, ClusterBomb, ClusterBombShell };
    public enum LightProjectileType { MachineGun, Freeze, Lightning };
    public enum GameState { Menu, Loading, Playing, Paused, ProfileSelect, Options, ProfileManagement, Tutorial, LoadingGame, GettingName };
    public enum ProfileState { Standard, Upgrades, Stats };

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
        Texture2D BlankTexture, HUDBarTexture, ShellCasing, Coin, PauseMenuBackground;

        Texture2D DefaultCursor, CrosshairCursor, FireCursor, WallCursor, SpikesCursor, BasicTurretCursor,
                  CannonTurretCursor, FlameThrowerCursor, CurrentCursorTexture, PrimaryCursorTexture,
                  SawbladeCursor, BombCursor, IceCursor, BlastRadius;
        Vector2 CursorPosition;
        Rectangle ScreenRectangle;
        SpriteFont ResourceFont;
        MouseState CurrentMouseState, PreviousMouseState;
        KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        int Resources, SelectedTurretIndex, TrapLimit, TowerButtons, ProfileNumber;
        string SelectButtonAssetName, TowerSlotAssetName, FileName, ContainerName, ProfileName;
        bool ReadyToPlace, IsLoading, Slow, BlastRadiusVisible; 
        float MenuSFXVolume, MenuMusicVolume;
        float GatlingSpeed = 0;
        float CannonSpeed = 0;

        //Sound effects
        SoundEffect MenuClick, FireTrapStart, LightningSound, CannonExplosion, CannonFire;
        SoundEffect MachineShot1, GroundImpact, Ricochet1, Ricochet2, Ricochet3;
        SoundEffect PlaceTrap, Splat1, Splat2;

        public Color HalfWhite = Color.Lerp(Color.White, Color.Transparent, 0.5f); 
        public Color FireColor = new Color(Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 200);
        public Color FireColor2 = new Color(Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 90);
        public Color DirtColor = new Color(51,31,0,100);
        public Color DirtColor2 = new Color(51,31,0,125);

        static Random Random = new Random();

        int LevelNumber;
        Level CurrentLevel;
        Wave CurrentWave = null;
        int CurrentWaveIndex = 0;
        int CurrentInvaderIndex = 0;

        float CurrentInvaderTime;
        float CurrentWaveTime;

        int CurrentWaveNumber = 0;
        int MaxWaves = 0;

        double Seconds;
        float VictoryTime;
        bool Victory;

        BinaryFormatter formatter = new BinaryFormatter();

        #region List declarations
        List<Button> SelectButtonList, TowerButtonList, MainMenuButtonList, PauseButtonList,
                     ProfileButtonList, ProfileDeleteList, SelectWeaponList, PlaceWeaponList;

        List<Trap> TrapList;
        List<Turret> TurretList;
        List<Invader> InvaderList;

        List<HeavyProjectile> HeavyProjectileList, InvaderHeavyProjectileList;
        List<LightProjectile> LightProjectileList, InvaderLightProjectileList;
        List<TimerHeavyProjectile> TimedProjectileList;

        List<Emitter> EmitterList, EmitterList2, AlphaEmitterList;
        List<Particle> ShellCasingList, CoinList;

        List<string> MainMenuNameList, PauseMenuNameList, IconNameList;

        List<Explosion> ExplosionList, EnemyExplosionList;

        List<LightningBolt> LightningList;
        List<BulletTrail> TrailList;
        #endregion

        #region Custom class declarations
        Button ProfileBackButton, ProfileManagementPlay, ProfileManagementBack,
               OptionsBack, OptionsSFXUp, OptionsSFXDown, OptionsMusicUp, OptionsMusicDown,
               GetNameOK, GetNameBack, SelectWeaponRight, SelectWeaponLeft;
        Tower Tower;
        StaticSprite Ground, TestBackground, ProfileMenuTitle, MainMenuBackground, SkyBackground, TextBox, Shield, MenuTower;
        AnimatedSprite LoadingAnimation;
        Nullable<TrapType> SelectedTrap;
        Nullable<TurretType> SelectedTurret;
        //HorizontalBar TowerHealthBar;
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
        DialogBox ExitDialog, DeleteProfileDialog, MainMenuDialog, ProfileMenuDialog, NoWeaponsDialog;
        InformationBox WeaponInformation, UpgradeInformation;
        Camera Camera;
        float CameraTime, CameraTime2;
        public StackedUpgrade StackedUpgrade = new StackedUpgrade();
        bool DialogVisible = false;
        ProgressBar BarHealth, BarShield;
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
            //graphics.IsFullScreen = CurrentSettings.FullScreen;
            //graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = true;
        }

        protected override void Initialize()
        {
            SecondaryContent = new ContentManager(Content.ServiceProvider, Content.RootDirectory);
            GameState = GameState.Menu;
            ContainerName = "Profiles";
            TrapLimit = 8;
            Camera = new Camera();
            Tower = new Tower("Tower2", new Vector2(32, 180), 300, 20, 3, 5000);
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

            MainMenuBackground = new StaticSprite("Backgrounds/Space2", new Vector2(0, 0));

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

            SelectWeaponLeft = new Button("Buttons/LeftArrow", new Vector2(40, 720 - 170));
            SelectWeaponLeft.LoadContent(SecondaryContent);

            SelectWeaponRight = new Button("Buttons/RightArrow", new Vector2(1280-40-50, 720 - 170));
            SelectWeaponRight.LoadContent(SecondaryContent);

            SelectWeaponList = new List<Button>();
            for (int i = 0; i < 30; i++)
            {               
                SelectWeaponList.Add(new Button("Buttons/NewButton", new Vector2(98 + (i * 92), 720-177)));
                SelectWeaponList[i].LoadContent(SecondaryContent);
            }

            PlaceWeaponList = new List<Button>();
            for (int i = 0; i < 10; i++)
            {
                PlaceWeaponList.Add(new Button("Buttons/NewButton", new Vector2(140+ (i * 100), 450), null, new Vector2(1.5f, 1.5f), null, "", "", "Left", null, true));
                PlaceWeaponList[i].LoadContent(SecondaryContent);
            }

            MenuTower = new StaticSprite("Tower", new Vector2(1280 - 300, 150));
            MenuTower.LoadContent(SecondaryContent);

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

                Tower.Update(gameTime);

                Seconds += gameTime.ElapsedGameTime.TotalMilliseconds;

                #region Testing shake
                CameraTime2 += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (CameraTime2 >= 100)
                {
                    Camera.Rotation = 0;
                }
                #endregion

                Shield.Update(gameTime);

                InvaderUpdate(gameTime);

                RightClickClearSelected();

                SelectButtonsUpdate();

                TowerButtonUpdate();                

                AttackTower();
                AttackTraps();

                RangedAttackTower();
                RangedAttackTraps();

                HandleWaves(gameTime);

                if (GameState != GameState.Playing)
                    return;

                //TowerHealthBar.Update(new Vector2(90, 19), (int)Tower.CurrentHP);
                BarHealth.Update(Tower.CurrentHP);
                BarShield.Update(Tower.CurrentShield);

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

                foreach (Emitter emitter in AlphaEmitterList)
                {
                    emitter.Update(gameTime);
                }

                for (int i = 0; i < AlphaEmitterList.Count; i++)
                {
                    if (AlphaEmitterList[i].Active == false)
                        AlphaEmitterList.RemoveAt(i);
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

                for (int i = 0; i < InvaderList.Count; i++)
                {
                    if (InvaderList[i].ParticleEmitter.Active == false &&                         
                        InvaderList[i].ParticleEmitter.ParticleList.Count == 0)
                        InvaderList.RemoveAt(i);                        
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

                if (CameraTime2 <= 100)
                {
                    CameraTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (CameraTime >= 20)
                    {
                        if (Camera.Rotation != -0.01f)
                        {
                            Camera.Rotation = MathHelper.Lerp(Camera.Rotation, -0.01f, 1f);
                            CameraTime = 0;
                            return;
                        }

                        if (Camera.Rotation != 0.01f)
                        {
                            Camera.Rotation = MathHelper.Lerp(Camera.Rotation, 0.01f, 1f);
                            CameraTime = 0;
                            return;
                        }
                    }
                }
            }

            MenuButtonsUpdate();

            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;

            LoadingAnimation.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            #region Draw menus
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
                spriteBatch.DrawString(ButtonFont, "Level: " + CurrentProfile.LevelNumber.ToString(), new Vector2(640, 192), Color.White);
                spriteBatch.DrawString(ButtonFont, SelectedTurret.ToString(), new Vector2(0, 0), Color.Red);
                spriteBatch.DrawString(ButtonFont, SelectedTrap.ToString(), new Vector2(0, 0), Color.Red);

                ProfileManagementPlay.Draw(spriteBatch);
                ProfileManagementBack.Draw(spriteBatch);

                if (UpgradeInformation != null)
                    UpgradeInformation.Draw(spriteBatch);

                foreach (Button button in SelectWeaponList)
                {
                    if (button.DestinationRectangle.Left > (SelectWeaponLeft.DestinationRectangle.Left - 24) &&
                        button.DestinationRectangle.Right < (SelectWeaponRight.DestinationRectangle.Right + 24))
                    {
                        button.Draw(spriteBatch);
                    }



                    if (button.DestinationRectangle.Right > SelectWeaponRight.DestinationRectangle.Center.X)
                    {
                        button.Color = Color.Lerp(button.Color, Color.Transparent, 0.3f);
                        button.CurrentIconColor = Color.Lerp(button.CurrentIconColor, Color.Transparent, 0.3f);
                    }

                    if (button.DestinationRectangle.Left < SelectWeaponLeft.DestinationRectangle.Center.X)
                    {
                        button.Color = Color.Lerp(button.Color, Color.Transparent, 0.3f);
                        button.CurrentIconColor = Color.Lerp(button.CurrentIconColor, Color.Transparent, 0.3f);
                    }



                    if (button.DestinationRectangle.Right < SelectWeaponRight.DestinationRectangle.Center.X && 
                        button.DestinationRectangle.Center.X > SelectWeaponLeft.DestinationRectangle.Center.X)
                    {
                        button.Color = Color.Lerp(button.Color, Color.White, 0.2f);
                        button.CurrentIconColor = Color.Lerp(button.CurrentIconColor, Color.White, 0.2f);
                    }

                    if (button.DestinationRectangle.Left > SelectWeaponLeft.DestinationRectangle.Center.X &&
                        button.DestinationRectangle.Center.X < SelectWeaponRight.DestinationRectangle.Center.X)
                    {
                        button.Color = Color.Lerp(button.Color, Color.White, 0.2f);
                        button.CurrentIconColor = Color.Lerp(button.CurrentIconColor, Color.White, 0.2f);
                    }
                }

                foreach (Button button in PlaceWeaponList)
                {
                    button.Draw(spriteBatch);
                }

                SelectWeaponLeft.Draw(spriteBatch);
                SelectWeaponRight.Draw(spriteBatch);

                MenuTower.Draw(spriteBatch);
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

            #region Draw things in game that SHOULD be shaken - Non-diegetic elements
            if (GameState == GameState.Playing || GameState == GameState.Paused && IsLoading == false)
            {
                GraphicsDevice.Clear(Color.Black);

                spriteBatch.Begin(SpriteSortMode.Deferred,
                            BlendState.AlphaBlend,
                            null,
                            null,
                            null,
                            null,
                            Camera.Transformation(GraphicsDevice));
                            
                SkyBackground.Draw(spriteBatch);
                TestBackground.Draw(spriteBatch);
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
                spriteBatch.DrawString(ButtonFont, Seconds.ToString(), new Vector2(32, 32), Color.Yellow);

                spriteBatch.End();
            }
            #endregion

            #region Draw stuff that isn't shaky
            if (GameState == GameState.Playing || GameState == GameState.Paused && IsLoading == false)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred,
                            BlendState.AlphaBlend);

                if (BlastRadiusVisible == true)
                {
                    spriteBatch.Draw(BlastRadius, new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 300, 100), new Rectangle(0, 0, BlastRadius.Width, BlastRadius.Height), HalfWhite, 0, new Vector2(BlastRadius.Width / 2, BlastRadius.Height / 2), SpriteEffects.None, 1);
                }

                BarHealth.Draw(spriteBatch);
                BarShield.Draw(spriteBatch);

                #region Drawing buttons
                spriteBatch.Draw(HUDBarTexture, new Rectangle(graphics.PreferredBackBufferWidth / 2, 728, HUDBarTexture.Width, HUDBarTexture.Height), null, Color.White, 0, new Vector2(HUDBarTexture.Width / 2, HUDBarTexture.Height), SpriteEffects.None, 0);

                foreach (Button button in SelectButtonList)
                {
                    button.Draw(spriteBatch);
                }
                #endregion

                spriteBatch.DrawString(HUDFont, HeavyProjectileList.Count.ToString(), new Vector2(10, 720 - 30), Color.White);

                int PercentageHP = (int)MathHelper.Clamp((float)((100d / (double)Tower.MaxHP) * (double)Tower.CurrentHP), 0, 100);
                int PercentageShield = (int)MathHelper.Clamp((float)((100d / (double)Tower.MaxShield) * (double)Tower.CurrentShield), 0, 100);

                spriteBatch.DrawString(HUDFont, PercentageHP.ToString() + "%", new Vector2(1280 / 2 + 1, 12), Color.Black);
                spriteBatch.DrawString(HUDFont, PercentageHP.ToString() + "%", new Vector2(1280 / 2, 11), Color.LightGray);

                spriteBatch.DrawString(HUDFont, PercentageShield.ToString() + "%", new Vector2(1280 / 2 + 1, 34), Color.Black);
                spriteBatch.DrawString(HUDFont, PercentageShield.ToString() + "%", new Vector2(1280 / 2, 33), Color.LightGray);

                spriteBatch.DrawString(HUDFont, Resources.ToString(), new Vector2(0, 100), Color.White);
                spriteBatch.DrawString(HUDFont, (CurrentWaveNumber).ToString() + "/" + MaxWaves, new Vector2(250, 100), Color.White);

                spriteBatch.DrawString(HUDFont, Tower.ShieldOn.ToString(), new Vector2(100, 100), Color.White);
                spriteBatch.DrawString(HUDFont, Tower.CurrentShieldTime.ToString(), new Vector2(100, 120), Color.White);

                spriteBatch.DrawString(HUDFont, CurrentWaveTime.ToString(), new Vector2(300, 400), Color.Yellow);               

                spriteBatch.End();
            }
            #endregion

            #region Draw with additive blending - Makes stuff look glowy
            if (GameState == GameState.Playing || GameState == GameState.Paused && IsLoading == false)
            {
                spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive);
                //Shield.Draw(spriteBatch);

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

                foreach (Button towerSlot in TowerButtonList)
                {
                    towerSlot.Draw(spriteBatch);
                }
                spriteBatch.End();
            }            
            #endregion

            #region Draw stuff that SHOULD be shaken with ADDITIVE blending
            if (GameState == GameState.Playing || GameState == GameState.Paused && IsLoading == false)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred,
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
            

            if (GameState == GameState.Playing || GameState == GameState.Paused && IsLoading == false)
            {
                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend,
                            null,
                            null,
                            null,
                            null,
                            Camera.Transformation(GraphicsDevice));
                //This draws the timing bar for the turrets, but also makes 
                //sure that it doesn't draw for the blank turrets, which
                //would make the timing bar appear in the top right corner
                foreach (Turret turret in TurretList)
                {
                    if (turret != null)
                    {
                        turret.TimingBar.Draw(spriteBatch);
                        turret.HealthBar.Draw(spriteBatch);
                    }
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

                spriteBatch.End();
            }            

            
            #endregion

            #region Draw things that have to be drawn on top of everything else
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

            if (NoWeaponsDialog != null)
            {
                NoWeaponsDialog.Draw(spriteBatch);
            }
            #endregion

            CursorDraw(spriteBatch);
            spriteBatch.DrawString(HUDFont, Slow.ToString(), Vector2.Zero, Color.Red);

            if (Victory == true && GameState == GameState.Playing && IsLoading == false)
            {                
                spriteBatch.Draw(PauseMenuBackground, new Rectangle(0, 0, 1280, 720), Color.White);
                spriteBatch.DrawString(HUDFont, "Victory.", new Vector2(1280 / 2, 720 / 2), Color.White);
            }

            spriteBatch.End();
            #endregion
        }

        #region Handle Game Content
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            MainMenuBackground.LoadContent(SecondaryContent);

            BlankTexture = SecondaryContent.Load<Texture2D>("Blank");

            DefaultCursor = SecondaryContent.Load<Texture2D>("Cursors/DefaultCursor");
            CrosshairCursor = SecondaryContent.Load<Texture2D>("Cursors/Crosshair");
            FireCursor = SecondaryContent.Load<Texture2D>("Icons/FireTrapIcon");
            WallCursor = SecondaryContent.Load<Texture2D>("Icons/WallIcon");
            SpikesCursor = SecondaryContent.Load<Texture2D>("Icons/SpikesIcon");
            SawbladeCursor = SecondaryContent.Load<Texture2D>("Icons/SawbladeIcon");
            BombCursor = SecondaryContent.Load<Texture2D>("Cursors/BombCursor");
            IceCursor = SecondaryContent.Load<Texture2D>("Icons/SnowFlakeIcon");

            BasicTurretCursor = SecondaryContent.Load<Texture2D>("Icons/MachineGunTurretIcon");
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
                ProfileManagementPlay.JustClicked = false;

                IsLoading = true;

                CameraTime2 = 200;

                BarHealth = new ProgressBar(new Vector2(1280 / 2, 16), "HealthBar/Background", "HealthBar/Outline", "HealthBar/HealthBarForeground", Tower.MaxHP, Tower.CurrentHP);
                BarHealth.LoadContent(Content);

                BarShield = new ProgressBar(new Vector2(1280 / 2, 38), "HealthBar/Background", "HealthBar/Outline", "HealthBar/ShieldBarForeground", Tower.MaxShield, Tower.CurrentShield);
                BarShield.LoadContent(Content);

                HUDBarTexture = Content.Load<Texture2D>("InterfaceBar");

                BlastRadius = Content.Load<Texture2D>("BlastRadius");                

                ReadyToPlace = false;

                Ground = new StaticSprite("Ground1", new Vector2(-40, -40));
                Ground.LoadContent(Content);
                Ground.Position = new Vector2(-40, -40);

                TestBackground = new StaticSprite("BackgroundTest2", new Vector2(-40, -40));
                TestBackground.Scale = new Vector2(1, 1);
                TestBackground.LoadContent(Content);

                SelectButtonAssetName = "InterfaceButton";
                TowerSlotAssetName = "Buttons/TurretSlotButton";

                SkyBackground = new StaticSprite("LightSky", Vector2.Zero);
                SkyBackground.LoadContent(Content);

                Shield = new StaticSprite("Shield", Vector2.Zero, null, Color.Lerp(Color.White, Color.Transparent, 0.5f), null, null, null, null, null, 100);
                Shield.Transparency = 1;
                Shield.LoadContent(Content);
                

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

                    IconNameList[Index] = PlaceWeaponList[Index].IconName;
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

                //TowerHealthBar = new HorizontalBar(Content, new Vector2(220, 20), (int)Tower.MaxHP, (int)Tower.CurrentHP);
                ShellCasing = Content.Load<Texture2D>("Particles/MachineShell");
                Coin = Content.Load<Texture2D>("Particles/Coin");
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
                    Button button = new Button(SelectButtonAssetName, new Vector2(270 + (i * 73), 720 - HUDBarTexture.Height + 23), IconNameList[i], null, null, "", "", "Left", null, false);
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
                AlphaEmitterList = new List<Emitter>();
                ExplosionList = new List<Explosion>();
                EnemyExplosionList = new List<Explosion>();
                ShellCasingList = new List<Particle>();
                CoinList = new List<Particle>();
                #endregion
                                
                MaxWaves = CurrentLevel.WaveList.Count;
                CurrentWaveNumber = 1;
                CurrentWaveIndex = 0;

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
        }
        #endregion


        #region BUTTON stuff that needs to be done every step
        private void TowerButtonUpdate()
        {
            //This places the selected turret type into the right slot on the tower when the tower slot has been clicked
            int Index;

            foreach (Button towerButton in TowerButtonList)
            {
                if (this.IsActive == true)
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

                                if (Resources >= new MachineGunTurret(Vector2.Zero).ResourceCost)
                                {
                                    TowerButtonList[Index].ButtonActive = false;
                                    TurretList[Index] = new MachineGunTurret(TowerButtonList[Index].CurrentPosition); //Fix this to make sure that the BasicTurret has the resource names built in.

                                    //Apply the upgrades for the gatling turret in here
                                    TurretList[Index].FireDelay = PercentageChange(TurretList[Index].FireDelay, GatlingSpeed);
                                    TurretList[Index].Damage = (int)PercentageChange(TurretList[Index].Damage, 0);

                                    TurretList[Index].LoadContent(Content);
                                    Resources -= new MachineGunTurret(Vector2.Zero).ResourceCost;
                                    SelectedTurret = null;
                                    TurretList[Index].Selected = true;
                                }
                                break;

                            case TurretType.Cannon:
                                if (Resources >= new CannonTurret(Vector2.Zero).ResourceCost)
                                {
                                    TowerButtonList[Index].ButtonActive = false;
                                    TurretList[Index] = new CannonTurret(TowerButtonList[Index].CurrentPosition);

                                    //Apply the upgrades for the cannon turret in here
                                    TurretList[Index].FireDelay = PercentageChange(TurretList[Index].FireDelay, CannonSpeed);
                                    TurretList[Index].Damage = (int)PercentageChange(TurretList[Index].Damage, 0);

                                    TurretList[Index].LoadContent(Content);
                                    Resources -= new CannonTurret(Vector2.Zero).ResourceCost;
                                    SelectedTurret = null;
                                    TurretList[Index].Selected = true;


                                }
                                break;

                            case TurretType.FlameThrower:
                                if (Resources >= new FlameThrowerTurret(Vector2.Zero).ResourceCost)
                                {
                                    TowerButtonList[Index].ButtonActive = false;
                                    TurretList[Index] = new FlameThrowerTurret(TowerButtonList[Index].CurrentPosition);

                                    //Apply the upgrades for the flamethrower turret in here

                                    TurretList[Index].LoadContent(Content);
                                    Resources -= new FlameThrowerTurret(Vector2.Zero).ResourceCost;
                                    SelectedTurret = null;
                                    TurretList[Index].Selected = true;

                                }
                                break;

                            case TurretType.Lightning:
                                if (Resources >= new LightningTurret(Vector2.Zero).ResourceCost)
                                {
                                    TowerButtonList[Index].ButtonActive = false;
                                    TurretList[Index] = new LightningTurret(TowerButtonList[Index].CurrentPosition);

                                    //Apply the upgrades for the flamethrower turret in here

                                    TurretList[Index].LoadContent(Content);
                                    Resources -= new LightningTurret(Vector2.Zero).ResourceCost;
                                    SelectedTurret = null;
                                    TurretList[Index].Selected = true;
                                }
                                break;

                            case TurretType.Cluster:
                                if (Resources >= new ClusterTurret(Vector2.Zero).ResourceCost)
                                {
                                    TowerButtonList[Index].ButtonActive = false;
                                    TurretList[Index] = new ClusterTurret(TowerButtonList[Index].CurrentPosition);

                                    //Apply the upgrades for the flamethrower turret in here

                                    TurretList[Index].LoadContent(Content);
                                    Resources -= new ClusterTurret(Vector2.Zero).ResourceCost;
                                    SelectedTurret = null;
                                    TurretList[Index].Selected = true;
                                }
                                break;
                        }
                    }
                }
            }
        }

        private void SelectButtonsUpdate()
        {
            //This makes sure that when the button at the bottom of the screen is clicked, the corresponding trap or turret is actually selected//
            //This will code will need to be added to every time that a new trap/turret is added to the game.

            int Index, Index2;

            foreach (Button button in SelectButtonList)
            {
                if (this.IsActive == true)
                {
                    button.Update();

                    #region Check if a select button has been clicked
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
                                            if (CurrentProfile.FireTrap == true && Resources >= new FireTrap(Vector2.Zero).ResourceCost)
                                            {
                                                SelectedTrap = TrapType.Fire;
                                                SelectedTurret = null;
                                            }
                                        }
                                        break;

                                    case "IceTrap":
                                        {
                                            if (CurrentProfile.IceTrap == true && Resources >= new IceTrap(Vector2.Zero).ResourceCost)
                                            {
                                                SelectedTrap = TrapType.Ice;
                                            }
                                        }
                                        break;

                                    case "WallTrap":
                                        {
                                            if (CurrentProfile.WallTrap == true && Resources >= new Wall(Vector2.Zero).ResourceCost)
                                            {
                                                SelectedTrap = TrapType.Wall;
                                                SelectedTurret = null;
                                            }
                                        }
                                        break;

                                    case "MachineGunTurret":
                                        {
                                            if (CurrentProfile.MachineGunTurret == true && Resources >= new MachineGunTurret(Vector2.Zero).ResourceCost)
                                            {
                                                SelectedTrap = null;
                                                SelectedTurret = TurretType.MachineGun;
                                            }
                                        }
                                        break;

                                    case "CannonTurret":
                                        {
                                            if (CurrentProfile.CannonTurret == true && Resources >= new CannonTurret(Vector2.Zero).ResourceCost)
                                            {
                                                SelectedTrap = null;
                                                SelectedTurret = TurretType.Cannon;
                                            }
                                        }
                                        break;

                                    case "SawBladeTrap":
                                        {
                                            if (CurrentProfile.SawBladeTrap == true && Resources >= new SawBladeTrap(Vector2.Zero).ResourceCost)
                                            {
                                                SelectedTrap = TrapType.SawBlade;
                                                SelectedTurret = null;
                                            }
                                        }
                                        break;

                                    case "FlameThrowerTurret":
                                        {
                                            if (CurrentProfile.FlameThrowerTurret == true && Resources >= new FlameThrowerTurret(Vector2.Zero).ResourceCost)
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
                    #endregion

                    #region Change Icon Colour if it's unavailable
                    Index2 = SelectButtonList.IndexOf(button);

                    if (Index2 <= CurrentProfile.Buttons.Count - 1)
                    {
                        switch (CurrentProfile.Buttons[Index2])
                        {
                            case "FireTrap":
                                {
                                    if (Resources < new FireTrap(Vector2.Zero).ResourceCost)
                                    {
                                        button.CurrentIconColor = Color.Gray;
                                    }
                                    else
                                    {
                                        button.CurrentIconColor = Color.White;
                                    }
                                }
                                break;

                            case "MachineGunTurret":
                                {
                                    if (Resources < new MachineGunTurret(new Vector2(0, 0)).ResourceCost)
                                    {
                                        button.CurrentIconColor = Color.Gray;
                                    }
                                    else
                                    {
                                        button.CurrentIconColor = Color.White;
                                    }
                                }
                                break;
                        }
                    }
                    #endregion
                }
            }
        }

        private void MenuButtonsUpdate()
        {
            if (this.IsActive == true)
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
                                    ExitDialog = new DialogBox(new Vector2(1280 / 2, 720 / 2), "Exit", "Do you want to exit?", "Cancel");
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
                                    ExitDialog = new DialogBox(new Vector2(1280 / 2, 720 / 2), "Exit", "Do you want to exit? All progress will be lost.", "Cancel");
                                    ExitDialog.LoadContent(SecondaryContent);
                                    DialogVisible = true;
                                    break;
                            }
                        }
                    }
                }
                #endregion

                #region Handling Profile Management Button Presses
                if (GameState == GameState.ProfileManagement)
                {
                    ProfileManagementPlay.Update();
                    ProfileManagementBack.Update();
                    RightClickClearSelected();

                    #region Play Button
                    if (ProfileManagementPlay.JustClicked == true)
                    {
                        if (CurrentProfile != null)
                        {
                            if (CurrentProfile.Buttons.All(String => String == null))
                            {
                                NoWeaponsDialog = new DialogBox(new Vector2(1280 / 2, 720 / 2), "OK", "You have no weapons to use!", "");
                                NoWeaponsDialog.LoadContent(SecondaryContent);
                                DialogVisible = true;
                            }
                            else
                            {
                                SelectedTrap = null;
                                SelectedTurret = null;
                                LevelNumber = CurrentProfile.LevelNumber;
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
                    if (ProfileManagementBack.JustClicked == true)
                    {
                        SetProfileNames();
                        SelectedTrap = null;
                        SelectedTurret = null;
                        StorageDevice.BeginShowSelector(this.SaveProfile, null);
                        GameState = GameState.ProfileSelect;
                        //CurrentProfile = null;
                    }
                    #endregion

                    #region Select Weapon Buttons
                    //(i.e. The buttons that show which weapons are available and let the player select one)
                    //#region Change color if weapon has already been placed.
                    //foreach (Button button in SelectWeaponList)
                    //{
                    //    switch (SelectWeaponList.IndexOf(button))
                    //    {
                    //        case 0:
                    //            if (CurrentProfile.Buttons.Contains("MachineGunTurret"))
                    //                SelectWeaponList[SelectWeaponList.IndexOf(button)].CurrentIconColor = Color.Gray;
                    //            else
                    //                SelectWeaponList[SelectWeaponList.IndexOf(button)].CurrentIconColor = Color.White;
                    //            break;

                    //        case 1:
                    //            if (CurrentProfile.Buttons.Contains("LightningTurret"))
                    //                SelectWeaponList[SelectWeaponList.IndexOf(button)].CurrentIconColor = Color.Gray;
                    //            else
                    //                SelectWeaponList[SelectWeaponList.IndexOf(button)].CurrentIconColor = Color.White;
                    //            break;

                    //        case 2:
                    //            if (CurrentProfile.Buttons.Contains("FireTrap"))
                    //                SelectWeaponList[SelectWeaponList.IndexOf(button)].CurrentIconColor = Color.Gray;
                    //            else
                    //                SelectWeaponList[SelectWeaponList.IndexOf(button)].CurrentIconColor = Color.White;
                    //            break;
                    //    }
                    //}
                    //#endregion

                    if (SelectWeaponList.Any(Button => Button.DestinationRectangle.Contains(new Point(Mouse.GetState().X, Mouse.GetState().Y))))
                    {
                        Button MousedButton = SelectWeaponList.Find(Button => Button.DestinationRectangle.Contains(new Point(Mouse.GetState().X, Mouse.GetState().Y)));

                        if (MousedButton != null)
                        if (MousedButton.Active == true && 
                            MousedButton.DestinationRectangle.Left > SelectWeaponLeft.DestinationRectangle.Right && 
                            MousedButton.DestinationRectangle.Right < SelectWeaponRight.DestinationRectangle.Left && 
                            WeaponInformation == null)
                        {
                            switch (SelectWeaponList.IndexOf(MousedButton))
                            {
                                default:
                                    WeaponInformation = new InformationBox("Complete more levels to unlock this weapon");
                                    break;

                                case 0:
                                    if (CurrentProfile.MachineGunTurret == true)
                                        WeaponInformation = new InformationBox("Machine gun turret is unlocked.");
                                    else
                                        WeaponInformation = new InformationBox("Complete more levels to unlock this weapon");
                                    break;

                                case 1:
                                    if (CurrentProfile.LightningTurret == true)
                                        WeaponInformation = new InformationBox("Lightning turret is unlocked.");
                                    else
                                        WeaponInformation = new InformationBox("Complete more levels to unlock this weapon");
                                    break;

                                case 2:
                                    if (CurrentProfile.FireTrap == true)
                                        WeaponInformation = new InformationBox("Fire trap is unlocked.");
                                    else
                                        WeaponInformation = new InformationBox("Complete more levels to unlock this weapon");
                                    break;
                            }
                        }

                        if (WeaponInformation != null && WeaponInformation.Loaded == false)
                            WeaponInformation.LoadContent(SecondaryContent);
                    }

                    if (SelectWeaponList.All(Button => !Button.DestinationRectangle.Contains(new Point(Mouse.GetState().X, Mouse.GetState().Y))))
                    {
                        WeaponInformation = null;
                    }

                    foreach (Button button in SelectWeaponList)
                    {
                        //if (button.CurrentPosition.X > 40 && button.CurrentPosition.X < 1280 - 50 - 40)                        
                            button.Update();

                            if (button.DestinationRectangle.Left > SelectWeaponLeft.DestinationRectangle.Right &&
                                button.DestinationRectangle.Right < SelectWeaponRight.DestinationRectangle.Left)
                                button.Active = true;
                            else
                                button.Active = false;

                        #region Check which weapon has been selected and if it can be selected
                        if (button.JustClicked == true)
                        {
                            switch (SelectWeaponList.IndexOf(button))
                            {
                                case 0:
                                    if (CurrentProfile.MachineGunTurret == true && !CurrentProfile.Buttons.Contains("MachineGunTurret"))
                                    {
                                        SelectedTrap = null;
                                        SelectedTurret = TurretType.MachineGun;
                                    }
                                    break;

                                case 1:
                                    if (CurrentProfile.LightningTurret == true && !CurrentProfile.Buttons.Contains("LightningTurret"))
                                    {
                                        SelectedTrap = null;
                                        SelectedTurret = TurretType.Lightning;
                                    }
                                    break;

                                case 2:
                                    if (CurrentProfile.FireTrap == true && !CurrentProfile.Buttons.Contains("FireTrap"))
                                    {
                                        SelectedTrap = TrapType.Fire;
                                        SelectedTurret = null;
                                    }
                                    break;
                            }
                        }
                        #endregion

                        #region Change the icon depending if the player has access to that weapon
                        for (int i = 0; i < SelectWeaponList.Count; i++)
                        {
                            if (SelectWeaponList[i].IconName == "Icons/LockIcon")
                                switch (i)
                                {
                                    case 0:
                                        if (CurrentProfile.MachineGunTurret == true)
                                        {
                                            SelectWeaponList[i].IconName = "Icons/MachineGunTurretIcon";
                                            SelectWeaponList[i].LoadContent(SecondaryContent);
                                        }
                                        break;

                                    case 1:
                                        if (CurrentProfile.LightningTurret == true)
                                        {
                                            SelectWeaponList[i].IconName = "Icons/MachineGunTurretIcon";
                                            SelectWeaponList[i].LoadContent(SecondaryContent);
                                        }
                                        break;

                                    case 2:
                                        if (CurrentProfile.FireTrap == true)
                                        {
                                            SelectWeaponList[i].IconName = "Icons/FireTrapIcon";
                                            SelectWeaponList[i].LoadContent(SecondaryContent);
                                        }
                                        break;
                                }
                        }
                        #endregion
                    }

                    #endregion

                    #region Place Weapon Buttons
                    foreach (Button button in PlaceWeaponList)
                    {
                        button.Update();

                        if (button.JustClicked == true)
                        {
                            if (SelectedTurret == null && SelectedTrap == null)
                            {
                                switch (CurrentProfile.Buttons[PlaceWeaponList.IndexOf(button)])
                                {
                                    case "MachineGunTurret":
                                        SelectedTurret = TurretType.MachineGun;
                                        SelectedTrap = null;
                                        CurrentProfile.Buttons[PlaceWeaponList.IndexOf(button)] = null;
                                        PlaceWeaponList[PlaceWeaponList.IndexOf(button)].IconName = null;
                                        PlaceWeaponList[PlaceWeaponList.IndexOf(button)].LoadContent(SecondaryContent);
                                        break;

                                    case "FireTrap":
                                        SelectedTurret = null;
                                        SelectedTrap = TrapType.Fire;
                                        CurrentProfile.Buttons[PlaceWeaponList.IndexOf(button)] = null;
                                        PlaceWeaponList[PlaceWeaponList.IndexOf(button)].IconName = null;
                                        PlaceWeaponList[PlaceWeaponList.IndexOf(button)].LoadContent(SecondaryContent);
                                        break;
                                }
                                return;
                            }

                            switch (SelectedTurret)
                            {
                                case TurretType.MachineGun:
                                    CurrentProfile.Buttons[PlaceWeaponList.IndexOf(button)] = "MachineGunTurret";
                                    PlaceWeaponList[PlaceWeaponList.IndexOf(button)].IconName = "Icons/MachineGunTurretIcon";
                                    PlaceWeaponList[PlaceWeaponList.IndexOf(button)].LoadContent(SecondaryContent);
                                    SelectedTurret = null;
                                    SelectedTrap = null;
                                    break;
                            }

                            switch (SelectedTrap)
                            {
                                case TrapType.Fire:
                                    CurrentProfile.Buttons[PlaceWeaponList.IndexOf(button)] = "FireTrap";
                                    PlaceWeaponList[PlaceWeaponList.IndexOf(button)].IconName = "Icons/FireTrapIcon";
                                    PlaceWeaponList[PlaceWeaponList.IndexOf(button)].LoadContent(SecondaryContent);
                                    SelectedTurret = null;
                                    SelectedTrap = null;
                                    break;
                            }
                        }

                        if (button.JustRightClicked == true)
                        {
                            CurrentProfile.Buttons[PlaceWeaponList.IndexOf(button)] = null;
                            button.IconName = null;
                            button.LoadContent(SecondaryContent);
                        }
                    }
                    #endregion

                    #region Move weapons left
                    SelectWeaponLeft.Update();

                    if (SelectWeaponLeft.JustClicked == true)
                    {
                        foreach (Button button in SelectWeaponList)
                        {
                            button.NextPosition.X -= 92;
                            button.IconNextPosition.X -= 92;
                        }
                    }
                    #endregion

                    #region Move weapons right
                    SelectWeaponRight.Update();

                    if (SelectWeaponRight.JustClicked == true)
                    {
                        foreach (Button button in SelectWeaponList)
                        {
                            button.NextPosition.X += 92;
                            button.IconNextPosition.X += 92;
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
                        button.Update();

                        if (button.JustClicked == true)
                        {

                            MenuClick.Play();

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

                if (NoWeaponsDialog != null)
                {
                    NoWeaponsDialog.Update();

                    if (NoWeaponsDialog.LeftButton.JustClicked == true)
                    {
                        MenuClick.Play();
                        DialogVisible = false;
                        NoWeaponsDialog = null;
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
                invader.Update(gameTime);
                invader.Move();

                #region This controls what happens when the invaders die, depending on their type
                if (invader.CurrentHP <= 0)
                {
                    for (int i = 0; i < invader.ResourceValue/5; i++)
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
                          new Vector2(0.02f, 0.06f), Color.DarkRed, Color.Red, 0.1f, 0.2f, 20, 10, true, new Vector2(invader.MaxY, invader.MaxY),false, null, true, false));
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

                RangedInvader rangedInvader = invader as RangedInvader;

                if (rangedInvader != null)
                {
                    switch (rangedInvader.InvaderType)
                    {
                        case InvaderType.Archer:
                            var value = Vector2.Distance(rangedInvader.Position, new Vector2(Tower.DestinationRectangle.Left, Tower.DestinationRectangle.Bottom));

                            if (value < rangedInvader.Range.Y + Random.Next(0,30))
                            {
                                invader.CurrentMoveVector = Vector2.Zero;
                            }
                            break;
                    }
                }
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
            foreach (Invader invader in InvaderList)
            {
                if (Vector2.Distance(PointToVector(invader.DestinationRectangle.Center), new Vector2(Tower.DestinationRectangle.Right, invader.DestinationRectangle.Center.Y)) < 5)
                {
                    invader.CurrentMoveVector = new Vector2(0, 0);

                    if (invader.CanAttack == true)
                        Tower.TakeDamage(invader.AttackPower);
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
            //                                Emitter newEmitter = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
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
                if (heavyProjectile.DestinationRectangle.Intersects(Tower.DestinationRectangle) 
                    && heavyProjectile.Active == true)
                {
                    switch (heavyProjectile.HeavyProjectileType)
                    {
                        case HeavyProjectileType.Acid:
                            {
                                Tower.TakeDamage(heavyProjectile.Damage);
                                Shield.Transparency = 0.5f;
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

                    if (turret.Active == true &&
                        turret.SelectBox.Contains(new Point(Mouse.GetState().X, Mouse.GetState().Y)) &&
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
                    CursorPosition.Y < (720 - HUDBarTexture.Height) &&
                    turret.Selected == true)
                {
                    Vector2 MousePosition, Direction;
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
                                //MachineShot1.Play();
                               
                                CurrentProfile.ShotsFired++;
                               
                                CurrentProjectile = new MachineGunProjectile(new Vector2(turret.TestVector.X,
                                                                                         turret.TestVector.Y), Direction);
                                LightProjectileList.Add(CurrentProjectile);

                                Emitter FlashEmitter = new Emitter("Particles/FireParticle",
                                        new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
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
                                    (float)RandomDouble(-3, 6), 0.7f, Color.White, Color.White, 0.2f, true,
                                    (float)RandomDouble(608, 640), false, 1, true));
                            }
                            break;
                        #endregion

                        #region Cannon turret
                        case TurretType.Cannon:
                            {
                                //BarrelEnd = new Vector2((float)Math.Cos(turret.Rotation) * (turret.BarrelRectangle.Width - 20),
                                //                        (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height + 20));
                                CannonFire.Play();

                                Emitter FlashEmitter = new Emitter("Particles/FireParticle",
                                    new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                    new Vector2(
                                    MathHelper.ToDegrees(-(float)Math.Atan2(turret.Direction.Y, turret.Direction.X)),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(turret.Direction.Y, turret.Direction.X))),
                                    new Vector2(10, 20), new Vector2(1, 6), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(-2, 2), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.2f, 1, 5,
                                    false, new Vector2(0, 720), true, 1);

                                EmitterList.Add(FlashEmitter);
                                EmitterList[EmitterList.IndexOf(FlashEmitter)].LoadContent(Content);

                                HeavyProjectile = new CannonBall(new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 12, turret.Rotation, 0.2f, turret.Damage, 100,
                                    new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 520, 630), 630));

                                HeavyProjectile.LoadContent(Content);
                                HeavyProjectileList.Add(HeavyProjectile);

                                Vector2 BarrelStart = new Vector2((float)Math.Cos(turret.Rotation) * (45),
                                                                  (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height / 2));

                                ShellCasingList.Add(new Particle(ShellCasing,
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
                                HeavyProjectile = new FlameProjectile(new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 
                                    (float)RandomDouble(7, 9), turret.Rotation, 0.3f, 5, 
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
                                CurrentProjectile = new LightningProjectile(new Vector2(turret.BarrelEnd.X,
                                                                                        turret.BarrelEnd.Y), Direction);
                                LightProjectileList.Add(CurrentProjectile);

                                LightningSound.Play();
                            }
                            break;
                        #endregion

                        #region Cluster turret
                        case TurretType.Cluster:
                            {
                                TimerHeavyProjectile heavyProjectile;

                                Emitter FlashEmitter = new Emitter("Particles/FireParticle",
                                    new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y),
                                    new Vector2(
                                    MathHelper.ToDegrees(-(float)Math.Atan2(turret.Direction.Y, turret.Direction.X)),
                                    MathHelper.ToDegrees(-(float)Math.Atan2(turret.Direction.Y, turret.Direction.X))),
                                    new Vector2(10, 20), new Vector2(1, 6), 0.01f, true, new Vector2(0, 360),
                                    new Vector2(-2, 2), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.2f, 1, 5, false,
                                    new Vector2(0, 720), true);

                                EmitterList.Add(FlashEmitter);
                                EmitterList[EmitterList.IndexOf(FlashEmitter)].LoadContent(Content);

                                heavyProjectile = new ClusterBombShell(1000, new Vector2(turret.BarrelEnd.X, turret.BarrelEnd.Y), 12, turret.Rotation, 0.2f, 5,
                                    new Vector2(MathHelper.Clamp(turret.Position.Y + 32, 520, 630), 630));

                                heavyProjectile.LoadContent(Content);
                                TimedProjectileList.Add(heavyProjectile);

                                ShellCasingList.Add(new Particle(ShellCasing,
                                    new Vector2(turret.BarrelRectangle.X, turret.BarrelRectangle.Y),
                                    turret.Rotation - MathHelper.ToRadians((float)RandomDouble(175, 185)),
                                    (float)RandomDouble(3, 6), 500, 1f, true, (float)RandomDouble(-10, 10),
                                    (float)RandomDouble(-6, 6), 1.5f, Color.White, Color.White, 0.2f, true,
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
                                Emitter ExplosionEmitter = new Emitter("Particles/FireParticle",
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                        new Vector2(0, 180), new Vector2(2, 5), new Vector2(1, 30), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.1f, 1, 5, false,
                                        new Vector2(0, 720), true);

                                EmitterList.Add(ExplosionEmitter);
                                EmitterList[EmitterList.IndexOf(ExplosionEmitter)].LoadContent(Content);

                                Color SmokeColor1 = Color.Lerp(Color.DarkGray, Color.Transparent, 0.25f);
                                Color SmokeColor2 = Color.Lerp(Color.Gray, Color.Transparent, 0.25f);

                                Emitter SmokeEmitter = new Emitter("Particles/Smoke",
                                        new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY-30),
                                        new Vector2(0, 0), new Vector2(1f, 2f), new Vector2(30, 40), 0.01f, true, new Vector2(0, 360),
                                        new Vector2(0, 0), new Vector2(0.8f, 1.2f), SmokeColor1, SmokeColor2, -0.2f, 0.3f, 10, 1, false,
                                        new Vector2(0, 720), false);

                                EmitterList2.Add(SmokeEmitter);
                                EmitterList2[EmitterList2.IndexOf(SmokeEmitter)].LoadContent(Content);

                                Emitter DebrisEmitter = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                new Vector2(70, 110), new Vector2(5, 7), new Vector2(30, 110), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                new Vector2(0.007f, 0.05f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 0.2f, 10, 10, true, new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 8));

                                EmitterList.Add(DebrisEmitter);
                                EmitterList[EmitterList.IndexOf(DebrisEmitter)].LoadContent(Content);

                                Emitter DebrisEmitter2 = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                new Vector2(80, 100), new Vector2(2, 8), new Vector2(80, 150), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                new Vector2(0.01f, 0.02f), Color.Gray, Color.SaddleBrown, 0.2f, 0.3f, 1, 10, true, new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 8));

                                EmitterList.Add(DebrisEmitter2);
                                EmitterList[EmitterList.IndexOf(DebrisEmitter2)].LoadContent(Content);

                                Emitter SparkEmitter = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY-20),
                                new Vector2(0, 360), new Vector2(1, 4), new Vector2(50, 80), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                new Vector2(0.01f, 0.02f), Color.Orange, Color.Yellow, 0.2f, 0.1f, 1, 10, true, new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 8));

                                EmitterList.Add(SparkEmitter);
                                EmitterList[EmitterList.IndexOf(SparkEmitter)].LoadContent(Content);

                                ExplosionList.Add(new Explosion(heavyProjectile.Position, heavyProjectile.BlastRadius, heavyProjectile.Damage));

                                CannonExplosion.Play();

                                if (CameraTime2 > 20)
                                {
                                    Camera.Origin = heavyProjectile.Position;
                                    CameraTime2 = 0;
                                }
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

                                if (CameraTime2 > 20)
                                    CameraTime2 = 0;
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

                            //            Emitter newEmitter = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
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
                        heavyProjectile = new ClusterBomb(position, speed, (float)RandomDouble(angleRange.X, angleRange.Y), 0.3f, 5,
                            new Vector2(MaxY, MaxY));
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

                        Vector2 CollisionEnd;
                        BulletTrail Trail;

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
                                                LightningList.Add(new LightningBolt(turret.BarrelEnd, CollisionEnd, Color.MediumPurple));
                                                LightningList[i].LoadContent(Content);
                                            }
                                            break;

                                        case LightProjectileType.MachineGun:
                                            Trail = new BulletTrail(turret.BarrelEnd, CollisionEnd);
                                            Trail.LoadContent(Content);
                                            TrailList.Add(Trail);
                                            break;
                                    }
                                    break;

                                case InvaderType.Soldier:
                                    Splat1.Play();
                                    GroundImpact.Play();

                                    CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)DistToInvader),
                                                              turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToInvader));

                                    switch (CurrentProjectile.LightProjectileType)
                                    {
                                        case LightProjectileType.Lightning:
                                            for (int i = 0; i < 5; i++)
                                            {
                                                LightningList.Add(new LightningBolt(turret.BarrelEnd, CollisionEnd, Color.MediumPurple));
                                                LightningList[i].LoadContent(Content);
                                            }
                                            break;

                                        case LightProjectileType.MachineGun:
                                            Trail = new BulletTrail(turret.BarrelEnd, CollisionEnd);
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
                                    CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)DistToInvader),
                                                               turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToInvader));

                                    switch (CurrentProjectile.LightProjectileType)
                                    {
                                        case LightProjectileType.Lightning:
                                            for (int i = 0; i < 5; i++)
                                            {
                                                LightningList.Add(new LightningBolt(turret.BarrelEnd, CollisionEnd, Color.MediumPurple));
                                                LightningList[i].LoadContent(Content);
                                            }
                                            break;

                                        case LightProjectileType.MachineGun:
                                            Trail = new BulletTrail(turret.BarrelEnd, CollisionEnd);
                                            Trail.LoadContent(Content);
                                            TrailList.Add(Trail);

                                            switch (HitInvader.InvaderType)
                                            {
                                                case InvaderType.Soldier:
                                                    Splat1.Play();
                                                    GroundImpact.Play();

                                                    EmitterList.Add(new Emitter("Particles/Splodge", new Vector2(HitInvader.DestinationRectangle.Center.X, CollisionEnd.Y),
                                                    new Vector2(MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) - (float)RandomDouble(0, 45),
                                                    MathHelper.ToDegrees(-(float)Math.Atan2(CurrentProjectile.Ray.Direction.Y, CurrentProjectile.Ray.Direction.X)) + (float)RandomDouble(0, 45)),
                                                    new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360),
                                                    new Vector2(1, 3), new Vector2(0.01f, 0.03f), Color.DarkRed, Color.Red,
                                                    0.1f, 0.1f, 10, 5, true, new Vector2(HitInvader.MaxY, HitInvader.MaxY), false, 1, true, false));

                                                    EmitterList[EmitterList.Count - 1].LoadContent(Content);
                                                    break;
                                            }
                                            break;
                                    }                                  
                                                                      
                                    break;

                                case TrapType.Wall:
                                    CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)DistToTrap),
                                                               turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToTrap));

                                    if (CurrentProjectile.LightProjectileType == LightProjectileType.MachineGun)
                                    {
                                        Trail = new BulletTrail(turret.BarrelEnd, CollisionEnd);
                                        Trail.LoadContent(Content);
                                        TrailList.Add(Trail);
                                    }

                                    if (CurrentProjectile.LightProjectileType == LightProjectileType.Lightning)
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            LightningList.Add(new LightningBolt(turret.BarrelEnd, CollisionEnd,
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
                                        Trail = new BulletTrail(turret.BarrelEnd, CollisionEnd);
                                        Trail.LoadContent(Content);
                                        TrailList.Add(Trail);
                                    }

                                    if (CurrentProjectile.LightProjectileType == LightProjectileType.Lightning)
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            LightningList.Add(new LightningBolt(turret.BarrelEnd, CollisionEnd,
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

                        Vector2 CollisionEnd;
                        BulletTrail Trail;
                      
                        switch (HitTrap.TrapType)
                        {
                            default:
                                //CurrentProjectile = null;
                                //This ended up as null before defining CollisionEnd... Broke game. Not sure why. Needs to be fixed.

                                //OK, so I think I've fixed it now. Keep an eye out for it though.
                                //Bug happened when shooting through a trap and then the projectile didn't hit the ground
                                //because the turret was placed in the lowest slot.
                                var DistToGround = CurrentProjectile.Ray.Intersects(Ground.BoundingBox);

                                if (DistToGround == null)
                                    DistToGround = 1280;

                                CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)DistToGround),
                                                           turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToGround));

                                if (CurrentProjectile.LightProjectileType == LightProjectileType.MachineGun)
                                {
                                    GroundImpact.Play();

                                    if (Random.NextDouble() > 0.92)
                                    {
                                        Double test;
                                        test = Random.NextDouble();

                                        if (test < .33)
                                        {
                                            Ricochet1.Play();
                                        }
                                        else
                                            if (test > .33 && test < .66)
                                            {
                                                Ricochet2.Play();
                                            }
                                            else
                                                if (test > 66)
                                                {
                                                    Ricochet3.Play();
                                                }
                                    }

                                    Trail = new BulletTrail(turret.BarrelEnd, CollisionEnd);
                                    Trail.LoadContent(Content);
                                    TrailList.Add(Trail);

                                    Emitter DebrisEmitter = new Emitter("Particles/Splodge", new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                        new Vector2(60, 120), new Vector2(2, 4), new Vector2(20, 40), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                        new Vector2(0.01f, 0.03f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 0.1f, 2, 2, true, new Vector2(CollisionEnd.Y + 8, CollisionEnd.Y + 8));
                                    EmitterList.Add(DebrisEmitter);
                                    EmitterList[EmitterList.IndexOf(DebrisEmitter)].LoadContent(Content);

                                    Emitter SmokeEmitter = new Emitter("Particles/Smoke", new Vector2(CollisionEnd.X, CollisionEnd.Y - 4),
                                        new Vector2(90, 90), new Vector2(0.5f, 1f), new Vector2(20, 30), 1f, true, new Vector2(0, 0),
                                        new Vector2(-2, 2), new Vector2(0.5f, 1f), DirtColor, DirtColor2, 0f, 0.02f, 10, 1, false, new Vector2(0, 720), false);
                                    EmitterList.Add(SmokeEmitter);
                                    EmitterList[EmitterList.IndexOf(SmokeEmitter)].LoadContent(Content);

                                    Emitter SparkEmitter = new Emitter("Particles/GlowBall", new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                        new Vector2(0, 0), new Vector2(0, 0), new Vector2(2, 5), 1f, true, new Vector2(0, 0),
                                        new Vector2(0, 0), new Vector2(0.25f, 0.25f), FireColor, FireColor2, 0f, 0.1f, 500, 1,
                                        false, new Vector2(0, 720));
                                    AlphaEmitterList.Add(SparkEmitter);
                                    AlphaEmitterList[AlphaEmitterList.IndexOf(SparkEmitter)].LoadContent(Content);
                                
                                    }

                                if (CurrentProjectile.LightProjectileType == LightProjectileType.Lightning)
                                {
                                    for (int i = 0; i < 5; i++)
                                    {
                                        LightningList.Add(new LightningBolt(turret.BarrelEnd, CollisionEnd,
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
                                        Trail = new BulletTrail(turret.BarrelEnd, CollisionEnd);
                                        Trail.LoadContent(Content);
                                        TrailList.Add(Trail);
                                    }

                                if (CurrentProjectile.LightProjectileType == LightProjectileType.Lightning)
                                {
                                    for (int i = 0; i < 5; i++)
                                    {
                                        LightningList.Add(new LightningBolt(turret.BarrelEnd, CollisionEnd,
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
                                        Trail = new BulletTrail(turret.BarrelEnd, CollisionEnd);
                                        Trail.LoadContent(Content);
                                        TrailList.Add(Trail);
                                    }

                                    if (CurrentProjectile.LightProjectileType == LightProjectileType.Lightning)
                                    {
                                        for (int i = 0; i < 5; i++)
                                        {
                                            LightningList.Add(new LightningBolt(turret.BarrelEnd, CollisionEnd,
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

                        Vector2 CollisionEnd;
                        BulletTrail Trail;
                     
                        CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * (float)DistToInvader),
                                                   turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToInvader));

                        if (CurrentProjectile.LightProjectileType == LightProjectileType.MachineGun)
                        {
                            Trail = new BulletTrail(turret.BarrelEnd, CollisionEnd);
                            Trail.LoadContent(Content);
                            TrailList.Add(Trail);
                        }

                        if (CurrentProjectile.LightProjectileType == LightProjectileType.Lightning)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                LightningList.Add(new LightningBolt(turret.BarrelEnd, CollisionEnd,
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
                                Splat1.Play();
                                GroundImpact.Play();
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

                        Vector2 CollisionEnd;
                        BulletTrail Trail;

                        if (DistToGround != null)
                        {
                            if (turret.CanShoot == false &&
                                CurrentMouseState.LeftButton == ButtonState.Pressed)
                            {
                                CollisionEnd = new Vector2(turret.TestVector.X + (CurrentProjectile.Ray.Direction.X * (float)DistToGround),
                                                           turret.TestVector.Y + (CurrentProjectile.Ray.Direction.Y * (float)DistToGround));

                                switch (turret.TurretType)
                                {
                                    case TurretType.MachineGun:
                                        GroundImpact.Play();
                                        if (CurrentProjectile.LightProjectileType == LightProjectileType.MachineGun)
                                        {
                                            if (Random.NextDouble() > 0.92)
                                            {
                                                Double test;
                                                test = Random.NextDouble();

                                                if (test < .33)
                                                {
                                                    Ricochet1.Play();                                                    
                                                }
                                                else
                                                if (test > .33 && test < .66)
                                                {
                                                    Ricochet2.Play();
                                                }
                                                else
                                                if (test > 66)
                                                {
                                                    Ricochet3.Play();
                                                }
                                            }

                                            Trail = new BulletTrail(turret.BarrelEnd, CollisionEnd);
                                            Trail.LoadContent(Content);
                                            TrailList.Add(Trail);
                                        }

                                        Emitter DebrisEmitter = new Emitter("Particles/Splodge", new Vector2(CollisionEnd.X, CollisionEnd.Y),
                                            new Vector2(60, 120), new Vector2(2, 4), new Vector2(20, 40), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                            new Vector2(0.01f, 0.03f), Color.DarkSlateGray, Color.SaddleBrown, 0.2f, 0.1f, 2, 2, true, new Vector2(CollisionEnd.Y + 8, CollisionEnd.Y + 8));
                                        EmitterList.Add(DebrisEmitter);
                                        EmitterList[EmitterList.IndexOf(DebrisEmitter)].LoadContent(Content);

                                        Emitter SmokeEmitter = new Emitter("Particles/Smoke", new Vector2(CollisionEnd.X, CollisionEnd.Y-4),
                                            new Vector2(90, 90), new Vector2(0.5f, 1f), new Vector2(20, 30), 1f, true, new Vector2(0, 0),
                                            new Vector2(-2, 2), new Vector2(0.5f, 1f), DirtColor, DirtColor2, 0f, 0.02f, 10, 1, false, new Vector2(0, 720), false);
                                        EmitterList.Add(SmokeEmitter);
                                        EmitterList[EmitterList.IndexOf(SmokeEmitter)].LoadContent(Content);

                                        Emitter SparkEmitter = new Emitter("Particles/GlowBall", new Vector2(CollisionEnd.X, CollisionEnd.Y), 
                                            new Vector2(0, 0), new Vector2(0, 0), new Vector2(2, 5), 1f, true, new Vector2(0, 0), 
                                            new Vector2(0, 0), new Vector2(0.25f, 0.25f), FireColor, FireColor2, 0f, 0.1f, 500, 1, 
                                            false, new Vector2(0, 720));
                                        AlphaEmitterList.Add(SparkEmitter);
                                        AlphaEmitterList[AlphaEmitterList.IndexOf(SparkEmitter)].LoadContent(Content);
                                        break;

                                    case TurretType.Lightning:                                        
                                        if (CurrentProjectile.LightProjectileType == LightProjectileType.Lightning)
                                        {
                                            for (int i = 0; i < 5; i++)
                                            {
                                                LightningList.Add(new LightningBolt(turret.BarrelEnd, CollisionEnd,
                                                                                    Color.MediumPurple));
                                                LightningList[i].LoadContent(Content);
                                            }
                                        }

                                        DirtEmitter = new Emitter("Particles/Smoke", new Vector2(CollisionEnd.X, CollisionEnd.Y),
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
                        Vector2 CollisionEnd;
                        BulletTrail Trail;

                        Vector2 TestVector = new Vector2(turret.BarrelRectangle.X + (float)Math.Cos(turret.Rotation - 90) * (turret.BarrelPivot.Y - turret.BarrelRectangle.Height / 2),
                                                         turret.BarrelRectangle.Y + (float)Math.Sin(turret.Rotation - 90) * (turret.BarrelPivot.Y - turret.BarrelRectangle.Height / 2));

                        CollisionEnd = new Vector2(turret.BarrelRectangle.X + (CurrentProjectile.Ray.Direction.X * 1280),
                                                   turret.BarrelRectangle.Y + (CurrentProjectile.Ray.Direction.Y * 1280));

                        if (CurrentProjectile.LightProjectileType == LightProjectileType.MachineGun)
                        {
                            MachineShot1.Play();
                            Trail = new BulletTrail(turret.BarrelEnd, CollisionEnd);
                            Trail.LoadContent(Content);
                            TrailList.Add(Trail);
                        }

                        if (CurrentProjectile.LightProjectileType == LightProjectileType.Lightning)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                LightningList.Add(new LightningBolt(turret.BarrelEnd, CollisionEnd,
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
                            PlaceTrap.Play();
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

                            Emitter SmokeEmitter = new Emitter("Particles/Smoke", new Vector2(NewTrap.Position.X + 16, NewTrap.Position.Y - 4),
                                new Vector2(70, 110), new Vector2(0.2f, 0.5f), new Vector2(250, 350), 1f, true, new Vector2(-20, 20),
                                new Vector2(-4, 4), new Vector2(0.5f, 0.5f), SmokeColor, SmokeColor2, 0.0f, -1, 300, 1, false,
                                new Vector2(0, 720), false, CursorPosition.Y / 720);

                            SmokeEmitter.LoadContent(Content);
                            NewTrap.TrapEmitterList.Add(SmokeEmitter);
                                                        
                            FireEmitter.LoadContent(Content);
                            NewTrap.TrapEmitterList.Add(FireEmitter);

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
                            PlaceTrap.Play();
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

                #region Remove trap if middle-clicked
                if (trap.DestinationRectangle.Contains(new Point(Mouse.GetState().X, Mouse.GetState().Y)) &&
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
                        HitTrap.CurrentDetonateDelay = 0;

                        if (HitTrap.CurrentDetonateLimit > 0)
                            HitTrap.CurrentDetonateLimit -= 1;

                        switch (HitTrap.TrapType)
                        {
                            #region Fire Trap
                            case TrapType.Fire:                                
                            switch (HitTrap.CurrentDetonateLimit)
                                {
                                    case 4:
                                        HitTrap.TrapEmitterList[1].EndColor =
                                            Color.Lerp(HitTrap.TrapEmitterList[1].EndColor, new Color(255, 0, 0, 255), 0.25f);
                                        break;

                                    case 3:
                                        HitTrap.TrapEmitterList[1].EndColor =
                                            Color.Lerp(HitTrap.TrapEmitterList[1].EndColor, new Color(255, 0, 0, 255), 0.25f);
                                        break;

                                    case 2:
                                        HitTrap.TrapEmitterList[1].EndColor =
                                            Color.Lerp(HitTrap.TrapEmitterList[1].EndColor, new Color(255, 0, 0, 255), 0.25f);
                                        break;

                                    case 1:
                                        HitTrap.TrapEmitterList[1].EndColor =
                                            Color.Lerp(HitTrap.TrapEmitterList[1].EndColor, new Color(255, 0, 0, 255), 0.25f);
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

                    foreach (Button button in PlaceWeaponList)
                    {
                        button.IconName = null;
                        button.LoadContent(SecondaryContent);
                    }

                    for (int i = 0; i < CurrentProfile.Buttons.Count; i++)
                    {
                        if (CurrentProfile.Buttons[i] != null)
                        {
                            PlaceWeaponList[i].IconName = "Icons/" + CurrentProfile.Buttons[i] + "Icon";
                            PlaceWeaponList[i].LoadContent(SecondaryContent);
                        }
                    }

                    foreach (Button button in SelectWeaponList)
                    {
                        button.IconName = "Icons/LockIcon";
                        button.LoadContent(SecondaryContent);
                    }
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

                foreach (Button button in PlaceWeaponList)
                {
                    button.IconName = null;
                    button.LoadContent(SecondaryContent);
                }

                foreach (Button button in SelectWeaponList)
                {
                    button.IconName = "Icons/LockIcon";
                    button.LoadContent(SecondaryContent);
                }

                #region Change the icon depending if the player has access to that weapon
                for (int i = 0; i < SelectWeaponList.Count; i++)
                {
                    if (SelectWeaponList[i].IconName == "Icons/LockIcon")
                        switch (i)
                        {
                            case 0:
                                if (CurrentProfile.MachineGunTurret == true)
                                {
                                    SelectWeaponList[i].IconName = "Icons/MachineGunTurretIcon";
                                    SelectWeaponList[i].LoadContent(SecondaryContent);
                                }
                                break;

                            case 1:
                                if (CurrentProfile.LightningTurret == true)
                                {
                                    SelectWeaponList[i].IconName = "Icons/MachineGunTurretIcon";
                                    SelectWeaponList[i].LoadContent(SecondaryContent);
                                }
                                break;

                            case 2:
                                if (CurrentProfile.FireTrap == true)
                                {
                                    SelectWeaponList[i].IconName = "Icons/FireTrapIcon";
                                    SelectWeaponList[i].LoadContent(SecondaryContent);
                                }
                                break;
                        }
                }
                #endregion

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

            CurrentProfile = new Profile()
            {
                Name = NameInput.RealString,
                LevelNumber = 1,

                Points = 10,

                CannonTurret = false,
                MachineGunTurret = true,
                CatapultTrap = false,
                FlameThrowerTurret = false,
                ClusterTurret = false,

                FireTrap = false,
                SpikesTrap = false,
                LightningTurret = false,
                SawBladeTrap = false,
                WallTrap = false,
                IceTrap = false,

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
            if (GameState == GameState.Playing && IsLoading == false)
            {
                CurrentInvaderTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                
                if (CurrentInvaderIndex > CurrentWave.InvaderList.Count -1 && InvaderList.Count == 0)
                CurrentWaveTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (CurrentInvaderTime >= CurrentWave.InvaderTime)
                {
                    if (CurrentInvaderIndex < CurrentWave.InvaderList.Count)
                    {
                        Invader invader = CurrentWave.InvaderList[CurrentInvaderIndex];
                        invader.LoadContent(Content);
                        InvaderList.Add(invader);
                        CurrentWave.InvaderList[CurrentInvaderIndex] = null;
                        CurrentInvaderIndex++;
                    }

                    CurrentInvaderTime = 0;
                }

                if (CurrentWaveTime > CurrentWave.WaveTime &&
                    CurrentWaveIndex < CurrentLevel.WaveList.Count - 1)
                {
                    CurrentWaveIndex++;
                    CurrentWaveNumber++;
                    CurrentWaveTime = 0;

                    if (CurrentWaveIndex < CurrentLevel.WaveList.Count)
                    {
                        CurrentInvaderIndex = 0;
                        CurrentWave = CurrentLevel.WaveList[CurrentWaveIndex];
                    }
                }

                //Victory condition
                if (CurrentWaveNumber == CurrentLevel.WaveList.Count &&
                    InvaderList.Count == 0 &&
                    CurrentLevel.WaveList.All(Wave => Wave.InvaderList.All(Invader => Invader == null)))
                {
                    VictoryTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    Victory = true;

                    if (VictoryTime >= 5000)
                    {
                        IsLoading = false;
                        CurrentProfile.LevelNumber++;
                        StorageDevice.BeginShowSelector(this.SaveProfile, null);
                        ResetUpgrades();
                        UnloadGameContent();
                        MaxWaves = CurrentLevel.WaveList.Count;
                        CurrentWaveNumber = 1;
                        CurrentWaveIndex = 0;
                        GameState = GameState.ProfileManagement;
                    }
                }
            }
        }

        //Handle levels
        public void LoadLevel(int number)
        {
            Victory = false;
            CurrentWaveNumber = 0;
            //CurrentLevel = Content.Load<Level>("Levels/Level" + number);
            switch (LevelNumber)
            {
                case 1:
                    CurrentLevel = new Level1();
                    CurrentWaveIndex = 0;
                    CurrentInvaderIndex = 0;
                    CurrentWave = CurrentLevel.WaveList[0];
                    break;

                case 2:
                    CurrentLevel = new Level2();
                    CurrentWaveIndex = 0;
                    CurrentInvaderIndex = 0;
                    CurrentWave = CurrentLevel.WaveList[0];
                    break;

                case 3:
                    CurrentLevel = new Level3();
                    CurrentWaveIndex = 0;
                    CurrentInvaderIndex = 0;
                    CurrentWave = CurrentLevel.WaveList[0];
                    break;

                case 4:
                    CurrentLevel = new Level4();
                    CurrentWaveIndex = 0;
                    CurrentInvaderIndex = 0;
                    CurrentWave = CurrentLevel.WaveList[0];
                    break;

                case 5:
                    CurrentLevel = new Level5();
                    CurrentWaveIndex = 0;
                    CurrentInvaderIndex = 0;
                    CurrentWave = CurrentLevel.WaveList[0];
                    break;

                case 6:
                    CurrentLevel = new Level6();
                    CurrentWaveIndex = 0;
                    CurrentInvaderIndex = 0;
                    CurrentWave = CurrentLevel.WaveList[0];
                    break;

                case 7:
                    CurrentLevel = new Level7();
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
                        {
                            CurrentCursorTexture = BlankTexture;
                            BlastRadiusVisible = false;
                        }
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

                    case TrapType.Barrel:
                        CurrentCursorTexture = BombCursor;
                        BlastRadiusVisible = true;
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
