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
        Vector2 Offset;
        Vector2 ActualSize, TextPosition;
        Rectangle DestinationRectangle;
        Color Color;
        public bool Loaded;

        public InformationBox(string text)
        {
            Text = text;
            Loaded = false;
            ActualSize = Vector2.Zero;
            Color = Color.Transparent;
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

            Loaded = true;
        }

        public void Update()
        {
            MouseState MouseState = Mouse.GetState();
            Position = new Vector2(MouseState.X, MouseState.Y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            MouseState MouseState = Mouse.GetState();
                       Color = Color.Lerp(Color, Color.Lerp(Color.White, Color.Transparent, 0.30f), 0.02f);

            ActualSize = Vector2.Lerp(ActualSize, new Vector2(Font.MeasureString(Text).X, Font.MeasureString(Text).Y), 0.2f);

            Vector2 MeasureSize = new Vector2(Font.MeasureString(Text).X, Font.MeasureString(Text).Y);

            if (MouseState.X <= MeasureSize.X + 16)
            {
                Offset.X = MeasureSize.X + 4;
            }
            else
            {
                Offset.X = -16;
            }

            if (MouseState.Y >= MeasureSize.Y)
            {
                Offset.Y = -MeasureSize.Y;
            }
            else
            {
                Offset.Y = 0;
            }

            spriteBatch.Draw(BoxTexture, 
                new Rectangle((int)(MouseState.X - ActualSize.X + Offset.X - 2),
                              (int)(MouseState.Y + Offset.Y - 2),
                              (int)ActualSize.X + 16 + 4,
                              (int)ActualSize.Y + 4),
                    Color.Gray);

            spriteBatch.Draw(BoxTexture,
                new Rectangle((int)(MouseState.X - ActualSize.X + Offset.X),
                              (int)(MouseState.Y + Offset.Y),
                              (int)ActualSize.X + 16,
                              (int)ActualSize.Y),
                    Color.White);

            spriteBatch.DrawString(Font, Text,
                new Vector2(MouseState.X - ActualSize.X + Offset.X + 8,
                            MouseState.Y + Offset.Y - 8), Color);

        }
    }
}
