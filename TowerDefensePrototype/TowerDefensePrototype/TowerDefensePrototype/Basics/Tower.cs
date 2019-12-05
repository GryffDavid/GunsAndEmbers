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
        public double CurrentTime, Time;
        
        public Tower(string assetName, Vector2 position, int totalHitpoints, int maxShield, int slots)
        {
            AssetName = assetName;
            Position = position;
            MaxHP = totalHitpoints;
            CurrentHP = MaxHP;
            Slots = slots;
            CurrentShield = maxShield;
            MaxShield = maxShield;
            ShieldOn = true;
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
                CurrentTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            if (ShieldOn == false && CurrentTime >= 3000)
            {
                CurrentShield += 0.05f;
                CurrentShield = MathHelper.Clamp(CurrentShield, 0, MaxShield);
            }

            if (ShieldOn == false && CurrentTime >= 3000 && CurrentShield == MaxShield)
            {
                ShieldOn = true;
                CurrentTime = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, null, Color.White, MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, 1);
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
