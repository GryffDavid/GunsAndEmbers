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
        public string Name;
        public int LevelNumber;
        public int Points;
        public List<string> Buttons;

        //What turrets the player has access to
        public bool FlameThrower, MachineGun, Cannon;

        //What traps the player has access to
        public bool Fire, Spikes, Wall, Catapult;
    }
}
