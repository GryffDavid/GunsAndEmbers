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
    class JumpMan : LightRangedInvader
    {
        public JumpMan(Vector2 position)
        {
            Speed = 0.68f;
            Position = position;
            CurrentHP = 20;
            MaxHP = 20;
            ResourceMinMax = new Vector2(8, 20);
            InvaderType = InvaderType.JumpMan;
            YRange = new Vector2(700, 900);
            InvaderState = InvaderState.Walk;
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            //if (Velocity.X < 0)
            //{
            //    CurrentInvaderState = InvaderState.Walk;
            //}

            //switch (InvaderState)
            //{
            //    case InvaderState.Walk:
            //        CurrentAnimation = AnimationList[0];
            //        break;
            //}

            base.Update(gameTime, cursorPosition);
        }
    }
}
