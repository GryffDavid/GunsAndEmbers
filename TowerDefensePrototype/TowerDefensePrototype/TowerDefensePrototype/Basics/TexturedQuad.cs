using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class TexturedQuad
    {
        public VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[4];
        public int[] Indices = new int[6];
        /// <summary>
        /// Create a 4 vertex polygon with the arrays of colours and positions provided
        /// </summary>
        /// <param name="colors">Array of colours: Top Left, Top Right, Bottom Right, Bottom Left</param>
        /// <param name="positions">Array of vertex positions: Top Left, Top Right, Bottom Right, Bottom Left</param>
        public TexturedQuad(Vector2[] positions, Color[] colors, Vector2[] texCoordinates)
        {
            for (int i = 0; i < positions.Count(); i++)
            {
                Vertices[i].Position = new Vector3(positions[i].X, positions[i].Y, 0);
                Vertices[i].Color = colors[i];
                Vertices[i].TextureCoordinate = texCoordinates[i];
            }

            Indices[0] = 0;
            Indices[1] = 1;
            Indices[2] = 2;
            Indices[3] = 2;
            Indices[4] = 3;
            Indices[5] = 0;
        }

        public void DrawQuad(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indices, 0,
                Indices.Length / 3, VertexPositionColorTexture.VertexDeclaration);
   
        }
    }
}
