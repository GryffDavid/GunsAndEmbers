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
    #region Turret Enums
    //NEW_TURRET A **turret type to be added here**
    public enum TurretType
    {
        MachineGun,
        Cannon,
        FlameThrower,
        Lightning,
        Cluster,
        FelCannon,
        Beam,
        Freeze,
        Boomerang,
        Grenade,
        GasGrenade,
        Shotgun,
        PersistentBeam,
        Harpoon,
        StickyMine
    };
    public enum TurretAnimationState { Overheated, ReadyToFire, Stunned };
    public enum TurretFireType { FullAuto, SemiAuto, Single, Beam };

    #endregion

    public abstract class Turret : Drawable
    {
        public event EventHandler<EventArgs> TurretClickHappened;
        public void CreateTurretClick()
        {
            OnTurretClickHappened();
        }
        protected virtual void OnTurretClickHappened()
        {
            if (TurretClickHappened != null)
                TurretClickHappened(this, null);
        }

        private static int _ResourceCost = 200;
        public static int ResourceCost
        {
            get { return _ResourceCost; }
        }

        public bool InOut, PrevInOut;
        public Texture2D TurretBase, TurretBarrel;
        public Vector2 Direction, MousePosition, BarrelPivot, BasePivot, 
                       FrameSize, BarrelEnd, BarrelCenter, FireDirection;
        public Rectangle BaseRectangle, BarrelRectangle, SelectBox;
        MouseState CurrentMouseState, PreviousMouseState;
        public float Rotation, MaxHealth, CurrentHealth, FireRotation, CurrentHeat, MaxHeat,
                     CurrentHeatTime, MaxHeatTime, CoolValue, ShotHeat, BlastRadius, AngleOffset,
                     MaxAngleOffset, MinAngleOffset, LaunchVelocity;
        public bool Selected, CanShoot, Animated, Looping, Overheated;
        public double FireDelay, CurrentFrameTime;
        public double ElapsedTime = 0;
        public int Damage, CurrentFrame;
        //public static int ResourceCost;        
        //static Random Random = new Random();
        public TurretType TurretType;
        //public Color Color;
        public TurretAnimation CurrentAnimation;
        public List<Emitter> EmitterList = new List<Emitter>();
        public UIBar TimingBar, HealthBar;

        //Use ref for this
        public AmmoBelt AmmoBelt;

        public UIOutline TurretOutline;

        //Chance to Effect - i.e. fire, slow, freeze etc.
        public float ChanceToEffect;

        //Type of fire: Full-auto, semi-auto, reload
        public TurretFireType TurretFireType;// = TurretFireType.SemiAuto;

        //Shots per magazine: Number of shots before turrets need to be reloaded
        public int MagazineCapacity;

        //Range: Pixel-distance before damage drops off
        public float Range;

        //Total number of times the turret can be fired before needing to be replaced.
        //This is meant for powerful weapons like the lightning turret which are slightly OP
        public int Charges;

        //Responsiveness: The LERP value that determines how fast the turret can move from one direction to another
        public float Responsiveness;

        //The time between when the mouse is clicked and then the shot is fired. 
        //Like the Deathstar building up power before blowing up Alderaan
        public float ChargeTime;

        public int ShotsFired = 0;

        private ButtonState _LeftButtonState;
        public ButtonState LeftButtonState
        {
            get { return _LeftButtonState; }
            set
            {
                _LeftButtonState = value;
                PrevInOut = InOut;
                if (SelectBox.Contains(new Point((int)MousePosition.X, (int)MousePosition.Y)) == true)
                {
                    //In
                    InOut = true;
                }
                else
                {
                    //Out
                    InOut = false;
                }

                if (PrevInOut == true && 
                    InOut == true &&
                    value == ButtonState.Released)
                {
                    CreateTurretClick();
                }
            }
        }

        public virtual void Initialize(ContentManager contentManager)
        {
            CanShoot = false;

            Color = Color.White;
            BaseRectangle = new Rectangle();
            BarrelRectangle = new Rectangle();
            
            if (Active == true)
            {
                if (CurrentAnimation.TotalFrames > 0)
                {
                    FrameSize = new Vector2(TurretBarrel.Width / CurrentAnimation.TotalFrames, TurretBarrel.Height);
                }
                else
                {
                    FrameSize = new Vector2(TurretBarrel.Width, TurretBarrel.Height);
                }
            }

            TimingBar = new UIBar(new Vector2(Position.X, Position.Y + 48), new Vector2(32, 4), Color.DodgerBlue);
            HealthBar = new UIBar(new Vector2(Position.X, Position.Y + 52), new Vector2(32, 4), Color.White); 

            CurrentHealth = MaxHealth;
            CurrentHeat = 0;

            ElapsedTime = FireDelay;

            SelectBox = new Rectangle((int)Position.X - 32, (int)Position.Y - 32, 96, 96);
            BoundingBox = new BoundingBox(new Vector3(SelectBox.X, SelectBox.Y, 0), new Vector3(SelectBox.X + SelectBox.Width, SelectBox.Y + SelectBox.Height, 0));

            Direction = new Vector2(1, 1);
        }

        public virtual void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            CurrentMouseState = Mouse.GetState();

            if (CurrentMouseState.LeftButton != PreviousMouseState.LeftButton)
                LeftButtonState = CurrentMouseState.LeftButton;

            if (double.IsNaN(Rotation) == true)
                Rotation = -20;

            MousePosition = cursorPosition;

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Update(gameTime);
            }

            //if (Active == true && Selected == true)
            //{
            //    FireRotation = Rotation + MathHelper.ToRadians((float)(-AngleOffset + Random.NextDouble() * (AngleOffset - (-AngleOffset))));
            //}

            if (double.IsNaN(Rotation) == true)
                Rotation = -20;

            FireDirection.X = (float)Math.Cos(FireRotation);
            FireDirection.Y = (float)Math.Sin(FireRotation);

            if (double.IsNaN(Rotation) == true)
                Rotation = -20;

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
                    CurrentHeat -= CoolValue * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60);
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

            TimingBar.Update((float)FireDelay, (float)ElapsedTime);
            HealthBar.Update((float)MaxHealth, (float)CurrentHealth);

            //If this is causing problems, put it back in the IF ACTIVE check.
            //It's only here to make sure that the ammo belt behaves correctly
            /////////////
            BarrelCenter = new Vector2(BarrelRectangle.X + (float)Math.Cos(Rotation - 90) * (BarrelPivot.Y - BarrelRectangle.Height / 2),
                                       BarrelRectangle.Y + (float)Math.Sin(Rotation - 90) * (BarrelPivot.Y - BarrelRectangle.Height / 2));

            BarrelEnd = new Vector2(BarrelCenter.X + (float)Math.Cos(Rotation) * (BarrelRectangle.Width - BarrelPivot.X),
                                    BarrelCenter.Y + (float)Math.Sin(Rotation) * (BarrelRectangle.Width - BarrelPivot.X));
            /////////////

            if (Active == true)
            {
                if (Selected == true)
                {
                    if (MousePosition - new Vector2(BarrelCenter.X, BarrelCenter.Y) == Vector2.Zero)
                    {
                        MousePosition += Vector2.One;
                    }
                    
                    Direction = MousePosition - new Vector2(BarrelCenter.X, BarrelCenter.Y);
                    Direction.Normalize();

                    if (Overheated == false)
                        Rotation = MathHelper.Lerp(Rotation, (float)Math.Atan2((double)Direction.Y, (double)Direction.X), 0.1f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60));
                    else
                        Rotation = MathHelper.Lerp(Rotation, MathHelper.ToRadians(40), 0.1f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60));
                    
                    if (double.IsNaN(Rotation) == true)
                        Rotation = -20;
                }
                else
                {
                    if (Overheated == false)
                        Rotation = MathHelper.Lerp(Rotation, MathHelper.ToRadians(-20), 0.1f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60));
                    else
                        Rotation = MathHelper.Lerp(Rotation, MathHelper.ToRadians(40), 0.1f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60));

                    if (double.IsNaN(Rotation) == true)
                        Rotation = -20;
                }
            }

            if (AmmoBelt != null)
            {
                Vector2 direction = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));
                direction.Normalize();

                AmmoBelt.Nodes[0].CurrentPosition = BarrelCenter + (direction * AmmoBelt.ShellTexture.Width / 2);
                AmmoBelt.Nodes2[0].CurrentPosition = BarrelCenter - (direction * AmmoBelt.ShellTexture.Width / 2);
                AmmoBelt.Update(gameTime);
            }

            float Percent = (CurrentHeat / MaxHeat) * 100;
            Color = Color.Lerp(Color.White, Color.Lerp(Color.Red, Color.White, 0.5f), Percent / 100);

            SourceRectangle = new Rectangle(0 + (int)FrameSize.X * CurrentFrame, 0, (int)FrameSize.X, (int)FrameSize.Y);            

            PreviousMouseState = CurrentMouseState;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

        }

        public override void DrawSpriteDepth(GraphicsDevice graphics, Effect effect)
        {

        }

        public override void DrawSpriteNormal(GraphicsDevice graphics, BasicEffect basicEffect)
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
            
            if (double.IsNaN(Rotation) == true)
                Rotation = -20;

            FireDirection.X = (float)Math.Cos(FireRotation);
            FireDirection.Y = (float)Math.Sin(FireRotation);
        }

        public void DrawBars(GraphicsDevice graphicsDevice, BasicEffect basicEffect)
        {
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                TimingBar.Draw(graphicsDevice);
                HealthBar.Draw(graphicsDevice);
            }
        }
    }
}
