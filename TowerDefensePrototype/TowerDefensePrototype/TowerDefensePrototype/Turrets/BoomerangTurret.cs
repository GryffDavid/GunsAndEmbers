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
    class BoomerangTurret : Turret
    {
        public BoomerangTurret(Vector2 position)
        {
            Active = true;
            TurretType = TurretType.Boomerang;
            Position = position;
            Selected = true;
            FireDelay = 5000;
            Damage = 200;
            Animated = false;
            Looping = false;
            Health = 500;
            ResourceCost = 600;

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
