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
using GameDataTypes;

namespace TowerDefensePrototype
{
    public class Level
    {
        public Texture2D GroundTexture, SkyBackgroundTexture, ForegroundTexture;
        public int Number, Resources;
        public WorldType WorldType;
        public List<Wave> WaveList;
        public Weather StartWeather;
        public List<SoundEffect> AmbienceList = new List<SoundEffect>();
        public LevelDialogue LevelDialogue;

        //A float in the middle of the list changes the delay between invaders
        //An in in the middle of the list creates a pause without changing the timing
        
        //IDEA: A string in the middle of the list points to dialogue that needs to be said, e.g.:
        //"That pneumatic ram appears to need other soldiers to operate it. Take them out and the
        //ram will stop, but keep an eye out for soldiers retreating to take up his comrades' place!"

        public virtual void LoadContent(ContentManager contentManager)
        {

        }

        //STORE WAVES HERE IN A LIST//
        //CALL WAVE BY IT'S INDEX IN THE LIST//
        //I.E. CurrentWave = Level1.WaveList[WaveIndex]
        //Freaking. Brilliant.//
    }
}
