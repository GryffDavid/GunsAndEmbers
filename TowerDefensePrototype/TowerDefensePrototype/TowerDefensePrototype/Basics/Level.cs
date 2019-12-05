using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TowerDefensePrototype
{
    public class Level
    {
        public int Number;
        public string WorldType;
        public List<Wave> WaveList; 

        //STORE WAVES HERE IN A LIST//
        //CALL WAVE BY IT'S INDEX IN THE LIST//
        //I.E. CurrentWave = Level1.WaveList[WaveIndex]
        //Freaking. Brilliant.//
    }
}
