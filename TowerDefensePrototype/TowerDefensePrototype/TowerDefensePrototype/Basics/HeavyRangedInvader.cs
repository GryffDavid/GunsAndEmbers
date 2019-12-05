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
        public Animation BarrelAnimation;
        public Rectangle BarrelDestinationRectangle, BarrelSourceRectangle;
        public int RangedAttackPower;
        public Vector2 AngleRange, DistanceRange, PowerRange, BarrelPivot, BasePivot;
        public float MinDistance, CurrentAngle, NextAngle;
        public float BarrelCurrentFrameDelay, BarrelCurrentFrame;
        public bool InRange = false;
        public bool BarrelAnimated = false;

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            if (BarrelAnimation != null)
            {
                if (BarrelAnimated == true)
                {
                    BarrelCurrentFrameDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (BarrelCurrentFrameDelay > BarrelAnimation.FrameDelay && BarrelAnimation.TotalFrames > 1)
                    {
                        BarrelCurrentFrame++;

                        if (BarrelCurrentFrame >= BarrelAnimation.TotalFrames)
                        {
                            BarrelCurrentFrame = 0;
                            BarrelAnimated = false;
                        }

                        BarrelCurrentFrameDelay = 0;
                    }

                }

               

                BarrelSourceRectangle = new Rectangle(
                    (int)(BarrelCurrentFrame * (BarrelAnimation.Texture.Width / BarrelAnimation.TotalFrames)), 
                    0, 
                    (int)(BarrelAnimation.Texture.Width / BarrelAnimation.TotalFrames), 
                    (int)BarrelAnimation.Texture.Height);

                BarrelDestinationRectangle = new Rectangle((int)BasePivot.X, (int)BasePivot.Y,
                                                           (int)((BarrelAnimation.Texture.Width / BarrelAnimation.TotalFrames) * Scale.X), 
                                                           (int)(BarrelAnimation.Texture.Height * Scale.Y));
            }

            base.Update(gameTime, cursorPosition);
        }

        public override void TrapDamage(Trap trap)
        {
            throw new NotImplementedException();
        }
    }
}
