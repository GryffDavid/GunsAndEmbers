using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameDataTypes
{
    //IDEA: Could turn this into a dialogue tree and use the XML file to 
    //store the dialogue paths and data

    //public struct Dialogue
    //{
    //    string Line;
    //    bool Option1;
    //    bool Option2;
    //}

    public class StoryDialogue
    {
        public List<string> Lines = new List<string>();
    }
}
