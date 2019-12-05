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

            Wave Wave1 = new Wave(false, 200, 6000,
                 new Tank(new Vector2(2050, 600)),
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

            //Wave Wave2 = new Wave(false, 800, 6000,
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)),
            //    new Soldier(new Vector2(2050, 600)));

            WaveList.Add(Wave1);
            //WaveList.Add(Wave2);
        }
    }
}
