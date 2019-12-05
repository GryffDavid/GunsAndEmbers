using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public abstract class Drawable
    {
        public bool Active;
        public float DrawDepth;

        public virtual void Draw(SpriteBatch spriteBatch)
        {

        }

        public virtual void Draw(SpriteBatch spriteBatch, BasicEffect effect, GraphicsDevice graphicsDevice)
        {

        }

        public virtual void Draw(SpriteBatch spriteBatch, BasicEffect effect, GraphicsDevice graphicsDevice, Effect shadowEffect, List<Light> lightList)
        {

        }

        public virtual void DrawSpriteDepth(GraphicsDevice graphics, Effect effect)
        {

        }

        public virtual void DrawSpriteNormal(GraphicsDevice graphics, BasicEffect basicEffect)
        {

        }

        public virtual void DrawSpriteNormal(SpriteBatch spriteBatch)
        {

        }
    }
}
