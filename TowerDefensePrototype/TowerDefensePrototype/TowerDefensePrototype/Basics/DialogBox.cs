using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class DialogBox
    {
        Vector2 Position, TextPosition;
        Texture2D Box, LeftButtonSprite, RightButtonSprite;
        public Button LeftButton, RightButton;
        string LeftText, RightText, BoxText;
        SpriteFont TextFont;

        public DialogBox(Texture2D boxSprite, Texture2D leftButton, Texture2D rightButton, SpriteFont font, Vector2 position, string leftText, string boxText, string rightText = "")
        {
            Position = position;
            BoxText = boxText;
            LeftText = leftText;

            TextFont = font;
            LeftButtonSprite = leftButton;
            RightButtonSprite = rightButton;
            Box = boxSprite;

            if (rightText != "")
                RightText = rightText;
            else
                RightText = null;
        }

        public void LoadContent()
        {
        //    Box = contentManager.Load<Texture2D>("DialogBox");

            LeftButton = new Button(LeftButtonSprite, new Vector2(-300,
            Position.Y + 128 - (Box.Height / 2)), null, new Vector2(2, 1), null, LeftText, TextFont, "Left", Color.White);
            LeftButton.NextPosition.X = Position.X - (Box.Width / 2);
            LeftButton.Scale = new Vector2(2,1);            
            LeftButton.LoadContent();
            LeftButton.NextScale = new Vector2(1, 1);

            if (RightText != null)
            {
                RightButton = new Button(RightButtonSprite, new Vector2(1280+300,
                    Position.Y + 128 - (Box.Height / 2)), null, new Vector2(2, 1), null, RightText, TextFont, "Right", Color.White);
                RightButton.NextPosition.X = Position.X + 252 - (Box.Width / 2);
                RightButton.Scale = new Vector2(2, 1);                
                RightButton.LoadContent();
                RightButton.NextScale = new Vector2(1, 1);
            }

            //TextFont = contentManager.Load<SpriteFont>("Fonts/DefaultFont");

            for (int i = 0; i < BoxText.Length; i++)
            {
                if ((i % 45) == 1 && i != 1)
                {
                    int k;
                    k = BoxText.LastIndexOf(" ", i);
                    BoxText = BoxText.Insert(k + 1, Environment.NewLine);
                }
            }
        }

        public void Update(Vector2 cursorPosition, GameTime gameTime)
        {
            LeftButton.Update(cursorPosition, gameTime);
            
            if (RightText != null)
                RightButton.Update(cursorPosition, gameTime);

            Vector2 textSize = TextFont.MeasureString(BoxText);
            TextPosition = new Vector2(Position.X - (TextFont.MeasureString(BoxText).X / 2),
                                       Position.Y - textSize.Y);


            //TextPosition = new Vector2(Position.X - (TextFont.MeasureString(BoxText).X / 2),
            //                           Position.Y + 50 - (Box.Height / 2) - TextFont.MeasureString(BoxText[0].ToString()).Y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Box, new Rectangle((int)Position.X - (Box.Width/2), (int)Position.Y - (Box.Height/2), Box.Width, Box.Height), Color.White);
            spriteBatch.DrawString(TextFont, BoxText, TextPosition, Color.White);
            LeftButton.Draw(spriteBatch);

            if (RightText != null)
            RightButton.Draw(spriteBatch);
        }
    }
}
