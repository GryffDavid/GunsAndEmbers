using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace TowerDefensePrototype
{
    class Turret
    {
        public String TurretAsset, BaseAsset;
        public Texture2D TurretBase, TurretBarrel;
        public Vector2 Direction, Position, MousePosition;
        MouseState CurrentMouseState, PreviousMouseState;
        float Rotation;
        public bool Selected, Active;

        public void LoadContent(ContentManager contentManager)
        {
            if (Active == true)
            {
                TurretBase = contentManager.Load<Texture2D>(BaseAsset);
                TurretBarrel = contentManager.Load<Texture2D>(TurretAsset);
            }
        }

        public void Update()
        {
            if (Active == true)
            {
                if (Selected == true)
                {
                    CurrentMouseState = Mouse.GetState();
                    MousePosition = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);

                    Direction = MousePosition - Position;
                    Direction.Normalize();

                    Rotation = (float)Math.Atan2((double)Direction.Y, (double)Direction.X) + MathHelper.ToRadians(0);

                    PreviousMouseState = CurrentMouseState;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                spriteBatch.Draw(TurretBarrel, new Rectangle((int)Position.X, (int)Position.Y, TurretBarrel.Width, TurretBarrel.Height), null, Color.White, Rotation, new Vector2(24, TurretBarrel.Height / 2), SpriteEffects.None, 1f);
                spriteBatch.Draw(TurretBase, new Rectangle((int)Position.X - 20, (int)Position.Y - 16, TurretBase.Width, TurretBase.Height), Color.White);
            }
        }
    }
}
