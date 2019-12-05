using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class ParallaxBackground
    {
        public class BackgroundElement
        {
            public int DepthPosition;
            public Vector2 Velocity;
            public Vector2 Position, DestPosition;
            public Texture2D Texture;
            public Rectangle DestinationRectangle;
        }

        public List<BackgroundElement> BackgroundList = new List<BackgroundElement>();

        public ParallaxBackground()
        {

        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
