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
            MoveVector = new Vector2(-0.5f, 0);
            Position = position;
            CurrentHP = 40;
            MaxHP = 40;
            MoveDelay = 10;
            ResourceMinMax = new Vector2(1, 5);
            YRange = new Vector2(700, 900);
            AttackPower = 1;
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
                AssetName = "Invaders/Invader",
                TotalFrames = 1,
                FrameDelay = 250
            };

            DustEmitter = new Emitter("Particles/Smoke", new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Bottom - 8),
                                      new Vector2(60, 60), new Vector2(0.5f, 1f), new Vector2(20, 30), 0.5f, true, new Vector2(0, 0),
                                      new Vector2(-2, 2), new Vector2(0.25f, 0.5f), Color.SaddleBrown, Color.SaddleBrown, 0f, -1, 600,
                                      1, false, new Vector2(0, 720), false);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void TrapDamage(TrapType trapType)
        {
            if (VulnerableToTrap == true)
            {
                switch (trapType)
                {
                    case TrapType.Fire:
                        CurrentHP -= 10;
                        DamageOverTime(3000, 2, 300, Color.Red);
                        break;

                    case TrapType.Spikes:
                        CurrentHP -= 10;
                        break;

                    case TrapType.Catapult:
                        Trajectory(new Vector2(5, -10));
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
