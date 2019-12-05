using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class PartitionedBar
    {
        public Texture2D Texture;
        public int CurrentValue, MaxValue, Separation;
        Vector2 MaxSize, UnitSize, Position;

        public PartitionedBar(int separation, int maxValue, Vector2 maxSize, Vector2 position)
        {
            //5 pixel gap between units/segments
            Position = position;
            Separation = separation;
            CurrentValue = maxValue;
            MaxValue = maxValue;
            MaxSize = maxSize;

            float diff = MaxValue * Separation;
            UnitSize = new Vector2((MaxSize.X - diff) / MaxValue, MaxSize.Y);
        }

        public void Update(int currentValue, int? maxValue = null)
        {
            CurrentValue = currentValue;

            if (maxValue != null)
            {
                MaxValue = maxValue.Value;
            }
        }

        public void LoadContent(ContentManager contentManager)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //float thing = (Position.X + MaxSize.X) - (Position.X + ((Separation + UnitSize.X) * (MaxValue - 1)) + UnitSize.X);

            for (int p = 0; p < MaxValue; p++)
            {
                spriteBatch.Draw(Texture, new Rectangle((int)Position.X + ((Separation + (int)UnitSize.X) * p), (int)Position.Y, (int)UnitSize.X, (int)UnitSize.Y), Color.Gray);             
            }

            for (int p = 0; p < CurrentValue; p++)
            {
                spriteBatch.Draw(Texture, new Rectangle((int)Position.X + ((Separation + (int)UnitSize.X) * p), (int)Position.Y, (int)UnitSize.X, (int)UnitSize.Y), Color.White);
            }
        }
    }
}
