using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class JetEngine : VectorSprite
    {
        public object Tether;
        public Invader InvaderTether;
        public Vector2 TetherOffset, PivotPoint;
        //public float Rotation;
        public Emitter JetEmitter;

        public JetEngine(Vector2 position, Vector2 size, Texture2D texture, Color color) : base(position, size, texture, color)
        {
            Rotation = -75f;            
        }

        public void Update(GameTime gameTime)
        {
            InvaderTether = (Tether as Invader);

            if (InvaderTether != null)
            {
                Position = (Tether as Invader).Position + TetherOffset;
                JetEmitter.Position = Position;

                if (InvaderTether.Orientation == SpriteEffects.FlipHorizontally)
                {
                    Rotation = -75f + 180;
                    JetEmitter.AngleRange = new Vector2(-15 + 180, -15 + 180);
                }
                else
                {
                    Rotation = -75f;
                    JetEmitter.AngleRange = new Vector2(-15, -15);
                }
            }
            
            DrawDepth = (Tether as Invader).DrawDepth + (1f / 1080f);
            JetEmitter.DrawDepth = DrawDepth;            

            JetEmitter.Update(gameTime);
            //Rotation += 0.5f;
            base.Update(Position);
        }

        public override void Draw(GraphicsDevice graphics, BasicEffect effect)
        {
            //JetEmitter.Draw(graphics, effect);
            base.Draw(graphics, effect);
        }
    }
}
