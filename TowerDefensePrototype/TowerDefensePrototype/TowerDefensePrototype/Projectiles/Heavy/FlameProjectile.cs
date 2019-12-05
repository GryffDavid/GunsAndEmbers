using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class FlameProjectile : HeavyProjectile
    {
        public FlameProjectile(object source, Texture2D texture, Texture2D particleTexture, Vector2 position, 
                               float speed, float angle, float gravity, float damage, Vector2? yrange = null)
            : base(source, texture, position, speed, angle, gravity, damage, yrange)
        {
            HeavyProjectileType = HeavyProjectileType.FlameThrower;

            //Active = true;
            //Rotate = true;

            Color FireColor = Color.Orange;
            FireColor.A = 100;

            Color FireColor2 = Color.Orange;
            FireColor2.A = 200;

            //EmitterList.Add(new Emitter(particleTexture, new Vector2(Position.X + 16, Position.Y + 8), new Vector2(0, 0),
            //    new Vector2(0, 0), new Vector2(640, 960), 0.75f, true, new Vector2(0, 0), new Vector2(0, 0),
            //    new Vector2(0.1f, 0.1f), FireColor * 0.5f, Color.Black, -0.05f, -1, 25, 1, false, new Vector2(0, 720), true, DrawDepth,
            //    null, null, null, null, null, null, null, true, true));

            //Emitter NewEmitterName = new Emitter(particleTexture, new Vector2(Position.X + 16, Position.Y + 8), new Vector2(-4f, 4f), 
            //    new Vector2(1f, 3f), new Vector2(1200f, 1500f), 1f, false, new Vector2(0f, 0f), new Vector2(-2f, 2f), 
            //    new Vector2(0.1085f, 0.109f), new Color(255, 128, 0, 0), new Color(0, 0, 0, 255), -0.004f, 0f, 16f, 8, false,
            //    new Vector2(0f, 720f), true, DrawDepth, false, false, new Vector2(0f, 0f), new Vector2(0f, 0f), 0f, true, 
            //    new Vector2(0.021f, 0.036f), false, false, 0f, false, false, true, null);
            //EmitterList.Add(NewEmitterName);

            Emitter flameEmitter = new Emitter(particleTexture, new Vector2(Position.X + 16, Position.Y + 8), new Vector2(0, 360),
                new Vector2(0.25f, 0.5f), new Vector2(640, 960), 1f, false, new Vector2(-35, 35), new Vector2(-0.5f, 0.5f),
                new Vector2(0.025f, 0.05f), new Color(255, 128, 0, 60), new Color(0, 0, 0, 255), -0.00f, -1, 24, 1, false, new Vector2(0, 720), true, DrawDepth,
                null, null, null, null, null, null, null, false, false, 150f);
            flameEmitter.Emissive = true;
            this.Emissive = true;

            EmitterList.Add(flameEmitter);
        }
    }
}
