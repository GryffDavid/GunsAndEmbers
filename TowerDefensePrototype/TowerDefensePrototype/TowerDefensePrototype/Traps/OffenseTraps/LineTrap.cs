using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class LineTrap : Trap
    {
        public LineTrap(Vector2 position)
        {
            Position = position;
            Solid = false;
            MaxHP = 50;
            TrapType = TrapType.Line;
            DetonateDelay = 1500;
            DetonateLimit = 5;
            AffectedTime = 300;
            ResourceCost = 200;
            Animated = false;
        }
    }
}
