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
        public Texture2D Rect;
        public Texture2D TurretBase, TurretBarrel, TurretAnimationTexture;
        public Vector2 Direction, Position, MousePosition, BarrelPivot, BasePivot, 
            FrameSize, BarrelEnd, BarrelCenter, FireDirection;
        public Rectangle BaseRectangle, BarrelRectangle, SelectBox, SourceRectangle;
        MouseState CurrentMouseState, PreviousMouseState;
        public float Rotation, Health, CurrentHealth, FireRotation, CurrentHeat, MaxHeat, 
            CurrentHeatTime, MaxHeatTime, CoolValue, ShotHeat;
        public bool Selected, Active, JustClicked, CanShoot, Animated, Looping, Overheated;
        public double FireDelay, CurrentFrameTime;
        public double ElapsedTime = 0;
        public int Damage, CurrentFrame, ResourceCost;
        public float AngleOffset, MaxAngleOffset, MinAngleOffset;
        public static Random Random = new Random();
        public TurretType TurretType;
        public HorizontalBar TimingBar, HealthBar, HeatBar;
        public Color Color;        
        public Animation CurrentAnimation;
        public List<Emitter> EmitterList = new List<Emitter>();
        public List<Rectangle> RectList = new List<Rectangle>();
        public SpriteFont Font;

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

            Rect = contentManager.Load<Texture2D>("Icons/TrapIcons/FireTrapIcon");
            Font = contentManager.Load<SpriteFont>("Fonts/DefaultFont");
        }

        public void Update(GameTime gameTime, Vector2 cursorPosition)
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
                {
                    CurrentHeat -= CoolValue;
                }

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
                    MousePosition = cursorPosition;

                    BarrelCenter = new Vector2(BarrelRectangle.X + (float)Math.Cos(Rotation - 90) * (BarrelPivot.Y - BarrelRectangle.Height / 2),
                                             BarrelRectangle.Y + (float)Math.Sin(Rotation - 90) * (BarrelPivot.Y - BarrelRectangle.Height / 2));

                    BarrelEnd = new Vector2(BarrelCenter.X + (float)Math.Cos(Rotation) * (BarrelRectangle.Width - BarrelPivot.X),
                                            BarrelCenter.Y + (float)Math.Sin(Rotation) * (BarrelRectangle.Width - BarrelPivot.X));

                    Direction = MousePosition - new Vector2(BarrelCenter.X, BarrelCenter.Y);
                    Direction.Normalize();

                    if (Overheated == false)
                        Rotation = (float)Math.Atan2((double)Direction.Y, (double)Direction.X);
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

            float Percent = (CurrentHeat / MaxHeat) * 100;
            Color = Color.Lerp(Color.White, Color.Lerp(Color.Red, Color.White, 0.5f), Percent / 100);

            SourceRectangle = new Rectangle(0 + (int)FrameSize.X * CurrentFrame, 0, (int)FrameSize.X, (int)FrameSize.Y);

            #region Handle selection
            if ((InsideRotatedRectangle(BarrelRectangle, BarrelPivot,
                new Vector2(cursorPosition.X, cursorPosition.Y), MathHelper.ToRadians(Rotation)) == true ||
                new Rectangle((int)(BaseRectangle.X - BasePivot.X), (int)(BaseRectangle.Y - BasePivot.Y),
                BaseRectangle.Width, BaseRectangle.Height).Contains(new Point((int)cursorPosition.X, (int)cursorPosition.Y)) == true) &&
                CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)
            {
                JustClicked = true;
            }
            else
            {
                JustClicked = false;
            }
            #endregion                

            PreviousMouseState = CurrentMouseState;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            
        }

        public bool InsideRotatedRectangle(Rectangle originalRectangle, Vector2 rotationPoint, Vector2 mousePoint, float rotationAngle)
        {
            Vector2 A1, B1, C1, D1;
            Vector2 rA, rB, rC, rD;
            double m1, m2, m3, m4;
            double AB, DC, BC, AD;

            Func<Vector2, float, Vector2> RotatePoint = (point, angle) =>
                {
                    double s = Math.Sin(angle);
                    double c = Math.Cos(angle);

                    point.X -= BarrelRectangle.X;
                    point.Y -= BarrelRectangle.Y;

                    point.X -= BarrelPivot.X;
                    point.Y -= BarrelPivot.Y;

                    double xnew = point.X * c - point.Y * s;
                    double ynew = point.X * s + point.Y * c;

                    point.X = (float)xnew + BarrelRectangle.X;
                    point.Y = (float)ynew + BarrelRectangle.Y;

                    return new Vector2(point.X, point.Y);
                };

            Func<Vector2, Vector2, float> GetGradient = (point1, point2) =>
                {
                    return (point1.Y - point2.Y) / (point1.X - point2.X);
                };

            A1 = new Vector2(originalRectangle.X, originalRectangle.Y);
            B1 = new Vector2(originalRectangle.X + originalRectangle.Width, originalRectangle.Y);
            C1 = new Vector2(originalRectangle.X + originalRectangle.Width, originalRectangle.Y + originalRectangle.Height);
            D1 = new Vector2(originalRectangle.X, originalRectangle.Y + originalRectangle.Height);

            rA = RotatePoint(A1, Rotation);
            rB = RotatePoint(B1, Rotation);
            rC = RotatePoint(C1, Rotation);
            rD = RotatePoint(D1, Rotation);

            m1 = GetGradient(rA, rB);
            m2 = GetGradient(rB, rC);
            m3 = GetGradient(rC, rD);
            m4 = GetGradient(rD, rA);

            AB = m1 * mousePoint.X - m1 * rA.X + rA.Y;
            BC = m2 * mousePoint.X - m2 * rB.X + rB.Y;
            DC = m3 * mousePoint.X - m3 * rC.X + rC.Y;
            AD = m4 * mousePoint.X - m4 * rD.X + rD.Y;

            List<bool> BList = new List<bool>();
            int num, num2;
            num = Math.Abs((int)AB - (int)DC);
            num2 = Math.Abs((int)BC - (int)AD);

            if (AB < DC)
                if (Enumerable.Range((int)AB, num).Contains((int)mousePoint.Y) == true) BList.Add(true);

            if (DC < AB)
                if (Enumerable.Range((int)DC, num).Contains((int)mousePoint.Y) == true) BList.Add(true);

            if (AD < BC)
                if (Enumerable.Range((int)AD, num2).Contains((int)mousePoint.Y) == true) BList.Add(true);

            if (BC < AD)
                if (Enumerable.Range((int)BC, num2).Contains((int)mousePoint.Y) == true) BList.Add(true);


            if (BList.Count(t => t = true) >= 2)
                return true;
            else
                return false;
        }

        public void ChangeFireDirection()
        {
            FireRotation = Rotation + MathHelper.ToRadians((float)(-AngleOffset + Random.NextDouble() * (AngleOffset - (-AngleOffset))));

            FireDirection.X = (float)Math.Cos(FireRotation);
            FireDirection.Y = (float)Math.Sin(FireRotation);
        }
    }
}
