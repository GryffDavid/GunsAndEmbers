using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowerDefensePrototype
{
    [Serializable]
    public class Weapon
    {
        public Nullable<TurretType> CurrentTurret;
        public Nullable<TrapType> CurrentTrap;

        public Weapon(Nullable<TurretType> turret, Nullable<TrapType> trap)
        {
            CurrentTrap = trap;
            CurrentTurret = turret;

            if (CurrentTurret != null)
                CurrentTrap = null;

            if (CurrentTrap != null)
                CurrentTurret = null;
        }
    }
}
