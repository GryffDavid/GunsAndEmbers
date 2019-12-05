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
            StartDelay = 6;

            Wave Wave1 = new Wave(false, 200, 6000,
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
