using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TowerDefensePrototype
{
    public class VerletProjectile : Drawable
    {
        public Texture2D StickTexture, PointTexture;
        public float Bounce = 0.7f;
        float Gravity = 0.35f;
        float Friction = 1.0f;
        double Time;
        bool Rotate = true;

        public Rectangle Constraints;

        public class Stick
        {
            public Node Point1, Point2;
            public float Length;
            public Vector2 Center, PreviousCenter, Direction;
            public bool Rotate = true;
            public float Rotation;
            public Rectangle DestinationRectangle;
        }

        public class Node
        {
            public Vector2 CurrentPosition, PreviousPosition, Velocity;
            public bool Pinned;
            public float Friction;
        }

        public Node Node1 = new Node();
        public Node Node2 = new Node();
        public Stick Sticks = new Stick();

        public VerletProjectile(Rectangle? constraints = null)
        {
            if (constraints == null)
            {
                Constraints = new Rectangle(0, 0, 1920, 890);
            }
            else
            {
                Constraints = constraints.Value;
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            Time += gameTime.ElapsedGameTime.TotalMilliseconds;
            
            if (Time > 16)
            {
                for (int i = 0; i < 20; i++)
                {
                    ConstrainNodes();
                    UpdateSticks();
                }

                UpdateNodes();

                Time = 0;
            }
        }

        //public override void Draw(SpriteBatch spriteBatch)
        //{

        //}


        public void UpdateNodes()
        {
            if (Rotate == true)
            {
                Sticks.Point1.Velocity = (Sticks.Center - Sticks.PreviousCenter) * Friction;
                Sticks.Point1.CurrentPosition = Sticks.Center - (Sticks.Direction * Sticks.Length / 2);
                Sticks.Point1.PreviousPosition = Sticks.Point1.CurrentPosition;
                Sticks.Point1.CurrentPosition += Sticks.Point1.Velocity * Sticks.Point1.Friction;

                Sticks.Point1.CurrentPosition.Y += Gravity;

                Sticks.Point2.Velocity = (Sticks.Center - Sticks.PreviousCenter) * Friction;
                Sticks.Point2.CurrentPosition = Sticks.Center + (Sticks.Direction * Sticks.Length / 2);
                Sticks.Point2.PreviousPosition = Sticks.Point2.CurrentPosition;
                Sticks.Point2.CurrentPosition += Sticks.Point2.Velocity * Sticks.Point2.Friction;

                Sticks.Point2.CurrentPosition.Y += Gravity;
            }
            else
            {
                if (Node1.Pinned == false)
                {
                    Node1.Velocity = (Node1.CurrentPosition - Node1.PreviousPosition) * Friction;
                    Node1.PreviousPosition = Node1.CurrentPosition;
                    Node1.CurrentPosition += Node1.Velocity * Node1.Friction;
                    Node1.CurrentPosition.Y += Gravity;

                    if (Node1.Velocity.X < 1f && Node1.Velocity.Y < 1f)
                    {
                        Node1.Velocity = new Vector2(0, 0);
                    }
                }

                if (Node2.Pinned == false)
                {
                    Node2.Velocity = (Node2.CurrentPosition - Node2.PreviousPosition) * Friction;
                    Node2.PreviousPosition = Node2.CurrentPosition;
                    Node2.CurrentPosition += Node2.Velocity * Node2.Friction;
                    Node2.CurrentPosition.Y += Gravity;

                    if (Node2.Velocity.X < 1f && Node2.Velocity.Y < 1f)
                    {
                        Node2.Velocity = new Vector2(0, 0);
                    }
                }
            }                     
        }

        public void ConstrainNodes()
        {
            #region Node1
            if (Node1.CurrentPosition.X >= Constraints.Width)
            {
                Node1.CurrentPosition.X = Constraints.Width;
                Node1.PreviousPosition.X = Node1.CurrentPosition.X + Node1.Velocity.X * Bounce;
                Node1.Friction = 0.5f;
                Rotate = false;
            }

            if (Node1.CurrentPosition.X <= 0)
            {
                Node1.CurrentPosition.X = 0;
                Node1.PreviousPosition.X = Node1.CurrentPosition.X + Node1.Velocity.X * Bounce;
                Node1.Friction = 0.5f;
                Rotate = false;
            }

            if (Node1.CurrentPosition.Y >= Constraints.Height)
            {
                Node1.CurrentPosition.Y = Constraints.Height;
                Node1.PreviousPosition.Y = Node1.CurrentPosition.Y + Node1.Velocity.Y * Bounce;
                Node1.Friction = 0.5f;
                Rotate = false;
            }

            if (Node1.CurrentPosition.Y <= 0)
            {
                Node1.CurrentPosition.Y = 0;
                Node1.PreviousPosition.Y = Node1.CurrentPosition.Y + Node1.Velocity.Y * Bounce;
                Node1.Friction = 0.5f;
                Rotate = false;
            }

            if (Node1.CurrentPosition.Y < Constraints.Height &&
                Node1.CurrentPosition.Y > 0 &&
                Node1.CurrentPosition.X < Constraints.Width &&
                Node1.CurrentPosition.X > 0)
            {
                Node1.Friction = 1.0f;
            }
            #endregion

            #region Node2
            if (Node2.CurrentPosition.X >= Constraints.Width)
            {
                Node2.CurrentPosition.X = Constraints.Width;
                Node2.PreviousPosition.X = Node2.CurrentPosition.X + Node2.Velocity.X * Bounce;
                Node2.Friction = 0.5f;
                Rotate = false;
            }
            
            if (Node2.CurrentPosition.X <= 0)
            {
                Node2.CurrentPosition.X = 0;
                Node2.PreviousPosition.X = Node2.CurrentPosition.X + Node2.Velocity.X * Bounce;
                Node2.Friction = 0.5f;
                Rotate = false;
            }

            if (Node2.CurrentPosition.Y >= Constraints.Height)
            {
                Node2.CurrentPosition.Y = Constraints.Height;
                Node2.PreviousPosition.Y = Node2.CurrentPosition.Y + Node2.Velocity.Y * Bounce;
                Node2.Friction = 0.5f;
                Rotate = false;
            }

            if (Node2.CurrentPosition.Y <= 0)
            {
                Node2.CurrentPosition.Y = 0;
                Node2.PreviousPosition.Y = Node2.CurrentPosition.Y + Node2.Velocity.Y * Bounce;
                Node2.Friction = 0.5f;
                Rotate = false;
            }

            if (Node2.CurrentPosition.Y < Constraints.Height &&
                Node2.CurrentPosition.Y > 0 &&
                Node2.CurrentPosition.X < Constraints.Width &&
                Node2.CurrentPosition.X > 0)
            {
                Node2.Friction = 1.0f;
            }
            #endregion
        }

        public void UpdateSticks()
        {
            Vector2 currentDir = Sticks.Point2.CurrentPosition - Sticks.Point1.CurrentPosition;
            currentDir.Normalize();
            Sticks.Center = Sticks.Point1.CurrentPosition + currentDir * Sticks.Length / 2;

            Vector2 previousDir = Sticks.Point2.PreviousPosition - Sticks.Point1.PreviousPosition;
            previousDir.Normalize();
            Sticks.PreviousCenter = Sticks.Point1.PreviousPosition + previousDir * Sticks.Length / 2;

            Sticks.Direction = Sticks.Center - Sticks.PreviousCenter;
            Sticks.Direction.Normalize();

            float currentLength = Vector2.Distance(Sticks.Point1.CurrentPosition, Sticks.Point2.CurrentPosition);

            if (currentLength != Sticks.Length)
            {
                Vector2 Direction = Sticks.Point2.CurrentPosition - Sticks.Point1.CurrentPosition;
                Direction.Normalize();

                if (Sticks.Point2.Pinned == false)
                    Sticks.Point2.CurrentPosition -= (Direction * (currentLength - Sticks.Length) / 2);

                if (Sticks.Point1.Pinned == false)
                    Sticks.Point1.CurrentPosition += (Direction * (currentLength - Sticks.Length) / 2);
            }     
        }    
    }
}
