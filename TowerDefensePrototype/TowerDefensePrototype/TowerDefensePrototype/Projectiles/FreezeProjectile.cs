using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class FreezeProjectile : LightProjectile
    {
        public FreezeProjectile(Vector2 position, Vector2 direction, float? damage = null)
            : base(position, direction, damage)
        {
            LightProjectileType = LightProjectileType.Freeze;
        }
    }
}
