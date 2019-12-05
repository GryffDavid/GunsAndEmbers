﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TowerDefensePrototype
{
    public class Level6 : Level
    {
        public Level6()
        {
            Number = 6;
            WorldType = "Basic";
            WaveList = new List<Wave>();

            Wave Wave1 = new Wave(false, 1000, 25000,
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)));

            Wave Wave2 = new Wave(false, 800, 6000,
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)),
                new Soldier(new Vector2(1300, 400)));

            WaveList.Add(Wave1);
            WaveList.Add(Wave2);
        }
    }
}