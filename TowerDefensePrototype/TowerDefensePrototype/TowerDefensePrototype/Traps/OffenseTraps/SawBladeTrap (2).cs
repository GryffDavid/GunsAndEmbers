using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class SawBladeTrap : Trap
    {
        public SawBladeTrap(Vector2 position)
        {
            Position = position;
            Solid = false;
            MaxHP = 50;
            TrapType = TrapType.SawBlade;
            DetonateDelay = 2000;
            DetonateLimit = 5;
            //AffectedTime = 300;
        }
    }
}
