using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace TowerDefensePrototype
{
    class SuicideBomber : Invader
    {
        public SuicideBomber(Vector2 position)
        {
            Active = true;
            Direction = new Vector2(-2, 0);
            Position = position;
            CurrentHP = 100;
            MaxHP = 100;
            //MoveDelay = 15;
            ResourceMinMax = new Vector2(1, 5);
            CurrentAttackDelay = 0;
            AttackDelay = 500;
            AttackPower = 200;
            CurrentFrame = 0;
            InvaderType = InvaderType.SuicideBomber;
            YRange = new Vector2(700, 900);
            Airborne = false;

            CurrentAnimation = new Animation()
            {
                TotalFrames = 1, 
                FrameDelay = 120 
            };
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
                        //Trajectory(new Vector2(5, -10));
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
