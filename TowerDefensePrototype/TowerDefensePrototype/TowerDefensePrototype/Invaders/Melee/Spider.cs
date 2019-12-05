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
        public override float OriginalSpeed { get { return 0.65f; } }

        public Spider(Vector2 position, Vector2? yRange = null)
            : base(position, yRange)
         {
            MaxHP = 40;            
            ResourceMinMax = new Vector2(1, 5);
            InvaderType = InvaderType.Spider;
        }
    }
}
