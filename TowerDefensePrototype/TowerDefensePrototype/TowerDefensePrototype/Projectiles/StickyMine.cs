using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class StickyMine : Drawable
    {
        Texture2D Texture;
        Vector2 Position;
        Rectangle DestinationRectangle;

        public StickyMine()
        {

        }

        public void Update(GameTime gameTime)
        {

        }

        public override void Draw(GraphicsDevice graphics, BasicEffect effect)
        {
            
            base.Draw(graphics, effect);
        }
    }
}
