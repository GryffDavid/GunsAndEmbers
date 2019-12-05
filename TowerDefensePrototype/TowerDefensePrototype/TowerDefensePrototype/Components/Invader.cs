using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace TowerDefensePrototype
{
    #region Invader Enums
    public enum AnimationState_Invader { Walk, Stand, Melee, Shoot };

    public enum InvaderFireType { Single, Burst, Beam };

    public struct RangedAttackTiming
    {
        public float CurrentFireDelay, MaxFireDelay;
        public int CurrentBurstNum, MaxBurstNum;
        public float CurrentBurstTime, MaxBurstTime;
    }

    public enum InvaderBuildType { Mechanical, Organic }; //Whether the invader is affected by things like gas.

    public enum InvaderEmotion { Normal, Fear, Anger };

    #region InvaderType
    //NEW_INVADER A **invader type to be added**
    public enum InvaderType
    {
        Soldier,
        BatteringRam,
        GunShip,
        Archer,
        Tank,
        Spider,
        Slime,
        SuicideBomber,
        FireElemental,
        StationaryCannon,
        HealDrone,
        JumpMan,
        RifleMan,
        ShieldGenerator,
        HarpoonCannon,
        DropShip
    };
    #endregion

    #region MacroBehaviour
    public enum MacroBehaviour
    {
        AttackTower,
        AttackTraps,
        AttackTurrets,
        OperateVehicle,
        IsOperated
    }; 
    #endregion

    #region MicroBehaviour
    public enum MicroBehaviour
    {
        MovingForwards,
        MovingBackwards,
        Stationary,
        AdjustTrajectory,
        Attack,
        FollowWaypoints
    };
    #endregion


    #region DOTStruct
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
    #endregion

    #region SlowStruct
    public class SlowStruct
    {
        public float MaxDelay, CurrentDelay, SpeedPercentage;
        public float PreviousSpeed; //The speed value before the invader was slowed
    };
    #endregion

    #region FreezeStruct
    /// <summary>
    /// Delay determines how long the invader stays frozen for.
    /// UnfrozenVelocity stores the velocity of the invader before it was frozen.
    /// </summary>
    public class FreezeStruct
    {
        public float MaxDelay;
        public float CurrentDelay = 0;
        public Vector2 UnfrozenVelocity;
    }; 
    #endregion

    #region MeleeStruct
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
    #endregion
    
    public abstract class Invader : Drawable
    {
        public virtual float OriginalSpeed { get { return 1.0f; } }

        VectorSprite EmotionSprite;

        private InvaderEmotion _CurrentEmotion;
        public InvaderEmotion CurrentEmotion
        {
            get 
            { 
                return _CurrentEmotion; 
            }

            set 
            { 
                _CurrentEmotion = value;

                switch (_CurrentEmotion)
                {
                    case InvaderEmotion.Fear:
                        {
                            EmotionSprite = new VectorSprite(new Vector2(DestinationRectangle.Center.X - 16, DestinationRectangle.Top - 40), new Vector2(32, 32), FearEmotionIcon, new Color(255, 0, 0, 25));
                        }
                        break;

                    case InvaderEmotion.Normal:
                        {
                            EmotionSprite = null;
                        }
                        break;
                }
            }
        }
        

        //InvaderEmotion CurrentEmotion = InvaderEmotion.Fear;

        #region Variables used for invaders
        public static ObservableCollection<Trap> TrapList;
        public static List<Invader> InvaderList;
        public static Tower Tower;
        public static List<Emitter> EmitterList;
        public static List<Drawable> DrawableList;

        #region Vertex declarations
        public VertexPositionColorTexture[] shadowVertices = new VertexPositionColorTexture[4];
        public int[] shadowIndices = new int[6];

        //public VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];
        //public int[] invaderIndices = new int[6];

        public VertexPositionColorTexture[] normalVertices = new VertexPositionColorTexture[4];
        public int[] normalIndices = new int[6];
        #endregion

        #region Movement Related Variables
        public SlowStruct CurrentSlow;
        public FreezeStruct CurrentFreeze;
        public float Speed, SlowSpeed;
        public Vector2 Direction = new Vector2(-1f, 0);
        public Vector2 Velocity;

        //public SpriteEffects Orientation = SpriteEffects.None;

        private SpriteEffects _Orientation;
        public SpriteEffects Orientation 
        {
            get { return _Orientation; }
            set { _Orientation = value; }
        }
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
        
        #region HitByBeam
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
        #endregion

        #region Color
        private Color _Color;
        new public Color Color
        {
            get { return _Color; }
            set
            {
                _Color = value;

                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i].Color = value;
                }
            }
        }
        #endregion

        #region AnimationState
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
                    CurrentAnimation.GetFrameSize();

                    if (CurrentAnimation.CurrentInvaderState == AnimationState_Invader.Stand)
                        CurrentAnimation.CurrentFrame = 0;// Random.Next(0, CurrentAnimation.TotalFrames);

                    CurrentAnimation.CurrentFrameDelay = 0;
                }
            }
        }
        #endregion

        #region MacroBehaviour
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
        #endregion

        #region MicroBehaviour
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
        #endregion
        
        #region BehaviourDelay
        public double MaxBehaviourDelay = 1500;
        public double CurrentBehaviourDelay;
        #endregion

        #region HitObject
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
        #endregion

        public Texture2D CurrentEmotionIcon, FearEmotionIcon;

        public float NextYPos; //The point on the Y axis that the invader should move towards

        public Trap TargetTrap;
        public Invader TargetInvader;
        public Invader OperatingVehicle;

        public bool TargettingInvader = false;

        public DamageOverTimeStruct CurrentDOT;

        public InvaderType InvaderType;
        public InvaderAnimation CurrentAnimation;
        public List<InvaderAnimation> AnimationList;

        public UIOutline InvaderOutline;
        public UIBar HealthBar;
        public SoundEffectInstance MoveLoop;

        public Texture2D Shadow, IceBlock;
        public Vector2 YRange, Center, ShadowPosition;
        public Vector2 ResourceMinMax;

        public Color DOTColor, FrozenColor, ShadowColor;

        public InvaderMeleeStruct MeleeDamageStruct;

        public float MaxHP, CurrentHP, PreviousHP, Gravity, ShadowHeight, ShadowHeightMod;
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
        public Shield Shield;

        public int NeededOperators = 2;
        public int DeadOperators = 0; //Should maybe keep track of how many operators 
                                      //have died so that they don't ALL get lured away from the tower
        public List<Invader> OperatorList;// = new List<Invader>();

        public bool ShowDiagnostics = false;

        public Pathfinder Pathfinder;
        public List<Vector2> Waypoints = new List<Vector2>();
        public int CurrentWaypoint = 0;

        public bool TrapCollision = false;
        public bool TowerCollision = false;

        #endregion

        public Invader(Vector2 position, Vector2? yRange = null)
        {
            Position = position;

            if (yRange != null)
            {
                YRange = yRange.Value;
            }

        }

        public override void Initialize()
        {
            base.Active = true;            

            ResourceValue = Random.Next((int)ResourceMinMax.X, (int)ResourceMinMax.Y);

            //EmotionSprite = new VectorSprite(new Vector2(100, 100), new Vector2(32, 32), FearEmotionIcon, new Color(255, 0, 0, 25));

            if (NeededOperators > 0)
            {
                OperatorList = new List<Invader>();
                //OperatorList.Add(null);
                //OperatorList.Add(null);
            }

            CurrentHP = MaxHP;
            //MaxY = Random.Next((int)YRange.X, (int)YRange.Y);
            NextYPos = MaxY;
            PreviousMaxY = MaxY;
            
            if (Airborne == true)
            {
                Position.Y = MaxY;
            }

            Velocity = Direction * Speed;

            HealthBar = new UIBar(new Vector2(100, 100), new Vector2(32, 4), Color.DarkRed, false, true);

            Color = Color.White;

            if (IntelligenceRange != Vector2.Zero)
            {
                Intelligence = (Random.Next((int)IntelligenceRange.X * 100, (int)IntelligenceRange.Y * 100))/10;
            }

            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y,
                                                 (int)(CurrentAnimation.FrameSize.X),
                                                 (int)(CurrentAnimation.FrameSize.Y));

            #region Set up Vertices
            #region Sprite Vertices

            base.Initialize();
            vertices[0].TextureCoordinate = CurrentAnimation.dTopLeftTexCooord;
            vertices[1].TextureCoordinate = CurrentAnimation.dTopRightTexCoord;
            vertices[2].TextureCoordinate = CurrentAnimation.dBottomRightTexCoord;
            vertices[3].TextureCoordinate = CurrentAnimation.dBottomLeftTexCoord;
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

            BoundingBox = new BoundingBox(new Vector3(DestinationRectangle.Left + 6, DestinationRectangle.Top + 6, 0),
                                          new Vector3(DestinationRectangle.Right - 6, DestinationRectangle.Bottom, 0));

            Center = new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Center.Y);
        }

        public virtual void Update(GameTime gameTime, Vector2 cursorPosition) 
        {
            if (Active == true)
            {
                if (EmotionSprite != null)
                    EmotionSprite.Update(new Vector2(DestinationRectangle.Center.X - 16, DestinationRectangle.Top - 40));

                OperatorList.RemoveAll(Invader => Invader.Active == false);

                #region Hit a Trap
                Trap hitTrap = TrapList.FirstOrDefault(Trap => Trap.CollisionBox.Intersects(CollisionBox) && Trap.Solid == true);
                if (hitTrap != null)
                {
                    TargetTrap = hitTrap;
                    TrapCollision = true;
                }
                else
                {
                    //This next line should be handles on a per-invader type basis. i.e. Soldiers lose target when not colliding. Mobile cannon keeps target even when not colliding
                    //invader.TargetTrap = null;
                    TrapCollision = false;
                } 
                #endregion

                #region Hit the Tower
                if (Tower.BoundingBox.Intersects(BoundingBox))
                {
                    TowerCollision = true;
                }
                else
                {
                    TowerCollision = false;
                }                
                #endregion

                if (Waypoints.Count == 0)
                {
                    if ((BoundingBox.Max.Y + Velocity.Y) < MaxY)
                        Velocity.Y += Gravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                    if (Velocity.X != 0 && InvaderAnimationState != AnimationState_Invader.Walk)
                    {
                        InvaderAnimationState = AnimationState_Invader.Walk;
                    }

                    if (Velocity.X == 0 && InvaderAnimationState != AnimationState_Invader.Stand && Frozen == false)
                    {
                        var thing = this.InvaderType;
                        InvaderAnimationState = AnimationState_Invader.Stand;
                    }
                }

                if (CurrentBehaviourDelay <= MaxBehaviourDelay)
                {
                    CurrentBehaviourDelay += gameTime.ElapsedGameTime.TotalMilliseconds;
                    ThinkingAnimation.Position = new Vector2(Center.X - 12, DestinationRectangle.Top - 28);
                    ThinkingAnimation.Update(gameTime);
                }

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
                        Velocity.X = CurrentFreeze.UnfrozenVelocity.X;
                        CurrentFreeze = null;
                    }
                }
                #endregion

                #region Update position and vertices
                if (Velocity != Vector2.Zero)
                {
                    Position += Velocity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                    vertices[0].Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top, 0);
                    vertices[1].Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top, 0);
                    vertices[2].Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top + DestinationRectangle.Height, 0);
                    vertices[3].Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top + DestinationRectangle.Height, 0);

                    normalVertices[0].Position = vertices[0].Position;
                    normalVertices[1].Position = vertices[1].Position;
                    normalVertices[2].Position = vertices[2].Position;
                    normalVertices[3].Position = vertices[3].Position;
                }
                #endregion

                #region Draw the invader White if it's not frozen, burning etc.
                if (Frozen == false && Burning == false && HitByBeam == false)
                {
                    Color = Color.White;
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
                    DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y,
                                                         (int)(CurrentAnimation.FrameSize.X),
                                                         (int)(CurrentAnimation.FrameSize.Y));

                    BoundingBox = new BoundingBox(new Vector3(DestinationRectangle.Left + 6, DestinationRectangle.Top + 6, 0),
                                                  new Vector3(DestinationRectangle.Right - 6, DestinationRectangle.Bottom, 0));

                    CollisionBox = new BoundingBox(new Vector3(DestinationRectangle.Left + 6, DestinationRectangle.Top + DestinationRectangle.Height - ZDepth, 0),
                                                   new Vector3(DestinationRectangle.Right - 6, DestinationRectangle.Bottom, 0));

                    Center = new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Center.Y);
                    HealthBar.Update(MaxHP, CurrentHP, gameTime, new Vector2(Center.X + Velocity.X, Position.Y - 8));
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

                    switch (_Orientation)
                    {
                        case SpriteEffects.None:
                            {
                                vertices[0].TextureCoordinate = CurrentAnimation.dTopLeftTexCooord;
                                vertices[1].TextureCoordinate = CurrentAnimation.dTopRightTexCoord;
                                vertices[2].TextureCoordinate = CurrentAnimation.dBottomRightTexCoord;
                                vertices[3].TextureCoordinate = CurrentAnimation.dBottomLeftTexCoord;

                                normalVertices[0].TextureCoordinate = CurrentAnimation.nTopLeftTexCooord;
                                normalVertices[1].TextureCoordinate = CurrentAnimation.nTopRightTexCoord;
                                normalVertices[2].TextureCoordinate = CurrentAnimation.nBottomRightTexCoord;
                                normalVertices[3].TextureCoordinate = CurrentAnimation.nBottomLeftTexCoord;
                            }
                            break;

                        case SpriteEffects.FlipHorizontally:
                            {
                                vertices[0].TextureCoordinate = CurrentAnimation.dTopRightTexCoord;
                                vertices[1].TextureCoordinate = CurrentAnimation.dTopLeftTexCooord;
                                vertices[2].TextureCoordinate = CurrentAnimation.dBottomLeftTexCoord;
                                vertices[3].TextureCoordinate = CurrentAnimation.dBottomRightTexCoord;
                                                                
                                normalVertices[0].TextureCoordinate = CurrentAnimation.nTopRightTexCoord;
                                normalVertices[1].TextureCoordinate = CurrentAnimation.nTopLeftTexCooord;
                                normalVertices[2].TextureCoordinate = CurrentAnimation.nBottomLeftTexCoord;
                                normalVertices[3].TextureCoordinate = CurrentAnimation.nBottomRightTexCoord;                                
                            }
                            break;
                    }
                }
                #endregion

                #region Handle the invader selection outline
                if (InvaderOutline != null)
                {
                    InvaderOutline.Position = Position;// +new Vector2(0, 4);

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

                if (CurrentMicroBehaviour != MicroBehaviour.FollowWaypoints)
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

                Texture = CurrentAnimation.Texture;
            }
        }


        public override void Draw(GraphicsDevice graphics, BasicEffect effect, Effect shadowEffect)
        {
            if (Active == true)
            {
                if (EmotionSprite != null)
                    EmotionSprite.Draw(graphics, effect);

                //effect.TextureEnabled = true;
                //effect.VertexColorEnabled = true;
                //effect.Texture = CurrentAnimation.Texture;

                if (InAir == false)
                {
                    #region Draw invader shadows
                    //foreach (Light light in lightList)
                    //{
                    //    double dist = Math.Sqrt(Math.Pow(0.45f * (DestinationRectangle.Center.X - light.Position.X), 2) + Math.Pow(DestinationRectangle.Bottom - light.Position.Y, 2));

                    //    float lightDistance = Vector2.Distance(new Vector2(Center.X, DestinationRectangle.Bottom), new Vector2(light.Position.X, light.Position.Y));

                    //    lightDistance = (float)dist;

                    //    if (lightDistance < light.Range)
                    //    {
                    //        Vector2 direction = new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Bottom) - new Vector2(light.Position.X, light.Position.Y);
                    //        direction.Normalize();

                           
                    //        ShadowHeightMod = lightDistance / (light.Range / 10);
                    //        ShadowHeight = MathHelper.Clamp(CurrentAnimation.FrameSize.Y * ShadowHeightMod, 16, 64);
                    //        float width = MathHelper.Clamp(CurrentAnimation.FrameSize.Y * ShadowHeightMod, 16, 92);

                    //        ShadowColor = Color.Lerp(Color.Lerp(Color.Black, Color.Transparent, 0f), Color.Transparent, lightDistance / light.Radius);
                    //        foreach (Light light3 in lightList.FindAll(Light2 => Vector2.Distance(ShadowPosition, new Vector2(Light2.Position.X, Light2.Position.Y)) < light.Radius && Light2 != light).ToList())
                    //        {
                    //            ShadowColor *= MathHelper.Clamp(Vector2.Distance(new Vector2(light3.Position.X, light3.Position.Y), ShadowPosition) / light3.Radius, 0.8f, 1f);
                    //        }

                    //        shadowVertices[0] = new VertexPositionColorTexture()
                    //        {
                    //            Position = new Vector3(ShadowPosition.X, ShadowPosition.Y, 0),
                    //            TextureCoordinate = CurrentAnimation.dBottomLeftTexCoord,
                    //            Color = ShadowColor
                    //        };

                    //        shadowVertices[1] = new VertexPositionColorTexture()
                    //        {
                    //            Position = new Vector3(ShadowPosition.X + CurrentAnimation.FrameSize.X, ShadowPosition.Y, 0),
                    //            TextureCoordinate = CurrentAnimation.dBottomRightTexCoord,
                    //            Color = ShadowColor
                    //        };

                    //        shadowVertices[2] = new VertexPositionColorTexture()
                    //        {
                    //            Position = new Vector3(ShadowPosition.X + CurrentAnimation.FrameSize.X + (direction.X * width), ShadowPosition.Y + (direction.Y * ShadowHeight), 0),
                    //            TextureCoordinate = CurrentAnimation.dTopRightTexCoord,
                    //            Color = ShadowColor * 0.85f
                    //        };

                    //        shadowVertices[3] = new VertexPositionColorTexture()
                    //        {
                    //            Position = new Vector3(ShadowPosition.X + (direction.X * width), ShadowPosition.Y + (direction.Y * ShadowHeight), 0),
                    //            TextureCoordinate = CurrentAnimation.dTopLeftTexCooord,
                    //            Color = ShadowColor * 0.85f
                    //        };

                    //        //This stops backface culling when the shadow flips vertically
                    //        if (direction.Y > 0)
                    //        {
                    //            shadowIndices[0] = 0;
                    //            shadowIndices[1] = 1;
                    //            shadowIndices[2] = 2;
                    //            shadowIndices[3] = 2;
                    //            shadowIndices[4] = 3;
                    //            shadowIndices[5] = 0;
                    //        }
                    //        else
                    //        {
                    //            shadowIndices[0] = 3;
                    //            shadowIndices[1] = 2;
                    //            shadowIndices[2] = 1;
                    //            shadowIndices[3] = 1;
                    //            shadowIndices[4] = 0;
                    //            shadowIndices[5] = 3;
                    //        }

                    //        shadowEffect.Parameters["Texture"].SetValue(CurrentAnimation.Texture);
                    //        shadowEffect.Parameters["texSize"].SetValue(new Vector2(CurrentAnimation.Texture.Width, CurrentAnimation.Texture.Height));

                    //        foreach (EffectPass pass in shadowEffect.CurrentTechnique.Passes)
                    //        {
                    //            pass.Apply();
                    //            graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, shadowVertices, 0, 4, shadowIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                    //        }
                    //    }
                    //}
                    #endregion
                }

                #region Draw invader sprite
                //foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                //{
                //    pass.Apply();
                //    graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                //}
                base.Draw(graphics, effect, shadowEffect);

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
                    graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
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

        public void UpdateMeleeDelay(GameTime gameTime)
        {
            //This should only be called if the invader is actually ALLOWED to fire
            //i.e. They can't fire when moving, can't fire when facing the wrong way etc.
            if (MeleeDamageStruct != null)
            {
                if (MeleeDamageStruct.CurrentAttackDelay < MeleeDamageStruct.MaxAttackDelay)
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

        public void ExplosionDamage(int change)
        {
            if (Active == true)
            {
                if (VulnerableToTurret == true)
                {
                    if (Shield == null || Shield.ShieldOn == false)
                    {
                        CurrentHP -= change;
                    }
                    else
                        if (Shield.ShieldOn == true)
                        {
                            Shield.TakeDamage(change);
                        }
                }
            }
        }

        public void TurretDamage(int change)
        {
            if (Active == true)
            {
                if (VulnerableToTurret == true)
                {
                    CurrentHP += change;
                }
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
                CurrentFreeze.UnfrozenVelocity = Velocity;
            }
        }

        public void Trajectory(Vector2 velocity)
        {
            if (velocity.Y < 0)
                InAir = true;

            Velocity = velocity;
        }


        public void SetOperatingVehicle(Invader operatingVehicle)
        {
            OperatingVehicle = operatingVehicle;
        }
    }
}
