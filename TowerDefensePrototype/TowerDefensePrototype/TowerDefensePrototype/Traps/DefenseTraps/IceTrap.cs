using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class IceTrap : Trap
    {
        public IceTrap(Vector2 position)
            : base(position)
        {
            Solid = false;
            MaxHP = 50;
            TrapType = TrapType.Ice;
            DetonateDelay = 4000;
            DetonateLimit = 5;
            //AffectedTime = 300;
        }       
    }
}
