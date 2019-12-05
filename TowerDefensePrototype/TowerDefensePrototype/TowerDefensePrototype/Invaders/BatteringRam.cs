using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class BatteringRam : Invader
    {
        public BatteringRam(Vector2 position)
        {
            Active = true;
            CanMove = true;
            Position = position;
            AssetName = "BatteringRam";
            CurrentHP = 1000;             
            MoveVector = new Vector2(-1, 0);
            MaxHP = 1000;
            MoveDelay = 10;
            ResourceMinMax = new Vector2(1, 5);
            CurrentAttackDelay = 0;
            AttackDelay = 1500;
            AttackPower = 4;
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
                        break;

                    case TrapType.Spikes:
                        CurrentHP -= 10;
                        break;

                    case TrapType.Catapult:
                        Trajectory(new Vector2(5, -10));
                        break;
                }
            }
        }
    }
}
