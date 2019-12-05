using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class VectorSprite : Drawable
    {
        Texture2D Texture;
        Vector2 Position, Size;
        Rectangle DestinationRectangle;
        Color Color;
        VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];
        int[] indices = new int[6];

        public VectorSprite(Vector2 position, Vector2 size, Texture2D texture, Color color)
        {
            Texture = texture;
            Position = position;
            Size = size;
            Color = color;
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

            vertices[0] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(Position, 0),
                TextureCoordinate = new Vector2(0, 0)
            };

            vertices[1] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(Position.X + Size.X, Position.Y, 0),
                TextureCoordinate = new Vector2(1, 0)
            };

            vertices[2] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(Position.X + Size.X, Position.Y + Size.Y, 0),
                TextureCoordinate = new Vector2(1, 1)
            };

            vertices[3] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(Position.X, Position.Y + Size.Y, 0),
                TextureCoordinate = new Vector2(0, 1)
            };

            //vertices[4] = new VertexPositionColorTexture()
            //{
            //    Color = Color,
            //    Position = new Vector3(Position, 0),
            //    TextureCoordinate = new Vector2(0, 0)
            //};

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 3;
            indices[5] = 0;
        }
        
        public void Update(Vector2 position)
        {
            Position = position;
            vertices[0] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(Position, 0),
                TextureCoordinate = new Vector2(0, 0)
            };

            vertices[1] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(Position.X + Size.X, Position.Y, 0),
                TextureCoordinate = new Vector2(1, 0)
            };

            vertices[2] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(Position.X + Size.X, Position.Y + Size.Y, 0),
                TextureCoordinate = new Vector2(1, 1)
            };

            vertices[3] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(Position.X, Position.Y + Size.Y, 0),
                TextureCoordinate = new Vector2(0, 1)
            };
        }

        public override void Draw(GraphicsDevice graphics, BasicEffect effect)
        {
            effect.Texture = Texture;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2);
            }

            base.Draw(graphics, effect);
        }
    }
}
