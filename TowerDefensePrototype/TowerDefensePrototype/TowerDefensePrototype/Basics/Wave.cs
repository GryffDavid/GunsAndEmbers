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
