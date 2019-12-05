using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class Emitter
    {
        public Vector2 Position, AngleRange;
        public List<Particle> ParticleList;       
        public Texture2D Texture;
        public Vector2 ScaleRange, HPRange, RotationIncrementRange, SpeedRange, StartingRotationRange;
        public float Transparency, Gravity, ActiveSeconds, CurrentTime, Interval, IntervalTime;
        public Color StartColor, EndColor;
        public bool Active, Fade, CanBounce;
        public string TextureName;
        public int Burst;
        Random Random;

        public Emitter(String textureName, Vector2 position, Vector2 angleRange, Vector2 speedRange, Vector2 hpRange, float startingTransparency, bool fade, Vector2 startingRotationRange,
            Vector2 rotationIncrement, Vector2 scaleRange, Color startColor, Color endColor, float gravity, float activeSeconds, float interval, int burst, bool canBounce)
        {
            Active = true;
            TextureName = textureName;
            SpeedRange = speedRange;
            HPRange = hpRange;
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
            ActiveSeconds = activeSeconds;;
            Interval = interval;
            Burst = burst;
            CanBounce = canBounce;
            Random = new Random();
        }

        public void LoadContent(ContentManager contentManager)
        {
            if (Active == true)
            {
                Texture = contentManager.Load<Texture2D>(TextureName);
            }
        }

        public void Update(GameTime gameTime)
        {
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

                if (Burst < 2)
                {
                    float angle, hp, scale, rotation, speed, startingRotation;
                    angle = -MathHelper.ToRadians(Random.Next((int)AngleRange.X, (int)AngleRange.Y));
                    hp = (float)DoubleRange(HPRange.X, HPRange.Y);
                    scale = (float)DoubleRange(ScaleRange.X, ScaleRange.Y);
                    rotation = (float)DoubleRange(RotationIncrementRange.X, RotationIncrementRange.Y);
                    speed = (float)DoubleRange(SpeedRange.X, SpeedRange.Y);
                    startingRotation = (float)DoubleRange(StartingRotationRange.X, StartingRotationRange.Y);

                    IntervalTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (IntervalTime > Interval)
                    {
                        ParticleList.Add(new Particle(Texture, Position, angle, speed, hp, Transparency, Fade, startingRotation, rotation, scale, StartColor, EndColor, Gravity, CanBounce));
                        IntervalTime = 0;
                    }
                }
                else
                {
                    IntervalTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (IntervalTime > Interval)
                    {
                        for (int i = 0; i < Burst; i++)
                        {
                            float angle, hp, scale, rotation, speed, startingRotation;

                            angle = -MathHelper.ToRadians(Random.Next((int)AngleRange.X, (int)AngleRange.Y));
                            hp = (float)DoubleRange(HPRange.X, HPRange.Y);
                            scale = (float)DoubleRange(ScaleRange.X, ScaleRange.Y);
                            rotation = (float)DoubleRange(RotationIncrementRange.X, RotationIncrementRange.Y);
                            speed = (float)DoubleRange(SpeedRange.X, SpeedRange.Y);
                            startingRotation = (float)DoubleRange(StartingRotationRange.X, StartingRotationRange.Y);
                            ParticleList.Add(new Particle(Texture, Position, angle, speed, hp, Transparency, Fade, startingRotation, rotation, scale, StartColor, EndColor, Gravity, CanBounce));
                        }
                        IntervalTime = 0;
                    }
                }                                   
            }

            for (int i = 0; i < ParticleList.Count; i++)
            {
                if (ParticleList[i].Active == false)
                    ParticleList.RemoveAt(i);
            }     

            foreach (Particle particle in ParticleList)
            {
                particle.Update();
            }
        }      

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Particle particle in ParticleList)
            {
                particle.Draw(spriteBatch);
            }
        }

        public double DoubleRange(double one, double two)
        {
            return one +   Random.NextDouble() * (two - one);
        }
    }
}
