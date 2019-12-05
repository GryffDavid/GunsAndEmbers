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
        public Rectangle BaseRectangle, BarrelRectangle, SelectBox;
        MouseState CurrentMouseState, PreviousMouseState;
        public float Rotation;
        public bool Selected, Active, JustClicked, CanShoot;
        public Color Color;
        public double FireDelay;
        public int Damage, AngleOffset;
        public Random Random;
        public Vector2 FireDirection;
        public float FireRotation;
        public TurretType TurretType;
        public HorizontalBar TimingBar;
        public double ElapsedTime = 0;

        public void LoadContent(ContentManager contentManager)
        {
            CanShoot = false;

            TimingBar = new HorizontalBar(contentManager, new Vector2(32, 4), (int)FireDelay, (int)ElapsedTime, Color.Green, Color.DarkRed);

            Color = Color.White;
            BaseRectangle = new Rectangle();
            BarrelRectangle = new Rectangle();
            
            if (Active == true)
            {
                TurretBase = contentManager.Load<Texture2D>(BaseAsset);
                TurretBarrel = contentManager.Load<Texture2D>(TurretAsset);
            }

            SelectBox = new Rectangle((int)Position.X, (int)Position.Y-24, 64, 64);

            Random = new Random();
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

            TimingBar.Update(new Vector2(Position.X, Position.Y + 40), (int)ElapsedTime);

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
                if (SelectBox.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)) && CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)            
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

                FireRotation = Rotation + MathHelper.ToRadians((float)(-AngleOffset + Random.NextDouble() * (AngleOffset - (-AngleOffset))));

                FireDirection.X = (float)Math.Cos(FireRotation);
                FireDirection.Y = (float)Math.Sin(FireRotation);

            PreviousMouseState = CurrentMouseState;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            
        }
    }
}
