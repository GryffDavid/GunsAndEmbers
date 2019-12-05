using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class ShellCasing : VerletObject
    {
        //public VertexPositionColorTexture[] shellVertices = new VertexPositionColorTexture[4];
        //public int[] shellIndices = new int[6];

        public Texture2D ShellTexture;
        static Random Random = new Random();

        float DelayTime;
        float MaxDelayTime = 4000f;

        float CurrentTime;
        //public Color Color = Color.White;

        int sourceHeight;
        //Rectangle SourceRectangle;
                
        public ShellCasing(Vector2 position, Vector2 velocity, Texture2D shellTexture, Vector2? yRange = null)
        {
            Active = true;
            CurrentTime = 0;
            
            ShellTexture = shellTexture;

            Nodes.Add(new Node()
            {
                CurrentPosition = position,
                PreviousPosition = position,
                Pinned = false
            });

            Nodes.Add(new Node()
            {
                CurrentPosition = position + new Vector2(7, 7),
                PreviousPosition = position - velocity,
                Pinned = false
            });

            Sticks.Add(new Stick()
            { 
                Length = ShellTexture.Width/2, 
                Point1 = Nodes[0], 
                Point2 = Nodes[1]
            });

            if (yRange == null)
            {
                YRange = new Vector2(870, 870 + 80);
            }
            else
            {
                YRange = yRange.Value;
            }

            sourceHeight = ShellTexture.Height;
            SourceRectangle = new Rectangle(0, 0, ShellTexture.Width, ShellTexture.Height);
        }
        
        public override void Update(GameTime gameTime)
        {
            if (DelayTime < MaxDelayTime)
            {
                DelayTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (Sticks[0].Rotation < MathHelper.ToRadians(90))
                {
                    sourceHeight = ShellTexture.Height;
                }
                else
                {
                    sourceHeight = 0;
                }
            }

            if (DelayTime >= MaxDelayTime)
            {
                CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (Sticks[0].Rotation < MathHelper.ToRadians(90))
                {
                    if (CurrentTime > (100 / (ShellTexture.Height / 7)))
                    {
                        BounceY++;
                        sourceHeight--;

                        foreach (Node node in Nodes)
                        {
                            node.CurrentPosition.Y++;
                            node.PreviousPosition.Y++;
                        }

                        foreach (Node node in Nodes2)
                        {
                            node.CurrentPosition.Y++;
                            node.PreviousPosition.Y++;
                        }
                        CurrentTime = 0;
                    }

                    SourceRectangle = new Rectangle(0, 0, ShellTexture.Width, sourceHeight);

                    if (sourceHeight <= 0)
                    {
                        Active = false;
                    }
                }
                else
                {
                    if (CurrentTime > (100 / (ShellTexture.Height / 7)))
                    {
                        sourceHeight++;
                        CurrentTime = 0;
                    }

                    SourceRectangle = new Rectangle(0, sourceHeight, ShellTexture.Width, ShellTexture.Height - sourceHeight);

                    if (sourceHeight > ShellTexture.Height)
                    {
                        Active = false;
                    }
                }
            }
            
            foreach (Stick stick in Sticks)
            {
                Vector2 dir = stick.Point2.CurrentPosition - stick.Point1.CurrentPosition;
                float rot = (float)Math.Atan2(dir.Y, dir.X);

                stick.Rotation = rot;
                
                stick.DestinationRectangle = new Rectangle(
                        (int)stick.Point1.CurrentPosition.X, 
                        (int)stick.Point1.CurrentPosition.Y,
                        (int)(ShellTexture.Width), (int)(SourceRectangle.Height));
            }


            Color = Color.White;          
            base.Update(gameTime);
        }

        public override void Draw(GraphicsDevice graphics, BasicEffect effect)
        {
            Matrix preserveWorld = effect.World;

            effect.TextureEnabled = true;
            effect.VertexColorEnabled = true;
            effect.Texture = ShellTexture;

            foreach (Stick stick in Sticks)
            {
                Vector2 Direction = stick.Point2.CurrentPosition - stick.Point1.CurrentPosition;
                Direction.Normalize();

                float rot = (float)Math.Atan2(Direction.Y, Direction.X);

                if (SourceRectangle.Height > 0)
                {
                    #region Draw rope segments
                    effect.World = Matrix.CreateTranslation(new Vector3(-(stick.Point1.CurrentPosition.X + ShellTexture.Width / 2), -(stick.Point1.CurrentPosition.Y + ShellTexture.Height / 2), 0)) *
                                   Matrix.CreateRotationZ(rot) *
                                   Matrix.CreateTranslation(new Vector3(stick.Point1.CurrentPosition.X, stick.Point1.CurrentPosition.Y, 0));

                    //TEXTURE COORDINATES NEED TO BE NORMALIZED FOR THE SHADER. i.e. ShellTexture.Height/2 = 0.5f;

                    if (Sticks[0].Rotation < MathHelper.ToRadians(90))
                    {
                        vertices[0] = new VertexPositionColorTexture()
                        {
                            Position = new Vector3(stick.DestinationRectangle.Left, stick.DestinationRectangle.Top, 0),
                            TextureCoordinate = new Vector2(0, ((float)SourceRectangle.Y / (float)ShellTexture.Height)),
                            Color = Color.White
                        };

                        vertices[1] = new VertexPositionColorTexture()
                        {
                            Position = new Vector3(stick.DestinationRectangle.Left + stick.DestinationRectangle.Width, stick.DestinationRectangle.Top, 0),
                            TextureCoordinate = new Vector2(1, ((float)SourceRectangle.Y / (float)ShellTexture.Height)),
                            Color = Color.White
                        };

                        vertices[2] = new VertexPositionColorTexture()
                        {
                            Position = new Vector3(stick.DestinationRectangle.Left + stick.DestinationRectangle.Width, stick.DestinationRectangle.Top + stick.DestinationRectangle.Height, 0),
                            TextureCoordinate = new Vector2(1, ((float)SourceRectangle.Height / (float)ShellTexture.Height)),
                            Color = Color.White
                        };

                        vertices[3] = new VertexPositionColorTexture()
                        {
                            Position = new Vector3(stick.DestinationRectangle.Left, stick.DestinationRectangle.Top + stick.DestinationRectangle.Height, 0),
                            TextureCoordinate = new Vector2(0, ((float)SourceRectangle.Height / (float)ShellTexture.Height)),
                            Color = Color.White
                        };
                    }
                    else
                    {
                        vertices[0] = new VertexPositionColorTexture()
                        {
                            Position = new Vector3(stick.DestinationRectangle.Left, stick.DestinationRectangle.Top, 0),
                            TextureCoordinate = new Vector2(0, ((float)SourceRectangle.Y / (float)ShellTexture.Height)),
                            Color = Color.White
                        };

                        vertices[1] = new VertexPositionColorTexture()
                        {
                            Position = new Vector3(stick.DestinationRectangle.Left + stick.DestinationRectangle.Width, stick.DestinationRectangle.Top, 0),
                            TextureCoordinate = new Vector2(1, ((float)SourceRectangle.Y / (float)ShellTexture.Height)),
                            Color = Color.White
                        };

                        vertices[2] = new VertexPositionColorTexture()
                        {
                            Position = new Vector3(stick.DestinationRectangle.Left + stick.DestinationRectangle.Width, stick.DestinationRectangle.Top + stick.DestinationRectangle.Height, 0),
                            TextureCoordinate = new Vector2(1, 1),
                            Color = Color.White
                        };

                        vertices[3] = new VertexPositionColorTexture()
                        {
                            Position = new Vector3(stick.DestinationRectangle.Left, stick.DestinationRectangle.Top + stick.DestinationRectangle.Height, 0),
                            TextureCoordinate = new Vector2(0, 1),
                            Color = Color.White
                        };
                    }

                    indices[0] = 0;
                    indices[1] = 1;
                    indices[2] = 2;
                    indices[3] = 2;
                    indices[4] = 3;
                    indices[5] = 0;

                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                    }
                }
                #endregion
            }

            effect.World = preserveWorld;

            //foreach (Stick stick in Sticks)
            //{
            //    spriteBatch.Draw(ShellTexture, stick.DestinationRectangle, SourceRectangle, 
            //        Color, stick.Rotation, new Vector2(0, ShellTexture.Height / 2), SpriteEffects.None, 0);
            //}
        }
    }
}
