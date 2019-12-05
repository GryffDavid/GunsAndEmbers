using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{    
    class GatlingTurret : Turret
    {      
        public GatlingTurret(Vector2 position)
        {
            Active = true;
            TurretType = TurretType.MachineGun;
            BaseAsset = "Turrets/MachineTurretBase";
            Position = position;
            Selected = true;
            FireDelay = 200;
            Damage = 5;
            AngleOffset = 5;
            Animated = false;
            Looping = false;
            ResourceCost = 200;

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
                BaseRectangle = new Rectangle((int)Position.X, (int)Position.Y, TurretBase.Width, TurretBase.Height);
                BarrelRectangle = new Rectangle((int)Position.X, (int)Position.Y, TurretBarrel.Width/CurrentAnimation.TotalFrames, TurretBarrel.Height);

                BarrelPivot = new Vector2(32, 32);
                BasePivot = new Vector2(40, 5);

                spriteBatch.Draw(TurretBarrel, BarrelRectangle, SourceRectangle, Color, Rotation, BarrelPivot, SpriteEffects.None, 0.69f);

                spriteBatch.Draw(TurretBase, BaseRectangle, null, Color, 0, BasePivot, SpriteEffects.None, 0.7f);
            }
        }
    }
}
