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
            CanMove = true;
            MoveVector = new Vector2(-1, 0);
            Position = position;
            AssetName = "PixelTestStrip";
            CurrentHP = 50;
            MaxHP = 50;
            MoveDelay = 5;
            ResourceMinMax = new Vector2(1, 5);
            CurrentAttackDelay = 0;
            AttackDelay = 1500;
            AttackPower = 4;
            FrameSize = new Vector2(30, 60);
            FrameDelay = 120;
            TotalFrames = 9;
            CurrentFrame = 0;
            InvaderType = InvaderType.Soldier;
            YRange = new Vector2(475, 560);          
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
                        DamageOverTime(3000, 2, 300, Color.Lime);
                        break;

                    case TrapType.Spikes:
                        CurrentHP -= 10;
                        break;

                    case TrapType.Catapult:
                        Trajectory(new Vector2(5, -10));
                        break;

                    case TrapType.Ice:
                        Freeze(4000);
                        break;

                    case TrapType.Tar:
                        MakeSlow(4000, 80);
                        break;
                }
            }
        }
    }
}
