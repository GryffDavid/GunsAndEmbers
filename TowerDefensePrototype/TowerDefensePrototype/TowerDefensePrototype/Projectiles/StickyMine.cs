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
        //Texture2D Texture;
        //Vector2 Position;
        //Rectangle DestinationRectangle;
        int MineNumber;
        
        public StickyMine(Texture2D texture, Vector2 position, int mineNumber)
        {
            MineNumber = mineNumber;
            Texture = texture;
            Position = position;
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
        }

        public override void Initialize()
        {
            
            base.Initialize();
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
