using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public abstract class HeavyProjectile
    {
        public Texture2D Texture, Shadow;
        public string TextureName;
        public List<Emitter> EmitterList;
        public Vector2 Velocity, Position, YRange;
        public float Angle, Speed, Gravity, CurrentRotation, CurrentTransparency, MaxY;
        public bool Active, Rotate, Fade, CanBounce, BouncedOnGround, StopBounce, HardBounce;
        public Color CurrentColor;
        public HeavyProjectileType HeavyProjectileType;
        public Rectangle DestinationRectangle, CollisionRectangle;
        public float Damage, BlastRadius;
        static Random Random = new Random();

        public void LoadContent(ContentManager contentManager)
        {
            Shadow = contentManager.Load<Texture2D>("Shadow");
            Texture = contentManager.Load<Texture2D>(TextureName);

            foreach (Emitter emitter in EmitterList)
            {
                emitter.LoadContent(contentManager);
            }            

            CurrentTransparency = 0;

            MaxY = Random.Next((int)YRange.X, (int)YRange.Y);
        }

        public virtual void Update(GameTime gameTime)
        {         
            if (Active == true)
            {
                Position += Velocity;
                Velocity.Y += Gravity;

                foreach (Emitter emitter in EmitterList)
                {
                    emitter.Position = Position;
                }
            }

            if (Rotate == true)
                CurrentRotation = (float)Math.Atan2(Velocity.Y, Velocity.X);

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Update(gameTime);
            }

            if (Fade == true)
            {
                CurrentTransparency += 0.1f;
            }

            if (CanBounce == true)
                if (Position.Y >= MaxY && BouncedOnGround == false)
                {
                    if (HardBounce == true)
                        Position.Y -= Velocity.Y;

                    Velocity.Y = -Velocity.Y / 3;
                    Velocity.X = Velocity.X / 3;
                    //RotationIncrement = RotationIncrement * 3;
                    BouncedOnGround = true;
                }

            if (StopBounce == true &&
                BouncedOnGround == true &&
                Position.Y > MaxY)
            {
                Velocity.Y = -Velocity.Y / 2;

                Velocity.X *= 0.9f;

                //RotationIncrement = MathHelper.Lerp(RotationIncrement, 0, 0.2f);

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
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {           
            if (Active == true)
            {
                spriteBatch.Draw(Shadow, new Rectangle((int)Position.X, (int)MaxY, Texture.Width, Texture.Height / 3), Color.Lerp(Color.White, Color.Transparent, 0.75f));

                CurrentColor = Color.Lerp(Color.White, Color.Transparent, CurrentTransparency);
                DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
                CollisionRectangle = new Rectangle(DestinationRectangle.X, DestinationRectangle.Y, DestinationRectangle.Width / 2, DestinationRectangle.Height / 2);
                spriteBatch.Draw(Texture, DestinationRectangle, null, CurrentColor, CurrentRotation,
                    new Vector2(Texture.Width / 2, Texture.Height / 2), SpriteEffects.None, MaxY / 1080);

                foreach (Emitter emitter in EmitterList)
                {
                    emitter.Draw(spriteBatch);
                }
            }
        }
    }
}
