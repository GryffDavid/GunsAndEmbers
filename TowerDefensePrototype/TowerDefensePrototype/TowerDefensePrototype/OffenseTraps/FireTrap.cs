
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
            AssetName = "FireTrap";
            CurrentHP = 50;
            TrapType = TrapType.Fire;
            DetonateDelay = 300;
        }
    }
}


