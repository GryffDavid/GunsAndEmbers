using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using GameDataTypes;

namespace TowerDefensePrototype
{
    public class LevelDialogue
    {
        public Game1 Game1;
        public int CurrentID = 0;
        public float CurrentTime, MaxTime;
        public List<DialogueItem> ItemsList;
        public string CurrentText = "";
        public static Random Random = new Random();

        public MouseState CurrentMouseState, PreviousMouseState;

        public StoryDialogueBox DialogueBox;
        public ButtonMarker TutorialMarker;

        public LevelDialogue(Game1 game1)
        {
            Game1 = game1;
        }

        public virtual void Update(GameTime gameTime)
        {

        }

        public void Next()
        {
            if (CurrentID < ItemsList.Count - 1)
            {
                CurrentID++;
                DialogueBox.LengthIndex = 0;
            }
        }
    }
}
