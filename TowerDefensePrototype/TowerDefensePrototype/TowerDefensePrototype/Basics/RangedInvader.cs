using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    abstract class RangedInvader : Invader
    {
        public HeavyProjectileType ProjectileType;

        public override void Update(GameTime gameTime)
        {
            
            base.Update(gameTime);
        }

        public override void TrapDamage(TrapType trapType)
        {
            throw new NotImplementedException();
        }
    }
}
