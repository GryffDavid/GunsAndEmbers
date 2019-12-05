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

            CurrentInvaderState = InvaderState.Walking;
        }

        public void Update(GameTime gameTime)
        {
            if (Velocity.X != 0)
            {
                CurrentInvaderState = InvaderState.Walking;
            }
            else
            {
                CurrentInvaderState = InvaderState.Standing;
            }

            if (CurrentInvaderState != PreviousInvaderState || PreviousInvaderState == null)
            {
                switch (CurrentInvaderState)
                {
                    case InvaderState.Walking:
                        CurrentTexture = TextureList[0];
                        CurrentAnimation = new Animation() { Texture = CurrentTexture, TotalFrames = 4, FrameDelay = 150 };
                        CurrentAnimation.Looping = true;
                        CurrentAnimation.Animated = true;
                        break;

                    case InvaderState.Standing:
                        CurrentTexture = TextureList[1];
                        CurrentAnimation = new Animation() { Texture = CurrentTexture, TotalFrames = 2, FrameDelay = 300 };
                        CurrentAnimation.Looping = true;
                        CurrentAnimation.Animated = true;
                        break;
                }


                CurrentAnimation.GetFrameSize();
                CurrentFrame = Random.Next(0, CurrentAnimation.TotalFrames);
            }
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
