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
    class CannonTurret : Turret
    {
        public CannonTurret(Vector2 position)
        {
            Active = true;
            DamageType = DamageType.Concussive;
            TurretType = TurretType.Cannon;
            Position = position;
            Selected = true;
            FireDelay = 8000;            
            Damage = 14;
            Animated = false;
            Looping = false;
            MaxHealth = 500;
            ResourceCost = 600;
            BlastRadius = 270;
            MaxHealth = 200;
            LaunchVelocity = 12;

            TurretFireType = TurretFireType.Single;
            
            CurrentAnimation = new Animation()
            {
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
