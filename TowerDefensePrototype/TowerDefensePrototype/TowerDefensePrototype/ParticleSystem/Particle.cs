using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class Particle
    {
        public Texture2D Texture;
        public Vector2 CurrentPosition, Direction, Velocity, YRange, Origin, StartingPosition;
        public Rectangle DestinationRectangle;
        public float Angle, Speed, CurrentHP, MaxHP, CurrentTransparency, Scale, MaxY;
        public float RotationIncrement, CurrentRotation, Gravity, DrawDepth, Friction;
        public Color CurrentColor, EndColor, StartColor;
        public bool Active, Fade, BouncedOnGround, CanBounce, Shrink, StopBounce, HardBounce, Shadow, RotateVelocity;
        static Random Random = new Random();
        public SpriteEffects Orientation;

        public Particle(Texture2D texture, Vector2 position, float angle, float speed, float maxHP,
            float startingTransparency, bool fade, float startingRotation, float rotationChange,
            float scale, Color startColor, Color endColor, float gravity, bool canBounce, float maxY, bool shrink,
            float? drawDepth = null, bool? stopBounce = false, bool? hardBounce = true, bool? shadow = false, 
            bool? rotateVelocity = false, float? friction = null, SpriteEffects? orientation = SpriteEffects.None)
        {
            Active = true;
            Texture = texture;
            CurrentPosition = position;
            StartingPosition = position;
            Angle = angle;
            Speed = speed;
            MaxHP = maxHP;
            CurrentHP = maxHP;
            CurrentTransparency = startingTransparency;
            StartColor = startColor;
            CurrentColor = startColor;
            EndColor = endColor;
            Scale = scale;
            Fade = fade;
            Gravity = gravity;
            CanBounce = canBounce;
            Shrink = shrink;
            RotateVelocity = rotateVelocity.Value;

            //CurrentRotation = MathHelper.ToRadians(startingRotation);
            //RotationIncrement = MathHelper.ToRadians(rotationChange);
            RotationIncrement = rotationChange;
            CurrentRotation = startingRotation;

            //Direction.X = (float)Math.Sin(MathHelper.ToRadians(Angle));
            //Direction.Y = (float)Math.Cos(MathHelper.ToRadians(Angle));

            Direction.X = (float)Math.Cos(Angle);
            Direction.Y = (float)Math.Sin(Angle);

            Velocity = Direction * Speed;

            MaxY = maxY;

            if (drawDepth == null)
                DrawDepth = 0;
            else
                DrawDepth = drawDepth.Value;

            StopBounce = stopBounce.Value;

            HardBounce = hardBounce.Value;

            Shadow = shadow.Value;

            Orientation = orientation.Value;

            if (friction != null)
                Friction = friction.Value;
            else
                Friction = 0;     

            Origin = new Vector2(Texture.Width / 2, Texture.Height / 2);
        }

        public void Update(GameTime gameTime)
        {
            CurrentHP -= (float)(1 * gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

            if (CurrentHP <= 0)
                Active = false;

            else
                Active = true;

            if (Active == true)
            {
                CurrentRotation += RotationIncrement *((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                CurrentRotation = CurrentRotation % 360;
                CurrentPosition += Velocity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                Velocity.Y += Gravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                if (RotateVelocity == true)
                {
                    CurrentRotation = MathHelper.ToDegrees((float)Math.Atan2(Velocity.Y, Velocity.X));
                }

                DestinationRectangle = new Rectangle((int)CurrentPosition.X, (int)CurrentPosition.Y, (int)(Texture.Width * Scale), (int)(Texture.Height * Scale));
            }

            if (CanBounce == true)
                if (CurrentPosition.Y >= MaxY && BouncedOnGround == false)
                {
                    if (HardBounce == true)
                        CurrentPosition.Y -= Velocity.Y;// *((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                    Velocity.Y = (-Velocity.Y / 3);// *((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                    Velocity.X = (Velocity.X / 3);// *((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                    RotationIncrement = (RotationIncrement * 3);// *((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                    BouncedOnGround = true;
                }

            if (StopBounce == true &&
                BouncedOnGround == true &&
                CurrentPosition.Y > MaxY)
            {
                Velocity.Y = (-Velocity.Y / 2);// *((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                Velocity.X *= 0.9f;// *((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                RotationIncrement = MathHelper.Lerp(RotationIncrement, 0, 0.2f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));

                if (Velocity.Y < 0.2f && Velocity.Y > 0)
                {
                    Velocity.Y = 0;
                }

                if (Velocity.Y > -0.2f && Velocity.Y < 0)
                {
                    Velocity.Y = 0;
                }

                if (Velocity.X < 0.2f && Velocity.X > 0)
                {
                    Velocity.X = 0;
                }

                if (Velocity.X > -0.2f && Velocity.X < 0)
                {
                    Velocity.X = 0;
                }
            }

            if (Velocity.X > 0)
            {
                Velocity.X -= Friction;
            }

            if (Velocity.X < 0)
            {
                Velocity.X += Friction;
            }

            if (Velocity.Y == 0 && CanBounce == true)
            {
                if (CurrentRotation < 0)
                    CurrentRotation += 360;

                if (CurrentRotation > 270 && CurrentRotation <= 360)
                {
                    if (MathHelper.Distance(CurrentRotation, 360) >= 45)
                        CurrentRotation = MathHelper.Lerp(CurrentRotation, 360, 0.5f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
                    else
                        CurrentRotation = MathHelper.Lerp(CurrentRotation, 360, 0.2f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
                }

                if (CurrentRotation > 180 && CurrentRotation <= 270)
                {
                    if (MathHelper.Distance(CurrentRotation, 180) >= 45)
                        CurrentRotation = MathHelper.Lerp(CurrentRotation, 180, 0.5f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
                    else
                        CurrentRotation = MathHelper.Lerp(CurrentRotation, 180, 0.2f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
                }

                if (CurrentRotation > 90 && CurrentRotation <= 180)
                {
                    if (MathHelper.Distance(CurrentRotation, 180) >= 45)
                        CurrentRotation = MathHelper.Lerp(CurrentRotation, 180, 0.5f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
                    else
                        CurrentRotation = MathHelper.Lerp(CurrentRotation, 180, 0.2f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
                }

                if (CurrentRotation >= 0 && CurrentRotation <= 90)
                {
                    if (MathHelper.Distance(CurrentRotation, 0) >= 45)
                        CurrentRotation = MathHelper.Lerp(CurrentRotation, 0, 0.5f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
                    else
                        CurrentRotation = MathHelper.Lerp(CurrentRotation, 0, 0.2f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
                }
            }

            float PercentageHP = CurrentHP / MaxHP;

            if (Fade == true)
            {
                CurrentTransparency = MathHelper.Lerp(PercentageHP, CurrentTransparency, PercentageHP * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
                //CurrentTransparency = MathHelper.Lerp(1, 0, ((MaxHP / 100) * CurrentHP) / 100);
            }

            if (Shrink == true)
            {
                Scale = MathHelper.Lerp(Scale, (Scale * (PercentageHP)), PercentageHP * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
            }



            CurrentColor = Color.Lerp(CurrentColor, EndColor, (PercentageHP / (CurrentHP * 0.5f)) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
            //double PercentHP = (100 / MaxHP) * CurrentHP;
            //CurrentColor = Color.Lerp(StartColor, EndColor, PercentageHP/100);            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                spriteBatch.Draw(Texture, DestinationRectangle, null, Color.Lerp(Color.Transparent, CurrentColor, CurrentTransparency),
                                 MathHelper.ToRadians(CurrentRotation), Origin, Orientation, DrawDepth);
            }
        }

        public void DrawShadow(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                if (Shadow == true)
                {
                    float PercentToGround = (100 / (500 - MaxY)) * (CurrentPosition.Y - MaxY);
                    float SizeScale = (2 * PercentToGround) / 100;
                    float ColorScale = (150 - PercentToGround) / 100;

                    ColorScale = MathHelper.Clamp(ColorScale, 0.005f, 1f);
                    SizeScale = MathHelper.Clamp(SizeScale, 1f, 2f);

                    Vector2 ShadowScale = new Vector2(SizeScale * Scale, SizeScale * Scale);
                    Color ShadowColor = Color.Lerp(Color.Transparent, Color.Black, ColorScale * 0.05f);

                    spriteBatch.Draw(Texture,
                        new Rectangle((int)CurrentPosition.X, (int)MaxY + 4, (int)(Texture.Width * ShadowScale.X / 2), (int)(Texture.Height * ShadowScale.Y / 2)),
                        null, Color.Lerp(Color.Transparent, ShadowColor, CurrentTransparency),
                        MathHelper.ToRadians(CurrentRotation), Origin, SpriteEffects.None, (DestinationRectangle.Bottom / 1080));

                    spriteBatch.Draw(Texture,
                        new Rectangle((int)CurrentPosition.X, (int)MaxY + 4, (int)(Texture.Width * ShadowScale.X / 1.7f), (int)(Texture.Height * ShadowScale.Y / 1.5f)),
                        null, Color.Lerp(Color.Transparent, ShadowColor, CurrentTransparency),
                        MathHelper.ToRadians(CurrentRotation), Origin, SpriteEffects.None, (DestinationRectangle.Bottom / 1080));

                    spriteBatch.Draw(Texture,
                        new Rectangle((int)CurrentPosition.X, (int)MaxY + 4, (int)(Texture.Width * ShadowScale.X / 1.5f), (int)(Texture.Height * ShadowScale.Y / 1.5f)),
                        null, Color.Lerp(Color.Transparent, ShadowColor, CurrentTransparency),
                        MathHelper.ToRadians(CurrentRotation), Origin, SpriteEffects.None, (DestinationRectangle.Bottom / 1080));

                    spriteBatch.Draw(Texture,
                        new Rectangle((int)CurrentPosition.X, (int)MaxY + 4, (int)(Texture.Width * ShadowScale.X / 1.3f), (int)(Texture.Height * ShadowScale.Y / 1.5f)),
                        null, Color.Lerp(Color.Transparent, ShadowColor, CurrentTransparency),
                        MathHelper.ToRadians(CurrentRotation), Origin, SpriteEffects.None, (DestinationRectangle.Bottom / 1080));

                    spriteBatch.Draw(Texture,
                        new Rectangle((int)CurrentPosition.X, (int)MaxY + 4, (int)(Texture.Width * ShadowScale.X), (int)(Texture.Height * ShadowScale.Y)),
                        null, Color.Lerp(Color.Transparent, ShadowColor, CurrentTransparency),
                        MathHelper.ToRadians(CurrentRotation), Origin, SpriteEffects.None, (DestinationRectangle.Bottom / 1080));
                }
            }
        }
    }
}
