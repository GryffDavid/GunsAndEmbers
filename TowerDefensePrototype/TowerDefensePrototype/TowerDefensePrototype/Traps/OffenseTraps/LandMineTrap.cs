using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class LandMineTrap : Trap
    {
        private static int _ResourceCost = 25;
        public static new int ResourceCost
        {
            get { return _ResourceCost; }
        }

        public LandMineTrap(Vector2 position)
            : base(position)
        {
            Solid = false;
            MaxHP = 50;
            TrapType = TrapType.LandMine;
            DetonateDelay = 10000;
            DetonateLimit = 1;
            OnGround = true;
            NormalDamage = 100;
            PowerCost = 1;
        }
    }
}
