﻿using System;
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
            TextureName = "Projectiles/FelProjectile2";
            HeavyProjectileType = HeavyProjectileType.FelProjectile;
            Angle = angle;
            Speed = speed;
            Gravity = gravity;
            Position = position;
            BlastRadius = blastRadius;
            EmitterList = new List<Emitter>();

            if (yrange == null)
            {
                YRange = new Vector2(520, 630);
            }
            else
            {
                YRange = yrange.Value;
            }

            Velocity.X = (float)(Math.Cos(angle) * speed);
            Velocity.Y = (float)(Math.Sin(angle) * speed);

            //Color ParticleColor1 = Color.LimeGreen;
            //Color ParticleColor2 = Color.Purple;

            //EmitterList.Add(new Emitter("Particles/Smoke", new Vector2(Position.X + 16, Position.Y + 8), new Vector2(90, 180),
            //    new Vector2(1.5f, 2), new Vector2(15, 20), 0.2f, true, new Vector2(-20, 20), new Vector2(-4, 4),
            //    new Vector2(0.25f, 0.5f), ParticleColor1, ParticleColor2, 0.0f, -1, 1, 1, false, new Vector2(0, 720)));

            //EmitterList.Add(new Emitter("Particles/FireParticle", new Vector2(Position.X + 16, Position.Y + 8),
            //                    new Vector2(0, 360), new Vector2(0.5f, 0.75f), new Vector2(20, 30), 0.01f, true, new Vector2(-20, 20),
            //                    new Vector2(-4, 4), new Vector2(0.5f, 0.75f), Color.LimeGreen, Color.Green, 0.0f, -1, 10, 1, false, new Vector2(0, 720),
            //                    false, null));

            Damage = damage;
        }
    }
}