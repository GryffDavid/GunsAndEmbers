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
    public class LoadoutDialogue : LevelDialogue
    {
        public LoadoutDialogue(Game1 game1)
            : base(game1)
        {
            
        }

        public override void Update(GameTime gameTime)
        {
            CurrentMouseState = Mouse.GetState();

            CurrentText = ItemsList[CurrentID].Message;

            DialogueBox.CompleteText = DialogueBox.WrapText(CurrentText);
            DialogueBox.Update(gameTime);

            TutorialMarker.Update(gameTime);
            ////CurrentLevel.LevelDialogue.TutorialMarker.

            switch (CurrentID)
            {
                default:
                    if (CurrentMouseState.LeftButton == ButtonState.Released &&
                        PreviousMouseState.LeftButton == ButtonState.Pressed &&
                        DialogueBox.LengthIndex == DialogueBox.CompleteText.Length &&
                        DialogueBox.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                    {
                        CurrentID++;
                        DialogueBox.LengthIndex = 0;
                    }
                    break;

                case 2:
                    if (Game1.SelectedTrap != null)
                    {
                        CurrentID++;
                        DialogueBox.LengthIndex = 0;
                    }
                    break;
            }


            PreviousMouseState = CurrentMouseState;

            base.Update(gameTime);
        }
    }
}
