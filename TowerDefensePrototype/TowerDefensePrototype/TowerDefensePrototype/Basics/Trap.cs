using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public abstract class Trap
    {
        public Texture2D Texture;
        public Rectangle DestinationRectangle, SourceRectangle;
        public BoundingBox BoundingBox;
        public TrapType TrapType;
        public List<Emitter> TrapEmitterList = new List<Emitter>();
        public HorizontalBar TimingBar, HealthBar, DetonateBar;
        public Vector2 Scale, FrameSize, Position;
        public Animation CurrentAnimation;
        public bool Active, Solid, CanTrigger, Animated, Affected;
        public float MaxHP, CurrentHP, DetonateDelay, CurrentDetonateDelay, AffectedTime, CurrentAffectedTime, DrawDepth, Bottom;        
        public int ElapsedTime, FrameTime, FrameCount, CurrentFrame, ResourceCost, Radius, DetonateLimit, CurrentDetonateLimit;

        public virtual void Initialize()
        {
            Active = true;

            TimingBar = new HorizontalBar(new Vector2(32, 4), (int)DetonateDelay, (int)CurrentDetonateDelay, Color.Green, Color.DarkRed);
            HealthBar = new HorizontalBar(new Vector2(32, 4), (int)MaxHP, (int)CurrentHP, Color.Green, Color.DarkRed);

            CurrentDetonateLimit = DetonateLimit;
            CurrentDetonateDelay = DetonateDelay;
            Affected = false;
            CurrentAffectedTime = AffectedTime;
            CurrentHP = MaxHP;

            if (Animated == false)
            {
                FrameCount = 1;
            }

            FrameSize = new Vector2(Texture.Width / FrameCount, Texture.Height);
            BoundingBox = new BoundingBox(new Vector3((int)(Position.X - FrameSize.X / 2), (int)(Position.Y - FrameSize.Y), 0), 
                                          new Vector3((int)(Position.X - FrameSize.X / 2) + FrameSize.X, (int)(Position.Y - FrameSize.Y) + FrameSize.Y, 0));
            ElapsedTime = 0;
            CurrentFrame = 0;

            Bottom = BoundingBox.Max.Y;
            DrawDepth = Bottom / 1080;

            foreach (Emitter emitter in TrapEmitterList)
            {
                emitter.DrawDepth = Bottom / 1080;
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            if (CurrentDetonateDelay < DetonateDelay)
            CurrentDetonateDelay += gameTime.ElapsedGameTime.Milliseconds;

            CurrentAffectedTime += gameTime.ElapsedGameTime.Milliseconds;

            if (Animated == true)
            {
                ElapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (ElapsedTime > FrameTime)
                {
                    CurrentFrame++;

                    if (CurrentFrame == FrameCount)
                        CurrentFrame = 0;

                    ElapsedTime = 0;
                }
            }

            //DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y - Texture.Height, (int)(Texture.Width), (int)(Texture.Height));
            SourceRectangle = new Rectangle(CurrentFrame * (int)FrameSize.X, 0, (int)FrameSize.X, (int)FrameSize.Y);
            DestinationRectangle = new Rectangle((int)BoundingBox.Min.X, (int)BoundingBox.Min.Y, (int)(BoundingBox.Max.X - BoundingBox.Min.X), (int)(BoundingBox.Max.Y - BoundingBox.Min.Y));

            if (CurrentAffectedTime >= AffectedTime)
            {
                Affected = false;
            }
            else
            {
                Affected = true;
            }

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

            if (CurrentDetonateLimit == -1)
            {
                Active = true;
            }

            TimingBar.Update(new Vector2(DestinationRectangle.X, DestinationRectangle.Bottom + 16), (int)CurrentDetonateDelay);
            HealthBar.Update(new Vector2(DestinationRectangle.X, DestinationRectangle.Bottom + 24), (int)CurrentHP); 

            if (TrapEmitterList.Count > 0)
            {
                foreach (Emitter emitter in TrapEmitterList)
                {
                    emitter.Update(gameTime);
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentHP <= 0)
                Active = false;
            
            if (TrapEmitterList.Count > 0)
            {
                foreach (Emitter emitter in TrapEmitterList)
                {
                    emitter.Draw(spriteBatch);
                }
            }

            if (Active == true)
            {
                spriteBatch.Draw(Texture, DestinationRectangle, SourceRectangle, Color.White, MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, DrawDepth);
            }
        }
    }
}
