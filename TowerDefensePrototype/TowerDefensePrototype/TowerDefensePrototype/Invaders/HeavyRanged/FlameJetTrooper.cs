using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace TowerDefensePrototype
{
    class FlameJetTrooper : HeavyRangedInvader
    {
        //Ray ,FlameRay; //USE THIS RAY TO CHECK IF THE TRAP IS BEING DAMAGED AND TO DETERMINE THE ANGLE OF THE FLAMES

        public override float OriginalSpeed { get { return 0.68f; } }

        enum SpecificBehaviour { None, Hovering, Landing };
        SpecificBehaviour JumpManBehaviour = SpecificBehaviour.None;

        public Vector2 HoverRange = new Vector2(90, 110);
        public Vector2 TrapDirection;

        public float FlameTime, CurrentFlameTime;
        

        public FlameJetTrooper(Vector2 position, Vector2? yRange = null)
            : base(position, yRange)
        {
            MaxHP = 20;
            ResourceMinMax = new Vector2(8, 20);
            InvaderType = InvaderType.FlameJetTrooper;
            //InAir = false;
            //Airborne = false;
            //CanAttack = true;

            InvaderAnimationState = AnimationState_Invader.Walk;
            CurrentMacroBehaviour = MacroBehaviour.AttackTower;
            CurrentMicroBehaviour = MicroBehaviour.MovingForwards;

            AngleRange = new Vector2(25, 30);
            TowerDistanceRange = new Vector2(1500, 1500);
            TrapDistanceRange = new Vector2(250, 350);
            LaunchVelocityRange = new Vector2(10, 15);

            RangedAttackTiming = new RangedAttackTiming()
            {
                MaxFireDelay = 250,
                CurrentFireDelay = 0
            };

            RangedDamage = 10;

            CurrentAngle = 0;
            EndAngle = 0;

            FlameTime = 3500f;

            //if (TrapList != null && TrapList.Count > 0)
            //{
            //    //Find the closest trap to target
            //    TargetTrap = TrapList.OrderBy(Trap => Vector2.Distance(Position, Trap.Position)).FirstOrDefault(Trap => Trap.Solid == true);
            //}
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
                        if (TargetTrap != null)
                        {
                            TrapDirection = new Vector2(TargetTrap.Center.X, TargetTrap.Bottom) - Center;
                            TrapDirection.Normalize();

                            float Angle = (float)Math.Atan2(TrapDirection.Y, TrapDirection.X);// -(float)Math.PI / 2;

                            EmitterList.ForEach(Emitter =>
                                {
                                    if (Emitter.Tether == this && Emitter.TextureName == "FlameThrower")
                                    {
                                        Emitter.AngleRange = new Vector2(MathHelper.ToDegrees(-Angle), MathHelper.ToDegrees(-Angle));
                                    }
                                });
                        }

                        if (CurrentFlameTime < FlameTime)
                        {
                            CurrentFlameTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        }

                        //if (CurrentFlameTime >= FlameTime)
                        //{
                        //    JumpManBehaviour = SpecificBehaviour.None;
                        //    CurrentMicroBehaviour = MicroBehaviour.MovingForwards;
                        //    Gravity = 0.2f;
                        //    Airborne = false;
                        //    CurrentFlameTime = 0;

                        //    EmitterList.ForEach(Emitter =>
                        //    {
                        //        //Had to use the TextureName to make sure if an invader has another particle effect applied that
                        //        //I don't cancel that out too. e.g. If the invader is already on fire/poisoned and uses the flamethrower
                        //        //I don't want to also cancel out the particle effect that shows the fire/poison
                        //        if (Emitter.Tether == this && Emitter.TextureName == "FlameThrower")
                        //            Emitter.AddMore = false;
                        //    });
                        //}

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

                            if (TargetTrap == null)
                            {
                                if (TrapList != null && TrapList.Count > 0)
                                {
                                    //Find the closest trap to target
                                    TargetTrap = TrapList.OrderBy(Trap => Vector2.Distance(Position, Trap.Position)).FirstOrDefault(Trap => MaxY > Trap.CollisionBox.Min.Y && MaxY < Trap.CollisionBox.Max.Y && Trap.Solid == true);
                                }
                            }

                            if (TargetTrap != null)
                            {
                                TrapPosition = TargetTrap.BoundingBox.Min.X;
                                DistToTrap = Position.X - TrapPosition;
                            }

                            switch (CurrentMacroBehaviour)
                            {
                                #region Attack Tower
                                case MacroBehaviour.AttackTower:
                                    {
                                        if (InTowerRange == true)
                                        {
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
                                        UpdateFireDelay(gameTime);
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
