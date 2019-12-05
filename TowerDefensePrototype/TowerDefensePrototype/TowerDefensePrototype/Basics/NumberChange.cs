using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class NumberChange
    {
        SpriteFont SpriteFont;
        int Number;
        Vector2 Position, Change;
        public bool Active;
        Color Color;

        float CurrentTime;

        public NumberChange(SpriteFont spriteFont, Vector2 position, Vector2 change, int number)
        {
            Active = true;
            SpriteFont = spriteFont;
            Position = position;
            Change = change;
            Number = number;

            if (number < 0)
            {
                Color = new Color(255, 0, 0, 255);
            }
            else
            {
                Color = new Color(255, 255, 255, 255);
            }
        }

        public void Update(GameTime gameTime)
        {
            Position += Change * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60);

            CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentTime > 2000)
            {                
                CurrentTime = 0;
                Active = false;
            }

            Color = Color.Lerp(Color, Color.Transparent, 0.05f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(SpriteFont, Number.ToString(), Position, Color);
        }
    }
}
