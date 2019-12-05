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

        float CurrentTime, MaxTime;
        float Transparency;

        public Color Color = Color.White;
        public float ShrinkDelay = 2000f; //Only shrink after 2 seconds have passed
                
        public ShellCasing(Vector2 position, Vector2 velocity, Texture2D shellTexture, Vector2? scale = null)
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
            
        }

        public override void Update(GameTime gameTime)
        {
            CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            Transparency = ((100 / MaxTime) * CurrentTime)/100;

            if (Transparency >= 1)
            {
                Active = false;
            }

            foreach (Stick stick in Sticks)
            {
                Vector2 dir = stick.Point2.CurrentPosition - stick.Point1.CurrentPosition;
                float rot = (float)Math.Atan2(dir.Y, dir.X);

                stick.Rotation = rot;

                stick.DestinationRectangle = new Rectangle(
                        (int)stick.Point1.CurrentPosition.X, (int)stick.Point1.CurrentPosition.Y,
                        (int)(ShellTexture.Width * Scale.X), (int)(ShellTexture.Height * Scale.Y));
            }

            Color = Color.Lerp(Color.White, Color.Transparent, Transparency);            
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (Stick stick in Sticks)
            {
                spriteBatch.Draw(ShellTexture, stick.DestinationRectangle, null, Color, stick.Rotation, 
                                 new Vector2(0, ShellTexture.Height / 2), SpriteEffects.None, 0);
            }
        }
    }
}
