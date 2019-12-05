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
        public Texture2D TurretBase, TurretBarrel, Line;
        public Vector2 Direction, Position, MousePosition;
        public Rectangle BaseRectangle, BarrelRectangle;
        MouseState CurrentMouseState, PreviousMouseState;
        public float Rotation;
        public bool Selected, Active, JustClicked, CanShoot;
        public Color Color;
        public MuzzleFlash Flash;
        public double FireDelay;

        double ElapsedTime = 0;

        public void LoadContent(ContentManager contentManager)
        {
            //CanShoot = true;
            Color = Color.White;
            BaseRectangle = new Rectangle();
            BarrelRectangle = new Rectangle();
            
            if (Active == true)
            {
                TurretBase = contentManager.Load<Texture2D>(BaseAsset);
                TurretBarrel = contentManager.Load<Texture2D>(TurretAsset);
            }

            Flash = new MuzzleFlash(contentManager, BarrelRectangle.Width);

            Line = contentManager.Load<Texture2D>("Line");
        }

        public void Update(GameTime gameTime)
        {
            CurrentMouseState = Mouse.GetState();
            
            ElapsedTime += gameTime.ElapsedGameTime.TotalSeconds;

            if (ElapsedTime >= FireDelay)
            {
                CanShoot = true;
                ElapsedTime = 0;
                return;
            }

            //if (CanShoot == true && CurrentMouseState.LeftButton == ButtonState.Pressed && Selected == true)
            //{
            //    Flash.Flash(0, 3);
            //    ElapsedTime = 0;
            //}                       

            if (Active == true)
            {
                if (Selected == true)
                {
                    CurrentMouseState = Mouse.GetState();
                    MousePosition = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);

                    Direction = MousePosition - new Vector2(BarrelRectangle.X, BarrelRectangle.Y);
                    Direction.Normalize();

                    Rotation = (float)Math.Atan2((double)Direction.Y, (double)Direction.X);

                    PreviousMouseState = CurrentMouseState;
                }
                else
                {
                    Rotation = MathHelper.ToRadians(-20);
                }
            }

            #region Handle selection
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
            #endregion

            PreviousMouseState = CurrentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (Active == true)
            {
                Flash.Draw(spriteBatch, gameTime, new Vector2(BarrelRectangle.X, BarrelRectangle.Y), Rotation);

                BaseRectangle = new Rectangle((int)Position.X - 12, (int)Position.Y - 16-6, TurretBase.Width, TurretBase.Height);
                BarrelRectangle = new Rectangle((int)Position.X+8, (int)Position.Y-6, TurretBarrel.Width, TurretBarrel.Height);

                spriteBatch.Draw(TurretBarrel, BarrelRectangle, null, Color, Rotation, new Vector2(24, TurretBarrel.Height / 2), SpriteEffects.None, 1f);
                spriteBatch.Draw(Line, new Rectangle(BarrelRectangle.X, BarrelRectangle.Y, Line.Width, Line.Height), null, Color, Rotation, Vector2.Zero, SpriteEffects.None, 1f);
                spriteBatch.Draw(TurretBase, BaseRectangle, Color);               
            }
        }
    }
}
