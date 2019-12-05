using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GameDataTypes
{
    public class DialogueItem
    {
        public string Message;

        [ContentSerializer(Optional = true)]
        public Nullable<float> Time = null;
    }
}
