using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Soldier : Invader
    {      
        public Soldier(Vector2 position)
        {
            Active = true;
            CanMove = true;
            Position = position;
            AssetName = "Soldier";
            HP = 100;
        }

        public override void Behaviour()
        {
            if (CanMove == true)
            Position.X -= 1;
        }
    }
}
