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
        public override float OriginalSpeed { get { return 0.68f; } }

        public JumpMan(Vector2 position, Vector2? yRange = null)
            : base(position, yRange)
        {
            MaxHP = 20;
            ResourceMinMax = new Vector2(8, 20);
            InvaderType = InvaderType.JumpMan;
            InvaderAnimationState = AnimationState_Invader.Walk;
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
