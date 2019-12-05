using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Wall : Trap
    {
        public Wall(Vector2 position)
        {
            Position = position;
        }

        public override int HP
        {
            get { return 50; }
        }

        public override bool  Solid
        {
            get { return true; }
        }

        public override string AssetName 
        { 
            get { return "Wall"; } 
        }

        //public override TrapType TrapType
        //{
        //    get { return TrapType.Wall; }
        //}
    }
}
