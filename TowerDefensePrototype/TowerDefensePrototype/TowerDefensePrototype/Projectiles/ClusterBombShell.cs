using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace TowerDefensePrototype
{
    class ClusterBombShell : TimerHeavyProjectile
    {
        public ClusterBombShell(float maxTime, Vector2 position, float speed, float angle, float gravity, float damage, Vector2? yrange = null)
        {
            MaxTime = maxTime;

            Active = true;
            Rotate = true;
            Fade = false;
            TextureName = "Projectiles/ClusterBombShell";
            HeavyProjectileType = HeavyProjectileType.ClusterBombShell;
            Angle = angle;
            Speed = speed;
            Gravity = gravity;
            Position = position;

            if (yrange == null)
            {
                YRange = new Vector2(520, 630);
            }
            else
            {
                YRange = yrange.Value;
            }

            Velocity.X = (float)(Math.Cos(angle) * speed);
            Velocity.Y = (float)(Math.Sin(angle) * speed);

            Color ParticleColor1 = Color.Gray;
            Color ParticleColor2 = Color.DarkGray;

            Emitter = new Emitter("Particles/Smoke", new Vector2(Position.X + 16, Position.Y + 8), new Vector2(90, 180),
                new Vector2(1.5f, 2), new Vector2(15, 20), 0.2f, true, new Vector2(-20, 20), new Vector2(-4, 4),
                new Vector2(0.25f, 0.5f), ParticleColor1, ParticleColor2, 0.0f, -1, 1, 1, false, new Vector2(0, 720));

            Damage = 50;
        }
    }
}
