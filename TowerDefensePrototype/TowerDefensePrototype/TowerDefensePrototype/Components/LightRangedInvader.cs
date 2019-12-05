using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    abstract class LightRangedInvader : Invader
    {
        private object _HitObject;
        public new object HitObject
        {
            get { return _HitObject; }
            set
            {
                PreviousHitObject = _HitObject;
                _HitObject = value;

                TotalHits++;

                if (_HitObject != null)
                {
                    #region Hit the ground
                    if (_HitObject.GetType() == typeof(StaticSprite))
                    {
                        HitGround++;
                        return;
                    }
                    #endregion

                    #region Hit the shield
                    if (_HitObject.GetType() == typeof(Shield))
                    {
                        HitShield++;
                        return;
                    }
                    #endregion

                    #region Hit the tower
                    if (_HitObject.GetType() == typeof(Tower))
                    {
                        HitTower++;
                        return;
                    }
                    #endregion

                    #region Hit a turret
                    if (_HitObject.GetType().BaseType == typeof(Turret))
                    {
                        HitTurret++;
                        return;
                    }
                    #endregion

                    #region Hit a trap
                    if (_HitObject.GetType().BaseType == typeof(Trap))
                    {
                        HitTrap++;
                        return;
                    }
                    #endregion
                }
            }
        }

        public LightRangedInvader(Vector2 position, Vector2? yRange = null)
            : base(position, yRange)
        {

        }

        #region For handling ranged attacking
        //public InvaderFireType FireType; //Whether the invader fires a single projectile, fires a burst or fires a beam etc.
        public Vector2 TowerDistanceRange; //How far away from the tower the invader will be before stopping to fire
        public Vector2 AngleRange; //The angle that the projectile is fired at.

        public bool InTowerRange = false;
        public bool InTrapRange = false;
        public float DistToTower = 1920;
        //public float DistToTrap;//, TrapPosition;
        public float MinTowerRange;//, MinTrapRange;

        public float RangedDamage; //How much damage the projectile does
        public float CurrentFireDelay, MaxFireDelay; //How many milliseconds between shots
        //public int CurrentBurstShots;//, MaxBurstShots; //How many shots are fired in a row before a longer recharge is needed

        public float CurrentAngle = 0;
        public float EndAngle;

        public int TotalHits = 0;

        public int HitGround = 0;
        public int HitTower = 0;
        public int HitShield = 0;
        public int HitTurret = 0;
        public int HitTrap = 0;
        #endregion

        public override void Initialize()
        {
            MinTowerRange = Random.Next((int)TowerDistanceRange.X, 
                                        (int)TowerDistanceRange.Y);

            base.Initialize();
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            //if (DistToTower <= MinTowerRange)
            //{
            //    Velocity.X = 0;
            //    InTowerRange = true;
            //}
            DistToTower = Position.X - Tower.DestinationRectangle.Right;

            base.Update(gameTime, cursorPosition);
        }

        public void UpdateFireDelay(GameTime gameTime)
        {
            //if (RangedDamageStruct != null)
            {
                CurrentFireDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (CurrentFireDelay >= MaxFireDelay)
                {
                    CanAttack = true;
                    CurrentFireDelay = 0;
                }
                else
                {
                    CanAttack = false;
                }
            }
        }

        public void ResetCollisions()
        {
            TotalHits = 0;

            HitGround = 0;
            HitTower = 0;
            HitShield = 0;
            HitTurret = 0;
            HitTrap = 0;
        }
    }
}
