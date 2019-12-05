using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class StickyMineProjectile : HeavyProjectile
    {
        public StickyMineProjectile(object source, Texture2D texture, Texture2D particleTexture, Vector2 position,
            float speed, float angle, float gravity, float damage, float blastRadius, Vector2? yrange = null) 
            : base(source, texture, position, speed, angle, gravity, damage, yrange, blastRadius, false)
        {
            HeavyProjectileType = HeavyProjectileType.StickyMine;
        }
    }
}
