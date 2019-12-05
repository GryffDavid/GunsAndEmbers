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
        public int MaxPowerUnits, CurrentPowerUnits;
        public bool ShieldOn;
        public double CurrentShieldTime, ShieldTime;
        public Color Color;
        public BoundingBox BoundingBox;
        public BoundingSphere ShieldBoundingSphere;
        
        public Tower(string assetName, Vector2 position, int totalHitpoints, int maxShield, int slots, float shieldTime, int powerUnits)
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
            MaxPowerUnits = powerUnits;
            CurrentPowerUnits = powerUnits;
        }

        public void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(AssetName);
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            BoundingBox = new BoundingBox(new Vector3(DestinationRectangle.Left, DestinationRectangle.Top, 0),
                                          new Vector3(DestinationRectangle.Right, DestinationRectangle.Bottom, 0));

            ShieldBoundingSphere = new BoundingSphere(new Vector3(DestinationRectangle.Center.X, DestinationRectangle.Center.Y, 0), 300);
        }

        public void Update(GameTime gameTime)
        {
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);

            if (ShieldOn == false)
            {
                CurrentShieldTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            if (ShieldOn == false && 
                CurrentShieldTime >= ShieldTime)
            {
                CurrentShield += 0.05f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60);
                CurrentShield = MathHelper.Clamp(CurrentShield, 0, MaxShield);
            }

            if (ShieldOn == false && 
                CurrentShieldTime >= ShieldTime && 
                CurrentShield == MaxShield)
            {
                ShieldOn = true;
                CurrentShieldTime = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //DRAW SHADOW HERE

            spriteBatch.Draw(Texture, DestinationRectangle, null, Color, 0, Vector2.Zero, SpriteEffects.None, 1);
        }

        public void TakeDamage(float value)
        {
            if (ShieldOn == true &&
                CurrentShield > 0)
            {
                CurrentShield -= value;
            }

            if (ShieldOn == true &&
                CurrentShield <= 0)
            {
                ShieldOn = false;
            }

            if (ShieldOn == false)
            {
                CurrentHP -= value;
            }
        }
    }
}
