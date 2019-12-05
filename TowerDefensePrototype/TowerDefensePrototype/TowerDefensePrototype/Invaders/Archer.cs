using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Archer : RangedInvader
    {
        public Archer(Vector2 position)
        {
            Active = true;
            CanMove = true;
            MoveVector = new Vector2(-1, 0);
            Position = position;
            AssetName = "Star";
            CurrentHP = 50;
            MaxHP = 50;
            MoveDelay = 50;
            ResourceMinMax = new Vector2(1, 5);
            CurrentAttackDelay = 0;
            AttackDelay = 1500;
            AttackPower = 4;
            ProjectileType = HeavyProjectileType.Arrow;
            InvaderType = TowerDefensePrototype.InvaderType.Soldier;
        }
    }
}
