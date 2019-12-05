﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace TowerDefensePrototype
{
    class RifleMan : LightRangedInvader
    {
        public RifleMan(Vector2 position)
        {
            Active = true;
            Direction = new Vector2(-1f, 0);
            Speed = 0.68f;
            ActualPosition = position;
            CurrentHP = 20;
            MaxHP = 20;
            ResourceMinMax = new Vector2(8, 20);
            CurrentAttackDelay = 0;
            AttackDelay = 1500;
            TowerAttackPower = 24;
            TrapAttackPower = 6;
            CurrentFrame = 0;
            InvaderType = InvaderType.RifleMan;
            YRange = new Vector2(700, 900);            
            InvaderState = InvaderState.Walk;
            Airborne = false;
            DistanceRange = new Vector2(800, 900);            
            //BlendState = BlendState.Additive;
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            //if (Velocity.X < 0)
            //{
            //    CurrentInvaderState = InvaderState.Walk;
            //}

            //switch (InvaderState)
            //{
            //    case InvaderState.Walk:
            //        CurrentAnimation = AnimationList[0];
            //        break;
            //}

            base.Update(gameTime, cursorPosition);
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
