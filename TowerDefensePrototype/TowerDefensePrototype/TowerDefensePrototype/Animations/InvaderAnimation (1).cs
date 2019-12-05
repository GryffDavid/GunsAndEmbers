using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowerDefensePrototype
{
    public class InvaderAnimation : Animation
    {
        public AnimationState_Invader CurrentInvaderState;

        public new InvaderAnimation ShallowCopy()
        {
            return (InvaderAnimation)this.MemberwiseClone();
        }
    }
}
