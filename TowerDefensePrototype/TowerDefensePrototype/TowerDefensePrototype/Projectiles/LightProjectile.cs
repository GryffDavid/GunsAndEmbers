using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public abstract class LightProjectile
    {
        public Ray Ray;
        public bool Active;
        public Vector2 Position;
        public LightProjectileType LightProjectileType;

        public void Update(GameTime gameTime)
        {

        }        
    }
}
