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
            AngleOffset = 0;
            Animated = false;
            Looping = false;

            CurrentAnimation = new Animation()
            {
                AssetName = "Turrets/MachineTurretAnimation4",
                TotalFrames = 6
            };            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                BaseRectangle = new Rectangle((int)Position.X+20, (int)Position.Y+6, TurretBase.Width, TurretBase.Height);
                BarrelRectangle = new Rectangle((int)Position.X+20, (int)Position.Y+6, TurretBarrel.Width/CurrentAnimation.TotalFrames, TurretBarrel.Height);

                BarrelPivot = new Vector2(20, TurretBarrel.Height / 2);
                BasePivot = new Vector2(TurretBase.Width / 2, TurretBase.Height / 2-10);

                spriteBatch.Draw(TurretBarrel, BarrelRectangle, SourceRectangle, Color, Rotation, BarrelPivot, SpriteEffects.None, 0.99f);

                spriteBatch.Draw(TurretBase, BaseRectangle, null, Color, 0, BasePivot, SpriteEffects.None, 1f);
            }
        }
    }
}
