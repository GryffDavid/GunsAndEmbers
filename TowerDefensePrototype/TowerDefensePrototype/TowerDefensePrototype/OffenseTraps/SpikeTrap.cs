﻿using System;
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
        }

        public override int HP
        {
            get { return 50; }
        }

        public override bool Solid
        {
            get { return false; }
        }

        public override string AssetName { get { return "SpikeTrap"; } }

        //public override TrapType TrapType
        //{
        //    get { return TrapType.Spikes; }
        //}
    }
}
