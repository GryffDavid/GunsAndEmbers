using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Airship : HeavyRangedInvader
    {
        //This could have its gun turret mounted UNDERNEATH the main body of the ship instead of on top
        public Airship(Vector2 position)
        {
            Active = true;
            Position = position;
            MaxHP = 300;
            CurrentHP = MaxHP;
            ResourceMinMax = new Vector2(1, 5);
            InvaderType = InvaderType.Airship;
            YRange = new Vector2(60, 150);
        }
    }
}
