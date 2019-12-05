﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class Animation
    {
        public Texture2D Texture;
        public Vector2 FrameSize;
        public Rectangle SourceRectangle;
        public int TotalFrames;
        public int CurrentFrame = 0;
        public double FrameDelay, CurrentFrameDelay;
        public bool Animated = false;
        public bool Looping = false;        

        public Vector2 GetFrameSize()
        {
            FrameSize = new Vector2(Texture.Width / TotalFrames, Texture.Height);
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

            SourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), 0, (int)FrameSize.X, (int)FrameSize.Y);
        }
    }
}
