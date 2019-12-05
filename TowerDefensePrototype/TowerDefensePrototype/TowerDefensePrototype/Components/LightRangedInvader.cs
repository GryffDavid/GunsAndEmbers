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
        #region For handling ranged attacking
        public InvaderFireType FireType; //Whether the invader fires a single projectile, fires a burst or fires a beam etc.
        public Vector2 TowerDistanceRange; //How far away from the tower the invader will be before stopping to fire
        public Vector2 AngleRange; //The angle that the projectile is fired at.

        public bool InTowerRange = false;
        public bool InTrapRange = false;
        public float DistToTower = 1920;
        public float DistToTrap, TrapPosition;
        public float MinTowerRange, MinTrapRange;

        public float RangedDamage; //How much damage the projectile does
        public float CurrentFireDelay, MaxFireDelay; //How many milliseconds between shots
        public int CurrentBurstShots, MaxBurstShots; //How many shots are fired in a row before a longer recharge is needed
        #endregion

        public override void Initialize()
        {
            MinTowerRange = Random.Next((int)TowerDistanceRange.X, 
                                        (int)TowerDistanceRange.Y);

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
