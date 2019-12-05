using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TowerDefensePrototype
{
    public class Level3 : Level
    {
        public Level3()
        {
            Number = 3;
            WorldType = "Basic";
            WaveList = new List<Wave>();

            Wave Wave1 = new Wave(false, 800, 6000,
                new Spider(new Vector2(1300, 400)),
                new Spider(new Vector2(1300, 400)),
                new Spider(new Vector2(1300, 400)),
                new Spider(new Vector2(1300, 400)),
                new Spider(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Spider(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Spider(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Spider(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Spider(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)));

            Wave Wave2 = new Wave(false, 800, 6000,
                new Archer(new Vector2(1300, 400)),
                new Archer(new Vector2(1300, 400)),
                new Archer(new Vector2(1300, 400)),
                new Archer(new Vector2(1300, 400)),
                new Archer(new Vector2(1300, 400)),
                new Archer(new Vector2(1300, 400)),
                new Archer(new Vector2(1300, 400)),
                new Archer(new Vector2(1300, 400)),
                new Archer(new Vector2(1300, 400)),
                new Archer(new Vector2(1300, 400)),
                new Archer(new Vector2(1300, 400)));

            WaveList.Add(Wave1);
            WaveList.Add(Wave2);
        }
    }
}
