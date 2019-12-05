using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public abstract class HeavyProjectile : VerletProjectile
    {
        public Texture2D Texture;
        public List<Emitter> EmitterList;
        public Vector2 Velocity, Position, YRange, Scale, Origin, Direction;
        public float Angle, Speed, Gravity, CurrentRotation, CurrentTransparency, MaxY;
        public bool Rotate, Fade, Shadow;
        public Color CurrentColor;
        public HeavyProjectileType HeavyProjectileType;
        public Rectangle DestinationRectangle, CollisionRectangle;
        public float Damage, BlastRadius;
        static Random Random = new Random();

        public HeavyProjectile(Texture2D texture, Vector2 position, float speed, float angle, float gravity, float damage,
                               Vector2? yrange = null, float? blastRadius = null)
        {
            Active = true;
            Texture = texture;
            Angle = angle;
            Speed = speed;
            Gravity = gravity;
            Position = position;
            Damage = damage;

            MaxY = 890;

            Velocity.X = (float)(Math.Cos(angle) * speed);
            Velocity.Y = (float)(Math.Sin(angle) * speed);

            if (yrange == null)
            {
                YRange = new Vector2(500, 600);
            }
            else
            {
                YRange = yrange.Value;
            }

            if (blastRadius.HasValue)
                BlastRadius = blastRadius.Value;

            Node1 = new Node()
            {
                CurrentPosition = Position,
                PreviousPosition = Position - Velocity,
                Pinned = false
            };

            Node2 = new Node()
            {
                CurrentPosition = new Vector2(Position.X - (float)Math.Cos(Angle) * (Texture.Width),
                                              Position.Y - (float)Math.Sin(Angle) * (Texture.Width)),
                PreviousPosition = new Vector2(Position.X - (float)Math.Cos(Angle) * (Texture.Width),
                                              Position.Y - (float)Math.Sin(Angle) * (Texture.Width)) - Velocity,
                Pinned = false
            };

            Sticks = new Stick()
            {
                Length = Texture.Width,
                Rotate = true,
                Point1 = Node2,
                Point2 = Node1
            };
        }

        public void Initialize()
        {
            Active = true;
            CurrentTransparency = 0;
            MaxY = Random.Next((int)YRange.X, (int)YRange.Y);
            Constraints = new Rectangle(0, 0, 1920, (int)MaxY);
            Scale = new Vector2(1, 1);
            Origin = new Vector2(Texture.Width / 2, Texture.Height / 2);
            Shadow = true;
            DrawDepth = MaxY / 1080;
        }

        public override void Update(GameTime gameTime)
        {
            if (Active == true)
            {
                DestinationRectangle = new Rectangle((int)Sticks.Point1.CurrentPosition.X, 
                                                     (int)Sticks.Point1.CurrentPosition.Y,
                                                     Texture.Width, Texture.Height);

                CollisionRectangle = new Rectangle((int)Sticks.Point1.CurrentPosition.X,
                                                   (int)Sticks.Point1.CurrentPosition.Y,
                                                   Texture.Width, Texture.Height);

                foreach (Emitter emitter in EmitterList)
                {
                    emitter.Position = Node2.CurrentPosition;
                }

                Position = Node1.CurrentPosition;
            }

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Update(gameTime);
            }

            if (Fade == true)
            {
                CurrentTransparency += 0.1f;
            }

            Vector2 dir = Sticks.Point2.CurrentPosition - Sticks.Point1.CurrentPosition;
            Sticks.Rotation = (float)Math.Atan2(dir.Y, dir.X);

            Sticks.DestinationRectangle = new Rectangle(
                                              (int)Sticks.Point1.CurrentPosition.X, 
                                              (int)Sticks.Point1.CurrentPosition.Y, 
                                              Texture.Width, Texture.Height);
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                    spriteBatch.Draw(Texture, Sticks.DestinationRectangle, null, Color.White, Sticks.Rotation, 
                                     new Vector2(0, Texture.Height / 2), SpriteEffects.None, 0);
            }

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Draw(spriteBatch);
            }
        }
    }
}
