using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class FireElemental : Invader
    {    
        public FireElemental(Vector2 position)
        {
            Position = position;
            CurrentHP = 50;
            MaxHP = 50;
            ResourceMinMax = new Vector2(50, 100);
            InvaderType = InvaderType.FireElemental;
            YRange = new Vector2(700, 900);
        }
    }
}
