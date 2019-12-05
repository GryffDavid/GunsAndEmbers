using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class Tower
    {
        public Texture2D Texture;
        string AssetName;
        public Vector2 Position;
        public Rectangle DestinationRectangle;
        public float MaxHP, CurrentHP, Slots, MaxShield, CurrentShield, ShieldStrength;
        
        public Tower(string assetName, Vector2 position, int totalHitpoints, int totalShield, float shieldStrength)
        {
            AssetName = assetName;
            Position = position;
            MaxHP = totalHitpoints;
            CurrentHP = MaxHP;
            MaxShield = totalShield;
            CurrentShield = MaxShield;
            ShieldStrength = shieldStrength;
        }

        public void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(AssetName);
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
        }

        public void Update(GameTime gameTime)
        {            

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, null, Color.White, MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, 1);
        }

        public void TakeDamage(int value)
        {
            if (CurrentShield > 0)
            {
                CurrentShield -= MathHelper.Clamp(value, 0, ShieldStrength);
                CurrentHP -= MathHelper.Clamp(value - ShieldStrength, 0, value);
            }
            else
            {
                CurrentHP -= value;
            }
        }
    }
}
