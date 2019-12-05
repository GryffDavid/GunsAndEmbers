using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace TowerDefensePrototype
{
    [XmlInclude(typeof(Upgrade1))]
    [XmlInclude(typeof(Upgrade2))]

    public class Upgrade
    {
        public int GatlingSpeed;
        public string Text;
    }
}
 