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
        private static int _ResourceCost = 120;
        public static new int ResourceCost
        {
            get { return _ResourceCost; }
        }

        public FireTrap(Vector2 position)
            : base(position)
        {
            Solid = false;
            MaxHP = 50;
            TrapType = TrapType.Fire;
            DetonateDelay = 1500;
            DetonateLimit = 5;
            //AffectedTime = 300;
            PowerCost = 1;

            NormalDamage = 0;

            InvaderDOT = new DamageOverTimeStruct()
            {
                InitialDamage = 10,
                Damage = 1,
                MaxDelay = 3600,
                MaxInterval = 300,
                Color = Color.Orange
            };

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


