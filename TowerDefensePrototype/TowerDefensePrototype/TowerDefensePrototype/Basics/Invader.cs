using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public abstract class Invader
    {
        public string AssetName;
        public Texture2D CurrentTexture;
        public Rectangle DestinationRectangle, SourceRectangle;
        public Vector2 Position, MoveVector, ResourceMinMax, Velocity;
        public bool Active, CanMove, VulnerableToTurret, VulnerableToTrap, CanAttack;
        public Color Color;
        public BoundingBox BoundingBox;
        public Double MoveDelay, CurrentDelay, AttackDelay, CurrentAttackDelay;
        public int MaxHP, CurrentHP, ResourceValue;        
        public abstract void TrapDamage(TrapType trapType);
        public int AttackPower;
        public Random Random;
        public float Gravity;
        HorizontalBar HPBar;
        public InvaderType InvaderType;
        public int TotalFrames, CurrentFrame;
        public double CurrentFrameDelay, FrameDelay;
        public Vector2 FrameSize;
        public List<Emitter> InvaderEmitterList;

        public void LoadContent(ContentManager contentManager)
        {            
            VulnerableToTurret = true;
            VulnerableToTrap = true;
            CurrentTexture = contentManager.Load<Texture2D>(AssetName);
            Color = Color.White;
            HPBar = new HorizontalBar(contentManager, new Vector2(32, 4), MaxHP, CurrentHP);
            InvaderEmitterList = new List<Emitter>();
        }

        public virtual void Update(GameTime gameTime)
        {
            if (Active == true)
            {
                Random = new Random();
                ResourceValue = Random.Next((int)ResourceMinMax.X, (int)ResourceMinMax.Y);

                VulnerableToTurret = true;

                CurrentDelay += gameTime.ElapsedGameTime.Milliseconds;
                CurrentAttackDelay += gameTime.ElapsedGameTime.Milliseconds;

                if (CurrentHP <= 0)
                    Active = false;

                if (Position.X > 1280)
                {
                    VulnerableToTurret = false;
                    VulnerableToTrap = false;
                }
                else
                {
                    VulnerableToTurret = true;
                    VulnerableToTrap = true;
                }


                if (CurrentAttackDelay >= AttackDelay)
                {
                    CanAttack = true;
                    CurrentAttackDelay = 0;
                }
                else
                {
                    CanAttack = false;
                }

                HPBar.Update(new Vector2(Position.X, Position.Y - 16), CurrentHP);                

                Position += Velocity;

                Velocity.Y += Gravity;

                CurrentFrameDelay += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (CurrentFrameDelay > FrameDelay)
                {
                    CurrentFrame++;

                        if (CurrentFrame == TotalFrames)
                        {
                            CurrentFrame = 0;
                        }

                    CurrentFrameDelay = 0;
                }

                SourceRectangle = new Rectangle((int)(CurrentFrame*FrameSize.X), 0, (int)FrameSize.X, (int)FrameSize.Y);

                DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)FrameSize.X, (int)FrameSize.Y);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {                
                BoundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, 0), new Vector3(Position.X + FrameSize.X, Position.Y + FrameSize.Y, 0));
                spriteBatch.Draw(CurrentTexture, DestinationRectangle, SourceRectangle, Color);

                HPBar.Draw(spriteBatch);
            }

            foreach (Emitter emitter in InvaderEmitterList)
            {
                emitter.Draw(spriteBatch);
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

        public void Trajectory(Vector2 velocity)
        {
            Velocity = velocity;
        }
               
        public void Move()
        {
            if (Active == true)
            {
                if (CurrentDelay > MoveDelay)
                {
                    Position += MoveVector;
                    CurrentDelay = 0;
                }
            }
        }
    }
}
