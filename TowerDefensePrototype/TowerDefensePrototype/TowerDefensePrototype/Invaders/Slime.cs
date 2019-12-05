using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Slime : Invader
    {
        public Slime(Vector2 position)
        {
            Active = true;
            Direction = new Vector2(-1, 0);
            Position = position;
            CurrentHP = 100;
            MaxHP = 100;
            //MoveDelay = 5;
            ResourceMinMax = new Vector2(1, 5);
            CurrentAttackDelay = 0;
            AttackDelay = 1500;
            TowerAttackPower = 4;            
            CurrentFrame = 0;
            InvaderType = InvaderType.Slime;
            YRange = new Vector2(700, 900);

            CurrentAnimation = new Animation() 
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
