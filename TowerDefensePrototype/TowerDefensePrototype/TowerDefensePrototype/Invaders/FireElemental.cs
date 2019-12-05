using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class FireElemental : Invader
    {
    
        public FireElemental(Vector2 position)
        {
            Active = true;
            MoveVector = new Vector2(-1, 0);
            Position = position;
            CurrentHP = 50;
            MaxHP = 50;
            MoveDelay = 10;
            ResourceMinMax = new Vector2(50, 100);
            CurrentAttackDelay = 0;
            AttackDelay = 3000;
            AttackPower = 1;
            CurrentFrame = 0;
            InvaderType = InvaderType.FireElemental;
            YRange = new Vector2(700, 900);

            CurrentAnimation = new Animation()
            {
                TotalFrames = 1,
                FrameDelay = 500
            };

            Color FireColor = new Color(Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 200);
            Color FireColor2 = new Color(Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 90);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void TrapDamage(TrapType trapType)
        {

        }
    }
}
