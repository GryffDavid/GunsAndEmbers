using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public abstract class Drawable
    {
        public float DrawDepth = 0;

        public virtual void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
