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
    public abstract class Turret
    {
        public String TurretAsset, BaseAsset;
        public Texture2D TurretBase, TurretBarrel;
        public Vector2 Direction, Position, MousePosition;
        public Rectangle BaseRectangle, TurretRectangle;
        MouseState CurrentMouseState, PreviousMouseState;
        float Rotation;
        public bool Selected, Active, JustClicked;
        public Color Color;

        public void LoadContent(ContentManager contentManager)
        {
            Color = Color.White;
            BaseRectangle = new Rectangle();
            TurretRectangle = new Rectangle();

            if (Active == true)
            {
                TurretBase = contentManager.Load<Texture2D>(BaseAsset);
                TurretBarrel = contentManager.Load<Texture2D>(TurretAsset);
            }
        }

        public void Update()
        {
            CurrentMouseState = Mouse.GetState();

            if ((PreviousMouseState.RightButton == ButtonState.Pressed) && (CurrentMouseState.RightButton == ButtonState.Released))
            {
                Selected = false;
            }

            if (Active == true)
            {
                if (Selected == true)
                {
                    CurrentMouseState = Mouse.GetState();
                    MousePosition = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);

                    Direction = MousePosition - Position;
                    Direction.Normalize();

                    Rotation = (float)Math.Atan2((double)Direction.Y, (double)Direction.X);

                    PreviousMouseState = CurrentMouseState;
                }
                else
                {
                    Rotation = MathHelper.ToRadians(-20);
                }
            }

            if ((BaseRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)) && CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed) ||
            (TurretRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)) && CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed))
            {
                JustClicked = true;
            }
            else
            {
                JustClicked = false;
            }
            
            if (Selected == true)
            {
                Color = Color.Red;
            }
            else
                Color = Color.White;   

            PreviousMouseState = CurrentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {            
            if (Active == true)
            {
                BaseRectangle = new Rectangle((int)Position.X - 20, (int)Position.Y - 16, TurretBase.Width, TurretBase.Height);
                TurretRectangle = new Rectangle((int)Position.X, (int)Position.Y, TurretBarrel.Width, TurretBarrel.Height);

                spriteBatch.Draw(TurretBarrel, TurretRectangle, null, Color, Rotation, new Vector2(24, TurretBarrel.Height / 2), SpriteEffects.None, 1f);

                spriteBatch.Draw(TurretBase, BaseRectangle, Color);
            }
        }
    }
}
