using System;
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
        public BarrelTrap(Vector2 position)
        {
            Position = position;
            Solid = false;
            AssetName = "Traps/BarrelTrap";
            MaxHP = 50;
            TrapType = TrapType.Barrel;
            DetonateDelay = 10000;
            DetonateLimit = -1;
            FrameTime = 30;
            FrameCount = 1;
        }
    }
}
