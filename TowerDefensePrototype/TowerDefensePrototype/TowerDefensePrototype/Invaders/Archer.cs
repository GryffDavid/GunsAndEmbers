using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class Archer : HeavyRangedInvader
    {
        public Archer(Vector2 position)
        {
            Active = true;
            Direction = new Vector2(-1.75f, 0);
            Position = position;
            CurrentHP = 50;
            MaxHP = 50;
            //MoveDelay = 5;
            ResourceMinMax = new Vector2(1, 5);
            CurrentAttackDelay = 0;
            AttackDelay = 3000;
            TowerAttackPower = 1;
            CurrentFrame = 0;
            InvaderType = InvaderType.Archer;
            YRange = new Vector2(700, 900);
            RangedAttackPower = 7;
            Range = new Vector2(200, 600);
            AngleRange = new Vector2(110, 160);
            PowerRange = new Vector2(9, 12);

            CurrentAnimation = new Animation()
            {
                TotalFrames = 1,
                FrameDelay = 250
            };
        }

        public override void TrapDamage(Trap trap)
        {
            if (VulnerableToTrap == true)
            {
                switch (trap.TrapType)
                {
                    default:
                        CurrentHP -= trap.NormalDamage;
                        DamageOverTime(trap.InvaderDOT, trap.InvaderDOT.Color);
                        Freeze(trap.InvaderFreeze, trap.InvaderDOT.Color);
                        MakeSlow(trap.InvaderSlow);
                        break;
                }
            }
        }
    }
}
