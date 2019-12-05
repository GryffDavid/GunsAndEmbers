using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class HarpoonCannon : HeavyRangedInvader
    {
        enum SpecificBehaviour { Attached, PullTaut, Retract, Unattached };
        SpecificBehaviour HarpoonCannonBehaviour = SpecificBehaviour.Unattached;
        float RopeDelay, MaxRopeDelay;
        public Rope Rope;
        public Turret HarpoonedTurret; //The turret that has been harpooned
        

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

            AngleRange = new Vector2(25, 30);
            TowerDistanceRange = new Vector2(750, 850);
            TrapDistanceRange = new Vector2(250, 350);
            LaunchVelocityRange = new Vector2(25, 30);

            RangedAttackTiming = new RangedAttackTiming()
            {
                MaxFireDelay = 6500,
                CurrentFireDelay = 0
            };

            //MaxFireDelay = 6500;
            //CurrentFireDelay = 0;
            RangedDamage = 10;

            CurrentAngle = 0;
            EndAngle = 0;

            MaxRopeDelay = 100f;
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

                            //The invader has pulled down a turret. Move forwards again for another go
                            if (Rope == null || Rope.Sticks.Count == 0)
                            {
                                CurrentMicroBehaviour = MicroBehaviour.MovingForwards;
                            }
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

                            switch (CurrentMacroBehaviour)
                            {
                                #region AttackTower
                                case MacroBehaviour.AttackTower:
                                    {
                                        //If the invader is in range, get an angle and start adjusting the barrel
                                        if (DistToTower <= MinTowerRange)
                                        {
                                            CurrentMicroBehaviour = MicroBehaviour.Stationary;

                                            if (InTowerRange == false)
                                            {
                                                EndAngle = GetNextAngle();
                                            }

                                            InTowerRange = true;
                                            CurrentMicroBehaviour = MicroBehaviour.AdjustTrajectory;
                                        }
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
                            //This is done after the harpoon is attached. Start pulling backwards
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
                            Velocity.X = 0;
                            CanAttack = false;

                            if (CurrentAngle != EndAngle)
                            {
                                if (Math.Abs(EndAngle - CurrentAngle) > MathHelper.ToRadians(0.25f))
                                {
                                    CurrentAngle = MathHelper.Lerp(CurrentAngle, EndAngle, 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                                }
                                else
                                {
                                    EndAngle = CurrentAngle;
                                    CurrentMicroBehaviour = MicroBehaviour.Attack;
                                }
                            }
                            else
                            {
                                EndAngle = CurrentAngle;
                                CurrentMicroBehaviour = MicroBehaviour.Attack;
                            }
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
                                        //In range, but the harpoon is currently not attached to anything
                                        if (InTowerRange == true &&
                                            HarpoonCannonBehaviour == SpecificBehaviour.Unattached)
                                        {
                                            UpdateFireDelay(gameTime);

                                            //There was a collision of some sort
                                            if (TotalHits > 0)
                                            {
                                                //Hit a turret! The rope is attached to the turret now. Retract the rope until taut.
                                                if (HitTurret >= 1)
                                                {
                                                    HarpoonCannonBehaviour = SpecificBehaviour.Attached;
                                                    ResetCollisions();
                                                }

                                                //Hit the edge of the screen. Do nothing. Retract the rope.
                                                if (HitScreen >= 1)
                                                {
                                                    HarpoonCannonBehaviour = SpecificBehaviour.Retract;
                                                    ResetCollisions();
                                                }

                                                //Hit the tower (Technically impossible because of collision check code for harpoon projectiles
                                                //Do nothing. Retract the rope
                                                if (HitTower >= 1)
                                                {
                                                    HarpoonCannonBehaviour = SpecificBehaviour.Retract;
                                                    ResetCollisions();
                                                }
                                            }
                                        }
                                    }
                                    break; 
                                #endregion
                            }
                        }
                        break;
                    #endregion
                }


            #region Handle Rope Behaviour
            switch (HarpoonCannonBehaviour)
            {
                #region Unattached
                case SpecificBehaviour.Unattached:
                    {

                    }
                    break;
                #endregion

                #region Attached
                case SpecificBehaviour.Attached:
                    {
                        RopeDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                        if (Rope.Sticks.Count > 50 &&
                            RopeDelay > MaxRopeDelay &&
                            HarpoonedTurret.CurrentHealth > 0)
                        {
                            Rope.Segments = Rope.Sticks.Count;
                            Rope.Sticks.RemoveAt(Rope.Sticks.Count - 1);
                            Rope.StartPoint = BarrelEnd;
                            RopeDelay = 0;
                        }

                        //The turret was destroyed! Retract the rope and move forwards again
                        if (HarpoonedTurret.CurrentHealth == 0)
                        {
                            Rope.Sticks.RemoveAt(0);
                            HarpoonCannonBehaviour = SpecificBehaviour.Retract;
                            break;
                        }

                        //The rope has been retracted enough to be taut. Move backwards to pull the turret down
                        if (Rope.Sticks.Count == 50 && Rope.Sticks[0] != null)
                        {
                            HarpoonCannonBehaviour = SpecificBehaviour.PullTaut;
                            break;
                        }
                    }
                    break; 
                #endregion

                #region PullTaut
                case SpecificBehaviour.PullTaut:
                    {
                        //The rope is shorter now. Move backwards to pull the turret down
                        Rope.StartPoint = BarrelEnd;
                        CurrentMicroBehaviour = MicroBehaviour.MovingBackwards;

                        //Moved back 300 pixels. Destroy the turret and detach the rope
                        if (DistToTower > MinTowerRange + 300)
                        {
                            if (Rope.Sticks.Count == 50 && Rope.Sticks[0] != null)
                            {
                                Rope.Sticks.RemoveAt(0);
                                HarpoonedTurret.CurrentHealth = 0;
                                CurrentMicroBehaviour = MicroBehaviour.Stationary;
                                HarpoonCannonBehaviour = SpecificBehaviour.Retract;
                                break;
                            }
                        }

                        //The turret was destroyed before it could be pulled down by this invader. Detach the rope
                        if (HarpoonedTurret.CurrentHealth == 0)
                        {
                            Rope.Sticks.RemoveAt(0);
                            CurrentMicroBehaviour = MicroBehaviour.Stationary;
                            HarpoonCannonBehaviour = SpecificBehaviour.Retract;
                            break;
                        }
                    }
                    break;
                #endregion

                #region Retract
                case SpecificBehaviour.Retract:
                    {
                        if (Rope != null)
                        {
                            //Remove a rope segment every few milliseconds to retract the rope - make it shorter
                            RopeDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                                                        
                            if (Rope.Sticks.Count > 1 &&
                                RopeDelay > MaxRopeDelay)
                            {
                                Rope.Sticks.RemoveAt(0);

                                if (Rope.Segments > 0)
                                    Rope.Segments = Rope.Sticks.Count - 1;

                                Rope.Sticks.RemoveAt(Rope.Sticks.Count - 1);
                                Rope.StartPoint = BarrelEnd;
                                RopeDelay = 0;
                            }

                            if (Rope.Sticks.Count == 1)
                            {
                                Rope = null;
                                HarpoonCannonBehaviour = SpecificBehaviour.Unattached;
                            }
                        }
                    }
                    break;
                #endregion
            } 
            #endregion
                

            BarrelPivot = new Vector2(BarrelAnimation.FrameSize.X / 2, BarrelAnimation.FrameSize.Y / 2);
            BasePivot = new Vector2(Position.X, Position.Y);

            base.Update(gameTime, cursorPosition);
        }
    }
}
