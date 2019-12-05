using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace TowerDefensePrototype
{
    class HorizontalBar
    {
        Texture2D Box;
        public int MaxHealth, CurrentHealth;
        int CurrentLength;
        Vector2 Position, MaxSize;

        public HorizontalBar(ContentManager contentManager, Vector2 maxSize, int maxHealth, int currentHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = currentHealth;

            MaxSize = maxSize;

            Box = contentManager.Load<Texture2D>("WhiteBlock");
        }

        public void Update(Vector2 position, int currentHealth)
        {
            Position = position;

            CurrentHealth = (int)MathHelper.Clamp(currentHealth, 0, MaxHealth);

            CurrentLength = (int)((MaxSize.X / MaxHealth) * CurrentHealth);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Box, new Rectangle((int)Position.X, (int)Position.Y, CurrentLength, (int)MaxSize.Y), Color.DarkRed);
        }
    }
}
