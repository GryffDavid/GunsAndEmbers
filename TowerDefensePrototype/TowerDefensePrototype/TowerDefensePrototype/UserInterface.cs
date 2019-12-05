using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TowerDefensePrototype
{
    class UserInterface
    {
        //XNA Declarations
        ContentManager ContentManager;
        Texture2D BackgroundTexture, PrimaryCursorTexture, SecondaryCursorTexture;
        Vector2 Position, CursorPosition;
        Rectangle DestinationRectangle;
        SpriteFont ResourceFont;
        MouseState CurrentMouseState, PreviousMouseState;
        KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        int Resources;
        string BackgroundAssetName, SelectButtonAssetName, TrapSlotAssetName, TowerSlotAssetName;

        //List declarations
        List<Button> SelectButtonList;
        List<Button> TowerButtonList;
        List<Button> TrapsButtonList;
        List<Trap> TrapList;
        List<Turret> TurretList;
        List<Invader> InvaderList;
        List<string> IconNameList;
        List<Projectile> ProjectileList;
        
        Tower Tower;
        StaticSprite Ground;
        TrapType SelectedTrap;
        TurretType SelectedTurret;
        CursorType CurrentCursor;

        //GameTime GameTime;

        StaticSprite Marker;

        int Rounds = 0;

        //Basic XNA calls
        public UserInterface(int trapButtons, int towerButtons, int resources, ContentManager contentManager)
        {           
            Marker = new StaticSprite("WhiteBlock", new Vector2(0, 0));

            ProjectileList = new List<Projectile>();

            Tower = new Tower("Tower", new Vector2(32, 304-65));          
            Ground = new StaticSprite("Ground", new Vector2(0, 720 - 160 - 65));
            
            Resources = resources;
            ContentManager = contentManager;            

            #region IconNameList 
            //This gets the names of the icons that are to appear on the
            //buttons that allow the player to select traps/turrets they want to place
                IconNameList = new List<string>();
                IconNameList.Add(null);
                IconNameList.Add("SpikeTrap");
                IconNameList.Add("FireTrap");
                IconNameList.Add("BasicTurretIcon");
                IconNameList.Add(null);
                IconNameList.Add(null);
                IconNameList.Add(null);
                IconNameList.Add(null);
            #endregion

            #region Setting up the buttons
            BackgroundAssetName = "UI";
            SelectButtonAssetName = "Button";
            TrapSlotAssetName = "TrapButton";
            TowerSlotAssetName = "TrapButton";

            Position = new Vector2(0, 560);

            SelectButtonList = new List<Button>();
            TowerButtonList = new List<Button>();
            TrapsButtonList = new List<Button>();

            for (int i = 0; i < trapButtons; i++)
            {
                TrapsButtonList.Add(new Button(TrapSlotAssetName, new Vector2((128 * i) + 256+64, Position.Y - 32-65)));
                TrapsButtonList[i].LoadContent(ContentManager);
            }        

            for (int i = 0; i < towerButtons; i++)
            {
                TowerButtonList.Add(new Button(TowerSlotAssetName, new Vector2(48+64+32+8, 272 + ((38+90) * i)-65)));
                TowerButtonList[i].LoadContent(ContentManager);
            }

            for (int i = 0; i < 8; i++)
            {
                SelectButtonList.Add(new Button(SelectButtonAssetName, new Vector2(16 + (i * 160), Position.Y + 16), IconNameList[i]));
                SelectButtonList[i].LoadContent(ContentManager);
            }
            #endregion           

            #region List Creating Code
            //This code just creates the lists for the buttons and traps with the right number of possible slots
                TrapList = new List<Trap>();                      
                for (int i = 0; i < trapButtons; i++)
                {
                    TrapList.Add(new BlankTrap());
                    //TrapList[i].Position = TrapsButtonList[i].Position;
                    TrapList[i].LoadContent(ContentManager);                    
                }

                TurretList = new List<Turret>();
                for (int i = 0; i < towerButtons; i++)
                {
                    TurretList.Add(new BlankTurret());
                    TurretList[i].LoadContent(ContentManager);
                }            

                InvaderList = new List<Invader>();
                for (int i = 0; i < 20; i++)
                {
                    InvaderList.Add(new Soldier(new Vector2(1260+(46*i), 720 - 160 - 32-65)));
                    InvaderList[i].LoadContent(ContentManager);
                }

                //for (int i = 0; i < TrapList.Count; i++)
                //{
                //    TrapList[i].Position = TrapsButtonList[i].Position;
                //}
            #endregion                                       
        }

        public void LoadContent()
        {
            Marker.LoadContent(ContentManager);
            ResourceFont = ContentManager.Load<SpriteFont>("ResourceFont");
            Tower.LoadContent(ContentManager);
            Ground.LoadContent(ContentManager);
            BackgroundTexture = ContentManager.Load<Texture2D>(BackgroundAssetName);
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, BackgroundTexture.Width, BackgroundTexture.Height);
        }

        public void Update(GameTime gameTime)
        {
            //This is just the stuff that needs to be updated every step
            //This is where I call all the smaller procedures that I broke the update into             
            

            CurrentMouseState = Mouse.GetState();
            CursorPosition = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);

            TrapCollision();
            
            RightClickClearSelected();         
                                      
            SelectButtonsUpdate();
            TowerButtonUpdate();
            TrapButtonUpdate();

            TurretUpdate();
            InvaderUpdate(gameTime);            

            foreach (Turret turret in TurretList)
            {
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
                turret.Update(gameTime);
            }                     

            PreviousMouseState = CurrentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //MoveInvaders();

            if (ProjectileList.Count == 1)
                ProjectileUpdate();

            spriteBatch.DrawString(ResourceFont, "Resources: " + Resources.ToString(), new Vector2(0, 0), Color.White);
            spriteBatch.DrawString(ResourceFont, "Rounds: " + Rounds.ToString(), new Vector2(0, 32), Color.White);

            #region Background stuff
                Ground.Draw(spriteBatch);
                Tower.Draw(spriteBatch);
                Marker.Draw(spriteBatch);
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

                foreach (Button trapSlot in TrapsButtonList)
                {
                    trapSlot.Draw(spriteBatch);
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
                    turret.Draw(spriteBatch);
                }
            #endregion

            CursorDraw(spriteBatch);                        
            //MoveInvaders();                       
        }

        
        //Buttons update
        private void TrapButtonUpdate()
        {
            //This code makes sure that the selected trap is placed at the Trap Slot that the player has selected
            //It also deducts the correct amount of resources from the players' current store.
            //It then deactivates the Trap Slot that was selected to make sure that players can't 
            //place more than one trap on each slot
            int Index;

            foreach (Button trapButton in TrapsButtonList)
            {
                trapButton.Update();

                if (trapButton.JustClicked == true)
                {                    
                    Index = TrapsButtonList.IndexOf(trapButton);

                    foreach (Invader invader in InvaderList)
                    {
                        if (invader.DestinationRectangle.Intersects(trapButton.DestinationRectangle))
                        {
                            SelectedTrap = TrapType.Blank;
                        }
                    }

                    switch (SelectedTrap)
                    {
                        case TrapType.Wall:

                            if (Resources >= 50)
                            {
                                TrapList[Index] = new Wall(new Vector2(trapButton.Position.X, trapButton.Position.Y + 32));
                                TrapList[Index].LoadContent(ContentManager);
                                Resources -= 50;
                                SelectedTrap = TrapType.Blank;
                                trapButton.ButtonActive = false;
                            }
                            break;

                        case TrapType.Spikes:

                            if (Resources >= 30)
                            {
                                TrapList[Index] = new SpikeTrap(new Vector2(trapButton.Position.X, trapButton.Position.Y + 32));
                                TrapList[Index].LoadContent(ContentManager);
                                Resources -= 30;
                                SelectedTrap = TrapType.Blank;
                                trapButton.ButtonActive = false;
                            }
                            break;

                        case TrapType.Fire:

                            if (Resources >= 60)
                            {
                                TrapList[Index] = new FireTrap(new Vector2(trapButton.Position.X, trapButton.Position.Y + 32));
                                TrapList[Index].LoadContent(ContentManager);
                                Resources -= 60;
                                SelectedTrap = TrapType.Blank;
                                trapButton.ButtonActive = false;
                            }
                            break;
                    }
                }
            }
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
                                TurretList[Index] = new BasicTurret("BasicTurret", "BasicTurretBase", TowerButtonList[Index].Position);
                                TurretList[Index].LoadContent(ContentManager);
                                Resources -= 100;
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
                            SelectedTrap = TrapType.Blank;
                            SelectedTurret = TurretType.Blank;
                            break;

                        case 1:
                            SelectedTrap = TrapType.Spikes;
                            SelectedTurret = TurretType.Blank;
                            break;

                        case 2:
                            SelectedTrap = TrapType.Fire;
                            SelectedTurret = TurretType.Blank;
                            break;

                        case 3:
                            SelectedTurret = TurretType.Basic;
                            SelectedTrap = TrapType.Blank;
                            break;
                    }
                }
            }

            if (CurrentKeyboardState.IsKeyUp(Keys.D1) && PreviousKeyboardState.IsKeyDown(Keys.D1))
            {
                SelectedTrap = TrapType.Wall;
                SelectedTurret = TurretType.Blank;
                ClearTurretSelect();
            }

            if (CurrentKeyboardState.IsKeyUp(Keys.D2) && PreviousKeyboardState.IsKeyDown(Keys.D2))
            {
                SelectedTrap = TrapType.Spikes;
                SelectedTurret = TurretType.Blank;
                ClearTurretSelect();
            }

            if (CurrentKeyboardState.IsKeyUp(Keys.D3) && PreviousKeyboardState.IsKeyDown(Keys.D3))
            {
                SelectedTrap = TrapType.Fire;
                SelectedTurret = TurretType.Blank;
                ClearTurretSelect();
            }

            if (CurrentKeyboardState.IsKeyUp(Keys.D4) && PreviousKeyboardState.IsKeyDown(Keys.D4))
            {
                SelectedTurret = TurretType.Basic;
                SelectedTrap = TrapType.Blank;
                ClearTurretSelect();
            }
            
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
                if (InvaderList[i].CurrentHealth <= 0)
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

        //Turret stuff that needs to be called every step
        private void TurretUpdate()
        {            
            //This piece of code makes sure that two turrets cannot be selected at any one time
            ProjectileList.Clear();

            foreach (Turret turret in TurretList)
            {
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
            ProjectileList.Clear();

            foreach (Turret turret in TurretList)
            {
                if (turret.Selected == true)
                {
                    Vector2 MousePosition, Direction;

                    CurrentMouseState = Mouse.GetState();
                    MousePosition = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);

                    Direction = MousePosition - new Vector2(turret.BarrelRectangle.X, turret.BarrelRectangle.Y);
                    Direction.Normalize();

                    ProjectileList.Add(new Projectile(new Vector2(turret.BarrelRectangle.X, turret.BarrelRectangle.Y), new Vector2(Direction.X, Direction.Y)));
                }
            }
        }

        private void ProjectileUpdate()
        {          
            foreach (Turret turret1 in TurretList)
            {
                if (turret1.Selected == true)
                {
                    //This checks if the ray hit ANY invader and ANY wall. If it hit both an invader and a wall (Doesn't matter which invader or which wall
                    //just that it hit them. Then it checks if the invader was closer to the origin of the ray. If it was, the invader is destroyed//
                    //IF the wall was hit before the invader, then it does nothing. 
                    if (InvaderList.Any(TestInvader => TestInvader.BoundingBox.Intersects(ProjectileList[0].Ray) != null) &&
                        TrapList.Any(TestTrap => TestTrap.BoundingBox.Intersects(ProjectileList[0].Ray) != null && TestTrap.TrapType != TrapType.Blank) &&
                        ProjectileList[0].Active == true)
                    {
                        Invader HitInvader = InvaderList.First(TestInvader => TestInvader.BoundingBox.Intersects(ProjectileList[0].Ray) != null);
                        Trap HitTrap = TrapList.First(TestTrap => TestTrap.BoundingBox.Intersects(ProjectileList[0].Ray) != null);

                        if (ProjectileList[0].Ray.Intersects(HitInvader.BoundingBox) < ProjectileList[0].Ray.Intersects(HitTrap.BoundingBox) && HitInvader.Active == true )
                        {
                            if (ProjectileList.Count > 0)
                            {
                                ProjectileList[0].Active = false;
                                //HitInvader.CurrentHealth -= turret1.Damage;
                                HitInvader.TurretDamage(-turret1.Damage);
                                return;
                            }
                        }
                    }

                    //If the projectile ray DOES NOT intersect with any of the traps and it DOES intersect with an invader. Then it destroys the invader//
                    //This is to make sure that the ray doesn't have to intersect with a trap at any point to be able to kill an invader.
                    if (TrapList.All(Trap => Trap.BoundingBox.Intersects(ProjectileList[0].Ray) == null) &&
                        InvaderList.Any(TestInvader => TestInvader.BoundingBox.Intersects(ProjectileList[0].Ray) != null) &&
                        ProjectileList[0].Active == true)
                    {
                        Invader HitInvader = InvaderList.First(TestInvader => TestInvader.BoundingBox.Intersects(ProjectileList[0].Ray) != null);

                        if (ProjectileList.Count > 0 && HitInvader.Active == true)
                        {
                            ProjectileList[0].Active = false;
                            //HitInvader.CurrentHealth -= turret1.Damage;            
                            HitInvader.TurretDamage(-turret1.Damage);
                            return;
                        }
                    }

                    //This makes sure that the ray doesn't intersect with ANY traps and makes sure that it doesn't intersect with ANY invaders
                    //If both of those conditions are true AND it intersects with the ground at any point, it calculates the ground collision point
                    //And creates a particle effect there so that the player knows they didn't hit an invader or a trap
                    //It was made so that if the ray intersects an invader at all, it will abort the ray. It doesn't really matter that the invader is hit before
                    //the ground because the invaders are always above the ground and none of the turrets are below the ground i.e. the ground will never be hit first
                    //due to the placement of the elements in the game.
                    if (TrapList.All(Trap => Trap.BoundingBox.Intersects(ProjectileList[0].Ray) == null) &&
                                InvaderList.All(Invader => Invader.BoundingBox.Intersects(ProjectileList[0].Ray) == null) &&
                                ProjectileList[0].Ray.Intersects(Ground.BoundingBox) != null && ProjectileList[0].Active == true)
                    {
                        Nullable<double> Distance = ProjectileList[0].Ray.Intersects(Ground.BoundingBox);

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

                                    Marker.Position = newPos;
                                }
                            }
                        }
                    }
                }
            }
            ProjectileList.Clear();
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
                for (int i = 5; i > -1; i--)
                {
                    if (i != 0)
                    {
                        if (invader.Position.X < (TrapList[i].DestinationRectangle.Right) &&
                            (invader.Position.X) > TrapList[i].DestinationRectangle.X &&
                            TrapList[i].TrapType != TrapType.Blank)
                        {
                            invader.TrapDamage(TrapList[i].TrapType);
                            invader.VulnerableToTrap = false;
                            break;
                        }

                        if (invader.Position.X < (TrapList[i].DestinationRectangle.Right) &&
                            (invader.Position.X) > TrapList[i].DestinationRectangle.X &&
                            TrapList[i].TrapType == TrapType.Blank)
                        {
                            invader.VulnerableToTrap = false;
                            break;
                        }

                        if ((invader.DestinationRectangle.Right) < (TrapList[i].DestinationRectangle.X) && (invader.Position.X > (TrapList[i - 1].DestinationRectangle.Right)))
                        {
                            invader.VulnerableToTrap = true;
                            break;
                        }
                    }
                    else
                    {
                        if (invader.Position.X < (TrapList[i].DestinationRectangle.Right) &&
                            (invader.Position.X) > TrapList[i].DestinationRectangle.X &&
                            TrapList[i].TrapType != TrapType.Blank)
                        {
                            invader.TrapDamage(TrapList[i].TrapType);
                            invader.VulnerableToTrap = false;
                        }

                        if (invader.Position.X < (TrapList[i].DestinationRectangle.Right) &&
                            (invader.Position.X) > TrapList[i].DestinationRectangle.X &&
                            TrapList[i].TrapType == TrapType.Blank)
                        {
                            invader.VulnerableToTrap = false;
                        }

                        if ((invader.DestinationRectangle.Right) < (TrapList[i].DestinationRectangle.X) && (invader.Position.X > (TrapList[i].DestinationRectangle.Right)))
                        {
                            invader.VulnerableToTrap = true;
                        }
                    }
                }
            }
        }

        private void CursorDraw(SpriteBatch spriteBatch)
        {
            if ((TurretList[0].Selected == true) || (TurretList[1].Selected == true) || (TurretList[2].Selected == true))
            {
                PrimaryCursorTexture = ContentManager.Load<Texture2D>("TurretCrosshair");
                CurrentCursor = CursorType.Crosshair;
            }
            else
            {
                PrimaryCursorTexture = ContentManager.Load<Texture2D>("DefaultCursor");
                CurrentCursor = CursorType.Default;
            }

            switch (SelectedTrap)
            {
                case TrapType.Blank:
                    switch (SelectedTurret)
                    {
                        case TurretType.Blank:
                            SecondaryCursorTexture = ContentManager.Load<Texture2D>("Blank");
                            break;

                        case TurretType.Basic:
                            SecondaryCursorTexture = ContentManager.Load<Texture2D>("BasicTurretIcon");
                            break;
                    }
                    break;

                case TrapType.Fire:
                    SecondaryCursorTexture = ContentManager.Load<Texture2D>("FireTrap");
                    break;

                case TrapType.Spikes:
                    SecondaryCursorTexture = ContentManager.Load<Texture2D>("SpikeTrap");
                    break;

                case TrapType.Wall:
                    SecondaryCursorTexture = ContentManager.Load<Texture2D>("Wall");
                    break;
            }


            spriteBatch.Draw(SecondaryCursorTexture, new Rectangle((int)CursorPosition.X - (SecondaryCursorTexture.Width/2), (int)CursorPosition.Y - SecondaryCursorTexture.Height, SecondaryCursorTexture.Width, SecondaryCursorTexture.Height), Color.White);

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
        }
        #endregion
    }
}
