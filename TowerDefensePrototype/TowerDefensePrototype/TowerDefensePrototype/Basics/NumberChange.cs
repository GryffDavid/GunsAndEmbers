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
            Color = Color.DarkRed;
        }

        public void Update(GameTime gameTime)
        {
            Position += Change;

            CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentTime > 1000)
            {                
                CurrentTime = 0;
                Active = false;
            }

            Color = Color.Lerp(Color, Color.Transparent, 0.07f);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(SpriteFont, Number.ToString(), Position, Color);
        }
    }
}
