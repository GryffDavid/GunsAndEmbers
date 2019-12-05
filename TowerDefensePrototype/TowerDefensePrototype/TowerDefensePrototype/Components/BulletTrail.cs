using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class BulletTrail : Drawable
    {
        //public Texture2D Segment;
        public Vector2 Source, Destination, Direction;
        public float Thickness, Alpha, AlphaMultiplier, FadeOutRate, Angle, ThicknessScale, Length;
        //public Color Color = Color.White;
        public Vector2 MiddleScale, MiddleOrigin, CapOrigin;
        public Turret SourceTurret;

        public BulletTrail(Vector2 source, Vector2 destination, float thickness = 1f)
        {
            Emissive = true;
            Source = source;
            Destination = destination;
            Thickness = thickness;
            Alpha = 1f;
            AlphaMultiplier = 0.5f;
            FadeOutRate = 0.4f;
            Direction = Destination - Source;
            Angle = (float)Math.Atan2(Direction.Y, Direction.X);
            const float ImageThickness = 8;
            ThicknessScale = Thickness / ImageThickness;
            Color = Color.White;
            //Color = Color.Lerp(Color, Color.Transparent, AlphaMultiplier);
            //Color.A = 255;

            DrawDepth = destination.Y / 1080f;
            Length = Vector2.Distance(source, destination);

            vertices[0] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(0, 0, 0),
                TextureCoordinate = new Vector2(0, 0)
            };

            vertices[1] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(Length, 0, 0),
                TextureCoordinate = new Vector2(1, 0)
            };

            vertices[2] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(Length, 3, 0),
                TextureCoordinate = new Vector2(1, 1)
            };

            vertices[3] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(0, 8, 0),
                TextureCoordinate = new Vector2(0, 1)
            };

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 3;
            indices[5] = 0;
        }

        public void Update(GameTime gameTime)
        {
            Color = Color.Lerp(Color, Color.Transparent, FadeOutRate * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60));

            vertices[0].Color = Color;
            vertices[1].Color = Color;
            vertices[2].Color = Color;
            vertices[3].Color = Color;
        }
        public override void Draw(GraphicsDevice graphics, BasicEffect effect)
        {
            effect.VertexColorEnabled = true;
            effect.World = Matrix.CreateRotationZ(Angle) *
                           Matrix.CreateTranslation(new Vector3(Source.X, Source.Y, 0));

            base.Draw(graphics, effect);
        }

        public void SetUp()
        {
            MiddleOrigin = new Vector2(0, Texture.Height / 2);
            MiddleScale = new Vector2(Direction.Length(), ThicknessScale);
        }
    }
}
