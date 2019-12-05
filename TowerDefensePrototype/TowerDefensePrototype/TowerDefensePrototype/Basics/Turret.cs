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
        public Rectangle BaseRectangle, BarrelRectangle;
        MouseState CurrentMouseState, PreviousMouseState;
        public float Rotation;
        public bool Selected, Active, JustClicked;
        public Color Color;
        public MuzzleFlash Flash;

        public void LoadContent(ContentManager contentManager)
        {
           

            Color = Color.White;
            BaseRectangle = new Rectangle();
            BarrelRectangle = new Rectangle();

            if (Active == true)
            {
                TurretBase = contentManager.Load<Texture2D>(BaseAsset);
                TurretBarrel = contentManager.Load<Texture2D>(TurretAsset);
                Flash = new MuzzleFlash(contentManager, TurretBarrel.Bounds.Width);
            }
        }

        public void Update()
        {
            CurrentMouseState = Mouse.GetState();

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

            if (BaseRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)) && CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)            
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

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (Active == true)
            {
                BaseRectangle = new Rectangle((int)Position.X - 12, (int)Position.Y - 16-6, TurretBase.Width, TurretBase.Height);
                BarrelRectangle = new Rectangle((int)Position.X+8, (int)Position.Y-6, TurretBarrel.Width, TurretBarrel.Height);

                Flash.Draw(spriteBatch, gameTime, new Vector2(BarrelRectangle.X, BarrelRectangle.Y), Rotation);

                spriteBatch.Draw(TurretBarrel, BarrelRectangle, null, Color, Rotation, new Vector2(24, TurretBarrel.Height / 2), SpriteEffects.None, 1f);
                spriteBatch.Draw(TurretBase, BaseRectangle, Color);
            }
        }
    }
}
