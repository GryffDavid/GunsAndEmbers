using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TowerDefensePrototype
{
    public class Level1 : Level
    {
        public Level1()
        {
            Number = 1;
            WorldType = "Basic";
            WaveList = new List<Wave>();

            Wave Wave1 = new Wave(false, 500, 6000,
                new Spider(new Vector2(1300, 400)),
                new Spider(new Vector2(1300, 400)),
                new Spider(new Vector2(1300, 400)));

            Wave Wave2 = new Wave(false, 500, 5000,
               new Spider(new Vector2(1300, 400)),
               new Spider(new Vector2(1300, 400)),
               new Spider(new Vector2(1300, 400)));

            Wave Wave3 = new Wave(false, 500, 4000,
               new Spider(new Vector2(1300, 400)),
               new Spider(new Vector2(1300, 400)),
               new Spider(new Vector2(1300, 400)));

            Wave Wave4 = new Wave(false, 500, 4000,
               new Soldier(new Vector2(1300, 400)),
               new Soldier(new Vector2(1300, 400)),
               new Soldier(new Vector2(1300, 400)));

            Wave Wave5 = new Wave(false, 500, 4000,
               new Soldier(new Vector2(1300, 400)),
               new Soldier(new Vector2(1300, 400)),
               new Soldier(new Vector2(1300, 400)));

            WaveList.Add(Wave1);
            WaveList.Add(Wave2);
            WaveList.Add(Wave3);
            WaveList.Add(Wave4);
            WaveList.Add(Wave5);
        }
    }
}
