﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace TowerDefensePrototype
{
    public class Upgrade1 : Upgrade
    {        
        public Upgrade1()
        {
            GatlingSpeed = -50;
            Text = "Increases gatling gun speed 50%.";
        }
    }
}
