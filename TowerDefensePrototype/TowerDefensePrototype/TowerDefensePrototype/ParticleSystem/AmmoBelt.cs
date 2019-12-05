using System;
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

        public AmmoBelt(Vector2 anchorPosition, Texture2D shellTexture)
        {
            ShellTexture = shellTexture;

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
                    Length = ShellTexture.Height/2
                });
            }

            for (int i = 0; i < Nodes.Count - 1; i++)
            {
                Sticks.Add(new Stick()
                {
                    Point1 = Nodes2[i],
                    Point2 = Nodes2[i + 1],
                    Length = ShellTexture.Height / 2
                });
            }

            for (int i = 0; i < Nodes.Count - 1; i++)
            {
                Sticks2.Add(new Stick()
                {
                    Point1 = Nodes[i],
                    Point2 = Nodes2[i],
                    Length = ShellTexture.Width / 2
                });
            }



        }

        public override void Update(GameTime gameTime)
        {
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

                    if (stick.Length == ShellTexture.Width / 2)
                        spriteBatch.Draw(ShellTexture, new Rectangle((int)stick.Point1.CurrentPosition.X, (int)stick.Point1.CurrentPosition.Y, ShellTexture.Width / 2, ShellTexture.Height / 2), null, Color.White, rot + (float)Math.PI, new Vector2(ShellTexture.Width / 2, ShellTexture.Height / 2), SpriteEffects.None, 1);
                }
            }
            base.Draw(spriteBatch);
        }
    }
}
