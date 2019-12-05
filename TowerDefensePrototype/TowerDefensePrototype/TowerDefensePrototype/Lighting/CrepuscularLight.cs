using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TowerDefensePrototype
{
    class CrepuscularLight
    {
        public Vector2 Position;
        public float Decay, Exposure, Density, Weight, Depth;
        public int NumSamples = 120;
    }
}
