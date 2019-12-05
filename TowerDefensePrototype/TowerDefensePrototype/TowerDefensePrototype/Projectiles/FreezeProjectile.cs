using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class FreezeProjectile : LightProjectile
    {
        //public FreezeProjectile(Vector2 position, float speed, float angle, float gravity)
        //    {
        //        Active = true;
        //        Rotate = true;
        //        Fade = false;
        //        TextureName = "Blank";
        //        HeavyProjectileType = HeavyProjectileType.Ice;
        //        Angle = angle;
        //        Speed = speed;
        //        Gravity = gravity;
        //        Position = position;

        //        Velocity.X = (float)(Math.Cos(angle) * speed);
        //        Velocity.Y = (float)(Math.Sin(angle) * speed);

        //        Color FireColor = Color.LightBlue;
        //        FireColor.A = 100;

        //        Color FireColor2 = Color.LightSkyBlue;
        //        FireColor2.A = 200;

        //        float DrawDepth = (560 - Position.Y) / 100;

        //        Emitter = new Emitter("Splodge", new Vector2(Position.X + 16, Position.Y + 8),
        //            new Vector2(90, 90),
        //            new Vector2(1.5f, 2), new Vector2(30, 35), 0.1f, true,
        //            new Vector2(-20, 20), new Vector2(-4, 4),
        //            new Vector2(0.2f, 0.2f), FireColor, FireColor2, 0.0f, -1, 1, 5, false, new Vector2(0, 720), true, 0);
        //    }   

        public FreezeProjectile(Vector2 position, Vector2 Direction)
        {
            Active = true;
            Position = position;
            Ray = new Ray(new Vector3(Position.X, Position.Y, 0), new Vector3(Direction.X, Direction.Y, 0));
            LightProjectileType = LightProjectileType.Freeze;
        }
    }
}
