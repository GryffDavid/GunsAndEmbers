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
        public int RangedAttackPower, MaxBurst, CurrentBurst;
        public Vector2 AngleRange, Range;
        public float MaxBurstDelay, CurrentBurstDelay;

        public override void TrapDamage(Trap trap)
        {
            throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime)
        {            
            base.Update(gameTime);
            
            if (CurrentBurst >= MaxBurst &&
                CurrentBurstDelay < MaxBurstDelay)
            {
                CurrentBurstDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            if (CurrentBurstDelay >= MaxBurstDelay)
            {
                CurrentBurstDelay = 0;
                CurrentBurst = 0;
            }
        }
    }
}
