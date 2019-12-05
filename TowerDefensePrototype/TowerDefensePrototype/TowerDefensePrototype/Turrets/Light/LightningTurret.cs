﻿using System;
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

            CurrentAnimation = new InvaderAnimation()
            {
                TotalFrames = 0
            };

            Charges = 6;
        }

        public override void Initialize(ContentManager contentManager)
        {
            BarrelPivot = new Vector2(45, TurretBarrel.Height / 2 - 8);
            BasePivot = new Vector2(TurretBase.Width / 2 + 10, TurretBase.Height / 2 - 20);
            base.Initialize(contentManager);
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            BaseRectangle = new Rectangle((int)Position.X, (int)Position.Y,
                                          TurretBase.Width, TurretBase.Height);

            //Got a divide by zero error here
            BarrelRectangle = new Rectangle((int)Position.X, (int)Position.Y,
                                            TurretBarrel.Width, TurretBarrel.Height);

            base.Update(gameTime, cursorPosition);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                spriteBatch.Draw(TurretBase, BaseRectangle, null, Color, 0, BasePivot, SpriteEffects.None, 1f);

                spriteBatch.Draw(TurretBarrel, BarrelRectangle, SourceRectangle, Color, Rotation, BarrelPivot, SpriteEffects.None, 0.99f);                
            }
        }
    }
}
