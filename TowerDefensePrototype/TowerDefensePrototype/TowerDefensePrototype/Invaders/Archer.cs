using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class Archer : RangedInvader
    {
        public Archer(Vector2 position)
        {
            Active = true;
            MoveVector = new Vector2(-1.75f, 0);
            Position = position;
            CurrentHP = 50;
            MaxHP = 50;
            MoveDelay = 5;
            ResourceMinMax = new Vector2(1, 5);
            CurrentAttackDelay = 0;
            AttackDelay = 3000;
            AttackPower = 1;
            CurrentFrame = 0;
            InvaderType = InvaderType.Archer;
            YRange = new Vector2(525, 630);
            RangedAttackPower = 7;
            Range = new Vector2(200, 600);
            AngleRange = new Vector2(110, 160);
            PowerRange = new Vector2(9, 12);

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
                }
            }
        }
    }
}
