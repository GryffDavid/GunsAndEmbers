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
        public FlameProjectile (Vector2 position, float speed, float angle, float gravity)
        {
            Active = true;
            TextureName = "Blank";
            HeavyProjectileType = HeavyProjectileType.FlameThrower;
            Angle = angle;
            Speed = speed;
            Gravity = gravity;
            Position = position;

            Velocity.X = (float)(Math.Cos(angle) * speed);
            Velocity.Y = (float)(Math.Sin(angle) * speed);

            Color FireColor = Color.Orange;
            FireColor.A = 100;

            Color FireColor2 = Color.Orange;
            FireColor2.A = 200;

            Emitter = new Emitter("star", new Vector2(Position.X + 16, Position.Y + 8), new Vector2(75, 105), new Vector2(1.5f, 2), new Vector2(30, 35), 0.1f, true, new Vector2(-20, 20), new Vector2(-4, 4), new Vector2(1, 2f), FireColor, FireColor2, 0.0f, -1, 1, 1);
        }
    }
}
