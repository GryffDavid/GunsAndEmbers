using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class ButtonMarker
    {
        public Texture2D Texture;
        Rectangle DestinationRectangle;
        float Transparency = 1.0f;        
        Color Color = Color.White;
        public Vector2 Position, Origin, StartSize, MaxSize, CurrentSize;
        public bool Active = false;

        public ButtonMarker(Vector2 position, Texture2D texture)
        {
            Active = false;
            Texture = texture;
            Position = position;
            StartSize = new Vector2(1, 1);
            MaxSize = StartSize * 3f;
            CurrentSize = StartSize;
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y,
                (int)(Texture.Width * CurrentSize.X),
                (int)(Texture.Height * CurrentSize.Y));

        }

        public void LoadContent(ContentManager content)
        {

        }

        public void Update(GameTime gameTime)
        {
            if (Active == true)
            {
                CurrentSize += new Vector2(0.05f, 0.05f);

                if (CurrentSize.X >= MaxSize.X &&
                    CurrentSize.Y >= MaxSize.Y)
                {
                    CurrentSize = StartSize;
                }

                Transparency = 1.0f - ((StartSize.X / MaxSize.X) * CurrentSize.X);
            }

            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y,
                    (int)(Texture.Width * CurrentSize.X),
                    (int)(Texture.Height * CurrentSize.Y));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                spriteBatch.Draw(Texture, DestinationRectangle, null, Color * Transparency, 0, new Vector2(Texture.Width / 2, Texture.Height / 2), SpriteEffects.None, 0);
            }
        }
    }
}
