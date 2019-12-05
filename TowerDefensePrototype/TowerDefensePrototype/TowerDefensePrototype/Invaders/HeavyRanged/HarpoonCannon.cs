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
                                #region Attack Tower
                                case MacroBehaviour.AttackTower:
                                    {
                                        if (DistToTower <= MinTowerRange)
                                        {
                                            //When the invader gets in range. It chooses the final firing angle
                                            if (InTowerRange == false)
                                            {
                                                float nextAngle = Random.Next((int)AngleRange.X, (int)AngleRange.Y);
                                                EndAngle = MathHelper.ToRadians(nextAngle);
                                                Speed = 0.75f;
                                            }

                                            InTowerRange = true;
                                            CurrentMicroBehaviour = MicroBehaviour.AdjustTrajectory;
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
                                        if (InTowerRange == true && HarpoonCannonBehaviour == SpecificBehaviour.Unattached)
                                        {
                                            UpdateFireDelay(gameTime);

                                            if (TotalHits > 0)
                                            {
                                                if (HitTurret == 1)
                                                {
                                                    HarpoonCannonBehaviour = SpecificBehaviour.Attached;                                                    
                                                    ResetCollisions();
                                                }

                                                if (HitScreen == 1)
                                                {
                                                    HarpoonCannonBehaviour = SpecificBehaviour.Retract;
                                                    ResetCollisions();
                                                }
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
                }


            switch (HarpoonCannonBehaviour)
            {
                case SpecificBehaviour.Unattached:
                    {

                    }
                    break;

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

                        if (HarpoonedTurret.CurrentHealth == 0)
                        {
                            Rope.Sticks.RemoveAt(0);
                            HarpoonCannonBehaviour = SpecificBehaviour.Retract;
                            break;
                        }

                        if (Rope.Sticks.Count == 50 && Rope.Sticks[0] != null)
                        {
                            HarpoonCannonBehaviour = SpecificBehaviour.PullTaut;
                            break;
                        }
                    }
                    break;

                case SpecificBehaviour.PullTaut:
                    {
                        Rope.StartPoint = BarrelEnd;
                        CurrentMicroBehaviour = MicroBehaviour.MovingBackwards;

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

                        if (HarpoonedTurret.CurrentHealth == 0)
                        {
                            Rope.Sticks.RemoveAt(0);
                            CurrentMicroBehaviour = MicroBehaviour.Stationary;
                            HarpoonCannonBehaviour = SpecificBehaviour.Retract;
                            break;
                        }
                    }
                    break;

                case SpecificBehaviour.Retract:
                    {
                        if (Rope != null)
                        {
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
            }
                

            BarrelPivot = new Vector2(BarrelAnimation.FrameSize.X / 2, BarrelAnimation.FrameSize.Y / 2);
            BasePivot = new Vector2(Position.X, Position.Y);

            base.Update(gameTime, cursorPosition);
        }
    }
}
