using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace TowerDefensePrototype
{
    class RifleMan : LightRangedInvader
    {
        public RifleMan(Vector2 position)
        {
            Position = position;            
            Speed = 0.68f;            
            CurrentHP = 20;
            MaxHP = 20;
            ResourceMinMax = new Vector2(8, 20);
            InvaderType = InvaderType.RifleMan;
            YRange = new Vector2(700, 900);            
            InvaderState = AnimationState_Invader.Walk;

            RangedDamageStruct = new InvaderRangedStruct()
            {
                AngleRange = new Vector2(170, 190),
                Damage = 10,
                MaxFireDelay = 250,
                CurrentFireDelay = 0,
                DistanceRange = new Vector2(600, 800)
            };
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            base.Update(gameTime, cursorPosition);
        }
    }
}
