using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TowerDefensePrototype
{
    class Healer : Invader
    {
        //Fires a beam into other invaders to heal them. Red beam. Think medic from Team Fortress 2.
        public Healer(Vector2 position, Vector2? yRange = null) : base(position, yRange)
        {

        }
    }
}
