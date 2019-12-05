using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class StationaryCannon : HeavyRangedInvader
    {
        //AnimatedSprite Barrel;

        public StationaryCannon(Vector2 position)
        {
            Active = true;
            Direction = new Vector2(-1f, 0);
            Speed = 1.5f;
            Position = position;
            CurrentHP = 600;
            MaxHP = 600;
            ResourceMinMax = new Vector2(8, 20);
            CurrentAttackDelay = 0;
            AttackDelay = 3500;
            TowerAttackPower = 24;
            TrapAttackPower = 6;
            CurrentFrame = 0;
            InvaderType = InvaderType.StationaryCannon;
            YRange = new Vector2(700, 900);
            DistanceRange = new Vector2(800, 950);
            CurrentAngle = 0;
            NextAngle = 0;
            FinalAngle = 45;
            Airborne = false;

            InvaderState = InvaderState.Stand;
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            //Rotation of the barrel should start at 0 and rotate up slowly to it's decided position
            //after it's stopped moving forward towards the tower
            //This gives the player time to anticipate what it's about to do and counteract it, if possible.




            //If the invader gets into range, start moving the weapon to it's next position
            //if (InRange == true)
            //{
            //    NextAngle = MathHelper.ToRadians(FinalAngle);
            //}

            //BasePivot = new Vector2(Position.X + TextureList[0].Width / 2 + 10, Position.Y + 5);
            //CurrentAngle = MathHelper.SmoothStep(CurrentAngle, NextAngle, 0.1f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60.0f));

            base.Update(gameTime, cursorPosition);
        }

        public override void TrapDamage(Trap trap)
        {
            if (VulnerableToTrap == true)
            {
                switch (trap.TrapType)
                {
                    default:
                        CurrentHP -= trap.NormalDamage;

                        if (trap.InvaderDOT != null)
                            DamageOverTime(trap.InvaderDOT, trap.InvaderDOT.Color);

                        if (trap.InvaderFreeze != null)
                            Freeze(trap.InvaderFreeze, trap.InvaderDOT.Color);

                        if (trap.InvaderSlow != null)
                            MakeSlow(trap.InvaderSlow);
                        break;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(BarrelAnimation.Texture, BarrelDestinationRectangle, BarrelAnimation.DiffuseSourceRectangle,
                             Color, CurrentAngle, BarrelPivot, SpriteEffects.None, DrawDepth-0.001f);

            base.Draw(spriteBatch);
        }
    }
}
