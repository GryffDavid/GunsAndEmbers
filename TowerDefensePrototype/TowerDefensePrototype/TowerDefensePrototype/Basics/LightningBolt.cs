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
        float Jaggedness;
        Vector2 PreviousPoint;
        float PreviousDisplacement;
        float Pos;
        float Scale;
        float Envelope;
        float Displacement;
        float CurrentTime;
        float VibrateTime = 30;
        List<float> Positions = new List<float>();
        List<Line> Results = new List<Line>();
        Vector2 Tangent;
        Vector2 Normal;
        public Vector2 Point, Source, Destination;
        float Length;
        bool Vibrate;
        
        public bool Tethered;
        public Invader SourceInvader, DestinationInvader;

        public LightningBolt(Vector2 source, Vector2 destination, Color color, float fadeRate, float? sway = null, bool? vibrate = false)
        {
            Tethered = false;

            if (sway == null)
                Sway = 500f;
            else
                Sway = sway.Value;

            if (vibrate == null)
                Vibrate = false;
            else
                Vibrate = vibrate.Value;

            Source = source;
            Destination = destination;            

            Segments = CreateBolt(source, destination, 2);
            Color = color;
            Alpha = 1f;
            AlphaMultiplier = 0.6f;
            FadeOutRate = fadeRate;
        }

        //This is for a special lightning bolt which can be tethered between two invaders
        public LightningBolt(Invader source, Invader destination, Color color, float fadeRate, float? sway = null, bool? vibrate = false)
        {
            Tethered = true;

            if (sway == null)
                Sway = 500f;
            else
                Sway = sway.Value;

            if (vibrate == null)
                Vibrate = false;
            else
                Vibrate = vibrate.Value;

            SourceInvader = source;
            DestinationInvader = destination;

            Source = source.Center;
            Destination = destination.Center;

            Segments = CreateBolt(Source, Destination, 2);
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
        
        public void Update(GameTime gameTime)
        {
            if (Vibrate == true)
            {
                CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (CurrentTime > VibrateTime)
                {
                    Segments.Clear();
                    Results.Clear();
                    Positions.Clear();
                    CreateBolt(Source, Destination, 1);
                    CurrentTime = 0;
                }
            }

            Alpha -= FadeOutRate * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60);
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
            Segments.Clear();
            Results.Clear();
            Positions.Clear();

            Tangent = destination - source;
            Normal = Vector2.Normalize(new Vector2(Tangent.Y, -Tangent.X));
            Length = Tangent.Length();
            
            Positions.Add(0);

            for (int i = 0; i < Length / 8; i++)
                Positions.Add((float)RandomDouble(0,1));

            Positions.Sort();

            Jaggedness = 1.15f / Sway;

            PreviousPoint = source;
            PreviousDisplacement = 0;

            for (int i = 1; i < Positions.Count; i++)
            {
                Pos = Positions[i];

                Scale = (Length * 0.0023f) * (Pos - Positions[i - 1]);

                if (Pos > 0.95)
                    Envelope = 20 * (1 - Pos);
                else
                    Envelope = 1;

                Displacement = (float)RandomDouble(-Sway, Sway);
                Displacement -= (Displacement - PreviousDisplacement) * (1 - Scale);
                Displacement *= Envelope;

                Point = source + Pos * Tangent + Displacement * Normal;
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
