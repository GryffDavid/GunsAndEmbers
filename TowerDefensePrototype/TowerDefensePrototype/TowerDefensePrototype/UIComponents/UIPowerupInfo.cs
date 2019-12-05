using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    //Describes the powerup when the UIPowerup is moused over
    class UIPowerupInfo
    {
        public Powerup Powerup;
        Vector2 Position, Size;
        VertexPositionColor[] NameBoxVertices = new VertexPositionColor[4];
        int[] NameBoxIndices = new int[6];
        Color Color = Color.Lerp(Color.Black, Color.Transparent, 0.5f);
        SpriteFont Font;
        UIBar TimingBar;
        public bool Visible = false;
        bool Active;
        public string Name, Description;

        public UIPowerupInfo(Vector2 position, Powerup powerup, SpriteFont font)
        {
            Active = true;
            Position = position;
            Font = font;
            Powerup = powerup;
            Size = new Vector2(128, 64);
            TimingBar = new UIBar(Position, new Vector2(Size.X, 4), Color.LightSkyBlue);
            
            Description = "Increases explosive damage by 30%";

            for (int i = 0; i < Description.Length; i++)
            {
                if ((i % 18) == 1 && i != 1)
                {
                    int k;
                    k = Description.LastIndexOf(" ", i);
                    Description = Description.Insert(k + 1, Environment.NewLine);
                }
            }

            Size.Y = Font.MeasureString(Description).Y;


            //Top left
            NameBoxVertices[0] = new VertexPositionColor()
            {
                Position = new Vector3(Position, 0),
                Color = Color
            };

            //Top right
            NameBoxVertices[1] = new VertexPositionColor()
            {
                Position = new Vector3(Position.X + Size.X, Position.Y, 0),
                Color = Color
            };

            //Bottom right
            NameBoxVertices[2] = new VertexPositionColor()
            {
                Position = new Vector3(Position.X + Size.X, Position.Y + Size.Y, 0),
                Color = Color
            };

            //Bottom left
            NameBoxVertices[3] = new VertexPositionColor()
            {
                Position = new Vector3(Position.X, Position.Y + Size.Y, 0),
                Color = Color
            };

            NameBoxIndices[0] = 0;
            NameBoxIndices[1] = 1;
            NameBoxIndices[2] = 2;
            NameBoxIndices[3] = 2;
            NameBoxIndices[4] = 3;
            NameBoxIndices[5] = 0;
        }

        public void Update(GameTime gameTime)
        {
            Active = Powerup.Active;
            TimingBar.Update(Powerup.MaxTime, Powerup.CurrentTime);
        }

        public void LoadContent(ContentManager contentManager)
        {

        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect basicEffect)
        {
            if (Visible == true && Active == true)
            {
                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, NameBoxVertices, 0, 4, NameBoxIndices, 0, 2, VertexPositionColor.VertexDeclaration);
                    TimingBar.Draw(graphics);
                }

                spriteBatch.DrawString(Font, Description, Position + new Vector2(5, 5), Color.White, 0, Vector2.Zero, 0.75f, SpriteEffects.None, 0);
            }
        }
    }
}
