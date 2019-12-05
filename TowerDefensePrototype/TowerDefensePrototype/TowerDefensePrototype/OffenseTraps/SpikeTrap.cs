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
            Solid = false;
            AssetName = "Traps/SpikeTrap";
            TrapType = TrapType.Spikes;
            MaxHP = 50;
            DetonateDelay = 300;
            DetonateLimit = -1;
            FrameTime = 30;
            FrameCount = 1;
        }
    }
}
