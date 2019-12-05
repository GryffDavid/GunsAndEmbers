using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class FlameProjectile : HeavyProjectile
    {
        public FlameProjectile(object source, Texture2D texture, Texture2D particleTexture, Vector2 position, 
                               float speed, float angle, float gravity, float damage, Vector2? yrange = null)
            : base(source, texture, position, speed, angle, gravity, damage, yrange)
        {
            HeavyProjectileType = HeavyProjectileType.FlameThrower;

            Active = true;
            Rotate = true;

            Color FireColor = Color.Orange;
            FireColor.A = 100;

            Color FireColor2 = Color.Orange;
            FireColor2.A = 200;

            EmitterList = new List<Emitter>();
            EmitterList.Add(new Emitter(particleTexture, new Vector2(Position.X + 16, Position.Y + 8),
                new Vector2(90, 90),
                new Vector2(1.5f, 2), new Vector2(30, 35), 0.1f, true,
                new Vector2(-20, 20), new Vector2(-4, 4),
                new Vector2(1, 2f), FireColor, FireColor2, 0.0f, -1, 1, 1, false, new Vector2(0, 720)));
        }
    }
}
