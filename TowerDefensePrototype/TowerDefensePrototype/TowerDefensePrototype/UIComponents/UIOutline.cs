using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class UIOutline
    {
        Vector2 Position, Size;
        public Texture2D OutlineTexture;
        bool Visible;
        public Trap Trap;
        public Turret Turret;

        public UIOutline(Vector2 position, Vector2 size, Trap trap = null, Turret turret = null)
        {
            Position = position;
            Size = size;
            Trap = trap;
            Turret = turret;
            Visible = true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Visible == true)
            {
                //Top right
                spriteBatch.Draw(OutlineTexture,
                    new Rectangle(
                        (int)Position.X, (int)Position.Y,
                        OutlineTexture.Width, OutlineTexture.Height),
                        Color.White);

                //Top left
                spriteBatch.Draw(OutlineTexture,
                    new Rectangle(
                        (int)(Position.X + Size.X - OutlineTexture.Width), (int)Position.Y,
                        OutlineTexture.Width, OutlineTexture.Height),
                        null, Color.White, 0, new Vector2(0, 0), SpriteEffects.FlipHorizontally, 0);

                //Bottom right
                spriteBatch.Draw(OutlineTexture,
                    new Rectangle(
                        (int)(Position.X + Size.X - OutlineTexture.Width),
                        (int)(Position.Y + Size.Y - OutlineTexture.Height), OutlineTexture.Width, OutlineTexture.Height),
                        null, Color.White, MathHelper.ToRadians(180), new Vector2(OutlineTexture.Width, OutlineTexture.Height), SpriteEffects.None, 0);

                //Bottom left
                spriteBatch.Draw(OutlineTexture,
                    new Rectangle((int)Position.X, (int)(Position.Y + Size.Y - OutlineTexture.Height),
                        OutlineTexture.Width, OutlineTexture.Height),
                    null, Color.White, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);
            }
        }
    }
}
