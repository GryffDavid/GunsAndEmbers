using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class Spider : HeavyRangedInvader
    {
        public Spider(Vector2 position)
         {
            Position = position;
            CurrentHP = 40;
            MaxHP = 40;            
            ResourceMinMax = new Vector2(1, 5);
            InvaderType = InvaderType.Spider;
            YRange = new Vector2(700, 900);
        }
    }
}
