using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowerDefensePrototype
{
    class IceWorld : World
    {
        public IceWorld()
        {
            WorldType = WorldType.Ice;
            BackgroundAsset = "Backgrounds/TestBackground";
            GroundAsset = "Ground7";
        }
    }
}
