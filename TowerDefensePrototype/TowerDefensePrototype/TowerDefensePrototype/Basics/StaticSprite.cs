using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class StaticSprite
    {
        public Texture2D Texture;
        string AssetName;
        public Vector2 Position, Scale, Move;
        public Color Color, StartColor;
        public Rectangle DestinationRectangle;
        public BoundingBox BoundingBox;
        public bool VerticalLooping, HorizontalLooping, FadeIn, Active;
        public double CurrentTime, UpdateDelay, FadeTime, CurrentFadeTime;
        public float Rotation, DrawDepth;
        public float Transparency = 0;

        public StaticSprite(string assetName, Vector2 position, Vector2? scale = null, Color? color = null, 
                            Vector2? move = null, bool? horizontalLooping = null, bool? verticalLooping = null, 
                            double? updateDelay = null, float? rotation = null, float? fadeTime = null, bool fadeIn = false)
        {
            Active = true;
            AssetName = assetName;
            Position = position;

            if (horizontalLooping == null)
                HorizontalLooping = false;
            else
                HorizontalLooping = horizontalLooping.Value;

            if (verticalLooping == null)
                VerticalLooping = false;
            else
                VerticalLooping = verticalLooping.Value;

            if (scale == null)
                Scale = new Vector2(1, 1);
            else
                Scale = scale.Value;

            if (color == null)
                Color = Color.White;
            else
                Color = color.Value;

            if (move == null)
                Move = Vector2.Zero;
            else
                Move = move.Value;

            if (updateDelay == null)
                UpdateDelay = 1;
            else
                UpdateDelay = updateDelay.Value;

            if (rotation == null)
                Rotation = 0;
            else
                Rotation = rotation.Value;

            if (fadeTime == null)
                FadeTime = 0;
            else
            {
                FadeTime = fadeTime.Value;
                CurrentFadeTime = 0;
            }

            StartColor = Color;
        }

        public StaticSprite(Texture2D texture, Vector2 position, Vector2? scale = null, Color? color = null,
                            Vector2? move = null, bool? horizontalLooping = null, bool? verticalLooping = null, 
                            double? updateDelay = null, float? rotation = null, float? fadeTime = null, bool fadeIn = false)
        {
            Active = true;

            //If texture ends up being null for some reason, check that the level is set to load 
            //its resources - background, ground etc. are all static sprites

            Texture = texture;
            Position = position;

            if (horizontalLooping == null)
                HorizontalLooping = false;
            else
                HorizontalLooping = horizontalLooping.Value;

            if (verticalLooping == null)
                VerticalLooping = false;
            else
                VerticalLooping = verticalLooping.Value;

            if (scale == null)
                Scale = new Vector2(1, 1);
            else
                Scale = scale.Value;

            if (color == null)
                Color = Color.White;
            else
                Color = color.Value;

            if (move == null)
                Move = Vector2.Zero;
            else
                Move = move.Value;

            if (updateDelay == null)
                UpdateDelay = 1;
            else
                UpdateDelay = updateDelay.Value;

            if (rotation == null)
                Rotation = 0;
            else
                Rotation = rotation.Value;

            if (fadeTime == null)
                FadeTime = 0;
            else
            {
                FadeTime = fadeTime.Value;
                CurrentFadeTime = 0;
            }

            StartColor = Color;
            FadeIn = fadeIn;


            //If texture ends up being null for some reason, check that the level is set to load 
            //its resources - background, ground etc. are all static sprites
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)(Texture.Width * Scale.X), (int)(Texture.Height * Scale.Y));
        }

        int[] Indices = new int[6];
        VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[4];

        public void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(AssetName);
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)(Texture.Width * Scale.X), (int)(Texture.Height * Scale.Y));

            #region This is used when drawing the static sprite using vertices
            //Originally written so that I could draw the menu background 
            //without messing up the draw order of the weapon info bars

            //Top left
            Vertices[0] = new VertexPositionColorTexture()
            {
                Position = new Vector3(Position.X, Position.Y, 0),
                TextureCoordinate = new Vector2(0,0),
                Color = Color.White
            };

            //Top right
            Vertices[1] = new VertexPositionColorTexture()
            {
                Position = new Vector3(Position.X + DestinationRectangle.Width, Position.Y, 0),
                TextureCoordinate = new Vector2(1, 0),
                Color = Color.White
            };

            //Bottom right
            Vertices[2] = new VertexPositionColorTexture()
            {
                Position = new Vector3(Position.X + DestinationRectangle.Width, Position.Y + DestinationRectangle.Height, 0),
                TextureCoordinate = new Vector2(1, 1),
                Color = Color.White
            };

            //Bottom left
            Vertices[3] = new VertexPositionColorTexture()
            {
                Position = new Vector3(Position.X, Position.Y + DestinationRectangle.Height, 0),
                TextureCoordinate = new Vector2(0, 1),
                Color = Color.White
            };

            Indices[0] = 0;
            Indices[1] = 1;
            Indices[2] = 2;
            Indices[3] = 2;
            Indices[4] = 3;
            Indices[5] = 0;
            #endregion
        }

        public void Update(GameTime gameTime)
        {
            CurrentTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentFadeTime < FadeTime)
                CurrentFadeTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            //if (CurrentFadeTime > 10 && Transparency <= 1)
            //{
            //    Transparency += 0.01f;
            //    Color = Color.Lerp(Color.White, Color.Transparent, Transparency);
            //    CurrentFadeTime = 0;
            //}

            double PercentFade = CurrentFadeTime / FadeTime;

            if (FadeIn == false)
            {
                if (FadeTime > 0)
                {
                    Color = Color.Lerp(Color.White, Color.Transparent, (float)(CurrentFadeTime / FadeTime));
                }
            }
            else
            {
                if (FadeTime > 0)
                {
                    float percent = (float)((CurrentFadeTime / FadeTime) * Math.PI);
                    float SinPercent = (float)Math.Sin(percent);
                    Color = Color.Lerp(Color.Transparent, StartColor, SinPercent);
                    //Color = Color.White;
                }
            }

            if (CurrentFadeTime > FadeTime)
            {
                Color = Color.Transparent;
                Active = false;
            }


            if (CurrentTime > UpdateDelay)
            {                
                if (HorizontalLooping == true && DestinationRectangle.Left > 1280 && Move.X > 0)
                {
                    Position.X = 0 - DestinationRectangle.Width;
                }

                if (HorizontalLooping == true && DestinationRectangle.Right < 0 && Move.X < 0)
                {
                    Position.X = 1920;
                }

                if (VerticalLooping == true && DestinationRectangle.Top > 720 && Move.Y > 0)
                {
                    Position.Y = 0 - DestinationRectangle.Height;
                }

                if (VerticalLooping == true && DestinationRectangle.Bottom < 0 && Move.Y < 0)
                {
                    Position.Y = 1080;
                }

                Position += Move;

                CurrentTime = 0;
            }

            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)(Texture.Width * Scale.X), (int)(Texture.Height * Scale.Y));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)(Texture.Width * Scale.X), (int)(Texture.Height * Scale.Y));
            BoundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, 0), new Vector3(Position.X + (Texture.Width * Scale.X), Position.Y + (Texture.Width * Scale.Y), 0));
            spriteBatch.Draw(Texture, DestinationRectangle, null, Color, Rotation, Vector2.Zero, SpriteEffects.None, DrawDepth);
        }

        public void Draw(GraphicsDevice graphics, BasicEffect basicEffect)
        {
            basicEffect.Texture = Texture;
            basicEffect.TextureEnabled = true;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, 4, Indices, 0, 2);
            }

            basicEffect.TextureEnabled = false;
        }
    }
}
