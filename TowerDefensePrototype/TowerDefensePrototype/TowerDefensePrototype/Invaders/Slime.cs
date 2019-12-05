using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Slime : Invader
    {
        public Slime(Vector2 position)
        {
            Active = true;
            MoveVector = new Vector2(-1, 0);
            Position = position;
            AssetName = "Invaders/Slime";
            CurrentHP = 100;
            MaxHP = 100;
            MoveDelay = 5;
            ResourceMinMax = new Vector2(1, 5);
            CurrentAttackDelay = 0;
            AttackDelay = 1500;
            AttackPower = 4;
            FrameSize = new Vector2(50, 28);
            FrameDelay = 120;
            TotalFrames = 2;
            CurrentFrame = 0;
            InvaderType = InvaderType.Slime;
            YRange = new Vector2(525, 630);          
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
