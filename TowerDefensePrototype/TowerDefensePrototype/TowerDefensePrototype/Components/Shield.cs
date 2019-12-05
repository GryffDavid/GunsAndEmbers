using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class Shield : Drawable
    {
        public Texture2D ShieldTexture, ShieldSphereMap;
        public Rectangle DestinationRectangle;

        VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];
        int[] indices = new int[6];

        public bool ShieldOn;
        public float MaxShield, CurrentShield, RechargeRate;
        public double CurrentShieldTime, ShieldTime;
        public float CurrentRadius, MaxRadius;

        public Vector2 Position;
        //public BoundingSphere BoundingSphere;
        public object Tether;
        
        public Shield(float? maxRadius = 150)
        {
            MaxRadius = maxRadius.Value;
            CurrentRadius = 0;
            BoundingSphere = new BoundingSphere(new Vector3(Position.X, Position.Y, 0), CurrentRadius);
        }

        public void Update(GameTime gameTime, Vector2? position = null)
        {
            if (position != null)
            {
                Position = position.Value;
                BoundingSphere.Center = new Vector3(Position.X, Position.Y, 0);
                BoundingSphere.Radius = CurrentRadius;
                DestinationRectangle = new Rectangle((int)(Position.X - CurrentRadius), (int)(Position.Y - CurrentRadius), (int)CurrentRadius*2, (int)CurrentRadius*2);

                vertices[0] = new VertexPositionColorTexture()
                {
                    Color = Color.White,
                    Position = new Vector3(Position.X - CurrentRadius, Position.Y - CurrentRadius, 0),
                    TextureCoordinate = new Vector2(0, 0)
                };

                vertices[1] = new VertexPositionColorTexture()
                {
                    Color = Color.White,
                    Position = new Vector3(Position.X + CurrentRadius, Position.Y - CurrentRadius, 0),
                    TextureCoordinate = new Vector2(1, 0)
                };

                vertices[2] = new VertexPositionColorTexture()
                {
                    Color = Color.White,
                    Position = new Vector3(Position.X + CurrentRadius, Position.Y + CurrentRadius, 0),
                    TextureCoordinate = new Vector2(1, 1)
                };

                vertices[3] = new VertexPositionColorTexture()
                {
                    Color = Color.White,
                    Position = new Vector3(Position.X - CurrentRadius, Position.Y + CurrentRadius, 0),
                    TextureCoordinate = new Vector2(0, 1)
                };

                indices[0] = 0;
                indices[1] = 1;
                indices[2] = 2;
                indices[3] = 2;
                indices[4] = 3;
                indices[5] = 0;
            }

            if (Active == true)
            {
                if (ShieldOn == true && CurrentRadius != MaxRadius)
                {
                    CurrentRadius = MathHelper.Lerp(CurrentRadius, MaxRadius, 0.1f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60)); 
                }

                if (ShieldOn == false)
                {
                    CurrentShieldTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                }

                if (ShieldOn == false &&
                    CurrentShieldTime >= ShieldTime)
                {
                    CurrentShield += RechargeRate * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60);
                    CurrentShield = MathHelper.Clamp(CurrentShield, 0, MaxShield);
                }

                if (ShieldOn == false &&
                    CurrentShieldTime >= ShieldTime &&
                    CurrentShield == MaxShield)
                {
                    ShieldOn = true;
                    CurrentShieldTime = 0;
                }
            }

            DrawDepth = (Position.Y + CurrentRadius)/1080.0f;
        }

        public float TakeDamage(float value)
        {
            if (ShieldOn == true)
            {
                if (CurrentShield > 0)
                {
                    CurrentShield -= value;
                    return 0;
                }
                else
                {
                    ShieldOn = false;
                    CurrentRadius = 0;
                    return 0;
                }
            }
            else
            {
                return 0 - value;
            }
        }

        public override void Draw(GraphicsDevice graphics, BasicEffect effect)
        {
            if (Active == true && ShieldOn == true)
            {
                effect.Texture = ShieldTexture;

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                }
            }
            base.Draw(graphics, effect);
        }
    }
}
