using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class StationaryCannon : HeavyRangedInvader
    {
        public StationaryCannon(Vector2 position)
        {
            Position = position;
            Speed = 1.5f;
            MaxHP = 200;
            ResourceMinMax = new Vector2(8, 20);
            YRange = new Vector2(700, 900);

            InvaderType = InvaderType.StationaryCannon;

            InvaderAnimationState = AnimationState_Invader.Stand;
            CurrentMacroBehaviour = MacroBehaviour.AttackTower;
            CurrentMicroBehaviour = MicroBehaviour.MovingForwards;

            AngleRange = new Vector2(30, 60);
            DistanceRange = new Vector2(750, 850);
            LaunchVelocityRange = new Vector2(10, 15);
            MaxFireDelay = 1500;
            CurrentFireDelay = 0;
            RangedDamage = 10;

            CurrentAngle = 0;
            EndAngle = 0;
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            //Should possibly consider tracking a variable called something like NumLoops
            //To make sure that invaders doesn't get stuck in an infinite loop of getting stuck on a trap
            //then backing up, firing again, moving forward getting stuck on the trap etc. After say NumLoops = 6
            //change the Macro behaviour to something else - try a different tactic

            //Check how many times the CurrentMicroBehaviour changes without switching to Attack
            //This will let the invader know it's stuck in a loop and needs to change MacroBehaviour
            switch (CurrentMicroBehaviour)
            {
                #region Stationary
                case MicroBehaviour.Stationary:
                    {
                        switch (CurrentMacroBehaviour)
                        {
                            #region Attack Tower
                            case MacroBehaviour.AttackTower:
                                {
                                    if (InTowerRange == true && EndAngle != CurrentAngle)
                                    {
                                        CurrentMicroBehaviour = MicroBehaviour.AdjustTrajectory;
                                    }

                                    if (InTowerRange == true && EndAngle == CurrentAngle)
                                    {
                                        Speed = 0.75f;
                                        CurrentMicroBehaviour = MicroBehaviour.Attack;
                                    }

                                    if (InTowerRange == false && Velocity.X < 0)
                                    {
                                        Position.X += 3;
                                        TrapPosition = Position.X;
                                        MinTrapRange = 100;
                                        CurrentMacroBehaviour = MacroBehaviour.AttackTraps;
                                        CurrentMicroBehaviour = MicroBehaviour.MovingBackwards;
                                    }

                                    if (InTowerRange == false && Velocity.X > 0)
                                    {
                                        Position.X -= 3;
                                        CurrentMicroBehaviour = MicroBehaviour.MovingForwards;
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

                        Velocity.X = 0;
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

                #region MovingBackwards
                case MicroBehaviour.MovingBackwards:
                    {
                        Direction.X = 1;

                        if (Slow == true)
                            Velocity.X = Direction.X * SlowSpeed;
                        else
                            Velocity.X = Direction.X * Speed;

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
                                    DistToTrap = Position.X - TrapPosition;

                                    if (DistToTrap >= MinTrapRange)
                                    {
                                        InTrapRange = true;
                                        CurrentMicroBehaviour = MicroBehaviour.Stationary;
                                    }
                                }
                                break;
                            #endregion
                        }
                    }
                    break;
                #endregion

                #region AdjustTrajectory
                case MicroBehaviour.AdjustTrajectory:
                    {
                        Velocity.X = 0;

                        if (CurrentAngle != EndAngle)
                        {
                            if (Math.Abs(EndAngle - CurrentAngle) > MathHelper.ToRadians(0.25f))
                            {
                                CurrentAngle = MathHelper.Lerp(CurrentAngle, EndAngle, 0.02f * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
                            }
                            else
                            {
                                EndAngle = CurrentAngle;
                                CurrentMicroBehaviour = MicroBehaviour.Stationary;
                            }
                        }
                    }
                    break;
                #endregion

                #region Attack
                case MicroBehaviour.Attack:
                    {
                        if (InTowerRange == true)
                        {
                            UpdateFireDelay(gameTime);

                            //Check what the most recently fired projectiles hit and 
                            //adjust accordingly based on MacroBehaviour

                            #region Check most recently hit objects
                            if (HitObject != null)
                            {
                                TotalHits++;

                                //Hit the ground
                                if (HitObject.GetType() == typeof(StaticSprite))
                                {
                                    HitGround++;
                                }

                                //Hit the shield
                                if (HitObject.GetType() == typeof(Shield))
                                {
                                    HitShield++;
                                }

                                //Hit the tower
                                if (HitObject.GetType() == typeof(Tower))
                                {
                                    HitTower++;
                                }

                                //Hit a turret
                                if (HitObject.GetType().BaseType == typeof(Turret))
                                {
                                    HitTurret++;
                                }

                                //Hit a trap
                                if (HitObject.GetType().BaseType == typeof(Trap))
                                {
                                    HitTrap++;
                                }

                                HitObject = null;
                            }
                            #endregion

                            if (TotalHits >= 10)
                            {
                                switch (CurrentMacroBehaviour)
                                {
                                    #region Attack TOWER
                                    case MacroBehaviour.AttackTower:
                                        {
                                            if (HitGround > HitShield)
                                            {
                                                MinTowerRange -= 100;
                                                InTowerRange = false;
                                                CanAttack = false;
                                                HitObject = null;
                                                EndAngle = 0;
                                                CurrentMicroBehaviour = MicroBehaviour.AdjustTrajectory;
                                            }
                                        }
                                        break;
                                    #endregion

                                    #region Attack TRAPS
                                    case MacroBehaviour.AttackTraps:
                                        {

                                        }
                                        break;
                                    #endregion

                                    #region Attack TURRETS
                                    case MacroBehaviour.AttackTurrets:
                                        {

                                        }
                                        break;
                                    #endregion
                                }

                                TotalHits = 0;

                                HitGround = 0;
                                HitTower = 0;
                                HitShield = 0;
                                HitTurret = 0;
                                HitTrap = 0;
                            }
                        }
                    }
                    break;
                #endregion
            }

            BarrelPivot = new Vector2(BarrelAnimation.FrameSize.X / 2, BarrelAnimation.FrameSize.Y / 2);
            BasePivot = new Vector2(Position.X, Position.Y);

            base.Update(gameTime, cursorPosition);
        }
    }
}
