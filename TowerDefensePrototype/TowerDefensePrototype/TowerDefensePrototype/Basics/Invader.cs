using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public abstract class Invader
    {
        public int HP;

        public string AssetName;
        public Texture2D Texture;
        public Rectangle DestinationRectangle;
        public Vector2 Position;
        public bool Active;
        public abstract void Behaviour();
        public bool CanMove;
        public Color Color;
        public BoundingBox BoundingBox;

        public void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(AssetName);
            Color = Color.White;
        }

        public void Update()
        {
            if (HP <= 0)
                Active = false;            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {                
                DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
                BoundingBox = new BoundingBox(new Vector3(Position.X+8, Position.Y+8, 0), new Vector3(Position.X + 24, Position.Y + 24, 0));
                spriteBatch.Draw(Texture, DestinationRectangle, Color);
            }
        }

        public void ChangeHP(int change)
        {
            HP += change;
        }
    }
}
