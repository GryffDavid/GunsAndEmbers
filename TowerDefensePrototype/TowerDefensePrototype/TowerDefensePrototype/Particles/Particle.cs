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
        public Vector2 CurrentPosition, Direction, Velocity;
        public Rectangle DestinationRectangle;
        public float Angle, Speed, CurrentLife, MaxLife, CurrentTransparency, Scale;
        public float RotationIncrement, CurrentRotation, Gravity;
        public Color CurrentColor, EndColor;
        public bool Active, Fade, BouncedOnGround, CanBounce;
        

        public Particle(Texture2D texture, Vector2 position, float angle, float speed, float maxLife,
            float startingTransparency, bool fade, float startingRotation, float rotationChange,
            float scale, Color startColor, Color endColor, float gravity, bool canBounce)
        {
            Active = true;
            Texture = texture;
            CurrentPosition = position;
            Angle = angle;
            Speed = speed;
            MaxLife = maxLife;
            CurrentLife = maxLife;
            CurrentTransparency = startingTransparency;
            CurrentColor = startColor;
            EndColor = endColor;
            Scale = scale;
            Fade = fade;
            Gravity = gravity;
            CanBounce = canBounce; 

            CurrentRotation = MathHelper.ToRadians(startingRotation);
            RotationIncrement = MathHelper.ToRadians(rotationChange);

            Direction.X = (float)Math.Sin(MathHelper.ToRadians(Angle));
            Direction.Y = (float)Math.Cos(MathHelper.ToRadians(Angle));

            Direction.X = (float)Math.Cos(Angle);
            Direction.Y = (float)Math.Sin(Angle);

            Velocity = Direction * Speed;
        }

        public void Update()
        {
            CurrentLife--;            

            if (CurrentLife <= 0)
                Active = false;

            else
                Active = true;

            if (CanBounce == true)
                if (CurrentPosition.Y >= 493 && BouncedOnGround == false)
                {
                    Velocity.Y = -Velocity.Y / 2;
                    Velocity.X = Velocity.X / 2;
                    RotationIncrement = RotationIncrement * 2;
                    BouncedOnGround = true;
                }        

            if (Active == true)
            {
                CurrentRotation += RotationIncrement;
                CurrentPosition += Velocity;
                Velocity.Y += Gravity;
                DestinationRectangle = new Rectangle((int)CurrentPosition.X, (int)CurrentPosition.Y, (int)(Texture.Width * Scale), (int)(Texture.Height * Scale));
            }                      

            float LifePercentage = CurrentLife / MaxLife;

            if (Fade == true)
            {
                //CurrentTransparency *= (LifePercentage*1.4f);
                //CurrentTransparency = LifePercentage / (CurrentLife*2);
                CurrentTransparency = MathHelper.Lerp(LifePercentage, CurrentTransparency, LifePercentage);
            }

            CurrentColor = Color.Lerp(CurrentColor, EndColor, LifePercentage / (CurrentLife * 0.5f));


        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {                
                spriteBatch.Draw(Texture, DestinationRectangle, null, Color.Lerp(Color.Transparent, CurrentColor, CurrentTransparency), CurrentRotation, new Vector2(Texture.Width / 2, Texture.Height / 2), SpriteEffects.None, 0);
            }
        }
    }
}
