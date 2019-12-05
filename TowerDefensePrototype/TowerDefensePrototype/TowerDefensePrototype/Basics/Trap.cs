using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public abstract class Trap
    {
        Texture2D Texture;
        Rectangle DestinationRectangle;
        public String AssetName;
        public int HP;
        public Vector2 Scale, FrameSize, Position;
        public bool Active, Solid, CanTrigger;
        public BoundingBox BoundingBox;

        public virtual void LoadContent(ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(AssetName);
            BoundingBox = new BoundingBox(new Vector3((int)Position.X, (int)Position.Y, 0), new Vector3((int)Position.X + Texture.Width, (int)Position.Y - Texture.Height, 0));
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y - Texture.Height, (int)(Texture.Width), (int)(Texture.Height));
        }

        public virtual void Update(GameTime gameTime)
        {

        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, Color.White);
        }
    }
}
