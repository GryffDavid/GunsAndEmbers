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
   

    public abstract class Invader : Drawable
    {
        //public string AssetName;
        public Texture2D Shadow, IceBlock;
        public Rectangle DestinationRectangle;
        public Vector2 Position, Direction, ResourceMinMax, Velocity, YRange, Center, ShadowPosition;
        public Vector2 Scale = new Vector2(1, 1);
        public Color Color, BurnColor, FrozenColor, AcidColor;
        public BoundingBox BoundingBox;
        public Double AttackDelay, CurrentAttackDelay;
        public float MaxHP, CurrentHP, PreviousHP, MaxY, Gravity, Bottom, BurnDamage, Speed, SlowSpeed,
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
        public bool VulnerableToTurret, VulnerableToTrap, CanAttack,
                    Burning, Frozen, Slow, Airborne, HitByBeam, InAir, IsBeingHealed, IsTargeted;
  
        public InvaderType InvaderType;
        public InvaderAnimation CurrentAnimation;
        public List<InvaderAnimation> AnimationList;
        public Emitter FireEmitter, HealthEmitter;
        public InvaderBehaviour Behaviour;
        public DamageType DamageVulnerability;
        public UIOutline InvaderOutline;
        public UIBar HealthBar;
        public SoundEffectInstance MoveLoop;
        public SpriteEffects Orientation = SpriteEffects.None;
        public float Rotation = 0;

        private InvaderState _InvaderState;
        public InvaderState InvaderState
        {
            get { return _InvaderState; }
            set
            {
                _InvaderState = value;

                if (AnimationList != null)
                {
                    CurrentAnimation = AnimationList.Find(Animation => Animation.CurrentInvaderState == value);
                    CurrentAnimation.CurrentFrame = Random.Next(0, CurrentAnimation.TotalFrames);
                    CurrentAnimation.CurrentFrameDelay = 0;
                }
            }
        }

        #region Vertex declarations
        public VertexPositionColorTexture[] shadowVertices = new VertexPositionColorTexture[4];
        public int[] shadowIndices = new int[6];

        public VertexPositionColorTexture[] spriteVertices = new VertexPositionColorTexture[4];
        public int[] spriteIndices = new int[6];

        public VertexPositionColorTexture[] normalVertices = new VertexPositionColorTexture[4];
        public int[] normalIndices = new int[6];
        #endregion

        Color shadowColor;
        float height, heightMod;

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

                Position += Velocity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                Velocity.Y += Gravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                CurrentAttackDelay += gameTime.ElapsedGameTime.TotalMilliseconds;                
                VulnerableToTurret = true;

                if (CurrentHP <= 0)
                    Active = false;

                if (CurrentAnimation != null)
                HealthBar.Update(MaxHP, CurrentHP, gameTime, new Vector2(Position.X + (CurrentAnimation.FrameSize.X - HealthBar.MaxSize.X) / 2 + Velocity.X, Position.Y - 8));

                if (Velocity.X != 0 && InvaderState != InvaderState.Walk)
                {
                    InvaderState = InvaderState.Walk;
                }

                if (Velocity.X == 0 && InvaderState != InvaderState.Stand)
                {
                    InvaderState = InvaderState.Stand;
                }

                #region Update Fire Emitter
                //if (FireEmitter != null)
                //{
                //    FireEmitter.Update(gameTime);
                //    FireEmitter.Position = new Vector2(DestinationRectangle.Left +
                //                                       Random.Next(0, CurrentTexture.Width / CurrentAnimation.TotalFrames),
                //                                       DestinationRectangle.Bottom - Random.Next(0, CurrentTexture.Height));

                //    if (FireEmitter.ParticleList.Count == 0 && FireEmitter.AddMore == false)
                //    {
                //        FireEmitter = null;
                //    }
                //}
                #endregion

                #region Update Health Emitter
                //if (HealthEmitter != null)
                //{
                //    HealthEmitter.Update(gameTime);
                //    HealthEmitter.Position = new Vector2(DestinationRectangle.Left +
                //                                       Random.Next(0, CurrentTexture.Width / CurrentAnimation.TotalFrames),
                //                                       DestinationRectangle.Bottom - Random.Next(0, CurrentTexture.Height));

                //    if (HealthEmitter.ParticleList.Count == 0 && HealthEmitter.AddMore == false)
                //    {
                //        HealthEmitter = null;
                //    }
                //}
                #endregion

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
                #endregion

                #region Handle the invader selection outline
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
                #endregion

                if (CurrentAnimation != null)
                {
                    DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y,
                                                         (int)(CurrentAnimation.FrameSize.X * Scale.X),
                                                         (int)(CurrentAnimation.FrameSize.Y * Scale.Y));

                    BoundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, 0),
                                                  new Vector3(Position.X + (CurrentAnimation.FrameSize.X * Scale.X),
                                                              Position.Y + (CurrentAnimation.FrameSize.Y * Scale.Y), 0));

                    Center = new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Center.Y);
                }
                
                if (HitByBeam == true)
                {
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

                Bottom = DestinationRectangle.Bottom;
                DrawDepth = Bottom / 1080.0f;

                //if (IsBeingHealed == true)
                //{
                //    Color = Color.Black;
                //}
            }
        }

        
        public override void Draw(SpriteBatch spriteBatch, BasicEffect effect, GraphicsDevice graphics, Effect shadowEffect, List<Light> lightList)
        {
            if (Active == true)
            {
                if (CurrentAnimation != null)
                {
                    effect.TextureEnabled = true;
                    effect.VertexColorEnabled = true;
                    effect.Texture = CurrentAnimation.Texture;

                    float animX = (float)(1f / CurrentAnimation.TotalFrames) * CurrentAnimation.CurrentFrame;
                    float animWid = (float)(1f / CurrentAnimation.TotalFrames);

                    #region Draw the sprite
                    spriteVertices[0] = new VertexPositionColorTexture()
                    {
                        Color = Color,
                        Position = new Vector3(DestinationRectangle.X, DestinationRectangle.Y, 0),
                        TextureCoordinate = new Vector2(animX, 0)
                    };

                    spriteVertices[1] = new VertexPositionColorTexture()
                    {
                        Color = Color,
                        Position = new Vector3(DestinationRectangle.X + CurrentAnimation.DiffuseSourceRectangle.Width, DestinationRectangle.Y, 0),
                        TextureCoordinate = new Vector2(animX + animWid, 0)
                    };

                    spriteVertices[2] = new VertexPositionColorTexture()
                    {
                        Color = Color,
                        Position = new Vector3(DestinationRectangle.X + CurrentAnimation.DiffuseSourceRectangle.Width, DestinationRectangle.Y + CurrentAnimation.DiffuseSourceRectangle.Height, 0),
                        TextureCoordinate = new Vector2(animX + animWid, 0.5f)
                    };

                    spriteVertices[3] = new VertexPositionColorTexture()
                    {
                        Color = Color,
                        Position = new Vector3(DestinationRectangle.X, DestinationRectangle.Y + CurrentAnimation.DiffuseSourceRectangle.Height, 0),
                        TextureCoordinate = new Vector2(animX, 0.5f)
                    };

                    spriteIndices[0] = 0;
                    spriteIndices[1] = 1;
                    spriteIndices[2] = 2;
                    spriteIndices[3] = 2;
                    spriteIndices[4] = 3;
                    spriteIndices[5] = 0;

                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, spriteVertices, 0, 4, spriteIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                    }
                    #endregion

                    foreach (Light light in lightList)
                    {
                        ShadowPosition = new Vector2(Position.X, Position.Y + CurrentAnimation.DiffuseSourceRectangle.Height - 4);
                        float lightDistance = Vector2.Distance(ShadowPosition, new Vector2(light.Position.X, light.Position.Y));

                        Vector2 direction = ShadowPosition - new Vector2(light.Position.X, light.Position.Y);
                        direction.Normalize();

                        heightMod = lightDistance / (light.Range / 10);
                        height = CurrentAnimation.DiffuseSourceRectangle.Height * heightMod;
                        height = MathHelper.Clamp(CurrentAnimation.DiffuseSourceRectangle.Height * heightMod, 16, 64);
                        float width = CurrentAnimation.DiffuseSourceRectangle.Height * heightMod;
                        width = MathHelper.Clamp(CurrentAnimation.DiffuseSourceRectangle.Height * heightMod, 16, 92);
                        
                        shadowColor = Color.Lerp(Color.Black, Color.Transparent, lightDistance / light.Radius);

                        //Reduce the density of the shadow based on the proximate lights
                        foreach (Light light3 in lightList.FindAll(Light2 => Vector2.Distance(ShadowPosition, new Vector2(Light2.Position.X, Light2.Position.Y)) < light.Radius && Light2 != light).ToList())
                        {
                            shadowColor *= MathHelper.Clamp(Vector2.Distance(new Vector2(light3.Position.X, light3.Position.Y), ShadowPosition) / light3.Radius, 0.8f, 1f);
                        }



                        #region Draw the shadow

                        #endregion
                    }
                }

                if (HealthEmitter != null)
                {
                    HealthEmitter.Draw(spriteBatch);
                }

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

        public override void DrawSpriteDepth(GraphicsDevice graphics, Effect effect)
        {
            if (CurrentAnimation != null)
            {                
                effect.Parameters["Texture"].SetValue(CurrentAnimation.Texture);
                effect.Parameters["depth"].SetValue(DrawDepth);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, spriteVertices, 0, 4, spriteIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                }
            }
        }

        public override void DrawSpriteNormal(GraphicsDevice graphics, BasicEffect effect)
        {
            if (CurrentAnimation != null)
            {
                effect.TextureEnabled = true;
                effect.VertexColorEnabled = true;
                effect.Texture = CurrentAnimation.Texture;

                float animX = (float)(1f / CurrentAnimation.TotalFrames) * CurrentAnimation.CurrentFrame;
                float animWid = (float)(1f / CurrentAnimation.TotalFrames);

                #region Draw the sprite
                normalVertices[0] = new VertexPositionColorTexture()
                {
                    Color = Color.White,
                    Position = new Vector3(DestinationRectangle.X, DestinationRectangle.Y, 0),
                    TextureCoordinate = new Vector2(animX, 0.5f)
                };

                normalVertices[1] = new VertexPositionColorTexture()
                {
                    Color = Color.White,
                    Position = new Vector3(DestinationRectangle.X + CurrentAnimation.NormalSourceRectangle.Width, DestinationRectangle.Y, 0),
                    TextureCoordinate = new Vector2(animX + animWid, 0.5f)
                };

                normalVertices[2] = new VertexPositionColorTexture()
                {
                    Color = Color.White,
                    Position = new Vector3(DestinationRectangle.X + CurrentAnimation.NormalSourceRectangle.Width, DestinationRectangle.Y + CurrentAnimation.NormalSourceRectangle.Height, 0),
                    TextureCoordinate = new Vector2(animX + animWid, 1f)
                };

                normalVertices[3] = new VertexPositionColorTexture()
                {
                    Color = Color.White,
                    Position = new Vector3(DestinationRectangle.X, DestinationRectangle.Y + CurrentAnimation.NormalSourceRectangle.Height, 0),
                    TextureCoordinate = new Vector2(animX, 1f)
                };

                normalIndices[0] = 0;
                normalIndices[1] = 1;
                normalIndices[2] = 2;
                normalIndices[3] = 2;
                normalIndices[4] = 3;
                normalIndices[5] = 0;

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, normalVertices, 0, 4, normalIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                }
                #endregion
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
