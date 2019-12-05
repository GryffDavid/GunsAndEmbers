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
        public string AssetName;
        public Texture2D Texture;
        public Rectangle DestinationRectangle;
        public Vector2 Position, MoveVector;
        public bool Active, Attacking, CanMove, VulnerableToTurret, VulnerableToTrap;
        public Color Color;
        public BoundingBox BoundingBox;
        public Double MoveDelay, CurrentDelay;
        //GameTime GameTime;
        public int MaxHealth, CurrentHealth;
        HorizontalBar HealthBar;
        public abstract void TrapDamage(TrapType trapType);

        public void LoadContent(ContentManager contentManager)
        {
            VulnerableToTurret = true;
            VulnerableToTrap = true;
            Texture = contentManager.Load<Texture2D>(AssetName);
            Color = Color.White;
            HealthBar = new HorizontalBar(contentManager, new Vector2(32, 4), MaxHealth, CurrentHealth);
        }

        public void Update(GameTime gameTime)
        {
            //GameTime = gameTime;
            VulnerableToTurret = true;

            CurrentDelay += gameTime.ElapsedGameTime.Milliseconds;

            if (CurrentHealth <= 0)
                Active = false;

            if (Position.X > 1280)
                Active = false;
            else
                Active = true;

            HealthBar.Update(new Vector2(Position.X, Position.Y - 16), CurrentHealth);
        }

        public void Draw(SpriteBatch spriteBatch)
        {           
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            BoundingBox = new BoundingBox(new Vector3(Position.X + 8, Position.Y + 8, 0), new Vector3(Position.X + 24, Position.Y + 24, 0));
            spriteBatch.Draw(Texture, DestinationRectangle, Color);

            HealthBar.Draw(spriteBatch);
        }

        public void TurretDamage(int change)
        {
            if (VulnerableToTurret == true)           
            CurrentHealth += change;
        }
               
        public void Move()
        {
            if (CurrentDelay > MoveDelay)
            {
                Position += MoveVector;
                CurrentDelay = 0;
            }   
        }
    }
}
