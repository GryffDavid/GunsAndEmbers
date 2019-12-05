﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class Arrow : HeavyProjectile
    {
        public Arrow(Vector2 position, float speed, float angle, float gravity)
        {
            Active = true;
            Rotate = true;
            Fade = false;
            TextureName = "Arrow";
            HeavyProjectileType = HeavyProjectileType.Arrow;
            Angle = angle;
            Speed = speed;
            Gravity = gravity;
            Position = position;

            Velocity.X = (float)(Math.Cos(angle) * speed);
            Velocity.Y = (float)(Math.Sin(angle) * speed);

            Emitter = new Emitter("Smoke", new Vector2(Position.X, Position.Y),
                new Vector2((float)Math.Atan2(Velocity.Y, Velocity.X), (float)Math.Atan2(Velocity.Y, Velocity.X)), new Vector2(1.5f, 2), new Vector2(50, 60), 0.2f, true, 
                new Vector2(-20, 20), new Vector2(-4, 4), new Vector2(0.1f, 0.3f), Color.White, Color.WhiteSmoke, 0.0f, -1, 1, 10, false, new Vector2(0, 720), true);

            Rotation = (float)Math.Atan2(Velocity.Y, Velocity.X);

            YRange = new Vector2(420, 530);

            Damage = 100;
        }
    }
}