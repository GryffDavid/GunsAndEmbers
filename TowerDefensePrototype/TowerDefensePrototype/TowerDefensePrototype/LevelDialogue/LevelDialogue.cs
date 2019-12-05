using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowerDefensePrototype
{
    class LevelDialogue
    {
        public class DialogueItem
        {
            public int ID;
            public string Text;
            public float Time;
        }

        public List<DialogueItem> DialogueItems;        
    }
}
