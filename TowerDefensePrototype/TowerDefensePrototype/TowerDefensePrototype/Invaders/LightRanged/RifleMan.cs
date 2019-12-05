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
    class RifleMan : LightRangedInvader
    {
        public override float OriginalSpeed { get { return 0.68f; } }

        public RifleMan(Vector2 position, Vector2? yRange = null)
            : base(position, yRange)
        {
            MaxHP = 20;
            ResourceMinMax = new Vector2(8, 20);
            InvaderType = InvaderType.RifleMan;        
            InvaderAnimationState = AnimationState_Invader.Walk;
            
            AngleRange = new Vector2(170, 190);
            RangedDamage = 10;
            MaxFireDelay = 250;
            CurrentFireDelay = 0;
            TowerDistanceRange = new Vector2(600, 800);
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            if (CurrentBehaviourDelay > MaxBehaviourDelay)
                switch (CurrentMicroBehaviour)
                {
                    #region Stationary
                    case MicroBehaviour.Stationary:
                        {
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
                                #region AttackTower
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

                                #region AttackTraps
                                case MacroBehaviour.AttackTraps:
                                    {

                                    }
                                    break; 
                                #endregion

                                #region AttackTurrets
                                case MacroBehaviour.AttackTurrets:
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

                        }
                        break; 
                    #endregion

                    #region AdjustTrajectory
                    case MicroBehaviour.AdjustTrajectory:
                        {
                            Velocity.X = 0;
                            CanAttack = false;
                            CurrentAngle = EndAngle;
                            CurrentMicroBehaviour = MicroBehaviour.Attack;
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
                                                #region Only hit shield
                                                if (HitShield == 10)
                                                {
                                                    CanAttack = true;
                                                    ResetCollisions();
                                                    break;
                                                }
                                                #endregion
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

                                #region Attack Turrets
                                case MacroBehaviour.AttackTurrets:
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
