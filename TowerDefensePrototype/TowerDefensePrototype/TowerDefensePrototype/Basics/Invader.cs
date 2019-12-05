using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public enum InvaderState { Walking, Standing, Melee, Ranged };

    public abstract class Invader
    {
        //public string AssetName;
        public Texture2D CurrentTexture, Shadow, IceBlock;
        public Rectangle DestinationRectangle, SourceRectangle;
        public Vector2 Position, Direction, ResourceMinMax, Velocity, YRange, FrameSize;
        public Vector2 Scale = new Vector2(1, 1);
        public Color Color, BurnColor, FrozenColor, AcidColor;
        public BoundingBox BoundingBox;
        public Double AttackDelay, CurrentAttackDelay;
        public float MaxHP, CurrentHP, PreviousHP, MaxY, Gravity, DrawDepth, Bottom, BurnDamage, Speed, SlowSpeed;
        public int ResourceValue, AttackPower, CurrentFrame;
        public abstract void TrapDamage(TrapType trapType);
        public static Random Random = new Random();
        public double BurnDelay, CurrentBurnDelay,
                      BurnInterval, CurrentBurnInterval, 
                      FreezeDelay, CurrentFreezeDelay,
                      SlowDelay, CurrentSlowDelay,
                      CurrentFrameDelay;
        public bool Active, VulnerableToTurret, VulnerableToTrap, CanAttack, 
                    Burning, Frozen, Slow, Airborne;     
  
        public InvaderType InvaderType;
        public Animation CurrentAnimation;
        public List<Texture2D> TextureList;
        public Emitter FireEmitter;
        public InvaderState CurrentInvaderState, PreviousInvaderState;

        public void Initialize()
        {
            VulnerableToTurret = true;
            VulnerableToTrap = true;
            Color = Color.White;
            MaxY = Random.Next((int)YRange.X, (int)YRange.Y);
            ResourceValue = Random.Next((int)ResourceMinMax.X, (int)ResourceMinMax.Y);
            //FrameSize = new Vector2(CurrentTexture.Width / CurrentAnimation.TotalFrames, CurrentTexture.Height);
            Velocity = Direction * Speed;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (Active == true)
            {
                Position += Velocity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                Velocity.Y += Gravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                CurrentAttackDelay += gameTime.ElapsedGameTime.TotalMilliseconds;                
                VulnerableToTurret = true;

                if (Velocity.X != 0)
                {
                    CurrentInvaderState = InvaderState.Walking;
                }
                else
                {
                    CurrentInvaderState = InvaderState.Standing;
                }

                if (CurrentInvaderState != PreviousInvaderState)
                {
                    CurrentFrameDelay = 0;
                    CurrentFrame = Random.Next(0, CurrentAnimation.TotalFrames);
                }

                FrameSize = new Vector2(CurrentTexture.Width / CurrentAnimation.TotalFrames, CurrentTexture.Height);

                //This disables the invader if it has 0 health left
                if (CurrentHP <= 0)
                    Active = false;

                if (FireEmitter != null)
                {
                    FireEmitter.Update(gameTime);

                    //This makes sure that the entire invader looks like it's on fire, not just part of it.
                    //Randomly jumps the emitter all around the invader rectangle while it's emitting particles
                    FireEmitter.Position = new Vector2(DestinationRectangle.Left +
                                                       Random.Next(0, CurrentTexture.Width / CurrentAnimation.TotalFrames),
                                                       DestinationRectangle.Bottom - Random.Next(0, CurrentTexture.Height));
                }

                #region This makes sure that the invader can't take damage if it's off screen (i.e. before it's visible to the player)
                if (Position.X > 1920)
                {
                    VulnerableToTurret = false;
                    VulnerableToTrap = false;
                }
                else
                {
                    VulnerableToTurret = true;
                    VulnerableToTrap = true;
                }
                #endregion

                #region This makes sure that the invader only attacks when every 1.5 seconds (Or other delay) - Set in the specific class
                if (CurrentAttackDelay >= AttackDelay)
                {
                    CanAttack = true;
                    CurrentAttackDelay = 0;
                }
                else
                {
                    CanAttack = false;
                }
                #endregion

                #region This controls how the invader takes damage when it's burning
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
                    FireEmitter.AddMore = false;
                }

                if (Burning == true)
                {
                    Color = BurnColor;
                }
                #endregion

                #region This controls how the invader behaves when it's frozen
                if (Frozen == true)
                {
                    CanAttack = false;
                    CurrentFreezeDelay += gameTime.ElapsedGameTime.TotalMilliseconds;
                    Color = FrozenColor;
                    Velocity.X = 0;
                }

                if (Frozen == true && CurrentFreezeDelay > FreezeDelay)
                {
                    Frozen = false;
                    CanAttack = true;
                    Velocity.X = Direction.X * Speed;
                    CurrentFreezeDelay = 0;
                }

                if (Frozen == false && Burning == false)
                {
                    Color = Color.White;
                }
                #endregion

                #region This controls how the invader behaves when it's slow
                if (Slow == true)
                {
                    CurrentSlowDelay += gameTime.ElapsedGameTime.TotalMilliseconds;
                    Velocity.X = Direction.X * SlowSpeed;
                }

                if (Slow == true && CurrentSlowDelay > SlowDelay)
                {
                    Slow = false;
                    Velocity.X = Direction.X * Speed;
                    CurrentSlowDelay = 0;
                }
                #endregion

                #region This animates the invader, but only if it's not frozen
                CurrentFrameDelay += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (CurrentFrameDelay > CurrentAnimation.FrameDelay)
                {
                    if (Frozen == false)
                    {
                        CurrentFrame++;

                        if (CurrentFrame == CurrentAnimation.TotalFrames)
                        {
                            CurrentFrame = 0;
                        }

                        CurrentFrameDelay = 0;
                    }
                }
                #endregion

                if (FireEmitter != null && FireEmitter.ParticleList.Count == 0 && FireEmitter.AddMore == false)
                {
                    FireEmitter = null;
                }

                SourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), 0, (int)FrameSize.X, (int)FrameSize.Y);
                DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, 
                                                     (int)(FrameSize.X * Scale.X), (int)(FrameSize.Y * Scale.Y));

                BoundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, 0), 
                                              new Vector3(Position.X + FrameSize.X, Position.Y + FrameSize.Y, 0));

                Bottom = DestinationRectangle.Bottom;
                DrawDepth = Bottom / 1080;

                PreviousInvaderState = CurrentInvaderState;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                spriteBatch.Draw(Shadow, new Rectangle(DestinationRectangle.Left, (int)MaxY - (DestinationRectangle.Height / 8), DestinationRectangle.Width, DestinationRectangle.Height / 4), Color.Lerp(Color.White, Color.Transparent, 0.75f));

                if (FireEmitter != null)
                {
                    FireEmitter.Draw(spriteBatch);
                }

                BoundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, 0),
                              new Vector3(Position.X + (FrameSize.X * Scale.X), Position.Y + (FrameSize.Y * Scale.Y), 0));

                if (CurrentTexture != null)
                spriteBatch.Draw(CurrentTexture, DestinationRectangle, SourceRectangle, Color, MathHelper.ToRadians(0),
                                 Vector2.Zero, SpriteEffects.None, DrawDepth);

                if (Frozen == true)
                {
                    double IceTransparency = ((75 / FreezeDelay) * CurrentFreezeDelay) / 100;
                    spriteBatch.Draw(IceBlock, new Rectangle((int)Position.X, DestinationRectangle.Bottom - IceBlock.Height + 8, IceBlock.Width, IceBlock.Height), null, Color.Lerp(Color.White, Color.Transparent, (float)IceTransparency), 0, Vector2.Zero, SpriteEffects.None, DrawDepth + 0.0001f);
                }
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

        //public void Trajectory(Vector2 velocity)
        //{
        //    Velocity = velocity;
        //}

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

        public void MakeSlow(float milliseconds, float speed)
        {
            Slow = true;
            SlowDelay = milliseconds;
            SlowSpeed = speed;
        }
    }
}
