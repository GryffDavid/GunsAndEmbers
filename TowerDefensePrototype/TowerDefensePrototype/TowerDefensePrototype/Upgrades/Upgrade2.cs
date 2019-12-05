using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace TowerDefensePrototype
{
    public class Upgrade2 : Upgrade
    {        
        public Upgrade2()
        {
            GatlingSpeed = -50;
            Text = "Rory is a butt face.";
        }
    }
}
