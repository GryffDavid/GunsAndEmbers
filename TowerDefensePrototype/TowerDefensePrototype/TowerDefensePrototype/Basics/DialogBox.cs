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
        Vector2 Position;
        Texture2D Box;
        public Button LeftButton, RightButton;
        string LeftText, RightText, BoxText;
        SpriteFont TextFont;

        public DialogBox(Vector2 position, string leftText, string boxText, string rightText = "")
        {
            Position = position;
            BoxText = boxText;
            LeftText = leftText;

            if (rightText != "")
                RightText = rightText;
            else
                RightText = null;
        }

        public void LoadContent(ContentManager contentManager)
        {
            Box = contentManager.Load<Texture2D>("DialogBox");

            LeftButton = new Button("Buttons/ButtonLeft", new Vector2(Position.X - (Box.Width / 2), Position.Y + 128 - (Box.Height/2)), null, 
                new Vector2(0.5f, 1), null, LeftText, "Fonts/ButtonFont", "Left", Color.White);
            LeftButton.LoadContent(contentManager);

            RightButton = new Button("Buttons/ButtonRight", new Vector2(Position.X + 250 - (Box.Width / 2), Position.Y + 128 - (Box.Height / 2)), null, 
                new Vector2(0.5f, 1), null, RightText, "Fonts/ButtonFont", "Right", Color.White);            
            RightButton.LoadContent(contentManager);

            TextFont = contentManager.Load<SpriteFont>(LeftButton.FontName);
        }

        public void Update()
        {
            LeftButton.Update();
            
            if (RightText != "")
            RightButton.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Box, new Rectangle((int)Position.X - (Box.Width/2), (int)Position.Y - (Box.Height/2), Box.Width, Box.Height), Color.White);
            spriteBatch.DrawString(TextFont, BoxText, new Vector2(Position.X - (TextFont.MeasureString(BoxText).X / 2), Position.Y + 50 - (Box.Height / 2)), Color.White);
            LeftButton.Draw(spriteBatch);

            if (RightText != "")
            RightButton.Draw(spriteBatch);
        }
    }
}
