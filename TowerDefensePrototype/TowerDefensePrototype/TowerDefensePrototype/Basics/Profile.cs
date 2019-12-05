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

        public Dictionary<TurretType, bool> TurretDictionary = new Dictionary<TurretType, bool>();
        public Dictionary<TrapType, bool> TrapDictionary = new Dictionary<TrapType, bool>();


        public Profile()
        {
            foreach (TurretType turretType in Enum.GetValues(typeof(TurretType)))
            {
                TurretDictionary.Add(turretType, true);
            }

            foreach (TrapType trapType in Enum.GetValues(typeof(TrapType)))
            {
                TrapDictionary.Add(trapType, true);
            }
        }

        //What turrets the player has access to
        //NEW_TURRET H **player access variables**
        //public bool FlameThrower,
        //            MachineGun,
        //            Cannon,
        //            Lightning,
        //            Cluster,
        //            FelCannon,
        //            Beam, Freeze,
        //            Grenade,
        //            Boomerang,
        //            PulseGun,
        //            Shotgun,
        //            PersistentBeam,
        //            GasGrenade,
        //            Harpoon,
        //            StickyMine;

        //What traps the player has access to
        //public bool Fire,
        //            Spikes,
        //            Wall,
        //            Catapult,
        //            SawBlade,
        //            Ice,
        //            Barrel,
        //            Line,
        //            Trigger,
        //            LandMine,
        //            Glue;
    }
}
