using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class Powerup
    {
        //Effect - This should be described as a stat and a percentage that are changed.

        //This is the object in the game that springs from the delivery pod.
        //Effect is the actual powerup effect that the player gets from picking up the powerup.

        //Effects should be created just like turrets. Each effect is slightly different and a child class of the main class
        //public PowerupEffect CurrentEffect = new PowerupEffect() 
        //{ 
        //    CurrentTime = 0, 
        //    MaxTime = 15000, 
        //    GeneralType = GeneralPowerup.BlastRadii
        //};

        public bool Active;
        public PowerupEffect CurrentEffect;
        public float CurrentTime, MaxTime;

        public Powerup(float maxTime, PowerupEffect powerupEffect)
        {
            Active = true;            
            MaxTime = maxTime;
            CurrentTime = MaxTime;
            CurrentEffect = powerupEffect;
        }

        public void Update(GameTime gameTime)
        {
            CurrentTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentTime <= 0)
            {
                Active = false;
            }

            //CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            //if (CurrentTime >= MaxTime)
            //{
            //    Active = false;
            //}
        }
    }
}
