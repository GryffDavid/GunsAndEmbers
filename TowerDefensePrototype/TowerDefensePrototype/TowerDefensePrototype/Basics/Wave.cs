using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowerDefensePrototype
{
    public class Wave
    {
        public List<object> InvaderList;
        public float TimeToNextWave, TimeToNextInvader;
        public bool Overflow;

        /// <summary>
        /// A set of invaders that are to be added to the level
        /// </summary>
        /// <param name="overflow">Whether the "Start Waves" button appears to give the player a break or not.</param>
        /// <param name="invaderTime">The interval between invaders</param>
        /// <param name="waveTime">The interval between waves</param>
        /// <param name="invaders">Can be an invader, an integer or a float. Invader gets added to the level. Integer adds a pause in ms in the middle of a wave. A float changes the delay between invaders.</param>

        public Wave(bool overflow, float invaderTime, float waveTime, params object[] invaders)
        {
            InvaderList = new List<object>();
            TimeToNextWave = waveTime;
            TimeToNextInvader = invaderTime;
            Overflow = overflow;

            for (int i = 0; i < invaders.Count(); i++)
            {
                object newInvader = invaders[i];
                InvaderList.Add(newInvader);
            }
        }
    }
}
