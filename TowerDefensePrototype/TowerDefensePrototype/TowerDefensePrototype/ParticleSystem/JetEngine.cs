using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class JetEngine : Drawable
    {
        public Texture2D JetSprite;
        public JetTrail JetEffect;

        public Vector2 Position;
        public float Rotation;
        public object Tether;

        public JetEngine()
        {
            Rotation = 45;
            Position = new Vector2(500, 500);
            JetEffect = new JetTrail();
        }

        public void Update(GameTime gameTime, Vector2 position)
        {
            Position = position;

            JetEffect.Update(gameTime, Position, 45);
        }

        public override void Draw(GraphicsDevice graphics, BasicEffect effect, Effect shadowEffect, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(JetSprite, new Rectangle((int)Position.X, (int)Position.Y, JetSprite.Width / 2, JetSprite.Height / 2),
                             null, Color.White, MathHelper.ToRadians(Rotation),
                             new Vector2(JetSprite.Width, JetSprite.Height / 2), SpriteEffects.None, 0);

            JetEffect.Draw(graphics, effect);
            base.Draw(graphics, effect, shadowEffect, spriteBatch);
        }
    }
}
