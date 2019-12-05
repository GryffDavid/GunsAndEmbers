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

namespace TowerDefensePrototype
{
    public enum TrapType { Blank, Wall, Spikes, Catapult, Fire };
    public enum TurretType { Blank, Basic, Cannon };
    public enum CursorType { Default, Crosshair };

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //XNA Declarations
        Texture2D BackgroundTexture, PrimaryCursorTexture, SecondaryCursorTexture;
        Vector2 Position, CursorPosition;
        Rectangle DestinationRectangle;
        SpriteFont ResourceFont;
        MouseState CurrentMouseState, PreviousMouseState;
        KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        int Resources;
        string BackgroundAssetName, SelectButtonAssetName, TowerSlotAssetName;
        bool ReadyToPlace;

        //List declarations
        List<Button> SelectButtonList;
        List<Button> TowerButtonList;

        List<Trap> TrapList;
        List<Turret> TurretList;
        List<Invader> InvaderList;
        List<string> IconNameList;

        Projectile CurrentProjectile;

        Tower Tower;
        StaticSprite Ground, Mountains;
        TrapType SelectedTrap;
        TurretType SelectedTurret;
        CursorType CurrentCursor;

        Texture2D Fire, ShellCasing, Line;
        List<Emitter> EmitterList;
        List<Particle> ParticleList;
        float LineRotation;
        Random Random;

        HorizontalBar TowerHealthBar;
        StaticSprite HUDTest;

        bool slow;

        //int Rounds = 0;
        int TrapLimit = 8;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            //IsMouseVisible = true;
            //graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
        }


        protected override void Initialize()
        {            
            //this.IsFixedTimeStep = false;
            
            ParticleList = new List<Particle>();
            Random = new Random();            

            int towerButtons = 3;
            Resources = 500;

            ReadyToPlace = false;

            CurrentProjectile = new Projectile(Vector2.Zero, Vector2.Zero);

            Tower = new Tower("Tower", new Vector2(32, 304 - 65), 500);
            Ground = new StaticSprite("Ground", new Vector2(0, 720 - 160 - 65));
            Mountains = new StaticSprite("MountainsBackground", new Vector2(0, 200));

            TowerHealthBar = new HorizontalBar(Content, new Vector2(220, 20), Tower.MaxHP, Tower.CurrentHP);
            HUDTest = new StaticSprite("TestHUD1", new Vector2(8, 8));

            #region IconNameList
            //This gets the names of the icons that are to appear on the
            //buttons that allow the player to select traps/turrets they want to place
            IconNameList = new List<string>();
            IconNameList.Add("WallIcon");
            IconNameList.Add("SpikesIcon");
            IconNameList.Add("FireIcon");
            IconNameList.Add("BasicTurretIcon");
            IconNameList.Add(null);
            IconNameList.Add(null);
            IconNameList.Add(null);
            IconNameList.Add(null);
            #endregion

            #region Setting up the buttons
            BackgroundAssetName = "UI";
            SelectButtonAssetName = "Button";
            //TrapSlotAssetName = "TrapButton";
            TowerSlotAssetName = "TrapButton";

            Position = new Vector2(0, 560);

            SelectButtonList = new List<Button>();
            TowerButtonList = new List<Button>();

            //TrapsButtonList = new List<Button>();

            //for (int i = 0; i < trapButtons; i++)
            //{
            //    TrapsButtonList.Add(new Button(TrapSlotAssetName, new Vector2((128 * i) + 256 + 64, Position.Y - 32 - 65)));
            //    TrapsButtonList[i].LoadContent(Content);
            //}

            for (int i = 0; i < towerButtons; i++)
            {
                TowerButtonList.Add(new Button(TowerSlotAssetName, new Vector2(48 + 64 + 32 + 8, 272 + ((38 + 90) * i) - 65)));
                TowerButtonList[i].LoadContent(Content);
            }

            for (int i = 0; i < 8; i++)
            {
                SelectButtonList.Add(new Button(SelectButtonAssetName, new Vector2(16 + (i * 160), Position.Y + 16), IconNameList[i]));
                SelectButtonList[i].LoadContent(Content);
            }
            #endregion

            #region List Creating Code
            //This code just creates the lists for the buttons and traps with the right number of possible slots
            TrapList = new List<Trap>();

            //for (int i = 0; i < trapButtons; i++)
            //{
            //    TrapList.Add(new BlankTrap());
            //    TrapList[i].Position = TrapsButtonList[i].Position;
            //    TrapList[i].LoadContent(Content);
            //}

            TurretList = new List<Turret>();
            for (int i = 0; i < towerButtons; i++)
            {
                TurretList.Add(new BlankTurret());
                TurretList[i].LoadContent(Content);
            }

            InvaderList = new List<Invader>();
            for (int i = 0; i < 1; i++)
            {
                InvaderList.Add(new Soldier(new Vector2(1260 + (46 * i), 720 - 160 - 32 - 65)));
                InvaderList[i].LoadContent(Content);
            }
            #endregion
            
            Fire = Content.Load<Texture2D>("star");
            ShellCasing = Content.Load<Texture2D>("shell");

            EmitterList = new List<Emitter>();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ResourceFont = Content.Load<SpriteFont>("ResourceFont");
            Tower.LoadContent(Content);
            Ground.LoadContent(Content);
            Mountains.LoadContent(Content);
            Line = Content.Load<Texture2D>("Line");

            BackgroundTexture = Content.Load<Texture2D>(BackgroundAssetName);
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, BackgroundTexture.Width, BackgroundTexture.Height);
            HUDTest.LoadContent(Content);
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            //This is just the stuff that needs to be updated every step
            //This is where I call all the smaller procedures that I broke the update into             
            CurrentMouseState = Mouse.GetState();
            CursorPosition = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);

            InvaderUpdate(gameTime);    

            RightClickClearSelected();

            SelectButtonsUpdate();

            TowerButtonUpdate();

            TrapPlacement();

            TrapUpdate(gameTime);

            ProjectileUpdate();

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
                if (EmitterList[i].Active == false)
                EmitterList.RemoveAt(i);
            }

            foreach (Particle particle in ParticleList)
            {
                particle.Update();
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
                    if (turret.Selected == true && CurrentMouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Pressed)
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

            PreviousMouseState = CurrentMouseState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SkyBlue);

            spriteBatch.Begin();

            HUDTest.Draw(spriteBatch);

            MoveInvaders();
            
            TowerHealthBar.Draw(spriteBatch);

            #region Background stuff
            Mountains.Draw(spriteBatch);            
            Tower.Draw(spriteBatch);            
            #endregion

            #region Drawing buttons
            spriteBatch.Draw(BackgroundTexture, DestinationRectangle, Color.White);

            foreach (Button button in SelectButtonList)
            {
                button.Draw(spriteBatch);
            }

            foreach (Button towerSlot in TowerButtonList)
            {
                towerSlot.Draw(spriteBatch);
            }
            #endregion

            #region Draw Traps, Invaders and Turrets
            foreach (Trap trap in TrapList)
            {
                trap.Draw(spriteBatch);
            }

            foreach (Invader invader in InvaderList)
            {
                invader.Draw(spriteBatch);
            }

            foreach (Turret turret in TurretList)
            {
                if (turret.Active == true)
                    turret.Draw(spriteBatch);
            }
            #endregion

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Draw(spriteBatch);
            }

            Ground.Draw(spriteBatch);

            spriteBatch.DrawString(ResourceFont, Resources.ToString(), new Vector2(102, 45), Color.White);
            //spriteBatch.DrawString(ResourceFont, "Rounds: " + Rounds.ToString(), new Vector2(0, 32), Color.White);

            double PercentageHP;

            PercentageHP = (100d / (double)Tower.MaxHP) * (double)Tower.CurrentHP;

            spriteBatch.DrawString(ResourceFont, ((int)PercentageHP).ToString() + "%", new Vector2(180, 16), Color.White);

            if (slow == true)
                spriteBatch.DrawString(ResourceFont, "Slow: " + slow.ToString(), new Vector2(0, 64), Color.Red);


            foreach (Particle particle in ParticleList)
            {
                particle.Draw(spriteBatch);
            }

            CursorDraw(spriteBatch);

            spriteBatch.Draw(Line, new Rectangle(TurretList[0].BarrelRectangle.X, TurretList[0].BarrelRectangle.Y, Line.Width, Line.Height), null, Color.White, LineRotation, Vector2.Zero, SpriteEffects.None, 0);

            spriteBatch.End();

            base.Draw(gameTime);
        }
                

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
                                TurretList[Index] = new BasicTurret("BasicTurret", "BasicTurretBase", TowerButtonList[Index].Position); //Fix this to make sure that the BasicTurret has the resource names built in.
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
                                TurretList[Index] = new CannonTurret("BasicTurret", "BasicTurretBase", TowerButtonList[Index].Position);
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

            CurrentKeyboardState = Keyboard.GetState();

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
                    }
                }
            }

            //if (CurrentKeyboardState.IsKeyUp(Keys.D1) && PreviousKeyboardState.IsKeyDown(Keys.D1))
            //{
            //    SelectedTrap = TrapType.Wall;
            //    SelectedTurret = TurretType.Blank;
            //    ClearTurretSelect();
            //}

            //if (CurrentKeyboardState.IsKeyUp(Keys.D2) && PreviousKeyboardState.IsKeyDown(Keys.D2))
            //{
            //    SelectedTrap = TrapType.Spikes;
            //    SelectedTurret = TurretType.Blank;
            //    ClearTurretSelect();
            //}

            //if (CurrentKeyboardState.IsKeyUp(Keys.D3) && PreviousKeyboardState.IsKeyDown(Keys.D3))
            //{
            //    SelectedTrap = TrapType.Fire;
            //    SelectedTurret = TurretType.Blank;
            //    ClearTurretSelect();
            //}

            //if (CurrentKeyboardState.IsKeyUp(Keys.D4) && PreviousKeyboardState.IsKeyDown(Keys.D4))
            //{
            //    SelectedTurret = TurretType.Basic;
            //    SelectedTrap = TrapType.Blank;
            //    ClearTurretSelect();
            //}

            PreviousKeyboardState = CurrentKeyboardState;
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
            }

            for (int i = 0; i < InvaderList.Count; i++)
            {
                if (InvaderList[i].CurrentHP <= 0)
                {
                    InvaderList.RemoveAt(i);
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
                        Tower.CurrentHP -= 4;
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
                    if ((invader.Position.X - (trap.DestinationRectangle.Right)) < 4 && trap.TrapType == TrapType.Wall)
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
            CurrentProjectile.Active = false;
            
            foreach (Turret turret in TurretList)
            {
                if (turret.Active == true && CursorPosition.Y < Position.Y)
                    if (turret.Selected == true)
                    {
                        turret.Random = new Random();
                        Vector2 MousePosition, Direction;

                        CurrentMouseState = Mouse.GetState();
                        MousePosition = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);

                        //Direction = MousePosition - new Vector2(turret.BarrelRectangle.X, turret.BarrelRectangle.Y);
                        Direction = turret.FireDirection;
                        Direction.Normalize();

                        LineRotation = turret.FireRotation;
                        CurrentProjectile = new Projectile(new Vector2(turret.BarrelRectangle.X, turret.BarrelRectangle.Y), new Vector2(Direction.X, Direction.Y));

                        ParticleList.Add(new Particle(ShellCasing, turret.Position, turret.Rotation - MathHelper.ToRadians((float)DoubleRange(175, 185)), (float)DoubleRange(2, 4), 500, 1f, false, (float)DoubleRange(-10, 10), (float)DoubleRange(-3, 6), 1, Color.White, Color.White, 0.09f, true));
                    }
            }
        }

        private void ProjectileUpdate()
        {
            foreach (Turret turret1 in TurretList)
            {
                if (turret1.Active == true)
                    if (turret1.Selected == true)
                    {
                        //This checks if the ray hit ANY invader and ANY wall. If it hit both an invader and a wall (Doesn't matter which invader or which wall
                        //just that it hit them. Then it checks if the invader was closer to the origin of the ray. If it was, the invader is destroyed//
                        //IF the wall was hit before the invader, then it does nothing. 
                        if (InvaderList.Any(TestInvader => TestInvader.BoundingBox.Intersects(CurrentProjectile.Ray) != null) &&
                            TrapList.Any(TestTrap => TestTrap.BoundingBox.Intersects(CurrentProjectile.Ray) != null) &&
                            CurrentProjectile.Active == true)
                        {
                            Invader HitInvader = InvaderList.First(TestInvader => TestInvader.BoundingBox.Intersects(CurrentProjectile.Ray) != null);
                            Trap HitTrap = TrapList.First(TestTrap => TestTrap.BoundingBox.Intersects(CurrentProjectile.Ray) != null);

                            if (CurrentProjectile.Ray.Intersects(HitInvader.BoundingBox) < CurrentProjectile.Ray.Intersects(HitTrap.BoundingBox) && HitInvader.Active == true)
                            {
                                CurrentProjectile.Active = false;
                                HitInvader.TurretDamage(-turret1.Damage);
                                if (HitInvader.CurrentHP <= 0)
                                Resources += HitInvader.ResourceValue;
                                return;
                            }

                            if (CurrentProjectile.Ray.Intersects(HitInvader.BoundingBox) > CurrentProjectile.Ray.Intersects(HitTrap.BoundingBox) && HitInvader.Active == true && HitTrap.TrapType != TrapType.Wall)
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
                            Invader HitInvader = InvaderList.First(TestInvader => TestInvader.BoundingBox.Intersects(CurrentProjectile.Ray) != null);

                            if (HitInvader.Active == true)
                            {
                                CurrentProjectile.Active = false;
                                HitInvader.TurretDamage(-turret1.Damage);
                                if (HitInvader.CurrentHP <= 0)
                                    Resources += HitInvader.ResourceValue;
                                return;
                            }
                        }

                        //if (TrapList.Any(Trap => Trap.BoundingBox.Intersects(CurrentProjectile.Ray) != null && Trap.TrapType == TrapType.Blank) &&
                        //            InvaderList.All(Invader => Invader.BoundingBox.Intersects(CurrentProjectile.Ray) == null) &&
                        //            CurrentProjectile.Ray.Intersects(Ground.BoundingBox) != null && CurrentProjectile.Active == true)
                        //{

                        //}


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
                                foreach (Turret turret in TurretList)
                                {
                                    if (turret.Selected == true)
                                    {
                                        Vector2 newPos;
                                        double Value1, Value2;

                                        newPos.Y = Ground.Position.Y;

                                        Value1 = Math.Pow(Distance.Value, 2); //This is the hypoteneuse
                                        Value2 = Math.Pow(Ground.Position.Y - turret.BarrelRectangle.Y, 2); //This is the height to the turret

                                        if (CurrentMouseState.X > turret.BarrelRectangle.X)
                                        {
                                            newPos.X = (turret.BarrelRectangle.X + (float)Math.Sqrt(Value1 - Value2)) - 16;
                                        }
                                        else
                                        {
                                            newPos.X = turret.BarrelRectangle.X - (float)Math.Sqrt(Value1 - Value2) - 16;
                                        }

                                        if (CurrentMouseState.LeftButton == ButtonState.Pressed && turret.CanShoot == true)
                                        {
                                                                                 
                                        }
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
                            //Color FireColor = new Color(255, 204, 2, 175);
                            Color FireColor = Color.Orange;
                            FireColor.A = 100;

                            Color FireColor2 = Color.Orange;
                            FireColor2.A = 200;

                            Emitter newEmitter = new Emitter("star", new Vector2(NewTrap.Position.X + 16, NewTrap.Position.Y + 8), new Vector2(75, 105), new Vector2(1.5f, 2), new Vector2(30, 35), 0.2f, true, new Vector2(-20, 20), new Vector2(-4, 4), new Vector2(1, 2f), FireColor, FireColor2, 0.0f, -1);
                            EmitterList.Add(newEmitter);
                            EmitterList[EmitterList.IndexOf(newEmitter)].LoadContent(Content);

                            TrapList[TrapList.IndexOf(NewTrap)].LoadContent(Content);
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
                        //invader.VulnerableToTrap = false;
                    }
                }

                if (TrapList.All(Trap => !Trap.BoundingBox.Intersects(invader.BoundingBox)))
                {
                    invader.VulnerableToTrap = true;
                }
            }
        }


        private void CursorDraw(SpriteBatch spriteBatch)
        {
            if ((TurretList[0].Selected == true) || (TurretList[1].Selected == true) || (TurretList[2].Selected == true))
            {
                PrimaryCursorTexture = Content.Load<Texture2D>("TurretCrosshair");
                CurrentCursor = CursorType.Crosshair;
            }
            else
            {
                PrimaryCursorTexture = Content.Load<Texture2D>("DefaultCursor");
                CurrentCursor = CursorType.Default;
            }

            switch (SelectedTrap)
            {
                case TrapType.Blank:
                    switch (SelectedTurret)
                    {
                        case TurretType.Blank:
                            SecondaryCursorTexture = Content.Load<Texture2D>("Blank");
                            break;

                        case TurretType.Basic:
                            SecondaryCursorTexture = Content.Load<Texture2D>("BasicTurretIcon");
                            break;
                    }
                    break;

                case TrapType.Fire:
                    SecondaryCursorTexture = Content.Load<Texture2D>("FireIcon");
                    break;

                case TrapType.Spikes:
                    SecondaryCursorTexture = Content.Load<Texture2D>("SpikesIcon");
                    break;

                case TrapType.Wall:
                    SecondaryCursorTexture = Content.Load<Texture2D>("WallIcon");
                    break;
            }


            spriteBatch.Draw(SecondaryCursorTexture, new Rectangle((int)CursorPosition.X - (SecondaryCursorTexture.Width / 2), (int)CursorPosition.Y - SecondaryCursorTexture.Height, SecondaryCursorTexture.Width, SecondaryCursorTexture.Height), Color.White);

            if (CurrentCursor == CursorType.Default)
            {
                spriteBatch.Draw(PrimaryCursorTexture, new Rectangle((int)CursorPosition.X, (int)CursorPosition.Y, PrimaryCursorTexture.Width, PrimaryCursorTexture.Height), Color.White);
            }
            else
            {
                spriteBatch.Draw(PrimaryCursorTexture, new Rectangle((int)CursorPosition.X - (PrimaryCursorTexture.Width / 2), (int)CursorPosition.Y - (PrimaryCursorTexture.Height / 2), PrimaryCursorTexture.Width, PrimaryCursorTexture.Height), Color.White);
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

        public double DoubleRange(double one, double two)
        {
            Random rand = new Random();
            return one + Random.NextDouble() * (two - one);
        }
    }
}
