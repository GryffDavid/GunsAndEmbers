using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace TowerDefensePrototype
{
    [Serializable]
    public class Profile
    {
        public int ProfileNumber;
        public string Name;
        public int LevelNumber;        
    }
}
