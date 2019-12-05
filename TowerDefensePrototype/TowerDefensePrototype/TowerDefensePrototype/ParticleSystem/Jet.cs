using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class Jet
    {
        Texture2D JetTexture;
        Invader Anchor;
        float Rotation = -90;
        Vector2 Position;
        Vector2 Size;
        float CurrentTime, MaxTime;
        float MaxLength, MinLength;

        public Jet(Texture2D jetTexture, Invader anchor)
        {
            JetTexture = jetTexture;
            Anchor = anchor;
            Size = new Vector2(jetTexture.Width / 2, jetTexture.Height / 2);
        }

        public void Update(GameTime gameTime)
        {
            Position = Anchor.Center;

            CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentTime > 30)
            {
                Size.Y = JetTexture.Height/2 * (float)Math.Sin(Math.PI*10 * CurrentTime);
                CurrentTime = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(JetTexture, new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y), 
                             null, Color.Turquoise, MathHelper.ToRadians(Rotation), new Vector2(JetTexture.Width / 2, 0),
                             SpriteEffects.None, 0);
        }
    }
}
