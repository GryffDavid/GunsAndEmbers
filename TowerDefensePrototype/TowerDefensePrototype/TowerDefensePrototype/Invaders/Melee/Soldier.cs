using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Soldier : Invader
    {
        public Soldier(Vector2 position)
        {
            Speed = 0.68f;
            Position = position;
            CurrentHP = 20;
            MaxHP = 20;
            ResourceMinMax = new Vector2(8, 20);
            YRange = new Vector2(700, 900);
            InvaderType = InvaderType.Soldier;

            InvaderState = InvaderState.Walk;

            MeleeDamageStruct = new InvaderMeleeStruct()
            {
                CurrentAttackDelay = 0,
                MaxAttackDelay = 500,
                Damage = 1
            };
        }
    }
}
