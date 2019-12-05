using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Soldier : Invader
    {
        //Done this way so that the speed can be reset if need be
        public override float OriginalSpeed { get { return 1.0f; } }

        public Soldier(Vector2 position, Vector2? yRange = null)
            : base(position, yRange)
        {
            MaxHP = 20;
            ResourceMinMax = new Vector2(8, 20);
            //YRange = new Vector2(700, 900);

            InvaderType = InvaderType.Soldier;

            InvaderAnimationState = AnimationState_Invader.Walk;
            CurrentMacroBehaviour = MacroBehaviour.AttackTower;
            CurrentMicroBehaviour = MicroBehaviour.MovingForwards;

            MeleeDamageStruct = new InvaderMeleeStruct()
            {
                CurrentAttackDelay = 0,
                MaxAttackDelay = 2000,
                Damage = 10
            };
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            //ResourceMinMax.X += 20;

            //Colliding with a trap or the tower. Stop moving
            if ((TrapCollision == true || TowerCollision == true) && 
                CurrentMicroBehaviour == MicroBehaviour.MovingForwards)
            {
                CurrentMicroBehaviour = MicroBehaviour.Stationary;
            }

            if (TargetTrap != null && TrapCollision == false)
            {
                TargetTrap = null;
            }

            #region Handle Orientation
            if (Direction.X < 0)
            {
                Orientation = SpriteEffects.None;
            }
            else
            {
                Orientation = SpriteEffects.FlipHorizontally;
            }
            #endregion

            if (CurrentBehaviourDelay > MaxBehaviourDelay)
            switch (CurrentMacroBehaviour)
            {
                //Operating vehicle behaviour
                case MacroBehaviour.OperateVehicle:
                    {
                        //Vehicle was destroyed. Revert back to regular behaviour
                        if (OperatingVehicle == null || OperatingVehicle.Active == false || OperatingVehicle.CurrentHP == 0)
                        {
                            CurrentMacroBehaviour = MacroBehaviour.AttackTower;
                            CurrentMicroBehaviour = MicroBehaviour.Stationary;
                            OperatingVehicle = null;
                            Speed = OriginalSpeed * (float)Game1.RandomDouble(0.75f, 1.5f);
                            CurrentBehaviourDelay = 0;
                        }

                        switch (CurrentMicroBehaviour)
                        {
                            #region MovingForwards
                            case MicroBehaviour.MovingForwards:
                                {
                                    Direction.X = -1;

                                    if (InAir == false)
                                    {
                                        if (Slow == true)
                                            Velocity.X = Direction.X * SlowSpeed;
                                        else
                                            Velocity.X = Direction.X * Speed;
                                    }

                                    if (OperatingVehicle != null &&
                                        (OperatingVehicle.CurrentMicroBehaviour == MicroBehaviour.Stationary ||
                                         OperatingVehicle.CurrentMicroBehaviour == MicroBehaviour.Attack))
                                    {
                                        CurrentMicroBehaviour = MicroBehaviour.Stationary;
                                    }
                                }
                                break; 
                            #endregion
                                
                            #region Stationary
                            case MicroBehaviour.Stationary:
                                {
                                    Velocity.X = 0;

                                    //This soldier hit a trap but the vehicle didn't. Stop the vehicle.
                                    if (TargetTrap != null &&
                                        TrapCollision == true &&
                                        OperatingVehicle != null && 
                                        OperatingVehicle.CurrentMicroBehaviour == MicroBehaviour.MovingForwards)
                                    {
                                        CurrentMicroBehaviour = MicroBehaviour.Attack;
                                        OperatingVehicle.CurrentMicroBehaviour = MicroBehaviour.Stationary;
                                        OperatingVehicle.CurrentMacroBehaviour = MacroBehaviour.IsOperated;
                                    }
                                }
                                break; 
                            #endregion

                            #region Attack
                            case MicroBehaviour.Attack:
                                {
                                    UpdateMeleeDelay(gameTime);

                                    if (OperatingVehicle != null && TargetTrap == null && TrapCollision == false)
                                    {
                                        CurrentMicroBehaviour = MicroBehaviour.MovingForwards;
                                        OperatingVehicle.CurrentMacroBehaviour = MacroBehaviour.IsOperated;
                                        OperatingVehicle.CurrentMicroBehaviour = MicroBehaviour.MovingForwards;
                                    }
                                }
                                break;
                            #endregion
                        }
                    }
                    break;
                
                //Regular behaviour
                case MacroBehaviour.AttackTower:
                case MacroBehaviour.AttackTraps:
                    {
                        if (TrapCollision == false &&
                            TowerCollision == false &&
                            CurrentMicroBehaviour != MicroBehaviour.MovingForwards &&
                            CurrentMicroBehaviour != MicroBehaviour.MovingBackwards &&
                            CurrentMicroBehaviour != MicroBehaviour.FollowWaypoints)
                        {
                            CurrentMicroBehaviour = MicroBehaviour.MovingForwards;
                        }

                        switch (CurrentMicroBehaviour)
                        {
                            #region Stationary
                            case MicroBehaviour.Stationary:
                                {
                                    Velocity.X = 0;
                                    CurrentMicroBehaviour = MicroBehaviour.Attack;
                                }
                                break;
                            #endregion

                            #region MovingForwards
                            case MicroBehaviour.MovingForwards:
                                {
                                    Direction.X = -1;

                                    if (Slow == true)
                                        Velocity.X = Direction.X * SlowSpeed;
                                    else
                                        Velocity.X = Direction.X * Speed;
                                }
                                break;
                            #endregion

                            #region MovingBackwards
                            case MicroBehaviour.MovingBackwards:
                                {
                                    Direction.X = 1;

                                    if (Slow == true)
                                        Velocity.X = Direction.X * SlowSpeed;
                                    else
                                        Velocity.X = Direction.X * Speed;
                                }
                                break;
                            #endregion

                            #region Attack
                            case MicroBehaviour.Attack:
                                {
                                    Velocity.X = 0;

                                    switch (CurrentMacroBehaviour)
                                    {
                                        #region Attack Tower
                                        case MacroBehaviour.AttackTower:
                                            {
                                                UpdateMeleeDelay(gameTime);
                                            }
                                            break;
                                        #endregion

                                        #region Attack Traps
                                        case MacroBehaviour.AttackTraps:
                                            {
                                                UpdateMeleeDelay(gameTime);
                                            }
                                            break;
                                        #endregion
                                    }
                                }
                                break;
                            #endregion

                            #region FollowWaypoints
                            case MicroBehaviour.FollowWaypoints:
                                {
                                    #region The vehicle was destroyed. Revert to regular behaviour
                                    if ((OperatingVehicle == null || 
                                         OperatingVehicle.Active == false || 
                                         OperatingVehicle.CurrentHP == 0) 
                                        && Waypoints.Count > 0)
                                    {
                                        OperatingVehicle = null;
                                        Pathfinder.TrapList.CollectionChanged -= Pathfinder.TrapsChanged;
                                        Pathfinder = null;
                                        Waypoints.Clear();
                                        CurrentMacroBehaviour = MacroBehaviour.AttackTower;
                                        CurrentMicroBehaviour = MicroBehaviour.MovingForwards;
                                    }
                                    #endregion

                                    #region Get waypoints
                                    if (Pathfinder != null && Waypoints.Count == 0)
                                    {
                                        Waypoints = Pathfinder.GetWaypoints();
                                    }
                                    #endregion

                                    #region There are still waypoints to be followed
                                    if (CurrentWaypoint < Waypoints.Count)
                                    {
                                        Direction = Waypoints[CurrentWaypoint] - (ShadowPosition + new Vector2(CurrentAnimation.FrameSize.X / 2, 4));
                                        Direction.Normalize();

                                        if (Frozen == false)
                                            Velocity = Direction * Speed;

                                        if (Vector2.Distance(ShadowPosition + new Vector2(CurrentAnimation.FrameSize.X / 2, 4), Waypoints[CurrentWaypoint]) < 2)
                                        {
                                            CurrentWaypoint++;
                                            //Velocity = new Vector2(0, 0);
                                        }
                                    }
                                    #endregion
                                    else if (Waypoints.Count > 0)
                                    #region Reached last waypoint. Clear waypoints. Face the tower. Start operating vehicle.
                                    {
                                        Direction.X = -1f;
                                        CurrentWaypoint = 0;
                                        Waypoints.Clear();
                                        Pathfinder.TrapList.CollectionChanged -= Pathfinder.TrapsChanged;
                                        Pathfinder = null;

                                        CurrentMicroBehaviour = MicroBehaviour.Stationary;
                                        CurrentMacroBehaviour = MacroBehaviour.OperateVehicle;
                                    }
                                    #endregion

                                    MaxY = DestinationRectangle.Bottom;
                                }
                                break;
                            #endregion
                        }
                    }
                    break;
            }
            
            
            base.Update(gameTime, cursorPosition);
        }
    }
}
