using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class AnimatedSprite
    {
        public Texture2D SpriteStrip;
        public Color Color;
        public Rectangle DestinationRectangle;
        public Rectangle SourceRectangle;
        public String AssetName;
        public Vector2 Scale, FrameSize, Position;
        public int ElapsedTime, FrameTime, FrameCount, CurrentFrame;        
        public bool Active, Looping;

        public void Initialize(string assetName, Vector2 position, Vector2 frameSize, int frameCount, int frameTime, Color color, Vector2 scale, bool looping)
        {
            Color = color;
            FrameSize = frameSize;
            FrameCount = frameCount;
            FrameTime = frameTime;
            Scale = scale;

            Looping = looping;
            Position = position;

            AssetName = assetName;

            ElapsedTime = 0;
            CurrentFrame = 0;

            Active = true;
        }

        public void LoadContent(ContentManager contentManager)
        {
            SpriteStrip = contentManager.Load<Texture2D>(AssetName);
        }

        public void Update(GameTime gameTime)
        {
            if (Active == false)
                return;

            ElapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (ElapsedTime > FrameTime)
            {
                CurrentFrame++;

                if (CurrentFrame == FrameCount)
                {
                    CurrentFrame = 0;

                    if (Looping == false)
                    {
                        Active = false;
                    }
                }

                ElapsedTime = 0;
            }

            SourceRectangle = new Rectangle(CurrentFrame * (int)FrameSize.X, 0, (int)FrameSize.X, (int)FrameSize.Y);
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)FrameSize.X, (int)FrameSize.Y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
            {
                spriteBatch.Draw(SpriteStrip, DestinationRectangle, SourceRectangle, Color);
            }
        }    
    }
}
