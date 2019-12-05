using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public enum TrapType
    {
        Wall,
        Spikes,
        Catapult,
        Fire, Ice,
        Barrel,
        SawBlade,
        Line,
        Trigger
    };
    public enum TrapAnimationState { Untriggered, Triggering, Active, Resetting };

    public abstract class Trap : Drawable
    {
        //public Texture2D CurrentTexture;
        
        public Rectangle DestinationRectangle, SourceRectangle;
        public TrapType TrapType;
        //public List<Emitter> TrapEmitterList = new List<Emitter>();
        public Vector2 Position, ShadowPosition, Center;
        public Vector2 Scale = new Vector2(1, 1);
        public bool Solid, CanTrigger;
        public bool OnGround = false; //Whether the trap is drawn on the ground layer on if it's sorted by depth as usual
        public float MaxHP, CurrentHP, DetonateDelay, CurrentDetonateDelay, Bottom;
        public int ResourceCost, DetonateLimit, CurrentDetonateLimit, CurrentFrame, PowerCost;
        public double CurrentFrameDelay;
        public double CurrentRemovalTime;
        public double MaxRemovalTime = 2000;
        public static Random Random = new Random();
        public UIBar TimingBar, HealthBar;
        
        //What the traps can do to the invaders upon collision
        public DamageOverTimeStruct InvaderDOT;
        public SlowStruct InvaderSlow;
        public FreezeStruct InvaderFreeze;
        public float NormalDamage;

        public UITrapQuickInfo TrapQuickInfo;

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

                    CurrentAnimation.CurrentFrameDelay = 0;
                }
            }
        }

        #region Vertex declarations
        public VertexPositionColorTexture[] shadowVertices = new VertexPositionColorTexture[4];
        public int[] shadowIndices = new int[6];

        public VertexPositionColorTexture[] trapVertices = new VertexPositionColorTexture[4];
        public int[] trapIndices = new int[6];

        public VertexPositionColorTexture[] normalVertices = new VertexPositionColorTexture[4];
        public int[] normalIndices = new int[6];
        #endregion

        Color shadowColor;
        float height, heightMod;

        public virtual void Initialize()
        {
            Active = true;

            CurrentHP = MaxHP;

            TimingBar = new UIBar(new Vector2(Position.X, Position.Y + CurrentAnimation.FrameSize.Y + 4), new Vector2(32, 4), Color.DodgerBlue);
            HealthBar = new UIBar(new Vector2(Position.X, Position.Y + CurrentAnimation.FrameSize.Y + 8), new Vector2(32, 4), Color.White); 

            CurrentDetonateLimit = DetonateLimit;
            CurrentDetonateDelay = DetonateDelay;
            //CurrentAffectedTime = AffectedTime;

            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)CurrentAnimation.FrameSize.X, (int)CurrentAnimation.FrameSize.Y);
            Center = new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Y);

            DrawDepth = (float)(DestinationRectangle.Bottom / 1080f);
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

            BoundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, 0),
                                          new Vector3(Position.X + CurrentAnimation.FrameSize.X, Position.Y + CurrentAnimation.FrameSize.Y, 0));

            ShadowPosition = new Vector2(Position.X + CurrentAnimation.FrameSize.X/2, Position.Y + CurrentAnimation.FrameSize.Y);
                        
            Bottom = BoundingBox.Max.Y;
            DrawDepth = (Bottom / 1080);


            //if (TrapEmitterList.Count > 0)
            //{
            //    foreach (Emitter emitter in TrapEmitterList)
            //    {
            //        emitter.Update(gameTime);
            //    }
            //}
        }


        public override void Draw(GraphicsDevice graphics, BasicEffect effect, Effect shadowEffect, List<Light> lightList)
        {
            //Draw the colour map using a basic effect
            if (Active == true)
            {
                effect.TextureEnabled = true;
                effect.VertexColorEnabled = true;
                effect.Texture = CurrentAnimation.Texture;

                #region Draw trap shadows
                foreach (Light light in lightList)
                {
                    float lightDistance = Vector2.Distance(ShadowPosition, new Vector2(light.Position.X, light.Position.Y));

                    if (lightDistance < light.LightDecay)
                    {
                        Vector2 direction = ShadowPosition - new Vector2(light.Position.X, light.Position.Y);
                        direction.Normalize();

                        heightMod = lightDistance / (light.Range / 10);
                        height = MathHelper.Clamp(CurrentAnimation.FrameSize.Y * heightMod, 16, 64);
                        float width = MathHelper.Clamp(CurrentAnimation.FrameSize.Y * heightMod, 16, 92);

                        shadowColor = Color.Lerp(Color.Lerp(Color.Black, Color.Transparent, 0f), Color.Transparent, lightDistance / light.Radius);
                        foreach (Light light3 in lightList.FindAll(Light2 => Vector2.Distance(ShadowPosition, new Vector2(Light2.Position.X, Light2.Position.Y)) < light.Radius && Light2 != light).ToList())
                        {
                            shadowColor *= MathHelper.Clamp(Vector2.Distance(new Vector2(light3.Position.X, light3.Position.Y), ShadowPosition) / light3.Radius, 0.8f, 1f);
                        }

                        shadowVertices[0] = new VertexPositionColorTexture()
                        {
                            Position = new Vector3(ShadowPosition.X - CurrentAnimation.FrameSize.X/2, ShadowPosition.Y, 0),
                            TextureCoordinate = CurrentAnimation.dBottomLeftTexCoord,
                            Color = shadowColor
                        };

                        shadowVertices[1] = new VertexPositionColorTexture()
                        {
                            Position = new Vector3(ShadowPosition.X + CurrentAnimation.FrameSize.X/2, ShadowPosition.Y, 0),
                            TextureCoordinate = CurrentAnimation.dBottomRightTexCoord,
                            Color = shadowColor
                        };

                        shadowVertices[2] = new VertexPositionColorTexture()
                        {
                            Position = new Vector3(ShadowPosition.X + CurrentAnimation.FrameSize.X/2 + (direction.X * width), ShadowPosition.Y + (direction.Y * height), 0),
                            TextureCoordinate = CurrentAnimation.dTopRightTexCoord,
                            Color = Color.Lerp(shadowColor, Color.Transparent, 0.85f)
                        };

                        shadowVertices[3] = new VertexPositionColorTexture()
                        {
                            Position = new Vector3(ShadowPosition.X - CurrentAnimation.FrameSize.X / 2 + (direction.X * width), ShadowPosition.Y + (direction.Y * height), 0),
                            TextureCoordinate = CurrentAnimation.dTopLeftTexCooord,
                            Color = Color.Lerp(shadowColor, Color.Transparent, 0.85f)
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

                #region Draw trap sprite
                trapVertices[0] = new VertexPositionColorTexture()
                {
                    Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top, 0),
                    TextureCoordinate = CurrentAnimation.dTopLeftTexCooord,
                    Color = Color.White
                };

                trapVertices[1] = new VertexPositionColorTexture()
                {
                    Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top, 0),
                    TextureCoordinate = CurrentAnimation.dTopRightTexCoord,
                    Color = Color.White
                };

                trapVertices[2] = new VertexPositionColorTexture()
                {
                    Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                    TextureCoordinate = CurrentAnimation.dBottomRightTexCoord,
                    Color = Color.White
                };

                trapVertices[3] = new VertexPositionColorTexture()
                {
                    Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                    TextureCoordinate = CurrentAnimation.dBottomLeftTexCoord,
                    Color = Color.White
                };

                trapIndices[0] = 0;
                trapIndices[1] = 1;
                trapIndices[2] = 2;
                trapIndices[3] = 2;
                trapIndices[4] = 3;
                trapIndices[5] = 0;

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, trapVertices, 0, 4, trapIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);                    
                }
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
                    graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, trapVertices, 0, 4, trapIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
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
