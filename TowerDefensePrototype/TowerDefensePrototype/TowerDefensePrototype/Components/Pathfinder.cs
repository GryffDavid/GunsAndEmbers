using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class Pathfinder
    {
        Texture2D GridTexture, BlockTexture;
        Vector2 GridSize = new Vector2(16, 16);

        //As the pathfinder is created it gets a list of all the possible obstacles and then
        //returns a path
        //If another obstacle is placed, the pathfinder is reset with new start position data
        //and new obstacle data
        //The "map" is regenerated for the pathfinder everytime a pathfinder object is created
        public Pathfinder()
        {

        }

        public void LoadContent(ContentManager content)
        {
            GridTexture = content.Load<Texture2D>("GridTexture");
            BlockTexture = content.Load<Texture2D>("Block");
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < 103; x++)
            {
                for (int y = 0; y < 18; y++)
                {
                    spriteBatch.Draw(GridTexture, new Vector2(272 + (x * GridSize.X), 672 + (y * GridSize.Y)), Color.Black);
                }
            }
        }
    }
}
