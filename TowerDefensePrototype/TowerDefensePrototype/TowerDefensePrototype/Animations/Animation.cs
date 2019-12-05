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
    //Regular - no special mapping applied. The animation is full height (1.0f)
    //Normal - there is a normal map in the texture file, the height for each frame is halved (0.5f)
    //Emissive - there is an emissive map and a normal map in the texture file, the height for each frame is in thirds (1.0f/3.0f)
    public enum AnimationType { Regular, Normal, Emissive };

    public abstract class Animation
    {
        public Texture2D Texture;
        public Vector2 FrameSize, NormalizedFrameSize;
        public Rectangle DiffuseSourceRectangle, NormalSourceRectangle, EmissiveSourceRectangle;
        public float nFrameX;
        public int TotalFrames = 1;
        public int CurrentFrame = 0;
        public double FrameDelay, CurrentFrameDelay;
        public bool Animated = false;
        public bool Looping = false;        
        public AnimationType AnimationType = AnimationType.Regular;

        //Diffuse texture coordinates
        public Vector2 dTopLeftTexCooord, dTopRightTexCoord, dBottomRightTexCoord, dBottomLeftTexCoord;

        //Normal texture coordinates
        public Vector2 nTopLeftTexCooord, nTopRightTexCoord, nBottomRightTexCoord, nBottomLeftTexCoord;

        //Emissive texture coordinates
        public Vector2 eTopLeftTexCooord, eTopRightTexCoord, eBottomRightTexCoord, eBottomLeftTexCoord;

        public Vector2 GetFrameSize()
        {
            switch (AnimationType)
            {
                case AnimationType.Regular:
                    FrameSize = new Vector2(Texture.Width / TotalFrames, Texture.Height);
                    NormalizedFrameSize = new Vector2((float)(1f / TotalFrames), 1f);                    
                    break;

                case AnimationType.Normal:
                    FrameSize = new Vector2(Texture.Width / TotalFrames, Texture.Height / 2);
                    NormalSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), (int)FrameSize.Y, (int)FrameSize.X, (int)FrameSize.Y);

                    NormalizedFrameSize = new Vector2((float)(1f / TotalFrames), 1f/2f);
                    nTopLeftTexCooord = new Vector2(nFrameX, NormalizedFrameSize.Y);
                    nTopRightTexCoord = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y);
                    nBottomRightTexCoord = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y * 2);
                    nBottomLeftTexCoord = new Vector2(nFrameX, NormalizedFrameSize.Y * 2);
                    break;

                case AnimationType.Emissive:
                    FrameSize = new Vector2(Texture.Width / TotalFrames, Texture.Height / 3);
                    NormalSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), (int)FrameSize.Y, (int)FrameSize.X, (int)FrameSize.Y);
                    EmissiveSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), (int)(FrameSize.Y * 2), (int)FrameSize.X, (int)FrameSize.Y);

                    NormalizedFrameSize = new Vector2((float)(1f / TotalFrames), 1f / 3f);
                    eTopLeftTexCooord = new Vector2(nFrameX, NormalizedFrameSize.Y * 2);
                    eTopRightTexCoord = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y * 2);
                    eBottomRightTexCoord = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y * 3);
                    eBottomLeftTexCoord = new Vector2(nFrameX, NormalizedFrameSize.Y * 3);
                    break;
            }

            DiffuseSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), 0, (int)FrameSize.X, (int)FrameSize.Y);
            dTopLeftTexCooord = new Vector2(nFrameX, 0);
            dTopRightTexCoord = new Vector2(nFrameX + NormalizedFrameSize.X, 0);
            dBottomRightTexCoord = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y);
            dBottomLeftTexCoord = new Vector2(nFrameX, NormalizedFrameSize.Y);
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

            nFrameX = NormalizedFrameSize.X * CurrentFrame;

            NormalSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), (int)FrameSize.Y, (int)FrameSize.X, (int)FrameSize.Y);
            nTopLeftTexCooord = new Vector2(nFrameX, NormalizedFrameSize.Y);
            nTopRightTexCoord = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y);
            nBottomRightTexCoord = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y * 2);
            nBottomLeftTexCoord = new Vector2(nFrameX, NormalizedFrameSize.Y * 2);

            EmissiveSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), (int)(FrameSize.Y * 2), (int)FrameSize.X, (int)FrameSize.Y);
            eTopLeftTexCooord = new Vector2(nFrameX, NormalizedFrameSize.Y * 2);
            eTopRightTexCoord = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y * 2);
            eBottomRightTexCoord = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y * 3);
            eBottomLeftTexCoord = new Vector2(nFrameX, NormalizedFrameSize.Y * 3);
            
            DiffuseSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), 0, (int)FrameSize.X, (int)FrameSize.Y);
            dTopLeftTexCooord = new Vector2(nFrameX, 0);
            dTopRightTexCoord = new Vector2(nFrameX + NormalizedFrameSize.X, 0);
            dBottomRightTexCoord = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y);
            dBottomLeftTexCoord = new Vector2(nFrameX, NormalizedFrameSize.Y);
        }
    }
}
