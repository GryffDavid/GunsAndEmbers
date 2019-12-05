﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class  BoomerangProjectile : HeavyProjectile
    {
        public BoomerangProjectile(Vector2 position, float speed, float angle, float gravity, float damage, float blastRadius, Vector2? yrange = null)
        {
            Active = true;
            Rotate = true;
            Fade = false;
            TextureName = "Projectiles/CannonRound";
            HeavyProjectileType = HeavyProjectileType.Boomerang;
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

            Color ParticleColor1 = Color.Gray;
            Color ParticleColor2 = Color.DarkGray;

            EmitterList.Add(new Emitter("Particles/Smoke", new Vector2(Position.X + 16, Position.Y + 8), new Vector2(90, 180), 
                new Vector2(1.5f, 2), new Vector2(15, 20), 0.2f, true, new Vector2(-20, 20), new Vector2(-4, 4), 
                new Vector2(0.25f, 0.5f), ParticleColor1, ParticleColor2, 0.0f, -1, 1, 1, false, new Vector2(0, 720)));

            Damage = damage;
        }
    }
}