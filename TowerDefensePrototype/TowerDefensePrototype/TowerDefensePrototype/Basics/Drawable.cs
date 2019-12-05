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
        public BlendState BlendState = BlendState.AlphaBlend;
        public bool Emissive = false;
        public bool Normal = false;


        public virtual void Draw(SpriteBatch spriteBatch)
        {

        }

        public virtual void Draw(GraphicsDevice graphics, Effect effect)
        {

        }

        public virtual void Draw(GraphicsDevice graphics, BasicEffect effect, Effect shadowEffect, List<Light> lightList)
        { 

        }

        public virtual void DrawSpriteDepth(GraphicsDevice graphics, Effect effect)
        {

        }

        public virtual void DrawSpriteNormal(GraphicsDevice graphics, BasicEffect basicEffect)
        {

        }
    }
}
