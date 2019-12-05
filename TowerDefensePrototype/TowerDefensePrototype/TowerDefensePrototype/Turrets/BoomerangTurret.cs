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
            BaseAsset = "Turrets/MachineTurretBase";
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
                AssetName = "Turrets/MachineTurretBarrel",
                TotalFrames = 6
            };       
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                BaseRectangle = new Rectangle((int)Position.X + 20, (int)Position.Y + 8, 
                    TurretBase.Width, TurretBase.Height);

                BarrelRectangle = new Rectangle((int)Position.X + 20, (int)Position.Y + 8, 
                    TurretBarrel.Width / CurrentAnimation.TotalFrames, TurretBarrel.Height);

                BarrelPivot = new Vector2(45, TurretBarrel.Height / 2);
                BasePivot = new Vector2(TurretBase.Width / 2, TurretBase.Height / 2 - 10);

                spriteBatch.Draw(TurretBarrel, BarrelRectangle, SourceRectangle, Color, Rotation, BarrelPivot, SpriteEffects.None, 0.89f);

                spriteBatch.Draw(TurretBase, BaseRectangle, null, Color, 0, BasePivot, SpriteEffects.None, 0.90f);
            }
        }        
    }
}
