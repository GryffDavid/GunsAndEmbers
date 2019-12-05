using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace TowerDefensePrototype
{
    [Serializable]
    public class Upgrade
    {
        public float GatlingSpeed, GatlingDamage, GatlingAccuracy;
        public float CannonSpeed, CannonAccuracy, CannonBlastRadius;
        public string Text;
    }
}
 