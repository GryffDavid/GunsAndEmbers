using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    #region Projectile Type
    //NEW_HEAVYPROJECTILE A **heavy projectile type to be added here**
    public enum HeavyProjectileType
    {
        CannonBall,
        FlameThrower,
        Arrow,
        Acid,
        Torpedo,
        ClusterBomb,
        ClusterBombShell,
        Fel,
        Boomerang,
        Grenade,
        GasGrenade,
        FireGrenade,
        Harpoon,
        StickyMine,
        DropMissile
    }; 
    #endregion
    
    public abstract class HeavyProjectile : Drawable
    {
        

        #region For Verlet Physics
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
        #endregion

        #region Vertex Declarations
        public VertexPositionColorTexture[] shadowVertices = new VertexPositionColorTexture[4];
        public int[] shadowIndices = new int[6];

        public VertexPositionColorTexture[] projectileVertices = new VertexPositionColorTexture[4];
        public int[] projectileIndices = new int[6];

        //public VertexPositionColorTexture[] normalVertices = new VertexPositionColorTexture[4];
        //public int[] normalIndices = new int[6];
        #endregion

        #region Movement Related Variables

        #endregion

        //public Texture2D Texture;
        
        //NO PREMATURE OPTIMISATION
        public List<Emitter> EmitterList = new List<Emitter>();
        //LEAVE IT ALONE FOR NOW

        public Vector2 Velocity, YRange, Origin, Center, BasePosition, TipPosition;
        public Vector2 Scale = Vector2.One;

        public float Angle, Speed, CurrentRotation, Damage, BlastRadius, Gravity, ShadowLength;
        float Bounce = 0.7f;        
        float Friction = 1.0f;

        double Time;

        public bool Shadow, Verlet;
        public bool Rotate = true;

        //public bool TrapSolid = true;
        //public bool TowerSolid = true;
        public bool ShieldSolid;
        //public bool TurretSolid = true;

        public Color CurrentColor;
        public HeavyProjectileType HeavyProjectileType;
        public Rectangle CollisionRectangle, Constraints;
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
            PreviousMaxY = MaxY;
            DrawDepth = MaxY / 1080;

            if (blastRadius.HasValue)
                BlastRadius = blastRadius.Value;

            Verlet = verlet.Value;
                        
            #region Set up special values based on whether the projectile is Verlet based or not
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
            #endregion

            #region Sprite Vertices
            projectileVertices[0] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top, 0),
                TextureCoordinate = new Vector2(0, 0),
                Color = Color.White
            };

            projectileVertices[1] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top, 0),
                TextureCoordinate = new Vector2(1, 0),
                Color = Color.White
            };

            projectileVertices[2] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                TextureCoordinate = new Vector2(1, 1),
                Color = Color.White
            };

            projectileVertices[3] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                TextureCoordinate = new Vector2(0, 1),
                Color = Color.White
            };

            projectileIndices[0] = 0;
            projectileIndices[1] = 1;
            projectileIndices[2] = 2;
            projectileIndices[3] = 2;
            projectileIndices[4] = 3;
            projectileIndices[5] = 0;
            #endregion

            #region Shadow Vertices
            shadowVertices[0] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left, MaxY, 0),
                TextureCoordinate = new Vector2(0, 0),
                Color = Color.White
            };

            shadowVertices[1] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, MaxY, 0),
                TextureCoordinate = new Vector2(1, 0),
                Color = Color.White
            };

            shadowVertices[2] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, MaxY + DestinationRectangle.Height, 0),
                TextureCoordinate = new Vector2(1, 1),
                Color = Color.White
            };

            shadowVertices[3] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left, MaxY + DestinationRectangle.Height, 0),
                TextureCoordinate = new Vector2(0, 1),
                Color = Color.White
            };

            shadowIndices[0] = 0;
            shadowIndices[1] = 1;
            shadowIndices[2] = 2;
            shadowIndices[3] = 2;
            shadowIndices[4] = 3;
            shadowIndices[5] = 0;
            #endregion
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

                    BasePosition = new Vector2(Position.X - (float)Math.Cos(CurrentRotation) * (Texture.Width / 2),
                                               Position.Y - (float)Math.Sin(CurrentRotation) * (Texture.Width / 2));

                    TipPosition = new Vector2(Position.X + (float)Math.Cos(CurrentRotation) * (Texture.Width / 2),
                                              Position.Y + (float)Math.Sin(CurrentRotation) * (Texture.Width / 2));

                    foreach (Emitter emitter in EmitterList)
                    {
                        emitter.Position = BasePosition;
                    }

                    if (Rotate == true)
                        CurrentRotation = (float)Math.Atan2(Velocity.Y, Velocity.X);

                    DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y,
                                                         Texture.Width * (int)Scale.X, Texture.Height * (int)Scale.Y);

                    if (CurrentRotation >= 0)
                    {

                        BoundingBox = new BoundingBox(new Vector3(Position.X - ((float)Math.Cos(CurrentRotation) * (Texture.Width / 4)),
                                                                  Position.Y - ((float)Math.Sin(CurrentRotation) * (Texture.Width / 4)), 0),

                                                      new Vector3(Position.X + ((float)Math.Cos(CurrentRotation) * (Texture.Width / 4)),
                                                                  Position.Y + ((float)Math.Sin(CurrentRotation) * (Texture.Width / 4)), 0)

                                                     );
                    }
                    else
                    {
                        BoundingBox = new BoundingBox(new Vector3(Position.X + ((float)Math.Cos(CurrentRotation) * (Texture.Width / 4)),
                                                                  Position.Y + ((float)Math.Sin(CurrentRotation) * (Texture.Width / 4)), 0),

                                                      new Vector3(Position.X - ((float)Math.Cos(CurrentRotation) * (Texture.Width / 4)),
                                                                  Position.Y - ((float)Math.Sin(CurrentRotation) * (Texture.Width / 4)), 0)

                                                     );
                    }
                }
                #endregion

                Center = new Vector2(DestinationRectangle.Left, DestinationRectangle.Top);

                #region Update Vertices
                if (Velocity != Vector2.Zero)
                {
                    ShadowLength = MathHelper.Clamp(Math.Abs(TipPosition.X - BasePosition.X), Texture.Height, Texture.Width);

                    projectileVertices[0].Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top, 0);
                    projectileVertices[1].Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top, 0);
                    projectileVertices[2].Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top + DestinationRectangle.Height, 0);
                    projectileVertices[3].Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top + DestinationRectangle.Height, 0);

                    shadowVertices[0].Position = new Vector3(DestinationRectangle.Left, MaxY, 0);
                    shadowVertices[1].Position = new Vector3(DestinationRectangle.Left + ShadowLength, MaxY, 0);
                    shadowVertices[2].Position = new Vector3(DestinationRectangle.Left + ShadowLength, MaxY + DestinationRectangle.Height, 0);
                    shadowVertices[3].Position = new Vector3(DestinationRectangle.Left, MaxY + DestinationRectangle.Height, 0);

                    shadowVertices[0].Color = Color.Black * 0.85f;
                    shadowVertices[1].Color = Color.Black * 0.85f;
                    shadowVertices[2].Color = Color.Black * 0.85f;
                    shadowVertices[3].Color = Color.Black * 0.85f;
                }
                #endregion
            }

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Update(gameTime);
            }
        }

        public override void Draw(GraphicsDevice graphics, BasicEffect effect, Effect shadowEffect, SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                effect.TextureEnabled = true;
                effect.VertexColorEnabled = true;
                effect.Texture = Texture;

                #region Draw projectile shadow
                shadowEffect.Parameters["Texture"].SetValue(Texture);
                shadowEffect.Parameters["texSize"].SetValue(new Vector2(Texture.Width, Texture.Height));

                foreach (EffectPass pass in shadowEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, shadowVertices, 0, 4, shadowIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                }
                #endregion

                #region Draw projectile sprite
                effect.World = Matrix.CreateTranslation(new Vector3(-Position.X - Origin.X, -Position.Y - Origin.Y, 0)) *
                               Matrix.CreateRotationZ(CurrentRotation) *
                               Matrix.CreateTranslation(new Vector3(Position.X, Position.Y, 0));

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, projectileVertices, 0, 4, projectileIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                }
                #endregion
            }

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Draw(spriteBatch);
            }

            base.Draw(graphics, effect, shadowEffect, spriteBatch);
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
