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
        public Vector2 CurrentPosition, Direction, Velocity, YRange;
        public Rectangle DestinationRectangle;
        public float Angle, Speed, CurrentHP, MaxHP, CurrentTransparency, Scale, MaxY;
        public float RotationIncrement, CurrentRotation, Gravity, DrawDepth;
        public Color CurrentColor, EndColor;
        public bool Active, Fade, BouncedOnGround, CanBounce, Shrink;

        public Particle(Texture2D texture, Vector2 position, float angle, float speed, float maxHP,
            float startingTransparency, bool fade, float startingRotation, float rotationChange,
            float scale, Color startColor, Color endColor, float gravity, bool canBounce, float maxY, bool shrink, float? drawDepth = null)
        {
            Active = true;
            Texture = texture;
            CurrentPosition = position;
            Angle = angle;
            Speed = speed;
            MaxHP = maxHP;
            CurrentHP = maxHP;
            CurrentTransparency = startingTransparency;
            CurrentColor = startColor;
            EndColor = endColor;
            Scale = scale;
            Fade = fade;
            Gravity = gravity;
            CanBounce = canBounce;
            Shrink = shrink;

            CurrentRotation = MathHelper.ToRadians(startingRotation);
            RotationIncrement = MathHelper.ToRadians(rotationChange);

            Direction.X = (float)Math.Sin(MathHelper.ToRadians(Angle));
            Direction.Y = (float)Math.Cos(MathHelper.ToRadians(Angle));

            Direction.X = (float)Math.Cos(Angle);
            Direction.Y = (float)Math.Sin(Angle);

            Velocity = Direction * Speed;

            MaxY = maxY;

            if (drawDepth == null)
                DrawDepth = 0;
            else
                DrawDepth = drawDepth.Value;
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
                    Velocity.Y = -Velocity.Y / 3;
                    Velocity.X = Velocity.X / 3;
                    RotationIncrement = RotationIncrement * 3;
                    BouncedOnGround = true;
                }        

            if (Active == true)
            {
                CurrentRotation += RotationIncrement;
                CurrentPosition += Velocity;
                Velocity.Y += Gravity;
                DestinationRectangle = new Rectangle((int)CurrentPosition.X, (int)CurrentPosition.Y, (int)(Texture.Width * Scale), (int)(Texture.Height * Scale));
            }

            float PercentageHP = CurrentHP / MaxHP;

            if (Fade == true)
            {
                CurrentTransparency = MathHelper.Lerp(PercentageHP, CurrentTransparency, PercentageHP);
            }

            if (Shrink == true)
            {
                Scale = MathHelper.Lerp(Scale, (Scale*(PercentageHP)), PercentageHP);
            }

            CurrentColor = Color.Lerp(CurrentColor, EndColor, PercentageHP / (CurrentHP * 0.5f));
        }

        public void Draw(SpriteBatch spriteBatch)
        { 
            if (Active == true)
            {         
                spriteBatch.Draw(Texture, DestinationRectangle, null, Color.Lerp(Color.Transparent, CurrentColor, CurrentTransparency),
                    CurrentRotation, new Vector2(Texture.Width / 2, Texture.Height / 2), SpriteEffects.None, DrawDepth);
            }
        }
    }
}
