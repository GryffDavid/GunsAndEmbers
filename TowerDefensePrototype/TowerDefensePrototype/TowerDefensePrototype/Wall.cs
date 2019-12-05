using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class Wall : Trap
    {
        string AssetName;
        int HP;

        public Wall(int hp, int slotNumber)
        {
            AssetName = "Wall";
            HP = hp;
            base.ResourceCost = 100;
            base.Initialize(AssetName, new Vector2(100, 100));
        }
    }
}
