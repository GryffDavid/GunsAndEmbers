using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class SpikesTrap : Trap
    {
        public SpikesTrap(Vector2 position)
            : base(position)
        {
            Solid = false;
            TrapType = TrapType.Spikes;
            MaxHP = 50;
            DetonateDelay = 2000;
            DetonateLimit = 8;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
