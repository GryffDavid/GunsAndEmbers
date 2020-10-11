using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace TowerDefensePrototype
{
    class ClusterBombShell : TimerHeavyProjectile
    {
        public ClusterBombShell(object source, float maxTime, Texture2D texture, Texture2D particleTexture, Vector2 position, 
                                float speed, float angle, float gravity, float damage, float blastRadius, Vector2? yrange = null, bool? verlet = false)
            : base(source, maxTime, texture, position, speed, angle, gravity, damage, blastRadius, yrange, verlet)
        {
            HeavyProjectileType = HeavyProjectileType.ClusterBombShell;

            EmitterList.Add(new Emitter(particleTexture, new Vector2(Position.X + 16, Position.Y + 8), new Vector2(0, 360),
                new Vector2(0.25f, 0.5f), new Vector2(640, 960), 1f, false, new Vector2(-35, 35), new Vector2(-0.5f, 0.5f),
                new Vector2(0.025f, 0.05f), Color.DarkGray, Color.Gray, -0.00f, -1, 10, 5, false, new Vector2(0, 720), true, DrawDepth,
                null, null, null, null, null, null, null, false, false, 150f));
        }
    }
}
