using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    abstract class HeavyRangedInvader : Invader
    {
        public InvaderAnimation BarrelAnimation;
        public Rectangle BarrelDestinationRectangle;
        public int RangedAttackPower;
        public Vector2 AngleRange, DistanceRange, PowerRange, BarrelPivot, BasePivot, BarrelEnd;
        public float MinDistance, CurrentAngle, NextAngle, FinalAngle;
        public bool InRange = false;

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            if (BarrelAnimation != null)
            {
                BarrelAnimation.Update(gameTime);

                BarrelDestinationRectangle = new Rectangle((int)BasePivot.X, (int)BasePivot.Y,
                                                           (int)((BarrelAnimation.FrameSize.X) * Scale.X),
                                                           (int)(BarrelAnimation.FrameSize.Y * Scale.Y));
            }

            Vector2 BarrelCenter = new Vector2(BarrelDestinationRectangle.X + (float)Math.Cos(CurrentAngle - 90) * (BarrelPivot.Y - BarrelDestinationRectangle.Height / 2),
                                               BarrelDestinationRectangle.Y + (float)Math.Sin(CurrentAngle - 90) * (BarrelPivot.Y - BarrelDestinationRectangle.Height / 2));

            BarrelEnd = new Vector2(BarrelCenter.X - (float)Math.Cos(CurrentAngle) * (BarrelPivot.X),
                                    BarrelCenter.Y - (float)Math.Sin(CurrentAngle) * (BarrelPivot.X));

            base.Update(gameTime, cursorPosition);
        }

        public override void TrapDamage(Trap trap)
        {
            throw new NotImplementedException();
        }
    }
}
