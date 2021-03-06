﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TowerDefensePrototype
{
    public class AmmoBelt : VerletObject
    {
        public Texture2D ShellTexture;
        public float CurrentTime, MaxTime, Transparency;

        public AmmoBelt(Vector2 anchorPosition, Texture2D shellTexture)
        {
            Active = true;
            ShellTexture = shellTexture;
            Transparency = 1.0f;

            Nodes.Add(new Node()
            {
                CurrentPosition = anchorPosition,
                PreviousPosition = anchorPosition,
                Pinned = true
            });

            for (int i = 0; i < 15; i++)
            {
                Nodes.Add(new Node()
                {
                    CurrentPosition = anchorPosition,
                    PreviousPosition = anchorPosition,
                    Pinned = false
                });
            }

            Nodes2.Add(new Node()
            {
                CurrentPosition = anchorPosition,
                PreviousPosition = anchorPosition,
                Pinned = true
            });

            for (int i = 0; i < 15; i++)
            {
                Nodes2.Add(new Node()
                {
                    CurrentPosition = anchorPosition,
                    PreviousPosition = anchorPosition,
                    Pinned = false
                });
            }

            for (int i = 0; i < Nodes.Count - 1; i++)
            {
                Sticks.Add(new Stick()
                {
                    Point1 = Nodes[i],
                    Point2 = Nodes[i + 1],
                    Length = ShellTexture.Height-2
                });
            }

            for (int i = 0; i < Nodes.Count - 1; i++)
            {
                Sticks.Add(new Stick()
                {
                    Point1 = Nodes2[i],
                    Point2 = Nodes2[i + 1],
                    Length = ShellTexture.Height-2
                });
            }

            for (int i = 0; i < Nodes.Count - 1; i++)
            {
                Sticks2.Add(new Stick()
                {
                    Point1 = Nodes[i],
                    Point2 = Nodes2[i],
                    Length = ShellTexture.Width
                });
            }

            //Update(new GameTime());

            foreach (Node node in Nodes)
            {
                node.CurrentPosition = anchorPosition;
            }

            foreach (Node node in Nodes2)
            {
                node.CurrentPosition = anchorPosition;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (MaxTime > 0)
            {
                CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                Transparency = 1.0f - (((100 / MaxTime) * CurrentTime) / 100);

                if (CurrentTime > MaxTime)
                {
                    Active = false;
                }
            }
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (Stick stick in Sticks2)
            {
                if (Sticks2.IndexOf(stick) > 0)
                {
                    Vector2 dir = stick.Point2.CurrentPosition - stick.Point1.CurrentPosition;
                    float rot = (float)Math.Atan2(dir.Y, dir.X);

                    if (stick.Length == ShellTexture.Width)
                        spriteBatch.Draw(ShellTexture, 
                            new Rectangle(
                                (int)stick.Point1.CurrentPosition.X, 
                                (int)stick.Point1.CurrentPosition.Y, 
                                ShellTexture.Width, ShellTexture.Height),
                            null, Color.White * Transparency, rot + (float)Math.PI, new Vector2(ShellTexture.Width/2, ShellTexture.Height), SpriteEffects.None, 1);
                }
            }

            base.Draw(spriteBatch);
        }
    }
}
