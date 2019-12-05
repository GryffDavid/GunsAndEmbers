using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class UserInterface
    {
        ContentManager ContentManager;
        Texture2D BackgroundTexture;
        string BackgroundAssetName, SelectButtonAssetName, TrapSlotAssetName, TowerSlotAssetName;
        Vector2 Position;
        Rectangle DestinationRectangle;
        int Resources;
        public List<Button> SelectButtonList;
        public List<Button> TowerButtonList;
        public List<Button> TrapsButtonList;
        public List<Trap> TrapList;
        public List<Turret> TurretList;
        public List<Invader> InvaderList;
        List<string> IconNameList;

        SpriteFont ResourceFont;
        Tower Tower;
        StaticSprite Ground;
        TrapType SelectedTrap;
        TurretType SelectedTurret;

        public UserInterface(int traps, int slots, int resources, ContentManager contentManager)
        {
            Tower = new Tower("Tower", new Vector2(32, 304));
            Ground = new StaticSprite("Ground", new Vector2(0, 720 - 160 - 48));
            Resources = resources;
            ContentManager = contentManager;

            #region IconNameList 
            //This gets the names of the icons that are to appear on the
            //buttons that allow the player to select traps/turrets they want to place
                IconNameList = new List<string>();
                IconNameList.Add("Wall");
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

            //TestTurret = new BasicTurret("BasicTurret", "BasicTurretBase", new Vector2(100, 100));

            for (int i = 0; i < traps; i++)
            {
                TrapsButtonList.Add(new Button(TrapSlotAssetName, new Vector2((128 * i) + 256+64, Position.Y - 32)));                
            }        

            for (int i = 0; i < slots; i++)
            {
                TowerButtonList.Add(new Button(TowerSlotAssetName, new Vector2(48+64+32, 320 + ((38+64) * i))));
            }

            for (int i = 0; i < 8; i++)
            {
                SelectButtonList.Add(new Button(SelectButtonAssetName, new Vector2(16 + (i * 160), Position.Y + 16), IconNameList[i]));
            }
            #endregion

            #region List Creating Code
            //This code just creates the lists for the buttons and traps with the right number of possible slots
                TrapList = new List<Trap>();                      
                for (int i = 0; i < traps; i++)
                {
                    TrapList.Add(new BlankTrap(new Vector2(100, 100)));           
                }

                TurretList = new List<Turret>();
                for (int i = 0; i < slots; i++)
                {
                    TurretList.Add(new BlankTurret());
                }            

                InvaderList = new List<Invader>();
                for (int i = 0; i < 8; i++)
                {
                    InvaderList.Add(new Soldier(new Vector2(1260+(32*i), 720 - 160 - 37)));
                }
            #endregion

            SelectedTrap = TrapType.Blank;
        }

        public void LoadContent(ContentManager contentManager)
        {
            //Loads the content for the basic stuff
            //These are all static
            Tower.LoadContent(contentManager);
            Ground.LoadContent(contentManager);
            ResourceFont = contentManager.Load<SpriteFont>("ResourceFont");

            #region Loading basic button content
            //This loads all the textures for the buttons that won't need to be changed at any point
            //Some of the content loading isn't called here because it needs to be called only when a trap is placed
            //Or when something is changed. The traps only load textures when they're placed so their LoadContent calls
            //Only happen in the Update event once the trap has been placed. Same applies to turrets.
            BackgroundTexture = contentManager.Load<Texture2D>(BackgroundAssetName);

            foreach (Button button in SelectButtonList)
            {
                button.LoadContent(contentManager);
            }

            foreach (Button towerSlot in TowerButtonList)
            {
                towerSlot.LoadContent(contentManager);
            }

            foreach (Button trapSlot in TrapsButtonList)
            {
                trapSlot.LoadContent(contentManager);
            }

            foreach (Trap trap in TrapList)
            {
                trap.LoadContent(contentManager);
            }    
            #endregion            

            foreach (Invader invader in InvaderList)
            {
                invader.LoadContent(contentManager);
            }

            foreach (Turret turret in TurretList)
            {
                turret.LoadContent(contentManager);
            }
        }

        public void Update(GameTime gameTime)
        {
            //This is just the stuff that needs to be updated every step
            //This is where I call all the smaller procedures that I broke the update into

            #region Basic button update stuff
            //This just updates the buttons every step so that they react properly to the mouse and clicking etc.
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, BackgroundTexture.Width, BackgroundTexture.Height);

            foreach (Button button in SelectButtonList)
            {
                button.Update(gameTime);
            }

            foreach (Button towerSlot in TowerButtonList)
            {
                towerSlot.Update(gameTime);
            }

            foreach (Button trapSlot in TrapsButtonList)
            {
                trapSlot.Update(gameTime);
            }
            #endregion                               
                        
            SelectTrapUpdate();
            TowerButtonUpdate();
            TrapButtonUpdate();
            CheckInvaderCollisions();
           

            TurretUpdate();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(ResourceFont, "Resources: " + Resources.ToString(), new Vector2(0, 0), Color.White); 

            Ground.Draw(spriteBatch);
            Tower.Draw(spriteBatch);
            InvaderUpdate();
            TurretUpdate();

            #region Setting up the buttons
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
                turret.Draw(spriteBatch);
            }            
        }

        private void TrapButtonUpdate()
        {
            //This code makes sure that the selected trap is placed at the Trap Slot that the player has selected
            //It also deducts the correct amount of resources from the players' current store.
            //It then deactivates the Trap Slot that was selected to make sure that players can't 
            //place more than one trap on each slot
            int Index;

            foreach (Button trapSlot in TrapsButtonList)
            {
                if (trapSlot.JustClicked == true)
                {                    
                    Index = TrapsButtonList.IndexOf(trapSlot);

                    foreach (Invader invader in InvaderList)
                    {
                        if (invader.DestinationRectangle.Intersects(trapSlot.DestinationRectangle))
                        {
                            SelectedTrap = TrapType.Blank;
                        }
                    }

                    switch (SelectedTrap)
                    {
                        case TrapType.Wall:

                            if (Resources >= 50)
                            {
                                TrapList[Index] = new Wall(new Vector2(trapSlot.Position.X, trapSlot.Position.Y + 32));
                                TrapList[Index].LoadContent(ContentManager);
                                Resources -= 50;
                                SelectedTrap = TrapType.Blank;
                                trapSlot.ButtonActive = false;
                            }
                            break;

                        case TrapType.Spikes:

                            if (Resources >= 30)
                            {
                                TrapList[Index] = new SpikeTrap(new Vector2(trapSlot.Position.X, trapSlot.Position.Y + 32));
                                TrapList[Index].LoadContent(ContentManager);
                                Resources -= 30;
                                SelectedTrap = TrapType.Blank;
                                trapSlot.ButtonActive = false;
                            }
                            break;

                        case TrapType.Fire:

                            if (Resources >= 60)
                            {
                                TrapList[Index] = new FireTrap(new Vector2(trapSlot.Position.X, trapSlot.Position.Y + 32));
                                TrapList[Index].LoadContent(ContentManager);
                                Resources -= 60;
                                SelectedTrap = TrapType.Blank;
                                trapSlot.ButtonActive = false;
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

        private void SelectTrapUpdate()
        {
            //This makes sure that when the button at the bottom of the screen is clicked, the corresponding trap or turret is actually selected//
            //This will code will need to be added to every time that a new trap/turret is added to the game.
            int Index;

            foreach (Button button in SelectButtonList)
            {
                if (button.JustClicked == true)
                {
                    ClearTurretSelect();
                    Index = SelectButtonList.IndexOf(button);

                    switch (Index)
                    {
                        case 0:
                            SelectedTrap = TrapType.Wall;
                            break;

                        case 1:
                            SelectedTrap = TrapType.Spikes;
                            break;

                        case 2:
                            SelectedTrap = TrapType.Fire;
                            break;

                        case 3:
                            SelectedTurret = TurretType.Basic;
                            break;
                    }
                }
            }
        }

        private void CheckInvaderCollisions()
        {

        }

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

        private void ClearTurretSelect()
        {
            //This forces all turrets to become un-selected.
            foreach (Turret turret in TurretList)
            {
                turret.Selected = false;
            }
        }

        private void TurretUpdate()
        {
            //This piece of code makes sure that two turrets cannot be selected at any one time
            foreach (Turret turret in TurretList)
            {
                if (turret.JustClicked == true)
                {
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
    }
}
