using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowerDefensePrototype
{
    public class Wave
    {
        public List<Invader> InvaderList;
        public float WaveTime, InvaderTime;
        public bool Overflow;

        public Wave(bool overflow, float invaderTime, float waveTime, params Invader[] invaders)
        {
            InvaderList = new List<Invader>();
            WaveTime = waveTime;
            InvaderTime = invaderTime;
            Overflow = overflow;

            for (int i = 0; i < invaders.Count(); i++)
            {
                Invader newInvader = invaders[i];
                InvaderList.Add(newInvader);
            }
        }
    }
}
