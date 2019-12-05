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

            //if (ItemsList[CurrentID].Time != -1)
            //{

            //}

            switch (CurrentID)
            {
                default:
                    {
                        CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                        if (CurrentTime > ItemsList[CurrentID].Time)
                        {
                            CurrentID++;
                            CurrentTime = 0;
                        }
                    }
                    break;

                case 3:
                    //if (Game1.CurrentProfile.Buttons[0] == null)
                    //    Game1.CurrentProfile.Buttons[0] = new Weapon(TurretType.MachineGun, null);

                    if (Game1.CurrentProfile.Buttons[0] == null)
                        Game1.DynamicAddWeapon(0, TurretType.MachineGun, null);

                    if (Game1.SelectedTurret == TurretType.MachineGun)
                    {
                        CurrentID++;
                    }
                    break;

                case 4:
                    if (Game1.TurretList.Count<Turret>(Turret => Turret != null && Turret.TurretType == TurretType.MachineGun) > 0)
                    {
                        CurrentID++;
                    }
                    break;

                case 5:
                    if (Game1.TurretList[0].ShotsFired == 1)
                    {
                        CurrentID++;
                    }
                    break;

                case 6:
                    {
                        CurrentID++;
                        //SPAWN TUTORIAL CRATE HERE
                        //if (Game1.TurretList[0].ShotsFired == 10)
                        //{
                        //    CurrentID++;
                        //}
                    }
                    break;

                case 7:
                    if (Game1.SelectedTurret == null)
                    {
                        CurrentID++;

                        Game1.Resources += 120;

                        if (Game1.CurrentProfile.Buttons[1] == null)
                            Game1.DynamicAddWeapon(1, null, TrapType.Fire);
                    }
                    break;

                case 8:
                    {
                        if (Game1.SelectedTrap == TrapType.Fire)
                        {
                            CurrentID++;
                        }
                    }
                    break;

                case 9:
                    {
                        //COULD MAKE THESE APPEAR AS "OBJECTIVES" I.E. SHOW "FIRE TRAPS PLACED 1/5" etc.
                        //THE IDEA IS OK, BUT I ALSO LIKE THE IDEA OF JUST THROWING THE PLAYER INTO A LEVEL
                        //WITHOUT ANY OBJECTIVE EXCEPT "DO ANYTHING YOU CAN TO DEFEND THIS TOWER".
                        //THE OBJECTIVES COULD BE USED TO EARN NEW THINGS ETC. BUT MAYBE SHOULDN'T BE THE
                        //CORE MECHANIC
                        if (Game1.TrapList.Count > 0)
                        if (Game1.TrapList[0].TrapType == TrapType.Fire)
                        {
                            Game1.Resources += (4 * 120);
                            CurrentID++;
                        }
                    }
                    break;

                case 10:
                    {
                        if (CrateNum < 6)
                        {
                            if (Game1.InvaderList.Count == 0)
                            {

                                Crate crate = new Crate(new Vector2(1000, 100), new Vector2(690, 930));
                                Game1.AddInvader(crate, gameTime);
                                CrateNum++;
                            }
                        }
                        else
                        {
                            CurrentID++;
                        }
                    }
                    break;

                case 11:
                    {

                    }
                    break;
                    
            }
            

            base.Update(gameTime);
        }
    }
}
