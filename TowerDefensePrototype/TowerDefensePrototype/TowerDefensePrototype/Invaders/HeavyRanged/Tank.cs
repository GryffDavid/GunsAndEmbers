using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;  
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Tank : Invader
    {
        public Tank(Vector2 position)
        {
            Active = true;
            Direction = new Vector2(-1, 0);
            Position = position;
            CurrentHP = 800;
            MaxHP = 800;
            //MoveDelay = 40;
            ResourceMinMax = new Vector2(5, 20);
            CurrentAttackDelay = 0;
            AttackDelay = 1500;
            TowerAttackPower = 30;            
            CurrentFrame = 0;
            InvaderType = InvaderType.Tank;
            Scale = new Vector2(1f, 1f);
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
