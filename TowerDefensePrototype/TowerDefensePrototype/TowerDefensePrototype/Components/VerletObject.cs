﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TowerDefensePrototype
{
    public class VerletObject : Drawable
    {
        float Bounce = 0.6f;
        float Gravity = 0.03f;
        float Friction = 0.999f;

        public Vector2 YRange;
        //float MaxY;

        public class Stick
        {
            public Node Point1, Point2;
            public float Length;
            public float Rotation;
            public Rectangle DestinationRectangle;
        }

        public class Node
        {
            public Vector2 CurrentPosition, PreviousPosition, Velocity;
            public bool Pinned;
        }

        public List<Node> Nodes = new List<Node>();
        public List<Node> Nodes2 = new List<Node>();
        public List<Stick> Sticks = new List<Stick>();
        public List<Stick> Sticks2 = new List<Stick>();

        double Time;

        public VerletObject()
        {
            YRange = new Vector2(690, 960);
            MaxY = (float)Game1.RandomDouble(870, 870+80);
        }

        public virtual void Update(GameTime gameTime)
        {
            Time += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (Time > 16)
            {
                UpdateNodes(gameTime);

                //for (int i = 0; i < 20; i++)
                //{
                //    ConstrainNodes(gameTime);
                //    UpdateSticks(gameTime);
                //}

                for (int i = 0; i < 10; i++)
                {
                    ConstrainNodes(gameTime);
                    UpdateSticks(gameTime);
                }

                Time = 0;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

        }


        public void UpdateSticks(GameTime gameTime)
        {
            foreach (Stick stick in Sticks)
            {
                Vector2 directioon = stick.Point1.CurrentPosition - stick.Point2.CurrentPosition;
                float currentLength = directioon.Length();

                if (currentLength != stick.Length)
                {
                    directioon.Normalize();

                    if (stick.Point2.Pinned == false)
                        stick.Point2.CurrentPosition += (directioon * (currentLength - stick.Length) / 2);

                    if (stick.Point1.Pinned == false)
                        stick.Point1.CurrentPosition -= (directioon * (currentLength - stick.Length) / 2);
                }
            }

            foreach (Stick stick in Sticks2)
            {
                Vector2 directioon = stick.Point1.CurrentPosition - stick.Point2.CurrentPosition;
                float currentLength = directioon.Length();

                if (currentLength != stick.Length)
                {
                    directioon.Normalize();

                    if (stick.Point2.Pinned == false)
                        stick.Point2.CurrentPosition += (directioon * (currentLength - stick.Length) / 2);

                    if (stick.Point1.Pinned == false)
                        stick.Point1.CurrentPosition -= (directioon * (currentLength - stick.Length) / 2);
                }
            }
        }

        public void ConstrainNodes(GameTime gameTime)
        {
            foreach (Node node in Nodes)
            {
                if (node.Pinned == false)
                {
                    if (node.CurrentPosition.X > 1920)
                    {
                        node.CurrentPosition.X = 1920;
                        node.PreviousPosition.X = node.CurrentPosition.X + node.Velocity.X * Bounce;
                    }

                    if (node.CurrentPosition.X < 0)
                    {
                        node.CurrentPosition.X = 0;
                        node.PreviousPosition.X = node.CurrentPosition.X + node.Velocity.X * Bounce;
                    }

                    if (node.CurrentPosition.Y > MaxY)
                    {
                        node.CurrentPosition.Y = MaxY;
                        node.PreviousPosition.Y = node.CurrentPosition.Y + node.Velocity.Y * Bounce;
                    }

                    if (node.CurrentPosition.Y < 0)
                    {
                        node.CurrentPosition.Y = 0;
                        node.PreviousPosition.Y = node.CurrentPosition.Y + node.Velocity.Y * Bounce;
                    }
                }
            }

            foreach (Node node in Nodes2)
            {
                if (node.Pinned == false)
                {
                    if (node.CurrentPosition.X > 1920)
                    {
                        node.CurrentPosition.X = 1920;
                        node.PreviousPosition.X = node.CurrentPosition.X + node.Velocity.X * Bounce;
                    }

                    if (node.CurrentPosition.X < 0)
                    {
                        node.CurrentPosition.X = 0;
                        node.PreviousPosition.X = node.CurrentPosition.X + node.Velocity.X * Bounce;
                    }

                    if (node.CurrentPosition.Y > MaxY)
                    {
                        node.CurrentPosition.Y = MaxY;
                        node.PreviousPosition.Y = node.CurrentPosition.Y + node.Velocity.Y * Bounce;
                    }

                    if (node.CurrentPosition.Y < 0)
                    {
                        node.CurrentPosition.Y = 0;
                        node.PreviousPosition.Y = node.CurrentPosition.Y + node.Velocity.Y * Bounce;
                    }
                }
            }
        }

        public void UpdateNodes(GameTime gameTime)
        {
            foreach (Node node in Nodes)
            {
                if (node.Pinned == false)
                {
                    node.Velocity = (node.CurrentPosition - node.PreviousPosition) * Friction;
                    node.PreviousPosition = node.CurrentPosition;
                    node.Velocity.Y += Gravity * (float)Time;
                    node.CurrentPosition += node.Velocity;

                    if (node.CurrentPosition.Y >= MaxY)
                    {
                        Friction = 0.92f;
                    }
                    else
                    {
                        Friction = 0.999f;
                    }
                }
            }

            foreach (Node node in Nodes2)
            {
                if (node.Pinned == false)
                {
                    node.Velocity = (node.CurrentPosition - node.PreviousPosition) * Friction;
                    node.PreviousPosition = node.CurrentPosition;
                    node.Velocity.Y += Gravity * (float)Time;
                    node.CurrentPosition += node.Velocity;

                    if (node.CurrentPosition.Y >= MaxY)
                    {
                        Friction = 0.92f;
                    }
                    else
                    {
                        Friction = 0.999f;
                    }
                }
            }
        }
    }
}
