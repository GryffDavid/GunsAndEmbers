using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameDataTypes;
using Microsoft.Xna.Framework;

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
            CurrentText = ItemsList[CurrentID].Message;

            switch (CurrentID)
            {
                default:
                    CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (CurrentTime > ItemsList[CurrentID].Time)
                    {
                        CurrentID++;
                        CurrentTime = 0;
                    }
                    break;

                case 3:
                    if (Game1.CurrentProfile.Buttons[0] == null)
                        Game1.DynamicAddWeapon(0, TurretType.MachineGun, null);

                    if (Game1.SelectedTurret == TurretType.MachineGun)
                    {
                        CurrentID++;
                    }
                    break;

                case 4:
                    if (Game1.TurretList.Count<Turret>(Turret => Turret != null && 
                        Turret.TurretType == TurretType.MachineGun) > 0)
                    {
                        CurrentID++;
                    }
                    break;

                case 5:
                    if (Game1.TurretList[0].ShotsFired >= 5)
                    {
                        CurrentID++;

                        Crate crate = new Crate(new Vector2(Random.Next(800, 1200), 500), new Vector2(690, 930));
                        Game1.AddInvader(crate, gameTime);                        
                    }
                    break;

                case 6:
                    if (Game1.InvaderList.Count == 0)
                    {
                        CurrentID++;
                    }
                    break;

                case 7:
                    if (Game1.CurrentTurret == null)
                    {
                        CurrentID++;
                        Game1.DynamicAddWeapon(1, null, TrapType.Fire);
                        Game1.Resources += 120;
                    }
                    break;

                case 8:
                    if (Game1.SelectedTrap == TrapType.Fire)
                    {
                        CurrentID++;
                    }
                    break;

                case 9:
                    if (Game1.TrapList.Count > 0)
                        if (Game1.TrapList[0].TrapType == TrapType.Fire)
                        {
                            Game1.Resources += (4 * 120);
                            CurrentID++;
                        }
                    break;

                case 10:
                    if (Game1.TrapList.Count == 5)
                    {
                        CurrentID++;
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
                        CurrentID++;
                        CurrentTime = 0;
                    }
                    break;

                case 12:
                    //The turret has overheated. Warn the player about this mechanic
                    if (Game1.TurretList[0].Overheated == true)
                    {
                        CurrentID++;
                        Game1.Resources += 250;
                    }

                    //The wave has been defeated before the player overheats the turret. Move along anyway.
                    if (Game1.StartWave == false)
                    {
                        CurrentID = 15;
                    }
                    break;

                case 13:
                    if (Game1.TurretList[0].Overheated == false ||
                        (Game1.TurretList[1] != null && Game1.TurretList[1].TurretType == TurretType.MachineGun))
                    {
                        CurrentID++;
                    }
                    break;

                case 14:
                    if (Game1.StartWave == false && Game1.InvaderList.Count == 0)
                    {
                        CurrentID = 15;
                    }
                    break;

                case 16:
                    if (Game1.TurretList[0] == null)
                    {
                        CurrentID++;
                        Game1.DynamicAddWeapon(2, TurretType.Cannon, null);
                        Game1.Resources = 600;
                    }
                    break;

                case 17:
                    if (Game1.TurretList[0] != null &&
                        Game1.TurretList[0].TurretType == TurretType.Cannon)
                    {
                        CurrentID++;
                    }
                    break;

                case 18:
                    //The cannon has been fired
                    if (Game1.TurretList[0].ShotsFired >= 1 &&
                        Game1.TurretList[0].TurretType == TurretType.Cannon)
                    {
                        CurrentID++;                        
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
                                CrateNum++;
                            }
                        }
                        else
                        {
                            //All the crates were destroyed                            
                            if (Game1.InvaderList.Count == 0)
                            {
                                CurrentID++;
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
                            CurrentID++;
                            CurrentTime = 0;
                        }
                    }
                    break;

                case 23:
                    if (Game1.StartWave == false && Game1.InvaderList.Count == 0)
                    {
                        CurrentID++;
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

            base.Update(gameTime);
        }
    }
}
