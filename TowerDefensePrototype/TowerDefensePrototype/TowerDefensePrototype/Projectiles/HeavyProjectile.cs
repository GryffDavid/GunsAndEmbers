using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public abstract class HeavyProjectile : Drawable
    {
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
        
        public Texture2D Texture;
        public List<Emitter> EmitterList;
        public Vector2 Velocity, Position, YRange, Scale, Origin, Direction;

        public float Angle, Speed, CurrentRotation, CurrentTransparency, MaxY, Damage, BlastRadius, Gravity;
        float Bounce = 0.7f;        
        float Friction = 1.0f;

        double Time;

        public bool Shadow, Verlet;
        public bool Rotate = true;

        public Color CurrentColor;
        public HeavyProjectileType HeavyProjectileType;
        public Rectangle DestinationRectangle, CollisionRectangle, Constraints;
        static Random Random = new Random();

        public Node Node1 = new Node();
        public Node Node2 = new Node();
        public Stick Sticks = new Stick();

        public HeavyProjectile(Texture2D texture, Vector2 position, float speed, float angle, float gravity, float damage,
                               Vector2? yrange = null, float? blastRadius = null, bool? verlet = false)
        {
            //Initialise all the regular variables
            Active = true;
            Texture = texture;
            Angle = angle;
            Speed = speed;
            Gravity = gravity;
            Position = position;
            Damage = damage;

            MaxY = 890;

            Velocity.X = (float)(Math.Cos(angle) * speed);
            Velocity.Y = (float)(Math.Sin(angle) * speed);

            if (yrange == null)
            {
                YRange = new Vector2(500, 600);
            }
            else
            {
                YRange = yrange.Value;
            }

            if (blastRadius.HasValue)
                BlastRadius = blastRadius.Value;

            Verlet = verlet.Value;
                        
            //Set up special values based on whether the projectile is Verlet based or not
            if (Verlet == true)
            {
                Node1 = new Node()
                {
                    CurrentPosition = Position,
                    PreviousPosition = Position - Velocity,
                    Pinned = false
                };

                Node2 = new Node()
                {
                    CurrentPosition = new Vector2(Position.X - (float)Math.Cos(Angle) * (Texture.Width),
                                                  Position.Y - (float)Math.Sin(Angle) * (Texture.Width)),
                    PreviousPosition = new Vector2(Position.X - (float)Math.Cos(Angle) * (Texture.Width),
                                                  Position.Y - (float)Math.Sin(Angle) * (Texture.Width)) - Velocity,
                    Pinned = false
                };

                Sticks = new Stick()
                {
                    Length = Texture.Width,
                    Rotate = true,
                    Point1 = Node2,
                    Point2 = Node1
                };
            }
        }

        public void Initialize()
        {
            Active = true;
            CurrentTransparency = 0;
            MaxY = Random.Next((int)YRange.X, (int)YRange.Y);            
            Scale = new Vector2(1, 1);
            Origin = new Vector2(Texture.Width / 2, Texture.Height / 2);
            Shadow = true;
            DrawDepth = MaxY / 1080;

            if (Verlet == true)
            {
                Constraints = new Rectangle(0, 0, 1920, (int)MaxY);
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            #region Uses Verlet-based physics
            if (Verlet == true)
            {
                if (Active == true)
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

                    DestinationRectangle = new Rectangle((int)Sticks.Point1.CurrentPosition.X,
                                                     (int)Sticks.Point1.CurrentPosition.Y,
                                                     Texture.Width, Texture.Height);

                    CollisionRectangle = new Rectangle((int)Sticks.Point1.CurrentPosition.X,
                                                       (int)Sticks.Point1.CurrentPosition.Y,
                                                       Texture.Width, Texture.Height);

                    foreach (Emitter emitter in EmitterList)
                    {
                        emitter.Position = Node2.CurrentPosition;
                    }

                    Position = Node1.CurrentPosition;
                }

                Vector2 dir = Sticks.Point2.CurrentPosition - Sticks.Point1.CurrentPosition;
                Sticks.Rotation = (float)Math.Atan2(dir.Y, dir.X);

                Sticks.DestinationRectangle = new Rectangle(
                                                  (int)Sticks.Point1.CurrentPosition.X,
                                                  (int)Sticks.Point1.CurrentPosition.Y,
                                                  Texture.Width, Texture.Height);
            }
            #endregion
            else
            #region Uses Euler-based physics
            {
                if (Active == true)
                {
                    //This line simulates air friction. 
                    //I'm not sure if I should keep it. I thought it'd make the projectiles look a bit better.
                    //It kinda does.
                    Velocity.X = MathHelper.Lerp(Velocity.X, 0, 0.005f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));

                    Position += Velocity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                    Velocity.Y += Gravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                    foreach (Emitter emitter in EmitterList)
                    {
                        Vector2 projectileRear = new Vector2(Position.X - (float)Math.Cos(CurrentRotation) * (Texture.Width/2),
                                                             Position.Y - (float)Math.Sin(CurrentRotation) * (Texture.Width/2));

                        emitter.Position = projectileRear;
                    }
                }

                if (Rotate == true)
                    CurrentRotation = (float)Math.Atan2(Velocity.Y, Velocity.X);
            }
            #endregion

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Verlet == true)
            {
                if (Active == true)
                {
                    spriteBatch.Draw(Texture, Sticks.DestinationRectangle, null, Color.White, Sticks.Rotation,
                                     new Vector2(0, Texture.Height / 2), SpriteEffects.None, 0);
                }
            }
            else
            {
                if (Active == true)
                {
                    DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y,
                                                         Texture.Width * (int)Scale.X, Texture.Height * (int)Scale.Y);

                    CollisionRectangle = new Rectangle(DestinationRectangle.X, DestinationRectangle.Y,
                                                       DestinationRectangle.Width / 2, DestinationRectangle.Height / 2);

                    spriteBatch.Draw(Texture, DestinationRectangle, null, Color.White, CurrentRotation,
                                     new Vector2(Origin.X, Origin.Y), SpriteEffects.None, DrawDepth);
                }
            }

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Draw(spriteBatch);
            }
        }


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
