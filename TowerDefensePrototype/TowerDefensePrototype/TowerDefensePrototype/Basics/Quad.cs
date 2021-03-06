﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class Quad
    {
        public VertexPositionColor[] Vertices = new VertexPositionColor[4];
        public int[] Indices = new int[6];
        /// <summary>
        /// Create a 4 vertex polygon with the arrays of colours and positions provided
        /// </summary>
        /// <param name="colors">Array of colours: Top Left, Top Right, Bottom Right, Bottom Left</param>
        /// <param name="positions">Array of vertex positions: Top Left, Top Right, Bottom Right, Bottom Left</param>
        public Quad(Vector2[] positions, Color[] colors)
        {
            for (int i = 0; i < positions.Count(); i++)
            {
                Vertices[i].Position = new Vector3(positions[i].X, positions[i].Y, 0);
                Vertices[i].Color = colors[i];
            }

            Indices[0] = 1;
            Indices[1] = 3;
            Indices[2] = 0;
            Indices[3] = 0;
            Indices[4] = 3;
            Indices[5] = 2;
        }

        public void DrawQuad(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indices, 0, 
                                                     Indices.Length / 3, VertexPositionColor.VertexDeclaration);   
        }
    }
}
