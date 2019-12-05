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
        public Texture2D Texture;
        public string TextureName;
        public List<Emitter> EmitterList;
        public Vector2 Velocity, Position, YRange, Scale, Origin, Direction;
        public float Angle, Speed, Gravity, CurrentRotation, CurrentTransparency, MaxY, DrawDepth;
        public bool Active, Rotate, Fade, CanBounce, BouncedOnGround, StopBounce, HardBounce, Shadow;
        public Color CurrentColor;
        public HeavyProjectileType HeavyProjectileType;
        public Rectangle DestinationRectangle, CollisionRectangle;
        public float Damage, BlastRadius;
        static Random Random = new Random();

        public void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(TextureName);

            foreach (Emitter emitter in EmitterList)
            {
                emitter.LoadContent(contentManager);
            }

            CurrentTransparency = 0;

            MaxY = Random.Next((int)YRange.X, (int)YRange.Y);
            Scale = new Vector2(1, 1);
            Origin = new Vector2(Texture.Width / 2, Texture.Height / 2);
            Shadow = true;
            DrawDepth = MaxY / 1080;
        }

        public virtual void Update(GameTime gameTime)
        {         
            if (Active == true)
            {
                Position += Velocity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                Velocity.Y += Gravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

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
                        Position.Y -= Velocity.Y * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                    Velocity.Y = (-Velocity.Y / 3) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                    Velocity.X = (Velocity.X / 3) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                    //RotationIncrement = RotationIncrement * 3;
                    BouncedOnGround = true;
                }

            if (StopBounce == true &&
                BouncedOnGround == true &&
                Position.Y > MaxY)
            {
                Velocity.Y = (-Velocity.Y / 2) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                Velocity.X *= 0.9f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

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
        }

        public void Draw(SpriteBatch spriteBatch)
        {           
            if (Active == true)
            {
                CurrentColor = Color.Lerp(Color.White, Color.Transparent, CurrentTransparency);
                DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width*(int)Scale.X, Texture.Height*(int)Scale.Y);
                CollisionRectangle = new Rectangle(DestinationRectangle.X, DestinationRectangle.Y, DestinationRectangle.Width / 2, DestinationRectangle.Height / 2);
                spriteBatch.Draw(Texture, DestinationRectangle, null, CurrentColor, CurrentRotation,
                    new Vector2(Origin.X, Origin.Y), SpriteEffects.None, DrawDepth);                

                if (Shadow == true)
                {
                    //This makes the scale change depending on distance to ground
                    float PercentToGround = (100 / MaxY) * (Position.Y - MaxY);
                    float ClampedGroundPercent = MathHelper.Clamp((100 - PercentToGround) / 100, 1f, 2f);
                    Vector2 ShadowScale = new Vector2(ClampedGroundPercent * Scale.X, ClampedGroundPercent * Scale.Y);

                    //This makes the scale change depending on perspective distance from foreground
                    float PercentToFore = (100 / (YRange.Y - YRange.X)) * (YRange.Y - MaxY);
                    float ClampedForePercent = MathHelper.Clamp((115 - PercentToFore) / 100, 0.3f, 0.7f);

                    //Multiply both values to get distance and perspective based shadow
                    ShadowScale = ShadowScale * ClampedForePercent;

                    Color ShadowColor = Color.Lerp(Color.Transparent, Color.Black, 0.02f);

                    spriteBatch.Draw(Texture,
                        new Rectangle((int)Position.X, (int)MaxY + 4, (int)(Texture.Width * ShadowScale.X), (int)(Texture.Height * ShadowScale.Y)),
                        null, ShadowColor, CurrentRotation, Origin, SpriteEffects.None, (DestinationRectangle.Bottom / 1080));

                    //spriteBatch.Draw(Texture,
                    //   new Rectangle((int)Position.X, (int)MaxY + 4, (int)(Texture.Width * ShadowScale.X / 1.7f), (int)(Texture.Height * ShadowScale.Y / 1.7f)),
                    //   null, ShadowColor, CurrentRotation, Origin, SpriteEffects.None, (DestinationRectangle.Bottom / 1080));

                    //spriteBatch.Draw(Texture,
                    //   new Rectangle((int)Position.X, (int)MaxY + 4, (int)(Texture.Width * ShadowScale.X / 1.5f), (int)(Texture.Height * ShadowScale.Y / 1.5f)),
                    //   null, ShadowColor, CurrentRotation, Origin, SpriteEffects.None, (DestinationRectangle.Bottom / 1080));

                    //spriteBatch.Draw(Texture,
                    //   new Rectangle((int)Position.X, (int)MaxY + 4, (int)(Texture.Width * ShadowScale.X / 1.3f), (int)(Texture.Height * ShadowScale.Y / 1.3f)),
                    //   null, ShadowColor, CurrentRotation, Origin, SpriteEffects.None, (DestinationRectangle.Bottom / 1080));

                    //spriteBatch.Draw(Texture,
                    //   new Rectangle((int)Position.X, (int)MaxY + 4, (int)(Texture.Width * ShadowScale.X), (int)(Texture.Height * ShadowScale.Y)),
                    //   null, ShadowColor, CurrentRotation, Origin, SpriteEffects.None, (DestinationRectangle.Bottom / 1080));
                }
            }

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Draw(spriteBatch);
            }
        }       
    }
}
