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
            Speed = 1.5f;
            Position = position;
            CurrentHP = 600;
            MaxHP = 600;
            ResourceMinMax = new Vector2(8, 20);
            YRange = new Vector2(700, 900);
            InvaderType = InvaderType.StationaryCannon;            
            InvaderState = InvaderState.Stand;
            CurrentAngle = 45;
            
            RangedDamageStruct = new InvaderRangedStruct()
            {
                AngleRange = new Vector2(170, 190),
                Damage = 10,
                MaxFireDelay = 750,
                CurrentFireDelay = 0,
                DistanceRange = new Vector2(800, 900)
            };
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            //Rotation of the barrel should start at 0 and rotate up slowly to it's decided position
            //after it's stopped moving forward towards the tower
            //This gives the player time to anticipate what it's about to do and counteract it, if possible.

            //THIS INVADER SHOULD BE ABLE TO TELL WHERE THE PROJECTILE LANDS. IF IT HITS A TRAP THEN IT SHOULD ADJUST THE ANGLE
            //IT SHOULD ALSO BE ABLE TO ADAPT TO A WALL BEING PLACED DIRECTLY IN FRONT OF IT. IT SHOULD THEN START TO BACK UP 
            //GIVING ITSELF MORE ROOM TO FIRE AGAIN
            //IT SHOULD NEVER GET WEDGED UP AGAINST A WALL AND FIRE INTO THE WALL DIRECTLY IN FRONT OF IT


            //If the invader gets into range, start moving the weapon to it's next position
            //if (InRange == true)
            //{
            //    NextAngle = MathHelper.ToRadians(FinalAngle);
            //}

            UpdateFireDelay(gameTime);

            BarrelPivot = new Vector2(BarrelAnimation.FrameSize.X/2, BarrelAnimation.FrameSize.Y/2);
            BasePivot = new Vector2(Position.X , Position.Y);

            //BasePivot = new Vector2(Position.X + TextureList[0].Width / 2 + 10, Position.Y + 5);
            //CurrentAngle = MathHelper.SmoothStep(CurrentAngle, NextAngle, 0.1f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
            //CurrentAngle += 0.01f;
            base.Update(gameTime, cursorPosition);
        }

        //public override void Draw(SpriteBatch spriteBatch)
        //{
        //    spriteBatch.Draw(BarrelAnimation.Texture, BarrelDestinationRectangle, BarrelAnimation.DiffuseSourceRectangle,
        //                     Color, CurrentAngle, BarrelPivot, SpriteEffects.None, DrawDepth - 0.001f);

        //    base.Draw(spriteBatch);
        //}
    }
}
