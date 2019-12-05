using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace TowerDefensePrototype
{
    class SuicideBomber : Invader
    {
        public SuicideBomber(Vector2 position)
        {
            Active = true;
            Direction = new Vector2(-2, 0);
            Position = position;
            CurrentHP = 100;
            MaxHP = 100;
            //MoveDelay = 15;
            ResourceMinMax = new Vector2(1, 5);
            CurrentAttackDelay = 0;
            AttackDelay = 500;
            TowerAttackPower = 200;
            CurrentFrame = 0;
            InvaderType = InvaderType.SuicideBomber;
            YRange = new Vector2(700, 900);
            Airborne = false;

            CurrentAnimation = new InvaderAnimation()
            {
                TotalFrames = 1, 
                FrameDelay = 120 
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
