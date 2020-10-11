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
        static Random Random = new Random();
        float CurrentTime, MaxTime, Angle;

        public NumberChange(SpriteFont spriteFont, Vector2 position, Vector2 change, int number, Color? color = null)
        {
            Active = true;
            SpriteFont = spriteFont;
            Position = position;
            Change = change;
            Number = number;
            MaxTime = 500;
            Angle = (float)Game1.RandomDouble(-120, -60);

            if (color == null)
            {
                if (number < 0)
                {
                    OriginalColor = new Color(255, 0, 0, 255);
                }
                else
                {
                    OriginalColor = new Color(255, 255, 255, 255);
                }
            }
            else
            {
                OriginalColor = color.Value;
            }

            Change = 5 * new Vector2((float)Math.Cos(MathHelper.ToRadians(Angle)), (float)Math.Sin(MathHelper.ToRadians(Angle)));

        }

        public void Update(GameTime gameTime)
        {
            Change.Y += 0.2f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60);

            Position += Change *(float)(gameTime.ElapsedGameTime.TotalSeconds * 60);

            CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentTime > MaxTime)
            {                
                CurrentTime = 0;
                Active = false;
            }

            double colorPercent = (100.0 / MaxTime * CurrentTime) / 100.0;
            Color = OriginalColor;// Color.Lerp(OriginalColor, Color.Transparent, (float)colorPercent);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(SpriteFont, Number.ToString(), Position, Color, 0, Vector2.Zero, 1.1f, SpriteEffects.None, 0);
            spriteBatch.DrawString(SpriteFont, Number.ToString(), Position, Color.White, 0, Vector2.Zero - Vector2.One, 1.0f, SpriteEffects.None, 0);
        }
    }
}
