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
        public Vector2 Scale;
        public Texture2D ShellTexture;
        static Random Random = new Random();

        float DelayTime;
        float MaxDelayTime = 4000f;

        float CurrentTime, MaxTime;
        float Transparency;

        public Color Color = Color.White;
        public float ShrinkDelay = 2000f; //Only shrink after 2 seconds have passed

        int sourceHeight;

        Rectangle SourceRectangle;
                
        public ShellCasing(Vector2 position, Vector2 velocity, Texture2D shellTexture, Vector2? scale = null, Vector2? yRange = null)
        {
            Active = true;

            CurrentTime = 0;
            MaxTime = 8000;

            //100/MaxTime * CurrentTime

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

            if (scale == null)            
                Scale = new Vector2(1, 1);
            else            
                Scale = scale.Value;

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
                    if (CurrentTime > 100)
                    {
                        MaxY++;
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
                    if (CurrentTime > 100)
                    {
                        //MaxY++;
                        sourceHeight++;

                        //foreach (Node node in Nodes)
                        //{
                        //    node.CurrentPosition.Y++;
                        //    node.PreviousPosition.Y++;
                        //}

                        //foreach (Node node in Nodes2)
                        //{
                        //    node.CurrentPosition.Y++;
                        //    node.PreviousPosition.Y++;
                        //}
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
                        (int)stick.Point1.CurrentPosition.X, (int)stick.Point1.CurrentPosition.Y,
                        (int)(ShellTexture.Width), (int)(SourceRectangle.Height));
            }


            Color = Color.Lerp(Color.White, Color.Transparent, Transparency);            
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (Stick stick in Sticks)
            {
                spriteBatch.Draw(ShellTexture, stick.DestinationRectangle, SourceRectangle, 
                    Color, stick.Rotation, new Vector2(0, ShellTexture.Height / 2), SpriteEffects.None, 0);
            }
        }
    }
}
