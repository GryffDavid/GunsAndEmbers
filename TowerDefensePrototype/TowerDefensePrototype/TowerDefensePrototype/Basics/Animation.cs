using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace TowerDefensePrototype
{
    public abstract class Animation
    {
        public Texture2D Texture;
        public Vector2 FrameSize;
        public Rectangle DiffuseSourceRectangle, NormalSourceRectangle;
        public int TotalFrames = 1;
        public int CurrentFrame = 0;
        public double FrameDelay, CurrentFrameDelay;
        public bool Animated = false;
        public bool Looping = false;
        public bool Emissive = false;
        public bool Normal = true;

        public Vector2 GetFrameSize()
        {
            if (Normal == true)
            {
                FrameSize = new Vector2(Texture.Width / TotalFrames, Texture.Height / 2);
                NormalSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), (int)FrameSize.Y, (int)FrameSize.X, (int)FrameSize.Y);
            }
            else
            {
                FrameSize = new Vector2(Texture.Width / TotalFrames, Texture.Height);
            }

            DiffuseSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), 0, (int)FrameSize.X, (int)FrameSize.Y);

            return FrameSize;
        }

        public void Update(GameTime gameTime)
        {
            if (Animated == true)
            {
                CurrentFrameDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (CurrentFrameDelay > FrameDelay && TotalFrames > 1)
                {
                    CurrentFrame++;

                    if (CurrentFrame >= TotalFrames)
                    {
                        CurrentFrame = 0;

                        if (Looping == false)
                            Animated = false;
                    }

                    CurrentFrameDelay = 0;
                }
            }

            DiffuseSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), 0, (int)FrameSize.X, (int)FrameSize.Y);
            NormalSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), (int)FrameSize.Y, (int)FrameSize.X, (int)FrameSize.Y);
        }
    }
}
