using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class DropMissile : HeavyProjectile
    {
        float CurrentDropTime, MaxDropTime;
        Texture2D ParticleTexture;

        public DropMissile(object source, Texture2D texture, Texture2D particleTexture, Vector2 position,
                           float speed, float angle, float gravity, float damage, float blastRadius, Vector2? yRange = null)
            : base(source, texture, position, speed, angle, gravity, damage, yRange, blastRadius)
        {
            ParticleTexture = particleTexture;
            HeavyProjectileType = HeavyProjectileType.DropMissile;
            Rotate = false;
            CurrentRotation = MathHelper.ToRadians(150f); ;
            MaxDropTime = 600f;            
        }

        public override void Update(GameTime gameTime)
        {
            CurrentDropTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentDropTime > MaxDropTime)
            {
                if (EmitterList.Count == 0)
                {
                    EmitterList.Add(new Emitter(ParticleTexture, new Vector2(Position.X + 16, Position.Y + 8), new Vector2(0f, 0f),
                                                    new Vector2(6.8f, 6.8f), new Vector2(150, 150), 1f, true,
                                                    new Vector2(0f, 360f), new Vector2(-2f, 2f), new Vector2(1f, 1f),
                                                    new Color(255, 128, 0, 63), new Color(0, 0, 255, 255), 0f, 0f, 16f,
                                                    4, false, new Vector2(0f, 720f), true, 0, false, false,
                                                    new Vector2(0f, 0f), new Vector2(0f, 0f), 0f, false, new Vector2(0.045f, 0.045f),
                                                    false, false, 0f, false, false, false, null));
                }

                if (Velocity.X > -40f)
                    Velocity.X -= 3f;

                Gravity = 0.05f;
                if (Velocity.Y > 0)
                {
                    Velocity.Y -= 1f;
                }

                Rotate = true;
            }

            base.Update(gameTime);
        }
    }
}
