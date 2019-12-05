using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class BatteringRam : Invader
    {
        public BatteringRam(Vector2 position)
        {
            Active = true;
            Position = position;
            AssetName = "BatteringRam";
            HP = 1000;
        }

        public override void Behaviour()
        {
            Position.X -= 0.5f;
        }
    }
}
