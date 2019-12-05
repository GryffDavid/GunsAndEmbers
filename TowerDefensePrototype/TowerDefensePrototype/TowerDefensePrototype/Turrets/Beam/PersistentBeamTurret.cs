using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{    
    class PersistentBeamTurret : Turret
    {
        private static int _ResourceCost = 128;
        public static new int ResourceCost
        {
            get { return _ResourceCost; }
        }

        public PersistentBeamTurret(Vector2 position)
        {
            Active = true;
            TurretType = TurretType.PersistentBeam;
            Position = position;
            Selected = true;
            //The length of time the player has to keep the beam on an invader to do damage
            FireDelay = 1500;
            Damage = 12;
            AngleOffset = 0f;
            Animated = false;
            Looping = false;
            MaxHeat = 100;
            MaxHeatTime = 10000;
            CoolValue = 0.25f;
            ShotHeat = 1f;

            TurretFireType = TurretFireType.Beam;


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
            foreach (Emitter emitter in EmitterList)
            {
                emitter.Draw(spriteBatch);
            }

            if (Active == true)
            {
                spriteBatch.Draw(TurretBarrel, BarrelRectangle, SourceRectangle, Color, Rotation, BarrelPivot, SpriteEffects.None, 0.89f);

                spriteBatch.Draw(TurretBase, BaseRectangle, null, Color, 0, BasePivot, SpriteEffects.None, 0.90f);
            }
        }
    }
}
