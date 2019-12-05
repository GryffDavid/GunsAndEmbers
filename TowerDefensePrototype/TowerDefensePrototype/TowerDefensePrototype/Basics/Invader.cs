using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    abstract class Invader
    {
        public int HP;

        public string AssetName;
        public Texture2D Texture;
        public Rectangle DestinationRectangle;
        public Vector2 Position;
        public bool Active;
        public abstract void Behaviour();        

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
            spriteBatch.Draw(Texture, DestinationRectangle, Color.White);
        }

        public void ChangeHP(int change)
        {
            HP += change;
        }
    }
}
