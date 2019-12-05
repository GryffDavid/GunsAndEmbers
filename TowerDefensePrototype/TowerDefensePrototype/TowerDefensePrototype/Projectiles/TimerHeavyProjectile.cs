using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class TimerHeavyProjectile : HeavyProjectile
    {
        public double CurrentTime, MaxTime;
        public bool Detonated = false;

        public TimerHeavyProjectile(object source, float maxTime, Texture2D texture, Vector2 position, 
                                    float speed, float angle, float gravity, float damage, float blastRadius, Vector2? yrange = null, bool? verlet = false)
            : base(source, texture, position, speed, angle, gravity, damage, yrange, blastRadius, verlet)
        {
            MaxTime = maxTime;
        }

        public override void Update(GameTime gameTime)
        {
            CurrentTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentTime > MaxTime)
            {
                Detonated = true;
            }

            base.Update(gameTime);
        }
    }
}
