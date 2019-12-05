using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    abstract class LightRangedInvader : Invader
    {        
        public override void Initialize()
        {
            MinTowerRange = Random.Next((int)DistanceRange.X, 
                                   (int)DistanceRange.Y);

            base.Initialize();
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            if (DistToTower <= MinTowerRange)
            {
                Velocity.X = 0;
                InTowerRange = true;
            }

            base.Update(gameTime, cursorPosition);
        }

        public void UpdateFireDelay(GameTime gameTime)
        {
            //if (RangedDamageStruct != null)
            {
                CurrentFireDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (CurrentFireDelay >= MaxFireDelay)
                {
                    CanAttack = true;
                    CurrentFireDelay = 0;
                }
                else
                {
                    CanAttack = false;
                }
            }
        }
    }
}
