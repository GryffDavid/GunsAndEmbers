using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class  CannonBall : HeavyProjectile
    {
        Vector2 startingPosition;

        public CannonBall(Vector2 position, float speed, float angle, float gravity, float damage, float blastRadius, Vector2? yrange = null)
        {
            Active = true;
            Rotate = true;
            Fade = false;
            TextureName = "Projectiles/CannonRound";
            HeavyProjectileType = HeavyProjectileType.CannonBall;
            Angle = angle;
            Speed = speed;
            Gravity = gravity;
            Position = position;
            startingPosition = position;
            BlastRadius = blastRadius;
            EmitterList = new List<Emitter>();

            if (yrange == null)
            {
                YRange = new Vector2(500, 600);
            }
            else
            {
                YRange = yrange.Value;
            }

            Velocity.X = (float)(Math.Cos(angle) * speed);
            Velocity.Y = (float)(Math.Sin(angle) * speed);

            Color ParticleColor2 = Color.Lerp(Color.DarkGray, Color.Transparent, 0.25f);
            Color ParticleColor1 = Color.Lerp(Color.Gray, Color.Transparent, 0.25f);

            EmitterList.Add(new Emitter("Particles/Smoke", new Vector2(Position.X + 16, Position.Y + 8), new Vector2(90, 180),
                new Vector2(0, 0), new Vector2(40, 60), 0.9f, true, new Vector2(0, 360), new Vector2(-0.5f, 0.5f),
                new Vector2(0.25f, 0.5f), ParticleColor1, ParticleColor2, -0.00f, -1, 10, 1, false, new Vector2(0, 720), null, null,
                null, null, null, null, null, null, null, true, true));

            Damage = damage;
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Particle particle in EmitterList[0].ParticleList)
            {
                if (Vector2.Distance(particle.StartingPosition, startingPosition) < 64 && 
                    (particle.MaxHP - particle.CurrentHP) <= 18 &&
                    (particle.MaxHP - particle.CurrentHP) >= 7)
                {
                    //Change the speed of the smoke particle based on it's distance to 64 pixels from origin
                    particle.Velocity.X = (float)(Math.Cos(Angle) * Speed);
                    particle.Velocity.Y = (float)(Math.Sin(Angle) * Speed);
                    particle.Friction = 0.5f;
                    particle.Shrink = true;
                    //particle.Gravity = -0.1f;
                }
            }

            base.Update(gameTime);
        }
    }
}
