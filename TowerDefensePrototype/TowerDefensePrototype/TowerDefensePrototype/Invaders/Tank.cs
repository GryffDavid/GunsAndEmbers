﻿using System;
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
            AssetName = "Invaders/Tank";
            CurrentHP = 500;
            MaxHP = 500;
            MoveDelay = 40;
            ResourceMinMax = new Vector2(20, 90);
            CurrentAttackDelay = 0;
            AttackDelay = 1500;
            AttackPower = 30;
            FrameSize = new Vector2(126, 64);
            FrameDelay = 120;
            TotalFrames = 1;
            CurrentFrame = 0;
            InvaderType = InvaderType.Tank;
            Scale = new Vector2(1f, 1f);
            YRange = new Vector2(525, 630);
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
