using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public enum TrapState { Untriggered, Triggering, Active, Resetting };

    public abstract class Trap
    {
        public Texture2D CurrentTexture;
        public List<Texture2D> TextureList;
        public Animation CurrentAnimation;
        public Rectangle DestinationRectangle, SourceRectangle;
        public BoundingBox BoundingBox;
        public TrapType TrapType;
        public List<Emitter> TrapEmitterList = new List<Emitter>();
        public Vector2 Position, FrameSize;
        public Vector2 Scale = new Vector2(1, 1);
        public bool Active, Solid, CanTrigger, Affected;
        public float MaxHP, CurrentHP, DetonateDelay, CurrentDetonateDelay, AffectedTime, CurrentAffectedTime, Bottom, DrawDepth;
        public int ResourceCost, DetonateLimit, CurrentDetonateLimit, CurrentFrame;
        public double CurrentFrameDelay;
        public static Random Random = new Random();
        public UIBar TimingBar, HealthBar;
        
        //What the traps can do to the invaders upon collision
        public DamageOverTimeStruct InvaderDOT;
        public SlowStruct InvaderSlow;
        public FreezeStruct InvaderFreeze;
        public float NormalDamage;

        public virtual void Initialize()
        {
            Active = true;

            CurrentHP = MaxHP;

            TimingBar = new UIBar(new Vector2(Position.X, Position.Y + TextureList[0].Height + 4), new Vector2(32, 4), Color.DodgerBlue);
            HealthBar = new UIBar(new Vector2(Position.X, Position.Y + TextureList[0].Height + 8), new Vector2(32, 4), Color.White); 

            CurrentDetonateLimit = DetonateLimit;
            CurrentDetonateDelay = DetonateDelay;
            CurrentAffectedTime = AffectedTime;

            //This is mostly here so that traps can have an effect around them when placed, such as a dust burst
            if (CurrentAnimation == null && CurrentTexture != null)
            {
                FrameSize = new Vector2(CurrentTexture.Width, CurrentTexture.Height);
                DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)FrameSize.X, (int)FrameSize.Y);
            }

            Affected = false;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (CurrentHP <= 0)
                Active = false;

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

            //Handle the animations
            //if (CurrentTrapState != PreviousTrapState)
            //{
            //    CurrentFrameDelay = 0;
            //    CurrentFrame = Random.Next(0, CurrentAnimation.TotalFrames);
            //}

            //This is so that if the animation bugs out I can know which trap is the problem.
            TrapType traptype = TrapType;

            if (CurrentAnimation != null)
            {
                FrameSize = new Vector2(CurrentTexture.Width / CurrentAnimation.TotalFrames, CurrentTexture.Height);
            }
            else
            {
                FrameSize = new Vector2(CurrentTexture.Width, CurrentTexture.Height);
            }

            SourceRectangle = new Rectangle((int)(CurrentFrame * FrameSize.X), 0, (int)FrameSize.X, (int)FrameSize.Y);
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y,
                                                 (int)(FrameSize.X * Scale.X), (int)(FrameSize.Y * Scale.Y));

            BoundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, 0),
                                          new Vector3(Position.X + FrameSize.X, Position.Y + FrameSize.Y, 0));

            Bottom = BoundingBox.Max.Y;
            DrawDepth = (Bottom / 1080);

            //Bottom = BoundingBox.Max.Y;
            //--------------------------------//


            if (TrapEmitterList.Count > 0)
            {
                foreach (Emitter emitter in TrapEmitterList)
                {
                    emitter.Update(gameTime);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
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
                spriteBatch.Draw(CurrentTexture, DestinationRectangle, SourceRectangle, Color.White, 
                                 MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, DrawDepth);
            }
        }

        public void DrawBars(GraphicsDevice graphicsDevice, BasicEffect basicEffect)
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
