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
        public Emitter Emitter;
        public Vector2 Velocity, Position, YRange;
        public float Angle, Speed, Gravity, Rotation, CurrentTransparency, MaxY;
        public bool Active, Rotate, Fade;
        public Color CurrentColor;
        public HeavyProjectileType HeavyProjectileType;
        public Rectangle DestinationRectangle, CollisionRectangle;
        public int Damage;
        public Random Random;

        public void LoadContent(ContentManager contentManager)
        {            
            Texture = contentManager.Load<Texture2D>(TextureName);

            if (Emitter != null)
            Emitter.LoadContent(contentManager);

            CurrentTransparency = 0;

            Random = new Random();

            MaxY = Random.Next((int)YRange.X, (int)YRange.Y);
        }

        public void Update(GameTime gameTime)
        {
            if (Active == true)
            {
                Position += Velocity;
                Velocity.Y += Gravity;

                if (Emitter != null)
                Emitter.Position = new Vector2(Position.X, Position.Y);                                
            }

            if (Rotate == true)
                Rotation = (float)Math.Atan2(Velocity.Y, Velocity.X);

            if (Emitter != null)
            Emitter.Update(gameTime);

            if (Fade == true)
            {
                CurrentTransparency += 0.1f;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {           
            if (Active == true)
            {
                CurrentColor = Color.Lerp(Color.White, Color.Transparent, CurrentTransparency);
                DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
                CollisionRectangle = new Rectangle(DestinationRectangle.X, DestinationRectangle.Y, DestinationRectangle.Width / 2, DestinationRectangle.Height / 2);
                spriteBatch.Draw(Texture, DestinationRectangle, null, CurrentColor, Rotation, 
                    new Vector2(Texture.Width/2, Texture.Height/2), SpriteEffects.None, MaxY/720);
            }

            if (Emitter != null)
            Emitter.Draw(spriteBatch);
        }
    }
}
