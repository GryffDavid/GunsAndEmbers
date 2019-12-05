using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype.Invaders.HeavyRanged
{
    class HarpoonCannon : HeavyRangedInvader
    {
        public HarpoonCannon(Vector2 position, Vector2? yRange = null)
            : base(position, yRange)
        {
            Speed = 1f;
            MaxHP = 200;
            ResourceMinMax = new Vector2(8, 20);
            YRange = new Vector2(700, 900);
            IntelligenceRange = new Vector2(0f, 1.0f);

            InvaderType = InvaderType.HarpoonCannon;

            InvaderAnimationState = AnimationState_Invader.Stand;
            CurrentMacroBehaviour = MacroBehaviour.AttackTower;
            CurrentMicroBehaviour = MicroBehaviour.MovingForwards;

            AngleRange = new Vector2(30, 60);
            TowerDistanceRange = new Vector2(750, 850);
            TrapDistanceRange = new Vector2(250, 350);
            LaunchVelocityRange = new Vector2(12, 17);
            MaxFireDelay = 1500;
            CurrentFireDelay = 0;
            RangedDamage = 10;

            CurrentAngle = 0;
            EndAngle = 0;
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            if (CurrentBehaviourDelay > MaxBehaviourDelay)
                switch (CurrentMicroBehaviour)
                {
                    #region Stationary
                    case MicroBehaviour.Stationary:
                        {
                            Velocity.X = 0;
                        }
                        break;
                    #endregion

                    #region MovingForwards
                    case MicroBehaviour.MovingForwards:
                        {
                            #region Move
                            Direction.X = -1;

                            if (Slow == true)
                                Velocity.X = Direction.X * SlowSpeed;
                            else
                                Velocity.X = Direction.X * Speed;
                            #endregion
                        }
                        break;
                    #endregion

                    #region MovingBackwards
                    case MicroBehaviour.MovingBackwards:
                        {
                            #region Move
                            Direction.X = 1;

                            if (Slow == true)
                                Velocity.X = Direction.X * SlowSpeed;
                            else
                                Velocity.X = Direction.X * Speed;
                            #endregion
                        }
                        break;
                    #endregion

                    #region AdjustTrajectory
                    case MicroBehaviour.AdjustTrajectory:
                        {

                        }
                        break;
                    #endregion

                    #region Attack
                    case MicroBehaviour.Attack:
                        {
                            switch (CurrentMacroBehaviour)
                            {
                                #region Attack Tower
                                case MacroBehaviour.AttackTower:
                                    {

                                    }
                                    break;
                                #endregion

                                #region Attack Traps
                                case MacroBehaviour.AttackTraps:
                                    {

                                    }
                                    break;
                                #endregion
                            }
                        }
                        break;
                    #endregion
                }

            //BarrelPivot = new Vector2(BarrelAnimation.FrameSize.X / 2, BarrelAnimation.FrameSize.Y / 2);
            //BasePivot = new Vector2(Position.X, Position.Y);

            base.Update(gameTime, cursorPosition);
        }
    }
}
