using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class BlankTrap : Trap
    {
        public BlankTrap()
        {
            AssetName = "Blank";
            CurrentHP = 100000;
            TrapType = TrapType.Blank;
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, 1, 1);
        }
    }
}
