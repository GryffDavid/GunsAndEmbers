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
        public Texture2D ShellTexture;
        static Random Random = new Random();

        float CurrentTime, MaxTime;
        float Transparency;
                
        public ShellCasing(Vector2 position, Vector2 velocity, Texture2D shellTexture)
        {
            CurrentTime = 0;
            MaxTime = 8000;

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
                PreviousPosition = position - new Vector2(Random.Next(-15, -8), Random.Next(-15, -8)),
                Pinned = false
            });

            Sticks.Add(new Stick() 
            { 
                Length = ShellTexture.Width/2, 
                Point1 = Nodes[0], 
                Point2 = Nodes[1]
            });
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //Calculate rotation of shell from verlet object
            //Draw shell with calculated rotation
            foreach (Stick stick in Sticks)
            {
                Vector2 dir = stick.Point2.CurrentPosition - stick.Point1.CurrentPosition;
                float rot = (float)Math.Atan2(dir.Y, dir.X);

                spriteBatch.Draw(ShellTexture, new Rectangle((int)stick.Point1.CurrentPosition.X, (int)stick.Point1.CurrentPosition.Y, ShellTexture.Width/2, ShellTexture.Height/2), null, Color.White, rot, new Vector2(0, ShellTexture.Height / 2), SpriteEffects.None, 0);
            }
        }
    }
}
