﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class BarrelTrap : Trap
    {
        private static int _ResourceCost = 200;
        public static new int ResourceCost
        {
            get { return _ResourceCost; }
        }

        public BarrelTrap(Vector2 position)
            : base(position)
        {
            Solid = true;
            MaxHP = 50;
            TrapType = TrapType.Barrel;
            DetonateDelay = 10000;
            DetonateLimit = 1;
        }
    }
}
