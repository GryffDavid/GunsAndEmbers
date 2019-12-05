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
        public Vector2 BarrelPivot, BasePivot;
        public Rectangle BaseRectangle, BarrelRectangle;
        MouseState CurrentMouseState, PreviousMouseState;
        public float Rotation;
        public bool Selected, Active, JustClicked, CanShoot;
        public Color Color;
        public double FireDelay;
        public int Damage;
        public Random Random;
        public Vector2 FireDirection;
        public float FireRotation;
        public TurretType TurretType;

        public double ElapsedTime = 0;

        public void LoadContent(ContentManager contentManager)
        {
            CanShoot = false;

            Color = Color.White;
            BaseRectangle = new Rectangle();
            BarrelRectangle = new Rectangle();
            
            if (Active == true)
            {
                TurretBase = contentManager.Load<Texture2D>(BaseAsset);
                TurretBarrel = contentManager.Load<Texture2D>(TurretAsset);
            }
            //Line = contentManager.Load<Texture2D>("Projectile");
        }

        public void Update(GameTime gameTime)
        {
            ElapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (ElapsedTime > FireDelay)
            {
                CanShoot = true;
            }
            else
            {
                CanShoot = false;
            }

            CurrentMouseState = Mouse.GetState();

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
            
                //if (Selected == true)
                //{
                //    Color = Color.Red;
                //}
                //else
                //    Color = Color.White;
            #endregion

                Random = new Random();

                FireRotation = Rotation + MathHelper.ToRadians((float)(-2 + Random.NextDouble() * (2 - (-2))));

                FireDirection.X = (float)Math.Cos(FireRotation);
                FireDirection.Y = (float)Math.Sin(FireRotation);

            PreviousMouseState = CurrentMouseState;

        }

        //public void Draw(SpriteBatch spriteBatch)
        //{
        //    if (Active == true)
        //    {
        //        //if (Selected == true)
        //        //    spriteBatch.Draw(Line, new Rectangle(BarrelRectangle.X, BarrelRectangle.Y, Line.Width * 8, Line.Height), null, Color.White, Rotation, new Vector2 (0,+(Line.Height/2)), SpriteEffects.None, 1f);

        //        BaseRectangle = new Rectangle((int)Position.X - 12, (int)Position.Y - 16-6, TurretBase.Width, TurretBase.Height);
        //        BarrelRectangle = new Rectangle((int)Position.X+8, (int)Position.Y-6, TurretBarrel.Width, TurretBarrel.Height);

        //        spriteBatch.Draw(TurretBarrel, BarrelRectangle, null, Color, Rotation, new Vector2(24, TurretBarrel.Height / 2), SpriteEffects.None, 1f);
               
        //        spriteBatch.Draw(TurretBase, BaseRectangle, Color);               
        //    }
        //}

        public virtual void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
