using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class CatapultTrap : Trap
    {
        public static new int ResourceCost = 200;

        //Maybe make invaders take extra turret damage when in the sky after using the catapult trap
        public CatapultTrap(Vector2 position)
            : base(position)
        {
            Solid = false;
            MaxHP = 50;
            TrapType = TrapType.Catapult;
            DetonateDelay = 5000;
            DetonateLimit = 8;
        }
    }
}
