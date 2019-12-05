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
        ContentManager myContentManager;
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
        //TurretType SelectedTurret;
        //int ActiveTurret;
        BasicTurret TestTurret;

        public UserInterface(int traps, int slots, int resources, ContentManager contentManager)
        {
            IconNameList = new List<string>();
            IconNameList.Add("Wall");
            IconNameList.Add("SpikeTrap");
            IconNameList.Add("FireTrap");
            IconNameList.Add(null);
            IconNameList.Add(null);
            IconNameList.Add(null);
            IconNameList.Add(null);
            IconNameList.Add(null);
            
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

            Tower = new Tower("Tower", new Vector2(32, 304));
            Ground = new StaticSprite("Ground", new Vector2(0, 720 - 160 - 48));
            Resources = resources;
            myContentManager = contentManager; 
           
            TrapList = new List<Trap>();
            TurretList = new List<Turret>();           

            for (int i = 0; i < traps; i++)
            {
                TrapList.Add(new BlankTrap(new Vector2(100, 100)));           
            }

            for (int i = 0; i < slots; i++)
            {
                TurretList.Add(new BlankTurret());
            }

            SelectedTrap = TrapType.Blank;
            InvaderList = new List<Invader>();

            for (int i = 0; i < 8; i++)
            {
                InvaderList.Add(new Soldier(new Vector2(1260+(32*i), 720 - 160 - 37)));
            }            
        }

        public void LoadContent(ContentManager contentManager)
        {
            Tower.LoadContent(contentManager);
            Ground.LoadContent(contentManager);
            ResourceFont = contentManager.Load<SpriteFont>("ResourceFont");
            //TestTurret.LoadContent(contentManager);
            #region Loading basic button content
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
                trap.LoadContent(myContentManager);
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
            #region Basic button update stuff
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

            TrapButtonUpdate();
            SelectTrapUpdate();
            InvaderUpdate();
            CheckInvaderCollisions();            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Ground.Draw(spriteBatch);
            Tower.Draw(spriteBatch);            

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
            spriteBatch.DrawString(ResourceFont, "Resources: " + Resources.ToString(), new Vector2(0, 0), Color.White);
            
            foreach (Trap trap in TrapList)
            {
               trap.Draw(spriteBatch);
            }

            foreach (Invader invader in InvaderList)
            {
                invader.Draw(spriteBatch);
            }

            //TestTurret.Update();
            //TestTurret.Draw(spriteBatch);

            foreach (Turret turret in TurretList)
            {
                turret.Update();
                turret.Draw(spriteBatch);
            }
        }

        private void TrapButtonUpdate()
        {
            int Index;

            foreach (Button trapSlot in TrapsButtonList)
            {
                //foreach (Invader invader in InvaderList)
                //{  
                    if (trapSlot.JustClicked == true)
                    {
                        #region Switch statement for which trap is going to be placed
                        Index = TrapsButtonList.IndexOf(trapSlot);
                        switch (SelectedTrap)
                        {
                            case TrapType.Wall:

                                if (Resources >= 50)
                                {
                                    TrapList[Index] = new Wall(new Vector2(trapSlot.Position.X, trapSlot.Position.Y + 32));
                                    TrapList[Index].LoadContent(myContentManager);
                                    Resources -= 50;
                                    SelectedTrap = TrapType.Blank;
                                    trapSlot.ButtonActive = false;
                                }
                                break;

                            case TrapType.Spikes:

                                if (Resources >= 30)
                                {
                                    TrapList[Index] = new SpikeTrap(new Vector2(trapSlot.Position.X, trapSlot.Position.Y + 32));
                                    TrapList[Index].LoadContent(myContentManager);
                                    Resources -= 30;
                                    SelectedTrap = TrapType.Blank;
                                    trapSlot.ButtonActive = false;
                                }
                                break;

                            case TrapType.Fire:

                                if (Resources >= 60)
                                {
                                    TrapList[Index] = new FireTrap(new Vector2(trapSlot.Position.X, trapSlot.Position.Y + 32));
                                    TrapList[Index].LoadContent(myContentManager);
                                    Resources -= 60;
                                    SelectedTrap = TrapType.Blank;
                                    trapSlot.ButtonActive = false;
                                }
                                break;
                        }
                        #endregion
                    }
                //}
            }            
        }

        private void SelectTrapUpdate()
        {
            int Index;

            foreach (Button button in SelectButtonList)
            {
                if (button.JustClicked == true)
                {
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
                    }
                }
            }

        }

        private void CheckInvaderCollisions()
        {

        }

        private void InvaderUpdate()
        {
            foreach (Invader invader in InvaderList)
            {
                invader.Update();
                invader.Behaviour();
            }
        }
    }
}
