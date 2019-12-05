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
    class FlameThrowerTurret : Turret
    {
        public FlameThrowerTurret(Vector2 position)
        {
            Active = true;
            TurretType = TurretType.FlameThrower;
            Position = position;
            Selected = true;
            FireDelay = 40;
            Damage = 20;
            AngleOffset = 0f;
            Animated = false;
            Looping = false;
            ResourceCost = 200;
            MaxHeat = 150;
            MaxHeatTime = 2000;
            CoolValue = 0.5f;
            ShotHeat = 10;

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
                                                TurretBarrel.Width / CurrentAnimation.TotalFrames, TurretBarrel.Height);

                BarrelPivot = new Vector2(32, 32);
                BasePivot = new Vector2(40, 5);

                spriteBatch.Draw(TurretBarrel, BarrelRectangle, SourceRectangle, Color, Rotation, BarrelPivot, SpriteEffects.None, 0.89f);

                spriteBatch.Draw(TurretBase, BaseRectangle, null, Color, 0, BasePivot, SpriteEffects.None, 0.90f);

                spriteBatch.Draw(TurretBase, new Rectangle((int)(BasePivot.X + Position.X), (int)(BasePivot.Y + Position.Y), 1920, 4), null, Color.White, FireRotation, Vector2.Zero, SpriteEffects.None, 0);
            }
        }
    }
}
