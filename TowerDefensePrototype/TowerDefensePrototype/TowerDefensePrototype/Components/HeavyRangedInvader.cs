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
        #region For drawing barrel animations
        public InvaderAnimation BarrelAnimation;
        public VertexPositionColorTexture[] barrelVertices = new VertexPositionColorTexture[4];        
        public Rectangle BarrelDestinationRectangle;
        public Vector2 BarrelPivot, BasePivot, BarrelEnd;
        public int[] barrelIndices = new int[6];
        #endregion

        public float CurrentAngle = 0;
        public float EndAngle;

        public int TotalHits = 0;

        public int HitGround = 0;
        public int HitTower = 0;
        public int HitShield = 0;
        public int HitTurret = 0;
        public int HitTrap = 0;

        public override void Initialize()
        {
            MinTowerRange = Random.Next((int)DistanceRange.X,
                                   (int)DistanceRange.Y);
            base.Initialize();
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {            
            //if (DistToTower <= MinRange)
            //{
            //    //Whether the invader stops or not should be based on their MicroBehaviour
            //    Velocity.X = 0;
            //    InRange = true;
            //}

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
    }
}
