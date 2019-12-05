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
        public Vector2 Velocity, Position;
        public float Angle, Speed, Gravity;
        public bool Active;
        public HeavyProjectileType HeavyProjectileType;

        public void LoadContent(ContentManager contentManager)
        {            
            Texture = contentManager.Load<Texture2D>(TextureName);
            Emitter.LoadContent(contentManager);
        }

        public void Update(GameTime gameTime)
        {
            if (Active == true)
            {
                Speed = 0;
                Position += Velocity;
                Velocity.Y += Gravity;

                Emitter.Position = new Vector2(Position.X + Texture.Width / 2, Position.Y + Texture.Height / 2);      
            }

            Emitter.Update(gameTime);         
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Position.Y > 482)
            {
                Active = false;
                Emitter.HPRange = new Vector2(0, 0);
            }

            if (Active == true)
            {
                spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height), Color.White);
            }

            Emitter.Draw(spriteBatch);
        }
    }
}
