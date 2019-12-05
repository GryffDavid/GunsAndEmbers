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
        public Vector2 ScaleRange, HPRange, RotationIncrementRange, SpeedRange, StartingRotationRange, EmitterDirection, EmitterVelocity;
        public float Transparency, Gravity, ActiveSeconds, CurrentTime, Interval, IntervalTime, MaxY, DrawDepth, EmitterSpeed, EmitterAngle, EmitterGravity;
        public Color StartColor, EndColor;
        public bool Active, Fade, CanBounce, AddMore, Shrink, StopBounce, HardBounce, BouncedOnGround, RotateVelocity;
        public string TextureName;
        public int Burst;
        static Random Random = new Random();

        public Emitter(String textureName, Vector2 position, Vector2 angleRange, Vector2 speedRange, Vector2 hpRange,
            float startingTransparency, bool fade, Vector2 startingRotationRange, Vector2 rotationIncrement, Vector2 scaleRange,
            Color startColor, Color endColor, float gravity, float activeSeconds, float interval, int burst, bool canBounce,
            Vector2 yrange, bool? shrink = null, float? drawDepth = null, bool? stopBounce = null, bool? hardBounce = null, Vector2? emitterSpeed = null,
            Vector2? emitterAngle = null, float? emitterGravity = null, bool? rotateVelocity = null)
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
            ActiveSeconds = activeSeconds;
            Interval = interval;
            IntervalTime = Interval;
            Burst = burst;
            CanBounce = canBounce;

            if (shrink == null)
                Shrink = false;
            else
                Shrink = shrink.Value;

            if (drawDepth == null)
                DrawDepth = 0;
            else
                DrawDepth = drawDepth.Value;

            if (stopBounce == null)
                StopBounce = false;
            else
                StopBounce = stopBounce.Value;

            if (hardBounce == null)
                HardBounce = false;
            else
                HardBounce = hardBounce.Value;

            if (emitterSpeed != null)
                EmitterSpeed = (float)DoubleRange(emitterSpeed.Value.X, emitterSpeed.Value.Y);
            else
                EmitterSpeed = 0;

            if (emitterAngle != null)
                EmitterAngle = -MathHelper.ToRadians((float)DoubleRange(emitterAngle.Value.X, emitterAngle.Value.Y));
            else
                EmitterAngle = 0;

            if (emitterGravity != null)
                EmitterGravity = emitterGravity.Value;
            else
                EmitterGravity = 0;

            if (EmitterSpeed != 0)
            {
                EmitterDirection.X = (float)Math.Sin(EmitterAngle);
                EmitterDirection.Y = (float)Math.Cos(EmitterAngle);
                EmitterVelocity = EmitterDirection * EmitterSpeed;
            }

            if (rotateVelocity != null)
                RotateVelocity = rotateVelocity.Value;
            else
                RotateVelocity = false;

            MaxY = Random.Next((int)yrange.X, (int)yrange.Y);
            AddMore = true;
        }

        public Emitter(Texture2D texture, Vector2 position, Vector2 angleRange, Vector2 speedRange, Vector2 hpRange,
           float startingTransparency, bool fade, Vector2 startingRotationRange, Vector2 rotationIncrement, Vector2 scaleRange,
           Color startColor, Color endColor, float gravity, float activeSeconds, float interval, int burst, bool canBounce,
           Vector2 yrange, bool? shrink = null, float? drawDepth = null, bool? stopBounce = null, bool? hardBounce = null, Vector2? emitterSpeed = null,
           Vector2? emitterAngle = null, float? emitterGravity = null, bool? rotateVelocity = null)
        {
            Active = true;
            Texture = texture;
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
            ActiveSeconds = activeSeconds;
            Interval = interval;
            IntervalTime = Interval;
            Burst = burst;
            CanBounce = canBounce;

            if (shrink == null)
                Shrink = false;
            else
                Shrink = shrink.Value;

            if (drawDepth == null)
                DrawDepth = 0;
            else
                DrawDepth = drawDepth.Value;

            if (stopBounce == null)
                StopBounce = false;
            else
                StopBounce = stopBounce.Value;

            if (hardBounce == null)
                HardBounce = false;
            else
                HardBounce = hardBounce.Value;

            if (emitterSpeed != null)
                EmitterSpeed = (float)DoubleRange(emitterSpeed.Value.X, emitterSpeed.Value.Y);
            else
                EmitterSpeed = 0;

            if (emitterAngle != null)
                EmitterAngle = -MathHelper.ToRadians((float)DoubleRange(emitterAngle.Value.X, emitterAngle.Value.Y));
            else
                EmitterAngle = 0;

            if (emitterGravity != null)
                EmitterGravity = emitterGravity.Value;
            else
                EmitterGravity = 0;

            if (EmitterSpeed != 0)
            {
                EmitterDirection.X = (float)Math.Sin(EmitterAngle);
                EmitterDirection.Y = (float)Math.Cos(EmitterAngle);
                EmitterVelocity = EmitterDirection * EmitterSpeed;
            }

            if (rotateVelocity != null)
                RotateVelocity = rotateVelocity.Value;
            else
                RotateVelocity = false;

            MaxY = Random.Next((int)yrange.X, (int)yrange.Y);
            AddMore = true;
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
                        AddMore = false;
                    }
                }

                if (EmitterSpeed != 0)
                {
                    EmitterVelocity.Y += EmitterGravity;
                    Position += EmitterVelocity;

                    if (CanBounce == true)
                        if (Position.Y >= MaxY && BouncedOnGround == false)
                        {
                            if (HardBounce == true)
                                Position.Y -= EmitterVelocity.Y;

                            EmitterVelocity.Y = -EmitterVelocity.Y / 3;
                            EmitterVelocity.X = EmitterVelocity.X / 3;
                            BouncedOnGround = true;
                        }

                    if (StopBounce == true &&
                        BouncedOnGround == true &&
                        Position.Y > MaxY)
                    {
                        EmitterVelocity.Y = -EmitterVelocity.Y / 2;

                        EmitterVelocity.X *= 0.9f;

                        if (EmitterVelocity.Y < 0.2f && EmitterVelocity.Y > 0)
                        {
                            EmitterVelocity.Y = 0;
                        }

                        if (EmitterVelocity.Y > -0.2f && EmitterVelocity.Y < 0)
                        {
                            EmitterVelocity.Y = 0;
                        }

                        if (EmitterVelocity.X < 0.2f && EmitterVelocity.X > 0)
                        {
                            EmitterVelocity.X = 0;
                        }

                        if (EmitterVelocity.X > -0.2f && EmitterVelocity.X < 0)
                        {
                            EmitterVelocity.X = 0;
                        }
                    }
                }

                if (Burst < 2)
                {
                    float angle, hp, scale, rotation, speed, startingRotation;
                    angle = -MathHelper.ToRadians((float)DoubleRange(AngleRange.X, AngleRange.Y));
                    hp = (float)DoubleRange(HPRange.X, HPRange.Y);
                    scale = (float)DoubleRange(ScaleRange.X, ScaleRange.Y);
                    rotation = (float)DoubleRange(RotationIncrementRange.X, RotationIncrementRange.Y);
                    speed = (float)DoubleRange(SpeedRange.X, SpeedRange.Y);
                    startingRotation = (float)DoubleRange(StartingRotationRange.X, StartingRotationRange.Y);

                    IntervalTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (IntervalTime > Interval && AddMore == true)
                    {
                        Particle NewParticle = new Particle(Texture, Position, angle, speed, hp, Transparency, Fade, startingRotation,
                            rotation, scale, StartColor, EndColor, Gravity, CanBounce, MaxY, Shrink, DrawDepth, StopBounce, HardBounce);
                        ParticleList.Add(NewParticle);
                        IntervalTime = 0;
                    }
                }
                else
                {
                    IntervalTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (IntervalTime > Interval && AddMore == true)
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
                            ParticleList.Add(new Particle(Texture, Position, angle, speed, hp, Transparency, Fade, startingRotation,
                                rotation, scale, StartColor, EndColor, Gravity, CanBounce, MaxY, Shrink, DrawDepth, StopBounce, HardBounce, false, RotateVelocity));
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
            return one + Random.NextDouble() * (two - one);
        }
    }
}
