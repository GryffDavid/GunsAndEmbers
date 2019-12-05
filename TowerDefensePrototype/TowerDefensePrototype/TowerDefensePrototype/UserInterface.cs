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

        //Custom class Declarations
        Tower Tower;
        StaticSprite Ground;
        TrapType SelectedTrap;
        TurretType SelectedTurret;
        CursorType CurrentCursor;

        GameTime GameTime;

        Ray TestRay;

        //Basic XNA calls
        public UserInterface(int trapButtons, int towerButtons, int resources, ContentManager contentManager)
        {
            Tower = new Tower("Tower", new Vector2(32, 304-65));          
            Ground = new StaticSprite("Ground", new Vector2(0, 720 - 160 - 65));
            
            Resources = resources;
            ContentManager = contentManager;            

            #region IconNameList 
            //This gets the names of the icons that are to appear on the
            //buttons that allow the player to select traps/turrets they want to place
                IconNameList = new List<string>();
                IconNameList.Add("WallIcon");
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
                    TrapList[i].LoadContent(ContentManager);
                }

                TurretList = new List<Turret>();
                for (int i = 0; i < towerButtons; i++)
                {
                    TurretList.Add(new BlankTurret());
                    TurretList[i].LoadContent(ContentManager);
                }            

                InvaderList = new List<Invader>();
                for (int i = 0; i < 8; i++)
                {
                    InvaderList.Add(new Soldier(new Vector2(1260+(32*i), 720 - 160 - 37-65)));
                    InvaderList[i].LoadContent(ContentManager);
                }
            #endregion           
        }

        public void LoadContent(ContentManager contentManager)
        {
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
            GameTime = gameTime;
            CurrentMouseState = Mouse.GetState();
            CursorPosition = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);
            RightClickClearSelected();         
                                      
            SelectButtonsUpdate();
            TowerButtonUpdate();
            TrapButtonUpdate();            

            PreviousMouseState = CurrentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(ResourceFont, "Resources: " + Resources.ToString(), new Vector2(0, 0), Color.White); 

            Ground.Draw(spriteBatch);
            Tower.Draw(spriteBatch);

            InvaderUpdate();
            TurretUpdate();

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
                    turret.Update();
                    turret.Draw(spriteBatch, GameTime);
                }
            #endregion

            CursorDraw(spriteBatch);

            TurretShoot();
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
                            SelectedTrap = TrapType.Wall;
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


        //Game elements update
        private void InvaderUpdate()
        {
            //This does all the stuff that would normally be in the Update call, but because this 
            //class became rather unwieldy, I broke up each call into separate smaller ones 
            //so that it's easier to manage
            foreach (Invader invader in InvaderList)
            {
                invader.Update();
                invader.Behaviour();
            }
        }

        private void TurretUpdate()
        {
            //This piece of code makes sure that two turrets cannot be selected at any one time
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

        private void TrapUpdate(GameTime gameTime)
        {
            foreach (Trap trap in TrapList)
            {
                trap.Update(gameTime);
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

        private void TurretShoot()
        {
            foreach (Turret turret in TurretList)
            {                
                if (turret.Selected == true)
                if (CurrentMouseState.LeftButton == ButtonState.Pressed && CurrentMouseState.Y < 720-160)
                {
                    turret.Flash.Flash(0, 3);
                }
            }
        }

        private void ProjectilesUpdate()
        {

        }



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
    }
}
