using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    struct ResourceChange
    {
        public Vector2 Position;
        public string Text;
        public float Transparency;
    }

    class ResourceCounter
    {
        SpriteFont Font;
        List<ResourceChange> Changes;

        public ResourceCounter()
        {

        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {

        }

        public void AddChange(int diff)
        {
            Changes.Add(new ResourceChange() { Text = diff.ToString() });
        }
    }
}
