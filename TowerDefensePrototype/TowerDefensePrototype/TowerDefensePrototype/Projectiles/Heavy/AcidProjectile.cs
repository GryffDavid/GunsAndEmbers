using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class AcidProjectile : HeavyProjectile
    {
        public AcidProjectile(Texture2D texture, Texture2D particleTexture, Vector2 position, float speed, float angle, float gravity, float damage, Vector2? yrange = null) : 
            base(texture, position, speed, angle, gravity, damage, yrange)
        {
            Rotate = true;
            HeavyProjectileType = HeavyProjectileType.Acid;

            EmitterList = new List<Emitter>();

            Color FireColor = Color.Lime;
            Color FireColor2 = Color.LimeGreen;

            EmitterList.Add(new Emitter(particleTexture, new Vector2(Position.X + 16, Position.Y + 8),
                new Vector2(90, 90),
                new Vector2(1.5f, 2), new Vector2(30, 45), 0.1f, true,
                new Vector2(-20, 20), new Vector2(-4, 4),
                new Vector2(0.1f, 0.25f), FireColor, FireColor2, 0.2f, -1, 1, 1, false, new Vector2(0, 720), true));
        }
    }
}
