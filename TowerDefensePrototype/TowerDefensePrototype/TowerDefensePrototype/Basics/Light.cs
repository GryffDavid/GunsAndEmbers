using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class Light
    {
        Texture2D LightTexture;
        Rectangle DestinationRectangle;
        Vector2 Position, Size;
        Color Color;
        float Opacity;
        byte Alpha;

        public Light(Vector2 position, float opacity, byte alpha, Color color, Vector2? size = null)
        {
            Position = position;
            Opacity = opacity;
            Alpha = alpha;
            Color = color;
            Size = size.Value;

            if (size != null)
                DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            else
                DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)LightTexture.Width, (int)LightTexture.Height);
        }

        public void LoadConent(ContentManager contentManager)
        {
            LightTexture = contentManager.Load<Texture2D>("Light");
        }

        public void Update(GameTime gameTime)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //Color.A = Alpha;
            //Color = Color.Lerp(Color, Color.Transparent, Opacity);
            spriteBatch.Draw(LightTexture, DestinationRectangle, Color);
        }
    }
}
