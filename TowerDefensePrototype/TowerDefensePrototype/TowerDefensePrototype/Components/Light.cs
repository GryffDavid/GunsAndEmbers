using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class Light
    {
        public Texture2D LightTexture;
        public float Range;
        public float Radius;

        public Vector3 Position, StartPosition;
        public Color Color;
        public float Power;
        public int LightDecay;
        public bool Active, Oscillate;
        public float Depth,
                     CurrentTime, MaxTime,
                     CurrentOscillationTime, MaxOscillationTime,
                     CurrentFlickerTime, MaxFlickerTime;
        public object Tether;

        public VertexPositionColorTexture[] lightVertices = new VertexPositionColorTexture[4];
        public int[] lightIndices = new int[6];
        public static Random Random = new Random();

        public Light(Vector3 position, Texture2D texture)
        {
            //Range = 500;
            //Radius = 250;
            LightTexture = texture;
            StartPosition = position;
            Position = position;
            MaxFlickerTime = 60;

            lightVertices[0] = new VertexPositionColorTexture()
            {
                Color = Color.White,
                Position = Position - new Vector3(LightTexture.Width / 2, LightTexture.Height / 2, 0),
                TextureCoordinate = new Vector2(0, 0)
            };

            lightVertices[1] = new VertexPositionColorTexture()
            {
                Color = Color.White,
                Position = Position - new Vector3(-LightTexture.Width / 2, LightTexture.Height / 2, 0),
                TextureCoordinate = new Vector2(1, 0)
            };

            lightVertices[2] = new VertexPositionColorTexture()
            {
                Color = Color.White,
                Position = Position + new Vector3(LightTexture.Width / 2, LightTexture.Height / 2, 0),
                TextureCoordinate = new Vector2(1, 1)
            };

            lightVertices[3] = new VertexPositionColorTexture()
            {
                Color = Color.White,
                Position = Position + new Vector3(-LightTexture.Width / 2, LightTexture.Height / 2, 0),
                TextureCoordinate = new Vector2(0, 1)
            };

            lightIndices[0] = 0;
            lightIndices[1] = 1;
            lightIndices[2] = 2;
            lightIndices[3] = 2;
            lightIndices[4] = 3;
            lightIndices[5] = 0;
        }

        public void Update(GameTime gameTime)
        {
            if (MaxTime > 0)
            {
                CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                float Percentage = CurrentTime / MaxTime;
                //LightDecay = (int)(Radius * Math.Sin(Math.PI * Percentage));
                //LightDecay = (int)(Radius - (Radius * Percentage));
                //LightDecay = (int)(Radius * Percentage);
                Position.Z -= 0.5f * Percentage;
                //Position.Y -= 0.25f * Percentage;
                Power = 0.015f;// *Percentage;
                
                if (CurrentTime >= MaxTime)
                {
                    Active = false;
                    CurrentTime = 0;
                }
            }

            if (Oscillate == true)
            {
                CurrentOscillationTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                CurrentFlickerTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                float Percentage = (CurrentOscillationTime / MaxOscillationTime);
                LightDecay = (int)(Radius + (2 * Math.Sin(Math.PI * Percentage)));

                //LightDecay = (int)(Radius + (10  * Math.Sin(Math.PI * Percentage + (Random.Next(10, 25))));
                //Position.Z = 8 * Percentage;
                //Power = 0.15f * Percentage;
                //Position.Z = MathHelper.Clamp(Position.Z + (float)RandomDouble(-1, 1), 14, 16);

                if (CurrentFlickerTime >= MaxFlickerTime)
                {
                    Position.Y = MathHelper.Clamp(Position.Y + (float)RandomDouble(-1, 1), StartPosition.Y - 2, StartPosition.Y + 2);
                    Position.X = MathHelper.Clamp(Position.X + (float)RandomDouble(-1, 1), StartPosition.X - 2, StartPosition.X + 2);
                    Position.Z = MathHelper.Clamp(Position.Z + (float)RandomDouble(-1, 1), StartPosition.Z - 1, StartPosition.Z + 1);
                    CurrentFlickerTime = 0;
                }

                if (CurrentOscillationTime >= MaxOscillationTime)
                {
                    CurrentOscillationTime = 0;
                    //MaxOscillationTime
                    MaxOscillationTime = MathHelper.Clamp(MaxOscillationTime + (float)RandomDouble(-50, 50), 400, 600);                    
                }
            }



            Depth = (Position.Y / 1080f);

            Invader tetherInvader = Tether as Invader;
            HeavyProjectile tetherProjectile = Tether as HeavyProjectile;

            if (tetherInvader != null)
            {
                Position = new Vector3(tetherInvader.Center.X, tetherInvader.Center.Y, 15);
                Depth = tetherInvader.DrawDepth - 0.1f;
            }

            if (tetherProjectile != null)
            {
                Position = new Vector3(tetherProjectile.Center.X, tetherProjectile.Center.Y, 15);
                Depth = tetherProjectile.DrawDepth;
            }
        }

        public void Draw(GraphicsDevice graphics, Effect effect)
        {
            effect.Parameters["LightTexture"].SetValue(LightTexture);
            effect.Parameters["LightSize"].SetValue(new Vector2(LightTexture.Width, LightTexture.Height));

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, lightVertices, 0, 4, lightIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
            }
        }

        public double RandomDouble(double a, double b)
        {
            return a + Random.NextDouble() * (b - a);
        }
    }
}
