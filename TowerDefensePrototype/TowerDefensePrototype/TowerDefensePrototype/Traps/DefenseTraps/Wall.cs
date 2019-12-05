using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Wall : Trap
    {
        public Wall(Vector2 position)
        {
            Position = position;
            Solid = true;
            MaxHP = 100;
            TrapType = TrapType.Wall;
            DetonateLimit = -1;

            CurrentTrapState = TrapState.Untriggered;
        }

        public override void Update(GameTime gameTime)
        {
            switch (CurrentTrapState)
            {
                case TrapState.Untriggered:
                    CurrentTexture = TextureList[0];
                    CurrentAnimation = new Animation() { Texture = CurrentTexture, TotalFrames = 1, FrameDelay = 150 };
                    break;
            }

            base.Update(gameTime);
        }
    }
}
