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
    public class StoryDialogueBox
    {
        public Texture2D BoxTexture;
        public Vector2 Position;
        public Rectangle DestinationRectangle;
        public MouseState CurrentMouseState, PreviousMouseState;
        public SpriteFont DialogueFont, TipFont;

        public string CompleteText, CurrentText, TipText;
        public float CurrentTime, MaxTime;
        public int LengthIndex;

        public StoryDialogueBox()
        {
            CurrentText = "";
            TipText = "Click to Continue";

            CurrentTime = 0;
            MaxTime = 5;
            LengthIndex = 0;

            Position = new Vector2(0, 100);
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, 435, 150);
        }

        public void Update(GameTime gameTime)
        {
            //Clicking while text is being typed out will skip the typing and just draw all the text
            //Clicking when the text has been shown will move to the next dialogue piece if it doesn't have a condition to be satisfied

            CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentTime >= MaxTime && LengthIndex < CompleteText.Length)
            {
                LengthIndex++;
            }

            CurrentText = CompleteText.Substring(0, LengthIndex);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (CompleteText != " " && CompleteText != "" && CompleteText != null)
            {
                spriteBatch.Draw(BoxTexture, DestinationRectangle, Color.Black * 0.85f);
                spriteBatch.Draw(BoxTexture, new Rectangle(DestinationRectangle.Left, DestinationRectangle.Top, 6, DestinationRectangle.Height), Color.White);
                spriteBatch.DrawString(DialogueFont, CurrentText, Position + new Vector2(32, 32), Color.White);

                if (LengthIndex == CompleteText.Length)
                    spriteBatch.DrawString(TipFont, TipText, new Vector2(Position.X, 0) + new Vector2(32, DestinationRectangle.Bottom - 24), Color.White, 0, Vector2.Zero, 0.75f, SpriteEffects.None, 0);
            } 
        }

        public string WrapText(string Text)
        {
            for (int i = 0; i < Text.Length; i++)
            {
                if ((i % 35) == 1 && i != 1)
                {
                    int k;
                    k = Text.LastIndexOf(" ", i);
                    Text = Text.Insert(k + 1, Environment.NewLine);
                }
            }

            return Text;
        }
    }
}
