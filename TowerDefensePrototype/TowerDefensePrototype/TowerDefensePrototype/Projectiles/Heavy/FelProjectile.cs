using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class FelProjectile : HeavyProjectile
    {
        public FelProjectile(object source, Texture2D texture, Texture2D glowballTexture, Texture2D smokeTexture, Vector2 position, 
                            float speed, float angle, float gravity, float damage, float blastRadius, Vector2? yrange = null) 
            : base(source, texture, position, speed, angle, gravity, damage, yrange, blastRadius)
        {
            HeavyProjectileType = HeavyProjectileType.FelProjectile;

            Rotate = true;

            EmitterList = new List<Emitter>();

            Emitter FlashSparks = new Emitter(glowballTexture, Position,
            new Vector2(0, 360), new Vector2(2, 3), new Vector2(15, 25), 1f, true, new Vector2(0, 360),
            new Vector2(2, 5), new Vector4(0.25f, 0.25f, 0.25f, 0.25f), Color.LimeGreen, Color.LimeGreen, 0.0f, -1, 1, 1,
            false, new Vector2(0, 720), false, null, false, false);

            Emitter FlashSmoke = new Emitter(smokeTexture, Position,
            new Vector2(0, 360), new Vector2(1, 2), new Vector2(5, 15), 1f, true, new Vector2(0, 360),
            new Vector2(2, 5), new Vector4(1f, 1f,  1f, 1f), Color.LimeGreen, Color.LimeGreen, 0.0f, -1, 1, 1,
            false, new Vector2(0, 720), false, null, false, false);

            EmitterList.Add(FlashSmoke);
            EmitterList.Add(FlashSparks);
        }
    }
}
