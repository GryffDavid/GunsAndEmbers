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
        public Wall(Vector2 position)
        {
            Position = position;
            Solid = true;
            CurrentHP = 50;
            AssetName = "Wall";
            TrapType = TrapType.Wall;
            DetonateLimit = -1;
        }
    }
}
