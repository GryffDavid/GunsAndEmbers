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
        private static int _ResourceCost = 600;
        public static new int ResourceCost
        {
            get { return _ResourceCost; }
        }

        public FlameThrowerTurret(Vector2 position)
        {
            Active = true;
            TurretType = TurretType.FlameThrower;
            Position = position;
            Selected = true;
            FireDelay = 40;
            Damage = 20;
            AngleOffset = 0f;
            Animated = false;
            Looping = false;
            MaxHeat = 150;
            MaxHeatTime = 2000;
            CoolValue = 0.5f;
            ShotHeat = 10;

            CurrentAnimation = new TurretAnimation()
            {
                TotalFrames = 6
            };

            MaxHealth = 100;

            //GENERATED EMITTER CODE:
            //Emitter NewEmitterName = new Emitter(TEXTURE_NAME, POSITION, new Vector2(-4f, 4f), new Vector2(9f, 10f), new Vector2(1200f, 1500f), 1f, false, new Vector2(0f, 0f), new Vector2(-2f, 2f), new Vector2(0.1085f, 0.109f), new Color(255, 128, 0, 180), new Color(255, 128, 0, 36), 0.084f, -1f, 51f, 4, false, new Vector2(0f, 720f), true, DRAWDEPTH, false, false, new Vector2(0f, 0f), new Vector2(0f, 0f), 0f, true, new Vector2(0.021f, 0.036f), false, false, 0f, false, false, true, null);
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
