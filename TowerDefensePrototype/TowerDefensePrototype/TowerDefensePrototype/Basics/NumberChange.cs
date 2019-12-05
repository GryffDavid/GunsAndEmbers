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
        Color OriginalColor, Color;

        float CurrentTime, MaxTime;

        public NumberChange(SpriteFont spriteFont, Vector2 position, Vector2 change, int number)
        {
            Active = true;
            SpriteFont = spriteFont;
            Position = position;
            Change = change;
            Number = number;
            MaxTime = 500;

            if (number < 0)
            {
                OriginalColor = new Color(255, 0, 0, 255);
            }
            else
            {
                OriginalColor = new Color(255, 255, 255, 255);
            }
        }

        public void Update(GameTime gameTime)
        {
            Position += Change * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60);

            CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentTime > MaxTime)
            {                
                CurrentTime = 0;
                Active = false;
            }
            double colorPercent = (100.0 / MaxTime * CurrentTime) / 100.0;
            Color = Color.Lerp(OriginalColor, Color.Transparent, (float)colorPercent);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(SpriteFont, Number.ToString(), Position, Color);
        }
    }
}
