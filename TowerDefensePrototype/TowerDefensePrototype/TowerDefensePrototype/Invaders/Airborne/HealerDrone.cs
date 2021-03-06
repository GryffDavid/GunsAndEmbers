﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class HealDrone : LightRangedInvader
    {
        public enum SpecificBehaviour { Heal, GetTarget, Circle };
        public SpecificBehaviour HealerBehaviour;
        public override float OriginalSpeed { get { return 1.5f; } }

        public Vector2 HealHeightRange = new Vector2(400, 500);
        public float CurrentHeight;
        public bool IsHealing = false;

        public List<LightningBolt> BoltList = new List<LightningBolt>();

        //MEDBOT
        //Flies above the other invaders and heals them when necessary.
        //Should check which invader is the most urgently in need of attention but not too close to the tower
        //Needs to then fly over to that invader to get in range and begin to heal it
        //The healing beam can be interrupted by a damage threshold - i.e. taking more than 10 damage in a single burst interrupts the heal
        //and it then needs to charge back up again
        //A red sine-wave shaped beam would be pretty cool. Just saying.

        //When shot down, this and other airborne invaders should have an explosion followed by smoke and then
        //falling to the ground, but maintaining the same X velocity
        public HealDrone(Vector2 position, Vector2? yRange = null) : base(position, yRange)
        {
            CurrentHeight = position.Y;   
            MaxHP = 30;
            ResourceMinMax = new Vector2(1, 5);
            InvaderType = InvaderType.HealDrone;
            YRange = new Vector2(100, 350);
            Airborne = true;
            InAir = true;
            InvaderAnimationState = AnimationState_Invader.Walk;
            HealerBehaviour = SpecificBehaviour.GetTarget;

            AngleRange = new Vector2(170, 190);
            RangedDamage = 10;

            RangedAttackTiming = new RangedAttackTiming()
            {
                MaxFireDelay = 250,
                CurrentFireDelay = 0
            };


            //MaxFireDelay = 250;
            //CurrentFireDelay = 0;
            TowerDistanceRange = new Vector2(600, 800);
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            foreach (LightningBolt bolt in BoltList)
            {
                bolt.Update(gameTime);
            }
            BoltList.RemoveAll(Bolt => Bolt.Alpha <= 0);

            switch (HealerBehaviour)
            {
                case SpecificBehaviour.Circle:
                    {

                    }
                    break;

                case SpecificBehaviour.GetTarget:
                    {

                    }
                    break;

                case SpecificBehaviour.Heal:
                    {

                    }
                    break;
            }


            if (TargettingInvader == false)
            {
                List<Invader> InRangeAndHurt = InvaderList.FindAll(Invader =>
                                    Invader.Position.X > Tower.DestinationRectangle.Right + 350 &&
                                    Invader.CurrentHP < Invader.MaxHP &&
                                    Invader.InvaderType != InvaderType.HealDrone &&
                                    Invader.InvaderType != InvaderType.StationaryCannon &&
                                    Invader.IsBeingHealed == false &&
                                    Invader.IsTargeted == false);

                #region There are invaders in range that need healing
                if (InRangeAndHurt.Count > 0)
                {
                    InRangeAndHurt = InRangeAndHurt.OrderByDescending(Invader => Invader.Position.X).ToList();

                    TargetInvader = InRangeAndHurt[0];
                    TargetInvader.CurrentHealDelay = 0;
                    TargetInvader.HealDelay = 500;
                    TargetInvader.IsTargeted = true;
                    IsHealing = true;
                    TargettingInvader = true;
                    CurrentHeight = TargetInvader.BoundingBox.Min.Y - 150;
                    DrawDepth = TargetInvader.DrawDepth;
                }
                #endregion
                else
                #region Idle behaviour
                {                    
                    if (BoundingBox.Min.Y < CurrentHeight - 30)
                    {
                        Velocity.Y = Game1.LerpTime(Velocity.Y, 1 * Speed, 0.1f, gameTime);
                    }

                    if (BoundingBox.Min.Y > CurrentHeight + 30)
                    {
                        Velocity.Y = Game1.LerpTime(Velocity.Y, -1 * Speed, 0.1f, gameTime);
                    }
                }
                #endregion
            }

            if (TargettingInvader == true)
            {
                #region The target got too close to the tower
                if (TargetInvader.Position.X < Tower.DestinationRectangle.Right + 350)
                {
                    TargettingInvader = false;
                    IsHealing = false;
                    TargetInvader.IsBeingHealed = false;
                    TargetInvader.IsTargeted = false;

                    CurrentHeight = (float)Game1.RandomDouble(YRange.X, YRange.Y);
                    Velocity.X = 0;
                }
                #endregion

                #region The drone isn't yet close enough to start healing
                if (Math.Abs(Center.X - TargetInvader.Center.X) > 100)
                {
                    Direction = TargetInvader.Center - Center;
                    Direction.Normalize();
                    Velocity.X = Game1.LerpTime(Velocity.X, Direction.X * 5f, 0.05f, gameTime);
                    Speed = 2.5f;
                }
                #endregion

                #region Orient the drone correctly
                if (TargetInvader != null)
                {
                    if (TargetInvader.Center.X > Center.X)
                    {
                        Orientation = SpriteEffects.FlipHorizontally;
                    }

                    if (TargetInvader.Center.X < Center.X)
                    {
                        Orientation = SpriteEffects.None;
                    }
                }
                #endregion

                #region The drone is close enough to the target to start healing
                if (Vector2.Distance(TargetInvader.Center, Center) < 200 &&
                    TargetInvader.CurrentHP < TargetInvader.MaxHP)
                {
                    TargetInvader.IsBeingHealed = true;

                    #region Actually heal the target
                    if (TargetInvader.CurrentHealDelay >= TargetInvader.HealDelay)
                    {
                        TargetInvader.HealDamage(1);
                        TargetInvader.CurrentHealDelay = 0;
                    }
                    #endregion

                    #region Make sure there's a beam
                    if (BoltList.Count < 5)
                    {
                        LightningBolt Lightning = new LightningBolt(this, TargetInvader, new Color(0, 150, 30, 255), 0.02f, 75f, true);
                        BoltList.Add(Lightning);
                    }
                    else
                    {
                        foreach (LightningBolt bolt in BoltList)
                        {
                            bolt.Source = Center + new Vector2(0, 8);
                            bolt.Destination = TargetInvader.Center;
                        }
                    }
                    #endregion
                }
                #endregion

                #region The targetted invader died
                if (TargetInvader == null || TargetInvader.Active == false)
                {
                    TargettingInvader = false;
                    IsHealing = false;

                    CurrentHeight = (float)Game1.RandomDouble(YRange.X, YRange.Y);
                    Velocity.X = 0;
                }
                #endregion

                #region If the target reaches full health, disengage healing
                if (TargetInvader.CurrentHP >= TargetInvader.MaxHP)
                {
                    TargetInvader.IsBeingHealed = false;
                    TargetInvader.IsTargeted = false;
                    TargettingInvader = false;
                    IsHealing = false;

                    CurrentHeight = (float)Game1.RandomDouble(YRange.X, YRange.Y);
                    Velocity.X = 0;
                }
                #endregion

                //THE DRONE MOVEMENT NEEDS TO BE A BIT RANDOM SO THAT TWO DRONES AREN'T MOVING EXACTLY THE SAME
                //AT THE SAME TIME. MAYBE JUST ADD OR SUBTRACT A TINY RANDOM NUMBER FROM VELOCITIES
                if (BoundingBox.Min.Y < CurrentHeight - 30)
                {
                    Velocity.Y = Game1.LerpTime(Velocity.Y, 1 * Speed, 0.1f, gameTime);
                }

                if (BoundingBox.Min.Y > CurrentHeight + 30)
                {
                    Velocity.Y = Game1.LerpTime(Velocity.Y, -1 * Speed, 0.1f, gameTime);
                }
            }
                        
            base.Update(gameTime, cursorPosition);
        }

        public override void Draw(GraphicsDevice graphics, BasicEffect effect, Effect shadowEffect)
        {
            Matrix world = effect.World;
            //float rot = 0;

            //if (Velocity.X > 0)
            //{
            //    rot = (float)Math.Atan2(-Velocity.Y / 4, Velocity.X);
            //    rot = MathHelper.Clamp(rot, 0, 45);
            //    float thing = MathHelper.ToDegrees(rot);
            //}

            //if (Velocity.X < 0)
            //{
            //    rot = (float)Math.Atan2(-Velocity.Y / 4, -Velocity.X);
            //    rot = MathHelper.Clamp(rot, -45, 0);
            //    //float thing = MathHelper.ToDegrees(rot);
            //}

            //if (Velocity.X == 0)
            //{
            //    rot = 0;
            //}



            //effect.World =
            //    Matrix.CreateTranslation(new Vector3(-Position.X - 37, -Position.Y - 27, 0)) *
            //    Matrix.CreateRotationZ(rot) *
            //    Matrix.CreateTranslation(new Vector3(Position.X + 37, Position.Y + 27, 0));

            base.Draw(graphics, effect);
            effect.World = world;
        }
    }
}
