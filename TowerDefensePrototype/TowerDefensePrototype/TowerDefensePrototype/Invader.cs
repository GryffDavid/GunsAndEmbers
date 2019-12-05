using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class Invader
    {
        Texture2D Texture;
        string AssetName;
        public Vector2 Position, Scale;
        Color Color;
        Rectangle DestinationRectangle;
        bool Active;
        int HP;

        public Invader(string assetName, Vector2 position, Vector2? scale = null, Color? color = null)
        {
            Active = true;
            HP = 100;
            AssetName = assetName;
            Position = position;

            if (scale == null)
                Scale = new Vector2(1, 1);
            else
                Scale = scale.Value;

            if (color == null)
                Color = Color.White;
            else
                Color = color.Value;
        }

        public void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(AssetName);
        }

        public void Update()
        {
            if (HP <= 0)            
                Active = false;            

            if (Active == true)            
                DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)            
                spriteBatch.Draw(Texture, DestinationRectangle, Color);            
        }
    }
}
