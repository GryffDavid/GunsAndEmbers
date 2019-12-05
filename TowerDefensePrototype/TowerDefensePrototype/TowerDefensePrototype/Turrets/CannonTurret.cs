﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace TowerDefensePrototype
{
    class CannonTurret : Turret
    {
        public CannonTurret(string turretName, string baseName, Vector2 position)
        {
            Active = true;
            TurretAsset = turretName;
            BaseAsset = baseName;
            Position = position;
            Selected = true;
            FireDelay = 200;
            Damage = 20;          
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                //if (Selected == true)
                //    spriteBatch.Draw(Line, new Rectangle(BarrelRectangle.X, BarrelRectangle.Y, Line.Width * 8, Line.Height), null, Color.White, Rotation, new Vector2 (0,+(Line.Height/2)), SpriteEffects.None, 1f);

                BaseRectangle = new Rectangle((int)Position.X - 12, (int)Position.Y - 16 - 6, TurretBase.Width, TurretBase.Height);
                BarrelRectangle = new Rectangle((int)Position.X + 8, (int)Position.Y - 6, TurretBarrel.Width, TurretBarrel.Height);

                spriteBatch.Draw(TurretBarrel, BarrelRectangle, null, Color, Rotation, new Vector2(24, TurretBarrel.Height / 2), SpriteEffects.None, 1f);

                spriteBatch.Draw(TurretBase, BaseRectangle, Color);
            }
        }
    }
}