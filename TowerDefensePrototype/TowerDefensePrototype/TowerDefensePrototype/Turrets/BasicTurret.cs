using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;


namespace TowerDefensePrototype
{    
    class BasicTurret : Turret
    {      
        public BasicTurret(string turretName, string baseName, Vector2 position)
        {


            Active = true;
            TurretAsset = turretName;
            BaseAsset = baseName;
            Position = position;
            Selected = true;
            FireDelay = 200;
            Damage = 1;
        }
    }
}
