using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace TowerDefensePrototype
{
    class FireTrap : Trap
    {
        public FireTrap(Vector2 position)
        {
            Position = position;
            Solid = false;
            MaxHP = 50;
            TrapType = TrapType.Fire;
            DetonateDelay = 1500;
            DetonateLimit = 5;
            AffectedTime = 300;
            ResourceCost = 200;
            Radius = 3;
            Animated = false;
        }
    }
}


