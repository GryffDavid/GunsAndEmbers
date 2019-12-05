using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class Light
    {
        public Texture2D LightTexture;
        public Color LightColor;
        public Vector2 LightPosition, Scale;
        public string TextureName;
        public float DrawDepth;
        public Trap TrapAnchor;

        public Light(string textureName, Vector2 position, Vector2 scale, Color color)
        {
            TextureName = textureName;
            LightPosition = position;
            LightColor = color;
            Scale = scale;
        }

        public Light(Texture2D texture, Vector2 position, Vector2 scale, Color color)
        {
            LightTexture = texture;
            LightPosition = position;
            LightColor = color;
            Scale = scale;
            DrawDepth = (LightPosition.Y) / 1080f;
        }

        public Light(Texture2D texture, Vector2 position, Vector2 scale, Color color, float drawDepth, Trap trapAnchor)
        {
            LightTexture = texture;
            LightPosition = position;
            LightColor = color;
            Scale = scale;
            DrawDepth = drawDepth;
            TrapAnchor = trapAnchor;
        }

        public void LoadContent(ContentManager contentManager)
        {
            LightTexture = contentManager.Load<Texture2D>(TextureName);
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(LightTexture, 
                new Rectangle(
                    (int)LightPosition.X, (int)LightPosition.Y, 
                    (int)(LightTexture.Width*Scale.X), (int)(LightTexture.Height*Scale.Y)),
                null, LightColor, 0, new Vector2(LightTexture.Width / 2, LightTexture.Height / 2), SpriteEffects.None, DrawDepth);
        }
    }
}
