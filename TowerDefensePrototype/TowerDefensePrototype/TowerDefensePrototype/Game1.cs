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
using GameDataTypes;

namespace TowerDefensePrototype
{
    public enum TrapType { Blank, Wall, Spikes, Catapult, Fire, Ice, Tar, Barrel, SawBlade };
    public enum TurretType { Blank, Gatling, Cannon, FlameThrower };
    public enum InvaderType { Soldier, BatteringRam, Airship, Archer, Tank, Spider, Slime, SuicideBomber };
    public enum HeavyProjectileType { CannonBall, FlameThrower, Arrow, Acid, Torpedo };
    public enum LightProjectileType { MachineGun, Freeze };
    public enum GameState { Menu, Loading, Playing, Paused, ProfileSelect, Options, ProfileManagement, Tutorial, LoadingGame };
    public enum WorldType { Normal, Ice, Lava };

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Variable declarations
        GraphicsDeviceManager graphics;
        ContentManager SecondaryContent;
        SpriteBatch spriteBatch, spriteBatch2, spriteBatch3, spriteBatch4;

        //XNA Declarations
        Texture2D BlankTexture, HUDBarTexture, PrimaryCursorTexture, 
            ShellCasing, LoadingScreenBackground, PauseMenuBackground, CurrentCursorTexture, 
            DefaultCursor, CrosshairCursor, FireCursor, WallCursor, SpikesCursor, 
            BasicTurretCursor, CannonTurretCursor, FlameThrowerCursor, HealthBarSprite, ShieldBarSprite;
        Vector2 CursorPosition, GroundCollisionPoint;
        Rectangle ScreenRectangle;
        SpriteFont ResourceFont;
        MouseState CurrentMouseState, PreviousMouseState;
        KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        int Resources, SelectedTurretIndex, TrapLimit, TowerButtons, ProfileNumber, WaveListIndex;
        string SelectButtonAssetName, TowerSlotAssetName, FileName, ContainerName, ProfileName;
        bool ReadyToPlace, slow, IsLoading;
        double CurrentWaveDelay;

        int GatlingSpeed = 0;
        int CannonSpeed = 0;

        #region List declarations
        List<Button> SelectButtonList, TowerButtonList, MainMenuButtonList, PauseButtonList, 
                     ProfileButtonList, ProfileDeleteList, UpgradesButtonList;

        List<Trap> TrapList;
        List<Turret> TurretList;
        List<Invader> InvaderList;
            
        List<HeavyProjectile> HeavyProjectileList, InvaderHeavyProjectileList;
        List<LightProjectile> LightProjectileList;
        
        List<Emitter> EmitterList, EmitterList2;
        List<Particle> ShellCasingList;

        List<string> MainMenuNameList, PauseMenuNameList, IconNameList;

        List<Explosion> ExplosionList, EnemyExplosionList;

        List<Upgrade> UpgradesList;

        List<Wave> WaveList;
        #endregion

        #region Custom class declarations
        Button ProfileBackButton, ProfileManagementPlay, ProfileManagementBack, OptionsBack, OptionsSave;
        Tower Tower;
        StaticSprite Ground, TestBackground, ProfileMenuTitle;
        AnimatedSprite LoadingAnimation;
        TrapType SelectedTrap;
        TurretType SelectedTurret;
        HorizontalBar TowerHealthBar;
        LightProjectile CurrentProjectile;
        Emitter DirtEmitter;      
        GameState GameState;
        Thread LoadingThread;
        Profile CurrentProfile;
        StorageDevice Device;
        Stream OpenFile;
        SpriteFont ButtonFont, HUDFont;
        World CurrentWorld;
        Level CurrentLevel;
        Settings CurrentSettings, DefaultSettings;          
        Wave CurrentWave;          
        Effect HealthBarEffect;
        Random Random;
        #endregion        

        //Sound effects
        //SoundEffect MachineGunLoop, Explosion1, Explosion2, Explosion3;
        //SoundEffect ButtonClick;
        #endregion


        #region Main Game Functions
        public Game1()
        {
            DefaultSettings = new Settings
            {
                FullScreen = false
            };

            LoadSettings();

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;           
            graphics.IsFullScreen = CurrentSettings.FullScreen;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Random rand = new Random();

            SecondaryContent = new ContentManager(Content.ServiceProvider, Content.RootDirectory);            
            GameState = GameState.Menu;
            ContainerName = "Profiles";
            TrapLimit = 8;
            TowerButtons = 3;

            MainMenuNameList = new List<string>();
            MainMenuNameList.Add("Play");
            MainMenuNameList.Add("Options");
            MainMenuNameList.Add("Exit");

            MainMenuButtonList = new List<Button>();
            MainMenuButtonList.Add(new Button("Buttons/ButtonLeft", new Vector2(0, 130), null, null, null, "PLAY", "Fonts/ButtonFont", "Left", Color.White));
            MainMenuButtonList.Add(new Button("Buttons/ButtonLeft", new Vector2(0, 130 + ((64 + 50) * 1)), null, null, null, "TUTORIAL", "Fonts/ButtonFont", "Left", Color.White));
            MainMenuButtonList.Add(new Button("Buttons/ButtonLeft", new Vector2(0, 130 + ((64 + 50) * 2)), null, null, null, "OPTIONS", "Fonts/ButtonFont", "Left", Color.White));
            MainMenuButtonList.Add(new Button("Buttons/ButtonLeft", new Vector2(0, 130 + ((64 + 50) * 3)), null, null, null, "CREDITS", "Fonts/ButtonFont", "Left", Color.White));
            MainMenuButtonList.Add(new Button("Buttons/ButtonLeft", new Vector2(0, 720-50-32), null, null, null, "EXIT", "Fonts/ButtonFont", "Left", Color.White));

            foreach (Button button in MainMenuButtonList)
            {
                button.LoadContent(SecondaryContent);
            }

            ProfileButtonList = new List<Button>();
            for (int i = 0; i < 6; i++)
            {
                ProfileButtonList.Add(new Button("Buttons/ButtonLeft", new Vector2(50, 72 + (i * 100)), null, null, null, "empty", "Fonts/ButtonFont", "Centre", Color.White));
                ProfileButtonList[i].LoadContent(SecondaryContent);
            }

            ProfileDeleteList = new List<Button>();
            for (int i = 0; i < 6; i++)
            {
                ProfileDeleteList.Add(new Button("Buttons/SmallButton", new Vector2(0, 72 + (i * 100)), null, null, null, "X", "Fonts/ButtonFont", "Left", Color.White));                
                ProfileDeleteList[i].LoadContent(SecondaryContent);
            }

            UpgradesButtonList = new List<Button>();
            for (int i = 0; i < 6; i++)
            {
                UpgradesButtonList.Add(new Button("Buttons/NewButton", new Vector2(100 + 64 * i, 100)));
                UpgradesButtonList[i].LoadContent(SecondaryContent);
            }

            ProfileBackButton = new Button("Buttons/ButtonRight", new Vector2(1280 - 450, 720 - 32 - 50), null, null, null, "Back", "Fonts/ButtonFont", "Right", Color.White);
            ProfileBackButton.LoadContent(SecondaryContent);

            ProfileManagementPlay = new Button("Buttons/ButtonRight", new Vector2(1280 - 450, 720 - 32 - 50), null, null, null, "Play", "Fonts/ButtonFont", "Right", Color.White);
            ProfileManagementPlay.LoadContent(SecondaryContent);

            ProfileManagementBack = new Button("Buttons/ButtonLeft", new Vector2(0, 720 - 32 - 50), null, null, null, "Back", "Fonts/ButtonFont", "Left", Color.White);
            ProfileManagementBack.LoadContent(SecondaryContent);

            OptionsBack = new Button("Buttons/ButtonLeft", new Vector2(0, 720 - 32 - 50), null, null, null, "Back", "Fonts/ButtonFont", "Left", Color.White);
            OptionsBack.LoadContent(SecondaryContent);

            OptionsSave = new Button("Buttons/ButtonRight", new Vector2(1280 - 450, 720 - 32 - 50), null, null, null, "Apply", "Fonts/ButtonFont", "Right", Color.White);
            OptionsSave.LoadContent(SecondaryContent);

            ProfileMenuTitle = new StaticSprite("ProfileMenuTitle", new Vector2(0, 32));
            ProfileMenuTitle.LoadContent(SecondaryContent);

            ScreenRectangle = new Rectangle(-128, -128, 1408, 848);

            LoadingAnimation = new AnimatedSprite("LoadingAnimation2", new Vector2(640-65, 320-65), new Vector2(131, 131), 17, 30, Color.White, Vector2.One, true);
            LoadingAnimation.LoadContent(SecondaryContent);

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

            if (gameTime.IsRunningSlowly == true)
            {
                slow = true;
            }


            if (GameState == GameState.Playing && IsLoading == false)
            {               
                //if (this.IsActive == false && GameState != GameState.Paused)
                //    GameState = GameState.Paused;

                if (CurrentKeyboardState.IsKeyUp(Keys.Escape) && PreviousKeyboardState.IsKeyDown(Keys.Escape))
                {
                    GameState = GameState.Paused;
                }

                #region Handle Particle Emitters and Particles
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
                #endregion

                InvaderUpdate(gameTime);

                RightClickClearSelected();

                SelectButtonsUpdate();

                TowerButtonUpdate();

                TrapPlacement();

                TrapUpdate(gameTime);

                HeavyProjectileUpdate(gameTime);
                LightProjectileUpdate();

                InvaderProjectileUpdate(gameTime);
                UpdateWave(gameTime);

                TurretUpdate();

                TrapCollision();

                AttackTower();
                AttackTraps();

                RangedAttackTower();
                RangedAttackTraps();

                EnemyExplosionsUpdate();

                TowerHealthBar.Update(new Vector2(90, 19), (int)Tower.CurrentHP);

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

                for (int i = 0; i < ShellCasingList.Count; i++)
                {
                    if (ShellCasingList[i].Active == false)
                        ShellCasingList.RemoveAt(i);
                }

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


                foreach (Turret turret in TurretList)
                {
                    if (turret.Active == true)
                        if (turret.Selected == true && CurrentMouseState.LeftButton == ButtonState.Pressed
                            && PreviousMouseState.LeftButton == ButtonState.Pressed && CurrentMouseState.Y < 720-64)
                        {
                            if (turret.CanShoot == true)
                            {
                                turret.ElapsedTime = 0;
                                TurretShoot();
                            }
                        }
                }                

                foreach (Turret turret in TurretList)
                {
                    if (turret.Active == true)
                        turret.Update(gameTime);
                }

                if (gameTime.IsRunningSlowly == true)
                {
                    slow = true;
                }
                else
                {
                    slow = false;
                }

                for (int i = 0; i < InvaderList.Count; i++)
                {
                    if (InvaderList[i].Active == false)
                        InvaderList.RemoveAt(i);
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
            #region Spritebatch
            spriteBatch.Begin();
            
            Color backgroundColor = new Color(22, 60, 90);
            GraphicsDevice.Clear(backgroundColor);

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
                spriteBatch.DrawString(ButtonFont, CurrentProfile.Name, new Vector2(16, 37), Color.White);
                //spriteBatch.DrawString(ProfileFont, CurrentProfile.Name, new Vector2(32, 16), Color.White);
                //spriteBatch.DrawString(ProfileFont, "Level: " + CurrentProfile.LevelNumber.ToString(), new Vector2(500, 16), Color.White);
                //spriteBatch.DrawString(ProfileFont, "Points: " + CurrentProfile.Points.ToString(), new Vector2(32, 128), Color.White);
                foreach (Button button in UpgradesButtonList)
                {
                    button.Draw(spriteBatch);
                }
                ProfileManagementPlay.Draw(spriteBatch);
                ProfileManagementBack.Draw(spriteBatch);
            }
            #endregion

            #region Draw Game
            if (GameState == GameState.Playing || GameState == GameState.Paused && IsLoading == false)
            {
                TestBackground.Draw(spriteBatch);                
                Ground.Draw(spriteBatch);
                Tower.Draw(spriteBatch);                

                #region Draw Traps, Invaders and Turrets
                foreach (Invader invader in InvaderList)
                {
                    invader.Draw(spriteBatch);
                }                

                foreach (Turret turret in TurretList)
                {
                    if (turret.Active == true)
                        turret.Draw(spriteBatch);
                }                

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

                //This draws the timing bar for the turrets, but also makes 
                //sure that it doesn't draw for the blank turrets, which
                //would make the timing bar appear in the top right corner
                foreach (Turret turret in TurretList)
                {
                    if (turret.TurretType != TurretType.Blank)
                        turret.TimingBar.Draw(spriteBatch);
                }

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

                foreach (Particle shellCasing in ShellCasingList)
                {
                    shellCasing.Draw(spriteBatch);
                }


                spriteBatch.DrawString(HUDFont, HeavyProjectileList.Count.ToString(), new Vector2(10, 720-30), Color.White);


                int PercentageHP;
                PercentageHP = (int)MathHelper.Clamp((float)((100d / (double)Tower.MaxHP) * (double)Tower.CurrentHP), 0, 100);

                int PercentageShield = (int)MathHelper.Clamp((float)((100d / (double)Tower.MaxShield) * (double)Tower.CurrentShield), 0, 100);
                Vector2 HPSize, ShieldSize;
                HPSize = HUDFont.MeasureString(PercentageHP.ToString());
                ShieldSize = HUDFont.MeasureString(PercentageShield.ToString());
                
                spriteBatch.DrawString(HUDFont, PercentageHP.ToString(), new Vector2(80-(HPSize.X/2),64), Color.White);
                spriteBatch.DrawString(HUDFont, PercentageShield.ToString(), new Vector2(80 - (ShieldSize.X / 2), 80), Color.White);

            }
            #endregion

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

            #region Draw Loading Screen
            if (GameState == GameState.Loading)
            {
                LoadingAnimation.Draw(spriteBatch);
                //spriteBatch.Draw(LoadingScreenBackground, new Rectangle(0, 0, 1280, 720), Color.White);
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
                OptionsBack.Draw(spriteBatch);
                OptionsSave.Draw(spriteBatch);
            }
            #endregion

            spriteBatch.End();
            #endregion

            double HealthValue;

            #region SpriteBatch3
            if (GameState == GameState.Playing || GameState == GameState.Paused && IsLoading == false)
            {
                HealthValue = (double)Tower.CurrentHP / (double)Tower.MaxHP;
                //HealthValue = 0.5;

                HealthBarEffect.Parameters["meterValue"].SetValue((float)HealthValue);

                spriteBatch3.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.LinearWrap,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, HealthBarEffect);

                spriteBatch3.Draw(HealthBarSprite, new Vector2(80, 80), null, Color.White, MathHelper.ToRadians(-90), new Vector2(HealthBarSprite.Width / 2, HealthBarSprite.Height / 2), 1f, SpriteEffects.None, 1);

                spriteBatch3.End();
            }
            #endregion

            double ShieldValue;

            #region SpriteBatch4
            if (GameState == GameState.Playing || GameState == GameState.Paused && IsLoading == false)
            {
                ShieldValue = (double)Tower.CurrentShield / (double)Tower.MaxShield;
                //ShieldValue = 0.5;

                HealthBarEffect.Parameters["meterValue"].SetValue((float)ShieldValue);

                spriteBatch4.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.LinearWrap,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, HealthBarEffect);
         
                spriteBatch4.Draw(ShieldBarSprite, new Vector2(80, 80), null, Color.White, MathHelper.ToRadians(90), new Vector2(HealthBarSprite.Width / 2, HealthBarSprite.Height / 2), 1f, SpriteEffects.None, 1);

                spriteBatch4.End();
            }
            #endregion

            #region Spritebatch2
            //This second spritebatch sorts everthing Back to Front, 
            //to make sure that the invaders are drawn correctly according to their Y value
            spriteBatch2.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

            if (GameState == GameState.Playing && IsLoading == false)
            {
                foreach (Invader invader in InvaderList)
                {
                    invader.Draw(spriteBatch2);
                }

                foreach (Emitter emitter in EmitterList)
                {
                    emitter.Draw(spriteBatch2);
                }

                foreach (Trap trap in TrapList)
                {
                    trap.Draw(spriteBatch2);
                }

                foreach (HeavyProjectile heavyProjectile in HeavyProjectileList)
                {
                    heavyProjectile.Draw(spriteBatch2);
                }

                foreach (HeavyProjectile heavyProjectile in InvaderHeavyProjectileList)
                {
                    heavyProjectile.Draw(spriteBatch2);
                }

                CursorDraw(spriteBatch2);
            }

            CursorDraw(spriteBatch2);

            spriteBatch2.End();
            #endregion            
            
        }
        #endregion


        #region Handle Game Content
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch2 = new SpriteBatch(GraphicsDevice);
            spriteBatch3 = new SpriteBatch(GraphicsDevice);
            spriteBatch4 = new SpriteBatch(GraphicsDevice);

            LoadingScreenBackground = SecondaryContent.Load<Texture2D>("Backgrounds/LoadingScreen");

            BlankTexture = SecondaryContent.Load<Texture2D>("Blank");

            DefaultCursor = SecondaryContent.Load<Texture2D>("Cursors/DefaultCursor");
            CrosshairCursor = SecondaryContent.Load<Texture2D>("Cursors/Crosshair");
            FireCursor = SecondaryContent.Load<Texture2D>("Icons/FireIcon");
            WallCursor = SecondaryContent.Load<Texture2D>("Icons/WallIcon");
            SpikesCursor = SecondaryContent.Load<Texture2D>("Icons/SpikesIcon");

            BasicTurretCursor = SecondaryContent.Load<Texture2D>("Icons/BasicTurretIcon2");
            CannonTurretCursor = SecondaryContent.Load<Texture2D>("Icons/CannonTurretIcon");
            FlameThrowerCursor = SecondaryContent.Load<Texture2D>("Icons/FlameThrowerTurretIcon");

            ButtonFont = SecondaryContent.Load<SpriteFont>("Fonts/ButtonFont");
            HUDFont = SecondaryContent.Load<SpriteFont>("Fonts/HUDFont");

            PrimaryCursorTexture = DefaultCursor;
            CurrentCursorTexture = BlankTexture;
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
                LoadLevel(CurrentProfile.LevelNumber);

                switch (CurrentLevel.WorldType)
                {
                    default:
                        CurrentWorld = new IceWorld();
                        break;

                    case "Ice":
                        CurrentWorld = new IceWorld();
                        break;

                    case "Lava":
                        CurrentWorld = new LavaWorld();
                        break;
                }

                UpgradesList = new List<Upgrade>();

                if (CurrentProfile.Upgrade1 == true)
                    UpgradesList.Add(new Upgrade1());

                HUDBarTexture = Content.Load<Texture2D>("Backgrounds/NewInterface");

                HealthBarEffect = Content.Load<Effect>("HealthBarEffect");
                HealthBarSprite = Content.Load<Texture2D>("HealthBar");
                ShieldBarSprite = Content.Load<Texture2D>("ShieldBar");

                WaveListIndex = 0;
                IsLoading = true;

                ReadyToPlace = false;

                Ground = new StaticSprite(CurrentWorld.GroundAsset, new Vector2(0, 0));
                Ground.LoadContent(Content);
                Ground.Position = new Vector2(0, 720-200);

                Tower = new Tower("Tower", new Vector2(32, 350), 2000, 1000, 10);

                TestBackground = new StaticSprite(CurrentWorld.BackgroundAsset, new Vector2(0, 335));
                TestBackground.Scale = new Vector2(1, 0.8f);
                TestBackground.LoadContent(Content);

                SelectButtonAssetName = "Buttons/NewButton";
                TowerSlotAssetName = "Buttons/TrapButton";

                #region IconNameList, PauseMenuNameList;
                //This gets the names of the icons that are to appear on the
                //buttons that allow the player to select traps/turrets they want to place
                IconNameList = new List<string>(8);

                for (int i = 0; i < 9; i++)
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
                            IconNameList[Index] = "Icons/BasicTurretIcon2";
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
                PauseMenuNameList.Add("Exit");
                #endregion                
       
                PauseMenuBackground = Content.Load<Texture2D>("Backgrounds/PauseMenuBackground");               

                LoadGameSounds();
                ShellCasingList = new List<Particle>();
                Random = new Random();
                
                Resources = 1000;

                TowerHealthBar = new HorizontalBar(Content, new Vector2(220, 20), (int)Tower.MaxHP, (int)Tower.CurrentHP);                
                ShellCasing = Content.Load<Texture2D>("Particles/Shell");
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
                    if (i < IconNameList.Count - 1)
                    {
                        SelectButtonList.Add(new Button(SelectButtonAssetName, new Vector2(277 + (i * 72), 720 - HUDBarTexture.Height + 11)));
                        SelectButtonList[i].LoadContent(Content);
                    }
                    else
                    {
                        SelectButtonList.Add(new Button(SelectButtonAssetName, new Vector2(277 + (i * 72), 720 - HUDBarTexture.Height + 11)));
                        SelectButtonList[i].LoadContent(Content);
                    }
                }

                for (int i = 0; i < 4; i++)
                {
                    PauseButtonList.Add(new Button("Buttons/ButtonLeft", new Vector2(640 - 225, 128 + (i * 128)), null, null, null, PauseMenuNameList[i], "Fonts/ButtonFont", "Left", Color.White));
                    PauseButtonList[i].LoadContent(Content);
                }
                #endregion

                #region List Creating Code
                //This code just creates the lists for the buttons and traps with the right number of possible slots
                TrapList = new List<Trap>();

                TurretList = new List<Turret>();
                for (int i = 0; i < TowerButtons; i++)
                {
                    TurretList.Add(new BlankTurret());
                    TurretList[i].LoadContent(Content);
                }

                InvaderList = new List<Invader>();

                foreach (Invader invader in InvaderList)
                {
                    invader.LoadContent(Content);
                }

                HeavyProjectileList = new List<HeavyProjectile>();
                InvaderHeavyProjectileList = new List<HeavyProjectile>();

                LightProjectileList = new List<LightProjectile>();

                EmitterList = new List<Emitter>();
                EmitterList2 = new List<Emitter>();

                ExplosionList = new List<Explosion>();
                EnemyExplosionList = new List<Explosion>();
                #endregion

                WaveList = new List<Wave>();

                WaveList.Add(new Wave()
                {
                    Active = true,
                    Delay = 1000,
                    InvaderType = InvaderType.SuicideBomber,
                    Number = 10
                });

                WaveList.Add(new Wave()
                {
                    Active = true,
                    Delay = 1000,
                    InvaderType = InvaderType.Soldier,
                    Number = 2
                });

                WaveList.Add(new Wave()
                {
                    Active = true,
                    Delay = 5000,
                    InvaderType = InvaderType.Tank,
                    Number = 6
                });                

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
            //Load all the game sounds here
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
                        case TurretType.Gatling:

                            if (Resources >= 100)
                            {
                                TowerButtonList[Index].ButtonActive = false;
                                TurretList[Index] = new GatlingTurret(TowerButtonList[Index].Position); //Fix this to make sure that the BasicTurret has the resource names built in.

                                //Apply the upgrades for the gatling turret in here
                                TurretList[Index].FireDelay = PercentageChange(TurretList[Index].FireDelay, GatlingSpeed);
                                TurretList[Index].Damage = (int)PercentageChange(TurretList[Index].Damage, 0);

                                TurretList[Index].LoadContent(Content);
                                Resources -= 100;
                                SelectedTurret = TurretType.Blank;
                                TurretList[Index].Selected = false;
                            }
                            break;

                        case TurretType.Cannon:
                            if (Resources >= 200)
                            {
                                TowerButtonList[Index].ButtonActive = false;
                                TurretList[Index] = new CannonTurret(TowerButtonList[Index].Position);

                                //Apply the upgrades for the cannon turret in here
                                TurretList[Index].FireDelay = PercentageChange(TurretList[Index].FireDelay, CannonSpeed);
                                TurretList[Index].Damage = (int)PercentageChange(TurretList[Index].Damage, 0);

                                TurretList[Index].LoadContent(Content);
                                Resources -= 200;
                                SelectedTurret = TurretType.Blank;
                                TurretList[Index].Selected = false;

                                
                            }
                            break;

                        case TurretType.FlameThrower:
                            if (Resources >= 200)
                            {
                                TowerButtonList[Index].ButtonActive = false;
                                TurretList[Index] = new FlameThrowerTurret(TowerButtonList[Index].Position);
                                
                                //Apply the upgrades for the flamethrower turret in here

                                TurretList[Index].LoadContent(Content);
                                Resources -= 200;
                                SelectedTurret = TurretType.Blank;
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
                                        SelectedTrap = TrapType.Blank;
                                        SelectedTurret = TurretType.Blank;
                                    }
                                    break;

                                case "":
                                    {
                                        SelectedTrap = TrapType.Blank;
                                        SelectedTurret = TurretType.Blank;
                                    }
                                    break;

                                case "FireTrap":
                                    {
                                        SelectedTrap = TrapType.Fire;
                                        SelectedTurret = TurretType.Blank;
                                    }
                                    break;

                                case "IceTrap":
                                    {
                                        SelectedTrap = TrapType.Ice;
                                        SelectedTurret = TurretType.Blank;
                                    }
                                    break;

                                case "Wall":
                                    {
                                        SelectedTrap = TrapType.Wall;
                                        SelectedTurret = TurretType.Blank;
                                    }
                                    break;

                                case "MachineGun":
                                    {
                                        SelectedTrap = TrapType.Blank;
                                        SelectedTurret = TurretType.Gatling;
                                    }
                                    break;

                                case "Cannon":
                                    {
                                        SelectedTrap = TrapType.Blank;
                                        SelectedTurret = TurretType.Cannon;
                                    }
                                    break;

                                case "SawBlade":
                                    {
                                        SelectedTrap = TrapType.SawBlade;
                                        SelectedTurret = TurretType.Blank;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            SelectedTrap = TrapType.Blank;
                            SelectedTurret = TurretType.Blank;
                        }
                    });

                    switch (Index)
                    {
                        case 0:
                            CheckLayout();
                            ReadyToPlace = true;
                            break;

                        case 1:
                            CheckLayout();
                            ReadyToPlace = true;
                            break;

                        case 2:
                            CheckLayout();
                            ReadyToPlace = true;
                            break;

                        case 3:
                            CheckLayout();
                            ReadyToPlace = true;
                            break;

                        case 4:
                            CheckLayout();
                            ReadyToPlace = true;
                            break;

                        case 5:
                            CheckLayout();
                            ReadyToPlace = true;
                            break;

                        case 6:
                            CheckLayout();
                            ReadyToPlace = true;
                            break;

                        case 7:
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
            if (GameState == GameState.Menu)
            {
                int Index;

                foreach (Button button in MainMenuButtonList)
                {
                    button.Update();

                    if (button.JustClicked == true)
                    {
                        Index = MainMenuButtonList.IndexOf(button);

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
                                break;

                            case 3:
                                
                                break;

                            case 4:
                                this.Exit();
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
            if (GameState == GameState.Paused)
            {
                int Index;

                foreach (Button button in PauseButtonList)
                {
                    button.Update();

                    if (button.JustClicked == true)
                    {
                        Index = PauseButtonList.IndexOf(button);

                        switch (Index)
                        {
                            case 0:
                                GameState = GameState.Playing;
                                break;

                            case 1:

                                break;

                            case 2:
                                GameState = GameState.Menu;
                                CurrentProfile = null;
                                ResetUpgrades();
                                UnloadGameContent();
                                break;

                            case 3:
                                this.Exit();
                                break;
                        }
                    }
                }
            }
            #endregion

            #region Handling Profile Select Menu Button Presses
            if (GameState == GameState.ProfileSelect)
            {
                int Index;

                foreach (Button button in ProfileButtonList)
                {
                    button.Update();

                    if (button.JustClicked == true)
                    {
                        Index = ProfileButtonList.IndexOf(button);

                        switch (Index)
                        {
                            case 0:
                                ProfileNumber = 1;
                                FileName = "Profile1.sav";
                                StorageDevice.BeginShowSelector(this.HandleProfile, null);
                                SetProfileNames();
                                break;

                            case 1:
                                ProfileNumber = 2;
                                FileName = "Profile2.sav";
                                StorageDevice.BeginShowSelector(this.HandleProfile, null);
                                SetProfileNames();
                                break;

                            case 2:
                                ProfileNumber = 3;
                                FileName = "Profile3.sav";
                                StorageDevice.BeginShowSelector(this.HandleProfile, null);
                                SetProfileNames();
                                break;

                            case 3:
                                ProfileNumber = 4;
                                FileName = "Profile4.sav";
                                StorageDevice.BeginShowSelector(this.HandleProfile, null);
                                SetProfileNames();
                                break;

                            case 4:
                                ProfileNumber = 5;
                                FileName = "Profile5.sav";
                                StorageDevice.BeginShowSelector(this.HandleProfile, null);
                                SetProfileNames();
                                break;

                            case 5:
                                ProfileNumber = 6;
                                FileName = "Profile6.sav";
                                StorageDevice.BeginShowSelector(this.HandleProfile, null);
                                SetProfileNames();
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
                                StorageDevice.BeginShowSelector(this.DeleteProfile, null);
                                SetProfileNames();
                                break;

                            case 1:
                                FileName = "Profile2.sav";
                                StorageDevice.BeginShowSelector(this.DeleteProfile, null);
                                SetProfileNames();
                                break;

                            case 2:
                                FileName = "Profile3.sav";
                                StorageDevice.BeginShowSelector(this.DeleteProfile, null);
                                SetProfileNames();
                                break;

                            case 3:
                                FileName = "Profile4.sav";
                                StorageDevice.BeginShowSelector(this.DeleteProfile, null);
                                SetProfileNames();
                                break;

                            case 4:
                                FileName = "Profile5.sav";
                                StorageDevice.BeginShowSelector(this.DeleteProfile, null);
                                SetProfileNames();
                                break;

                            case 5:
                                FileName = "Profile6.sav";
                                StorageDevice.BeginShowSelector(this.DeleteProfile, null);
                                SetProfileNames();
                                break;
                        }
                    }
                }

                ProfileBackButton.Update();

                if (ProfileBackButton.JustClicked == true)
                {
                    GameState = GameState.Menu;
                }
            }
            #endregion

            #region Handling Profile Management Button Presses
            if (GameState == GameState.ProfileManagement)
            {
                int Index;

                ProfileManagementPlay.Update();
                ProfileManagementBack.Update();

                foreach (Button button in UpgradesButtonList)
                {
                    button.Update();

                    if (button.JustClicked == true)
                    {
                        Index = UpgradesButtonList.IndexOf(button);

                        switch (Index)
                        {
                            case 0:
                                CurrentProfile.Upgrade1 = true;
                                StorageDevice.BeginShowSelector(this.SaveProfile, null);
                                //Save data to file. Depending on currently selected profile number
                                //The profile number needs to be set when the player selects a profile
                                break;

                            case 1:
                                CurrentProfile.Upgrade2 = true;
                                StorageDevice.BeginShowSelector(this.SaveProfile, null);
                                break;
                        }
                    }
                }

                if (ProfileManagementBack.JustClicked == true)
                {
                    GameState = GameState.ProfileSelect;
                }

                if (ProfileManagementPlay.JustClicked == true)
                {
                    if (CurrentProfile != null)
                    {
                        LoadUpgrades();
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
                OptionsSave.Update();

                if (OptionsBack.JustClicked == true)
                {
                    GameState = GameState.Menu;
                }

                if (OptionsSave.JustClicked == true)
                {
                    CurrentSettings = new Settings
                    {
                        FullScreen = true
                    };

                    SaveSettings();

                    graphics.IsFullScreen = CurrentSettings.FullScreen;
                    graphics.ApplyChanges();
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
                    switch (invader.InvaderType)
                    {
                        case InvaderType.Soldier: 
                        case InvaderType.SuicideBomber:                        
                            EmitterList2.Add(new Emitter("Particles/Splodge", new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                            new Vector2(0, 360), new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), new Vector2(1, 3),
                            new Vector2(0.02f, 0.06f), Color.DarkRed, Color.Red, 0.1f, 0.2f, 20, 10, true, new Vector2(invader.MaxY, invader.MaxY)));
                        EmitterList2[EmitterList2.Count - 1].LoadContent(Content);
                            break;

                        case InvaderType.Spider:
                            EmitterList2.Add(new Emitter("Particles/Splodge", new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                            new Vector2(0, 360), new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), new Vector2(1, 3),
                            new Vector2(0.02f, 0.06f), Color.Green, Color.Lime, 0.1f, 0.2f, 20, 10, true, new Vector2(invader.MaxY, invader.MaxY)));
                        EmitterList2[EmitterList2.Count - 1].LoadContent(Content);
                            break;

                        case InvaderType.Tank:
                            EmitterList2.Add(new Emitter("Particles/Splodge", new Vector2(invader.DestinationRectangle.Center.X, invader.DestinationRectangle.Center.Y),
                            new Vector2(0, 360), new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), new Vector2(1, 3),
                            new Vector2(0.02f, 0.06f), Color.DarkOliveGreen, Color.DarkSeaGreen, 0.1f, 0.2f, 20, 10, true, new Vector2(invader.MaxY, invader.MaxY)));
                        EmitterList2[EmitterList2.Count - 1].LoadContent(Content);
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
                //if (invader.InvaderType == InvaderType.Soldier || 
                //    invader.InvaderType == InvaderType.Tank || 
                //    invader.InvaderType == InvaderType.Spider ||
                //    invader.InvaderType == InvaderType.Slime || 
                //    invader.InvaderType == InvaderType.SuicideBomber)
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
            {
                foreach (Explosion explosion in ExplosionList)
                {
                    foreach (Invader invader in InvaderList)
                    {
                        if (Vector2.Distance(invader.Position, explosion.Position) < explosion.BlastRadius &&
                            explosion.Active == true)
                        {
                            //float blastStrength;
                            //blastStrength is calculated here. The closer the invader is to the blast, the higher blast strength should be
                            //Dependent on explosion.BlastRadius.
                            //invader.Trajectory(new Vector2(blastStrength, -blastStrength));
                            float Distance = Vector2.Distance(invader.Position, explosion.Position);

                            if (TrapList.Count < 1)
                            {
                                invader.CurrentHP -= (int)(explosion.Damage / 100 * (100 - (100 / explosion.BlastRadius) * Distance));
                            }

                            if (TrapList.Count > 0)
                            {
                                foreach (Trap trap in TrapList)
                                {
                                    if (trap.TrapType == TrapType.Wall)
                                        //This makes sure that the invaders aren't damaged if they're standing behind a wall
                                        if (invader.Position.X > trap.Position.X && explosion.Position.X < trap.Position.X)
                                        {
                                            //Do nothing
                                        }
                                        else
                                        {
                                            invader.CurrentHP -= (int)(explosion.Damage / 100 * (100 - (100 / explosion.BlastRadius) * Distance));
                                        }                                    
                                }
                            }
                        }
                    }

                    explosion.Active = false;
                }
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
                                Color FireColor = Color.DarkOrange;
                                FireColor.A = 200;

                                Color FireColor2 = Color.DarkOrange;
                                FireColor2.A = 90;

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

                                EnemyExplosionList.Add(new Explosion(invader.Position, 100, invader.AttackPower));

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
                                0.2f);

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
                        case TrapType.Wall:
                            {
                                #region
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
                                #endregion
                            }
                            break;

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


        private void InvaderProjectileUpdate(GameTime gameTime)
        {
            foreach (HeavyProjectile heavyProjectile in InvaderHeavyProjectileList)
            {
                heavyProjectile.Update(gameTime);

                if (heavyProjectile.Position.Y > heavyProjectile.MaxY && heavyProjectile.Active == true)
                {                   
                    switch (heavyProjectile.HeavyProjectileType)
                    {
                        case HeavyProjectileType.Acid:
                            {
                                Emitter newEmitter = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                new Vector2(0, 180), new Vector2(2, 3), new Vector2(10, 25), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                new Vector2(0.02f, 0.06f), Color.Lime, Color.LimeGreen, 0.2f, 0.2f, 20, 10, true, new Vector2(heavyProjectile.MaxY+8, heavyProjectile.MaxY+8));
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

                    heavyProjectile.Emitter.AddMore = false;
                    heavyProjectile.Active = false;
                    heavyProjectile.Velocity = Vector2.Zero;
                }

                if (heavyProjectile.DestinationRectangle.Intersects(Tower.DestinationRectangle) && heavyProjectile.Active == true)
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

                    heavyProjectile.Active = false;
                    heavyProjectile.Emitter.AddMore = false;
                    heavyProjectile.Velocity = Vector2.Zero;
                }
            }
        }

        private void EnemyExplosionsUpdate()
        {
            if (EnemyExplosionList.Count > 0)
            {
                foreach (Explosion explosion in EnemyExplosionList)
                {
                    if (Vector2.Distance(new Vector2(Tower.DestinationRectangle.Right, explosion.Position.Y), 
                        explosion.Position) < explosion.BlastRadius && explosion.Active == true)
                    {
                        float Distance = Vector2.Distance(new Vector2(Tower.DestinationRectangle.Right, Tower.DestinationRectangle.Bottom), explosion.Position);
                        Tower.TakeDamage((int)(explosion.Damage / 100 * (100 - (100 / explosion.BlastRadius) * Distance)));
                    }

                    //Add in the code here that will damage the traps when there's an exp

                    explosion.Active = false;
                }
            }
        }
        #endregion

        #region TURRET stuff that needs to be called every step
        private void TurretUpdate()
        {
            //This piece of code makes sure that two turrets cannot be selected at any one time
            foreach (Turret turret in TurretList)
            {
                if (turret.Active == true)
                    if (turret.JustClicked == true)
                    {
                        ClearSelected();
                        turret.Selected = true;
                        SelectedTurretIndex = TurretList.IndexOf(turret);
                        foreach (Turret turret2 in TurretList)
                        {
                            if (turret2 != turret)
                            {
                                turret2.Selected = false;
                            }
                        }
                    }
            }
        }
  
        private void TurretShoot()
        {
            //Get selected turret type//
            //Determine if it should be a heavy or a light projectile//
            //Get the turret's firing strength//
            //Get the turret's current angle//
            //Create the correct type of projectile using the values determined by the turret//

            //Light and heavy projectiles both have angle//
            //Only heavy projectiles need momentum etc. 
            foreach (Turret turret in TurretList)
            {
                if (turret.Active == true && CursorPosition.Y < (720 - HUDBarTexture.Height))
                {
                    if (turret.Selected == true)
                    {
                        //turret.Random = new Random();
                        Vector2 MousePosition, Direction;

                        CurrentMouseState = Mouse.GetState();
                        MousePosition = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);

                        Direction = turret.FireDirection;
                        Direction.Normalize();

                        switch (turret.TurretType)
                        {
                            case TurretType.Gatling:
                                {
                                    Color FireColor = Color.DarkOrange;
                                    FireColor.A = 200;

                                    Color FireColor2 = Color.DarkOrange;
                                    FireColor2.A = 90;

                                    Vector2 BarrelEnd = new Vector2((float)Math.Cos(turret.Rotation) * (turret.BarrelRectangle.Width - 25),
                                                                     (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height + 25));

                                    CurrentProjectile = new MachineGunProjectile(new Vector2(turret.BarrelRectangle.X, 
                                        turret.BarrelRectangle.Y), Direction);
                                    LightProjectileList.Add(CurrentProjectile);

                                    Emitter FlashEmitter = new Emitter("Particles/FireParticle", new Vector2(turret.BarrelRectangle.X + BarrelEnd.X, 
                                    turret.BarrelRectangle.Y + BarrelEnd.Y), 
                                    new Vector2(                                        
                                        MathHelper.ToDegrees(-(float)Math.Atan2(turret.Direction.Y, turret.Direction.X)),
                                        MathHelper.ToDegrees(-(float)Math.Atan2(turret.Direction.Y, turret.Direction.X))),
                                    new Vector2(20, 30), new Vector2(1, 4), 1f, true, new Vector2(0, 360),
                                new Vector2(-10, 10), new Vector2(2, 3), FireColor, FireColor2, 0.0f, 0.05f, 1, 10, 
                                false, new Vector2(0, 720), true);
                                    EmitterList.Add(FlashEmitter);
                                    EmitterList[EmitterList.IndexOf(FlashEmitter)].LoadContent(Content);

                                    ShellCasingList.Add(new Particle(ShellCasing,
                                        new Vector2(turret.BarrelRectangle.X, turret.BarrelRectangle.Y), 
                                        turret.Rotation - MathHelper.ToRadians((float)DoubleRange(175, 185)), 
                                        (float)DoubleRange(2, 4), 500, 1f, false, (float)DoubleRange(-10, 10), 
                                        (float)DoubleRange(-3, 6), 1, Color.White, Color.White, 0.09f, true, Random.Next(600, 630), false));
                                }
                                break;

                            case TurretType.Cannon:
                                {
                                    HeavyProjectile heavyProjectile;
                                    Vector2 BarrelEnd;

                                    Color FireColor = Color.DarkOrange;
                                    FireColor.A = 200;

                                    Color FireColor2 = Color.DarkOrange;
                                    FireColor2.A = 90;

                                    BarrelEnd = new Vector2((float)Math.Cos(turret.Rotation) * (turret.BarrelRectangle.Width - 20),
                                                            (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height + 20));

                                    Emitter FlashEmitter = new Emitter("Particles/FireParticle", 
                                        new Vector2(turret.BarrelRectangle.X + BarrelEnd.X, turret.BarrelRectangle.Y + BarrelEnd.Y),
                                new Vector2(
                                    MathHelper.ToDegrees(-(float)Math.Atan2(turret.Direction.Y, turret.Direction.X)), 
                                    MathHelper.ToDegrees(-(float)Math.Atan2(turret.Direction.Y, turret.Direction.X))), 
                                    new Vector2(10, 20), new Vector2(1, 6), 0.01f, true, new Vector2(0, 360),
                                new Vector2(-2, 2), new Vector2(3, 4), FireColor, FireColor2, 0.0f, 0.2f, 1, 5, false, new Vector2(0, 720), true);
                                    EmitterList.Add(FlashEmitter);
                                    EmitterList[EmitterList.IndexOf(FlashEmitter)].LoadContent(Content);

                                    heavyProjectile = new CannonBall(new Vector2(turret.BarrelRectangle.X + BarrelEnd.X, 
                                        turret.BarrelRectangle.Y + BarrelEnd.Y), 12, turret.Rotation, 0.2f);
                                    heavyProjectile.LoadContent(Content);
                                    HeavyProjectileList.Add(heavyProjectile);
                                }
                                break;

                            case TurretType.FlameThrower:
                                {
                                    HeavyProjectile heavyProjectile;
                                    Vector2 BarrelEnd;

                                    BarrelEnd = new Vector2((float)Math.Cos(turret.Rotation) * (turret.BarrelRectangle.Width-20),
                                                            (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height+20));

                                    heavyProjectile = new FlameProjectile(new Vector2(turret.BarrelRectangle.X + BarrelEnd.X-2, 
                                        turret.BarrelRectangle.Y + BarrelEnd.Y-2), (float)DoubleRange(7,9), turret.Rotation, 0.3f);
                                    heavyProjectile.Emitter.AngleRange = new Vector2(-(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) - 20,
                                -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) + 20); 
                                    heavyProjectile.LoadContent(Content);
                                    HeavyProjectileList.Add(heavyProjectile);
                                }
                                break;
                        }
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
                    heavyProjectile.Emitter.AngleRange = new Vector2(-(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) - 20,
                                -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X))) + 20);
                                
                #region This makes sure that particles are emitted from the wall in the correct position when a projectile hits it
                if (TrapList.Any(Trap => Trap.DestinationRectangle.Intersects(heavyProjectile.CollisionRectangle) && 
                    Trap.TrapType == TrapType.Wall))
                {
                    int index = TrapList.IndexOf(TrapList.First(Trap => Trap.DestinationRectangle.Intersects(heavyProjectile.CollisionRectangle)));
                    if (heavyProjectile.Active == true)
                    {                       
                        Emitter newEmitter = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                        new Vector2(-(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X)))-45, 
                            -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X)))+45),                            
                            new Vector2(2, 3), new Vector2(100, 200), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                        new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true, 
                        new Vector2(TrapList[index].DestinationRectangle.Bottom, TrapList[index].DestinationRectangle.Bottom));
                                                  
                        EmitterList2.Add(newEmitter);

                        EmitterList2[EmitterList2.IndexOf(newEmitter)].LoadContent(Content);

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
                        case HeavyProjectileType.CannonBall:
                            {
                                Color DirtColor = new Color();
                                DirtColor.A = 150;
                                DirtColor.R = 51;
                                DirtColor.G = 31;
                                DirtColor.B = 0;

                                Color DirtColor2 = DirtColor;
                                DirtColor2.A = 125;

                                Vector2 HeavyProjectileCollision = new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY);  

                                #region This makes the fire react to cannonballs
                                foreach (Trap trap in TrapList.Where(trap => trap.TrapType == TrapType.Fire))
                                {
                                    if (Vector2.Distance(new Vector2(trap.DestinationRectangle.Center.X, trap.DestinationRectangle.Bottom),
                                        HeavyProjectileCollision) <= 72)
                                    {
                                        if (trap.DestinationRectangle.Center.X > HeavyProjectileCollision.X)
                                        {
                                            trap.CurrentAffectedTime = 0;

                                            foreach (Emitter emitter in trap.TrapEmitterList)
                                                emitter.AngleRange = new Vector2(0, 45);
                                        }

                                        if (trap.DestinationRectangle.Center.X <= HeavyProjectileCollision.X)
                                        {
                                            trap.CurrentAffectedTime = 0;

                                            foreach (Emitter emitter in trap.TrapEmitterList)
                                                emitter.AngleRange = new Vector2(135, 180);
                                        }
                                    }
                                }
                                #endregion

                                Emitter newEmitter = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.MaxY),
                                new Vector2(0, 180), new Vector2(2, 3), new Vector2(10, 25), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                                new Vector2(0.02f, 0.06f), DirtColor, DirtColor2, 0.2f, 0.2f, 20, 10, true, new Vector2(heavyProjectile.MaxY + 8, heavyProjectile.MaxY + 8));
                                EmitterList2.Add(newEmitter);
                                EmitterList2[EmitterList2.IndexOf(newEmitter)].LoadContent(Content);

                                

                                ExplosionList.Add(new Explosion(heavyProjectile.Position, 300, heavyProjectile.Damage));
                            }
                            break;

                        case HeavyProjectileType.FlameThrower:
                            {
                                Color FireColor = Color.Black;
                                Color FireColor2 = Color.DarkGray;

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

                        case HeavyProjectileType.Acid:
                            {

                            }
                            break;                       
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
                if (InvaderList.Any(Invader2 => Invader2.DestinationRectangle.Intersects(heavyProjectile.DestinationRectangle)) && heavyProjectile.Active == true)
                {
                    int Index = InvaderList.IndexOf(InvaderList.First(Invader2 => Invader2.DestinationRectangle.Intersects(heavyProjectile.DestinationRectangle)));

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

                    switch (heavyProjectile.HeavyProjectileType)
                    {
                        case HeavyProjectileType.CannonBall:
                            switch (InvaderList[Index].InvaderType)
                            {
                                case InvaderType.Airship:                                    
                                    InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                                    DeactivateProjectile.Invoke();
                                    break;

                                case InvaderType.Soldier:

                                    break;

                                case InvaderType.Slime:

                                    break;

                                case InvaderType.Spider:

                                    break;

                                case InvaderType.Tank:
                                    InvaderList[Index].CurrentHP -= heavyProjectile.Damage;
                                    DeactivateProjectile.Invoke();

                                    Emitter newEmitter = new Emitter("Particles/Splodge", new Vector2(heavyProjectile.Position.X, heavyProjectile.Position.Y),
                        new Vector2(-(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X)))-45, 
                            -(MathHelper.ToDegrees((float)Math.Atan2(-heavyProjectile.Velocity.Y, -heavyProjectile.Velocity.X)))+90),                            
                            new Vector2(2, 3), new Vector2(100, 200), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                        new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true,
                        new Vector2(InvaderList[Index].MaxY, InvaderList[Index].MaxY));
                                                  
                        EmitterList2.Add(newEmitter);

                        EmitterList2[EmitterList2.IndexOf(newEmitter)].LoadContent(Content);
                                    break;
                            }           
                            break;
                    

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
                    }                          
                }                
                #endregion                
            }          
        }

        private void LightProjectileUpdate()
        {
            Ground.BoundingBox = new BoundingBox(new Vector3(0, MathHelper.Clamp(CursorPosition.Y, 525, 720), 0), new Vector3(1280, 720, 0));

            foreach (Turret turret1 in TurretList)
            {
                if (turret1.Active == true)
                    if (turret1.Selected == true)
                        if (CurrentProjectile != null)
                        {
                            double Angle = MathHelper.ToDegrees((float)Math.Atan2(CurrentProjectile.Ray.Direction.Y,
                                CurrentProjectile.Ray.Direction.X));
                            //This checks if the ray hit ANY invader and ANY wall. If it hit both an invader and a wall (Doesn't matter which invader or which wall
                            //just that it hit them. Then it checks if the invader was closer to the origin of the ray. If it was, the invader is destroyed//
                            //IF the wall was hit before the invader, then it does nothing. 
                            if (InvaderList.Any(TestInvader => TestInvader.BoundingBox.Intersects(CurrentProjectile.Ray) != null) &&
                                TrapList.Any(TestTrap => TestTrap.BoundingBox.Intersects(CurrentProjectile.Ray) != null) &&
                                CurrentProjectile.Active == true)
                            {
                                Invader HitInvader = InvaderList.First(TestInvader => TestInvader.BoundingBox.Intersects(
                                    CurrentProjectile.Ray) != null);

                                Trap HitTrap = TrapList.First(TestTrap => TestTrap.BoundingBox.Intersects(CurrentProjectile.Ray) != null);

                                if (CurrentProjectile.Ray.Intersects(HitInvader.BoundingBox) < CurrentProjectile.Ray.Intersects(
                                    HitTrap.BoundingBox) && HitInvader.Active == true)
                                {
                                    if (CurrentProjectile.LightProjectileType == LightProjectileType.MachineGun)
                                    {
                                        CurrentProjectile.Active = false;
                                        HitInvader.TurretDamage(-turret1.Damage);
                                        if (HitInvader.CurrentHP <= 0)
                                            Resources += HitInvader.ResourceValue;
                                        return;
                                    }

                                    if (CurrentProjectile.LightProjectileType == LightProjectileType.Freeze)
                                    {
                                        CurrentProjectile.Active = false;
                                        HitInvader.Freeze(3000, Color.Blue);
                                        if (HitInvader.CurrentHP <= 0)
                                            Resources += HitInvader.ResourceValue;
                                        return;
                                    }
                                }

                                if (CurrentProjectile.Ray.Intersects(HitInvader.BoundingBox) > CurrentProjectile.Ray.Intersects(
                                    HitTrap.BoundingBox) && HitInvader.Active == true && HitTrap.TrapType != TrapType.Wall)
                                {
                                    if (CurrentProjectile.LightProjectileType == LightProjectileType.MachineGun)
                                    {
                                        CurrentProjectile.Active = false;
                                        HitInvader.TurretDamage(-turret1.Damage);
                                        if (HitInvader.CurrentHP <= 0)
                                            Resources += HitInvader.ResourceValue;
                                        return;
                                    }

                                    if (CurrentProjectile.LightProjectileType == LightProjectileType.Freeze)
                                    {
                                        CurrentProjectile.Active = false;
                                        HitInvader.Freeze(5000, Color.Blue);
                                        if (HitInvader.CurrentHP <= 0)
                                            Resources += HitInvader.ResourceValue;
                                        return;
                                    }
                                }
                            }

                            //if (TrapList.Any(trap => trap.BoundingBox.Intersects(CurrentProjectile.Ray) != null) && CurrentProjectile.Active == true)
                            //{
                            //    Trap HitTrap = TrapList.First(TestTrap => TestTrap.BoundingBox.Intersects(CurrentProjectile.Ray) != null);

                            //    if (HitTrap.TrapType == TrapType.Barrel)
                            //    {
                            //        CurrentProjectile.Active = false;
                            //        ExplosionList.Add(new Explosion(HitTrap.Position, 200));
                            //        HitTrap.Active = false;
                            //    }
                            //}                            


                            //If the projectile ray DOES NOT intersect with any of the traps and it DOES intersect with an invader. Then it destroys the invader//
                            //This is to make sure that the ray doesn't have to intersect with a trap at any point to be able to kill an invader.
                            if (TrapList.All(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                                InvaderList.Any(TestInvader => TestInvader.BoundingBox.Intersects(CurrentProjectile.Ray) != null) &&
                                CurrentProjectile.Active == true)
                            {
                                Invader HitInvader = InvaderList.First(TestInvader => TestInvader.BoundingBox.Intersects(
                                    CurrentProjectile.Ray) != null);

                                if (HitInvader.Active == true)
                                {
                                    if (CurrentProjectile.LightProjectileType == LightProjectileType.MachineGun)
                                    {
                                        CurrentProjectile.Active = false;
                                        HitInvader.TurretDamage(-turret1.Damage);
                                        if (HitInvader.CurrentHP <= 0)
                                            Resources += HitInvader.ResourceValue;
                                        return;
                                    }

                                    if (CurrentProjectile.LightProjectileType == LightProjectileType.Freeze)
                                    {
                                        CurrentProjectile.Active = false;
                                        HitInvader.Freeze(5000, Color.Blue);
                                        if (HitInvader.CurrentHP <= 0)
                                            Resources += HitInvader.ResourceValue;
                                        return;
                                    }
                                }
                            }

                            //This makes sure that the ray doesn't intersect with ANY traps and makes sure that it doesn't intersect with ANY invaders
                            //If both of those conditions are true AND it intersects with the ground at any point, it calculates the ground collision point
                            //And creates a particle effect there so that the player knows they didn't hit an invader or a trap
                            //It was made so that if the ray intersects an invader at all, it will abort the ray. It doesn't really matter that the invader is hit before
                            //the ground because the invaders are always above the ground and none of the turrets are below the ground i.e. the ground will never be hit first
                            //due to the placement of the elements in the game.

                            if (TrapList.All(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                                    InvaderList.All(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                                    CurrentProjectile.Ray.Intersects(Ground.BoundingBox) != null && CurrentProjectile.Active == true)
                            {
                                Nullable<double> Distance = CurrentProjectile.Ray.Intersects(Ground.BoundingBox);

                                if (Distance != null)
                                {
                                    double Value1, Value2;

                                    foreach (Turret turret in TurretList)
                                    {
                                        GroundCollisionPoint.Y = Ground.BoundingBox.Min.Y;

                                        Value1 = Math.Pow(Distance.Value, 2); //This is the hypoteneuse

                                        Value2 = Math.Pow(Ground.BoundingBox.Min.Y - turret.BarrelRectangle.Y, 2); //This is the height to the turret

                                        if (CurrentMouseState.X > turret.BarrelRectangle.X)
                                        {
                                            GroundCollisionPoint.X = (turret.BarrelRectangle.X + (float)Math.Sqrt(Value1 - Value2));
                                        }
                                        else
                                        {
                                            GroundCollisionPoint.X = turret.BarrelRectangle.X - (float)Math.Sqrt(Value1 - Value2);
                                        }

                                        if (turret.Selected == true && turret.CanShoot == false
                                            && CurrentMouseState.LeftButton == ButtonState.Pressed
                                            && turret.TurretType == TurretType.Gatling)
                                        {
                                            Color DirtColor = new Color();
                                            DirtColor.A = 100;
                                            DirtColor.R = 51;
                                            DirtColor.G = 31;
                                            DirtColor.B = 0;

                                            Color DirtColor2 = DirtColor;
                                            DirtColor2.A = 125;

                                            DirtEmitter = new Emitter("Particles/Smoke", new Vector2(GroundCollisionPoint.X, GroundCollisionPoint.Y),
                                                new Vector2(90, 90), new Vector2(0.5f, 1f), new Vector2(20, 30), 1f, true, new Vector2(0, 0),
                                                new Vector2(-2, 2), new Vector2(0.5f, 1f), DirtColor, DirtColor2, 0f, 0.02f, 10, 1, false, new Vector2(0, 720), false);
                                            EmitterList.Add(DirtEmitter);
                                            EmitterList[EmitterList.IndexOf(DirtEmitter)].LoadContent(Content);
                                        }
                                    }

                                    CurrentProjectile.Active = false;
                                }
                            }

                            if (TrapList.All(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                                    InvaderList.All(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                                    CurrentProjectile.Ray.Intersects(Ground.BoundingBox) == null && CurrentProjectile.Active == true)
                            {
                                CurrentProjectile.Active = false;
                            }
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
                        if (Resources >= 20)
                        {
                            Trap NewTrap = new Wall(new Vector2(CursorPosition.X - 16, CursorPosition.Y));
                            TrapList.Add(NewTrap);
                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);
                            ReadyToPlace = false;
                            Resources -= 20;
                            ClearSelected();
                        }
                        break;

                    case TrapType.Fire:
                        if (Resources >= 60)
                        {
                            Trap NewTrap = new FireTrap(new Vector2(CursorPosition.X - 16, CursorPosition.Y));
                            TrapList.Add(NewTrap);

                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);

                            Color FireColor = Color.DarkOrange;
                            FireColor.A = 200;

                            Color FireColor2 = Color.DarkOrange;
                            FireColor2.A = 90;


                            Color SmokeColor = Color.Gray;
                            SmokeColor.A = 200;

                            Color SmokeColor2 = Color.WhiteSmoke;
                            SmokeColor.A = 175;

                            Emitter FireEmitter = new Emitter("Particles/FireParticle", new Vector2(NewTrap.Position.X+16, NewTrap.Position.Y),
                                new Vector2(70, 110), new Vector2(0.5f, 0.75f), new Vector2(40, 60), 0.01f, true, new Vector2(-20, 20),
                                new Vector2(-4, 4), new Vector2(1, 2f), FireColor, FireColor2, 0.0f, -1, 10, 1, false, new Vector2(0, 720),
                                false, CursorPosition.Y/720);
                            NewTrap.TrapEmitterList.Add(FireEmitter);

                            Emitter SmokeEmitter = new Emitter("Particles/Smoke", new Vector2(NewTrap.Position.X+16, NewTrap.Position.Y-4),
                                new Vector2(70, 110), new Vector2(0.2f, 0.5f), new Vector2(250, 350), 1f, true, new Vector2(-20, 20),
                                new Vector2(-4, 4), new Vector2(0.5f, 0.5f), SmokeColor, SmokeColor2, 0.0f, -1, 300, 1, false,
                                new Vector2(0, 720), false, CursorPosition.Y / 720);                            

                            NewTrap.TrapEmitterList.Add(SmokeEmitter);

                            NewTrap.TrapEmitterList[NewTrap.TrapEmitterList.IndexOf(SmokeEmitter)].LoadContent(Content);

                            NewTrap.TrapEmitterList.Add(FireEmitter);

                            NewTrap.TrapEmitterList[NewTrap.TrapEmitterList.IndexOf(FireEmitter)].LoadContent(Content);

                            ReadyToPlace = false;
                            Resources -= 60;
                            ClearSelected();
                        }
                        break;

                    case TrapType.Spikes:
                        if (Resources >= 30)
                        {
                            Trap NewTrap = new SpikeTrap(new Vector2(CursorPosition.X - 16, CursorPosition.Y));
                            TrapList.Add(NewTrap);
                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);
                            ReadyToPlace = false;
                            Resources -= 30;
                            ClearSelected();
                        }
                        break;

                    case TrapType.Catapult:
                        if (Resources >= 30)
                        {
                            Trap NewTrap = new CatapultTrap(new Vector2(CursorPosition.X - 16, CursorPosition.Y));
                            TrapList.Add(NewTrap);
                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);
                            ReadyToPlace = false;
                            Resources -= 30;
                            ClearSelected();
                        }
                        break;

                    case TrapType.Ice:
                        if (Resources >= 50)
                        {
                            Trap NewTrap = new IceTrap(new Vector2(CursorPosition.X - 16, CursorPosition.Y));
                            TrapList.Add(NewTrap);
                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);
                            ReadyToPlace = false;
                            Resources -= 50;
                            ClearSelected();
                        }
                        break;

                    case TrapType.Tar:
                        if (Resources >= 50)
                        {
                            Trap NewTrap = new TarTrap(new Vector2(CursorPosition.X - 16, CursorPosition.Y));
                            TrapList.Add(NewTrap);
                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);
                            ReadyToPlace = false;
                            Resources -= 50;
                            ClearSelected();
                        }
                        break;

                    case TrapType.Barrel:
                        if (Resources >= 50)
                        {
                            Trap NewTrap = new BarrelTrap(new Vector2(CursorPosition.X - 16, CursorPosition.Y));
                            TrapList.Add(NewTrap);
                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);
                            ReadyToPlace = false;
                            Resources -= 50;
                            ClearSelected();
                        }
                        break;

                    case TrapType.SawBlade:
                        if (Resources >= 50)
                        {
                            Trap NewTrap = new SawBladeTrap(new Vector2(CursorPosition.X - 16, CursorPosition.Y));
                            TrapList.Add(NewTrap);
                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);
                            ReadyToPlace = false;
                            Resources -= 50;
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

                if (trap.TrapType == TrapType.Fire)
                {
                    if (trap.Affected == false)
                    {
                        foreach (Emitter emitter in trap.TrapEmitterList)
                            emitter.AngleRange = Vector2.Lerp(emitter.AngleRange, new Vector2(70, 110), 0.02f);
                    }
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

                        switch (invader.InvaderType)
                        {
                            case InvaderType.Soldier:
                                switch (HitTrap.TrapType)
                                {
                                    case TrapType.SawBlade:
                                        EmitterList2.Add(new Emitter("Particles/Splodge", new Vector2(invader.DestinationRectangle.Center.X, HitTrap.DestinationRectangle.Center.Y),
                                    new Vector2(0, 65), new Vector2(2, 4), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), new Vector2(1, 3),
                                    new Vector2(0.02f, 0.06f), Color.DarkRed, Color.Red, 0.1f, 0.3f, 20, 20, true, new Vector2(invader.MaxY, invader.MaxY)));
                                        EmitterList2[EmitterList2.Count - 1].LoadContent(Content);
                                        break;
                                }
                                break;

                            case InvaderType.Slime:
                                switch (HitTrap.TrapType)
                                {
                                    case TrapType.SawBlade:
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
                    List<string> TempList = new List<string>();
                    TempList.Add("MachineGun");
                    TempList.Add("FireTrap");
                    TempList.Add("Wall");
                    TempList.Add("Cannon");
                    TempList.Add("FlameThrower");

                    CurrentProfile = new Profile()
                    {
                        Name = "Sabretkila",
                        LevelNumber = 1,
                        Points = 10,
                        Fire = true,
                        Cannon = false,
                        MachineGun = true,
                        Catapult = false,
                        FlameThrower = false,
                        Spikes = false,
                        Wall = true,
                        Buttons = TempList,
                        Upgrade1 = false, Upgrade2 = false, Upgrade3 = false, 
                        Credits = 10
                    };

                    StorageDevice.BeginShowSelector(this.NewProfile, null);
                    //GameState = GameState.GettingName;
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
            for (int i = 0; i < 6; i++)
            {
                FileName = "Profile" + (i + 1).ToString() + ".sav";
                StorageDevice.BeginShowSelector(this.GetProfileName, null);
                ProfileButtonList[i].Text = ProfileName;
            }
        }
        #endregion


        #region Handling enemy waves
        public void UpdateWave(GameTime gameTime)
        {
            CurrentWaveDelay += gameTime.ElapsedGameTime.TotalMilliseconds;

            CurrentWave = WaveList[WaveListIndex];

            if (CurrentWaveDelay >= CurrentWave.Delay && CurrentWave.Number > 0)
            {
                if (CurrentWave.InvaderType == InvaderType.Soldier)
                {
                    CurrentWave.Number--;
                    InvaderList.Add(new Soldier(new Vector2(1280, 500)));
                    InvaderList[InvaderList.Count - 1].LoadContent(Content);
                    CurrentWaveDelay = 0;
                }

                if (CurrentWave.InvaderType == InvaderType.Airship)
                {
                    CurrentWave.Number--;
                    InvaderList.Add(new Airship(new Vector2(1280, 128)));
                    InvaderList[InvaderList.Count - 1].LoadContent(Content);
                    CurrentWaveDelay = 0;
                }

                if (CurrentWave.InvaderType == InvaderType.Tank)
                {
                    CurrentWave.Number--;
                    InvaderList.Add(new Tank(new Vector2(1280, 500)));
                    InvaderList[InvaderList.Count - 1].LoadContent(Content);
                    CurrentWaveDelay = 0;
                }

                if (CurrentWave.InvaderType == InvaderType.Spider)
                {
                    CurrentWave.Number--;
                    InvaderList.Add(new Spider(new Vector2(1280, 400)));
                    InvaderList[InvaderList.Count - 1].LoadContent(Content);
                    CurrentWaveDelay = 0;
                }

                if (CurrentWave.InvaderType == InvaderType.Slime)
                {
                    CurrentWave.Number--;
                    InvaderList.Add(new Slime(new Vector2(1280, 400)));
                    InvaderList[InvaderList.Count - 1].LoadContent(Content);
                    CurrentWaveDelay = 0;
                }

                if (CurrentWave.InvaderType == InvaderType.SuicideBomber)
                {
                    CurrentWave.Number--;
                    InvaderList.Add(new SuicideBomber(new Vector2(1280, 400)));
                    InvaderList[InvaderList.Count - 1].LoadContent(Content);
                    CurrentWaveDelay = 0;
                }

            }

            if (CurrentWave.Number == 0)
            {
                if ((WaveList.Count -1) > WaveListIndex)
                WaveListIndex++;
            }
        }
        #endregion

         
        //Handle levels
        public void LoadLevel(int number)
        {
            CurrentLevel = Content.Load<Level>("Levels/Level" + number);
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
            }
            else
            {
                Stream stream = new FileStream("Content\\Settings\\Settings.xml", FileMode.Create);
                CurrentSettings = DefaultSettings;
                serializer.Serialize(stream, CurrentSettings);
                stream.Close();
            }
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


        //Handle upgrades
        public void LoadUpgrades()
        {
            if (CurrentProfile.Upgrade1 == true)
            {
                //Decreases the gatling reload speed by 50%
                GatlingSpeed -= 50;
                //Add in code here to do other effects such as DECREASING damage//
                //This could provide a sort of balance that the player has to choose between
                //...Might be a bad idea. My point is that multiple upgrades can be done at once
            }

            if (CurrentProfile.Upgrade2 == true)
            {
                CannonSpeed -= 75;
            }
        }

        public void ResetUpgrades()
        {
            GatlingSpeed = 0;
            CannonSpeed = 0;
        }


        //Some math functions
        public double DoubleRange(double one, double two)
        {
            return one + Random.NextDouble() * (two - one);
        }

        public double PercentageChange(double number, double percentage)
        {
            double newNumber = number;
            newNumber = newNumber + ((newNumber / 100) * percentage);
            return newNumber;
        }


        //Draw the correct cursor
        private void CursorDraw(SpriteBatch spriteBatch)
        {
            if (GameState == GameState.Playing)
            {
                if (TurretList.Any(Turret => Turret.Selected == true))
                {
                    PrimaryCursorTexture = CrosshairCursor;
                }
                else
                {
                    PrimaryCursorTexture = DefaultCursor;
                }

                switch (SelectedTrap)
                {
                    case TrapType.Blank:
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
                        CurrentCursorTexture = WallCursor;
                        break;
                }

                switch (SelectedTurret)
                {
                    case TurretType.Blank:
                        if (SelectedTrap == TrapType.Blank)
                            CurrentCursorTexture = BlankTexture;
                        break;

                    case TurretType.Gatling:
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
                    PrimaryCursorTexture = DefaultCursor;
                    spriteBatch.Draw(PrimaryCursorTexture, new Rectangle((int)CursorPosition.X, (int)CursorPosition.Y,
                                PrimaryCursorTexture.Width, PrimaryCursorTexture.Height), null, Color.White, MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, 1);
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
                turret.Selected = false;
            }
        }

        private void RightClickClearSelected()
        {
            if (CurrentMouseState.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed)
            {
                SelectedTurret = TurretType.Blank;
                SelectedTrap = TrapType.Blank;
                ReadyToPlace = false;

                foreach (Turret turret in TurretList)
                {
                    turret.Selected = false;
                }
            }
        }

        private void ClearSelected()
        {
            SelectedTurret = TurretType.Blank;
            SelectedTrap = TrapType.Blank;
            ReadyToPlace = false;
        }
        #endregion
    }
}
