using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class Emitter
    {
        public Vector2 Position, AngleRange;
        List<Particle> ParticleList;
        Texture2D ParticleTexture;
        

        public Texture2D Texture;
        public Vector2 ScaleRange, LifeRange, RotationIncrementRange, SpeedRange, StartingRotationRange;
        public float Transparency, Gravity, ActiveSeconds, CurrentTime;
        public Color StartColor, EndColor;
        public bool Active, Fade;
        Random Random;

        public Emitter(Texture2D texture, Vector2 position, Vector2 angleRange, Vector2 speedRange, Vector2 lifeRange, float startingTransparency, bool fade, Vector2 startingRotationRange,
            Vector2 rotationIncrement, Vector2 scaleRange, Color startColor, Color endColor, float gravity, float activeSeconds)
        {
            Active = true;
            Texture = texture;
            SpeedRange = speedRange;
            LifeRange = lifeRange;
            Transparency = startingTransparency;
            Fade = fade;
            StartingRotationRange = startingRotationRange;
            RotationIncrementRange = rotationIncrement;
            ScaleRange = scaleRange;
            StartColor = startColor;
            EndColor = endColor;
            Position = position;
            ParticleList = new List<Particle>();
            AngleRange = angleRange;
            Gravity = gravity;
            ActiveSeconds = activeSeconds;
            Random = new Random();
        }

        public void LoadContent(ContentManager contentManager)
        {
            if (Active == true)
            {
                ParticleTexture = contentManager.Load<Texture2D>("star");
            }
        }

        public void Update(GameTime gameTime)
        {
            //Random = new Random();

            if (Active == true)
            {
                //If the emitter is given a value smaller than or equal to 0, it will carry on emitting infinitely//
                //If the value is bigger than zero, it will only emit particles for the length of time given//
                if (ActiveSeconds > 0)
                {
                    CurrentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (CurrentTime > ActiveSeconds)
                    {
                        Active = false;
                    }
                }                

                float angle, life, scale, rotation, speed, startingRotation;
                
                angle = -MathHelper.ToRadians(Random.Next((int)AngleRange.X, (int)AngleRange.Y));
                life = (float)DoubleRange(LifeRange.X, LifeRange.Y);
                scale = (float)DoubleRange(ScaleRange.X, ScaleRange.Y);
                rotation = (float)DoubleRange(RotationIncrementRange.X, RotationIncrementRange.Y);
                speed = (float)DoubleRange(SpeedRange.X, SpeedRange.Y);
                startingRotation = (float)DoubleRange(StartingRotationRange.X, StartingRotationRange.Y);

                ParticleList.Add(new Particle(Texture, Position, angle, speed, life, Transparency, Fade, startingRotation, rotation, scale, StartColor, EndColor, Gravity, false));

                foreach (Particle particle in ParticleList)
                {
                    particle.Update();
                }

                for (int i = 0; i < ParticleList.Count; i++)
                {
                    if (ParticleList[i].Active == false)
                        ParticleList.RemoveAt(i);
                }
                
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                foreach (Particle particle in ParticleList)
                {
                    particle.Draw(spriteBatch);
                }
            }
        }

        public double DoubleRange(double one, double two)
        {
            //Random rand = new Random();
            return one +   Random.NextDouble() * (two - one);
        }
    }
}
