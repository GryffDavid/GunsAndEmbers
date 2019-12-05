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
    class GatlingTurret : Turret
    {      
        public GatlingTurret(Vector2 position)
        {
            Active = true;
            TurretType = TurretType.Gatling;
            TurretAsset = "Turrets/MachineTurretBarrel2";
            BaseAsset = "Turrets/MachineTurretBase";
            Position = position;
            Selected = true;
            FireDelay = 200;
            Damage = 7;
            AngleOffset = 2;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                BaseRectangle = new Rectangle((int)Position.X+20, (int)Position.Y+6, TurretBase.Width, TurretBase.Height);
                BarrelRectangle = new Rectangle((int)Position.X+20, (int)Position.Y+6, TurretBarrel.Width, TurretBarrel.Height);

                BarrelPivot = new Vector2(20, TurretBarrel.Height / 2);
                BasePivot = new Vector2(TurretBase.Width / 2, TurretBase.Height / 2-10);

                spriteBatch.Draw(TurretBarrel, BarrelRectangle, null, Color, Rotation, BarrelPivot, SpriteEffects.None, 1f);

                spriteBatch.Draw(TurretBase, BaseRectangle, null, Color, 0, BasePivot, SpriteEffects.None, 1f);
            }
        }
    }
}