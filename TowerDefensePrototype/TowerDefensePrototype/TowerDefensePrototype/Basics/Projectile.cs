using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Projectile
    {
        public Ray Ray;
        public bool Active;

        public Projectile(Vector2 Position, Vector2 Direction)
        {
            Active = true;
            Ray = new Ray(new Vector3(Position.X, Position.Y, 0), new Vector3(Direction.X, Direction.Y, 0));
        }
    }
}
