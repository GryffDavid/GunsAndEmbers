using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GameDataTypes
{
    public class Level1Dialogue : LevelDialogue
    {
        float CurrentTime, MaxTime;
        
        public Level1Dialogue()
        {
            
        }

        public override void Update(GameTime gameTime)
        {
            switch (CurrentID)
            {
                default:
                    CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    CurrentText = DialogueItems[CurrentID].Message;

                    if (CurrentTime > DialogueItems[CurrentID].Time)
                    {
                        CurrentID++;
                        CurrentTime = 0;
                    }
                    break;

                //case 0:

                //    break;

                //case 1:
                //    CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                //    CurrentText = "Welcome to your new assignment. You'll be using our new system of weapons to defend this outpost until further notice.";

                //    if (CurrentTime > 3000)
                //    {
                //        CurrentID++;
                //        CurrentTime = 0;
                //    }
                //    break;

                //case 2:
                //    CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                //    CurrentText = "You can place turrets onto your tower and you can place traps onto the terrain.";

                //    if (CurrentTime > 3000)
                //    {
                //        CurrentID++;
                //        CurrentTime = 0;
                //    }
                //    break;

                case 3:
                    
                    CurrentText = "Select the Machine Gun turret to place onto your tower";
                    if (Game.SelectedTurret == TurretType.MachineGun)
                    {
                        CurrentID++;
                    }
                    break;

                case 4:
                    //CurrentText = "Great. Now you can place it onto the slot on your tower.";
                    break;
            }
        }
    }
}
