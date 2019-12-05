﻿using System;
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
        public BoundingBox BoundingBox;

        public float MaxHP, CurrentHP, Slots;
        public int MaxPowerUnits, CurrentPowerUnits;

        public Shield Shield;

        public Color Color;
        
        public Tower(string assetName, Vector2 position, int totalHitpoints, int maxShield, int slots, float shieldTime, int powerUnits)
        {
            AssetName = assetName;
            Position = position;
            MaxHP = totalHitpoints;
            CurrentHP = MaxHP;
            Slots = slots;

            Shield = new TowerDefensePrototype.Shield() 
            { 
                CurrentShield = maxShield, 
                MaxShield = maxShield, 
                ShieldTime = shieldTime, 
                ShieldOn = true 
            };

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

            Shield.ShieldBoundingSphere = new BoundingSphere(new Vector3(DestinationRectangle.Center.X, DestinationRectangle.Center.Y, 0), 300);
        }

        public void Update(GameTime gameTime)
        {
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            Shield.Update(gameTime);

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //DRAW SHADOW HERE

            spriteBatch.Draw(Texture, DestinationRectangle, null, Color, 0, Vector2.Zero, SpriteEffects.None, 1);
        }

        public void TakeDamage(float value)
        {
            CurrentHP += Shield.TakeDamage(value);
        }
    }
}
