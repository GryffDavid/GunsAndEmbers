using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class Fire : AnimatedSprite
    {
        public void Initialize(Vector2 position)
        {
            Position = position;
            AssetName = "FireStrip";
            Color = Color.White;
            FrameSize = new Vector2(32,43);
            FrameCount = 4;
            FrameTime = 60;
            Scale = new Vector2(1,1);
            Looping = true;
            base.Initialize(AssetName, Position, FrameSize, FrameCount, FrameTime, Color, Scale, Looping);
        }
    }
}
