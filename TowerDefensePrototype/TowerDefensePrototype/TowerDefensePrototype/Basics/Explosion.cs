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
        public float BlastRadius, Damage;

        public Explosion(Vector2 position, float blastRadius, float damage)
        {
            Active = true;
            Position = position;
            BlastRadius = blastRadius;
            Damage = damage;
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(GameTime gameTime)
        {

        }
    }
}
