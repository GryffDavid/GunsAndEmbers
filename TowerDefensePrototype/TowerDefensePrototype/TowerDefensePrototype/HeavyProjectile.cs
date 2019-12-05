using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class HeavyProjectile
    {
        Texture2D Texture;
        Vector2 Velocity, Position;
        string TextureName;
        float Gravity;

        public HeavyProjectile(string textureName, Vector2 position, float speed, float angle, float gravity)
        {
            TextureName = textureName;
            Velocity.X = (float)(Math.Sin(angle) * speed);
            Velocity.Y = (float)(Math.Cos(angle) * speed);
            Gravity = gravity;
        }

        public void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(TextureName);
        }

        public void Update()
        {
            Position += Velocity;
            Velocity.Y += Gravity;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height), Color.White);
        }
    }
}
