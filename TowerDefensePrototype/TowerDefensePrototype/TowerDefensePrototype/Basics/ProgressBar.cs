using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace TowerDefensePrototype
{
    public class ProgressBar
    {
        Texture2D Background, Outline, ForeGround;
        string BackgroundName, OutlineName, ForegroundName;
        public float TotalHealth, CurrentHealth;
        float BarSize;
        Vector2 Position;

        public ProgressBar(Vector2 position, string background, string outline, string foreground, float totalHealth, float currentHealth)
        {
            Position = position;
            BackgroundName = background;
            OutlineName = outline;
            ForegroundName = foreground;
            TotalHealth = totalHealth;
            CurrentHealth = currentHealth;
        }

        public void LoadContent(ContentManager contentManager)
        {
            Background = contentManager.Load<Texture2D>(BackgroundName);
            Outline = contentManager.Load<Texture2D>(OutlineName);
            ForeGround = contentManager.Load<Texture2D>(ForegroundName);
        }

        public void Update(float Health)
        {
            BarSize = (ForeGround.Width / TotalHealth) * Health;
        }

        public void Draw(SpriteBatch spriteBatch)
        {            
            spriteBatch.Draw(Background, new Rectangle((int)Position.X - Background.Width/2, (int)Position.Y, Background.Width, Background.Height), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.98f);
            spriteBatch.Draw(ForeGround, new Rectangle((int)Position.X - ForeGround.Width / 2, (int)Position.Y, (int)BarSize, ForeGround.Height), new Rectangle(0, 0, (int)BarSize, ForeGround.Height), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.99f);
            spriteBatch.Draw(Outline, new Rectangle((int)Position.X - Outline.Width / 2, (int)Position.Y, Outline.Width, Outline.Height), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 1f);
        }
    }
}
