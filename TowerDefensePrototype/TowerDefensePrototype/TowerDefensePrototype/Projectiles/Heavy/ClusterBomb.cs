using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class  ClusterBomb : HeavyProjectile
    {
        public ClusterBomb(object source, Texture2D texture, Texture2D particleTexture, Vector2 position, 
                           float speed, float angle, float gravity, float damage, Vector2? yrange = null)
            : base(source, texture, position, speed, angle, gravity, damage, yrange)
        {
            HeavyProjectileType = HeavyProjectileType.ClusterBomb;

            Rotate = true;

            EmitterList = new List<Emitter>();

            Color ParticleColor1 = Color.Gray;
            Color ParticleColor2 = Color.DarkGray;

            EmitterList.Add(new Emitter(particleTexture, new Vector2(Position.X + 16, Position.Y + 8), new Vector2(90, 180), 
                new Vector2(1.5f, 2), new Vector2(15, 20), 0.2f, true, new Vector2(-20, 20), new Vector2(-4, 4),
                new Vector2(0.1f, 0.25f), ParticleColor1, ParticleColor2, 0.0f, -1, 1, 1, false, new Vector2(0, 720)));
        }
    }
}
