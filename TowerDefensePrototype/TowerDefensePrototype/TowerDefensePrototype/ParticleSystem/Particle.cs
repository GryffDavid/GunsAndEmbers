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
        public float RotationIncrement, CurrentRotation, Gravity, DrawDepth;
        public Color CurrentColor, EndColor, StartColor;
        public bool Active, Fade, BouncedOnGround, CanBounce, Shrink, StopBounce, HardBounce, Shadow;
        static Random Random = new Random();

        public Particle(Texture2D texture, Vector2 position, float angle, float speed, float maxHP,
            float startingTransparency, bool fade, float startingRotation, float rotationChange,
            float scale, Color startColor, Color endColor, float gravity, bool canBounce, float maxY, bool shrink, 
            float? drawDepth = null, bool? stopBounce = false, bool? hardBounce = true, bool? shadow = false)
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

            Origin = new Vector2(Texture.Width / 2, Texture.Height / 2);
        }

        public void Update()
        {
            CurrentHP--;            

            if (CurrentHP <= 0)
                Active = false;

            else
                Active = true;

            if (CanBounce == true)
                if (CurrentPosition.Y >= MaxY  && BouncedOnGround == false)
                {
                    if (HardBounce == true)
                    CurrentPosition.Y -= Velocity.Y;

                    Velocity.Y = -Velocity.Y / 3;
                    Velocity.X = Velocity.X / 3;                    
                    RotationIncrement = RotationIncrement * 3;
                    BouncedOnGround = true;
                }

            if (StopBounce == true && 
                BouncedOnGround == true && 
                CurrentPosition.Y > MaxY)
            {
                Velocity.Y = -Velocity.Y / 2;

                Velocity.X *= 0.9f;

                RotationIncrement = MathHelper.Lerp(RotationIncrement, 0, 0.2f);

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

            if (Velocity.Y == 0)
            {               
                if (CurrentRotation < 0)
                    CurrentRotation += 360;

                if (CurrentRotation > 270 && CurrentRotation <= 360)
                {
                    if (MathHelper.Distance(CurrentRotation, 360) >= 45)
                        CurrentRotation = MathHelper.Lerp(CurrentRotation, 360, 0.5f);
                    else
                        CurrentRotation = MathHelper.Lerp(CurrentRotation, 360, 0.2f);
                }

                if (CurrentRotation > 180 && CurrentRotation <= 270)
                {
                    if (MathHelper.Distance(CurrentRotation, 180) >= 45)
                        CurrentRotation = MathHelper.Lerp(CurrentRotation, 180, 0.5f);
                    else
                    CurrentRotation = MathHelper.Lerp(CurrentRotation, 180, 0.2f);
                }

                if (CurrentRotation > 90 && CurrentRotation <= 180)
                {
                    if (MathHelper.Distance(CurrentRotation, 180) >= 45)
                        CurrentRotation = MathHelper.Lerp(CurrentRotation, 180, 0.5f);
                    else
                        CurrentRotation = MathHelper.Lerp(CurrentRotation, 180, 0.2f);
                }

                if (CurrentRotation >= 0 && CurrentRotation <= 90)
                {
                    if (MathHelper.Distance(CurrentRotation, 0) >= 45)
                        CurrentRotation = MathHelper.Lerp(CurrentRotation, 0, 0.5f);
                    else
                        CurrentRotation = MathHelper.Lerp(CurrentRotation, 0, 0.2f);
                }

                DrawDepth = DestinationRectangle.Bottom / 1080;
            }

            if (Active == true)
            {
                CurrentRotation += RotationIncrement;
                CurrentRotation = CurrentRotation % 360;
                CurrentPosition += Velocity;
                Velocity.Y += Gravity;
                DestinationRectangle = new Rectangle((int)CurrentPosition.X, (int)CurrentPosition.Y, (int)(Texture.Width * Scale), (int)(Texture.Height * Scale));
            }

            float PercentageHP = CurrentHP / MaxHP;

            if (Fade == true)
            {
                CurrentTransparency = MathHelper.Lerp(PercentageHP, CurrentTransparency, PercentageHP);
                //CurrentTransparency = MathHelper.Lerp(1, 0, ((MaxHP / 100) * CurrentHP) / 100);
            }

            if (Shrink == true)
            {
                Scale = MathHelper.Lerp(Scale, (Scale*(PercentageHP)), PercentageHP);
            }

            CurrentColor = Color.Lerp(CurrentColor, EndColor, PercentageHP / (CurrentHP * 0.5f));
            //double PercentHP = (100 / MaxHP) * CurrentHP;
            //CurrentColor = Color.Lerp(StartColor, EndColor, PercentageHP/100);            
        }

        public void Draw(SpriteBatch spriteBatch)
        { 
            if (Active == true)
            {
                spriteBatch.Draw(Texture, DestinationRectangle, null, Color.Lerp(Color.Transparent, CurrentColor, CurrentTransparency),
                                 MathHelper.ToRadians(CurrentRotation), Origin, SpriteEffects.None, DrawDepth);
            }
        }

        public void DrawShadow(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                if (Shadow == true)
                {
                    float PercentToGround = (100 / (500 - MaxY)) * (CurrentPosition.Y - MaxY);
                    float SizeScale = (2*PercentToGround) / 100;
                    float ColorScale = (150 - PercentToGround) / 100;

                    ColorScale = MathHelper.Clamp(ColorScale, 0.005f, 1f);
                    SizeScale = MathHelper.Clamp(SizeScale, 1f, 2f);

                    Vector2 ShadowScale = new Vector2(SizeScale * Scale, SizeScale * Scale);
                    Color ShadowColor = Color.Lerp(Color.Transparent, Color.Black, ColorScale*0.05f);

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
