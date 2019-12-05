using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    //NEW_TRAP A **trap type to be added here**
    public enum TrapType
    {
        Wall,
        Spikes,
        Catapult,
        Fire, Ice,
        Barrel,
        SawBlade,
        Trigger,
        LandMine,
        FlameThrower,
        Glue
    };

    public enum TrapAnimationState { Untriggered, Triggering, Active, Resetting };

    public abstract class Trap : Drawable
    {
        //public Texture2D CurrentTexture;
        public Texture2D AmbientShadowTexture;
        //public Rectangle SourceRectangle;
        public TrapType TrapType;
        //public List<Emitter> TrapEmitterList = new List<Emitter>();
        public Vector2 ShadowPosition, Center;
        public Vector2 Scale = new Vector2(1, 1);
        public bool Solid, CanTrigger;
        public bool OnGround = false; //Whether the trap is drawn on the ground layer on if it's sorted by depth as usual
        public float MaxHP, CurrentHP, DetonateDelay, CurrentDetonateDelay, Bottom;
        public int DetonateLimit, CurrentDetonateLimit, CurrentFrame, PowerCost;
        public static int ResourceCost;
        public double CurrentFrameDelay;
        public double CurrentRemovalTime;
        public double MaxRemovalTime = 1000;
        public UIBar TimingBar, HealthBar;

        public float ChanceToFear = 0;

        //What the traps can do to the invaders upon collision
        public DamageOverTimeStruct InvaderDOT;
        public SlowStruct InvaderSlow;
        public FreezeStruct InvaderFreeze;
        public float NormalDamage;

        public UITrapQuickInfo TrapQuickInfo;
        public UIOutline TrapOutline;

        public TrapAnimation CurrentAnimation;
        public List<TrapAnimation> AnimationList;

        private TrapAnimationState _TrapState;
        public TrapAnimationState TrapState
        {
            get { return _TrapState; }
            set
            {
                _TrapState = value;

                if (AnimationList != null)
                {
                    CurrentAnimation = AnimationList.Find(Animation => Animation.CurrentTrapState == value);

                    if (CurrentAnimation.CurrentTrapState == TrapAnimationState.Untriggered)
                        CurrentAnimation.CurrentFrame = Random.Next(0, CurrentAnimation.TotalFrames);

                    //CurrentAnimation.CurrentFrame = 0;
                    CurrentAnimation.CurrentFrameDelay = 0;
                }
            }
        }

        #region Vertex declarations
        public VertexPositionColorTexture[] shadowVertices = new VertexPositionColorTexture[4];
        public int[] shadowIndices = new int[6];

        //public VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];
        //public int[] indices = new int[6];

        public VertexPositionColorTexture[] normalVertices = new VertexPositionColorTexture[4];
        public int[] normalIndices = new int[6];
        #endregion

        //public Color Color = Color.White;

        public Trap(Vector2 position)
        {
            Position = position;
        }

        public override void Initialize()
        {
            Active = true;

            CurrentHP = MaxHP;

            CurrentDetonateLimit = DetonateLimit;
            CurrentDetonateDelay = DetonateDelay;
            //CurrentAffectedTime = AffectedTime;

            DestinationRectangle = new Rectangle((int)(Position.X), (int)(Position.Y), (int)CurrentAnimation.FrameSize.X, (int)CurrentAnimation.FrameSize.Y);
            Center = new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Y);

            TimingBar = new UIBar(new Vector2(Center.X, Position.Y + CurrentAnimation.FrameSize.Y + 4), new Vector2(32, 4), Color.DodgerBlue, false, true);
            HealthBar = new UIBar(new Vector2(Center.X, Position.Y + CurrentAnimation.FrameSize.Y + 8), new Vector2(32, 4), Color.White, false, true);

            #region Sprite Vertices
            base.Initialize();
            vertices[0].TextureCoordinate = CurrentAnimation.dTopLeftTexCooord;
            vertices[1].TextureCoordinate = CurrentAnimation.dTopRightTexCoord;
            vertices[2].TextureCoordinate = CurrentAnimation.dBottomRightTexCoord;
            vertices[3].TextureCoordinate = CurrentAnimation.dBottomLeftTexCoord;
            #endregion

            if (OnGround == false)
            {
                DrawDepth = (float)(DestinationRectangle.Bottom / 1080f);
                MaxY = DestinationRectangle.Bottom;
                PreviousMaxY = MaxY;
            }
            //Affected = false;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (CurrentHP <= 0)
                Active = false;

            if (CurrentAnimation.TotalFrames > 1)
            {
                CurrentAnimation.Update(gameTime);
            }

            //#region Handle how long the trap stays affected by outside stimulus for
            //if (CurrentAffectedTime < AffectedTime)
            //    CurrentAffectedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            //if (CurrentAffectedTime >= AffectedTime)
            //{
            //    Affected = false;
            //}
            //else
            //{
            //    Affected = true;
            //}
            //#endregion

            vertices[0].TextureCoordinate = CurrentAnimation.dTopLeftTexCooord;
            vertices[1].TextureCoordinate = CurrentAnimation.dTopRightTexCoord;
            vertices[2].TextureCoordinate = CurrentAnimation.dBottomRightTexCoord;
            vertices[3].TextureCoordinate = CurrentAnimation.dBottomLeftTexCoord;

            #region Handle the timing between detonations and detonate limits
            if (CurrentDetonateDelay < DetonateDelay)
                CurrentDetonateDelay += gameTime.ElapsedGameTime.Milliseconds;

            if (CurrentDetonateDelay >= DetonateDelay)
            {
                CanTrigger = true;                
            }  
            else
            {
                CanTrigger = false;
            }

            if (CurrentDetonateLimit == 0)
            {
                Active = false;
            }

            //The trap has no limit on it's detonations
            if (CurrentDetonateLimit == -1)
            {
                Active = true;
            }
            #endregion

            TimingBar.Update((float)DetonateDelay, (float)CurrentDetonateDelay);
            HealthBar.Update((float)MaxHP, (float)CurrentHP);

            switch (TrapType)
            {
                default:
                    {
                        BoundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, 0),
                                             new Vector3(Position.X + CurrentAnimation.FrameSize.X,
                                                         Position.Y + CurrentAnimation.FrameSize.Y, 0));
                    }
                    break;

                case TrapType.Spikes:
                    {
                        BoundingBox = new BoundingBox(new Vector3(Position.X + 6, Position.Y, 0),
                                             new Vector3(Position.X + CurrentAnimation.FrameSize.X - 30,
                                                         Position.Y + CurrentAnimation.FrameSize.Y, 0));
                    }
                    break;
            }

            //BoundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, 0),
            //                              new Vector3(Position.X + CurrentAnimation.FrameSize.X, 
            //                                          Position.Y + CurrentAnimation.FrameSize.Y, 0));

            //switch (TrapType)
            //{
            //    default:
            //        CollisionBox = new BoundingBox(new Vector3(Position.X, Position.Y + CurrentAnimation.FrameSize.Y - ZDepth, 0),
            //                               new Vector3(Position.X + CurrentAnimation.FrameSize.X,
            //                                           Position.Y + CurrentAnimation.FrameSize.Y, 0));
            //        break;

            //    case TrapType.Spikes:
            //        CollisionBox = new BoundingBox(new Vector3(Position.X + 6, Position.Y + CurrentAnimation.FrameSize.Y - ZDepth, 0),
            //                               new Vector3(Position.X + CurrentAnimation.FrameSize.X - 6,
            //                                           Position.Y + CurrentAnimation.FrameSize.Y, 0));
            //        break;
            //}

            CollisionBox = new BoundingBox(new Vector3(Position.X, Position.Y + CurrentAnimation.FrameSize.Y - ZDepth, 0),
                                           new Vector3(Position.X + CurrentAnimation.FrameSize.X,
                                                       Position.Y + CurrentAnimation.FrameSize.Y, 0));

            ShadowPosition = new Vector2(Position.X + CurrentAnimation.FrameSize.X/2, Position.Y + CurrentAnimation.FrameSize.Y);
                        
            Bottom = BoundingBox.Max.Y;

            if (OnGround == false)
            {
                DrawDepth = (Bottom / 1080);
                MaxY = Bottom;
            }
            else
            {
                DrawDepth = 0f;
            }

            Texture = CurrentAnimation.Texture;
        }


        public override void Draw(GraphicsDevice graphics, BasicEffect effect, Effect shadowEffect)
        {
            //Draw the colour map using a basic effect
            if (Active == true)
            {
                base.Draw(graphics, effect, shadowEffect);
            }
        }

        public override void DrawSpriteDepth(GraphicsDevice graphics, Effect effect)
        {
            //Draw the same sprite as the color map, but with the depth effect applied
            if (Active == true)
            {
                effect.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(0, 0, 0)));
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
            //Draw the lower half of the sprite (The normal map) with a basic effect applied
            if (Active == true)
                if (CurrentAnimation.AnimationType == AnimationType.Normal || CurrentAnimation.AnimationType == AnimationType.Emissive)
                {
                    effect.TextureEnabled = true;
                    effect.VertexColorEnabled = true;
                    effect.Texture = CurrentAnimation.Texture;

                    normalVertices[0] = new VertexPositionColorTexture()
                    {
                        Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top, 0),
                        TextureCoordinate = CurrentAnimation.nTopLeftTexCooord,
                        Color = Color
                    };

                    normalVertices[1] = new VertexPositionColorTexture()
                    {
                        Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top, 0),
                        TextureCoordinate = CurrentAnimation.nTopRightTexCoord,
                        Color = Color
                    };

                    normalVertices[2] = new VertexPositionColorTexture()
                    {
                        Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                        TextureCoordinate = CurrentAnimation.nBottomRightTexCoord,
                        Color = Color
                    };

                    normalVertices[3] = new VertexPositionColorTexture()
                    {
                        Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                        TextureCoordinate = CurrentAnimation.nBottomLeftTexCoord,
                        Color = Color
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
        }


        public void DrawBars(GraphicsDevice graphicsDevice, BasicEffect basicEffect)
        {
            if (Active == true)
            {
                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    if (DetonateDelay > 0)
                        TimingBar.Draw(graphicsDevice);

                    HealthBar.Draw(graphicsDevice);
                }
            }
        }
    }
}
