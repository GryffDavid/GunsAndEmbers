using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class WaveCountDown
    {
        public Vector2 Origin;
        public float CurrentTime, Scale;
        public int CurrentSeconds;
        public SpriteFont Font;
        public Color Color;

        public WaveCountDown(int seconds)
        {
            CurrentSeconds = seconds;
            Scale = 0.5f;
            Color = Color.Red;
        }

        public void LoadContent(ContentManager contentManager)
        {
            Font = contentManager.Load<SpriteFont>("Fonts/WaveCountDownFont");
        }

        public void Update(GameTime gameTime)
        {
            CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            Scale += (0.005f);

            Scale = MathHelper.Clamp(Scale, 0.5f, 1f);


            if (CurrentTime >= 1000 && CurrentSeconds > 0)
            {
                Scale = 0.5f;
                CurrentTime = 0;
                CurrentSeconds -= 1;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Origin = new Vector2(Font.MeasureString(CurrentSeconds.ToString()).X/2,Font.MeasureString(CurrentSeconds.ToString()).Y/2);

            Color = Color.Lerp(Color.Red, Color.Transparent, CurrentTime / 1000);
            spriteBatch.DrawString(Font, CurrentSeconds.ToString(), new Vector2(1920 / 2, 1080 / 2), Color, 0, Origin, Scale, SpriteEffects.None, 0);
        }
    }
}
