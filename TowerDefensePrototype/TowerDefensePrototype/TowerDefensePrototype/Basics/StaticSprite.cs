﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class StaticSprite : Drawable
    {
        public Texture2D Texture;
        string AssetName;
        public Vector2 Position, Scale, Move;
        public Color Color;
        public Rectangle DestinationRectangle;
        public BoundingBox BoundingBox;
        public bool VerticalLooping, HorizontalLooping;
        public double CurrentTime, UpdateDelay, FadeTime, CurrentFadeTime;
        public float Rotation;
        public float Transparency = 0;

        public StaticSprite(string assetName, Vector2 position, Vector2? scale = null, Color? color = null, 
            Vector2? move = null, bool? horizontalLooping = null, bool? verticalLooping = null, double? updateDelay = null, float? rotation = null, float? fadeTime = null)
        {
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
        }

        public StaticSprite(Texture2D texture, Vector2 position, Vector2? scale = null, Color? color = null,
                            Vector2? move = null, bool? horizontalLooping = null, bool? verticalLooping = null, double? updateDelay = null, float? rotation = null, float? fadeTime = null)
        {
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

            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)(Texture.Width * Scale.X), (int)(Texture.Height * Scale.Y));
        }

        public void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(AssetName);
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)(Texture.Width * Scale.X), (int)(Texture.Height * Scale.Y));
        }

        public void Update(GameTime gameTime)
        {
            CurrentTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            CurrentFadeTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentFadeTime > 10 && Transparency <= 1)
            {
                Transparency += 0.01f;
                Color = Color.Lerp(Color.White, Color.Transparent, Transparency);
                CurrentFadeTime = 0;
            }

            if (CurrentTime > UpdateDelay)
            {                
                if (HorizontalLooping == true && DestinationRectangle.Left > 1280 && Move.X > 0)
                {
                    Position.X = 0 - DestinationRectangle.Width;
                }

                if (HorizontalLooping == true && DestinationRectangle.Right < 0 && Move.X < 0)
                {
                    Position.X = 1280;
                }

                if (VerticalLooping == true && DestinationRectangle.Top > 720 && Move.Y > 0)
                {
                    Position.Y = 0 - DestinationRectangle.Height;
                }

                if (VerticalLooping == true && DestinationRectangle.Bottom < 0 && Move.Y < 0)
                {
                    Position.Y = 720;
                }

                Position += Move;

                CurrentTime = 0;
            }

            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)(Texture.Width * Scale.X), (int)(Texture.Height * Scale.Y));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)(Texture.Width * Scale.X), (int)(Texture.Height * Scale.Y));
            BoundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, 0), new Vector3(Position.X + (Texture.Width * Scale.X), Position.Y + (Texture.Width * Scale.Y), 0));
            spriteBatch.Draw(Texture, DestinationRectangle, null, Color, Rotation, Vector2.Zero, SpriteEffects.None, DrawDepth);
        }
    }
}
