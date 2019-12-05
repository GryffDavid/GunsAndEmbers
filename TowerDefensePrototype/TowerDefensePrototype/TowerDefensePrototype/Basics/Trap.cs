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
        public Rectangle DestinationRectangle;
        public String AssetName;
        public int CurrentHP;
        public Vector2 Scale, FrameSize, Position;
        public bool Active, Solid, CanTrigger;
        public BoundingBox BoundingBox;
        public TrapType TrapType;
        public Emitter TrapEmitter;
        public float DetonateDelay, CurrentDetonateDelay;

        public virtual void LoadContent(ContentManager contentManager)
        {
            Active = true;
            Texture = contentManager.Load<Texture2D>(AssetName);
            BoundingBox = new BoundingBox(new Vector3((int)Position.X, (int)Position.Y, 0), new Vector3((int)Position.X + Texture.Width, (int)Position.Y - Texture.Height, 0));
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y - Texture.Height, (int)(Texture.Width), (int)(Texture.Height));
            CurrentDetonateDelay = DetonateDelay;
        }

        public virtual void Update(GameTime gameTime)
        {
            CurrentDetonateDelay += gameTime.ElapsedGameTime.Milliseconds;

            if (CurrentDetonateDelay >= DetonateDelay)
            {
                CanTrigger = true;                
            }
            else
            {
                CanTrigger = false;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentHP <= 0)
                Active = false;

            if (Active == true)
                spriteBatch.Draw(Texture, DestinationRectangle, Color.White);
        }
    }
}
