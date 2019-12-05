using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    //Draw the icon with a bar underneath it to show cooldown timer
    //Mousing over this should show an info box describing the powerup type, value and timing
    //re UIPowerupInfo

    public class UIPowerupIcon
    {
        public bool Active;
        Texture2D Icon;
        Vector2 CurrentPosition;
        public Powerup CurrentPowerup;
        UIBar TimingBar;
        public Rectangle DestinationRectangle;

        public UIPowerupIcon(Powerup powerup, Texture2D icon, Vector2 position)
        {
            Active = true;
            CurrentPosition = position;
            CurrentPowerup = powerup;
            Icon = icon;
            DestinationRectangle = new Rectangle((int)CurrentPosition.X, (int)CurrentPosition.Y, 32, 32);
            TimingBar = new UIBar(CurrentPosition + new Vector2(0, 32), new Vector2(32, 4), Color.LightSkyBlue);
        }

        public void Update(GameTime gameTime)
        {
            if (Active == true)
            {
                if (TimingBar != null)
                {
                    TimingBar.Update(CurrentPowerup.MaxTime, CurrentPowerup.CurrentTime);
                }

                Active = CurrentPowerup.Active;
            }
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, BasicEffect basicEffect)
        {
            if (Active == true)
            {
                spriteBatch.Draw(Icon, DestinationRectangle, Color.White);

                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    TimingBar.Draw(graphicsDevice);
                }
            }
        }
    }
}
