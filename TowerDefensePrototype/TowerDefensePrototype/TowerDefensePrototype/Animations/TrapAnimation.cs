﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowerDefensePrototype
{
    public class TrapAnimation : Animation
    {
        public TrapAnimationState CurrentTrapState;

        public new TrapAnimation ShallowCopy()
        {
            return (TrapAnimation)this.MemberwiseClone();
        }
    }
}
