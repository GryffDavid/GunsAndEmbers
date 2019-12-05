using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class AcidProjectile : HeavyProjectile
    {
        public AcidProjectile(Vector2 position, float speed, float angle, float gravity, Vector2? yrange = null)
        {
            Active = true;
            Rotate = true;
            Fade = false;
            TextureName = "Projectiles/AcidProjectileSprite";
            HeavyProjectileType = HeavyProjectileType.Acid;
            Angle = angle;
            Speed = speed;
            Gravity = gravity;
            Position = position;

            Velocity.X = (float)(Math.Cos(angle) * speed);
            Velocity.Y = (float)(Math.Sin(angle) * speed);

            Color FireColor = Color.Lime;

            Color FireColor2 = Color.LimeGreen;

            Emitter = new Emitter("Particles/Splodge", new Vector2(Position.X + 16, Position.Y + 8),
                new Vector2(90, 90),
                new Vector2(1.5f, 2), new Vector2(30, 45), 0.1f, true,
                new Vector2(-20, 20), new Vector2(-4, 4),
                new Vector2(0.1f, 0.25f), FireColor, FireColor2, 0.2f, -1, 1, 1, false, new Vector2(0, 720), true);

            if (yrange == null)
            {
                YRange = new Vector2(520, 630);
            }
            else
            {
                YRange = yrange.Value;
            }


            Damage = 50;
        }
    }
}
