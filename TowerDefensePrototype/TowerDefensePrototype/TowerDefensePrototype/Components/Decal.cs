using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Decal
    {
        public Texture2D Texture;
        public Vector2 Position, Scale;
        public float Rotation;
        public float TransparencyPercentage, CurrentTime, MaxTime;
        float CurrentFadeDelay, FadeDelay;
        
        public Decal(Texture2D texture, Vector2 position, float rotation, float imageScale)
        {
            Position = position;
            Texture = texture;
            Rotation = rotation;
            TransparencyPercentage = 100;
            MaxTime = 100;
            FadeDelay = 4000;

            //float PercentY = (100 / (maxV.Y - maxV.X)) * (maxV.Y - maxY);
            //float thing = (100 - PercentY) / 100;

            //thing = MathHelper.Clamp(thing, imageScale * 0.45f, imageScale);

            //Scale = new Vector2(thing, thing * (0.5f * thing));      
            Scale = new Vector2(1 * imageScale, 0.3f * imageScale);
        }

        public void Update(GameTime gameTime)
        {
            if (CurrentFadeDelay < FadeDelay)
            {
                CurrentFadeDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            if (CurrentFadeDelay >= FadeDelay)
            {
                CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            if (CurrentTime >= MaxTime)
            {
                TransparencyPercentage--;
                CurrentTime = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, (int)(Texture.Width*Scale.X), 
                            (int)(Texture.Height*Scale.Y)), null, Color.White * (TransparencyPercentage/100), Rotation, new Vector2(Texture.Width / 2, Texture.Height / 2), 
                            SpriteEffects.None, 0);
        }

        public void DrawNormal(SpriteBatch spriteBatch)
        {

        }
    }
}
