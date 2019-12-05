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
        public Nullable<Vector2> Position2;
        public Ray WallRay;
        public Plane WallPlane;

        public Wall(Vector2 position, Vector2? position2)
        {            
            Position = position;
            Solid = true;
            MaxHP = 100;
            AssetName = "NullBox";
            TrapType = TrapType.Wall;
            DetonateLimit = -1;
            Animated = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, 8, 8), Color.White);

            if (Position2 != null)
                spriteBatch.Draw(Texture, new Rectangle((int)Position2.Value.X, (int)Position2.Value.Y, 8, 8), Color.Red);
        }
    }
}
