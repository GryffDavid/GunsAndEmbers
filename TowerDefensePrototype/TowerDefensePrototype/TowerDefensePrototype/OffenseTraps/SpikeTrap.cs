using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class SpikeTrap : Trap
    {
        public SpikeTrap(Vector2 position)
        {
            Position = position;
            Solid = false;
            AssetName = "SpikeTrap";
            TrapType = TrapType.Spikes;
            HP = 50;
        }
    }
}
