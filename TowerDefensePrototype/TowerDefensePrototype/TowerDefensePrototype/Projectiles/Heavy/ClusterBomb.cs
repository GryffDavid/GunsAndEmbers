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

            EmitterList.Add(new Emitter(particleTexture, new Vector2(Position.X + 16, Position.Y + 8), new Vector2(0, 360),
                new Vector2(0.25f, 0.5f), new Vector2(640, 960), 1f, false, new Vector2(-35, 35), new Vector2(-0.5f, 0.5f),
                new Vector2(0.025f, 0.05f), Color.DarkGray, Color.Gray, -0.00f, -1, 10, 5, false, new Vector2(0, 720), true, DrawDepth,
                null, null, null, null, null, null, null, false, false, 150f));
        }
    }
}
