using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class GunShip : HeavyRangedInvader
    {
        //This could have its gun turret mounted UNDERNEATH the main body of the ship instead of on top
        public GunShip(Vector2 position, Vector2? yRange = null)
            : base(position, yRange)
        {
            MaxHP = 300;
            ResourceMinMax = new Vector2(1, 5);
            IntelligenceRange = new Vector2(0f, 1.0f);

            InvaderType = InvaderType.GunShip;

            InvaderAnimationState = AnimationState_Invader.Stand;
            CurrentMacroBehaviour = MacroBehaviour.AttackTower;
            CurrentMicroBehaviour = MicroBehaviour.MovingForwards;

            TowerDistanceRange = new Vector2(450, 580);

            //MaxFireDelay = 1500;
            //CurrentFireDelay = 0;
            RangedDamage = 80f;

            ZDepth = 64;

            FireType = InvaderFireType.Burst;
            RangedAttackTiming = new RangedAttackTiming()
            {
                CurrentBurstNum = 0,
                CurrentBurstTime = 0,
                MaxBurstNum = 5,
                MaxBurstTime = 2000,
                MaxFireDelay = 300,
                CurrentFireDelay = 0
            };

            YRange = new Vector2(60, 150);
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            switch (CurrentMicroBehaviour)
            {
                #region Stationary
                case MicroBehaviour.Stationary:
                    {
                        CanAttack = false;
                        Velocity.X = 0;

                        switch (CurrentMacroBehaviour)
                        {
                            case MacroBehaviour.AttackTower:
                                {
                                    CurrentMicroBehaviour = MicroBehaviour.Attack;
                                }
                                break;
                        }
                        
                    }
                    break; 
                #endregion

                #region MovingForwards
                case MicroBehaviour.MovingForwards:
                    {
                        if (DistToTower <= MinTowerRange)
                        {
                            InTowerRange = true;
                            CurrentMicroBehaviour = MicroBehaviour.Stationary;
                        }
                    }
                    break; 
                #endregion

                #region MovingBackwards
                case MicroBehaviour.MovingBackwards:
                    {

                    }
                    break; 
                #endregion

                #region Adjust Trajectory
                case MicroBehaviour.AdjustTrajectory:
                    {

                    }
                    break; 
                #endregion

                #region Attack
                case MicroBehaviour.Attack:
                    {
                        if (InTowerRange == true)
                        {
                            UpdateFireDelay(gameTime);
                        }
                    }
                    break;
                #endregion
            }

            base.Update(gameTime, cursorPosition);
        }
    }
}
