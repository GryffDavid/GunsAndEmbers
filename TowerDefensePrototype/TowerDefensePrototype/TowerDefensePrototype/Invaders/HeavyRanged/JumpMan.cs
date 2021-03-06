﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace TowerDefensePrototype
{
    class JumpMan : HeavyRangedInvader
    {
        public override float OriginalSpeed { get { return 0.68f; } }

        enum SpecificBehaviour { None, Hovering, Landing };
        SpecificBehaviour JumpManBehaviour = SpecificBehaviour.None;

        public Vector2 HoverRange = new Vector2(90, 110);

        public JumpMan(Vector2 position, Vector2? yRange = null)
            : base(position, yRange)
        {
            MaxHP = 20;
            ResourceMinMax = new Vector2(8, 20);
            InvaderType = InvaderType.JumpMan;
            //InAir = false;
            //Airborne = false;
            //CanAttack = true;

            InvaderAnimationState = AnimationState_Invader.Walk;
            CurrentMacroBehaviour = MacroBehaviour.AttackTower;
            CurrentMicroBehaviour = MicroBehaviour.MovingForwards;

            AngleRange = new Vector2(25, 30);
            TowerDistanceRange = new Vector2(1500, 1500);
            TrapDistanceRange = new Vector2(250, 350);
            LaunchVelocityRange = new Vector2(25, 30);

            RangedAttackTiming = new RangedAttackTiming()
            {
                MaxFireDelay = 6500,
                CurrentFireDelay = 0
            };

            RangedDamage = 10;

            CurrentAngle = 0;
            EndAngle = 0;
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            if (InAir == true &&
                Airborne == false &&
                Velocity.Y < 0.0f && Velocity.Y > -1f)
            {
                HoverRange = new Vector2(Position.Y - 10, Position.Y + 10);

                Velocity.Y = 0;
                Velocity.X = -0.1f;
                Airborne = true;
                Gravity = 0.1f;
                JumpManBehaviour = SpecificBehaviour.Hovering;
                CurrentMicroBehaviour = MicroBehaviour.Attack;
            }

            switch (JumpManBehaviour)
            {
                #region Hovering
                case SpecificBehaviour.Hovering:
                    {
                        if (Position.Y > HoverRange.X &&
                            Position.Y < HoverRange.Y)
                        {
                            Velocity.Y += Gravity;
                        }

                        if (Position.Y > HoverRange.Y)
                        {
                            Velocity.Y -= Gravity * 1.5f;
                        }
                    }
                    break;
                #endregion
            }

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

                            if (InAir == false)
                            {
                                if (Slow == true)
                                    Velocity.X = Direction.X * SlowSpeed;
                                else
                                    Velocity.X = Direction.X * Speed;
                            }
                            #endregion

                            switch (CurrentMacroBehaviour)
                            {
                                #region Attack Tower
                                case MacroBehaviour.AttackTower:
                                    {
                                        if (DistToTower <= MinTowerRange)
                                        {
                                            InTowerRange = true;

                                            if (InAir == false)
                                            {
                                                Trajectory(new Vector2(-3, -8));
                                                Gravity = 0.2f;
                                                Airborne = false;
                                                InAir = true;
                                            }
                                        }
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
                            //Velocity.X = 0;
                            //CanAttack = false;

                            //if (CurrentAngle != EndAngle)
                            //{
                            //    if (Math.Abs(EndAngle - CurrentAngle) > MathHelper.ToRadians(0.25f))
                            //    {
                            //        CurrentAngle = MathHelper.Lerp(CurrentAngle, EndAngle, 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                            //    }
                            //    else
                            //    {
                            //        EndAngle = CurrentAngle;
                            //        CurrentMicroBehaviour = MicroBehaviour.Attack;
                            //    }
                            //}
                            //else
                            //{
                            //    EndAngle = CurrentAngle;
                            //    CurrentMicroBehaviour = MicroBehaviour.Attack;
                            //}
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
                                        UpdateFireDelay(gameTime);
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


            base.Update(gameTime, cursorPosition);
        }
    }
}
