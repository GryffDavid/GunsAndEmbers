using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class JetEngine : Drawable
    {
        public Texture2D JetSprite;
        public Vector2 Position;
        public object Tether;

        public VertexPositionColorTexture[] spriteVertices = new VertexPositionColorTexture[4];
        public int[] spriteIndices = new int[6];

        public Rectangle DestinationRectangle;
        public JetTrail JetTrail;

        public float Rotation;

        Matrix Transform;

        public JetEngine(Texture2D sprite)
        {
            JetSprite = sprite;
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, JetSprite.Width, JetSprite.Height);

            spriteVertices[0] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top, 0),
                TextureCoordinate = new Vector2(0, 0),
                Color = Color.White
            };

            spriteVertices[1] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top, 0),
                TextureCoordinate = new Vector2(1, 0),
                Color = Color.White
            };

            spriteVertices[2] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                TextureCoordinate = new Vector2(1, 1),
                Color = Color.White
            };

            spriteVertices[3] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                TextureCoordinate = new Vector2(0, 1),
                Color = Color.White
            };

            spriteIndices[0] = 0;
            spriteIndices[1] = 1;
            spriteIndices[2] = 2;
            spriteIndices[3] = 2;
            spriteIndices[4] = 3;
            spriteIndices[5] = 0;

            JetTrail = new JetTrail();            
        }

        public void Update(GameTime gameTime)
        {
            Invader invaderTether = Tether as Invader;

            if (invaderTether != null)
            {
                Position = invaderTether.Center;
                DrawDepth = invaderTether.DrawDepth;
                JetTrail.DrawDepth = DrawDepth;

                Rotation = MathHelper.Lerp((float)Math.Atan2(-invaderTether.Velocity.X, -invaderTether.Velocity.Y), Rotation, 0.02f);
            }
            
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, JetSprite.Width, JetSprite.Height);

            spriteVertices[0].Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top, 0);
            spriteVertices[1].Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top, 0);
            spriteVertices[2].Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top + DestinationRectangle.Height, 0);
            spriteVertices[3].Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top + DestinationRectangle.Height, 0);

            JetTrail.Update(gameTime, Position, 0);            

            //Rotation += 0.001f;

            Transform = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                        Matrix.CreateRotationZ(Rotation) *
                        Matrix.CreateTranslation(new Vector3(Position.X, Position.Y, 0));
                        
        }

        public override void Draw(GraphicsDevice graphics, BasicEffect effect, Effect shadowEffect, SpriteBatch spriteBatch)
        {
            effect.World = Transform;
            JetTrail.Draw(graphics, effect);
            
            //spriteBatch.Draw(JetSprite, DestinationRectangle, null, Color.White, Rotation, new Vector2(JetSprite.Width/2, JetSprite.Height), SpriteEffects.None, 0);
            effect.World = Matrix.CreateTranslation(new Vector3(-JetSprite.Width / 2, -JetSprite.Height, 0)) * Transform;
            
            
            effect.Texture = JetSprite;
            effect.TextureEnabled = true;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, spriteVertices, 0, 4, spriteIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
            }

            base.Draw(graphics, effect);
        }
    }
}
