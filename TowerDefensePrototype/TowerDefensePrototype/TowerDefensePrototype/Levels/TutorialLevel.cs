using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TowerDefensePrototype
{
    public class TutorialLevel : Level
    {
        public TutorialLevel()
        {
            Number = 0;
            WorldType = "Basic";
            WaveList = new List<Wave>();

            Wave Wave1 = new Wave(false, 200, 6000,
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)),
                 new Soldier(new Vector2(2050, 600)));

            WaveList.Add(Wave1);
        }
    }
}
