using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class ExplosionEffect
    {
        public Texture2D Texture;
        Vector2 Position;
        float CurrentScale;
        Color Color;

        public ExplosionEffect(Vector2 position)
        {
            Position = position;
            CurrentScale = 0.1f;
        }

        public void Update(GameTime gameTime)
        {
            CurrentScale += 0.2f * (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 60f);
            Color = Color.Lerp(Color.White * 0.25f, Color.Transparent, CurrentScale);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentScale <= 1.0f)
                spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, (int)(Texture.Width * CurrentScale), (int)(Texture.Height * (CurrentScale * 0.66f))), null, Color, 0, new Vector2(Texture.Width / 2, Texture.Height / 2), SpriteEffects.None, 0);
        }
    }
}
