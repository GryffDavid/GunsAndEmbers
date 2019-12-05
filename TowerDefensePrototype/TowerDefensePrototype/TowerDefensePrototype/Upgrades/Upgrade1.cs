using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace TowerDefensePrototype
{
    [Serializable]
    public class Upgrade1 : Upgrade
    {        
        public Upgrade1()
        {
            GatlingDamage += 15;
            Text = "Increases gatling gun speed 50%.";
        }
    }
}
