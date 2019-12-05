using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;  
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Tank : Invader
    {
        public Tank(Vector2 position)
        {
            Active = true;
            MoveVector = new Vector2(-1, 0);
            Position = position;
            CurrentHP = 800;
            MaxHP = 800;
            MoveDelay = 40;
            ResourceMinMax = new Vector2(5, 20);
            CurrentAttackDelay = 0;
            AttackDelay = 1500;
            AttackPower = 30;            
            CurrentFrame = 0;
            InvaderType = InvaderType.Tank;
            Scale = new Vector2(1f, 1f);
            YRange = new Vector2(525, 630);
            Airborne = false;

            CurrentAnimation = new Animation() 
            { 
                AssetName = "Invaders/Invader", 
                TotalFrames = 1, 
                FrameDelay = 120 
            };
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

                        break;

                    case TrapType.Catapult:

                        break;

                    case TrapType.Ice:
                        Freeze(4000, Color.LightBlue);
                        break;

                    case TrapType.Tar:

                        break;
                }
            }
        }
    }
}
