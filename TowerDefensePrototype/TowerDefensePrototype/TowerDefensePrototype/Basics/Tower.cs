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
        public float MaxHP, CurrentHP, Slots, MaxShield, CurrentShield;
        public bool ShieldOn;
        public double CurrentShieldTime, ShieldTime;
        public Color Color;
        
        public Tower(string assetName, Vector2 position, int totalHitpoints, int maxShield, int slots, float shieldTime)
        {
            AssetName = assetName;
            Position = position;
            MaxHP = totalHitpoints;
            CurrentHP = MaxHP;
            Slots = slots;
            CurrentShield = maxShield;
            MaxShield = maxShield;
            ShieldTime = shieldTime;
            ShieldOn = true;
            Color = Color.White;
        }

        public void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(AssetName);
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
        }

        public void Update(GameTime gameTime)
        {
            if (ShieldOn == false)
            {
                CurrentShieldTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            if (ShieldOn == false && CurrentShieldTime >= ShieldTime)
            {
                CurrentShield += 0.05f;
                CurrentShield = MathHelper.Clamp(CurrentShield, 0, MaxShield);
            }

            if (ShieldOn == false && CurrentShieldTime >= ShieldTime && CurrentShield == MaxShield)
            {
                ShieldOn = true;
                CurrentShieldTime = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, null, Color, MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, 1);
        }

        public void TakeDamage(float value)
        {
            if (ShieldOn == true && CurrentShield > 0)
                CurrentShield -= value;

            if (ShieldOn == true && CurrentShield <= 0)
                ShieldOn = false;

            if (ShieldOn == false)
            {
                CurrentHP -= value;
            }
        }
    }
}
