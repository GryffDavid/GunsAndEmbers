using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class PopFragTrap : Trap
    {
        private static int _ResourceCost = 200;
        public static new int ResourceCost
        {
            get { return _ResourceCost; }
        }

        public PopFragTrap(Vector2 position)
            : base(position)
        {
            Position = position;
            Solid = false;
            MaxHP = 100;
            DetonateDelay = 1500;
            TrapType = TrapType.PopFrag;
            DetonateLimit = -1;

            PowerCost = 1;

            NormalDamage = 0;
        }
    }
}
