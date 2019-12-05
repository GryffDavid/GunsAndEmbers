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
            MoveVector = new Vector2(-1, 0);
            Position = position;
            CurrentHP = 150;
            MaxHP = 150;
            MoveDelay = 5;
            ResourceMinMax = new Vector2(1, 5);
            CurrentAttackDelay = 0;
            AttackDelay = 1500;
            AttackPower = 20;
            CurrentFrame = 0;            
            InvaderType = InvaderType.Soldier;
            YRange = new Vector2(525, 630);
            Airborne = false;

            ParticleEmitter = new Emitter("Particles/Smoke", new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Bottom - 8),
                                               new Vector2(60, 60), new Vector2(0.5f, 1f), new Vector2(20, 30), 0.5f, true, new Vector2(0, 0),
                                               new Vector2(-2, 2), new Vector2(0.25f, 0.5f), Color.SaddleBrown, Color.SaddleBrown, 0f, -1, 600, 1, false, new Vector2(0, 720), false);

            CurrentAnimation = new Animation() { AssetName = "Invaders/SoldierStrip3342", TotalFrames = 4, FrameDelay = 300 };
            CurrentFrame = Random.Next(0, CurrentAnimation.TotalFrames);
        }

        public override void Update(GameTime gameTime)
        {
            if (Active == true)
            {
                if (ParticleEmitter != null)
                {
                    ParticleEmitter.Update(gameTime);
                    ParticleEmitter.Position = new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Bottom);
                }
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
                        CurrentHP -= 10;
                        DamageOverTime(3000, 10, 300, Color.Red);
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
