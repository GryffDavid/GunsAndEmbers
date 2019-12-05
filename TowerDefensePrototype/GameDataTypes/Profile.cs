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
        public int LevelNumber, Points, Credits;
        public List<string> Buttons;

        //What turrets the player has access to
        public bool FlameThrower, MachineGun, Cannon, LightningTurret;

        //What traps the player has access to
        public bool Fire, Spikes, Wall, Catapult, SawBlade, Ice;

        //Upgrades the player has purchased
        public bool Upgrade1, Upgrade2, Upgrade3;
    }
}
