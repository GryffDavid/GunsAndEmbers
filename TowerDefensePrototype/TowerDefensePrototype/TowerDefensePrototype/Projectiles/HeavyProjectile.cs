using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class HeavyProjectile : Projectile
    {
        public Texture2D Texture;
        public Vector2 Velocity, Position;
        public string TextureName;
        public float Gravity;
        public bool Active;
        public Emitter Emitter;

        public HeavyProjectile(string textureName, Vector2 position, float speed, float angle, float gravity)
        {
            Active = true;
            Position = position;
            TextureName = textureName;
            Velocity.X = (float)(Math.Cos(angle) * speed);
            Velocity.Y = (float)(Math.Sin(angle) * speed);
            Gravity = gravity;                  
        }

        public void LoadContent(ContentManager contentManager)
        {
            if (Active == true)
            {
                Texture = contentManager.Load<Texture2D>(TextureName);
            }

            Color FireColor = Color.Orange;
            FireColor.A = 100;

            Color FireColor2 = Color.Orange;
            FireColor2.A = 200;

            Emitter = new Emitter("star", new Vector2(Texture.Width / 2, Texture.Height / 2), new Vector2(75, 105), new Vector2(1.5f, 2), new Vector2(30, 35), 0.2f, true, new Vector2(-20, 20), new Vector2(-4, 4), new Vector2(1, 2f), FireColor, FireColor2, 0.0f, -1);
            Emitter.LoadContent(contentManager);
        }

        public override void Update(GameTime gameTime)
        {           
            if (Active == true)
            {                
                Position += Velocity;
                //Velocity.X -= Gravity;
                Velocity.Y += Gravity;
                Emitter.Position = new Vector2(Position.X+Texture.Width / 2, Position.Y+Texture.Height / 2);    
            }

            Emitter.Update(gameTime);           
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Position.Y > 495)
            {
                Active = false;
                //SmokeEmitter.Active = false;
                Emitter.HPRange = new Vector2(0, 0);
            }

            if (Active == true)
            {                
                spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height), Color.White);
            }

            Emitter.Draw(spriteBatch);
        }
    }
}
