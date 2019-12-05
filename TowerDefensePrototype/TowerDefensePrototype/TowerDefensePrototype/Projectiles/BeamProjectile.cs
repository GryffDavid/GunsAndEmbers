using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    //This is the base class for the persistent beams
    class BeamProjectile : LightProjectile
    {
        public BeamProjectile(Vector2 position, Vector2 direction, float? damage = null)
            : base(position, direction, damage)
        {
            Active = true;
            Position = position;
            Ray = new Ray(new Vector3(Position.X, Position.Y, 0), new Vector3(direction.X, direction.Y, 0));

            if (damage != null)
                Damage = damage.Value;
            else
                Damage = 0;
        }
    }
}
