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
    #region Invader Enums
    public enum AnimationState_Invader { Walk, Stand, Melee, Shoot };


    public enum InvaderFireType { Burst };


    public enum InvaderType
    {
        Soldier,
        BatteringRam,
        Airship,
        Archer,
        Tank,
        Spider,
        Slime,
        SuicideBomber,
        FireElemental,
        StationaryCannon,
        HealDrone,
        JumpMan,
        RifleMan
    };

    public enum MacroBehaviour
    {
        AttackTower,
        AttackTraps,
        AttackTurrets
    };

    public enum MicroBehaviour
    {
        MovingForwards,
        MovingBackwards,
        Stationary,
        AdjustTrajectory,
        Attack
    };


    public class DamageOverTimeStruct
    {
        public float Damage, //How much damage is done each interval
                     InitialDamage, //The first burst of damage done to the invader on contact
                     MaxDelay, //How long the effect lasts for in total
                     CurrentDelay,
                     MaxInterval, //How long between regular damage being done
                     CurrentInterval;

        public Color Color; //The colour the invader turns while this DOT is active
    };

    public class SlowStruct
    {
        public float MaxDelay, CurrentDelay, SpeedPercentage;
        public float PreviousSpeed; //The speed value before the invader was slowed
    };

    public class FreezeStruct
    {
        public float MaxDelay, CurrentDelay;
    };

    public class InvaderMeleeStruct
    {
        public float Damage;
        public float CurrentAttackDelay, MaxAttackDelay;
    };
    #endregion

    public struct PreviousResult
    {
        public MacroBehaviour MacroBehaviour;
        public MicroBehaviour MicroBehaviour;
        public int NumHits; //Number of relevant hits from the previous batch of projectiles
        public float FireAngle; //The angle most recently used when firing
    }

    public abstract class Invader : Drawable
    {
        #region Variables used for invaders
        #region Vertex declarations
        public VertexPositionColorTexture[] shadowVertices = new VertexPositionColorTexture[4];
        public int[] shadowIndices = new int[6];

        public VertexPositionColorTexture[] invaderVertices = new VertexPositionColorTexture[4];
        public int[] invaderIndices = new int[6];

        public VertexPositionColorTexture[] normalVertices = new VertexPositionColorTexture[4];
        public int[] normalIndices = new int[6];
        #endregion

        #region Movement Related Variables
        public SlowStruct CurrentSlow;
        public FreezeStruct CurrentFreeze;
        public float Speed, SlowSpeed;
        public Vector2 Direction = new Vector2(-1f, 0);
        public Vector2 Position, Velocity;
        public SpriteEffects Orientation = SpriteEffects.None;
        #endregion

        #region Boolean variables
        public bool VulnerableToTurret = true;
        public bool VulnerableToTrap = true;
        public bool IsBeingHealed = false;
        public bool IsTargeted = false;
        public bool Airborne = false;
        public bool Solid = false; //Whether this invader stops heavy projectiles or not
        public new bool Active = true;

        public bool CanAttack, Burning, Frozen, Slow, InAir;
        #endregion

        private bool _HitByBeam;
        public bool HitByBeam
        {
            get { return _HitByBeam; }
            set
            {
                _HitByBeam = value;

                if (_HitByBeam == false)
                {
                    CurrentBeamDelay = 0;
                }
            }
        }

        private Color _Color;
        public Color Color
        {
            get { return _Color; }
            set
            {
                _Color = value;

                for (int i = 0; i < invaderVertices.Length; i++)
                {
                    invaderVertices[i].Color = value;
                }
            }
        }

        public Trap TargetTrap;

        private AnimationState_Invader _InvaderAnimationState;
        public AnimationState_Invader InvaderAnimationState
        {
            get { return _InvaderAnimationState; }
            set
            {
                _InvaderAnimationState = value;

                if (AnimationList != null)
                {
                    CurrentAnimation = AnimationList.Find(Animation => Animation.CurrentInvaderState == value);

                    if (CurrentAnimation.CurrentInvaderState == AnimationState_Invader.Stand)
                        CurrentAnimation.CurrentFrame = 0;// Random.Next(0, CurrentAnimation.TotalFrames);

                    CurrentAnimation.CurrentFrameDelay = 0;
                }
            }
        }

        public MacroBehaviour PreviousMacroBehaviour;
        private MacroBehaviour _CurrentMacroBehaviour;
        public MacroBehaviour CurrentMacroBehaviour
        {
            get { return _CurrentMacroBehaviour; }
            set 
            {
                PreviousMacroBehaviour = _CurrentMacroBehaviour;
                _CurrentMacroBehaviour = value; 
            }
        }

        public MicroBehaviour PreviousMicroBehavior;
        private MicroBehaviour _CurrentMicroBehaviour;
        public MicroBehaviour CurrentMicroBehaviour
        {
            get { return _CurrentMicroBehaviour; }
            set
            {
                PreviousMicroBehavior = _CurrentMicroBehaviour;
                _CurrentMicroBehaviour = value;                
            }
        }

        public double MaxBehaviourDelay = 1500;
        public double CurrentBehaviourDelay;

        public object PreviousHitObject;
        private object _HitObject;
        public object HitObject
        {
            get { return _HitObject; }
            set 
            {
                PreviousHitObject = _HitObject;
                _HitObject = value; 
            }
        }

        public DamageOverTimeStruct CurrentDOT;

        public InvaderType InvaderType;
        public InvaderAnimation CurrentAnimation;
        public List<InvaderAnimation> AnimationList;

        public DamageType DamageVulnerability;
        public UIOutline InvaderOutline;
        public UIBar HealthBar;
        public SoundEffectInstance MoveLoop;

        public Texture2D Shadow, IceBlock;
        public Rectangle DestinationRectangle;
        public Vector2 ResourceMinMax, YRange, Center, ShadowPosition;

        public Color DOTColor, FrozenColor, ShadowColor;

        public InvaderMeleeStruct MeleeDamageStruct;

        public float MaxHP, CurrentHP, PreviousHP, MaxY, Gravity, ShadowHeight, ShadowHeightMod;
        public int ResourceValue;
        public static Random Random = new Random();

        //Could be handled by structs - like damage, slow, freeze etc.
        public double BeamDelay, CurrentBeamDelay,
                      HealDelay, CurrentHealDelay;

        public Vector2 IntelligenceRange;
        public float Intelligence;        

        public Vector2 CowardiceRange;
        public float Cowardice; //The chance that this invader will decide to rather back up to avoid damage after taking a big hit
                                //0 means the invader will never, ever back up even after taking a heavy hit

        public AnimatedSprite ThinkingAnimation;
        #endregion

        public virtual void Initialize()
        {
            CurrentHP = MaxHP;
            MaxY = Random.Next((int)YRange.X, (int)YRange.Y);
            ResourceValue = Random.Next((int)ResourceMinMax.X, (int)ResourceMinMax.Y);
            
            if (Airborne == true)
            {
                Position.Y = MaxY;
            }

            Velocity = Direction * Speed;

            HealthBar = new UIBar(new Vector2(100, 100), new Vector2(32, 4), Color.DarkRed, false);

            Color = Color.White;

            if (IntelligenceRange != Vector2.Zero)
            {
                Intelligence = (Random.Next((int)IntelligenceRange.X * 100, (int)IntelligenceRange.Y * 100))/10;
            }

            #region Set up Vertices
            #region Sprite Vertices
            invaderVertices[0] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top, 0),
                TextureCoordinate = CurrentAnimation.dTopLeftTexCooord,
                Color = Color
            };

            invaderVertices[1] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top, 0),
                TextureCoordinate = CurrentAnimation.dTopRightTexCoord,
                Color = Color
            };

            invaderVertices[2] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                TextureCoordinate = CurrentAnimation.dBottomRightTexCoord,
                Color = Color
            };

            invaderVertices[3] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                TextureCoordinate = CurrentAnimation.dBottomLeftTexCoord,
                Color = Color
            };

            invaderIndices[0] = 0;
            invaderIndices[1] = 1;
            invaderIndices[2] = 2;
            invaderIndices[3] = 2;
            invaderIndices[4] = 3;
            invaderIndices[5] = 0;
            #endregion

            #region Normal Vertices
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
            #endregion
            #endregion
        }

        public virtual void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            if (Active == true)
            {
                Velocity.Y += Gravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                if (Velocity.X != 0 && InvaderAnimationState != AnimationState_Invader.Walk)
                {
                    InvaderAnimationState = AnimationState_Invader.Walk;
                }

                if (Velocity.X == 0 && InvaderAnimationState != AnimationState_Invader.Stand && Frozen == false)
                {
                    InvaderAnimationState = AnimationState_Invader.Stand;
                }

                if (CurrentBehaviourDelay <= MaxBehaviourDelay)
                {
                    CurrentBehaviourDelay += gameTime.ElapsedGameTime.TotalMilliseconds;
                    ThinkingAnimation.Position = new Vector2(Center.X - 12, DestinationRectangle.Top - 28);
                    ThinkingAnimation.Update(gameTime);
                }

                #region Update position and vertices
                if (Velocity != Vector2.Zero)
                {
                    Position += Velocity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                    invaderVertices[0].Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top, 0);
                    invaderVertices[1].Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top, 0);
                    invaderVertices[2].Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top + DestinationRectangle.Height, 0);
                    invaderVertices[3].Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top + DestinationRectangle.Height, 0);

                    normalVertices[0].Position = invaderVertices[0].Position;
                    normalVertices[1].Position = invaderVertices[1].Position;
                    normalVertices[2].Position = invaderVertices[2].Position;
                    normalVertices[3].Position = invaderVertices[3].Position;
                }
                #endregion

                #region Draw the invader White if it's not frozen, burning etc.
                if (Frozen == false && Burning == false && HitByBeam == false)
                {
                    Color = Color.White;
                }
                #endregion

                #region This controls how the invader behaves when it's frozen
                if (CurrentFreeze != null)
                {
                    if (Frozen == true)
                    {
                        CanAttack = false;
                        CurrentFreeze.CurrentDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        Color = FrozenColor;
                        Velocity.X = 0;
                    }

                    if (Frozen == true && CurrentFreeze.CurrentDelay > CurrentFreeze.MaxDelay)
                    {
                        Frozen = false;
                        CanAttack = true;
                        Velocity.X = Direction.X * Speed;
                        CurrentFreeze = null;
                    }
                }
                #endregion

                #region This controls how the invader behaves when it's slow
                if (CurrentSlow != null)
                {

                    if (Slow == true)
                    {
                        CurrentSlow.CurrentDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        Velocity.X = Direction.X * SlowSpeed;
                    }

                    if (Slow == true && CurrentSlow.CurrentDelay > CurrentSlow.MaxDelay)
                    {
                        Slow = false;
                        Velocity.X = Direction.X * Speed;
                        CurrentSlow = null;
                    }
                }
                #endregion

                if (CurrentAnimation != null)
                {
                    HealthBar.Update(MaxHP, CurrentHP, gameTime, new Vector2(Position.X + (CurrentAnimation.FrameSize.X - HealthBar.MaxSize.X) / 2 + Velocity.X, Position.Y - 8));

                    DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y,
                                                         (int)(CurrentAnimation.FrameSize.X),
                                                         (int)(CurrentAnimation.FrameSize.Y));

                    BoundingBox = new BoundingBox(new Vector3(Position.X + 6, Position.Y + 6, 0),
                                                  new Vector3(Position.X + ((CurrentAnimation.FrameSize.X - 6)),
                                                              Position.Y + 6 + ((CurrentAnimation.FrameSize.Y - 6)), 0));

                    Center = new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Center.Y + 6);
                }

                #region Melee Attack
                if (MeleeDamageStruct != null)
                {
                    MeleeDamageStruct.CurrentAttackDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (MeleeDamageStruct.CurrentAttackDelay >= MeleeDamageStruct.MaxAttackDelay)
                    {
                        CanAttack = true;
                        MeleeDamageStruct.CurrentAttackDelay = 0;
                    }
                    else
                    {
                        CanAttack = false;
                    }
                }
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

                #region This controls how the invader takes damage when it's burning
                if (CurrentDOT != null)
                {
                    if (Burning == true)
                    {
                        CurrentDOT.CurrentDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        CurrentDOT.CurrentInterval += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    }

                    if (CurrentDOT.CurrentInterval > CurrentDOT.MaxInterval)
                    {
                        CurrentHP -= (int)CurrentDOT.Damage;
                        CurrentDOT.CurrentInterval = 0;
                    }

                    if (Burning == true && CurrentDOT.CurrentDelay > CurrentDOT.MaxDelay)
                    {
                        Burning = false;
                        CurrentDOT.CurrentInterval = 0;
                        CurrentDOT.CurrentDelay = 0;
                        CurrentDOT = null;
                    }

                    if (Burning == true)
                    {
                        Color = DOTColor;
                    }
                }
                #endregion

                #region This animates the invader, but only if it's not frozen
                if (Frozen == false)
                {
                    CurrentAnimation.Update(gameTime);

                    invaderVertices[0].TextureCoordinate = CurrentAnimation.dTopLeftTexCooord;
                    invaderVertices[1].TextureCoordinate = CurrentAnimation.dTopRightTexCoord;
                    invaderVertices[2].TextureCoordinate = CurrentAnimation.dBottomRightTexCoord;
                    invaderVertices[3].TextureCoordinate = CurrentAnimation.dBottomLeftTexCoord;

                    normalVertices[0].TextureCoordinate = CurrentAnimation.nTopLeftTexCooord;
                    normalVertices[1].TextureCoordinate = CurrentAnimation.nTopRightTexCoord;
                    normalVertices[2].TextureCoordinate = CurrentAnimation.nBottomRightTexCoord;
                    normalVertices[3].TextureCoordinate = CurrentAnimation.nBottomLeftTexCoord;
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

                #region Play movement sound loop
                if (MoveLoop != null)
                {
                    if (MoveLoop.State != SoundState.Playing)
                    {
                        MoveLoop.Play();
                        MoveLoop.IsLooped = true;
                    }
                }
                #endregion

                if (Airborne == false)
                {
                    if ((BoundingBox.Max.Y + Velocity.Y) > MaxY)
                    {
                        Velocity.Y = 0;
                        Gravity = 0;
                        Position = new Vector2(Position.X, MaxY - DestinationRectangle.Height);
                        InAir = false;
                    }

                    if (BoundingBox.Max.Y < MaxY)
                    {
                        Gravity = 0.2f;
                    }
                }

                if (HitByBeam == true)
                {
                    Color = Color.Black;
                    CurrentBeamDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }

                if (IsBeingHealed == false)
                {
                    CurrentHealDelay = 0;
                }
                else
                {
                    CurrentHealDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }

                ShadowPosition = new Vector2(Position.X, Position.Y + CurrentAnimation.FrameSize.Y - 2);

                DrawDepth = MaxY / 1080.0f;
            }
        }


        public override void Draw(GraphicsDevice graphics, BasicEffect effect, Effect shadowEffect, List<Light> lightList)
        {
            if (Active == true)
            {
                effect.TextureEnabled = true;
                effect.VertexColorEnabled = true;
                effect.Texture = CurrentAnimation.Texture;

                if (InAir == false)
                {
                    #region Draw initial shadow
                    //if (Active == true)
                    //{
                    //    Vector2 direction = ShadowPosition - new Vector2(DestinationRectangle.X, DestinationRectangle.Y - 32);
                    //    direction.Normalize();

                    //    ShadowHeightMod = 1.0f;
                    //    ShadowHeight = MathHelper.Clamp(CurrentAnimation.FrameSize.Y * ShadowHeightMod, 16, 64);
                    //    float width = MathHelper.Clamp(CurrentAnimation.FrameSize.Y * ShadowHeightMod, 16, 92);

                    //    //This needs to be reduced by the percentage distance to the closest light source
                    //    ShadowColor = Color.Lerp(Color.Lerp(Color.Black, Color.Transparent, 0f), Color.Transparent, 0.25f);

                    //    shadowVertices[0] = new VertexPositionColorTexture()
                    //    {
                    //        Position = new Vector3(ShadowPosition.X, ShadowPosition.Y, 0),
                    //        TextureCoordinate = CurrentAnimation.dBottomLeftTexCoord,
                    //        Color = ShadowColor
                    //    };

                    //    shadowVertices[1] = new VertexPositionColorTexture()
                    //    {
                    //        Position = new Vector3(ShadowPosition.X + CurrentAnimation.FrameSize.X, ShadowPosition.Y, 0),
                    //        TextureCoordinate = CurrentAnimation.dBottomRightTexCoord,
                    //        Color = ShadowColor
                    //    };

                    //    shadowVertices[2] = new VertexPositionColorTexture()
                    //    {
                    //        Position = new Vector3(ShadowPosition.X + CurrentAnimation.FrameSize.X + (direction.X * width), ShadowPosition.Y + (direction.Y * ShadowHeight), 0),
                    //        TextureCoordinate = CurrentAnimation.dTopRightTexCoord,
                    //        Color = Color.Lerp(ShadowColor, Color.Transparent, 0.85f)
                    //    };

                    //    shadowVertices[3] = new VertexPositionColorTexture()
                    //    {
                    //        Position = new Vector3(ShadowPosition.X + (direction.X * width), ShadowPosition.Y + (direction.Y * ShadowHeight), 0),
                    //        TextureCoordinate = CurrentAnimation.dTopLeftTexCooord,
                    //        Color = Color.Lerp(ShadowColor, Color.Transparent, 0.85f)
                    //    };

                    //    //This stops backface culling when the shadow flips vertically
                    //    if (direction.Y > 0)
                    //    {
                    //        shadowIndices[0] = 0;
                    //        shadowIndices[1] = 1;
                    //        shadowIndices[2] = 2;
                    //        shadowIndices[3] = 2;
                    //        shadowIndices[4] = 3;
                    //        shadowIndices[5] = 0;
                    //    }
                    //    else
                    //    {
                    //        shadowIndices[0] = 3;
                    //        shadowIndices[1] = 2;
                    //        shadowIndices[2] = 1;
                    //        shadowIndices[3] = 1;
                    //        shadowIndices[4] = 0;
                    //        shadowIndices[5] = 3;
                    //    }

                    //    //shadowEffect.Parameters["Texture"].SetValue(CurrentAnimation.Texture);
                    //    //shadowEffect.Parameters["texSize"].SetValue(CurrentAnimation.FrameSize);

                    //    foreach (EffectPass pass in shadowEffect.CurrentTechnique.Passes)
                    //    {
                    //        pass.Apply();
                    //        graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, shadowVertices, 0, 4, shadowIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                    //    }
                    //}
                    #endregion

                    #region Draw invader shadows
                    foreach (Light light in lightList)
                    {
                        float lightDistance = Vector2.Distance(new Vector2(Center.X, DestinationRectangle.Bottom), new Vector2(light.Position.X, light.Position.Y));

                        if (lightDistance < light.Range)
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
                }

                #region Draw invader sprite
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
            if (Active == true)
            {
                #region Draw the sprite normal map
                if (CurrentAnimation.AnimationType == AnimationType.Normal || CurrentAnimation.AnimationType == AnimationType.Emissive)
                {
                    effect.TextureEnabled = true;
                    effect.VertexColorEnabled = true;
                    effect.Texture = CurrentAnimation.Texture;

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


        public virtual void TrapDamage(Trap trap)
        {
            if (VulnerableToTrap == true)
            {
                switch (trap.TrapType)
                {
                    default:
                        CurrentHP -= trap.NormalDamage;

                        if (trap.InvaderDOT != null)
                            DamageOverTime(trap.InvaderDOT, trap.InvaderDOT.Color);

                        if (trap.InvaderFreeze != null)
                            Freeze(trap.InvaderFreeze, trap.InvaderDOT.Color);

                        if (trap.InvaderSlow != null)
                            MakeSlow(trap.InvaderSlow);
                        break;
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


        public void DamageOverTime(DamageOverTimeStruct dotStruct, Color color)
        {
            if (Burning == false)
            {
                Burning = true;
                CurrentHP -= dotStruct.InitialDamage;
                CurrentDOT = dotStruct;
                DOTColor = color;
            }
        }

        public void MakeSlow(SlowStruct slowStruct)
        {
            Slow = true;
            CurrentSlow = slowStruct;
            SlowSpeed = slowStruct.SpeedPercentage;
        }

        public void Freeze(FreezeStruct freeze, Color frozenColor)
        {
            if (Frozen == false)
            {
                FrozenColor = frozenColor;
                Frozen = true;
                CurrentFreeze = freeze;
            }
        }

        public void Trajectory(Vector2 velocity)
        {
            if (velocity.Y < 0)
                InAir = true;

            Velocity = velocity;
        }
    }
}
