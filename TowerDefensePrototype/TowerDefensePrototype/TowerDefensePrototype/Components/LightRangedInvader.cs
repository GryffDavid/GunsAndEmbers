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
        public bool InRange = false;
        public InvaderRangedStruct RangedDamageStruct;
        public float MinDistance; //The distance the invader has decided it wants to get to before firing - created from Distance Range in the RangedDamageStruct

        //public int CurrentBurst;
        //public float CurrentBurstDelay, MinDistance;
        //public int RangedAttackPower, MaxBurst;
        //public Vector2 AngleRange, DistanceRange;
        //public InvaderFireType InvaderFireType;
        //public float MaxBurstDelay;
        
        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            if (RangedDamageStruct != null)
            {
                RangedDamageStruct.CurrentFireDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (RangedDamageStruct.CurrentFireDelay >= RangedDamageStruct.MaxFireDelay)
                {
                    CanAttack = true;
                    RangedDamageStruct.CurrentFireDelay = 0;
                }
                else
                {
                    CanAttack = false;
                }
            }

            //if (CurrentBurst >= MaxBurst &&
            //    CurrentBurstDelay < MaxBurstDelay)
            //{
            //    CurrentBurstDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            //}

            //if (CurrentBurstDelay >= MaxBurstDelay)
            //{
            //    CurrentBurstDelay = 0;
            //    CurrentBurst = 0;
            //}

            base.Update(gameTime, cursorPosition);
        }
    }
}
