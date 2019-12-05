using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TowerDefensePrototype
{
    public class Level2 : Level
    {
        public Level2()
        {
            Number = 2;
            WaveList = new List<Wave>();

            Wave Wave1 = new Wave(false, 800, 5000,
                new Soldier(new Vector2(2050, 400)),
                new Soldier(new Vector2(2050, 400)),
                new Spider(new Vector2(2050, 400)),
                new Soldier(new Vector2(2050, 400)),
                new Soldier(new Vector2(2050, 400)),
                new Spider(new Vector2(2050, 400)),
                new Soldier(new Vector2(2050, 400)),
                new Soldier(new Vector2(2050, 400)),
                new Spider(new Vector2(2050, 400)),
                new Soldier(new Vector2(2050, 400)),
                new Soldier(new Vector2(2050, 400)),
                new Soldier(new Vector2(2050, 400)),
                new Soldier(new Vector2(2050, 400)),
                new Soldier(new Vector2(2050, 400)),
                new Spider(new Vector2(2050, 400)),
                new Spider(new Vector2(2050, 400)),
                new Spider(new Vector2(2050, 400)),
                new Spider(new Vector2(2050, 400)),
                new Spider(new Vector2(2050, 400)),
                new Spider(new Vector2(2050, 400)));

            WaveList.Add(Wave1);
        }
    }
}
