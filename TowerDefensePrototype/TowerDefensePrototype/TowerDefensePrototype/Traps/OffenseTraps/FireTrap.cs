using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace TowerDefensePrototype
{
    class FireTrap : Trap
    {
        public FireTrap(Vector2 position)
        {
            Position = position;
            Solid = false;
            MaxHP = 50;
            TrapType = TrapType.Fire;
            DetonateDelay = 1500;
            DetonateLimit = 5;
            AffectedTime = 300;
            ResourceCost = 120;

            NormalDamage = 0;

            InvaderDOT = new DamageOverTimeStruct()
            {
                InitialDamage = 10,
                Damage = 1,
                Milliseconds = 3600,
                Interval = 300,
                Color = Color.Orange
            };

            //InvaderSlow = new SlowStruct()
            //{
            //    Milliseconds = 3000,
            //    SpeedPercentage = 0.5f
            //};
        }
    }
}


