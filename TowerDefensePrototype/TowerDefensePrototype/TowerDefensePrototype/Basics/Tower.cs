using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class Tower
    {
        public Texture2D Texture;
        string AssetName;
        public Vector2 Position;
        public Rectangle DestinationRectangle;
        public int MaxHP, CurrentHP;

        public Tower(string assetName, Vector2 position, int totalHitpoints)
        {
            AssetName = assetName;
            Position = position;
            MaxHP = totalHitpoints;
            CurrentHP = MaxHP;
        }

        public void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(AssetName);
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
        }

        public void Update(GameTime gameTime)
        {            

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, Color.White);
        }
    }
}
