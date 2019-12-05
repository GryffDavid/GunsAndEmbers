using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace TowerDefensePrototype
{
    class Shadow
    {
        public Texture2D Texture;
        public Vector2 Position, Scale;
        public float Rotation;
        public string TextureName;

        public Shadow(string textureName, Vector2 position)
        {
            Position = position;
            TextureName = textureName;
        }

        public void Update(Vector2 position, float rotation, Vector2 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(TextureName);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height), null, Color.Black, Rotation, new Vector2(Texture.Width / 2, Texture.Height / 2), SpriteEffects.None, 0);
        }
    }
}
