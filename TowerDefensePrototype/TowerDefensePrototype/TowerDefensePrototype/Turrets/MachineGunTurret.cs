﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{    
    class MachineGunTurret : Turret
    {      
        public MachineGunTurret(Vector2 position)
        {
            Active = true;
            TurretType = TurretType.MachineGun;
            DamageType = DamageType.Kinetic;
            Position = position;
            Selected = true;
            FireDelay = 200;
            Damage = 3;
            AngleOffset = 2.5f;
            Animated = false;
            Looping = false;
            ResourceCost = 200;
            MaxHeat = 100 ;
            ShotHeat = 10;            
            MaxHeatTime = 4000;
            CoolValue = 0.25f;
            Range = 500;
            TurretFireType = TurretFireType.FullAuto;

            CurrentAnimation = new Animation()
            {
                TotalFrames = 6
            };

            MaxHealth = 100;      
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (Emitter emitter in EmitterList)
            {
                emitter.Draw(spriteBatch);
            }

            if (Active == true)
            {
                BaseRectangle = new Rectangle((int)Position.X, (int)Position.Y, 
                                              TurretBase.Width, TurretBase.Height);

                BarrelRectangle = new Rectangle((int)Position.X, (int)Position.Y, 
                                                TurretBarrel.Width/CurrentAnimation.TotalFrames, TurretBarrel.Height);

                BarrelPivot = new Vector2(32, 32);
                BasePivot = new Vector2(40, 5);

                if (AmmoBelt != null)
                {
                    AmmoBelt.Draw(spriteBatch);
                }

                spriteBatch.Draw(TurretBarrel, BarrelRectangle, SourceRectangle, Color, Rotation, BarrelPivot, SpriteEffects.None, 0.89f);

                spriteBatch.Draw(TurretBase, BaseRectangle, null, Color, 0, BasePivot, SpriteEffects.None, 0.90f);
            }
        }
    }
}
