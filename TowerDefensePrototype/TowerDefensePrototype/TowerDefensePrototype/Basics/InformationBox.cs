using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace TowerDefensePrototype
{
    public class InformationBox
    {
        Vector2 Position;
        Texture2D BoxTexture;
        SpriteFont Font;
        String Text;        

        public InformationBox(string text)
        {
            Text = text;
        }

        public void LoadContent(ContentManager contentManager)
        {
            BoxTexture = contentManager.Load<Texture2D>("InformationBox");
            Font = contentManager.Load<SpriteFont>("Fonts/BoxFont");

            for (int i = 0; i < Text.Length; i++)
            {
                if ((i%40)==1)
                {
                    int k;
                    k = Text.LastIndexOf(" ", i);
                    Text = Text.Insert(k+1, Environment.NewLine);
                }
            }
        }

        public void Update()
        {
            MouseState MouseState = Mouse.GetState();
            Position = new Vector2(MouseState.X, MouseState.Y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 ActualSize = new Vector2(Font.MeasureString(Text).X, Font.MeasureString(Text).Y);
            MouseState MouseState = Mouse.GetState();
            Vector2 Offset;

            if (MouseState.Y <= (720 - 350))
            {
                Offset.Y = 0;
            }
            else
            {
                Offset.Y = -ActualSize.Y;
            }

            if (MouseState.X < (1280 - 300))
            {
                Offset.X = 0;
            }
            else
            {
                Offset.X = -ActualSize.X;
            }

            spriteBatch.Draw(BoxTexture, new Rectangle((int)(Position.X + Offset.X), (int)(Position.Y + Offset.Y)-16, 
                             (int)ActualSize.X+32, (int)ActualSize.Y+8), Color.White);
            spriteBatch.DrawString(Font, Text, new Vector2(Position.X + 16 + Offset.X, 
                                   Position.Y - Font.MeasureString(Text[0].ToString()).Y + Offset.Y), Color.White);

        }
    }
}
