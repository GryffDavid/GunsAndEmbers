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
        Texture2D Texture;
        string AssetName;
        public Vector2 Position, Scale, Move;
        Color Color;
        Rectangle DestinationRectangle;
        public BoundingBox BoundingBox;
        public bool VerticalLooping, HorizontalLooping;
        public double CurrentTime, UpdateDelay;

        public StaticSprite(string assetName, Vector2 position, Vector2? scale = null, Color? color = null, Vector2? move = null, bool? horizontalLooping = null, bool? verticalLooping = null, double? updateDelay = null)
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
        }

        public void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(AssetName);
        }

        public void Update(GameTime gameTime)
        {
            CurrentTime += gameTime.ElapsedGameTime.TotalMilliseconds;

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
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)(Texture.Width * Scale.X), (int)(Texture.Height * Scale.Y));
            BoundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, 0), new Vector3(Position.X + (Texture.Width * Scale.X), Position.Y + (Texture.Width * Scale.Y), 0));
            spriteBatch.Draw(Texture, DestinationRectangle, Color);
        }
    }
}
