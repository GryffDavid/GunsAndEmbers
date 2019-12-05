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
        public FelProjectile(Vector2 position, float speed, float angle, float gravity, float damage, float blastRadius, Vector2? yrange = null)
        {
            Active = true;
            Rotate = true;
            Fade = false;
            TextureName = "Blank";
            HeavyProjectileType = HeavyProjectileType.FelProjectile;
            Angle = angle;
            Speed = speed;
            Gravity = gravity;
            Position = position;
            BlastRadius = blastRadius;
            EmitterList = new List<Emitter>();

            if (yrange == null)
            {
                YRange = new Vector2(690, 930);
            }
            else
            {
                YRange = yrange.Value;
            }

            Velocity.X = (float)(Math.Cos(angle) * speed);
            Velocity.Y = (float)(Math.Sin(angle) * speed);

            Emitter FlashSparks = new Emitter("Particles/GlowBall", Position,
            new Vector2(0, 360), new Vector2(2, 3), new Vector2(15, 25), 1f, true, new Vector2(0, 360),
            new Vector2(2, 5), new Vector2(0.25f, 0.25f), Color.LimeGreen, Color.LimeGreen, 0.0f, -1, 1, 1,
            false, new Vector2(0, 720), false, null, false, false);

            Emitter FlashSmoke = new Emitter("Particles/Smoke", Position,
            new Vector2(0, 360), new Vector2(1, 2), new Vector2(5, 15), 1f, true, new Vector2(0, 360),
            new Vector2(2, 5), new Vector2(1f, 1f), Color.LimeGreen, Color.LimeGreen, 0.0f, -1, 1, 1,
            false, new Vector2(0, 720), false, null, false, false);

            EmitterList.Add(FlashSmoke);
            EmitterList.Add(FlashSparks);

            Damage = damage;
        }
    }
}
