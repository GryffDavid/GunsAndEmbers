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
        public Vector2 Direction, Position, MousePosition, BarrelPivot, BasePivot, 
            FrameSize, BarrelEnd, TestVector, FireDirection;
        public Rectangle BaseRectangle, BarrelRectangle, SelectBox, SourceRectangle;
        MouseState CurrentMouseState, PreviousMouseState;
        public float Rotation, Health, CurrentHealth, FireRotation, CurrentHeat, MaxHeat, 
            CurrentHeatTime, MaxHeatTime, CoolValue, ShotHeat;
        public bool Selected, Active, JustClicked, CanShoot, Animated, Looping, Overheated;
        public double FireDelay, CurrentFrameTime;
        public double ElapsedTime = 0;
        public int Damage, AngleOffset, CurrentFrame, ResourceCost;
        public static Random Random = new Random();
        public TurretType TurretType;
        public HorizontalBar TimingBar, HealthBar, HeatBar;
        public Color Color;        
        public Animation CurrentAnimation;
        public List<Emitter> EmitterList = new List<Emitter>();

        public void LoadContent(ContentManager contentManager)
        {
            CanShoot = false;

            TimingBar = new HorizontalBar(contentManager, new Vector2(32, 4), (int)FireDelay, (int)ElapsedTime, Color.Green, Color.DarkRed);
            HealthBar = new HorizontalBar(contentManager, new Vector2(32, 4), (int)Health, (int)CurrentHealth, Color.Green, Color.DarkRed);
            HeatBar = new HorizontalBar(contentManager, new Vector2(32, 4), (int)MaxHeat, (int)CurrentHeat, Color.Blue, Color.Orange);

            Color = Color.White;
            BaseRectangle = new Rectangle();
            BarrelRectangle = new Rectangle();
            
            if (Active == true)
            {
                TurretBase = contentManager.Load<Texture2D>(BaseAsset);
                TurretBarrel = contentManager.Load<Texture2D>(CurrentAnimation.AssetName);
                FrameSize = new Vector2(TurretBarrel.Width / CurrentAnimation.TotalFrames, TurretBarrel.Height);
            }
                        
            CurrentHealth = Health;
            CurrentHeat = 0;

            SelectBox = new Rectangle((int)Position.X - 32, (int)Position.Y - 32, 64, 64);
        }

        public void Update(GameTime gameTime)
        {
            foreach (Emitter emitter in EmitterList)
            {
                emitter.Update(gameTime);
            }

            FireRotation = Rotation + MathHelper.ToRadians((float)(-AngleOffset + Random.NextDouble() * (AngleOffset - (-AngleOffset))));

            FireDirection.X = (float)Math.Cos(FireRotation);
            FireDirection.Y = (float)Math.Sin(FireRotation);

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

            #region Handle heat
            if (MaxHeat != 0)
            {
                CurrentHeat = MathHelper.Clamp(CurrentHeat, 0, MaxHeat);

                if (CurrentHeat >= MaxHeat)
                {
                    Overheated = true;
                }
                else
                {
                    Overheated = false;
                }

                if (CurrentHeat < MaxHeat && CurrentHeat > 0)
                    CurrentHeat -= CoolValue;

                if (CurrentHeat >= MaxHeat)
                {
                    CurrentHeatTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (CurrentHeatTime > MaxHeatTime)
                    {
                        CurrentHeat -= ShotHeat;
                        CurrentHeatTime = 0;
                    }
                }
            }
            #endregion

            if (ElapsedTime > FireDelay && Overheated != true)
            {
                CanShoot = true;
            }
            else
            {
                CanShoot = false;
            }

            CurrentMouseState = Mouse.GetState();

            TimingBar.Update(new Vector2(Position.X, Position.Y + 40), (int)ElapsedTime);
            HealthBar.Update(new Vector2(Position.X, Position.Y + 48), (int)CurrentHealth);
            HeatBar.Update(new Vector2(Position.X, Position.Y + 56), (int)CurrentHeat);

            if (Active == true)
            {
                if (Selected == true)
                {
                    CurrentMouseState = Mouse.GetState();
                    MousePosition = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);

                    TestVector = new Vector2(BarrelRectangle.X + (float)Math.Cos(Rotation - 90) * (BarrelPivot.Y - BarrelRectangle.Height / 2),
                                             BarrelRectangle.Y + (float)Math.Sin(Rotation - 90) * (BarrelPivot.Y - BarrelRectangle.Height / 2));

                    BarrelEnd = new Vector2(TestVector.X + (float)Math.Cos(Rotation) * (BarrelRectangle.Width - BarrelPivot.X),
                                            TestVector.Y + (float)Math.Sin(Rotation) * (BarrelRectangle.Width - BarrelPivot.X));

                    Direction = MousePosition - new Vector2(TestVector.X, TestVector.Y);
                    Direction.Normalize();

                    if (Overheated == false)
                        Rotation = (float)Math.Atan2((double)Direction.Y, (double)Direction.X);
                        //Rotation = MathHelper.Lerp(Rotation, (float)Math.Atan2((double)Direction.Y, (double)Direction.X), 0.1f);
                    else
                        Rotation = MathHelper.Lerp(Rotation, MathHelper.ToRadians(40), 0.1f);

                    PreviousMouseState = CurrentMouseState;
                }
                else
                {
                    if (Overheated == false)
                        Rotation = MathHelper.Lerp(Rotation, MathHelper.ToRadians(-20), 0.1f);
                    else
                        Rotation = MathHelper.Lerp(Rotation, MathHelper.ToRadians(40), 0.1f);
                }
            }

            float Percent = (MaxHeat / 100) * CurrentHeat;
            Color = Color.Lerp(Color.White, Color.Lerp(Color.Red, Color.White, 0.5f), Percent / 100);

            SourceRectangle = new Rectangle(0 + (int)FrameSize.X * CurrentFrame, 0, (int)FrameSize.X, (int)FrameSize.Y);

            #region Handle selection
            if (SelectBox.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)) &&
                CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)
            {
                JustClicked = true;
            }
            else
            {
                JustClicked = false;
            }

            //if (Selected == true)
            //{
            //    Color = Color.White;
            //}
            //else
            //    Color = Color.White;
            #endregion                

            PreviousMouseState = CurrentMouseState;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            foreach (Emitter emitter in EmitterList)
            {
                emitter.Draw(spriteBatch);
            }
        }
    }
}
