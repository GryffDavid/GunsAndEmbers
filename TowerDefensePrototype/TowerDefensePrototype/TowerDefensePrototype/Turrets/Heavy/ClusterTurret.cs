﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class ClusterTurret : Turret
    {
        private static int _ResourceCost = 600;
        public static new int ResourceCost
        {
            get { return _ResourceCost; }
        }

        public ClusterTurret(Vector2 position)
        {
            Active = true;
            TurretType = TurretType.Cluster;
            Position = position;
            Selected = true;
            FireDelay = 5000;
            Damage = 14;
            Animated = false;
            Looping = false;
            MaxHealth = 500;
            BlastRadius = 220;
            MaxHealth = 200;
            LaunchVelocity = 19;

            TurretFireType = TurretFireType.SemiAuto;

            CurrentAnimation = new TurretAnimation()
            {
                TotalFrames = 6
            };
        }

        public override void Initialize(ContentManager contentManager)
        {
            BarrelPivot = new Vector2(32, 32);
            BasePivot = new Vector2(40, 5);
            base.Initialize(contentManager);
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            BaseRectangle = new Rectangle((int)Position.X, (int)Position.Y,
                                          TurretBase.Width, TurretBase.Height);

            BarrelRectangle = new Rectangle((int)Position.X, (int)Position.Y,
                                            TurretBarrel.Width / CurrentAnimation.TotalFrames, TurretBarrel.Height);

            base.Update(gameTime, cursorPosition);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                spriteBatch.Draw(TurretBarrel, BarrelRectangle, SourceRectangle, Color, Rotation, BarrelPivot, SpriteEffects.None, 0.99f);

                spriteBatch.Draw(TurretBase, BaseRectangle, null, Color, 0, BasePivot, SpriteEffects.None, 1f);
            }
        }  
    }
}
