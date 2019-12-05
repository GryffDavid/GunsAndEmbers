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

namespace TowerDefensePrototype
{
    public enum TrapType { Blank, Wall, Spikes, Catapult, Fire };
    public enum TurretType { Blank, Basic, Cannon, FlameThrower };
    public enum HeavyProjectileType { CannonBall, FlameThrower };
    public enum GameState { Menu, Loading, Playing, Paused, ProfileSelect, Options, ProfileManagement };

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        ContentManager SecondaryContent;
        SpriteBatch spriteBatch;
        SpriteBatch spriteBatch2;

        //XNA Declarations
        Texture2D BlankTexture, BackgroundTexture, PrimaryCursorTexture, 
            ShellCasing, LoadingScreenBackground, PauseMenuBackground, CurrentCursorTexture, 
            DefaultCursor, CrosshairCursor, FireCursor, WallCursor, SpikesCursor, 
            TurretCursor;
        Vector2 GroundPosition, CursorPosition, GroundCollisionPoint;
        Rectangle DestinationRectangle;
        SpriteFont ResourceFont;
        MouseState CurrentMouseState, PreviousMouseState;
        KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        int Resources, SelectedTurretIndex, TrapLimit, TowerButtons;
        string BackgroundAssetName, SelectButtonAssetName, TowerSlotAssetName, FileName, ContainerName;
        bool ReadyToPlace, slow, IsLoading;

        //List declarations
        List<Button> SelectButtonList;
        List<Button> TowerButtonList;
        List<Button> MainMenuButtonList;
        List<Button> PauseButtonList;
        List<Button> ProfileButtonList;

        List<Trap> TrapList;
        List<Turret> TurretList;
        List<Invader> InvaderList;
            
        List<HeavyProjectile> HeavyProjectileList;
        List<LightProjectile> LightProjectileList;
        
        List<Emitter> EmitterList;
        List<Particle> ParticleList;
        List<StaticSprite> CloudList;

        List<string> MainMenuNameList;
        List<string> PauseMenuNameList;
        List<string> IconNameList;    

        //Custom class declarations
        Tower Tower;
        StaticSprite Ground, Mountains, HUDTest;
        TrapType SelectedTrap;
        TurretType SelectedTurret;
        HorizontalBar TowerHealthBar;
        LightProjectile CurrentProjectile;
        Emitter DirtEmitter; 
 
        Random Random;
        GameState GameState;
        Thread LoadingThread;

        Profile CurrentPlayerProfile;
        StorageDevice Device;
        Stream OpenFile;      
        
        Camera Camera;

        //Main Game Functions//
        public Game1()
        {
            Camera = new Camera();
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            //IsMouseVisible = true;           
            //graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            SecondaryContent = new ContentManager(Content.ServiceProvider, Content.RootDirectory);          
            GameState = GameState.Menu;
            TrapLimit = 8;
            TowerButtons = 3;

            MainMenuNameList = new List<string>();
            MainMenuNameList.Add("New Game");
            MainMenuNameList.Add("Load Game");
            MainMenuNameList.Add("Options");
            MainMenuNameList.Add("Exit");

            MainMenuButtonList = new List<Button>();
            for (int i = 0; i < 4; i++)
            {
                MainMenuButtonList.Add(new Button("MenuButtonStrip", new Vector2(128, 128 + (i * 128)), null, null, null, MainMenuNameList[i], "MenuFont", null));
                MainMenuButtonList[i].LoadContent(SecondaryContent);
            }

            ProfileButtonList = new List<Button>();
            for (int i = 0; i < 6; i++)
            {
                ProfileButtonList.Add(new Button("MenuButtonStrip", new Vector2(128, 72 + (i * 100))));
                ProfileButtonList[i].LoadContent(SecondaryContent);
            }

            ContainerName = "TowerDefensePrototype";

            IsLoading = false;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch2 = new SpriteBatch(GraphicsDevice);
            LoadingScreenBackground = SecondaryContent.Load<Texture2D>("LoadingScreen");
            BlankTexture = SecondaryContent.Load<Texture2D>("Blank");            
            DefaultCursor = SecondaryContent.Load<Texture2D>("DefaultCursor");
            PrimaryCursorTexture = DefaultCursor;
            CurrentCursorTexture = BlankTexture;
        }

        protected override void UnloadContent()
        {
            Content.Unload();
            SecondaryContent.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            //This is just the stuff that needs to be updated every step
            //This is where I call all the smaller procedures that I broke the update into             
            CurrentMouseState = Mouse.GetState();
            CursorPosition = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);
            CurrentKeyboardState = Keyboard.GetState();

            if (GameState == GameState.Playing && IsLoading == false)
            {
                #region Handle Camera Shaking

                #endregion

                if (this.IsActive == false && GameState != GameState.Paused)
                    GameState = GameState.Paused;

                foreach (StaticSprite cloud in CloudList)
                {
                    cloud.Update(gameTime);
                }

                if (CurrentKeyboardState.IsKeyUp(Keys.Escape) && PreviousKeyboardState.IsKeyDown(Keys.Escape))
                {
                    GameState = GameState.Paused;
                }

                InvaderUpdate(gameTime);

                RightClickClearSelected();

                SelectButtonsUpdate();

                TowerButtonUpdate();

                TrapPlacement();

                TrapUpdate(gameTime);

                ProjectileUpdate(gameTime);

                TurretUpdate();

                TrapCollision();

                AttackTower();
                AttackTraps();

                TowerHealthBar.Update(new Vector2(90, 19), Tower.CurrentHP);

                foreach (Emitter emitter in EmitterList)
                {
                    emitter.Update(gameTime);
                }

                for (int i = 0; i < EmitterList.Count; i++)
                {
                    if (EmitterList[i].ParticleList.Count == 0 && EmitterList[i].Active == false)
                        EmitterList.RemoveAt(i);
                }

                foreach (Particle particle in ParticleList)
                {
                    particle.Update();
                }

                for (int i = 0; i < HeavyProjectileList.Count; i++)
                {
                    if (HeavyProjectileList[i].Active == false && HeavyProjectileList[i].Emitter.ParticleList.Count == 0)
                        HeavyProjectileList.RemoveAt(i);
                }

                for (int i = 0; i < ParticleList.Count; i++)
                {
                    if (ParticleList[i].Active == false)
                        ParticleList.RemoveAt(i);
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
                            && PreviousMouseState.LeftButton == ButtonState.Pressed)
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

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SkyBlue);

            #region Main Spritebatch
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Camera.Transformation(GraphicsDevice));
            
                if (GameState == GameState.Playing || GameState == GameState.Paused && IsLoading == false)
                {                  
                    MoveInvaders();                             

                    #region Background stuff
                    Tower.Draw(spriteBatch);
                    #endregion

                    foreach (Button towerSlot in TowerButtonList)
                    {
                        towerSlot.Draw(spriteBatch);
                    }

                    #region Draw Traps, Invaders and Turrets
                    foreach (Invader invader in InvaderList)
                    {
                        invader.Draw(spriteBatch);
                    }

                    foreach (Trap trap in TrapList)
                    {
                        trap.Draw(spriteBatch);
                    }
                
                    foreach (Turret turret in TurretList)
                    {
                        if (turret.Active == true)
                            turret.Draw(spriteBatch);
                    }
                    #endregion

                    foreach (HeavyProjectile heavyProjectile in HeavyProjectileList)
                    {
                        heavyProjectile.Draw(spriteBatch);
                    }

                    

                    Ground.Draw(spriteBatch);

                    foreach (Trap Trap in TrapList)
                    {
                        if (Trap.DetonateDelay > 0)
                        Trap.TimingBar.Draw(spriteBatch);
                    }

                    //This draws the timing bar for the turrets, but also makes 
                    //sure that it doesn't draw for the blank turrets, which
                    //would make the timing bar appear in the top right corner
                    foreach (Turret turret in TurretList)
                    {
                        if (turret.TurretType != TurretType.Blank)
                        turret.TimingBar.Draw(spriteBatch);
                    }

                    foreach (Particle particle in ParticleList)
                    {
                        particle.Draw(spriteBatch);
                    }                    
                }            

            spriteBatch.End();

            #endregion
            
            #region Secondary Spritebatch
                spriteBatch2.Begin();            

                if (GameState == GameState.Playing || GameState == GameState.Paused && IsLoading == false)
                {
                    //These are drawn here to make sure that they aren't shaken when the main camear shakes
                    CloudList[1].Draw(spriteBatch2);
                    CloudList[2].Draw(spriteBatch2);

                    #region Drawing buttons
                    spriteBatch2.Draw(BackgroundTexture, DestinationRectangle, Color.White);

                    foreach (Button button in SelectButtonList)
                    {
                        button.Draw(spriteBatch2);
                    }
                    #endregion

                    CloudList[0].Draw(spriteBatch2);
                    CloudList[3].Draw(spriteBatch2);

                    foreach (Emitter emitter in EmitterList)
                    {
                        emitter.Draw(spriteBatch2);
                    }
                }

                #region Draw Main Menu
                if (GameState == GameState.Menu)
                {
                    foreach (Button button in MainMenuButtonList)
                    {
                        button.Draw(spriteBatch2);
                    }
                }
                #endregion

                #region Draw Profile Select Menu
                if (GameState == GameState.ProfileSelect)
                {
                    foreach (Button button in ProfileButtonList)
                    {
                        button.Draw(spriteBatch2);
                    }
                }
                #endregion

                #region Draw Loading Screen
                if (GameState == GameState.Loading)
                {
                    spriteBatch2.Draw(LoadingScreenBackground, new Rectangle(0, 0, 1280, 720), Color.White);
                }
                #endregion

                #region Draw HUD
                if (slow == true)
                    spriteBatch2.DrawString(ResourceFont, "Slow: " + slow.ToString(), new Vector2(0, 64), Color.Red);
            
                if (GameState == GameState.Playing || GameState == GameState.Paused && IsLoading == false)
                {
                    HUDTest.Draw(spriteBatch2);
                    TowerHealthBar.Draw(spriteBatch2);

                    spriteBatch2.DrawString(ResourceFont, Resources.ToString(), new Vector2(102, 45), Color.White);

                    double PercentageHP;

                    PercentageHP = (100d / (double)Tower.MaxHP) * (double)Tower.CurrentHP;

                    spriteBatch2.DrawString(ResourceFont, ((int)PercentageHP).ToString() + "%", new Vector2(180, 16), Color.White);
                }
                #endregion

                #region Draw Pause Menu
                if (GameState == GameState.Paused)
                {
                    spriteBatch2.Draw(PauseMenuBackground, new Rectangle(0, 0, PauseMenuBackground.Width, PauseMenuBackground.Height), Color.White);

                    foreach (Button button in PauseButtonList)
                    {
                        button.Draw(spriteBatch2);                    
                    }
                }
                #endregion

                #region Draw Cursor
                if (GameState != GameState.Loading)
                {
                    CursorDraw();
                }
                #endregion

                

            spriteBatch2.End();
            #endregion

            base.Draw(gameTime);
        }


        //This is called as a seperate thread so that the 
        //loading screen can be displayed while the content is being loaded
        private void LoadGameContent()
        {
            if (GameState == GameState.Loading && IsLoading == false)
            {
                IsLoading = true;

                ReadyToPlace = false;

                GroundPosition = new Vector2(0, 560);

                BackgroundAssetName = "UI";
                SelectButtonAssetName = "Button";
                TowerSlotAssetName = "TrapButton";

                #region IconNameList, PauseMenuNameList;
                //This gets the names of the icons that are to appear on the
                //buttons that allow the player to select traps/turrets they want to place
                IconNameList = new List<string>();
                IconNameList.Add("WallIcon");
                IconNameList.Add("SpikesIcon");
                IconNameList.Add("FireIcon");
                IconNameList.Add("BasicTurretIcon");
                IconNameList.Add("BasicTurretIcon");
                IconNameList.Add(null);
                IconNameList.Add(null);
                IconNameList.Add(null);

                PauseMenuNameList = new List<string>();
                PauseMenuNameList.Add("Resume Game");
                PauseMenuNameList.Add("Options");
                PauseMenuNameList.Add("Main Menu");
                PauseMenuNameList.Add("Exit");
                #endregion

                //TrapCursor = Content.Load<Texture2D>("FireIcon");
                CrosshairCursor = Content.Load<Texture2D>("Crosshair");
                FireCursor = Content.Load<Texture2D>("FireIcon");
                WallCursor = Content.Load<Texture2D>("WallIcon");
                SpikesCursor = Content.Load<Texture2D>("SpikesIcon");
                TurretCursor = Content.Load<Texture2D>("BasicTurretIcon");

                CloudList = new List<StaticSprite>();
                CloudList.Add(new StaticSprite("Cloud1", new Vector2(0, 32), null, null, new Vector2(1, 0), true, false, 40));
                CloudList.Add(new StaticSprite("Cloud2", new Vector2(300, 26), new Vector2(1.5f, 1.5f), null, new Vector2(1, 0), true, false, 25));
                CloudList.Add(new StaticSprite("Cloud3", new Vector2(800, 16), new Vector2(1.5f, 1.5f), null, new Vector2(1, 0), true, false, 35));
                CloudList.Add(new StaticSprite("Cloud4", new Vector2(900, 0), null, null, new Vector2(1, 0), true, false, 28));

                Tower = new Tower("Tower", new Vector2(32, 304 - 65), 500);
                Ground = new StaticSprite("Ground", new Vector2(0, 495));
                Mountains = new StaticSprite("background3", new Vector2(0, 0));
                HUDTest = new StaticSprite("TestHUD1", new Vector2(8, 8));

                ParticleList = new List<Particle>();
                Random = new Random();
                
                Resources = 500;

                TowerHealthBar = new HorizontalBar(Content, new Vector2(220, 20), Tower.MaxHP, Tower.CurrentHP);                
                ShellCasing = Content.Load<Texture2D>("shell");
                ResourceFont = Content.Load<SpriteFont>("ResourceFont");
                Tower.LoadContent(Content);
                Ground.LoadContent(Content);
                PauseMenuBackground = Content.Load<Texture2D>("PauseMenuBackground");
                BackgroundTexture = Content.Load<Texture2D>(BackgroundAssetName);
                DestinationRectangle = new Rectangle((int)GroundPosition.X, (int)GroundPosition.Y, BackgroundTexture.Width, BackgroundTexture.Height);
                HUDTest.LoadContent(Content);

                foreach (StaticSprite cloud in CloudList)
                {
                    cloud.LoadContent(Content);
                }                

                #region Setting up the buttons                            
                SelectButtonList = new List<Button>();
                TowerButtonList = new List<Button>();
                PauseButtonList = new List<Button>();

                for (int i = 0; i < TowerButtons; i++)
                {
                    TowerButtonList.Add(new Button(TowerSlotAssetName, new Vector2(48 + 64 + 32 + 8, 272 + ((38 + 90) * i) - 65)));
                    TowerButtonList[i].LoadContent(Content);
                }

                for (int i = 0; i < 8; i++)
                {
                    SelectButtonList.Add(new Button(SelectButtonAssetName, new Vector2(16 + (i * 160), GroundPosition.Y + 16), IconNameList[i]));
                    SelectButtonList[i].LoadContent(Content);
                }

                for (int i = 0; i < 4; i++)
                {
                    PauseButtonList.Add(new Button("MenuButtonStrip", new Vector2(640 - 105, 128 + (i * 128)), null, null, null, PauseMenuNameList[i], "MenuFont", null));
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
                for (int i = 0; i < 30; i++)
                {
                    InvaderList.Add(new Soldier(new Vector2(1240 + (46 * i), 720 - 160 - 32 - 65)));
                    InvaderList[i].LoadContent(Content);
                }

                HeavyProjectileList = new List<HeavyProjectile>();
                LightProjectileList = new List<LightProjectile>();
                EmitterList = new List<Emitter>();
                #endregion

                IsLoading = false;

                GameState = GameState.Playing;                
            }
        }

        //This is called when the player exits to the main menu
        //It unloads the content from the ContentManager which has loaded
        //all the in game content, but not the menu content
        private void UnloadGameContent()
        {
            Content.Unload();
        }

       
        //These handle button presses and are checked on Update
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
                        case TurretType.Basic:

                            if (Resources >= 100)
                            {
                                TowerButtonList[Index].ButtonActive = false;
                                TurretList[Index] = new BasicTurret(TowerButtonList[Index].Position); //Fix this to make sure that the BasicTurret has the resource names built in.
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

                    switch (Index)
                    {
                        case 0:
                            SelectedTrap = TrapType.Wall;
                            SelectedTurret = TurretType.Blank;
                            ReadyToPlace = true;
                            break;

                        case 1:
                            SelectedTrap = TrapType.Spikes;
                            SelectedTurret = TurretType.Blank;
                            ReadyToPlace = true;
                            break;

                        case 2:
                            SelectedTrap = TrapType.Fire;
                            SelectedTurret = TurretType.Blank;
                            ReadyToPlace = true;
                            break;

                        case 3:
                            SelectedTurret = TurretType.Basic;
                            SelectedTrap = TrapType.Blank;
                            ReadyToPlace = true;
                            break;

                        case 4:
                            SelectedTurret = TurretType.Cannon;
                            SelectedTrap = TrapType.Blank;
                            ReadyToPlace = true;
                            break;

                        case 5:
                            SelectedTurret = TurretType.FlameThrower;
                            SelectedTrap = TrapType.Blank;
                            ReadyToPlace = true;
                            break;

                        case 6:
                            SelectedTurret = TurretType.Blank;
                            SelectedTrap = TrapType.Catapult;
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
                                GameState = GameState.Loading;
                                LoadingThread = new Thread(LoadGameContent);
                                LoadingThread.Start();
                                IsLoading = false;
                                break;

                            case 1:
                                GameState = GameState.ProfileSelect;
                                break;

                            case 2:

                                break;

                            case 3:
                                this.Exit();
                                break;

                        }
                    }
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

            #region Handling Profile Menu Button Presses
            if (GameState == GameState.ProfileSelect)
            {
                int Index;

                foreach (Button button in ProfileButtonList)
                {
                    button.Update();

                    if (button.JustClicked == true)
                    {
                        Index = ProfileButtonList.IndexOf(button);

                        HandleProfile(Index);
                    }
                }
            }
            #endregion
        }


        //Invader stuff that needs to be done every step
        private void InvaderUpdate(GameTime gameTime)
        {
            //This does all the stuff that would normally be in the Update call, but because this 
            //class became rather unwieldy, I broke up each call into separate smaller ones 
            //so that it's easier to manage
            foreach (Invader invader in InvaderList)
            {
                invader.Update(gameTime);

                if (invader.CurrentHP <= 0)
                {
                    EmitterList.Add(new Emitter("Splodge", new Vector2(invader.Position.X, invader.DestinationRectangle.Center.Y),
                        new Vector2(0, 360), new Vector2(1, 2), new Vector2(50, 100), 0.5f, true, new Vector2(0, 360), new Vector2(1, 3),
                        new Vector2(0.02f, 0.06f), Color.DarkRed, Color.Red, 0.1f, 0.2f, 20, 10, true));
                    EmitterList[EmitterList.Count - 1].LoadContent(Content);
                }
            }

            for (int i = 0; i < InvaderList.Count; i++)
            {
                if (InvaderList[i].CurrentHP <= 0)
                {                    
                    InvaderList.RemoveAt(i);
                }
            }            

            foreach (Invader invader in InvaderList)
            {
                if ((invader.DestinationRectangle.Bottom + invader.Velocity.Y) > 495)
                {
                    invader.Trajectory(Vector2.Zero);
                    invader.Position = new Vector2(invader.Position.X, 495 - invader.DestinationRectangle.Height);
                    invader.Gravity = 0;
                }

                if (invader.DestinationRectangle.Bottom < 495)
                {
                    invader.Gravity = 0.2f;
                }
            }
        }

        private void MoveInvaders()
        {
            foreach (Invader invader in InvaderList)
            {
                if (invader.CanMove == true)
                    invader.Move();
            }
        }

        private void AttackTower()
        {
            foreach (Invader invader in InvaderList)
            {
                if ((invader.Position.X - Tower.DestinationRectangle.Right) < 4)
                {
                    invader.CanMove = false;
                    if (invader.CanAttack == true)
                    {
                        Tower.CurrentHP -= invader.AttackPower;
                    }
                }

                if ((invader.Position.X - Tower.DestinationRectangle.Right) > 3)
                {
                    invader.CanMove = true;
                }
            }            
        }

        private void AttackTraps()
        {
            foreach (Invader invader in InvaderList)
            {
                foreach (Trap trap in TrapList)
                {
                    if (invader.DestinationRectangle.Left < trap.DestinationRectangle.Right 
                        && invader.DestinationRectangle.Right > trap.DestinationRectangle.Left
                        && invader.DestinationRectangle.Bottom > trap.DestinationRectangle.Top
                        && trap.TrapType == TrapType.Wall)
                    {
                        invader.CanMove = false;
                    }
                }
            }
        }


        //Turret stuff that needs to be called every step
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
                if (turret.Active == true && CursorPosition.Y < GroundPosition.Y)
                {

                    if (turret.Selected == true)
                    {
                        turret.Random = new Random();
                        Vector2 MousePosition, Direction;

                        CurrentMouseState = Mouse.GetState();
                        MousePosition = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);

                        Direction = turret.FireDirection;
                        Direction.Normalize();

                        switch (turret.TurretType)
                        {
                            case TurretType.Basic:
                                {
                                    CurrentProjectile = new LightProjectile(new Vector2(turret.BarrelRectangle.X, 
                                        turret.BarrelRectangle.Y), Direction);
                                    LightProjectileList.Add(CurrentProjectile);
                                    ParticleList.Add(new Particle(ShellCasing,
                                        new Vector2(turret.BarrelRectangle.X, turret.BarrelRectangle.Y), 
                                        turret.Rotation - MathHelper.ToRadians((float)DoubleRange(175, 185)), 
                                        (float)DoubleRange(2, 4), 500, 1f, false, (float)DoubleRange(-10, 10), 
                                        (float)DoubleRange(-3, 6), 1, Color.White, Color.White, 0.09f, true));
                                }
                                break;

                            case TurretType.Cannon:
                                {
                                    HeavyProjectile heavyProjectile;
                                    Vector2 BarrelEnd;

                                    BarrelEnd = new Vector2((float)Math.Cos(turret.Rotation) * (turret.BarrelRectangle.Width - 20),
                                                            (float)Math.Sin(turret.Rotation) * (turret.BarrelRectangle.Height + 20));

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

                                    heavyProjectile = new FlameProjectile(new Vector2(turret.BarrelRectangle.X + BarrelEnd.X, 
                                        turret.BarrelRectangle.Y + BarrelEnd.Y), (float)DoubleRange(7,9), turret.Rotation, 0.3f);
                                    heavyProjectile.LoadContent(Content);
                                    HeavyProjectileList.Add(heavyProjectile);
                                }
                                break;
                        }
                    }
                }
            }
        }

        private void ProjectileUpdate(GameTime gameTime)
        {
            foreach (HeavyProjectile heavyProjectile in HeavyProjectileList)
            {
                heavyProjectile.Update(gameTime);

                if (TrapList.Any(Trap => Trap.DestinationRectangle.Left - 16 < heavyProjectile.Position.X
                    && Trap.DestinationRectangle.Top < heavyProjectile.Position.Y
                    && Trap.DestinationRectangle.Right - 16 > heavyProjectile.Position.X && Trap.TrapType == TrapType.Wall))
                {
                    if (heavyProjectile.Active == true)
                    {
                        heavyProjectile.Active = false;

                        Emitter newEmitter = new Emitter("Splodge", new Vector2(heavyProjectile.Position.X+4, heavyProjectile.Position.Y),
                        new Vector2(90,270 ), new Vector2(2, 3), new Vector2(25, 50), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                        new Vector2(0.03f, 0.09f), Color.SlateGray, Color.SlateGray, 0.2f, 0.05f, 20, 10, true);

                        EmitterList.Add(newEmitter);

                        EmitterList[EmitterList.IndexOf(newEmitter)].LoadContent(Content);
                        
                        heavyProjectile.Emitter.HPRange = new Vector2(0, 0);
                    }
                }

                if (heavyProjectile.Position.Y > 482)
                {
                    if (heavyProjectile.Active == true && heavyProjectile.HeavyProjectileType == HeavyProjectileType.CannonBall)
                    {
                        Color FireColor = new Color();
                        FireColor.A = 150;
                        FireColor.R = 51;
                        FireColor.G = 31;
                        FireColor.B = 0;

                        Color FireColor2 = FireColor;                        
                        FireColor2.A = 125;

                        Vector2 HeavyProjectileCollision = new Vector2(heavyProjectile.Position.X + 16, 492);

                        foreach (Invader invader in InvaderList)
                        {
                            if (Vector2.Distance(invader.Position, HeavyProjectileCollision) <= 72)
                            {
                                float DistanceToCollision = Vector2.Distance(HeavyProjectileCollision, invader.Position);
                                invader.CurrentHP -= (int)((2/DistanceToCollision)*(32*20));
                            }
                        }

                        Emitter newEmitter = new Emitter("Splodge", new Vector2(heavyProjectile.Position.X + 16, 492),
                        new Vector2(0, 180), new Vector2(2, 3), new Vector2(25, 50), 2f, true, new Vector2(0, 360), new Vector2(1, 3),
                        new Vector2(0.03f, 0.09f), FireColor, FireColor2, 0.2f, 0.2f, 20, 10, true);
                        EmitterList.Add(newEmitter);
                        EmitterList[EmitterList.IndexOf(newEmitter)].LoadContent(Content);
                        heavyProjectile.Active = false;
                    }

                    if (heavyProjectile.Active == true && heavyProjectile.HeavyProjectileType == HeavyProjectileType.FlameThrower)
                    {
                        Color FireColor = Color.Black;

                        Color FireColor2 = Color.DarkGray;

                        Emitter newEmitter = new Emitter("smoke", new Vector2(heavyProjectile.Position.X + 16,
                            heavyProjectile.Position.Y + 16), new Vector2(20, 160), new Vector2(0.5f, 1), new Vector2(10, 12), 0.25f, false,
                            new Vector2(-20, 20), new Vector2(-4, 4), new Vector2(0.5f, 0.75f), FireColor, FireColor2, -0.2f, 0.5f, 3, 1, false);
                        EmitterList.Add(newEmitter);                        
                        EmitterList[EmitterList.IndexOf(newEmitter)].LoadContent(Content);
                        heavyProjectile.Active = false;
                    }
                }
            }

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
                                    CurrentProjectile.Active = false;
                                    HitInvader.TurretDamage(-turret1.Damage);
                                    if (HitInvader.CurrentHP <= 0)
                                        Resources += HitInvader.ResourceValue;
                                    return;
                                }

                                if (CurrentProjectile.Ray.Intersects(HitInvader.BoundingBox) > CurrentProjectile.Ray.Intersects(
                                    HitTrap.BoundingBox) && HitInvader.Active == true && HitTrap.TrapType != TrapType.Wall)
                                {
                                    CurrentProjectile.Active = false;
                                    HitInvader.TurretDamage(-turret1.Damage);
                                    if (HitInvader.CurrentHP <= 0)
                                        Resources += HitInvader.ResourceValue;
                                    return;
                                }
                            }

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
                                    CurrentProjectile.Active = false;
                                    HitInvader.TurretDamage(-turret1.Damage);
                                    if (HitInvader.CurrentHP <= 0)
                                        Resources += HitInvader.ResourceValue;
                                    return;
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
                                        GroundCollisionPoint.Y = Ground.Position.Y;

                                        Value1 = Math.Pow(Distance.Value, 2); //This is the hypoteneuse

                                        Value2 = Math.Pow(Ground.Position.Y - turret.BarrelRectangle.Y, 2); //This is the height to the turret

                                        if (CurrentMouseState.X > turret.BarrelRectangle.X)
                                        {
                                            GroundCollisionPoint.X = (turret.BarrelRectangle.X + (float)Math.Sqrt(Value1 - Value2)) - 16;
                                        }
                                        else
                                        {
                                            GroundCollisionPoint.X = turret.BarrelRectangle.X - (float)Math.Sqrt(Value1 - Value2) - 16;
                                        }

                                        if (turret.Selected == true && turret.CanShoot == false 
                                            && CurrentMouseState.LeftButton == ButtonState.Pressed 
                                            && turret.TurretType == TurretType.Basic)
                                        {
                                            Color DirtColor = new Color();
                                            DirtColor.A = 100;
                                            DirtColor.R = 51;
                                            DirtColor.G = 31;
                                            DirtColor.B = 0;

                                            Color DirtColor2 = DirtColor;
                                            DirtColor2.A = 125;

                                            DirtEmitter = new Emitter("smoke", new Vector2(GroundCollisionPoint.X, GroundCollisionPoint.Y), new Vector2(90, 90), new Vector2(0.5f, 1f), new Vector2(20, 30), 1f, true, new Vector2(0, 0), new Vector2(0, 2), new Vector2(0.5f, 1f), DirtColor, DirtColor2, 0f, 0.01f, 1, 1, false);

                                            EmitterList.Add(DirtEmitter);
                                            EmitterList[EmitterList.IndexOf(DirtEmitter)].LoadContent(Content);
                                        }
                                    }
                                }
                            }
                        }
            }
        }


        //Trap stuff that needs to be called every step
        private void TrapPlacement()
        {
            if (CurrentMouseState.LeftButton == ButtonState.Pressed &&
                PreviousMouseState.RightButton == ButtonState.Released &&
                ReadyToPlace == true &&
                TrapList.Count < TrapLimit &&
                CursorPosition.X > (Tower.Position.X + Tower.Texture.Width) &&
                CursorPosition.Y < Ground.Position.Y)
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
                            Trap NewTrap = new Wall(new Vector2(CursorPosition.X - 16, Ground.Position.Y));
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
                            Trap NewTrap = new FireTrap(new Vector2(CursorPosition.X - 16, Ground.Position.Y));
                            TrapList.Add(NewTrap);

                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);

                            Color FireColor = Color.DarkOrange;
                            FireColor.A = 200;

                            Color FireColor2 = Color.DarkOrange;
                            FireColor2.A = 90;


                            Color SmokeColor = Color.Gray;
                            SmokeColor.A = 200;

                            Color SmokeColor2 = Color.WhiteSmoke;
                            SmokeColor.A = 170;

                            Emitter newEmitter = new Emitter("star", new Vector2(NewTrap.Position.X + 16, NewTrap.Position.Y + 4),
                                new Vector2(70, 110), new Vector2(0.5f, 0.75f), new Vector2(40, 60), 0.01f, true, new Vector2(-20, 20),
                                new Vector2(-4, 4), new Vector2(1, 2f), FireColor, FireColor2, 0.0f, -1, 8, 1, false);
                            NewTrap.TrapEmitterList.Add(newEmitter);

                            Emitter newEmitter2 = new Emitter("smoke", new Vector2(NewTrap.Position.X + 16, NewTrap.Position.Y + 4),
                                new Vector2(70, 110), new Vector2(0.2f, 0.5f), new Vector2(250, 350), 1f, true, new Vector2(-20, 20),
                                new Vector2(-4, 4), new Vector2(0.5f, 0.5f), SmokeColor, SmokeColor2, 0.0f, -1, 300, 1, false);                            

                            NewTrap.TrapEmitterList.Add(newEmitter2);

                            NewTrap.TrapEmitterList[NewTrap.TrapEmitterList.IndexOf(newEmitter2)].LoadContent(Content);

                            NewTrap.TrapEmitterList.Add(newEmitter);

                            NewTrap.TrapEmitterList[NewTrap.TrapEmitterList.IndexOf(newEmitter)].LoadContent(Content);

                            ReadyToPlace = false;
                            Resources -= 60;
                            ClearSelected();
                        }
                        break;

                    case TrapType.Spikes:
                        if (Resources >= 30)
                        {
                            Trap NewTrap = new SpikeTrap(new Vector2(CursorPosition.X - 16, Ground.Position.Y));
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
                            Trap NewTrap = new CatapultTrap(new Vector2(CursorPosition.X - 16, Ground.Position.Y));
                            TrapList.Add(NewTrap);
                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);
                            ReadyToPlace = false;
                            Resources -= 30;
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
            }            
        }

        private void TrapCollision()
        {
            foreach (Invader invader in InvaderList)
            {
                if (TrapList.Any(Trap => Trap.BoundingBox.Intersects(invader.BoundingBox)))
                {
                    Trap HitTrap = TrapList.First(Trap => Trap.BoundingBox.Intersects(invader.BoundingBox));
                    if (TrapList[TrapList.IndexOf(HitTrap)].CanTrigger == true)
                    {
                        invader.TrapDamage(HitTrap.TrapType);
                        HitTrap.CurrentDetonateDelay = 0;
                        //invader.VulnerableToTrap = false;
                    }
                }

                if (TrapList.All(Trap => !Trap.BoundingBox.Intersects(invader.BoundingBox)))
                {
                    invader.VulnerableToTrap = true;
                }
            }
        }        


        //Draw the correct cursor
        private void CursorDraw()
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
                }

                switch (SelectedTurret)
                {
                    case TurretType.Blank:
                        if (SelectedTrap == TrapType.Blank)
                        CurrentCursorTexture = BlankTexture;
                        break;

                    case TurretType.Basic:
                        CurrentCursorTexture = TurretCursor;
                        break;

                    case TurretType.Cannon:
                        CurrentCursorTexture = TurretCursor;
                        break;

                    case TurretType.FlameThrower:
                        CurrentCursorTexture = TurretCursor;
                        break;
                }
            }


            if (GameState != GameState.Loading)
            {
                spriteBatch2.Draw(CurrentCursorTexture, new Rectangle((int)CursorPosition.X - (CurrentCursorTexture.Width / 2),
                    (int)CursorPosition.Y - CurrentCursorTexture.Height, CurrentCursorTexture.Width, CurrentCursorTexture.Height),
                    Color.White);

                if (PrimaryCursorTexture != CrosshairCursor)
                    spriteBatch2.Draw(PrimaryCursorTexture, new Rectangle((int)CursorPosition.X, (int)CursorPosition.Y,
                            PrimaryCursorTexture.Width, PrimaryCursorTexture.Height), Color.White);
                else
                    spriteBatch2.Draw(PrimaryCursorTexture, new Rectangle((int)CursorPosition.X-PrimaryCursorTexture.Width/2, (int)CursorPosition.Y-PrimaryCursorTexture.Height/2,
                        PrimaryCursorTexture.Width, PrimaryCursorTexture.Height), Color.White);    
            }
        }        


        //Generates a random double number
        public double DoubleRange(double one, double two)
        {
            Random rand = new Random();
            return one + Random.NextDouble() * (two - one);
        }


        //Various functions to clear current selections
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


        //Handling player profile data//

        public void HandleProfile(int profileNumber)
        {
            //Check if file Profile+ProfileNumber exists
            //If it does, the load the profile and resume the game
            //If it does not, get a profilename from the user
            //And set the default player values in the profile
            //Start the game with default beginning values            
        }

        public void SaveProfile()
        {

        }

        public void LoadProfile()
        {
            
        }
    }
}
