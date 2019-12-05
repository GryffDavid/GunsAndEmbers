using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class FireElemental : Invader
    {
    
        public FireElemental(Vector2 position)
        {
            Active = true;
            Direction = new Vector2(-1, 0);
            ActualPosition = position;
            CurrentHP = 50;
            MaxHP = 50;
            //MoveDelay = 10;
            ResourceMinMax = new Vector2(50, 100);
            CurrentAttackDelay = 0;
            AttackDelay = 3000;
            TowerAttackPower = 1;
            CurrentFrame = 0;
            InvaderType = InvaderType.FireElemental;
            YRange = new Vector2(700, 900);

            CurrentAnimation = new InvaderAnimation()
            {
                TotalFrames = 1,
                FrameDelay = 500
            };

            Color FireColor = new Color(Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 200);
            Color FireColor2 = new Color(Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 90);
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
