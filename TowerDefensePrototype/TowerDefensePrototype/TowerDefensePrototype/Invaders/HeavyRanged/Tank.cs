using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;  
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Tank : Invader
    {
        public Tank(Vector2 position)
        {
            Position = position;
            CurrentHP = 800;
            MaxHP = 800;
            ResourceMinMax = new Vector2(5, 20);
            InvaderType = InvaderType.Tank;
            YRange = new Vector2(700, 900);
        }
    }
}
