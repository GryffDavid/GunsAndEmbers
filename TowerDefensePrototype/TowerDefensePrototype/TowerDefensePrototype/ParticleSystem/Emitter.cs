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
        public Vector2 ScaleRange, HPRange, RotationIncrementRange, SpeedRange, StartingRotationRange, EmitterDirection, EmitterVelocity, YRange;
        public float Transparency, Gravity, ActiveSeconds, Interval, MaxY, EmitterSpeed,
                     EmitterAngle, EmitterGravity, Friction, FadeDelay, DrawDepth, StartingInterval;
        public Color StartColor, EndColor;
        public bool Active, Fade, CanBounce, AddMore, Shrink, StopBounce, HardBounce, BouncedOnGround, RotateVelocity, FlipHor, FlipVer, ReduceDensity;
        public string TextureName;
        public int Burst;
        static Random Random = new Random();
        public double IntervalTime, CurrentTime;
        public SpriteEffects Orientation = SpriteEffects.None;

        public Emitter(String textureName, Vector2 position, Vector2 angleRange, Vector2 speedRange, Vector2 hpRange,
            float startingTransparency, bool fade, Vector2 startingRotationRange, Vector2 rotationIncrement, Vector2 scaleRange,
            Color startColor, Color endColor, float gravity, float activeSeconds, float interval, int burst, bool canBounce,
            Vector2 yrange, bool? shrink = null, float? drawDepth = null, bool? stopBounce = null, bool? hardBounce = null, 
            Vector2? emitterSpeed = null, Vector2? emitterAngle = null, float? emitterGravity = null, 
            bool? rotateVelocity = null, float? friction = null, bool? flipHor = null, bool? flipVer = null, 
            float? fadeDelay = null, bool? reduceDensity = null)
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
            StartingInterval = interval;
            IntervalTime = Interval;            
            Burst = burst;
            CanBounce = canBounce;

            if (fadeDelay != null)
                FadeDelay = fadeDelay.Value;
            else
                FadeDelay = 0;

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


            if (flipVer == null)
                FlipVer = false;
            else
                FlipVer = flipVer.Value;
            
            if (flipHor == null)
                FlipHor = false;
            else
                FlipHor = flipHor.Value;


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

            if (reduceDensity != null)
                ReduceDensity = reduceDensity.Value;
            else
                ReduceDensity = false;

            if (friction != null)
                Friction = friction.Value;
            else
                Friction = 0;

            YRange = yrange;
            MaxY = Random.Next((int)yrange.X, (int)yrange.Y);
            AddMore = true;
        }

        public Emitter(Texture2D texture, Vector2 position, Vector2 angleRange, Vector2 speedRange, Vector2 hpRange,
           float startingTransparency, bool fade, Vector2 startingRotationRange, Vector2 rotationIncrement, Vector2 scaleRange,
           Color startColor, Color endColor, float gravity, float activeSeconds, float interval, int burst, bool canBounce,
           Vector2 yrange, bool? shrink = null, float? drawDepth = null, bool? stopBounce = null, bool? hardBounce = null, 
           Vector2? emitterSpeed = null, Vector2? emitterAngle = null, float? emitterGravity = null, bool? rotateVelocity = null,
            float? friction = null, bool? flipHor = null, bool? flipVer = null, float? fadeDelay = null, bool? reduceDensity = null)
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
            StartingInterval = interval;
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
            
            if (friction != null)
                Friction = friction.Value;
            else
                Friction = 0;

            if (fadeDelay != null)
                FadeDelay = fadeDelay.Value;
            else
                FadeDelay = 0;

            if (emitterSpeed != null)
                EmitterSpeed = (float)DoubleRange(emitterSpeed.Value.X, emitterSpeed.Value.Y);
            else
                EmitterSpeed = 0;

            if (emitterAngle != null)
            {
                EmitterAngle = -MathHelper.ToRadians((float)DoubleRange(emitterAngle.Value.X, emitterAngle.Value.Y));
            }
            else
            {
                EmitterAngle = 0;
            }

            if (emitterGravity != null)
                EmitterGravity = emitterGravity.Value;
            else
                EmitterGravity = 0;

            if (EmitterSpeed != 0)
            {
                EmitterDirection.X = (float)Math.Cos(EmitterAngle);
                EmitterDirection.Y = (float)Math.Sin(EmitterAngle);
                EmitterVelocity = EmitterDirection * EmitterSpeed;
                AngleRange = new Vector2(
                                -(MathHelper.ToDegrees((float)Math.Atan2(-EmitterVelocity.Y, -EmitterVelocity.X))) - 20,
                                -(MathHelper.ToDegrees((float)Math.Atan2(-EmitterVelocity.Y, -EmitterVelocity.X))) + 20);
            }

            if (rotateVelocity != null)
                RotateVelocity = rotateVelocity.Value;
            else
                RotateVelocity = false;

            if (reduceDensity != null)
                ReduceDensity = reduceDensity.Value;
            else
                ReduceDensity = false;

            if (flipHor == null)
                FlipHor = false;
            else
                FlipHor = flipHor.Value;

            if (flipVer == null)
                FlipVer = false;
            else
                FlipVer = flipVer.Value;

            YRange = yrange;
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
                    CurrentTime += gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (CurrentTime > ActiveSeconds*1000)
                    {
                        AddMore = false;
                    }
                }

                if (ReduceDensity == true)
                {
                    //After halftime, begin reducing the density from 100% down to 0% as the time continues to expire                    
                    //Interval = MathHelper.Lerp((float)Interval, (float)(Interval * 5), 0.0001f);
                    float PercentageThrough = ((float)CurrentTime / (ActiveSeconds * 1000)) * 100;

                    if (PercentageThrough >= 50)
                        Interval = StartingInterval + (Interval / 100 * PercentageThrough);
                    
                }

                if (EmitterSpeed != 0)
                {
                    EmitterVelocity.Y += EmitterGravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                    Position += EmitterVelocity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                    if (CanBounce == true)
                        if (Position.Y >= MaxY && BouncedOnGround == false)
                        {
                            if (HardBounce == true)
                                Position.Y -= EmitterVelocity.Y * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                            EmitterVelocity.Y = (-EmitterVelocity.Y / 3) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                            EmitterVelocity.X = (EmitterVelocity.X / 3) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                            BouncedOnGround = true;
                        }

                    if (StopBounce == true &&
                        BouncedOnGround == true &&
                        Position.Y > MaxY)
                    {
                        EmitterVelocity.Y = (-EmitterVelocity.Y / 2) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                        EmitterVelocity.X *= 0.9f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

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

                if (FlipHor == true && FlipVer == false)
                {
                    Orientation = RandomOrientation(SpriteEffects.None, SpriteEffects.FlipHorizontally);
                }

                if (FlipHor == false && FlipVer == true)
                {
                    Orientation = RandomOrientation(SpriteEffects.None, SpriteEffects.FlipVertically);
                }

                if (FlipHor == true && FlipVer == true)
                {
                    Orientation = RandomOrientation(SpriteEffects.None, SpriteEffects.FlipVertically, SpriteEffects.FlipHorizontally);
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
                    MaxY = Random.Next((int)YRange.X, (int)YRange.Y);
                    IntervalTime += gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (IntervalTime > Interval && AddMore == true)
                    {
                        Particle NewParticle = new Particle(Texture, Position, angle, speed, hp, Transparency, Fade, startingRotation,
                                                            rotation, scale, StartColor, EndColor, Gravity, CanBounce, MaxY, Shrink,
                                                            DrawDepth, StopBounce, HardBounce, false, RotateVelocity, Friction, Orientation, FadeDelay);
                        ParticleList.Add(NewParticle);
                        IntervalTime = 0;
                    }
                }
                else
                {
                    IntervalTime += gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (IntervalTime > Interval && AddMore == true)
                    {
                        int newBurst = (int)(Burst * (IntervalTime / Interval));

                        for (int i = 0; i < newBurst; i++)
                        {
                            float angle, hp, scale, rotation, speed, startingRotation;

                            angle = -MathHelper.ToRadians(Random.Next((int)AngleRange.X, (int)AngleRange.Y));
                            hp = (float)DoubleRange(HPRange.X, HPRange.Y);
                            scale = (float)DoubleRange(ScaleRange.X, ScaleRange.Y);
                            rotation = (float)DoubleRange(RotationIncrementRange.X, RotationIncrementRange.Y);
                            speed = (float)DoubleRange(SpeedRange.X, SpeedRange.Y);
                            startingRotation = (float)DoubleRange(StartingRotationRange.X, StartingRotationRange.Y);
                            MaxY = Random.Next((int)YRange.X, (int)YRange.Y);

                            ParticleList.Add(new Particle(Texture, Position, angle, speed, hp, Transparency, Fade, startingRotation,
                                                          rotation, scale, StartColor, EndColor, Gravity, CanBounce, MaxY, Shrink,
                                                          DrawDepth, StopBounce, HardBounce, false, RotateVelocity, Friction, Orientation, FadeDelay));
                        }
                        IntervalTime = 0;
                    }
                }
            }

            //for (int i = 0; i < ParticleList.Count; i++)
            //{
            //    if (ParticleList[i].Active == false)
            //        ParticleList.RemoveAt(i);
            //}

            ParticleList.RemoveAll(Particle => Particle.Active == false);

            foreach (Particle particle in ParticleList)
            {
                particle.Update(gameTime);
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

        private SpriteEffects RandomOrientation(params SpriteEffects[] Orientations)
        {
            List<SpriteEffects> OrientationList = new List<SpriteEffects>();

            foreach (SpriteEffects orientation in Orientations)
            {
                OrientationList.Add(orientation);
            }

            return OrientationList[Random.Next(0, OrientationList.Count)];
        }
    }
}
