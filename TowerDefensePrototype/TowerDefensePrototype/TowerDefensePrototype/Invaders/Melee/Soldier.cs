using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Soldier : Invader
    {      
        public Soldier(Vector2 position)
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
            InvaderType = InvaderType.Soldier;
            YRange = new Vector2(700, 900);
            Airborne = false;

            InvaderState = InvaderState.Walk;

            //DustEmitter = new Emitter("Particles/Smoke", new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Bottom - 8),
            //                                   new Vector2(60, 60), new Vector2(0.5f, 1f), new Vector2(20, 30), 0.5f, true, new Vector2(0, 0),
            //                                   new Vector2(-2, 2), new Vector2(0.25f, 0.5f), Color.SaddleBrown, Color.SaddleBrown, 0f, -1, 600, 1, false, new Vector2(0, 720), false);

            //CurrentAnimation = new Animation() 
            //{
            //    TotalFrames = 4, 
            //    FrameDelay = 150 
            //};

            //CurrentFrame = Random.Next(0, CurrentAnimation.TotalFrames);
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
