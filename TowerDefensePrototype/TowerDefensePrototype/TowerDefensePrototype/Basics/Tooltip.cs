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
    public class Tooltip
    {
        Vector2 Position, CursorPosition;
        Texture2D BoxTexture;
        SpriteFont Font;
        String Text;
        Vector2 Offset;
        Vector2 ActualSize;
        Color Color;
        public bool Loaded;

        public Tooltip(string text)
        {
            Text = text;
            Loaded = false;
            ActualSize = Vector2.Zero;
            Color = Color.Transparent;
        }

        public void LoadContent(ContentManager contentManager)
        {
            BoxTexture = contentManager.Load<Texture2D>("InformationBox");
            Font = contentManager.Load<SpriteFont>("Fonts/TooltipFont");

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

        public void Update(Vector2 cursorPosition)
        {
            CursorPosition = cursorPosition;
            Position = new Vector2(cursorPosition.X, cursorPosition.Y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Color = Color.Lerp(Color, Color.Lerp(Color.White, Color.Transparent, 0.30f), 0.02f);

            ActualSize = Vector2.Lerp(ActualSize, new Vector2(Font.MeasureString(Text).X, Font.MeasureString(Text).Y), 0.2f);

            Vector2 MeasureSize = new Vector2(Font.MeasureString(Text).X, Font.MeasureString(Text).Y);

            if (CursorPosition.X <= MeasureSize.X + 16)
            {
                Offset.X = MeasureSize.X + 4;
            }
            else
            {
                Offset.X = -16;
            }

            if (CursorPosition.Y >= MeasureSize.Y)
            {
                Offset.Y = -MeasureSize.Y;
            }
            else
            {
                Offset.Y = 0;
            }

            spriteBatch.Draw(BoxTexture,
                new Rectangle((int)(CursorPosition.X - ActualSize.X + Offset.X - 2),
                              (int)(CursorPosition.Y + Offset.Y - 2),
                              (int)ActualSize.X + 16 + 4,
                              (int)ActualSize.Y + 4),
                    Color.Gray);

            spriteBatch.Draw(BoxTexture,
                new Rectangle((int)(CursorPosition.X - ActualSize.X + Offset.X),
                              (int)(CursorPosition.Y + Offset.Y),
                              (int)ActualSize.X + 16,
                              (int)ActualSize.Y),
                    Color.White);

            spriteBatch.DrawString(Font, Text,
                new Vector2(CursorPosition.X - ActualSize.X + Offset.X + 8,
                            CursorPosition.Y + Offset.Y - 8), Color);

        }
    }
}
