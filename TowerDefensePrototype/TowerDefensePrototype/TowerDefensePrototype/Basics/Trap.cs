using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public abstract class Trap : Drawable
    {
        //public Texture2D CurrentTexture;
        
        public Rectangle DestinationRectangle, SourceRectangle;
        public BoundingBox BoundingBox;
        public TrapType TrapType;
        public List<Emitter> TrapEmitterList = new List<Emitter>();
        public Vector2 Position;
        public Vector2 Scale = new Vector2(1, 1);
        public bool Solid, CanTrigger, Affected;
        public float MaxHP, CurrentHP, DetonateDelay, CurrentDetonateDelay, AffectedTime, CurrentAffectedTime, Bottom;
        public int ResourceCost, DetonateLimit, CurrentDetonateLimit, CurrentFrame;
        public double CurrentFrameDelay;
        public static Random Random = new Random();
        public UIBar TimingBar, HealthBar;
        
        //What the traps can do to the invaders upon collision
        public DamageOverTimeStruct InvaderDOT;
        public SlowStruct InvaderSlow;
        public FreezeStruct InvaderFreeze;
        public float NormalDamage;

        public TrapAnimation CurrentAnimation;
        public List<TrapAnimation> AnimationList;

        private TrapState _TrapState;
        public TrapState TrapState
        {
            get { return _TrapState; }
            set
            {
                _TrapState = value;

                if (AnimationList != null)
                {
                    CurrentAnimation = AnimationList.Find(Animation => Animation.CurrentTrapState == value);

                    if (CurrentAnimation.CurrentTrapState == TrapState.Untriggered)
                        CurrentAnimation.CurrentFrame = Random.Next(0, CurrentAnimation.TotalFrames);

                    CurrentAnimation.CurrentFrameDelay = 0;
                }
            }
        }

        public virtual void Initialize()
        {
            Active = true;

            CurrentHP = MaxHP;

            TimingBar = new UIBar(new Vector2(Position.X, Position.Y + CurrentAnimation.FrameSize.Y + 4), new Vector2(32, 4), Color.DodgerBlue);
            HealthBar = new UIBar(new Vector2(Position.X, Position.Y + CurrentAnimation.FrameSize.Y + 8), new Vector2(32, 4), Color.White); 

            CurrentDetonateLimit = DetonateLimit;
            CurrentDetonateDelay = DetonateDelay;
            CurrentAffectedTime = AffectedTime;

            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)CurrentAnimation.FrameSize.X, (int)CurrentAnimation.FrameSize.Y);

            DrawDepth = (float)(DestinationRectangle.Bottom / 1080f);
            Affected = false;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (CurrentHP <= 0)
                Active = false;

            if (CurrentAnimation.TotalFrames > 1)
            {
                CurrentAnimation.Update(gameTime);
            }

            #region Handle how long the trap stays affected by outside stimulus for
            if (CurrentAffectedTime < AffectedTime)
                CurrentAffectedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentAffectedTime >= AffectedTime)
            {
                Affected = false;
            }
            else
            {
                Affected = true;
            }
            #endregion

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
                if (TrapEmitterList.Count > 0)
                {
                    foreach (Emitter emitter in TrapEmitterList)
                        emitter.AddMore = false;
                }
                else
                {
                    Active = false;
                }
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

            Bottom = BoundingBox.Max.Y;
            DrawDepth = (Bottom / 1080);


            if (TrapEmitterList.Count > 0)
            {
                foreach (Emitter emitter in TrapEmitterList)
                {
                    emitter.Update(gameTime);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (TrapEmitterList.Count > 0)
            {
                foreach (Emitter emitter in TrapEmitterList)
                {
                    emitter.Draw(spriteBatch);
                }
            }

            if (Active == true)
            {
                spriteBatch.Draw(CurrentAnimation.Texture, DestinationRectangle, CurrentAnimation.DiffuseSourceRectangle, Color.White, 
                                 MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, DrawDepth);
            }
        }

        public override void DrawSpriteNormal(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                spriteBatch.Draw(CurrentAnimation.Texture, DestinationRectangle, CurrentAnimation.NormalSourceRectangle, Color.White,
                                 MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, DrawDepth);
            }
        }

        public override void DrawSpriteDepth(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(CurrentAnimation.Texture, DestinationRectangle, CurrentAnimation.DiffuseSourceRectangle, new Color(DrawDepth * 255, DrawDepth * 255, DrawDepth * 255));
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
