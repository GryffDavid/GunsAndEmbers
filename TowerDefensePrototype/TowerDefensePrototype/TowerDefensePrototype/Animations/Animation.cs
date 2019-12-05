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
        public GraphicsType AnimationType = GraphicsType.Diffuse;

        public TexCoord DiffuseCoords, NormalCoords, EmissiveCoords;
        public Rectangle DiffuseSourceRectangle, NormalSourceRectangle, EmissiveSourceRectangle;

        public Texture2D Texture;

        /// <summary>
        /// The actual size in pixels of the frame
        /// </summary>
        public Vector2 FrameSize;

        /// <summary>
        /// The Frame Size based on a value out of 1.0f. Just like in a shader.
        /// </summary>
        public Vector2 NormalizedFrameSize;

        /// <summary>
        /// The current frame X position as a value out of 1.0f
        /// </summary>
        public float nFrameX;

        public int TotalFrames = 1;
        public int CurrentFrame = 0;
        public double FrameDelay, CurrentFrameDelay;
        public bool Animated = false;
        public bool Looping = false;

        public Vector2 GetFrameSize()
        {
            switch (AnimationType)
            {
                #region Diffuse Only
                case GraphicsType.Diffuse:
                    FrameSize = new Vector2(Texture.Width / TotalFrames, Texture.Height);
                    NormalizedFrameSize = new Vector2((float)(1f / TotalFrames), 1f);
                    break; 
                #endregion

                #region Normal
                case GraphicsType.Normal:
                    FrameSize = new Vector2(Texture.Width / TotalFrames, Texture.Height / 2);
                    NormalizedFrameSize = new Vector2((float)(1f / TotalFrames), 1f / 2f);

                    EmissiveSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), (int)FrameSize.Y, (int)FrameSize.X, (int)FrameSize.Y);

                    EmissiveCoords.TopLeft = new Vector2(nFrameX, NormalizedFrameSize.Y);
                    EmissiveCoords.TopRight = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y);
                    EmissiveCoords.BottomRight = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y * 2);
                    EmissiveCoords.BottomLeft = new Vector2(nFrameX, NormalizedFrameSize.Y * 2);
                    break; 
                #endregion

                #region Emissive
                case GraphicsType.Emissive:
                    FrameSize = new Vector2(Texture.Width / TotalFrames, Texture.Height / 2);
                    NormalizedFrameSize = new Vector2((float)(1f / TotalFrames), 1f / 2f);

                    NormalSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), (int)FrameSize.Y, (int)FrameSize.X, (int)FrameSize.Y);

                    NormalCoords.TopLeft = new Vector2(nFrameX, NormalizedFrameSize.Y);
                    NormalCoords.TopRight = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y);
                    NormalCoords.BottomRight = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y * 2);
                    NormalCoords.BottomLeft = new Vector2(nFrameX, NormalizedFrameSize.Y * 2);
                    break; 
                #endregion

                #region Normal AND Emissive
                case GraphicsType.NormalEmissive:
                    FrameSize = new Vector2(Texture.Width / TotalFrames, Texture.Height / 3);
                    NormalizedFrameSize = new Vector2((float)(1f / TotalFrames), 1f / 3f);

                    NormalSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), (int)FrameSize.Y, (int)FrameSize.X, (int)FrameSize.Y);
                    EmissiveSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), (int)(FrameSize.Y * 2), (int)FrameSize.X, (int)FrameSize.Y);

                    NormalCoords.TopLeft = new Vector2(nFrameX, NormalizedFrameSize.Y);
                    NormalCoords.TopRight = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y);
                    NormalCoords.BottomRight = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y * 2);
                    NormalCoords.BottomLeft = new Vector2(nFrameX, NormalizedFrameSize.Y * 2);

                    EmissiveCoords.TopLeft = new Vector2(nFrameX, NormalizedFrameSize.Y * 2);
                    EmissiveCoords.TopRight = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y * 2);
                    EmissiveCoords.BottomRight = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y * 3);
                    EmissiveCoords.BottomLeft = new Vector2(nFrameX, NormalizedFrameSize.Y * 3);
                    break; 
                #endregion
            }

            nFrameX = NormalizedFrameSize.X * CurrentFrame;

            DiffuseSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), 0, (int)FrameSize.X, (int)FrameSize.Y);
            DiffuseCoords.TopLeft = new Vector2(nFrameX, 0);
            DiffuseCoords.TopRight = new Vector2(nFrameX + NormalizedFrameSize.X, 0);
            DiffuseCoords.BottomRight = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y);
            DiffuseCoords.BottomLeft = new Vector2(nFrameX, NormalizedFrameSize.Y);

            return FrameSize;
        }

        public void Update(GameTime gameTime)
        {
            #region Update timing and if necessary advance the frame
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
            #endregion

            nFrameX = NormalizedFrameSize.X * CurrentFrame;

            //THIS WHOLE SECTION COULD CAUSE PROBLEMS BECAUSE I CHANGED THE APPROACH TO DIFFERENT MAPS
            //THE EMISSIVE AND NORMAL MAPS ARE NOW ASSUMED BOTH TO BE ONE STEP DOWN 
            //THE EMISSIVE MAP WILL ONLY APPEAR 2 STEPS DOWN IF THERE IS ALSO A NORMAL MAP
            //i.e. THE GRAPHICS TYPE IS SET TO GraphicsType.NormalEmissive

            #region COULD BE A PROBLEM
            NormalSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), (int)FrameSize.Y, (int)FrameSize.X, (int)FrameSize.Y);
            NormalCoords.TopLeft = new Vector2(nFrameX, NormalizedFrameSize.Y);
            NormalCoords.TopRight = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y);
            NormalCoords.BottomRight = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y * 2);
            NormalCoords.BottomLeft = new Vector2(nFrameX, NormalizedFrameSize.Y * 2);

            EmissiveSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), (int)(FrameSize.Y * 2), (int)FrameSize.X, (int)FrameSize.Y);
            EmissiveCoords.TopLeft = new Vector2(nFrameX, NormalizedFrameSize.Y * 2);
            EmissiveCoords.TopRight = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y * 2);
            EmissiveCoords.BottomRight = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y * 3);
            EmissiveCoords.BottomLeft = new Vector2(nFrameX, NormalizedFrameSize.Y * 3);

            DiffuseSourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), 0, (int)FrameSize.X, (int)FrameSize.Y);
            DiffuseCoords.TopLeft = new Vector2(nFrameX, 0);
            DiffuseCoords.TopRight = new Vector2(nFrameX + NormalizedFrameSize.X, 0);
            DiffuseCoords.BottomRight = new Vector2(nFrameX + NormalizedFrameSize.X, NormalizedFrameSize.Y);
            DiffuseCoords.BottomLeft = new Vector2(nFrameX, NormalizedFrameSize.Y); 
            #endregion
        }

        public Animation ShallowCopy()
        {
            return (Animation) this.MemberwiseClone();
        }
    }
}
