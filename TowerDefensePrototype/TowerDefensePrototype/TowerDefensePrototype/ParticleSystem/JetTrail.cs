using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class JetTrail : Drawable
    {
        public Vector2 Position, EndPosition;
        public float MaxWidth, MinLength, Rotation;

        VertexPositionColor[] trailVertices = new VertexPositionColor[3];
        List<Vector2> PreviousPoints = new List<Vector2>();

        public JetTrail()
        {
            Position = new Vector2(500, 500);
            MaxWidth = 16;
            MinLength = 24;
            Rotation = 45;

            EndPosition = new Vector2(Position.X + (float)Math.Cos(MathHelper.ToRadians(Rotation)) * (MinLength),
                                      Position.Y + (float)Math.Sin(MathHelper.ToRadians(Rotation)) * (MinLength));

            trailVertices[0] = new VertexPositionColor()
            {
                Position = new Vector3(Position.X - (float)Math.Cos(MathHelper.ToRadians(Rotation - 90)) * 8,
                                       Position.Y - (float)Math.Sin(MathHelper.ToRadians(Rotation - 90)) * 8, 0),
                Color = Color.White
            };

            trailVertices[1] = new VertexPositionColor()
            {
                Position = new Vector3(Position.X + (float)Math.Cos(MathHelper.ToRadians(Rotation - 90)) * 8,
                                       Position.Y + (float)Math.Sin(MathHelper.ToRadians(Rotation - 90)) * 8, 0),
                Color = Color.White
            };

            trailVertices[2] = new VertexPositionColor()
            {
                Position = new Vector3(EndPosition.X, EndPosition.Y, 0),
                Color = Color.White
            };
        }

        public void Update(GameTime gameTime, Vector2 position, float rotation)
        {
            Position = position;

            EndPosition = new Vector2(Position.X + (float)Math.Cos(MathHelper.ToRadians(Rotation)) * (MinLength),
                                      Position.Y + (float)Math.Sin(MathHelper.ToRadians(Rotation)) * (MinLength));

            trailVertices[0] = new VertexPositionColor()
            {
                Position = new Vector3(Position.X - (float)Math.Cos(MathHelper.ToRadians(Rotation - 90)) * 8,
                                       Position.Y - (float)Math.Sin(MathHelper.ToRadians(Rotation - 90)) * 8, 0),
                Color = Color.White
            };

            trailVertices[1] = new VertexPositionColor()
            {
                Position = new Vector3(Position.X + (float)Math.Cos(MathHelper.ToRadians(Rotation - 90)) * 8,
                                       Position.Y + (float)Math.Sin(MathHelper.ToRadians(Rotation - 90)) * 8, 0),
                Color = Color.White
            };

            trailVertices[2] = new VertexPositionColor()
            {
                Position = new Vector3(EndPosition.X, EndPosition.Y, 0),
                Color = Color.White
            };
        }

        public override void Draw(GraphicsDevice graphics, BasicEffect effect)
        {
            effect.TextureEnabled = false;
            effect.VertexColorEnabled = true;
            
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserPrimitives(PrimitiveType.TriangleStrip, trailVertices, 0, 1, VertexPositionColor.VertexDeclaration);
            }
        }
    }
}
