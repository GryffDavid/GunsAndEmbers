using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class FrameRateCounter
    {
        public SpriteFont spriteFont;

        int frameRate = 0;
        int MaxFrameRate = 0;
        int MinFrameRate = 10000;
        int previousFrameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;
        List<int> FPSList = new List<int>();

        public void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(0.25f))
            {
                elapsedTime -= TimeSpan.FromSeconds(1f);
                frameRate = frameCounter;
                frameCounter = 0;
                FPSList.Add(frameRate);
            }

            if (frameRate > MaxFrameRate)
            {
                MaxFrameRate = frameRate;
            }

            if (frameRate < MinFrameRate && frameRate > 0)
            {
                MinFrameRate = frameRate;
            }

            previousFrameRate = frameRate;

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            frameCounter++;

            string fps = string.Format("fps: {0}", frameRate);

            spriteBatch.DrawString(spriteFont, fps, new Vector2(11, 0), Color.Black);
            spriteBatch.DrawString(spriteFont, fps, new Vector2(10, 0), Color.White);

            string Maxfps = string.Format("Max fps: {0}", MaxFrameRate);

            spriteBatch.DrawString(spriteFont, Maxfps, new Vector2(128, 0), Color.Black);
            spriteBatch.DrawString(spriteFont, Maxfps, new Vector2(127, 0), Color.White);

            string Minfps = string.Format("Min fps: {0}", MinFrameRate);

            spriteBatch.DrawString(spriteFont, Minfps, new Vector2(320, 0), Color.Black);
            spriteBatch.DrawString(spriteFont, Minfps, new Vector2(320, 0), Color.White);
        }
    }
}
