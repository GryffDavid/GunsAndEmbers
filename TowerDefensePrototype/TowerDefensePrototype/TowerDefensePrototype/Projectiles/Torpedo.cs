using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class Torpedo : HeavyProjectile
    {
        public Torpedo(Vector2 position, float speed, float angle, float gravity, float damage, Vector2? yrange = null)
        {
            Active = true;
            Rotate = true;
            Fade = false;
            TextureName = "Projectiles/Torpedo";
            HeavyProjectileType = HeavyProjectileType.Torpedo;
            Angle = angle;
            Speed = speed;
            Gravity = gravity;
            Position = position;

            Velocity.X = (float)(Math.Cos(angle) * speed);
            Velocity.Y = (float)(Math.Sin(angle) * speed);

            Emitter = new Emitter("Particles/Smoke", Position,
                new Vector2((float)Math.Atan2(Velocity.Y, Velocity.X),(float)Math.Atan2(Velocity.Y, Velocity.X)),
                new Vector2(0, 0), new Vector2(40, 50), 0.5f, true, new Vector2(0, 0),
                new Vector2(0, 2), new Vector2(0.2f, 0.3f), Color.MediumPurple, Color.Purple, 0, -1, 1, 10, false, new Vector2(0, 720), true);
            
            Rotation = (float)Math.Atan2(Velocity.Y, Velocity.X);

            if (yrange == null)
            {
                YRange = new Vector2(520, 630);
            }
            else
            {
                YRange = yrange.Value;
            }


            Damage = 100;
        }
    }
}
