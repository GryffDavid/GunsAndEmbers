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
        public Texture2D TurretBase, TurretBarrel, Line, TurretAnimationTexture;
        public Vector2 Direction, Position, MousePosition, BarrelPivot, BasePivot;
        public Rectangle BaseRectangle, BarrelRectangle, SelectBox, SourceRectangle;
        MouseState CurrentMouseState, PreviousMouseState;
        public float Rotation;
        public bool Selected, Active, JustClicked, CanShoot;
        public Color Color;
        public double FireDelay;
        public int Damage, AngleOffset;
        public static Random Random = new Random();
        public Vector2 FireDirection;
        public float FireRotation;
        public TurretType TurretType;
        public HorizontalBar TimingBar;
        public double ElapsedTime = 0;

        public Animation CurrentAnimation;
        public double CurrentFrameTime;
        public int CurrentFrame;
        public Vector2 FrameSize;
        public bool Animated, Looping;

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
                TurretBarrel = contentManager.Load<Texture2D>(CurrentAnimation.AssetName);
                FrameSize = new Vector2(TurretBarrel.Width / CurrentAnimation.TotalFrames, TurretBarrel.Height);
            }

            SelectBox = new Rectangle((int)Position.X, (int)Position.Y-24, 64, 64);         
        }

        public void Update(GameTime gameTime)
        {
            if (Animated == true)
            {
                CurrentFrameTime += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (CurrentFrameTime > FireDelay/(CurrentAnimation.TotalFrames+1))
                {
                    CurrentFrame++;

                    if (CurrentFrame >= CurrentAnimation.TotalFrames)
                    {
                        CurrentFrame = 0;

                        if (Looping == false)
                        {
                            Animated = false;
                        }
                    }

                    CurrentFrameTime = 0;
                }
            }

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

            SourceRectangle = new Rectangle(0 + (int)FrameSize.X * CurrentFrame, 0, (int)FrameSize.X, (int)FrameSize.Y);

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
