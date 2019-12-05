using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class DropShipSkid : Drawable
    {
        public enum SkidState { Closed, Opening, Open, Closing };
        public SkidState CurrentSkidState = SkidState.Closed;
        VectorSprite Skid, Piston, Strut;

        float SkidRotation, PistonRotation, StrutRotation;


        public DropShipSkid()
        {
            
        }

        public void Update(GameTime gameTime)
        {

        }

    }
}
