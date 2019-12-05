using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TowerDefensePrototype
{
    public class Explosion
    {
        public Vector2 Position;
        public bool Active;
        public float BlastRadius;

        public Explosion(Vector2 position, float blastRadius)
        {
            Active = true;
            Position = position;
            BlastRadius = blastRadius;
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(GameTime gameTime)
        {

        }
    }
}
