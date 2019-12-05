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
    public class Level2Dialogue : LevelDialogue
    {
        int CrateNum = 0;


        public Level2Dialogue(Game1 game1)
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
            //CurrentLevel.LevelDialogue.TutorialMarker.

            switch (CurrentID)
            {
                default:
                    if (CurrentMouseState.LeftButton == ButtonState.Released &&
                        PreviousMouseState.LeftButton == ButtonState.Pressed &&
                        DialogueBox.LengthIndex == DialogueBox.CompleteText.Length &&
                        DialogueBox.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                    {
                        Next();
                    }
                    break;
            }

            PreviousMouseState = CurrentMouseState;

            base.Update(gameTime);
        }
    }
}
