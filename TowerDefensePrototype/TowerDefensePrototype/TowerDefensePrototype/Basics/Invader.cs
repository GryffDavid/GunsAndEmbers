using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public abstract class Invader
    {
        public string AssetName;
        public Texture2D CurrentTexture;
        public Rectangle DestinationRectangle, SourceRectangle;
        public Vector2 Position, MoveVector, ResourceMinMax, Velocity;
        public bool Active, CanMove, VulnerableToTurret, VulnerableToTrap, CanAttack, Burning, Frozen, Slow;
        public Color Color, BurnColor, FrozenColor, AcidColor;
        public BoundingBox BoundingBox;
        public Double CurrentMoveDelay, MoveDelay, CurrentDelay, AttackDelay, CurrentAttackDelay;
        public int MaxHP, CurrentHP, ResourceValue, MaxY;
        public Vector2 YRange;
        public abstract void TrapDamage(TrapType trapType);
        public int AttackPower;
        public Random Random;
        public float Gravity, BurnDamage, DrawDepth;
        public double BurnDelay, FreezeDelay, CurrentBurnDelay, CurrentFreezeDelay,
                        CurrentBurnInterval, BurnInterval, SlowDelay, CurrentSlowDelay;
        HorizontalBar HPBar;
        public InvaderType InvaderType;
        public int TotalFrames, CurrentFrame;
        public double CurrentFrameDelay, FrameDelay;
        public Vector2 FrameSize;
        public Vector2 Scale = new Vector2(1, 1);

        public void LoadContent(ContentManager contentManager)
        {
            Random = new System.Random();
            VulnerableToTurret = true;
            VulnerableToTrap = true;
            CurrentTexture = contentManager.Load<Texture2D>(AssetName);
            Color = Color.White;
            HPBar = new HorizontalBar(contentManager, new Vector2(32, 4), MaxHP, CurrentHP);
            CurrentMoveDelay = MoveDelay;
            MaxY = Random.Next((int)YRange.X, (int)YRange.Y); 
        }

        public virtual void Update(GameTime gameTime)
        {
            if (Active == true)
            {
                Random = new Random();
                ResourceValue = Random.Next((int)ResourceMinMax.X, (int)ResourceMinMax.Y);

                VulnerableToTurret = true;

                CurrentDelay += gameTime.ElapsedGameTime.Milliseconds;
                CurrentAttackDelay += gameTime.ElapsedGameTime.Milliseconds;

                if (CurrentHP <= 0)
                    Active = false;

                if (Position.X > 1280)
                {
                    VulnerableToTurret = false;
                    VulnerableToTrap = false;
                }
                else
                {
                    VulnerableToTurret = true;
                    VulnerableToTrap = true;
                }


                if (CurrentAttackDelay >= AttackDelay)
                {
                    CanAttack = true;
                    CurrentAttackDelay = 0;
                }
                else
                {
                    CanAttack = false;
                }

                if (Burning == true)
                {
                    CurrentBurnDelay += gameTime.ElapsedGameTime.TotalMilliseconds;
                    CurrentBurnInterval += gameTime.ElapsedGameTime.TotalMilliseconds;
                }

                if (CurrentBurnInterval > BurnInterval)
                {
                    CurrentHP -= (int)BurnDamage;
                    CurrentBurnInterval = 0;
                }

                if (Burning == true && CurrentBurnDelay > BurnDelay)
                {
                    Burning = false;
                    CurrentBurnInterval = 0;
                    CurrentBurnDelay = 0;
                }                              

                if (Burning == true)
                {
                    Color = BurnColor;
                }

                if (Frozen == true)
                {
                    CurrentFreezeDelay += gameTime.ElapsedGameTime.TotalMilliseconds;
                    Color = FrozenColor;
                    CanMove = false;
                }

                if (Frozen == true && CurrentFreezeDelay > FreezeDelay)
                {
                    Frozen = false;
                    CanMove = true;
                    CurrentFreezeDelay = 0;
                }

                if (Frozen == false && Burning == false)
                {
                    Color = Color.White;
                }

                if (Slow == true)
                {
                    CurrentSlowDelay += gameTime.ElapsedGameTime.TotalMilliseconds;
                    CurrentMoveDelay = MoveDelay * 5;                    
                }

                if (Slow == true && CurrentSlowDelay > SlowDelay)
                {
                    Slow = false;
                    CurrentMoveDelay = MoveDelay;
                    CurrentSlowDelay = 0;
                }

                HPBar.Update(new Vector2(Position.X, Position.Y - 16), CurrentHP);                

                Position += Velocity;

                Velocity.Y += Gravity;

                CurrentFrameDelay += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (CurrentFrameDelay > FrameDelay)
                {
                    if (Frozen == false)
                    {
                        CurrentFrame++;

                        if (CurrentFrame == TotalFrames)
                        {
                            CurrentFrame = 0;
                        }

                        CurrentFrameDelay = 0;
                    }
                }

                SourceRectangle = new Rectangle((int)(CurrentFrame*FrameSize.X), 0, (int)FrameSize.X, (int)FrameSize.Y);

                DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)(FrameSize.X*Scale.X), (int)(FrameSize.Y*Scale.Y));

                BoundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, 0), new Vector3(Position.X + FrameSize.X, Position.Y + FrameSize.Y, 0));

                DrawDepth = (YRange.Y - MaxY) / 100;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {                
                BoundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, 0), 
                              new Vector3(Position.X + (FrameSize.X * Scale.X), Position.Y + (FrameSize.Y * Scale.Y), 0));

                spriteBatch.Draw(CurrentTexture, DestinationRectangle, SourceRectangle, Color, MathHelper.ToRadians(0), 
                    Vector2.Zero, SpriteEffects.None, DrawDepth);

                HPBar.Draw(spriteBatch);
            }
        }

        public void TurretDamage(int change)
        {
            if (Active == true)
            {
                if (VulnerableToTurret == true)
                    CurrentHP += change;
            }
        }

        public void Trajectory(Vector2 velocity)
        {
            Velocity = velocity;
        }
               
        public void Move()
        {
            if (Active == true && CanMove == true)
            {
                if (CurrentDelay > CurrentMoveDelay)
                {
                    Position += MoveVector;
                    CurrentDelay = 0;
                }
            }
        }

        public void Freeze(float milliseconds, Color frozenColor)
        {
            if (Frozen == false)
            {
                FrozenColor = frozenColor;
                Frozen = true;
                FreezeDelay = milliseconds;
            }
        }

        public void DamageOverTime(float milliseconds, float damage, float interval, Color color)
        {
            if (Burning == false)
            {
                Burning = true;
                BurnDelay = milliseconds;
                BurnDamage = damage;
                BurnInterval = interval;
                BurnColor = color;
            }
        }

        public void MakeSlow(float milliseconds, float moveDelay)
        {
            Slow = true;
            SlowDelay = milliseconds;
            CurrentMoveDelay = moveDelay;
        }
    }
}
