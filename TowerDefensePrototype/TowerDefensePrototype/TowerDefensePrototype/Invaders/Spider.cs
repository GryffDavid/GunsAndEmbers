using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class Spider : RangedInvader
    {
        public Spider(Vector2 position)
         {
            Active = true;
            MoveVector = new Vector2(-1, 0);
            Position = position;
            AssetName = "Invaders/SpiderSprite";
            CurrentHP = 50;
            MaxHP = 50;
            MoveDelay = 20;
            ResourceMinMax = new Vector2(1, 5);
            CurrentAttackDelay = 0;
            AttackDelay = 3000;
            AttackPower = 1;
            FrameSize = new Vector2(60, 30);
            FrameDelay = 500;
            TotalFrames = 3;
            CurrentFrame = 0;
            InvaderType = InvaderType.Spider;
            YRange = new Vector2(525, 630);
            RangedAttackPower = 20;
            Range = new Vector2(200, 600); 
            AngleRange = new Vector2(110, 160);
            PowerRange = new Vector2(9, 12);            
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
