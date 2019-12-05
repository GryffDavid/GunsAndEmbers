using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public abstract class HeavyProjectile
    {
        public Texture2D Texture, Shadow;
        public string TextureName;
        public List<Emitter> EmitterList;
        public Vector2 Velocity, Position, YRange;
        public float Angle, Speed, Gravity, Rotation, CurrentTransparency, MaxY;
        public bool Active, Rotate, Fade;
        public Color CurrentColor;
        public HeavyProjectileType HeavyProjectileType;
        public Rectangle DestinationRectangle, CollisionRectangle;
        public float Damage, BlastRadius;
        static Random Random = new Random();

        public void LoadContent(ContentManager contentManager)
        {
            Shadow = contentManager.Load<Texture2D>("Shadow");
            Texture = contentManager.Load<Texture2D>(TextureName);

            foreach (Emitter emitter in EmitterList)
            {
                emitter.LoadContent(contentManager);
            }            

            CurrentTransparency = 0;

            MaxY = Random.Next((int)YRange.X, (int)YRange.Y);
        }

        public virtual void Update(GameTime gameTime)
        {         
            if (Active == true)
            {
                Position += Velocity;
                Velocity.Y += Gravity;

                foreach (Emitter emitter in EmitterList)
                {
                    emitter.Position = Position;
                }
            }

            if (Rotate == true)
                Rotation = (float)Math.Atan2(Velocity.Y, Velocity.X);

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Update(gameTime);
            }

            if (Fade == true)
            {
                CurrentTransparency += 0.1f;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {           
            if (Active == true)
            {
                spriteBatch.Draw(Shadow, new Rectangle((int)Position.X, (int)MaxY, Texture.Width, Texture.Height / 3), Color.Lerp(Color.White, Color.Transparent, 0.75f));

                CurrentColor = Color.Lerp(Color.White, Color.Transparent, CurrentTransparency);
                DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
                CollisionRectangle = new Rectangle(DestinationRectangle.X, DestinationRectangle.Y, DestinationRectangle.Width / 2, DestinationRectangle.Height / 2);
                spriteBatch.Draw(Texture, DestinationRectangle, null, CurrentColor, Rotation,
                    new Vector2(Texture.Width / 2, Texture.Height / 2), SpriteEffects.None, MaxY / 720);

                foreach (Emitter emitter in EmitterList)
                {
                    emitter.Draw(spriteBatch);
                }
            }
        }
    }
}
