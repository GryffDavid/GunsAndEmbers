using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowerDefensePrototype
{
    public class Wave
    {
        public List<Invader> InvaderList;
        public float TimeToNextWave, TimeToNextInvader;
        public bool Overflow;

        public Wave(bool overflow, float invaderTime, float waveTime, params Invader[] invaders)
        {
            InvaderList = new List<Invader>();
            TimeToNextWave = waveTime;
            TimeToNextInvader = invaderTime;
            Overflow = overflow;

            for (int i = 0; i < invaders.Count(); i++)
            {
                Invader newInvader = invaders[i];
                InvaderList.Add(newInvader);
            }
        }
    }
}
