using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    abstract class HeavyRangedInvader : Invader
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

        public HeavyRangedInvader(Vector2 position, Vector2? yRange) : base(position, yRange)
        {

        }

        #region For drawing barrel animations
        public InvaderAnimation BarrelAnimation;
        public VertexPositionColorTexture[] barrelVertices = new VertexPositionColorTexture[4];
        public Rectangle BarrelDestinationRectangle;
        public Vector2 BarrelPivot, BasePivot, BarrelEnd;
        public int[] barrelIndices = new int[6];
        #endregion

        #region For handling ranged attacking
        public InvaderFireType FireType; //Whether the invader fires a single projectile, fires a burst or fires a beam etc.
        public Vector2 TowerDistanceRange, TrapDistanceRange; //How far away from the tower the invader will be before stopping to fire
        public Vector2 AngleRange; //The angle that the projectile is fired at.
        public Vector2 LaunchVelocityRange; //The range of speeds that the invader can use to launch a heavy projectile
        
        public bool InTowerRange = false;
        public bool InTrapRange = false;
        public float DistToTower = 1920;
        public float DistToTrap, TrapPosition;
        public float MinTowerRange, MinTrapRange;

        public float RangedDamage; //How much damage the projectile does
        public float LaunchVelocity; //How fast the heavy projectile is travelling when launched
        public float CurrentFireDelay, MaxFireDelay; //How many milliseconds between shots
        public int CurrentBurstShots, MaxBurstShots; //How many shots are fired in a row before a longer recharge is needed

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
            #region Update Barrel Animation
            if (BarrelAnimation != null)
            {
                BarrelAnimation.Update(gameTime);
                BarrelDestinationRectangle = new Rectangle((int)BasePivot.X, (int)BasePivot.Y,
                                                           (int)(BarrelAnimation.FrameSize.X),
                                                           (int)(BarrelAnimation.FrameSize.Y));
            }
            
            BarrelEnd = new Vector2(BarrelDestinationRectangle.Center.X - (float)Math.Cos(CurrentAngle) * (BarrelPivot.X),
                                    BarrelDestinationRectangle.Center.Y - (float)Math.Sin(CurrentAngle) * (BarrelPivot.X));
            #endregion

            base.Update(gameTime, cursorPosition);
        }

        public override void Draw(GraphicsDevice graphics, BasicEffect effect, Effect shadowEffect, List<Light> lightList)
        {
            if (BarrelAnimation != null && Active == true)
            {
                effect.TextureEnabled = true;
                effect.VertexColorEnabled = true;
                effect.Texture = BarrelAnimation.Texture;

                barrelVertices[0] = new VertexPositionColorTexture()
                {
                    Position = new Vector3(BarrelDestinationRectangle.Left, BarrelDestinationRectangle.Top, 0),
                    TextureCoordinate = BarrelAnimation.dTopLeftTexCooord,
                    Color = Color
                };

                barrelVertices[1] = new VertexPositionColorTexture()
                {
                    Position = new Vector3(BarrelDestinationRectangle.Left + BarrelDestinationRectangle.Width, BarrelDestinationRectangle.Top, 0),
                    TextureCoordinate = BarrelAnimation.dTopRightTexCoord,
                    Color = Color
                };

                barrelVertices[2] = new VertexPositionColorTexture()
                {
                    Position = new Vector3(BarrelDestinationRectangle.Left + BarrelDestinationRectangle.Width, BarrelDestinationRectangle.Top + BarrelDestinationRectangle.Height, 0),
                    TextureCoordinate = BarrelAnimation.dBottomRightTexCoord,
                    Color = Color
                };

                barrelVertices[3] = new VertexPositionColorTexture()
                {
                    Position = new Vector3(BarrelDestinationRectangle.Left, BarrelDestinationRectangle.Top + BarrelDestinationRectangle.Height, 0),
                    TextureCoordinate = BarrelAnimation.dBottomLeftTexCoord,
                    Color = Color
                };

                barrelIndices[0] = 0;
                barrelIndices[1] = 1;
                barrelIndices[2] = 2;
                barrelIndices[3] = 2;
                barrelIndices[4] = 3;
                barrelIndices[5] = 0;

                Matrix view = effect.View;

                effect.View = Matrix.CreateTranslation(new Vector3(-Position.X - BarrelPivot.X, -Position.Y - BarrelPivot.Y, 0)) *
                              Matrix.CreateRotationZ(CurrentAngle) *
                              Matrix.CreateTranslation(new Vector3(Position.X + BarrelPivot.X, Position.Y + BarrelPivot.Y, 0));

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, barrelVertices, 0, 4, barrelIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                }

                effect.View = view;
            }

            base.Draw(graphics, effect, shadowEffect, lightList);
        }

        public void UpdateFireDelay(GameTime gameTime)
        {
            //This should only be called if the invader is actually ALLOWED to fire
            //i.e. They can't fire when moving, can't fire when facing the wrong way etc.

            if (CurrentFireDelay < MaxFireDelay)
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
