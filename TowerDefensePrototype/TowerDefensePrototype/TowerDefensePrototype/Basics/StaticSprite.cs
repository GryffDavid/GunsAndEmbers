using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class StaticSprite
    {
        Texture2D Texture;
        string AssetName;
        public Vector2 Position, Scale;
        Color Color;
        Rectangle DestinationRectangle;
        public BoundingBox BoundingBox;

        public StaticSprite(string assetName, Vector2 position, Vector2? scale = null, Color? color = null)
        {
            AssetName = assetName;
            Position = position;

            if (scale == null)
                Scale = new Vector2(1, 1);
            else
                Scale = scale.Value;

            if (color == null)
                Color = Color.White;
            else
                Color = color.Value;           
        }

        public void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(AssetName);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)(Texture.Width * Scale.X), (int)(Texture.Height * Scale.Y));
            BoundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, 0), new Vector3(Position.X + (Texture.Width * Scale.X), Position.Y + (Texture.Width * Scale.Y), 0));
            spriteBatch.Draw(Texture, DestinationRectangle, Color);
        }
    }
}
