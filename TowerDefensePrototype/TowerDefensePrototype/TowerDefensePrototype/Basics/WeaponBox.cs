using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class WeaponBox : Button
    {
        public Nullable<TurretType> ContainsTurret = null;
        public Nullable<TrapType> ContainsTrap = null;

        public WeaponBox(string buttonName, Vector2 postion, Vector2 scale) : base(buttonName, postion, null, scale )
        {

        }
    }
}