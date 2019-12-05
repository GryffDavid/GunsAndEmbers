using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class TarTrap : Trap
    {
        public TarTrap(Vector2 position)
        {
            Position = position;
            Solid = false;
            AssetName = "IceTrap";
            MaxHP = 50;
            TrapType = TrapType.Tar;
            DetonateDelay = 3000;
            DetonateLimit = 4;
            AffectedTime = 0;
        }    
    }
}
