using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class TestInvader : LightRangedInvader
    {
        public TestInvader(Vector2 position)
        {
            Active = true;
            Direction = new Vector2(-0.5f, 0);
            Position = position;
            CurrentHP = 40;
            MaxHP = 40;
            //MoveDelay = 10;
            ResourceMinMax = new Vector2(1, 5);
            YRange = new Vector2(700, 900);
            TowerAttackPower = 1;
            CurrentFrame = 0;
            InvaderType = InvaderType.TestInvader;
            RangedAttackPower = 7;
            AngleRange = new Vector2(160, 180);
            Range = new Vector2(600, 900);
            CurrentAttackDelay = 0;  
            AttackDelay = 300;
            CurrentBurstDelay = 0;
            MaxBurstDelay = 2000;
            MaxBurst = 5;
            CurrentBurst = 0;

            CurrentAnimation = new Animation()
            {
                TotalFrames = 1,
                FrameDelay = 250
            };
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
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
