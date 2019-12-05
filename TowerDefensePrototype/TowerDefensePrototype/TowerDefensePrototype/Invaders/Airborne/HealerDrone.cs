using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class HealDrone : LightRangedInvader
    {
        //MEDBOT
        //Flies above the other invaders and heals them when necessary.
        //Should check which invader is the most urgently in need of attention but not too close to the tower
        //Needs to then fly over to that invader to get in range and begin to heal it
        //The healing beam can be interrupted by a damage threshold - i.e. taking more than 10 damage in a single burst interrupts the heal
        //and it then needs to charge back up again
        //A red sine-wave shaped beam would be pretty cool. Just saying.
        public HealDrone(Vector2 position)
        {
            Active = true;
            Direction = new Vector2(-1, 0);
            Speed = 1.5f;
            Position = position;
            MaxHP = 300;
            CurrentHP = MaxHP;
            //MoveDelay = 20;
            ResourceMinMax = new Vector2(1, 5);
            CurrentAttackDelay = 0;
            AttackDelay = 1500;
            TowerAttackPower = 4;
            InvaderType = InvaderType.HealDrone;
            CurrentFrame = 0;
            YRange = new Vector2(100, 350);
            RangedAttackPower = 20;
            Airborne = true;

            CurrentInvaderState = InvaderState.Walking;

            CurrentAnimation = new Animation()
            {
                TotalFrames = 1,
                FrameDelay = 500
            };
        }

        public override void TrapDamage(Trap trap)
        {
            throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            if (Velocity.X != 0)
            {
                CurrentInvaderState = InvaderState.Walking;
            }
            else
            {
                CurrentInvaderState = InvaderState.Standing;
            }

            if (CurrentInvaderState != PreviousInvaderState || PreviousInvaderState == null)
            {
                switch (CurrentInvaderState)
                {
                    case InvaderState.Walking:
                        CurrentTexture = TextureList[0];
                        CurrentAnimation = new Animation() { Texture = CurrentTexture, TotalFrames = 1, FrameDelay = 150 };
                        CurrentAnimation.Looping = false;
                        CurrentAnimation.Animated = false;
                        break;

                    case InvaderState.Standing:
                        CurrentTexture = TextureList[0];
                        CurrentAnimation = new Animation() { Texture = CurrentTexture, TotalFrames = 1, FrameDelay = 300 };
                        CurrentAnimation.Looping = false;
                        CurrentAnimation.Animated = false;
                        break;
                }


                CurrentAnimation.GetFrameSize();
                CurrentFrame = Random.Next(0, CurrentAnimation.TotalFrames);
            }

            base.Update(gameTime, cursorPosition);
        }
    }
}
