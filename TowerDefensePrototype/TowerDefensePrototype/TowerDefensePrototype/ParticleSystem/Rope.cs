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
    class Rope : Drawable
    {
        public Texture2D StickTexture;
        float Bounce = 0.25f;
        float Gravity = 0.5f;
        float Friction = 0.95f;

        public int Segments = 110;

        public VertexPositionColorTexture[] projectileVertices = new VertexPositionColorTexture[4];
        public int[] projectileIndices = new int[6];

        public class Stick
        {
            public Node Point1, Point2;
            public float Length;
            public Rectangle DestinationRectangle;
        }

        public class Node
        {
            public Vector2 CurrentPosition, PreviousPosition, Velocity;
            public bool Tether = false;
        }

        public List<Node> Nodes = new List<Node>();
        public List<Stick> Sticks = new List<Stick>();

        public HeavyProjectile TetherProjectile;
        public Vector2 StartPoint, EndPoint;

        public Rope(Vector2 startPoint, object tether)
        {
            StartPoint = startPoint;

            if (tether as HeavyProjectile != null)
                TetherProjectile = (HeavyProjectile)tether;

            for (int i = 0; i < Segments; i++)
            {
                Nodes.Add(new Node()
                {
                    CurrentPosition = StartPoint,
                    PreviousPosition = StartPoint
                });
            }

            for (int i = 0; i < Nodes.Count - 1; i++)
            {
                Sticks.Add(new Stick()
                {
                    Point1 = Nodes[i],
                    Point2 = Nodes[i + 1],
                    Length = 10
                });
            }

        }

        public void LoadContent(ContentManager contentManager)
        {
            StickTexture = contentManager.Load<Texture2D>("RopeTexture");
        }

        public void Update(GameTime gameTime)
        {
            EndPoint = Nodes[0].CurrentPosition;

            if (TetherProjectile != null)
            {
                Nodes[0].CurrentPosition = TetherProjectile.BasePosition;
                Nodes[0].Tether = true;
            }

            if ((Segments - 1) > 0)
            {
                Nodes[Segments - 1].CurrentPosition = StartPoint;
                Nodes[Segments - 1].Tether = true;
            }


            foreach (Node node in Nodes)
            {
                if (node.Tether == false)
                {
                    node.Velocity = (node.CurrentPosition - node.PreviousPosition) * Friction;
                    node.PreviousPosition = node.CurrentPosition;
                    node.CurrentPosition += node.Velocity;
                    node.CurrentPosition.Y += Gravity;

                    #region Handle bouncing
                    if (node.CurrentPosition.X > 1920 - 8)
                    {
                        node.CurrentPosition.X = 1920 - 8;
                        node.PreviousPosition.X = (node.CurrentPosition.X + node.Velocity.X * Bounce);
                    }

                    if (node.CurrentPosition.X < -1920)
                    {
                        node.CurrentPosition.X = -1920;
                        node.PreviousPosition.X = (node.CurrentPosition.X + node.Velocity.X * Bounce);
                    }

                    if (node.CurrentPosition.Y > StartPoint.Y + 40)
                    {
                        node.CurrentPosition.Y = StartPoint.Y + 40;
                        node.PreviousPosition.Y = (node.CurrentPosition.Y + node.Velocity.Y * Bounce);
                    }

                    if (node.CurrentPosition.Y < 0)
                    {
                        node.CurrentPosition.Y = 0;
                        node.PreviousPosition.Y = (node.CurrentPosition.Y + node.Velocity.Y * Bounce);
                    }
                    #endregion
                }
            }

            for (int i = 0; i < 30; i++)
            {
                foreach (Stick stick in Sticks)
                {
                    Vector2 Direction = stick.Point2.CurrentPosition - stick.Point1.CurrentPosition;
                    float Dist = Vector2.Distance(stick.Point1.CurrentPosition, stick.Point2.CurrentPosition);
                    float Diff = stick.Length - Dist;
                    float percent = Diff / Dist / 2;
                    Vector2 Offset = Direction * percent;

                    if (stick.Point1.Tether == false)
                        stick.Point1.CurrentPosition -= Offset;

                    if (stick.Point2.Tether == false)
                        stick.Point2.CurrentPosition += Offset;

                    stick.DestinationRectangle = new Rectangle((int)stick.Point1.CurrentPosition.X, (int)stick.Point1.CurrentPosition.Y, (int)Direction.Length(), StickTexture.Height);
                }
            }
        }

        public override void Draw(GraphicsDevice graphics, BasicEffect effect)
        //public override void Draw(SpriteBatch spriteBatch)
        {

            Matrix preserveWorld = effect.World;

            effect.TextureEnabled = true;
            effect.VertexColorEnabled = true;
            effect.Texture = StickTexture;

            foreach (Stick stick in Sticks)
            {
                Vector2 Direction = stick.Point2.CurrentPosition - stick.Point1.CurrentPosition;
                Direction.Normalize();

                float rot = (float)Math.Atan2(Direction.Y, Direction.X);

                #region Draw projectile sprite
                effect.World = Matrix.CreateTranslation(new Vector3(-stick.Point1.CurrentPosition.X - 0, -stick.Point1.CurrentPosition.Y - 0, 0)) *
                               Matrix.CreateRotationZ(rot) *
                               Matrix.CreateTranslation(new Vector3(stick.Point1.CurrentPosition.X, stick.Point1.CurrentPosition.Y, 0));


                projectileVertices[0] = new VertexPositionColorTexture()
                {
                    Position = new Vector3(stick.DestinationRectangle.Left, stick.DestinationRectangle.Top, 0),
                    TextureCoordinate = new Vector2(0, 0),
                    Color = Color.White
                };

                projectileVertices[1] = new VertexPositionColorTexture()
                {
                    Position = new Vector3(stick.DestinationRectangle.Left + stick.DestinationRectangle.Width, stick.DestinationRectangle.Top, 0),
                    TextureCoordinate = new Vector2(1, 0),
                    Color = Color.White
                };

                projectileVertices[2] = new VertexPositionColorTexture()
                {
                    Position = new Vector3(stick.DestinationRectangle.Left + stick.DestinationRectangle.Width, stick.DestinationRectangle.Top + stick.DestinationRectangle.Height, 0),
                    TextureCoordinate = new Vector2(1, 1),
                    Color = Color.White
                };

                projectileVertices[3] = new VertexPositionColorTexture()
                {
                    Position = new Vector3(stick.DestinationRectangle.Left, stick.DestinationRectangle.Top + stick.DestinationRectangle.Height, 0),
                    TextureCoordinate = new Vector2(0, 1),
                    Color = Color.White
                };

                projectileIndices[0] = 0;
                projectileIndices[1] = 1;
                projectileIndices[2] = 2;
                projectileIndices[3] = 2;
                projectileIndices[4] = 3;
                projectileIndices[5] = 0;

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, projectileVertices, 0, 4, projectileIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                }
                #endregion
            }

            effect.World = preserveWorld;

            //foreach (Node node in Nodes)
            //{
            //    spriteBatch.Draw(PointTexture, new Rectangle((int)node.CurrentPosition.X, (int)node.CurrentPosition.Y, 8, 8),
            //                     null, Color.White, 0, new Vector2(4, 4), SpriteEffects.None, 0);
            //}

            //foreach (Stick stick in Sticks)
            //{
            //    //for (int l = 0; l < (int)Vector2.Distance(stick.Point1.CurrentPosition, stick.Point2.CurrentPosition); l += 10)
            //    //{
            //        Vector2 Direction = stick.Point2.CurrentPosition - stick.Point1.CurrentPosition;
            //        Direction.Normalize();

            //        spriteBatch.Draw(StickTexture, 
            //            new Rectangle(
            //                (int)(stick.Point1.CurrentPosition.X), 
            //                (int)(stick.Point1.CurrentPosition.Y), 
            //                StickTexture.Width, StickTexture.Height), 
                            
            //                null, Color.White, (float)Math.Atan2(Direction.Y, Direction.X), Vector2.Zero, SpriteEffects.None, DrawDepth);
            //    //}
            //}
        }
    }
}
