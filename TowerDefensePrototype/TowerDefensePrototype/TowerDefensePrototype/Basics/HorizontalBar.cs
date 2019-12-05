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
        public float MaxHP, CurrentHP;
        public Vector2 Position, MaxSize;
        Texture2D Box;
        Color FrontColor, BackColor;        
        int CurrentLength;      
        
        public HorizontalBar(ContentManager contentManager, Vector2 maxSize, float maxHP, float currentHP, Color? frontColor = null, Color? backColor = null)
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

        public void Update(Vector2 position, float currentHP)
        {
            Position = position;

            CurrentHP = (int)MathHelper.Clamp(currentHP, 0, MaxHP);

            CurrentLength = (int)((MaxSize.X / MaxHP) * CurrentHP);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Box, new Rectangle((int)Position.X, (int)Position.Y, (int)MaxSize.X, (int)MaxSize.Y), null, BackColor, 0, Vector2.Zero, SpriteEffects.None, 0.99f);
            spriteBatch.Draw(Box, new Rectangle((int)Position.X, (int)Position.Y, CurrentLength, (int)MaxSize.Y), null, FrontColor, 0, Vector2.Zero, SpriteEffects.None, 1);           
        }
    }
}
