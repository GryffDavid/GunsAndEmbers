using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class Torpedo : HeavyProjectile
    {
        public Torpedo(Texture2D texture, Texture2D particleTexture, Vector2 position, float speed, float angle, float gravity, float damage, Vector2? yrange = null)
            : base(texture, position, speed, angle, gravity, damage, yrange)
        {
            HeavyProjectileType = HeavyProjectileType.Torpedo;

            Rotate = true;
            Fade = false;

            EmitterList[0] = new Emitter(particleTexture, Position,
                new Vector2((float)Math.Atan2(Velocity.Y, Velocity.X),(float)Math.Atan2(Velocity.Y, Velocity.X)),
                new Vector2(0, 0), new Vector2(40, 50), 0.5f, true, new Vector2(0, 0),
                new Vector2(0, 2), new Vector2(0.2f, 0.3f), Color.MediumPurple, Color.Purple, 0, -1, 1, 10, 
                false, new Vector2(0, 720), true);
        }
    }
}
