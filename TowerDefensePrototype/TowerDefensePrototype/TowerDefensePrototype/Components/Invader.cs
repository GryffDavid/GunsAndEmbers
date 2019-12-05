﻿using System;
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
        public Color Color, BurnColor, FrozenColor, AcidColor, ShadowColor;
        public BoundingBox BoundingBox;
        public Double AttackDelay, CurrentAttackDelay;
        public float MaxHP, CurrentHP, PreviousHP, MaxY, Gravity, Bottom, BurnDamage, Speed, SlowSpeed,
                     TrapAttackPower, TowerAttackPower, TurretAttackPower, ShadowHeight, ShadowHeightMod;
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
                 
                    if (CurrentAnimation.CurrentInvaderState == InvaderState.Stand)
                        CurrentAnimation.CurrentFrame = 1;// Random.Next(0, CurrentAnimation.TotalFrames);

                    CurrentAnimation.CurrentFrameDelay = 0;
                }
            }
        }

        #region Vertex declarations
        public VertexPositionColorTexture[] shadowVertices = new VertexPositionColorTexture[4];
        public int[] shadowIndices = new int[6];

        public VertexPositionColorTexture[] invaderVertices = new VertexPositionColorTexture[4];
        public int[] invaderIndices = new int[6];

        public VertexPositionColorTexture[] normalVertices = new VertexPositionColorTexture[4];
        public int[] normalIndices = new int[6];
        #endregion
        
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

                if (Velocity.X == 0 && InvaderState != InvaderState.Stand && Frozen == false)
                {
                    InvaderState = InvaderState.Stand;                    
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

                    BoundingBox = new BoundingBox(new Vector3(Position.X + 6, Position.Y + 6, 0),
                                                  new Vector3(Position.X + 6 + ((CurrentAnimation.FrameSize.X - 6) * Scale.X),
                                                              Position.Y + 6 + ((CurrentAnimation.FrameSize.Y - 6) * Scale.Y), 0));

                    Center = new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Center.Y + 6);
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

                ShadowPosition = new Vector2(Position.X, Position.Y + CurrentAnimation.FrameSize.Y);

                Bottom = DestinationRectangle.Bottom;
                DrawDepth = Bottom / 1080.0f;

                //if (IsBeingHealed == true)
                //{
                //    Color = Color.Black;
                //}
            }
        }


        public override void Draw(GraphicsDevice graphics, BasicEffect effect, Effect shadowEffect, List<Light> lightList)
        {
            if (Active == true)
            {
                effect.TextureEnabled = true;
                effect.VertexColorEnabled = true;
                effect.Texture = CurrentAnimation.Texture;

                #region Draw trap shadows
                foreach (Light light in lightList)
                {
                    float lightDistance = Vector2.Distance(ShadowPosition, new Vector2(light.Position.X, light.Position.Y));

                    if (lightDistance < light.Radius)
                    {
                        Vector2 direction = ShadowPosition - new Vector2(light.Position.X, light.Position.Y);
                        direction.Normalize();

                        ShadowHeightMod = lightDistance / (light.Range / 10);
                        ShadowHeight = MathHelper.Clamp(CurrentAnimation.FrameSize.Y * ShadowHeightMod, 16, 64);
                        float width = MathHelper.Clamp(CurrentAnimation.FrameSize.Y * ShadowHeightMod, 16, 92);

                        ShadowColor = Color.Lerp(Color.Lerp(Color.Black, Color.Transparent, 0f), Color.Transparent, lightDistance / light.Radius);
                        foreach (Light light3 in lightList.FindAll(Light2 => Vector2.Distance(ShadowPosition, new Vector2(Light2.Position.X, Light2.Position.Y)) < light.Radius && Light2 != light).ToList())
                        {
                            ShadowColor *= MathHelper.Clamp(Vector2.Distance(new Vector2(light3.Position.X, light3.Position.Y), ShadowPosition) / light3.Radius, 0.8f, 1f);
                        }

                        shadowVertices[0] = new VertexPositionColorTexture()
                        {
                            Position = new Vector3(ShadowPosition.X, ShadowPosition.Y, 0),
                            TextureCoordinate = CurrentAnimation.dBottomLeftTexCoord,
                            Color = ShadowColor
                        };

                        shadowVertices[1] = new VertexPositionColorTexture()
                        {
                            Position = new Vector3(ShadowPosition.X + CurrentAnimation.FrameSize.X, ShadowPosition.Y, 0),
                            TextureCoordinate = CurrentAnimation.dBottomRightTexCoord,
                            Color = ShadowColor
                        };

                        shadowVertices[2] = new VertexPositionColorTexture()
                        {
                            Position = new Vector3(ShadowPosition.X + CurrentAnimation.FrameSize.X + (direction.X * width), ShadowPosition.Y + (direction.Y * ShadowHeight), 0),
                            TextureCoordinate = CurrentAnimation.dTopRightTexCoord,
                            Color = Color.Lerp(ShadowColor, Color.Transparent, 0.85f)
                        };

                        shadowVertices[3] = new VertexPositionColorTexture()
                        {
                            Position = new Vector3(ShadowPosition.X + (direction.X * width), ShadowPosition.Y + (direction.Y * ShadowHeight), 0),
                            TextureCoordinate = CurrentAnimation.dTopLeftTexCooord,
                            Color = Color.Lerp(ShadowColor, Color.Transparent, 0.85f)
                        };

                        //This stops backface culling when the shadow flips vertically
                        if (direction.Y > 0)
                        {
                            shadowIndices[0] = 0;
                            shadowIndices[1] = 1;
                            shadowIndices[2] = 2;
                            shadowIndices[3] = 2;
                            shadowIndices[4] = 3;
                            shadowIndices[5] = 0;
                        }
                        else
                        {
                            shadowIndices[0] = 3;
                            shadowIndices[1] = 2;
                            shadowIndices[2] = 1;
                            shadowIndices[3] = 1;
                            shadowIndices[4] = 0;
                            shadowIndices[5] = 3;
                        }

                        shadowEffect.Parameters["Texture"].SetValue(CurrentAnimation.Texture);
                        shadowEffect.Parameters["texSize"].SetValue(CurrentAnimation.FrameSize);

                        foreach (EffectPass pass in shadowEffect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, shadowVertices, 0, 4, shadowIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                        }
                    }
                }
                #endregion

                #region Draw invader sprite

                if (Frozen == true)
                {
                    int thing = 0;
                }

                invaderVertices[0] = new VertexPositionColorTexture()
                {
                    Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top, 0),
                    TextureCoordinate = CurrentAnimation.dTopLeftTexCooord,
                    Color = Color.White
                };

                invaderVertices[1] = new VertexPositionColorTexture()
                {
                    Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top, 0),
                    TextureCoordinate = CurrentAnimation.dTopRightTexCoord,
                    Color = Color.White
                };

                invaderVertices[2] = new VertexPositionColorTexture()
                {
                    Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                    TextureCoordinate = CurrentAnimation.dBottomRightTexCoord,
                    Color = Color.White
                };

                invaderVertices[3] = new VertexPositionColorTexture()
                {
                    Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                    TextureCoordinate = CurrentAnimation.dBottomLeftTexCoord,
                    Color = Color.White
                };

                invaderIndices[0] = 0;
                invaderIndices[1] = 1;
                invaderIndices[2] = 2;
                invaderIndices[3] = 2;
                invaderIndices[4] = 3;
                invaderIndices[5] = 0;

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, invaderVertices, 0, 4, invaderIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                }
                #endregion

                #region Draw ice block around the invader

                #endregion
            }
        }

        public override void DrawSpriteDepth(GraphicsDevice graphics, Effect effect)
        {
            //Draw the same sprite as the color map, but with the depth effect applied
            if (Active == true)
            {
                effect.Parameters["Texture"].SetValue(CurrentAnimation.Texture);
                effect.Parameters["depth"].SetValue(DrawDepth);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, invaderVertices, 0, 4, invaderIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                }
            }
        }

        public override void DrawSpriteNormal(GraphicsDevice graphics, BasicEffect effect)
        {
            //Draw the lower half of the sprite with a basic effect applied
            if (Active == true)
            {
                #region Draw the sprite normal map
                if (CurrentAnimation.AnimationType == AnimationType.Normal || CurrentAnimation.AnimationType == AnimationType.Emissive)
                {
                    effect.TextureEnabled = true;
                    effect.VertexColorEnabled = true;
                    effect.Texture = CurrentAnimation.Texture;

                    normalVertices[0] = new VertexPositionColorTexture()
                    {
                        Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top, 0),
                        TextureCoordinate = CurrentAnimation.nTopLeftTexCooord,
                        Color = Color.White
                    };

                    normalVertices[1] = new VertexPositionColorTexture()
                    {
                        Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top, 0),
                        TextureCoordinate = CurrentAnimation.nTopRightTexCoord,
                        Color = Color.White
                    };

                    normalVertices[2] = new VertexPositionColorTexture()
                    {
                        Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                        TextureCoordinate = CurrentAnimation.nBottomRightTexCoord,
                        Color = Color.White
                    };

                    normalVertices[3] = new VertexPositionColorTexture()
                    {
                        Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                        TextureCoordinate = CurrentAnimation.nBottomLeftTexCoord,
                        Color = Color.White
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
                }
                #endregion

                #region Draw the ice block normal map 

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

        public void Stun()
        {

        }

    }
}