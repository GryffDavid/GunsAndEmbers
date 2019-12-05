using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class  ClusterBomb : HeavyProjectile
    {
        public ClusterBomb(Vector2 position, float speed, float angle, float gravity, float damage, Vector2? yrange = null)
        {
            Active = true;
            Rotate = true;
            Fade = false;
            TextureName = "Projectiles/ClusterBomb";
            HeavyProjectileType = HeavyProjectileType.ClusterBomb;
            Angle = angle;
            Speed = speed;
            Gravity = gravity;
            Position = position;
            EmitterList = new List<Emitter>();

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

            EmitterList.Add(new Emitter("Particles/Smoke", new Vector2(Position.X + 16, Position.Y + 8), new Vector2(90, 180), 
                new Vector2(1.5f, 2), new Vector2(15, 20), 0.2f, true, new Vector2(-20, 20), new Vector2(-4, 4), 
                new Vector2(0.1f, 0.25f), ParticleColor1, ParticleColor2, 0.0f, -1, 1, 1, false, new Vector2(0, 720)));

            Damage = 20;

            //YRange = new Vector2(520, 630);
        }
    }
}
