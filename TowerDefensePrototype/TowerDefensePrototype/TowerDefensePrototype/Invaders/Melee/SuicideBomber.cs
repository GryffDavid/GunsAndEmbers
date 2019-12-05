using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace TowerDefensePrototype
{
    class SuicideBomber : Invader
    {
        public SuicideBomber(Vector2 position)
        {
            Position = position;
            CurrentHP = 100;
            MaxHP = 100;
            ResourceMinMax = new Vector2(1, 5);
            InvaderType = InvaderType.SuicideBomber;
            YRange = new Vector2(700, 900);
        }
    }
}
