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
        Texture2D Texture, Texture2;
        public Rectangle DestinationRectangle, SourceRectangle;
        public String AssetName;
        public float MaxHP, CurrentHP;
        public Vector2 Scale, FrameSize, Position;
        public bool Active, Solid, CanTrigger, Animated;
        public BoundingBox BoundingBox;
        public TrapType TrapType;
        public List<Emitter> TrapEmitterList;
        public float DetonateDelay, CurrentDetonateDelay;
        public HorizontalBar TimingBar, HealthBar, DetonateBar;
        public float DetonateLimit, CurrentDetonateLimit;
        public bool Affected;
        public float AffectedTime, CurrentAffectedTime;
        public float DrawDepth, Bottom;
        public int ElapsedTime, FrameTime, FrameCount, CurrentFrame;

        public virtual void LoadContent(ContentManager contentManager)
        {
            Active = true;                   
            TimingBar = new HorizontalBar(contentManager, new Vector2(32, 4), (int)DetonateDelay, (int)CurrentDetonateDelay, Color.Green, Color.DarkRed);
            HealthBar = new HorizontalBar(contentManager, new Vector2(32, 4), (int)MaxHP, (int)CurrentHP, Color.Green, Color.DarkRed);
            TrapEmitterList = new List<Emitter>();
            Texture = contentManager.Load<Texture2D>(AssetName);
            Texture2 = contentManager.Load<Texture2D>("TrapDetBlock");
            BoundingBox = new BoundingBox(new Vector3((int)Position.X, (int)Position.Y, 0), new Vector3((int)Position.X + Texture.Width, (int)Position.Y - Texture.Height, 0));
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
            ElapsedTime = 0;
            CurrentFrame = 0;
        }

        public virtual void Update(GameTime gameTime)
        {
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
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y - (int)FrameSize.Y, (int)FrameSize.X, (int)FrameSize.Y);


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

            Bottom = DestinationRectangle.Bottom;

            DrawDepth = Bottom / 720;
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

                for (int i = 0; i < CurrentDetonateLimit; i++)
                {
                    spriteBatch.Draw(Texture2, new Rectangle((int)Position.X+(6*i), (int)Position.Y + 32, Texture2.Width, Texture2.Height), Color.White);
                }
            }
        }
    }
}
