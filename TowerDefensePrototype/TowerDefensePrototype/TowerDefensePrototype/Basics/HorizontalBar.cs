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
    public class HorizontalBar
    {
        Texture2D Box;
        public int MaxHP, CurrentHP;
        int CurrentLength;
        public Vector2 Position, MaxSize;
        Color FrontColor, BackColor;

        public HorizontalBar(ContentManager contentManager, Vector2 maxSize, int maxHP, int currentHP, Color? frontColor = null, Color? backColor = null)
        {
            MaxHP = maxHP;
            CurrentHP = currentHP;

            MaxSize = maxSize;

            Box = contentManager.Load<Texture2D>("WhiteBlock");

            if (frontColor == null)
                FrontColor = Color.DarkRed;
            else
                FrontColor = frontColor.Value;

            if (backColor == null)
                BackColor = Color.Transparent;
            else
                BackColor = backColor.Value;
            
        }

        public void Update(Vector2 position, int currentHP)
        {
            Position = position;

            CurrentHP = (int)MathHelper.Clamp(currentHP, 0, MaxHP);

            CurrentLength = (int)((MaxSize.X / MaxHP) * CurrentHP);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Box, new Rectangle((int)Position.X, (int)Position.Y, (int)MaxSize.X, (int)MaxSize.Y), BackColor);
            spriteBatch.Draw(Box, new Rectangle((int)Position.X, (int)Position.Y, CurrentLength, (int)MaxSize.Y), FrontColor);            
        }
    }
}
