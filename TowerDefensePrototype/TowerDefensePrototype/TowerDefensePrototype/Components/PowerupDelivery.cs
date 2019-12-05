using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class PowerupDelivery : Drawable
    {
        //Lands from the sky. Dropped in. Has to be shot by the player to be picked up.
        //Provides a bonus of some sort.
        //This arrives shooting down from the sky like a pod which the player then shoots to open up and collect the Powerup

        public Texture2D Texture, TrailTexture, FinTexture;
        //public BoundingBox BoundingBox;
        public Vector2 Position, Velocity, Direction;
        public float Gravity, Rotation, Speed, FinRotation;
        public float CurrentHP, MaxHP;
        float OpeningDelay, CurrentDelay;
        public List<Emitter> EmitterList = new List<Emitter>();
        public UIBar HealthBar;
        public Color Color = Color.White;

        public class Fin
        {
            public Texture2D FinTexture;
            public Vector2 Position, Origin;
            public float Rotation;
        }

        Fin Fin1, Fin2;//, Fin3;

        public Powerup Powerup;

        //List<Fin> FinList = new List<Fin>();

        public PowerupDelivery(Texture2D shellTexture, Vector2 position, Texture2D trailTexture, Texture2D finTexture)
        {            
            MaxHP = 30;
            CurrentHP = MaxHP;
            Position = position;
            Texture = shellTexture;
            TrailTexture = trailTexture;
            FinTexture = finTexture;
            MaxY = 780;
            Gravity = 1f;
            Speed = 36f;
            Rotation = 95;
            Direction = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));
            Velocity = Direction * Speed;
            OpeningDelay = 1500;

            Emitter TrailEmitter = new Emitter(TrailTexture, new Vector2(Position.X + 16, Position.Y + 8), new Vector2(90, 180),
                new Vector2(0, 0), new Vector2(10, 20), 0.4f, true, new Vector2(0, 360), new Vector2(-0.5f, 0.5f),
                new Vector2(1f, 1f), Color.Lerp(Color.LightSkyBlue, Color.Transparent, 0.5f), Color.Lerp(Color.White, Color.Transparent, 0.5f), -0.00f, -1, 1, 1, false, new Vector2(0, 720), true, null,
                null, null, null, null, null, null, null, true, true, 0);
            EmitterList.Add(TrailEmitter);

            //Emitter EntryEmitter = new Emitter(TrailTexture, new Vector2(Position.X + 16, Position.Y + 8), new Vector2(90, 180),
            //    new Vector2(0, 0), new Vector2(10, 20), 0.4f, true, new Vector2(0, 360), new Vector2(-0.5f, 0.5f),
            //    new Vector2(1f, 1f), Color.Lerp(Color.Orange, Color.OrangeRed, 0.5f), Color.Lerp(Color.White, Color.Transparent, 0.5f), -0.00f, -1, 1, 1, false, new Vector2(0, 720), true, null,
            //    null, null, null, null, null, null, null, true, true, 0);
            //EmitterList.Add(EntryEmitter);

            Fin1 = new Fin()
            {
                Rotation = Rotation,
                FinTexture = finTexture,
                Position = Position,
                Origin = new Vector2(FinTexture.Width, FinTexture.Height)
            };

            Fin2 = new Fin()
            {
                Rotation = Rotation,
                FinTexture = finTexture,
                Position = Position,
                Origin = new Vector2(FinTexture.Width, 0)
            };

            //Fin3 = new Fin()
            //{
            //    Rotation = Rotation,
            //    FinTexture = finTexture,
            //    Position = Position,
            //    Origin = new Vector2(FinTexture.Width, FinTexture.Height/2)
            //};
        }

        public void LoadContent(ContentManager contentManager)
        {

        }

        public void Update(GameTime gameTime)
        {
            CurrentDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            Position += Velocity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

            BoundingBox = new BoundingBox() 
            {
                Min = new Vector3(Position.X - ((float)Math.Cos(Rotation) * (Texture.Height + Texture.Height/2)),
                                  Position.Y - ((float)Math.Sin(Rotation) * (Texture.Width - FinTexture.Width)), 0), 
                Max = new Vector3(Position.X, Position.Y, 0) 
            };

            if (HealthBar != null)
                HealthBar.Update(MaxHP, CurrentHP, gameTime);            

            if (Position.Y < MaxY)
            {
                Velocity.Y += Gravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                Rotation = (float)Math.Atan2(Velocity.Y, Velocity.X);
                FinRotation = Rotation;

                Fin1.Rotation = Rotation;
                Fin2.Rotation = Rotation;
                //Fin3.Rotation = Rotation;
            }

            Vector2 finPosition = new Vector2(Position.X - ((float)Math.Cos(Rotation) * (Texture.Width - FinTexture.Width - 1)),
                                              Position.Y - ((float)Math.Sin(Rotation) * (Texture.Width - FinTexture.Width - 1)));

            Fin1.Position = finPosition;
            Fin2.Position = finPosition;

            if (Position.Y >= MaxY)
            {
                if (HealthBar == null)
                {
                    HealthBar = new UIBar(new Vector2(Position.X - 16, Position.Y + 4), new Vector2(32, 4), Color.Red);
                }

                //Position.Y = MaxY;
                Velocity = Vector2.Zero;                     

                //if (CurrentDelay < OpeningDelay)
                //    Fin3.Position = finPosition;

                foreach (Emitter emitter in EmitterList)
                {
                    emitter.AddMore = false;
                }

                if (CurrentDelay >= OpeningDelay)
                {
                    float fin1Rot = MathHelper.ToDegrees(Fin1.Rotation);
                    float fin2Rot = MathHelper.ToDegrees(Fin2.Rotation);

                    if (fin1Rot < (MathHelper.ToDegrees(Rotation) + 35))
                        Fin1.Rotation += 0.002f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                    if (fin2Rot > (MathHelper.ToDegrees(Rotation) - 35))
                        Fin2.Rotation -= 0.002f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                    //if (fin2Rot > (MathHelper.ToDegrees(Rotation) - 35))
                    //{
                    //    Fin3.Position.X = Fin3.Position.X + ((float)Math.Cos(Rotation) * (0.03f* ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f)));
                    //    Fin3.Position.Y = Fin3.Position.Y + ((float)Math.Sin(Rotation) * (0.03f* ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f)));
                    //}
                }
            }

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Update(gameTime);
                emitter.Position = Position;
            }

            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height),
                             null, Color.White, Rotation, new Vector2(Texture.Width, Texture.Height / 2), SpriteEffects.None, (MaxY - 5) / 1080);

            //Rectangle boundingRect = new Rectangle(
            //                    (int)(BoundingBox.Min.X), (int)(BoundingBox.Min.Y),
            //                    (int)(BoundingBox.Max.X - BoundingBox.Min.X),
            //                    (int)(BoundingBox.Max.Y - BoundingBox.Min.Y));

            //spriteBatch.Draw(Texture, boundingRect, new Rectangle((int)(48), (int)(18), 2,2), 
            //                 Color.Black, 0, new Vector2(0, 0), SpriteEffects.None, (MaxY - 5) / 1080);


            spriteBatch.Draw(Fin1.FinTexture,
                             new Rectangle(
                                 (int)Fin1.Position.X, (int)Fin1.Position.Y,
                                 (int)Fin1.FinTexture.Width, (int)Fin1.FinTexture.Height),
                             null, Color, Fin1.Rotation, Fin1.Origin, SpriteEffects.None, (MaxY - 5) / 1080);

            spriteBatch.Draw(Fin2.FinTexture,
                             new Rectangle(
                                 (int)Fin2.Position.X, (int)Fin2.Position.Y,
                                 (int)Fin2.FinTexture.Width, (int)Fin2.FinTexture.Height),
                             null, Color, Fin2.Rotation, Fin2.Origin, SpriteEffects.FlipVertically, (MaxY - 5) / 1080);

            //spriteBatch.Draw(Fin3.FinTexture,
            //                 new Rectangle(
            //                     (int)Fin3.Position.X, (int)Fin3.Position.Y,
            //                     (int)Fin3.FinTexture.Width, (int)Fin3.FinTexture.Height/2),
            //                 null, Color.White, Fin3.Rotation, Fin3.Origin, SpriteEffects.FlipVertically, 1);

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Draw(spriteBatch);
            }
        }

        public void DrawBars(GraphicsDevice graphicsDevice, BasicEffect basicEffect)
        {
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                HealthBar.Draw(graphicsDevice);
            }
        }
    }
}
