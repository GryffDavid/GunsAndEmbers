using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace TowerDefensePrototype
{
    class GrenadeTurret : Turret
    {
        public GrenadeTurret(Vector2 position)
        {
            Active = true;
            TurretType = TurretType.Grenade;
            Position = position;
            Selected = true;
            FireDelay = 500;
            Damage = 100;
            Animated = false;
            Looping = false;
            MaxHeat = 50;
            MaxHeatTime = 2000;
            CoolValue = 0.15f;
            ShotHeat = 10;

            CurrentAnimation = new Animation()
            {
                TotalFrames = 6
            };   
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (Emitter emitter in EmitterList)
            {
                emitter.Draw(spriteBatch);
            }

            if (Active == true)
            {
                BaseRectangle = new Rectangle((int)Position.X, (int)Position.Y, TurretBase.Width, TurretBase.Height);
                BarrelRectangle = new Rectangle((int)Position.X, (int)Position.Y, TurretBarrel.Width / CurrentAnimation.TotalFrames, TurretBarrel.Height);

                BarrelPivot = new Vector2(32, 32);
                BasePivot = new Vector2(40, 5);

                spriteBatch.Draw(TurretBarrel, BarrelRectangle, SourceRectangle, Color, Rotation, BarrelPivot, SpriteEffects.None, 0.99f);

                spriteBatch.Draw(TurretBase, BaseRectangle, null, Color, 0, BasePivot, SpriteEffects.None, 1f);
            }

            base.Draw(spriteBatch);
        }  
    }
}
