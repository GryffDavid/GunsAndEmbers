﻿using System;
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
            Speed = 1;
            Position = position;
            CurrentHP = 20;
            MaxHP = 15;
            ResourceMinMax = new Vector2(8, 20);
            CurrentAttackDelay = 0;
            AttackDelay = 1500;
            AttackPower = 24;
            CurrentFrame = 0;            
            InvaderType = InvaderType.Soldier;
            YRange = new Vector2(700, 900);
            Airborne = false;

            CurrentInvaderState = InvaderState.Walking;

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

        public override void Update(GameTime gameTime)
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
                        break;

                    case InvaderState.Standing:
                        CurrentTexture = TextureList[1];
                        CurrentAnimation = new Animation() { Texture = CurrentTexture, TotalFrames = 2, FrameDelay = 300 };
                        break;
                }

                FrameSize = new Vector2(CurrentTexture.Width / CurrentAnimation.TotalFrames, CurrentTexture.Height);
                CurrentFrameDelay = 0;
                CurrentFrame = Random.Next(1, CurrentAnimation.TotalFrames);
            }

            base.Update(gameTime);
        }

        public override void TrapDamage(TrapType trapType)
        {
            if (VulnerableToTrap == true)
            {
                switch (trapType)
                {
                    case TrapType.Fire:
                        CurrentHP -= 7;
                        DamageOverTime(3000, 1, 300, Color.Orange);
                        break;

                    case TrapType.Spikes:
                        CurrentHP -= 10;
                        break;

                    case TrapType.Catapult:
                        //Trajectory(new Vector2(5, -10));
                        Velocity = new Vector2(5, -10);
                        break;

                    case TrapType.Ice:
                        Freeze(4000, Color.LightBlue);
                        break;

                    case TrapType.Tar:
                        MakeSlow(4000, 80);
                        break;

                    case TrapType.SawBlade:
                        CurrentHP -= 12;
                        break;
                }
            }
        }
    }
}
