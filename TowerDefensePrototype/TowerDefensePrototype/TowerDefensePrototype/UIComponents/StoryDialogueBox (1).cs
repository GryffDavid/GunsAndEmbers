using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameDataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace TowerDefensePrototype
{
    public class StoryDialogueBox
    {
        public bool Active;
        public Texture2D HeadIcon;
        public Rectangle IconRectangle;
        public Vector2 CurrentPosition, NextPosition, Size, TextPosition;
        public SpriteFont Font;
        public Color TextColor;
        float SizeOffset;

        VertexPositionColor[] BoxVertices = new VertexPositionColor[4];
        int[] BoxIndices = new int[6];
        int DialogueIndex;

        public Color BoxColor = Color.Lerp(Color.Black, Color.Transparent, 0.5f);

        public string Text, Name;

        KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        public StoryDialogue CurrentDialogue;

        public StoryDialogueBox(Vector2 currentPosition, Vector2 nextPosition, Vector2 size, Texture2D headIcon, SpriteFont font, StoryDialogue dialogue, string profileName)
        {
            Active = true;
            Name = profileName;
            DialogueIndex = 0;
            CurrentDialogue = dialogue;
            CurrentPosition = currentPosition;
            NextPosition = nextPosition;
            Size = size;
            HeadIcon = headIcon;
            Font = font;

            SizeOffset = (Size.Y - HeadIcon.Height) / 2;

            BoxVertices[0] = new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X, CurrentPosition.Y, 0),
                Color = BoxColor
            };

            BoxVertices[1] = new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + Size.X, CurrentPosition.Y, 0),
                Color = BoxColor
            };

            BoxVertices[2] = new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + Size.X, CurrentPosition.Y + Size.Y, 0),
                Color = BoxColor
            };

            BoxVertices[3] = new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X, CurrentPosition.Y + Size.Y, 0),
                Color = BoxColor
            };

            BoxIndices[0] = 0;
            BoxIndices[1] = 1;
            BoxIndices[2] = 2;
            BoxIndices[3] = 2;
            BoxIndices[4] = 3;
            BoxIndices[5] = 0;

            if (CurrentDialogue != null)
            {
                Text = CurrentDialogue.Lines[DialogueIndex];

                ReplaceName(Name);
                WrapText();
            }
        }

        public void WrapText()
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
        }

        public void ReplaceName(string name)
        {
            Text = Text.Replace("[name]", name);
        }

        public void Update(GameTime gameTime)
        {
            CurrentKeyboardState = Keyboard.GetState();

            //Advance the dialogue with the spacebar and deactivate the box when there is no more dialogue
            if (CurrentKeyboardState.IsKeyUp(Keys.Space) &&
                PreviousKeyboardState.IsKeyDown(Keys.Space))
            {
                if (DialogueIndex < CurrentDialogue.Lines.Count-1)
                {
                    DialogueIndex++;
                    Text = CurrentDialogue.Lines[DialogueIndex];
                    ReplaceName(Name);
                    WrapText();
                }
                else
                {
                    NextPosition = new Vector2(-Size.X, CurrentPosition.Y); 
                }
            }

            //(Size.Y - HeadIcon.Height) / 2;
            Vector2 textSize = Font.MeasureString(Text);
            TextPosition = new Vector2(CurrentPosition.X + 2 * SizeOffset + HeadIcon.Width, 
                                       CurrentPosition.Y + (Size.Y / 2) - (textSize.Y / 2));

            //Move the box
            if (NextPosition != CurrentPosition)
            {
                CurrentPosition = Vector2.Lerp(CurrentPosition, NextPosition, 0.15f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));

                if (Math.Abs(CurrentPosition.X - NextPosition.X) < 0.5f)
                {
                    CurrentPosition.X = NextPosition.X;
                }

                BoxVertices[0].Position = new Vector3(CurrentPosition.X, CurrentPosition.Y, 0);

                BoxVertices[1].Position = new Vector3(CurrentPosition.X + Size.X, CurrentPosition.Y, 0);

                BoxVertices[2].Position = new Vector3(CurrentPosition.X + Size.X, CurrentPosition.Y + Size.Y, 0);

                BoxVertices[3].Position = new Vector3(CurrentPosition.X, CurrentPosition.Y + Size.Y, 0);

                //IconRectangle = new Rectangle((int)(CurrentPosition.X + Size.X) - HeadIcon.Width - 8, (int)CurrentPosition.Y + 8, HeadIcon.Width, HeadIcon.Height);
            }

            PreviousKeyboardState = CurrentKeyboardState;
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect basicEffect)
        {
            if (Active == true && CurrentDialogue != null)
            {
                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, BoxVertices, 0, BoxVertices.Count(), BoxIndices, 0, BoxIndices.Count() / 3);
                }
                
                spriteBatch.Draw(HeadIcon, new Rectangle((int)(CurrentPosition.X + SizeOffset), (int)(CurrentPosition.Y + SizeOffset), HeadIcon.Width, HeadIcon.Height), Color.White);
                spriteBatch.DrawString(Font, Text, TextPosition, Color.White);
            }
        }
    }
}
