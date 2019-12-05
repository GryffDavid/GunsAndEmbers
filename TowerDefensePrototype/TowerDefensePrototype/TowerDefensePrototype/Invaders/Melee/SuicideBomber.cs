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
        public override float OriginalSpeed { get { return 0.65f; } }

        public SuicideBomber(Vector2 position, Vector2? yRange = null)
            : base(position, yRange)
        {
            MaxHP = 100;
            ResourceMinMax = new Vector2(1, 5);
            InvaderType = InvaderType.SuicideBomber;
        }
    }
}
