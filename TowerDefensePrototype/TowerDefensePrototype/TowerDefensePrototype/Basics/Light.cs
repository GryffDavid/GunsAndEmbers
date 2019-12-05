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

        public Vector3 Position;
        public Color Color;
        public float Power;
        public int LightDecay;
        public bool Active;
        public float Depth, CurrentTime, MaxTime;
        public object Tether;

        public VertexPositionColorTexture[] lightVertices = new VertexPositionColorTexture[4];
        public int[] lightIndices = new int[6];

        public Light()
        {
            //Range = 500;
            //Radius = 250;


        }

        public void Update()
        {
            lightVertices[0] = new VertexPositionColorTexture()
            {
                Color = Color.White,
                Position = Position - new Vector3(LightTexture.Width/2, LightTexture.Height/2, 0),
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
                Position = Position + new Vector3(LightTexture.Width/2, LightTexture.Height/2, 0),
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


            Depth = (Position.Y / 1080f);
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
    }
}
