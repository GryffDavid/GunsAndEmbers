using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowerDefensePrototype
{
    class LavaWorld : World
    {
        public LavaWorld()
        {
            WorldType = WorldType.Lava;
            BackgroundAsset = "Blank";
            GroundAsset = "Ground5";
        }
    }
}
