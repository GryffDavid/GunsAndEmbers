using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    abstract class Particle
    {
        public bool Active;
        public string AssetName;
        public Texture2D Texture;
        public Vector2 Velocity, Position;
        public Rectangle DestinationRectangle;

        public void LoadContent(ContentManager contentManager)
        {
            Active = true;
            Texture = contentManager.Load<Texture2D>(AssetName);
        }

        public void Update()
        {
            if (Active == true)
            {
                Position += Velocity;
                Velocity -= Vector2.One;
                DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                spriteBatch.Draw(Texture, DestinationRectangle, Color.White);
            }        
        }
    }
}
