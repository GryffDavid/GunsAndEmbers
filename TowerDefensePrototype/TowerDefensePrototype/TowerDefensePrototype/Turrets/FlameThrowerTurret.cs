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
            TurretAsset = "Turrets/FlameThrowerBarrel";
            BaseAsset = "Turrets/FlameThrowerBase";
            Position = position;
            Selected = true;
            FireDelay = 40;
            Damage = 20;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                BaseRectangle = new Rectangle((int)Position.X+16, (int)Position.Y+6, TurretBase.Width, TurretBase.Height);
                BarrelRectangle = new Rectangle((int)Position.X+18, (int)Position.Y+6, TurretBarrel.Width, TurretBarrel.Height);

                Vector2 BarrelPivot = new Vector2(18, TurretBarrel.Height / 2);
                Vector2 BasePivot = new Vector2(TurretBase.Width / 2-8, TurretBase.Height / 2 - 10);

                spriteBatch.Draw(TurretBarrel, BarrelRectangle, null, Color, Rotation, BarrelPivot, SpriteEffects.None, 1f);

                spriteBatch.Draw(TurretBase, BaseRectangle, null, Color, 0, BasePivot, SpriteEffects.None, 1f);
            }
        }
    }
}
