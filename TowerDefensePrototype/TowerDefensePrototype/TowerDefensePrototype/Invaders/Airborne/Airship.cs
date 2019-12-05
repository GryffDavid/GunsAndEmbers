using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Airship : HeavyRangedInvader
    {
        public Airship(Vector2 position)
        {
            Active = true;            
            Direction = new Vector2(-1, 0);
            ActualPosition = position;
            MaxHP = 300;
            CurrentHP = MaxHP;
            //MoveDelay = 20;
            ResourceMinMax = new Vector2(1, 5);
            CurrentAttackDelay = 0;
            AttackDelay = 1500;
            TowerAttackPower = 4;
            InvaderType = InvaderType.Airship;
            CurrentFrame = 0;
            YRange = new Vector2(60, 150);
            RangedAttackPower = 20;
            DistanceRange = new Vector2(200, 800);
            AngleRange = new Vector2(110, 160);
            PowerRange = new Vector2(9, 12);
            Airborne = true;

            CurrentAnimation = new InvaderAnimation()
            {
                TotalFrames = 1,
                FrameDelay = 500
            };
        }

        public override void TrapDamage(Trap trap)
        {            
            throw new NotImplementedException();
        }
    }
}
