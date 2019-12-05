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
        public int LevelNumber, Points, Credits, ShotsFired;
        public List<Weapon> Buttons;
        public List<Upgrade> UpgradesList;

        //What turrets the player has access to
        public bool FlameThrower, MachineGun, Cannon, Lightning, Cluster,
                    FelCannon, Beam, Freeze, Grenade, Boomerang, PulseGun, Shotgun, PersistentBeam;

        //What traps the player has access to
        public bool Fire, Spikes, Wall, Catapult, SawBlade, Ice, Barrel, Line, Trigger;
    }
}
