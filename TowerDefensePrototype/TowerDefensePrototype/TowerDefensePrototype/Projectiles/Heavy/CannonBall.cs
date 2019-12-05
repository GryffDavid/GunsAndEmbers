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
        public CannonBall(object source, Texture2D texture, Texture2D particleTexture, Vector2 position, 
                          float speed, float angle, float gravity, float damage, float blastRadius, Vector2? yrange = null) 
            : base(source, texture, position, speed, angle, gravity, damage, yrange, blastRadius, false)
        {
            HeavyProjectileType = HeavyProjectileType.CannonBall;

            EmitterList.Add(new Emitter(particleTexture, new Vector2(Position.X + 16, Position.Y + 8), new Vector2(90, 180),
                new Vector2(0, 0), new Vector2(640, 960), 0.75f, true, new Vector2(0, 360), new Vector2(-0.5f, 0.5f),
                new Vector2(0.25f, 0.5f), Color.DarkGray, Color.DarkGray, -0.00f, -1, 10, 1, false, new Vector2(0, 720), null, DrawDepth,
                null, null, null, null, null, null, null, true, true));
        }

        //public override void Update(GameTime gameTime)
        //{
        //    //THIS WAS TO MAKE THE SMOKE TRAIL MOVE IF IT'S CLOSE TO THE BARREL
        //    //MIGHT BE EXPENSIVE THOUGH
        //    //foreach (Particle particle in EmitterList[0].ParticleList)
        //    //{
        //    //    if (Vector2.Distance(particle.StartingPosition, startingPosition) < 64 && 
        //    //        (particle.MaxHP - particle.CurrentHP) <= 18 &&
        //    //        (particle.MaxHP - particle.CurrentHP) >= 7)
        //    //    {
        //    //        //Change the speed of the smoke particle based on it's distance to 64 pixels from origin
        //    //        particle.Velocity.X = (float)(Math.Cos(Angle) * Speed);
        //    //        particle.Velocity.Y = (float)(Math.Sin(Angle) * Speed);
        //    //        particle.Friction = new Vector2(0.5f, 0.5f);
        //    //        particle.Shrink = true;
        //    //        //particle.Gravity = -0.1f;
        //    //    }
        //    //}

        //    base.Update(gameTime);
        //}
    }
}
