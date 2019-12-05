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
        Texture2D TowerTexture;
        string AssetName;
        Vector2 Position;
        Rectangle DestinationRectangle;

        public Tower(string assetName, Vector2 position)
        {
            AssetName = assetName;
            Position = position;
        }

        public void LoadContent(ContentManager contentManager)
        {
            TowerTexture = contentManager.Load<Texture2D>(AssetName);
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, TowerTexture.Width, TowerTexture.Height);
        }

        public void Update(GameTime gameTime)
        {            

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TowerTexture, DestinationRectangle, Color.White);
        }
    }
}
