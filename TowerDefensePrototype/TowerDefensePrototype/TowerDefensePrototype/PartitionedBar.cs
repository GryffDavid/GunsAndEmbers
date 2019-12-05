using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class PartitionedBar
    {
        Texture2D Texture;
        int CurrentValue, MaxValue;
        Vector2 MaxSize, UnitSize;

        public PartitionedBar()
        {
            //5 pixel gap between units/segments
            UnitSize = new Vector2(MaxSize.X / MaxValue - (MaxValue * 5), MaxSize.Y);
        }

        public void Update(GameTime gameTime)
        {

        }

        public void LoadContent(ContentManager contentManager)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
