using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class LightningBolt
    {
        static public Texture2D Segment, Cap;
        public List<Line> Segments = new List<Line>();
        public float Alpha, AlphaMultiplier, FadeOutRate;
        public Color Color;
        static Random Random = new Random();
        public float Sway;

        public LightningBolt(Vector2 source, Vector2 destination, Color color, float fadeRate, float? sway = null)
        {
            if (sway == null)
                Sway = 500f;
            else
                Sway = sway.Value;

            Segments = CreateBolt(source, destination, 2);
            Color = color;
            Alpha = 1f;
            AlphaMultiplier = 0.6f;
            FadeOutRate = fadeRate;
        }

        public void LoadContent(ContentManager contentManager)
        {
            Segment = contentManager.Load<Texture2D>("Particles/Segment");
            Cap = contentManager.Load<Texture2D>("Particles/Cap");
        }

        public void Update()
        {
            Alpha -= FadeOutRate;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Alpha <= 0)
                return;

            foreach (Line segment in Segments)
            {
                segment.Draw(spriteBatch, Color * (Alpha * AlphaMultiplier));
            }
        }

        public List<Line> CreateBolt(Vector2 source, Vector2 destination, float thickness)
        {
            List<Line> Results = new List<Line>();
            Vector2 Tangent = destination - source;
            Vector2 Normal = Vector2.Normalize(new Vector2(Tangent.Y, -Tangent.X));
            float Length = Tangent.Length();

            List<float> Positions = new List<float>();
            Positions.Add(0);

            for (int i = 0; i < Length / 4; i++)
                Positions.Add((float)RandomDouble(0,1));

            Positions.Sort();

            float Jaggedness = 1.15f / Sway;

            Vector2 PreviousPoint = source;
            float PreviousDisplacement = 0;

            for (int i = 1; i < Positions.Count; i++)
            {
                float Pos = Positions[i];

                float Scale = (Length * 0.0023f) * (Pos - Positions[i - 1]);

                float Envelope;

                if (Pos > 0.95)
                    Envelope = 20 * (1 - Pos);
                else
                    Envelope = 1;

                float Displacement = (float)RandomDouble(-Sway, Sway);
                Displacement -= (Displacement - PreviousDisplacement) * (1 - Scale);
                Displacement *= Envelope;

                Vector2 Point = source + Pos * Tangent + Displacement * Normal;
                Results.Add(new Line(PreviousPoint, Point, thickness));
                PreviousPoint = Point;
                PreviousDisplacement = Displacement;
            }

            Results.Add(new Line(PreviousPoint, destination, thickness));

            return Results;
        }

        public double RandomDouble(double a, double b)
        {
            return Random.NextDouble() * (b - a) + a;
        }

        public class Line
        {
            public Vector2 Source, Destination;
            public float Thickness;

            public Line(Vector2 source, Vector2 destination, float thickness = 1)
            {
                Source = source;
                Destination = destination;
                Thickness = thickness;
            }

            public void Draw(SpriteBatch spriteBatch, Color color)
            {
                Vector2 Tangent = Destination - Source;
                float Theta = (float)Math.Atan2(Tangent.Y, Tangent.X);

                const float ImageThickness = 8;
                float ThicknessScale = Thickness / ImageThickness;

                Vector2 CapOrigin = new Vector2(Cap.Width, Cap.Height / 2);
                Vector2 MiddleOrigin = new Vector2(0, Segment.Height / 2);
                Vector2 MiddleScale = new Vector2(Tangent.Length(), ThicknessScale);

                spriteBatch.Draw(Segment, Source, null, color, Theta, MiddleOrigin, MiddleScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(Cap, Source, null, color, Theta, CapOrigin, ThicknessScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(Cap, Destination, null, color, Theta + MathHelper.Pi, CapOrigin, ThicknessScale, SpriteEffects.None, 0f);
            }
        }
    }
}
