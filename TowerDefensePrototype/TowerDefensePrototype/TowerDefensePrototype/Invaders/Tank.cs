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
            CanMove = true;
            MoveVector = new Vector2(-1, 0);
            Position = position;
            AssetName = "Troll";
            CurrentHP = 500;
            MaxHP = 500;
            MoveDelay = 40;
            ResourceMinMax = new Vector2(20, 90);
            CurrentAttackDelay = 0;
            AttackDelay = 1500;
            AttackPower = 30;
            FrameSize = new Vector2(37, 58);
            FrameDelay = 120;
            TotalFrames = 1;
            CurrentFrame = 0;
            InvaderType = InvaderType.Tank;
            Scale = new Vector2(1.5f, 1.5f);
            YRange = new Vector2(475, 560);
        }

        public override void TrapDamage(TrapType trapType)
        {
            if (VulnerableToTrap == true)
            {
                switch (trapType)
                {
                    case TrapType.Fire:

                        break;

                    case TrapType.Spikes:

                        break;

                    case TrapType.Catapult:

                        break;

                    case TrapType.Ice:

                        break;

                    case TrapType.Tar:

                        break;
                }
            }
        }
    }
}
