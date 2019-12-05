﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class TimerHeavyProjectile : HeavyProjectile
    {
        public double CurrentTime, MaxTime;
        public bool Detonated = false;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            CurrentTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentTime > MaxTime)
            {
                Detonated = true;
            }            
        }
    }
}