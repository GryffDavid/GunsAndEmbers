using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TowerDefensePrototype
{
    class WaterElemental : Invader
    {
        public WaterElemental(Vector2 position, Vector2? yRange = null)
            : base(position, yRange)
        {

        }
    }
}
