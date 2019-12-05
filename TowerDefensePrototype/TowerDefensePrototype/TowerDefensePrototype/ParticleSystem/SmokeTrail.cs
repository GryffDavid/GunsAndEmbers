using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TowerDefensePrototype
{
    public class SmokeTrail
    {
        VertexPositionColor[] vertices = new VertexPositionColor[81];

        public Vector2 StartPosition;// = new Vector2(1920/2, 1080/2);

        Vector2 ControlPoint1 = new Vector2(400, 600);
        Vector2 ControlPoint2 = new Vector2(200, 200);
        Vector2 ControlPoint3 = new Vector2(600, 100);

        float CurrentTime, Time2, MaxTime, Time3, MaxTime3;
        static Random Random = new Random();
        float MaxWidth = 2;
        float t = 0;
        float tMult = 1.0f;

        class SmokePoint
        {
            public Vector2 Position, Velocity, Friction, Acceleration, Direction;
            public float Width;
        }

        float Rad1 = 100;
        float NextRad1;

        float Rad2 = 100;
        float NextRad2;

        float VFriction = 1.0f;
        float NextVFriction = 1.0f;

        List<SmokePoint> Positions = new List<SmokePoint>();

        Color Color = Color.LightGray;

        float eightSize = 0.25f;
        bool AddMore = true;
        public HeavyProjectile ProjectileTether;

        public SmokeTrail(Vector2 startPosition)
        {
            StartPosition = startPosition;

            for (int i = 0; i < vertices.Length / 2 + 1; i++)
            {
                SmokePoint smokePoint = new SmokePoint()
                {
                    Position = StartPosition,
                    Acceleration = new Vector2(0, -0.05f),
                    Velocity = new Vector2(0, -0.5f),
                    Friction = new Vector2(1, 1)
                };
                smokePoint.Width = Random.Next(5, 8);

                Positions.Add(smokePoint);
            }

            MaxTime3 = Random.Next(1500, 8000);
            MaxTime = Random.Next(150, 800);
        }

        public void Update(GameTime gameTime)
        {
            CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            Time2 += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            Time3 += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (ProjectileTether != null)
            {
                StartPosition = ProjectileTether.BasePosition;

                if (ProjectileTether.Active == false)
                {
                    AddMore = false;
                    //Reduce the vertices and positions until they're all gone.
                    //i.e. the tip of the smoke gradually moves down the list
                    //Remove the tip
                }
            }

            #region Add a new point
            if (Time2 > 60 && AddMore == true)
            {
                if (Positions.Count > vertices.Length / 2)
                {
                    Positions.RemoveAt(vertices.Length / 2);
                }

                Positions.Insert(0, new SmokePoint()
                {
                    Position = StartPosition,
                    Width = Random.Next(5, 8),
                    Velocity = new Vector2(0.5f, -0.5f),
                    Acceleration = new Vector2(-0.002f, -0.05f),
                    Friction = new Vector2(0.85f, 1.0f)
                });

                Time2 = 0;
            }
            #endregion

            #region Update SmokePoints
            for (int i = 1; i < Positions.Count - 1; i++)
            {
                Vector2 Dir, NewDir;
                double angle;

                Dir = Positions[i].Position - Positions[i + 1].Position;
                Dir.Normalize();

                angle = Math.Atan2(Dir.Y, Dir.X) - MathHelper.ToRadians(90);
                NewDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                NewDir.Normalize();

                if (float.IsNaN(NewDir.X))
                {
                    NewDir = new Vector2(-1, 0);
                }

                Positions[i].Direction = NewDir;
            }

            Positions[0].Direction = new Vector2(1, 0);
            #endregion

            #region Apply forces to the points
            for (int i = Positions.Count - 1; i > 0; i--)
            {
                Positions[i].Velocity += (Positions[i].Acceleration * Positions[i].Friction);
                Positions[i].Position += Positions[i].Velocity;

                Positions[i].Friction.Y *= (Positions[i].Position.Y / StartPosition.Y) * VFriction;

                if (Positions[i].Position.Y > ControlPoint2.Y)
                {
                    Positions[i].Acceleration.X -= -Vector2.Normalize((ControlPoint2 - StartPosition)).X * 0.001f;
                }

                if (Positions[i].Position.Y > ControlPoint3.Y &&
                    Positions[i].Position.Y < ControlPoint2.Y)
                {
                    Positions[i].Acceleration.X -= -Vector2.Normalize((ControlPoint2 - StartPosition)).X * 0.001f;
                }

                if (Positions[i].Position.Y < ControlPoint3.Y)
                {
                    float dist = MathHelper.Clamp(Math.Abs(Positions[i].Position.X - StartPosition.X), 0, 120);

                    float perc = 100 - ((100 / 120) * dist);
                    Positions[i].Friction.Y = 1.0f - (perc / 180f);
                    Positions[i].Friction.X = 1.0f - (perc / 150f);
                }

                float dist2 = MathHelper.Clamp(Math.Abs(Positions[i].Position.X - StartPosition.X), 0, 120);

                float perc2 = 100 - ((100 / 120) * dist2);
                Positions[i].Friction.X = 1.0f - (perc2 / 200f);
            }
            #endregion

            #region Vertices
            for (int i = 0; i < vertices.Length - 1; i += 2)
            {
                MaxWidth = Positions[i / 2].Width;

                float newWidth = MathHelper.Lerp(MaxWidth, 0, (1.0f / (vertices.Length / 2)) * (i / 2));

                Vector2 newDir = Positions[i / 2].Direction;

                vertices[i].Position = new Vector3(Positions[i / 2].Position.X + (newDir.X * newWidth),
                                                   Positions[i / 2].Position.Y + (newDir.Y * newWidth), 0);

                vertices[i + 1].Position = new Vector3(Positions[i / 2].Position.X - (newDir.X * newWidth),
                                                       Positions[i / 2].Position.Y - (newDir.Y * newWidth), 0);

                vertices[i].Color = Color;
                vertices[i + 1].Color = Color;
            }

            vertices[vertices.Length - 1].Position = new Vector3(Positions[Positions.Count - 1].Position.X,
                                                                 Positions[Positions.Count - 1].Position.Y, 0);
            #endregion

            if (Time3 > MaxTime3)
            {
                if (tMult == -1.0f)
                {
                    tMult = 1.0f;
                }
                else
                {
                    tMult = -1.0f;
                }

                Time3 = 0;
                MaxTime3 = Random.Next(1500, 8000);
            }

            #region Move the control points
            if (CurrentTime > MaxTime)
            {
                NextRad1 = Random.Next(100, 500);
                NextRad2 = Random.Next(100, 250);
                NextVFriction = MathHelper.Lerp(VFriction, (float)RandomDouble(0.85f, 0.999f), 0.1f);

                CurrentTime = 0;

                MaxTime = Random.Next(150, 800);
                eightSize = (float)RandomDouble(0.1f, 0.8f);
            }

            Rad1 = MathHelper.Lerp(Rad1, NextRad1, 0.2f);
            Rad2 = MathHelper.Lerp(Rad2, NextRad2, 0.2f);
            VFriction = MathHelper.Lerp(VFriction, NextVFriction, 0.06f);

            t += 0.1f;

            ControlPoint1 = new Vector2(
                1.2f * (float)Math.Cos(eightSize * t),
                       (float)Math.Sin((eightSize * 2f) * t)) * 100f + new Vector2(StartPosition.X, StartPosition.Y - 80);

            ControlPoint2 = new Vector2(
                Rad1 * (float)Math.Cos(t * tMult),
                Rad2 * (float)Math.Sin(t * tMult)) + ControlPoint1;

            ControlPoint3 = Vector2.Lerp(ControlPoint3, ControlPoint2, (float)RandomDouble(0.0001f, 0.1f));

            if (t >= 8 * Math.PI)
            {
                t = 0;
            }
            #endregion
        }

        public void DrawVector(GraphicsDevice graphics, BasicEffect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length - 2);
            }
        }

        //public override void Draw(GraphicsDevice graphics, BasicEffect effect)
        //{
        //    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        //    {
        //        pass.Apply();
        //        graphics.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length - 2);
        //    }
        //}

        public double RandomDouble(double a, double b)
        {
            return a + Random.NextDouble() * (b - a);
        }
    }
}
