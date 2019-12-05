using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class UIInvaderInfo
    {
        public Texture2D Texture;
        public Vector2 Position;
        public bool Visible;
        public UIBar InvaderHealthBar;
        public Invader CurrentInvader;
        public SpriteFont Font;
        public Rectangle DestinationRectangle;
        public Vector2 DamageStringSize;

        public UIInvaderInfo(Invader invader)
        {
            CurrentInvader = invader;
            InvaderHealthBar = new UIBar(new Vector2(1920-390-45, 1080 - 15 - 85 - 15), new Vector2(390, 15), Color.Lerp(Color.White, Color.Transparent, 0.25f), true);
            DestinationRectangle = new Rectangle(1920 - 390 - 45, 1080 - 85 - 15, 390, 85);
        }

        public void Update(GameTime gameTime)
        {
            InvaderHealthBar.Update(CurrentInvader.MaxHP, CurrentInvader.CurrentHP);   
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            //spriteBatch.Draw(Texture, DestinationRectangle, Color.Lerp(Color.Black, Color.Transparent, 0.75f));
            InvaderHealthBar.Draw(graphicsDevice);
        }
    }
}
