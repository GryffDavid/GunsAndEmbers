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
            Active = true;
            CanMove = true;
            MoveVector = new Vector2(-1, 0);
            Position = position;
            AssetName = "Soldier";
            CurrentHealth = 50;
            MaxHealth = 50;
            MoveDelay = 10;
        }

        public override void TrapDamage(TrapType trapType)
        {
            if (VulnerableToTrap == true)
            {
                switch (trapType)
                {
                    case TrapType.Fire:
                        CurrentHealth -= 10;
                        break;

                    case TrapType.Spikes:
                        CurrentHealth -= 25;
                        break;
                }
            }
        }
    }
}
