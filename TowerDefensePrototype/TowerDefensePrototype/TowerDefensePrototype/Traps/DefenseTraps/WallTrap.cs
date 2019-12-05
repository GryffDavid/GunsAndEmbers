using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class WallTrap : Trap
    {
        private static int _ResourceCost = 200;
        public static new int ResourceCost
        {
            get { return _ResourceCost; }
        }

        public WallTrap(Vector2 position)
            : base(position)
        {
            Solid = true;
            MaxHP = 100;
            TrapType = TrapType.Wall;
            DetonateLimit = -1;
        }
    }
}
