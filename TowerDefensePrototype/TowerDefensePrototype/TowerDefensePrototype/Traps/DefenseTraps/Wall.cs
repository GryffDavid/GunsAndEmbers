using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Wall : Trap
    {
        public Wall(Vector2 position, Vector2? position2)
        {
            Position = position;
            Solid = true;
            MaxHP = 100;
            AssetName = "Traps/Trap";
            TrapType = TrapType.Wall;
            DetonateLimit = -1;
            Animated = false;
        }
    }
}
