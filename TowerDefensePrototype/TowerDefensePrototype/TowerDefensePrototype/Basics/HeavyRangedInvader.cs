using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    abstract class HeavyRangedInvader : Invader
    {
        public int RangedAttackPower;
        public Vector2 AngleRange, Range, PowerRange;

        public override void TrapDamage(TrapType trapType)
        {
            throw new NotImplementedException();
        }
    }
}
