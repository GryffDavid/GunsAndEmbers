using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace TowerDefensePrototype
{
    public class Level1 : Level
    {
        public Level1()
        {
            Number = 1;
            WaveList = new List<Wave>();
            WorldType = WorldType.Snowy;
            StartWeather = Weather.Snow;
            Resources = 1300;
                        
            //A float in the middle of the list changes the delay between invaders
            //An int in the middle of the list creates a pause without changing the timing

            Wave Wave1 = new Wave(true, 350, 100,
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850)),
                 new Soldier(new Vector2(2050, 600), new Vector2(700, 850))
                 //, new StationaryCannon(new Vector2(2050, 700), new Vector2(700, 850))
                   //, new RifleMan(new Vector2(2050, 600), new Vector2(700, 850))
                   //, new RifleMan(new Vector2(2050, 600), new Vector2(700, 850))
                   //, new RifleMan(new Vector2(2050, 600), new Vector2(700, 850))
                   //, new RifleMan(new Vector2(2050, 600), new Vector2(700, 850))
                   //, new RifleMan(new Vector2(2050, 600), new Vector2(700, 850))
                 );

            Wave Wave3 = new Wave(true, 200, 1000,
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)));

            Wave Wave4 = new Wave(true, 200, 1000,
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)));

            Wave Wave5 = new Wave(true, 200, 1000,
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)));

            Wave Wave6 = new Wave(true, 200, 3000,
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)));

            Wave Wave7 = new Wave(true, 200, 1000,
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)));

            Wave Wave8 = new Wave(false, 200, 1000,
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)));

            Wave Wave9 = new Wave(true, 200, 1000,
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)));

            WaveList.Add(Wave1);
            //WaveList.Add(Wave3);
            //WaveList.Add(Wave4);
            //WaveList.Add(Wave5);
            //WaveList.Add(Wave6);
            //WaveList.Add(Wave7);
            //WaveList.Add(Wave8);
            //WaveList.Add(Wave9);
        }

        public override void LoadContent(ContentManager contentManager)
        {
            GroundTexture = contentManager.Load<Texture2D>("Backgrounds/Ground");
            ForegroundTexture = contentManager.Load<Texture2D>("Backgrounds/Foreground");
            SkyBackgroundTexture = contentManager.Load<Texture2D>("Backgrounds/Sky");

            //AmbienceList = new List<SoundEffect>()
            //{
            //    contentManager.Load<SoundEffect>("Sounds/Ambience/PolarWindAmbience")
            //};
        }
    }
}
