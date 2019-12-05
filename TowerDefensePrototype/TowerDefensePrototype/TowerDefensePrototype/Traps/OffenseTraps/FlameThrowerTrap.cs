using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class FlameThrowerTrap : Trap
    {
        private static int _ResourceCost = 200;
        public static new int ResourceCost
        {
            get { return _ResourceCost; }
        }

        public float ActiveTime, CurrentActiveTime;


        public FlameThrowerTrap(Vector2 position)
            : base(position)
        {
            Solid = true;
            OnGround = true;
            MaxHP = 100;
            TrapType = TrapType.FlameThrower;
            DetonateLimit = -1;
            ActiveTime = 3000f;
            CurrentActiveTime = 0;
            ChanceToFear = 0.05f;
        }

        public override void Update(GameTime gameTime)
        {
            switch (TrapState)
            {
                default:
                    OnGround = false;
                    Solid = true;
                    break;

                case TrapAnimationState.Untriggered:
                    OnGround = true;
                    DrawDepth = 0f;
                    Solid = false;
                    break;

                case TrapAnimationState.Triggering:
                    OnGround = false;
                    Solid = true;
                    break;

                case TrapAnimationState.Resetting:
                    OnGround = false;
                    Solid = true;
                    break;
            }
            base.Update(gameTime);
        }
    }
}
