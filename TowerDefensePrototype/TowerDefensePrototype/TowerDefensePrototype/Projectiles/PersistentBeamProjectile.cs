using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class PersistentBeamProjectile : LightProjectile
    {
        public PersistentBeamProjectile(Vector2 position, Vector2 Direction, float? damage = null)
        {
            Active = true;
            Position = position;
            Ray = new Ray(new Vector3(Position.X, Position.Y, 0), new Vector3(Direction.X, Direction.Y, 0));
            LightProjectileType = LightProjectileType.PersistentBeam;

            if (damage != null)
                Damage = damage.Value;
            else
                Damage = 0;
        }
    }
}
