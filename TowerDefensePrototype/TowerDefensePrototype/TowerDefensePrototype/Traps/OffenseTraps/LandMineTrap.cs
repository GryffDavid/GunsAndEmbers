﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class LandMineTrap : Trap
    {
        public LandMineTrap(Vector2 position)
            : base(position)
        {
            Solid = false;
            MaxHP = 50;
            TrapType = TrapType.LandMine;
            DetonateDelay = 10000;
            DetonateLimit = 1;
            OnGround = true;
            NormalDamage = 100;
            ResourceCost = 25;
            PowerCost = 1;
        }
    }
}