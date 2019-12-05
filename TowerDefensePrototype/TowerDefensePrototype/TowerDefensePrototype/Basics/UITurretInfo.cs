using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class UITurretInfo
    {
        public Texture2D Texture, TurretIconTexture, DamageIconTexture, OverHeatIconTexture;
        public Vector2 Position;
        public UIBar TurretHealthBar, TurretTimingBar;
        public Turret CurrentTurret;
        public SpriteFont Font;
        public Rectangle DestinationRectangle;
        public UITurretInfo()
        {
            TurretHealthBar = new UIBar(new Vector2(45, 1080 - 15 - 85 - 15), new Vector2(390, 15), Color.Lerp(Color.White, Color.Transparent, 0.25f), true);
            TurretTimingBar = new UIBar(new Vector2(45, 1080 - 85 - 15), new Vector2(390, 15), Color.Lerp(Color.DodgerBlue, Color.Transparent, 0.25f), true);

            DestinationRectangle = new Rectangle(45, 1080 - 85 - 15, 390, 85);
        }

        public void Update(GameTime gameTime)
        {
            TurretHealthBar.Update(CurrentTurret.Health, CurrentTurret.CurrentHealth, gameTime);
            TurretTimingBar.Update((float)CurrentTurret.FireDelay, (float)CurrentTurret.ElapsedTime, gameTime);
            //CurrentTurret.CurrentHealth -= 0.1f;
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, Color.Lerp(Color.Black, Color.Transparent, 0.75f));

            if (CurrentTurret.Overheated == true)
            {
                spriteBatch.Draw(OverHeatIconTexture,
                    new Rectangle(
                    DestinationRectangle.X + DestinationRectangle.Width - OverHeatIconTexture.Width - 5,
                    DestinationRectangle.Bottom - 32 - 5,
                    OverHeatIconTexture.Width,
                    OverHeatIconTexture.Height), Color.White);
            }

            TurretHealthBar.Draw(graphicsDevice);
            TurretTimingBar.Draw(graphicsDevice);

            spriteBatch.Draw(TurretIconTexture,
                new Rectangle(
                    DestinationRectangle.X + TurretIconTexture.Width / 2 + 5,
                    DestinationRectangle.Y + DestinationRectangle.Height / 2,
                    TurretIconTexture.Width, TurretIconTexture.Height),
                    null, Color.White, 0, new Vector2(TurretIconTexture.Width / 2, TurretIconTexture.Height / 2), SpriteEffects.None, 0);

            spriteBatch.Draw(DamageIconTexture,
                new Rectangle(
                    DestinationRectangle.X + DestinationRectangle.Width - DamageIconTexture.Width / 2 - 5,
                    DestinationRectangle.Y + 5,
                    DamageIconTexture.Width / 2,
                    DamageIconTexture.Height / 2),
                    Color.White);


            //spriteBatch.DrawString(Font, CurrentTurret.Damage.ToString(),
            //    new Vector2(
            //        DestinationRectangle.X + TurretIconTexture.Width + 10 + Font.MeasureString(CurrentTurret.Damage.ToString()).X / 2, 
            //        DestinationRectangle.Y + DestinationRectangle.Height / 2 + 4), 
            //        Color.White, 0, 
            //        new Vector2(
            //            Font.MeasureString(CurrentTurret.Damage.ToString()).X / 2, 
            //            Font.MeasureString(CurrentTurret.Damage.ToString()).Y / 2), 
            //        1, SpriteEffects.None, 0);
     
        }
    }
}
