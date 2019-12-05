using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace TowerDefensePrototype
{
    class JumpMan : LightRangedInvader
    {
        public JumpMan(Vector2 position)
        {
            Active = true;
            Direction = new Vector2(-1f, 0);
            Speed = 0.68f;
            Position = position;
            CurrentHP = 40;
            MaxHP = 40;
            ResourceMinMax = new Vector2(8, 20);
            CurrentAttackDelay = 0;
            AttackDelay = 1500;
            TowerAttackPower = 24;
            TrapAttackPower = 6;
            CurrentFrame = 0;
            InvaderType = InvaderType.JumpMan;
            YRange = new Vector2(700, 900);
            Airborne = false;

            InvaderState = InvaderState.Walk;
        }

        public void Update(GameTime gameTime)
        {

        }

        public override void TrapDamage(Trap trap)
        {
            if (VulnerableToTrap == true)
            {
                switch (trap.TrapType)
                {
                    default:
                        CurrentHP -= trap.NormalDamage;

                        if (trap.InvaderDOT != null)
                            DamageOverTime(trap.InvaderDOT, trap.InvaderDOT.Color);

                        if (trap.InvaderFreeze != null)
                            Freeze(trap.InvaderFreeze, trap.InvaderDOT.Color);

                        if (trap.InvaderSlow != null)
                            MakeSlow(trap.InvaderSlow);
                        break;
                }
            }
        }
    }
}
