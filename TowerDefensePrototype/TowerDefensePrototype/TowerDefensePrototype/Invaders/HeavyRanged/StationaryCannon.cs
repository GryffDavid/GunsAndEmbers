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
        int AttackTowerLoopCounter; //How many times the invader made the decision to 
                                    //try shoot the tower instead of the trap after it ran into a trap

        int BounceLoopCounter; //How many time the invader has bounced between two solid traps

        Trap RecentTrapCollision;

        //Should keep track of previous operations. i.e. moving down meant the hit rate went up by a bit, keep moving down then.
        //if moving up reduced the hit rate, move up etc. Keeping track of previous hits vs. previous operations means
        //learning over time
        //
        //Compare most recent result with previous result. If the number of hits goes up by moving down, keep moving down
        //if the number of hits goes down when moving up, move down etc.

        //Moving forward 100 pixels to approach the tower more than 4 times without bumping into a trap
        //or hitting the tower/shield with a projectile means that the movement range should be updated
        //to moving more than 100 pixels

        //Gets stuck in loop of hitting ground. Too far up and too close to hit the trap. Keeps firing over because the trajectory never
        //gets low enough. Reduce the launch velocity then.

        //What happens when the player removes the trap that is currently targetted? The invader needs to react to that too

        public StationaryCannon(Vector2 position)
        {
            Position = position;
            Speed = 1.5f;
            MaxHP = 200;
            ResourceMinMax = new Vector2(8, 20);
            YRange = new Vector2(700, 900);
            IntelligenceRange = new Vector2(0.8f, 1.0f);

            InvaderType = InvaderType.StationaryCannon;

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
                        switch (CurrentMacroBehaviour)
                        {
                            #region Attack Tower
                            case MacroBehaviour.AttackTower:
                                {
                                    #region Hit a trap while moving forwards
                                    if (InTowerRange == false && TargetTrap != null && Velocity.X < 0)
                                    {
                                        TrapPosition = TargetTrap.BoundingBox.Min.X;
                                        Position.X += (Math.Abs(TargetTrap.BoundingBox.Max.X - BoundingBox.Min.X) + 1);

                                        //INTELLIGENCE DECISION REQUIRED HERE
                                        //It should affect the number of loop times
                                        //Chance to change behaviour 
                                        //and time to change

                                        #region Only adjust to AttackTraps if the invader is intelligent or stuck in a loop
                                        if (Random.NextDouble() > 0.1 || AttackTowerLoopCounter > 3)
                                        {
                                            MinTrapRange = Random.Next((int)TrapDistanceRange.X,
                                                                       (int)TrapDistanceRange.Y);
                                            CurrentMacroBehaviour = MacroBehaviour.AttackTraps;
                                            AttackTowerLoopCounter = 0;
                                        }
                                        else
                                        {
                                            MinTowerRange = Position.X + MinTrapRange;
                                            AttackTowerLoopCounter++;
                                        }
                                        #endregion

                                        CurrentMicroBehaviour = MicroBehaviour.MovingBackwards;
                                        CurrentBehaviourDelay = 0;                                        
                                    }
                                    #endregion

                                    #region Hit a trap while moving backwards
                                    if (InTowerRange == false && TargetTrap != null && Velocity.X > 0)
                                    {
                                        TrapPosition = TargetTrap.BoundingBox.Min.X;
                                        Position.X -= (Math.Abs(TargetTrap.BoundingBox.Min.X - BoundingBox.Max.X) + 1);

                                        CurrentMicroBehaviour = MicroBehaviour.MovingForwards;
                                    }
                                    #endregion
                                }
                                break;
                            #endregion

                            #region Attack Traps
                            case MacroBehaviour.AttackTraps:
                                {
                                    #region Hit a trap while moving backwards
                                    if (InTowerRange == false && TargetTrap != null && Velocity.X > 0)
                                    {
                                        TrapPosition = TargetTrap.BoundingBox.Min.X;
                                        Position.X -= (Math.Abs(TargetTrap.BoundingBox.Min.X - BoundingBox.Max.X) + 1);
                                        MinTowerRange = Random.Next((int)TowerDistanceRange.X, (int)TowerDistanceRange.Y);

                                        CurrentMicroBehaviour = MicroBehaviour.MovingForwards;
                                        BounceLoopCounter++;
                                    }
                                    #endregion

                                    #region Hit a trap while moving forwards - Bouncing back after reversing into a trap
                                    if (InTowerRange == false && TargetTrap != null && Velocity.X < 0)
                                    {
                                        TrapPosition = TargetTrap.BoundingBox.Min.X;
                                        Position.X += (Math.Abs(TargetTrap.BoundingBox.Max.X - BoundingBox.Min.X) + 1);

                                        //Definitely stuck between two traps. Attack the one behind it.
                                        //Might let other invaders in to help with the one in front

                                        //INTELLIGENCE DECISION REQUIRED HERE
                                        //It should affect the number of loop times
                                        if (BounceLoopCounter > 2)
                                        {
                                            InTrapRange = true;
                                            EndAngle = MathHelper.ToRadians(180);
                                            CurrentMicroBehaviour = MicroBehaviour.AdjustTrajectory;
                                            BounceLoopCounter = 0;
                                        }
                                        else
                                        {
                                            CurrentMicroBehaviour = MicroBehaviour.MovingBackwards;
                                        }
                                    }
                                    #endregion
                                }
                                break;
                            #endregion
                        }

                        CanAttack = false;
                        Velocity.X = 0;
                    }
                    break;
                #endregion

                #region MovingForwards
                case MicroBehaviour.MovingForwards:
                    {
                        #region Move
                        Direction.X = -1;
                        CanAttack = false;

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
                        CanAttack = false;

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
                                    if (MinTowerRange <= DistToTower)
                                    {
                                        //When the invader gets in range. It chooses the final firing angle
                                        if (InTowerRange == false)
                                        {
                                            EndAngle = MathHelper.ToRadians(Random.Next((int)AngleRange.X, (int)AngleRange.Y));
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
                                    DistToTrap = Position.X - TrapPosition;

                                    if (DistToTrap >= MinTrapRange)
                                    {
                                        InTrapRange = true;
                                        float nextAngle = Random.Next((int)10, (int)30);
                                        EndAngle = MathHelper.ToRadians(nextAngle);
                                        CurrentMicroBehaviour = MicroBehaviour.AdjustTrajectory;
                                        CurrentBehaviourDelay = 0;
                                        Velocity.X = 0;
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
                                    if (InTowerRange == true)
                                    {
                                        UpdateFireDelay(gameTime);

                                        if (TotalHits >= 10)
                                        {
                                            #region Hit traps, but not significant enough to warrant a Macro change
                                            if (HitTrap > 0 && HitTrap < HitShield)
                                            {
                                                CanAttack = false;
                                                EndAngle = MathHelper.Clamp(CurrentAngle - MathHelper.ToRadians(Random.Next(5, 15)), 0, 15);
                                                CurrentMicroBehaviour = MicroBehaviour.AdjustTrajectory;
                                                ResetCollisions();
                                                break;
                                            }
                                            #endregion

                                            #region Hit the ground too much, move closer
                                            if (HitGround > HitShield)
                                            {
                                                if (MathHelper.ToDegrees(CurrentAngle) <= 10)
                                                {
                                                    EndAngle = MathHelper.Clamp(CurrentAngle + MathHelper.ToRadians(Random.Next(5, 15)), 0, 15);
                                                    CurrentMicroBehaviour = MicroBehaviour.AdjustTrajectory;
                                                    ResetCollisions();
                                                }
                                                else
                                                {
                                                    MinTowerRange -= 100;
                                                    InTowerRange = false;
                                                    CanAttack = false;
                                                    EndAngle = 0;
                                                    CurrentMicroBehaviour = MicroBehaviour.AdjustTrajectory;
                                                    ResetCollisions();
                                                }
                                                break;
                                            }
                                            #endregion

                                            #region Hit traps a lot, change Macro to AttackTraps OR adjust trajectory up to shoot over the trap
                                            if (HitTrap > HitShield)
                                            {
                                                //Could also check the height of the Top of the trap above the invader. 
                                                //Too high up means it should back up and attack the trap. 
                                                //Low enough down means it should just back up and ajust its trajectory

                                                //INTELLIGENCE DECISION REQUIRED HERE
                                                //It should affect the chance to change behaviour
                                                //as well as the time it takes to change
                                                if (Random.NextDouble() > 0.5)
                                                {
                                                    CanAttack = false;
                                                    InTrapRange = false;
                                                    //MinTowerRange = Position.X + (MinTrapRange / 2);
                                                    EndAngle = MathHelper.Clamp(CurrentAngle - MathHelper.ToRadians(Random.Next(5, 15)), 0, 15);
                                                    CurrentMacroBehaviour = MacroBehaviour.AttackTraps;
                                                    CurrentMicroBehaviour = MicroBehaviour.AdjustTrajectory;
                                                    ResetCollisions();
                                                    break;
                                                }
                                                else
                                                {
                                                    CanAttack = false;
                                                    //MinTowerRange = Position.X + (MinTrapRange / 4);
                                                    EndAngle = MathHelper.Clamp(CurrentAngle + MathHelper.ToRadians(Random.Next(5, 15)), 0, 15);
                                                    CurrentMicroBehaviour = MicroBehaviour.AdjustTrajectory;
                                                    ResetCollisions();
                                                    break;
                                                }
                                            }
                                            #endregion
                                        }
                                    }
                                    else
                                    #region Not in range, but trying to attack. Move forwards
                                    {
                                        if (CurrentAngle == EndAngle)
                                            CurrentMicroBehaviour = MicroBehaviour.MovingForwards;
                                        else
                                            CurrentMicroBehaviour = MicroBehaviour.AdjustTrajectory;
                                    }
                                    #endregion
                                }
                                break;
                            #endregion

                            #region Attack Traps
                            case MacroBehaviour.AttackTraps:
                                {
                                    if (InTrapRange == true)
                                    {
                                        UpdateFireDelay(gameTime);

                                        #region If the target trap is not the closest, adjust accordingly
                                        /*Need to check if the most recently hit trap is the one that was bumped into
                                          If the most recently hit trap is further away than the one that was bumped into
                                          Move the trajectory down. If it's closer, change the target trap
                                          i.e. always target the closest trap because that's the one that's a problem.*/

                                        if (HitObject != TargetTrap)
                                        {
                                            RecentTrapCollision = HitObject as Trap;

                                            if (RecentTrapCollision != null && TargetTrap != null)
                                            {
                                                if (RecentTrapCollision.Position.X > TargetTrap.Position.X)
                                                {
                                                    TargetTrap = RecentTrapCollision;
                                                }

                                                if (RecentTrapCollision.Position.X < TargetTrap.Position.X)
                                                {
                                                    if (TargetTrap.CurrentHP <= 0)
                                                    {
                                                        TargetTrap = RecentTrapCollision;
                                                    }

                                                    HitObject = TargetTrap;
                                                    EndAngle = MathHelper.Clamp(CurrentAngle - MathHelper.ToRadians(Random.Next(5, 15)), 0, 15);
                                                    CurrentMicroBehaviour = MicroBehaviour.AdjustTrajectory;
                                                }
                                            }
                                        }
                                        #endregion

                                        if (TotalHits >= 10)
                                        {
                                            #region Trap was destroyed, attack tower again
                                            if (TargetTrap != null)
                                                if (TargetTrap.CurrentHP <= 0)
                                                {
                                                    EndAngle = 0;
                                                    TargetTrap = null;
                                                    HitObject = null;
                                                    InTrapRange = false;
                                                    CurrentMicroBehaviour = MicroBehaviour.AdjustTrajectory;
                                                    CurrentMacroBehaviour = MacroBehaviour.AttackTower;
                                                    ResetCollisions();
                                                    break;
                                                }
                                            #endregion

                                            #region Hit the shield, must be quite close to tower. Attack tower instead
                                            if (HitShield > 0)
                                            {
                                                MinTowerRange = DistToTower;
                                                InTowerRange = true;
                                                EndAngle = CurrentAngle + MathHelper.ToRadians(Random.Next(5, 15));
                                                CurrentMicroBehaviour = MicroBehaviour.AdjustTrajectory;
                                                CurrentMacroBehaviour = MacroBehaviour.AttackTower;
                                                ResetCollisions();
                                                CurrentBehaviourDelay = 0;
                                                break;
                                            }
                                            #endregion

                                            //If this trajectory is already very low it means the invader is too far away
                                            //either adjust up or move closer
                                            #region Hit the ground, but not a trap. Change trajectory.
                                            if (HitTrap == 0 || HitGround > HitTrap)
                                            {
                                                if (CurrentAngle < MathHelper.ToRadians(15))
                                                {
                                                    //INTELLIGENCE Cowardice/Intelligenc roll here to decide whether to adjust up or move closer
                                                    EndAngle = MathHelper.Clamp(CurrentAngle + MathHelper.ToRadians(Random.Next(5, 15)), 0, 15);
                                                }
                                                else
                                                {
                                                    EndAngle = MathHelper.Clamp(CurrentAngle - MathHelper.ToRadians(Random.Next(5, 15)), 0, 15);

                                                }

                                                CurrentMicroBehaviour = MicroBehaviour.AdjustTrajectory;
                                                ResetCollisions();
                                                break;
                                            }
                                            #endregion
                                        }
                                    }
                                    else
                                    {
                                        CurrentAngle = 0;
                                        CurrentMicroBehaviour = MicroBehaviour.AdjustTrajectory;
                                    }
                                }
                                break;
                            #endregion
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
