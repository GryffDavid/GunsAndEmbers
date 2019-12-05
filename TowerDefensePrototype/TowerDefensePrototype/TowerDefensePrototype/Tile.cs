using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public enum TileState { Open, Solid, Path, FinalPath, StartPos, EndPos };

    public class Tile
    {
        Texture2D TileTexture, CircleTexture;
        public TileState TileState = TileState.Open;

        public Vector2 Position, Size;
        public Rectangle DestinationRectangle;
        Color Color = Color.Transparent;

        public Tile(Vector2 index)
        {
            Size = new Vector2(32, 32);
            Position = new Vector2(272 + (Size.X * index.X), 672 + (Size.Y * index.Y));
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        }

        public void LoadContent(ContentManager content)
        {
            TileTexture = content.Load<Texture2D>("Block");
            CircleTexture = content.Load<Texture2D>("Circle");
        }

        public void Update()
        {
            switch (TileState)
            {
                case TileState.Path:
                    {
                        Color = Color.Purple;
                    }
                    break;

                case TileState.StartPos:
                    {
                        Color = Color.Red;
                    }
                    break;

                case TileState.EndPos:
                    {
                        Color = Color.Green;
                    }
                    break;

                case TileState.FinalPath:
                    {
                        Color = Color.Turquoise;
                    }
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TileTexture, DestinationRectangle, Color);
        }
    }
}
