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
        //public bool InRange = false;
        public InvaderRangedStruct RangedDamageStruct;
        public InvaderAnimation BarrelAnimation;
        public VertexPositionColorTexture[] barrelVertices = new VertexPositionColorTexture[4];        
        public Rectangle BarrelDestinationRectangle;
        public Vector2 BarrelPivot, BasePivot, BarrelEnd;
        public float CurrentAngle;
        public int[] barrelIndices = new int[6];

        //public object HitObject;
        //public float DistToTower = 1920;
        //public float MinDistance;

        public List<HeavyProjectile> FiredProjectiles = new List<HeavyProjectile>();//List of projectiles the invader has fired and are still active

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            if (RangedDamageStruct.DistToTower <= RangedDamageStruct.MinDistance)
            {
                Velocity.X = 0;
                RangedDamageStruct.InRange = true;
            }

            if (BarrelAnimation != null)
            {
                BarrelAnimation.Update(gameTime);

                BarrelDestinationRectangle = new Rectangle((int)BasePivot.X, (int)BasePivot.Y,
                                                           (int)(BarrelAnimation.FrameSize.X),
                                                           (int)(BarrelAnimation.FrameSize.Y));

                //CurrentAngle += 0.1f;
            }

            FiredProjectiles.RemoveAll(Projectile => Projectile.Active == false && Projectile.EmitterList.All(Emitter => Emitter.AddMore == false));

            //Vector2 BarrelCenter = new Vector2(BarrelDestinationRectangle.X + (float)Math.Cos(CurrentAngle - 90) * (BarrelPivot.Y - BarrelDestinationRectangle.Height / 2),
            //                                   BarrelDestinationRectangle.Y + (float)Math.Sin(CurrentAngle - 90) * (BarrelPivot.Y - BarrelDestinationRectangle.Height / 2));

            BarrelEnd = new Vector2(BarrelDestinationRectangle.Center.X - (float)Math.Cos(CurrentAngle) * (BarrelPivot.X),
                                    BarrelDestinationRectangle.Center.Y - (float)Math.Sin(CurrentAngle) * (BarrelPivot.X));

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
                              Matrix.CreateRotationZ(MathHelper.ToRadians(CurrentAngle)) *
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
            if (RangedDamageStruct != null)
            {
                RangedDamageStruct.CurrentFireDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (RangedDamageStruct.CurrentFireDelay >= RangedDamageStruct.MaxFireDelay)
                {
                    CanAttack = true;
                    RangedDamageStruct.CurrentFireDelay = 0;
                }
                else
                {
                    CanAttack = false;
                }
            }
        }
    }
}
