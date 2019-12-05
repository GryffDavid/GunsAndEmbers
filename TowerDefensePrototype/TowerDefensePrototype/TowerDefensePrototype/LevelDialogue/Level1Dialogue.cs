using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameDataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace TowerDefensePrototype
{
    public class Level1Dialogue : LevelDialogue
    {
        int CrateNum = 0;
        

        public Level1Dialogue(Game1 game1)
            : base(game1)
        {
            
        }

        public override void Update(GameTime gameTime)
        {
            CurrentMouseState = Mouse.GetState();

            CurrentText = ItemsList[CurrentID].Message;

            DialogueBox.CompleteText = DialogueBox.WrapText(CurrentText);
            DialogueBox.Update(gameTime);

            TutorialMarker.Update(gameTime);
            //CurrentLevel.LevelDialogue.TutorialMarker.

            switch (CurrentID)
            {
                default:
                    if (DialogueBox.MaxTime < 20)
                    {
                        if (CurrentMouseState.LeftButton == ButtonState.Released &&
                            PreviousMouseState.LeftButton == ButtonState.Pressed &&
                            DialogueBox.LengthIndex < DialogueBox.CompleteText.Length &&
                            DialogueBox.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                        {
                            DialogueBox.LengthIndex = DialogueBox.CompleteText.Length;
                            break;
                        }

                        if (CurrentMouseState.LeftButton == ButtonState.Released &&
                            PreviousMouseState.LeftButton == ButtonState.Pressed &&
                            DialogueBox.LengthIndex == DialogueBox.CompleteText.Length &&
                            DialogueBox.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                        {
                            Next();
                        }
                    }
                    break;

                case 3:
                    {
                        DialogueBox.TipText = "";

                        int index = Game1.CurrentProfile.Buttons.IndexOf(null);

                        TutorialMarker.Position = Game1.CooldownButtonList[index].CurrentPosition + Game1.CooldownButtonList[index].CurrentSize / 2;
                        TutorialMarker.Active = true;
                        //TutorialMarker.StartSize = Game1.CooldownButtonList[0].CurrentSize;
                        //TutorialMarker.MaxSize = TutorialMarker.StartSize * 3f;



                        if (Game1.CurrentProfile.Buttons[index] == null)
                            Game1.DynamicAddWeapon(index, TurretType.MachineGun, null);

                        if (Game1.SelectedTurret == TurretType.MachineGun)
                        {
                            Next();
                        }
                    }
                    break;

                case 4:
                    {
                        TutorialMarker.Position = Game1.TowerButtonList[0].CurrentPosition + Game1.TowerButtonList[0].FrameSize / 2;
                        TutorialMarker.Active = true;

                        if (Game1.TurretList.Count<Turret>(Turret => Turret != null &&
                            Turret.TurretType == TurretType.MachineGun) > 0)
                        {
                            Next();
                            TutorialMarker.Active = false;
                        }
                    }
                    break;

                case 5:
                    if (Game1.TurretList[0] != null)
                    {
                        if (Game1.TurretList[0].ShotsFired >= 5)
                        {
                            Next();

                            Crate crate = new Crate(new Vector2(Random.Next(800, 1200), 500), new Vector2(690, 930));
                            Game1.AddInvader(crate, gameTime);
                        }
                    }
                    break;

                case 6:
                    if (Game1.InvaderList.Count == 0)
                    {
                        Next();
                    }
                    break;

                case 7:
                    if (Game1.CurrentTurret == null)
                    {
                        int index = Game1.CurrentProfile.Buttons.IndexOf(null);

                        TutorialMarker.Position = Game1.CooldownButtonList[index].CurrentPosition + Game1.CooldownButtonList[index].CurrentSize / 2;
                        TutorialMarker.Active = true;

                        Next();
                        Game1.DynamicAddWeapon(index, null, TrapType.Fire);
                        Game1.Resources += 120;
                    }
                    break;

                case 8:
                    if (Game1.SelectedTrap == TrapType.Fire)
                    {
                        Next();

                        TutorialMarker.Active = false;
                    }
                    break;

                case 9:
                    if (Game1.TrapList.Count > 0)
                        if (Game1.TrapList[0].TrapType == TrapType.Fire)
                        {
                            Game1.Resources += (4 * 120);
                            Next();
                        }
                    break;

                case 10:
                    if (Game1.TrapList.Count == 5)
                    {
                        Next();
                    }
                    break;

                case 11:
                    if (Game1.StartWave == false)
                    {
                        //Game1.CurrentWaveIndex++;
                        Game1.StartWaves();
                    }

                    CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    
                    if (CurrentTime > ItemsList[CurrentID].Time)
                    {
                        Next();
                        CurrentTime = 0;
                    }
                    break;

                case 12:
                    //The turret has overheated. Warn the player about this mechanic
                    if (Game1.TurretList[0] != null)
                    {
                        if (Game1.TurretList[0].Overheated == true)
                        {
                            Next();
                            Game1.Resources += 250;
                        }
                    }

                    //The wave has been defeated before the player overheats the turret. Move along anyway.
                    if (Game1.StartWave == false)
                    {
                        DialogueBox.LengthIndex = 0;
                        CurrentID = 15;
                    }
                    break;

                case 13:
                    if (Game1.TurretList[0].Overheated == false ||
                        (Game1.TurretList[1] != null && Game1.TurretList[1].TurretType == TurretType.MachineGun))
                    {
                        Next();
                    }
                    break;

                case 14:
                    if (Game1.StartWave == false && Game1.InvaderList.Count == 0)
                    {
                        DialogueBox.LengthIndex = 0;
                        CurrentID = 15;
                    }
                    break;

                case 16:
                    if (Game1.TurretList[0] == null)
                    {
                        //if (Game1.SelectedTurret != TurretType.Cannon)
                        //{
                        TutorialMarker.Position = Game1.CooldownButtonList[2].CurrentPosition + Game1.CooldownButtonList[2].CurrentSize / 2;
                        TutorialMarker.Active = true;
                        //}

                        Next();
                        Game1.DynamicAddWeapon(2, TurretType.Cannon, null);
                        Game1.Resources = 600;
                    }
                    break;

                case 17:
                    if (Game1.TurretList[0] == null)
                    {
                        if (Game1.SelectedTurret != TurretType.Cannon)
                        {
                            TutorialMarker.Position = Game1.CooldownButtonList[2].CurrentPosition + Game1.CooldownButtonList[2].CurrentSize / 2;
                            TutorialMarker.Active = true;
                        }
                        else
                        {
                            TutorialMarker.Position = Game1.TowerButtonList[0].CurrentPosition + Game1.TowerButtonList[0].FrameSize / 2;
                            TutorialMarker.Active = true;
                        }
                    }

                    if (Game1.TurretList[0] != null &&
                        Game1.TurretList[0].TurretType == TurretType.Cannon)
                    {
                        TutorialMarker.Active = false;
                        Next();
                    }
                    break;

                case 18:
                    //The cannon has been fired
                    if (Game1.TurretList[0].ShotsFired >= 1 &&
                        Game1.TurretList[0].TurretType == TurretType.Cannon)
                    {
                        Next();                      
                    }
                    break;

                case 20:
                    {
                        if (CrateNum == 0)
                        {
                            for (int i = 0; i < 7; i++)
                            {
                                Crate crate = new Crate(new Vector2(Random.Next(800, 1200), 500), new Vector2(690, 930), 20);
                                Game1.AddInvader(crate, gameTime);
                            }

                            Next();
                        }
                        else
                        {
                            //All the crates were destroyed                            
                            if (Game1.InvaderList.Count == 0)
                            {
                                Next();
                            }
                        }
                    }
                    break;

                case 22:
                    {
                        if (Game1.StartWave == false)
                            Game1.StartWaves();

                        CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                        if (CurrentTime > ItemsList[CurrentID].Time)
                        {
                            Next();
                            CurrentTime = 0;
                        }
                    }
                    break;

                case 23:
                    if (Game1.StartWave == false && Game1.InvaderList.Count == 0)
                    {
                        Next();
                    }
                    break;

                case 24:
                    CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                        if (CurrentTime > ItemsList[CurrentID].Time)
                        {
                            Game1.PlayerVictory();
                            CurrentTime = 0;
                        }
                    break;
            }

            PreviousMouseState = CurrentMouseState;

            base.Update(gameTime);
        }
    }
}
