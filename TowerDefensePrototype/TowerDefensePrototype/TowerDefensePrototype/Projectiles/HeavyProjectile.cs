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
        public List<Emitter> EmitterList = new List<Emitter>();
        public Vector2 Velocity, Position, YRange, Origin, Center;
        public Vector2 Scale = Vector2.One;

        public float Angle, Speed, CurrentRotation, MaxY, Damage, BlastRadius, Gravity;
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
        public Stick Rod = new Stick();

        public object SourceObject;

        public HeavyProjectile(object source, Texture2D texture, Vector2 position, float speed, float angle, float gravity, float damage,
                               Vector2? yrange = null, float? blastRadius = null, bool? verlet = false)
        {
            //Initialise all the regular variables
            SourceObject = source;
            Active = true;
            Shadow = true;
            Texture = texture;
            Angle = angle;
            Speed = speed;
            Gravity = gravity;
            Position = position;
            Damage = damage;

            

            Velocity = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed);
            Origin = new Vector2(Texture.Width / 2, Texture.Height / 2);

            if (!yrange.HasValue)
                YRange = new Vector2(500, 600);
            else
                YRange = yrange.Value;

            MaxY = Random.Next((int)YRange.X, (int)YRange.Y);
            DrawDepth = MaxY / 1080;

            if (blastRadius.HasValue)
                BlastRadius = blastRadius.Value;

            Verlet = verlet.Value;
                        
            //Set up special values based on whether the projectile is Verlet based or not
            if (verlet.Value == true)
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

                Rod = new Stick()
                {
                    Length = Texture.Width,
                    Rotate = true,
                    Point1 = Node2,
                    Point2 = Node1
                };

                Constraints = new Rectangle(0, 0, 1920, (int)MaxY);
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            if (Active == true)
            {
                #region Uses Verlet-based physics
                if (Verlet == true)
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

                    DestinationRectangle = new Rectangle((int)Rod.Point1.CurrentPosition.X,
                                                            (int)Rod.Point1.CurrentPosition.Y,
                                                            Texture.Width, Texture.Height);

                    foreach (Emitter emitter in EmitterList)
                    {
                        emitter.Position = Node2.CurrentPosition;
                    }

                    Position = Node1.CurrentPosition;

                    Vector2 dir = Rod.Point2.CurrentPosition - Rod.Point1.CurrentPosition;
                    Rod.Rotation = (float)Math.Atan2(dir.Y, dir.X);

                    Rod.DestinationRectangle = new Rectangle(
                                                      (int)Rod.Point1.CurrentPosition.X,
                                                      (int)Rod.Point1.CurrentPosition.Y,
                                                      Texture.Width, Texture.Height);

                    BoundingBox = new BoundingBox(new Vector3(Rod.Center.X - ((float)Math.Cos(Rod.Rotation) * (Texture.Width / 4)),
                                                              Rod.Center.Y - ((float)Math.Sin(Rod.Rotation) * (Texture.Width / 4)), 0),
                                                  new Vector3(Rod.Center.X + ((float)Math.Cos(Rod.Rotation) * (Texture.Width / 4)),
                                                              Rod.Center.Y + ((float)Math.Sin(Rod.Rotation) * (Texture.Width / 4)), 0));
                }
                #endregion
                else
                #region Uses Euler-based physics
                {
                    //This line simulates air friction. 
                    //I'm not sure if I should keep it. I thought it'd make the projectiles look a bit better.
                    //It kinda does.
                    Velocity.X = MathHelper.Lerp(Velocity.X, 0, 0.005f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));

                    Position += Velocity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                    Velocity.Y += Gravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                    foreach (Emitter emitter in EmitterList)
                    {
                        Vector2 projectileRear = new Vector2(Position.X - (float)Math.Cos(CurrentRotation) * (Texture.Width / 2),
                                                             Position.Y - (float)Math.Sin(CurrentRotation) * (Texture.Width / 2));

                        emitter.Position = projectileRear;
                    }

                    DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y,
                                                         Texture.Width * (int)Scale.X, Texture.Height * (int)Scale.Y);

                    BoundingBox = new BoundingBox(new Vector3(Position.X - ((float)Math.Cos(CurrentRotation) * (Texture.Width / 4)),
                                                              Position.Y - ((float)Math.Sin(CurrentRotation) * (Texture.Width / 4)), 0),
                                                  new Vector3(Position.X + ((float)Math.Cos(CurrentRotation) * (Texture.Width / 4)),
                                                              Position.Y + ((float)Math.Sin(CurrentRotation) * (Texture.Width / 4)), 0));

                    if (Rotate == true)
                        CurrentRotation = (float)Math.Atan2(Velocity.Y, Velocity.X);
                }
                #endregion

                Center = new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Center.Y);
            }

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                if (Verlet == true)
                {
                    spriteBatch.Draw(Texture, Rod.DestinationRectangle, null, Color.White, Rod.Rotation,
                                     new Vector2(0, Texture.Height / 2), SpriteEffects.None, 0);
                }
                else
                {
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
                Rod.Point1.Velocity = (Rod.Center - Rod.PreviousCenter) * Friction;
                Rod.Point1.CurrentPosition = Rod.Center - (Rod.Direction * Rod.Length / 2);
                Rod.Point1.PreviousPosition = Rod.Point1.CurrentPosition;
                Rod.Point1.CurrentPosition += Rod.Point1.Velocity * Rod.Point1.Friction;

                Rod.Point1.CurrentPosition.Y += Gravity;

                Rod.Point2.Velocity = (Rod.Center - Rod.PreviousCenter) * Friction;
                Rod.Point2.CurrentPosition = Rod.Center + (Rod.Direction * Rod.Length / 2);
                Rod.Point2.PreviousPosition = Rod.Point2.CurrentPosition;
                Rod.Point2.CurrentPosition += Rod.Point2.Velocity * Rod.Point2.Friction;

                Rod.Point2.CurrentPosition.Y += Gravity;
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
            Vector2 currentDir = Rod.Point2.CurrentPosition - Rod.Point1.CurrentPosition;
            currentDir.Normalize();
            Rod.Center = Rod.Point1.CurrentPosition + currentDir * Rod.Length / 2;

            Vector2 previousDir = Rod.Point2.PreviousPosition - Rod.Point1.PreviousPosition;
            previousDir.Normalize();
            Rod.PreviousCenter = Rod.Point1.PreviousPosition + previousDir * Rod.Length / 2;

            Rod.Direction = Rod.Center - Rod.PreviousCenter;
            Rod.Direction.Normalize();

            float currentLength = Vector2.Distance(Rod.Point1.CurrentPosition, Rod.Point2.CurrentPosition);

            if (currentLength != Rod.Length)
            {
                Vector2 Direction = Rod.Point2.CurrentPosition - Rod.Point1.CurrentPosition;
                Direction.Normalize();

                if (Rod.Point2.Pinned == false)
                    Rod.Point2.CurrentPosition -= (Direction * (currentLength - Rod.Length) / 2);

                if (Rod.Point1.Pinned == false)
                    Rod.Point1.CurrentPosition += (Direction * (currentLength - Rod.Length) / 2);
            }
        }    
    }
}
