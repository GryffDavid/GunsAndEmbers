using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class GlueTrap : Trap
    {
        public GlueTrap(Vector2 position)
            : base(position)
        {
            Position = position;
            Solid = true;
            MaxHP = 100;
            TrapType = TrapType.Glue; //Remove "Trap" from TrapType enum name;

            DetonateLimit = -1;
            DetonateDelay = 3500;

            ResourceCost = 120;
            PowerCost = 1;

            NormalDamage = 0;

            //InvaderDOT = new DamageOverTimeStruct()
            //{
            //    InitialDamage = 10,
            //    Damage = 1,
            //    MaxDelay = 3600,
            //    MaxInterval = 300,
            //    Color = Color.Orange
            //};

            //InvaderSlow = new SlowStruct()
            //{
            //    Milliseconds = 3000,
            //    SpeedPercentage = 0.5f
            //};

            //InvaderFreeze = new FreezeStruct()
            //{
            //    CurrentDelay = 0,
            //    MaxDelay = 2000
            //};
        }
    }
}
