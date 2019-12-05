using System;
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
        /// <summary>
        /// Create a 4 vertex polygon with the arrays of colours and positions provided
        /// </summary>
        /// <param name="colors">Array of colours: Bottom left, top left, bottom right, top right</param>
        /// <param name="positions">Array of vertex positions: Bottom left, top left, bottom right, top right</param>
        public Quad(Vector2[] positions, Color[] colors)
        {
            for (int i = 0; i < positions.Count(); i++)
            {
                Vertices[i].Position = new Vector3(positions[i].X, positions[i].Y, 0);
                Vertices[i].Color = colors[i];
            }
        }
    }
}
