using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Xml.Serialization;

namespace GameDataTypes
{
    public class LevelDialogue
    {
        public class DialogueItem
        {
            public int ID;
            public string Message;
            public float Time;
        }

        [ContentSerializerIgnore]
        public Game Game;

        public int CurrentID;
        public string CurrentText;

        public List<DialogueItem> DialogueItems;
               
        public LevelDialogue()
        {

        }

        public void Initialize(Game game)
        {
            Game = game;
        }

        public virtual void Update(GameTime gameTime)
        {
            
        }
    }
}
