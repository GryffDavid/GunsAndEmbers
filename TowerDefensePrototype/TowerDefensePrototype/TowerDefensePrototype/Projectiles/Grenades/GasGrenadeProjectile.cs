using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    //This should rather implement verlet intergration for the physics calculations. It'll look far better
    class GasGrenadeProjectile : TimerHeavyProjectile
    {
        public GasGrenadeProjectile(object source, float maxTime, Texture2D texture, Texture2D particleTexture, Vector2 position, 
                                 float speed, float angle, float gravity, float damage, float blastRadius, Vector2? yrange = null, bool? verlet = true) 
            : base(source, maxTime, texture, position, speed, angle, gravity, damage, blastRadius, yrange, verlet)
        {
            HeavyProjectileType = HeavyProjectileType.GasGrenade;

            Rotate = true;

            //CanBounce = true;
            //HardBounce = true;
            //StopBounce = true;

            EmitterList = new List<Emitter>();            

            Color ParticleColor1 = Color.Gray;
            Color ParticleColor2 = Color.DarkGray;

            EmitterList.Add(new Emitter(particleTexture, new Vector2(Position.X + 16, Position.Y + 8), new Vector2(90, 180),
                 new Vector2(0, 0), new Vector2(40, 60), 0.9f, true, new Vector2(0, 360), new Vector2(-0.5f, 0.5f),
                 new Vector4(0.25f, 0.5f, 0.25f, 0.5f), ParticleColor1, ParticleColor2, -0.00f, -1, 10, 1, false, new Vector2(0, 720), null, null,
                 null, null, null, null, null, null, null, true, true));
        }
    }
}
