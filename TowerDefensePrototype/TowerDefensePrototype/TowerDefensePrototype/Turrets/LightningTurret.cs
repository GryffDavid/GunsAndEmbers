using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class LightningTurret : Turret
    {
        public LightningTurret(Vector2 position)
        {
            Active = true;
            TurretType = TurretType.Lightning;
            Position = position;
            Selected = true;
            //FireDelay = 6000;
            FireDelay = 200;
            Damage = 150;
            AngleOffset = 2;
            Animated = false;
            Looping = false;

            CurrentAnimation = new Animation()
            {
                TotalFrames = 0
            };            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                BaseRectangle = new Rectangle((int)Position.X+20, (int)Position.Y+6, TurretBase.Width, TurretBase.Height);
                BarrelRectangle = new Rectangle((int)Position.X+20, (int)Position.Y+6, TurretBarrel.Width, TurretBarrel.Height);

                BarrelPivot = new Vector2(45, TurretBarrel.Height / 2 - 8);
                BasePivot = new Vector2(TurretBase.Width / 2 + 10, TurretBase.Height / 2 - 20);

                spriteBatch.Draw(TurretBase, BaseRectangle, null, Color, 0, BasePivot, SpriteEffects.None, 1f);

                spriteBatch.Draw(TurretBarrel, BarrelRectangle, SourceRectangle, Color, Rotation, BarrelPivot, SpriteEffects.None, 0.99f);                
            }
        }
    }
}
