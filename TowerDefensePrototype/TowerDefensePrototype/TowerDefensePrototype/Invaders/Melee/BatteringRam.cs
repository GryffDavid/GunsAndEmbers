using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class BatteringRam : Invader
    {
        //The invaders needed to make sure the ram functions
        int CurrentOperators = 0;
        int NeededOperators = 2;

        public BatteringRam()
        {

        }

        public void Update(GameTime gameTime)
        {
            switch (CurrentMicroBehaviour)
            {
                #region Moving Forwards
                case MicroBehaviour.MovingForwards:

                    break;
                #endregion

                #region Stationary
                case MicroBehaviour.Stationary:

                    break;
                #endregion

                #region Attack
                case MicroBehaviour.Attack:

                    break;
                #endregion
            }
        }
    }
}
