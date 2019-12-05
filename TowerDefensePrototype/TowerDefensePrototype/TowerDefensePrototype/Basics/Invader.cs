using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace TowerDefensePrototype
{
    public enum InvaderState { Walking, Standing, Melee, Ranged };

    public abstract class Invader
    {
        //public string AssetName;
        public Texture2D CurrentTexture, Shadow, IceBlock;
        public Rectangle DestinationRectangle;
        public Vector2 Position, Direction, ResourceMinMax, Velocity, YRange, Center;
        public Vector2 Scale = new Vector2(1, 1);
        public Color Color, BurnColor, FrozenColor, AcidColor;
        public BoundingBox BoundingBox;
        public Double AttackDelay, CurrentAttackDelay;
        public float MaxHP, CurrentHP, PreviousHP, MaxY, Gravity, Bottom, BurnDamage, Speed, SlowSpeed, DrawDepth,
                     TrapAttackPower, TowerAttackPower, TurretAttackPower;
        public int ResourceValue, CurrentFrame;
        public abstract void TrapDamage(Trap trap);
        public static Random Random = new Random();
        public double BurnDelay, CurrentBurnDelay,
                      BurnInterval, CurrentBurnInterval,
                      FreezeDelay, CurrentFreezeDelay,
                      SlowDelay, CurrentSlowDelay,
                      BeamDelay, CurrentBeamDelay,
                      HealDelay, CurrentHealDelay;
        public bool Active, VulnerableToTurret, VulnerableToTrap, CanAttack,
                    Burning, Frozen, Slow, Airborne, HitByBeam, InAir, IsBeingHealed, IsTargeted;
  
        public InvaderType InvaderType;
        public Animation CurrentAnimation;
        public List<Texture2D> TextureList;
        public Emitter FireEmitter, HealthEmitter;
        public InvaderState CurrentInvaderState;
        public InvaderState? PreviousInvaderState = null;
        public InvaderBehaviour Behaviour;// = InvaderBehaviour.AttackTower;
        public DamageType DamageVulnerability;
        public UIOutline InvaderOutline;
        //public UIBar HealthBar;
        public UIBar HealthBar;
        public SoundEffectInstance MoveLoop;
        public SpriteEffects Orientation = SpriteEffects.None;
        public float Rotation = 0;

        private InvaderBehaviour RandomOrientation(params InvaderBehaviour[] Orientations)
        {
            List<InvaderBehaviour> OrientationList = new List<InvaderBehaviour>();

            foreach (InvaderBehaviour orientation in Orientations)
            {
                OrientationList.Add(orientation);
            }

            return OrientationList[Random.Next(0, OrientationList.Count)];
        }

        public void Initialize()
        {
            Behaviour = RandomOrientation(InvaderBehaviour.AttackTower, InvaderBehaviour.AttackTraps);
            HitByBeam = false;
            VulnerableToTurret = true;
            VulnerableToTrap = true;
            IsBeingHealed = false;
            //InAir = false;
            Color = Color.White;
            MaxY = Random.Next((int)YRange.X, (int)YRange.Y);
            ResourceValue = Random.Next((int)ResourceMinMax.X, (int)ResourceMinMax.Y);

            if (this.GetType().BaseType.Name == "HeavyRangedInvader")
            {
                HeavyRangedInvader rangedInvader = this as HeavyRangedInvader;
                rangedInvader.MinDistance = Random.Next((int)rangedInvader.DistanceRange.X, (int)rangedInvader.DistanceRange.Y);
            }

            if (this.GetType().BaseType.Name == "LightRangedInvader")
            {
                LightRangedInvader rangedInvader = this as LightRangedInvader;
                rangedInvader.MinDistance = Random.Next((int)rangedInvader.DistanceRange.X, (int)rangedInvader.DistanceRange.Y);
            }

            if (Airborne == true)
            {
                Position.Y = MaxY;
            }

            Velocity = Direction * Speed;

            HealthBar = new UIBar(new Vector2(100, 100), new Vector2(32, 4), Color.DarkRed, false);
            HealthBar.CurrentScale = new Vector2(1, 1);
        }

        public virtual void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            if (Active == true)
            {
                if (MoveLoop != null)
                {
                    if (MoveLoop.State != SoundState.Playing)
                    {
                        MoveLoop.Play();
                        MoveLoop.IsLooped = true;
                    }
                }

                //if (Velocity.X > 0)
                //{
                //    Orientation = SpriteEffects.FlipHorizontally;
                //}
                //else
                //{
                //    Orientation = SpriteEffects.None;
                //}

                Position += Velocity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                Velocity.Y += Gravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                CurrentAttackDelay += gameTime.ElapsedGameTime.TotalMilliseconds;                
                VulnerableToTurret = true;
                
                //This disables the invader if it has 0 health left
                if (CurrentHP <= 0)
                    Active = false;

                //HealthBar.Position = Position;
                HealthBar.Update(MaxHP, CurrentHP, gameTime, new Vector2(Position.X + (CurrentAnimation.FrameSize.X - HealthBar.MaxSize.X) / 2 + Velocity.X, Position.Y - 8));
                //HealthBar.MaxValue = MaxHP;
                //HealthBar.CurrentValue = CurrentHP;

                if (FireEmitter != null)
                {
                    FireEmitter.Update(gameTime);

                    //This makes sure that the entire invader looks like it's on fire, not just part of it.
                    //Randomly jumps the emitter all around the invader rectangle while it's emitting particles
                    FireEmitter.Position = new Vector2(DestinationRectangle.Left +
                                                       Random.Next(0, CurrentTexture.Width / CurrentAnimation.TotalFrames),
                                                       DestinationRectangle.Bottom - Random.Next(0, CurrentTexture.Height));
                }

                if (HealthEmitter != null)
                {
                    HealthEmitter.Update(gameTime);

                    HealthEmitter.Position = new Vector2(DestinationRectangle.Left +
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

                    if (FireEmitter != null)
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

                if (Frozen == false)
                {
                    CurrentAnimation.Update(gameTime);
                }
                //CurrentFrameDelay += gameTime.ElapsedGameTime.TotalMilliseconds;

                //if (CurrentFrameDelay > CurrentAnimation.FrameDelay && CurrentAnimation.TotalFrames > 1)
                //{
                //    if (Frozen == false)
                //    {
                //        CurrentFrame++;

                //        if (CurrentFrame >= CurrentAnimation.TotalFrames)
                //        {
                //            CurrentFrame = 0;
                //        }

                //        CurrentFrameDelay = 0;
                //    }
                //}
                #endregion

                if (FireEmitter != null && FireEmitter.ParticleList.Count == 0 && FireEmitter.AddMore == false)
                {
                    FireEmitter = null;
                }

                if (HealthEmitter != null && HealthEmitter.ParticleList.Count == 0 && HealthEmitter.AddMore == false)
                {
                    HealthEmitter = null;
                }

                if (InvaderOutline != null)
                {
                    InvaderOutline.Position = Position;

                    if (DestinationRectangle.Contains(new Point((int)cursorPosition.X, (int)cursorPosition.Y)))
                    {
                        InvaderOutline.Visible = true;
                    }
                    else
                    {
                        InvaderOutline.Visible = false;
                    }
                }

                //SourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), 0, (int)FrameSize.X, (int)FrameSize.Y);
                DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y,
                                                     (int)(CurrentAnimation.FrameSize.X * Scale.X), (int)(CurrentAnimation.FrameSize.Y * Scale.Y));

                BoundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, 0),
                                              new Vector3(Position.X + CurrentAnimation.FrameSize.X, Position.Y + CurrentAnimation.FrameSize.Y, 0));

                Center = new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Center.Y);

                if (HitByBeam == true)
                {
                    //Color = Color.Purple;
                    CurrentBeamDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }

                if (IsBeingHealed == false)
                {
                    CurrentHealDelay = 0;
                }

                if (IsBeingHealed == true)
                {
                    CurrentHealDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
                //else
                //{
                //    Color = Color.White;
                //}

                Bottom = DestinationRectangle.Bottom;
                DrawDepth = Bottom / 1080.0f;

                PreviousInvaderState = CurrentInvaderState;

                //if (IsBeingHealed == true)
                //{
                //    Color = Color.Black;
                //}
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                if (Airborne == false)
                    spriteBatch.Draw(Shadow, new Rectangle(DestinationRectangle.Left, (int)MaxY - (DestinationRectangle.Height / 8), DestinationRectangle.Width, DestinationRectangle.Height / 4), Color.Lerp(Color.White, Color.Transparent, 0.75f));
                
                BoundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, 0),
                              new Vector3(Position.X + (CurrentAnimation.FrameSize.X * Scale.X), Position.Y + (CurrentAnimation.FrameSize.Y * Scale.Y), 0));

                if (HealthEmitter != null)
                {
                    HealthEmitter.Draw(spriteBatch);
                }

                if (CurrentTexture != null)
                    spriteBatch.Draw(CurrentTexture, DestinationRectangle, CurrentAnimation.SourceRectangle, Color, MathHelper.ToRadians(Rotation),
                                 Vector2.Zero, Orientation, DrawDepth);

                if (FireEmitter != null)
                {
                    FireEmitter.Draw(spriteBatch);
                }

               
                if (Frozen == true)
                {
                    double IceTransparency = ((75 / FreezeDelay) * CurrentFreezeDelay) / 100;
                    spriteBatch.Draw(IceBlock, 
                        new Rectangle(
                            (int)Position.X, 
                            DestinationRectangle.Bottom - IceBlock.Height + 8, 
                            IceBlock.Width, IceBlock.Height), 
                        null, Color.Lerp(Color.White, Color.Transparent, (float)IceTransparency), 0,
                        Vector2.Zero, Orientation, DrawDepth + 0.0001f);
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

        public void HealDamage(int change)
        {
            if (Active == true)
            {
                CurrentHP += change;
            }
        }

        public void Trajectory(Vector2 velocity)
        {
            if (velocity.Y < 0)
                InAir = true;

            Velocity = velocity;
        }

        public void Freeze(FreezeStruct freeze, Color frozenColor)
        {
            if (Frozen == false)
            {
                FrozenColor = frozenColor;
                Frozen = true;
                FreezeDelay = freeze.Milliseconds;
            }
        }

        public void DamageOverTime(DamageOverTimeStruct dotStruct, Color color)
        {
            if (Burning == false)
            {
                Burning = true;
                CurrentHP -= dotStruct.InitialDamage;
                BurnDelay = dotStruct.Milliseconds;
                BurnDamage = dotStruct.Damage;
                BurnInterval = dotStruct.Interval;
                BurnColor = color;
            }
        }

        public void MakeSlow(SlowStruct slowStruct)
        {
            Slow = true;
            SlowDelay = slowStruct.Milliseconds;
            SlowSpeed = slowStruct.SpeedPercentage;
        }
    }
}
