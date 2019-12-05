using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class BeamTurret : Turret
    {
        public static new int ResourceCost = 200;

        public BeamTurret(Vector2 position)
        {
            Active = true;
            TurretType = TurretType.Beam;
            Position = position;
            Selected = true;
            FireDelay = 1000;
            Damage = 100;
            AngleOffset = 0;
            Animated = false;
            Looping = false;

            TurretFireType = TurretFireType.SemiAuto;

            CurrentAnimation = new TurretAnimation()
            {
                TotalFrames = 6
            };

            MaxHealth = 100;
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
                spriteBatch.Draw(TurretBarrel, BarrelRectangle, SourceRectangle, Color, Rotation, BarrelPivot, SpriteEffects.None, 0.89f);
                spriteBatch.Draw(TurretBase, BaseRectangle, null, Color, 0, BasePivot, SpriteEffects.None, 0.90f);
            }
        }
    }
}
