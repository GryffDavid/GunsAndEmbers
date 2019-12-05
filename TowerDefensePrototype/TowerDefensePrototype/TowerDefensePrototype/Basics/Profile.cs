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
        public List<string> Buttons;
        public List<TurretType> Buttons2;
        public List<Upgrade> UpgradesList;

        //What turrets the player has access to
        public bool FlameThrowerTurret, MachineGunTurret, CannonTurret, LightningTurret, ClusterTurret,
                    FelCannonTurret, BeamTurret, FreezeTurret, GrenadeTurret;

        //What traps the player has access to
        public bool FireTrap, SpikesTrap, WallTrap, CatapultTrap, SawBladeTrap, IceTrap, BarrelTrap;
    }
}
