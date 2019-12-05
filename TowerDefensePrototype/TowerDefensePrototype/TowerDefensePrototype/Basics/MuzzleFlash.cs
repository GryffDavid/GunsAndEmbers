using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class MuzzleFlash
    {
        public bool Visible;
        Vector2 Position;
        float Rotation;
        double Alpha, FadeIncrement, FadeTime;
        Texture2D CurrentTexture, Texture1, Texture2, Texture3;
        int TextureNumber, BarrelLength;

        public MuzzleFlash(ContentManager contentManager, int barrelLength)
        {
            Texture1 = contentManager.Load<Texture2D>("Flash1");
            Texture2 = contentManager.Load<Texture2D>("Flash2");
            Texture3 = contentManager.Load<Texture2D>("Flash3");
            Visible = true;
            FadeIncrement = 0.3;
            FadeTime = 0.02;
            BarrelLength = barrelLength -32;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime, Vector2 position, float rotation)
        {
            Position = position;
            Rotation = rotation;


            if (Alpha <= 0)
            {
                Visible = false;
            }

            if (Visible == true)
            {
                switch (TextureNumber)
                {
                    case 0:
                        CurrentTexture = Texture1;
                        break;

                    case 1:
                        CurrentTexture = Texture2;
                        break;

                    case 2:
                        CurrentTexture = Texture3;
                        break;
                }

                FadeTime -= gameTime.ElapsedGameTime.TotalSeconds;

                if (FadeTime <= 0)
                {
                    Alpha -= FadeIncrement;
                    FadeTime = 0.02;
                }

                spriteBatch.Draw(CurrentTexture, new Rectangle((int)Position.X, (int)Position.Y, (int)(CurrentTexture.Width), (int)(CurrentTexture.Height)), null, Color.Lerp(Color.Transparent, Color.White, (float)Alpha), Rotation, new Vector2(-BarrelLength,CurrentTexture.Height/2), SpriteEffects.None, 1);
            }
        }

        public void Flash(int rangeStart, int rangeEnd)
        {
            if (Visible == false)
            {
                Random r = new Random();
                TextureNumber = r.Next(rangeStart, rangeEnd);
                Alpha = 1;
                Visible = true;
            }
        }
    }
}
