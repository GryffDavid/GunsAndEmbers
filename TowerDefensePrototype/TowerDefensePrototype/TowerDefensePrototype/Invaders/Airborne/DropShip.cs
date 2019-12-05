using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class DropShip : Invader
    {
        enum SpecificBehaviour { Approach, OpenDoors, DropOff, Retreat};
        SpecificBehaviour DropShipBehaviour;

        public override float OriginalSpeed { get { return 1.8f; } }

        public Texture2D DoorTexture, VapourTexture;
        public DropShipDoor LeftDoor, RightDoor;

        Vector2 HoverRange = new Vector2(590, 610);

        //Drop ship needs to arrive from top right corner, but not move in normally like the other invaders
        //instead it should pull in at an angle with its decent like a helicopter getting ready to land,
        //with the front pitching down and then take off again after invaders have been dropped off
        //with the front pitching back up, taking off and flying away

        //The drop ship getting close to the ground could disrupt smoke and fire effects too. Blowing away from the 
        //center of the dropship
        public DropShip(Vector2 position, Vector2? yRange = null)
            : base(position, yRange)
        {
            MaxHP = 400;
            CurrentHP = MaxHP;
            InvaderType = InvaderType.DropShip;
            YRange = new Vector2(-100, -100);
            Airborne = true;
            InAir = true;
            InvaderAnimationState = AnimationState_Invader.Walk;
            DropShipBehaviour = SpecificBehaviour.Approach;            
        }

        public override void Initialize()
        {
            LeftDoor = new DropShipDoor(DoorTexture, this, 
                                        new Vector2(343, 171), 
                                        new Vector2(-DoorTexture.Width, 0), 0.2f);

            RightDoor = new DropShipDoor(DoorTexture, this, 
                                        new Vector2(343 + DoorTexture.Width, 171), 
                                        new Vector2(DoorTexture.Width, 0), 0.2f);
            LeftDoor.Initialize();
            RightDoor.Initialize();
            base.Initialize();
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            LeftDoor.Update(gameTime);
            RightDoor.Update(gameTime);

            switch (DropShipBehaviour)
            {
                #region Approach
                case SpecificBehaviour.Approach:
                    {
                        if (Position.X < 960)
                        {
                            Velocity.X = 0;
                            DropShipBehaviour = SpecificBehaviour.OpenDoors;
                        }

                    }
                    break;
                #endregion

                #region OpenDoors
                case SpecificBehaviour.OpenDoors:
                    {
                        //Velocity.X = 0;
                        if (LeftDoor.CurrentState == DropShipDoor.DoorState.Closed &&
                            RightDoor.CurrentState == DropShipDoor.DoorState.Closed)
                        {
                            LeftDoor.CurrentState = DropShipDoor.DoorState.Opening;
                            RightDoor.CurrentState = DropShipDoor.DoorState.Opening;
                        }

                        if (LeftDoor.CurrentState == DropShipDoor.DoorState.Open &&
                            RightDoor.CurrentState == DropShipDoor.DoorState.Open)
                        {
                            DropShipBehaviour = SpecificBehaviour.DropOff;
                        }
                    }
                    break;
                #endregion

                #region DropOff
                case SpecificBehaviour.DropOff:
                    {
                        if (LeftDoor.CurrentState == DropShipDoor.DoorState.Closing &&
                            RightDoor.CurrentState == DropShipDoor.DoorState.Closing)
                        {
                            DropShipBehaviour = SpecificBehaviour.Retreat;
                        }
                    }
                    break;
                #endregion

                #region Retreat
                case SpecificBehaviour.Retreat:
                    {
                        Velocity.X = -3f;
                        Velocity.Y = -0.5f;
                    }
                    break;
                #endregion
            }

            base.Update(gameTime, cursorPosition);
        }

        public override void Draw(GraphicsDevice graphics, BasicEffect effect, Effect shadowEffect)
        {
            //Matrix world = effect.World;
            //float rot = 0;

            //if (Velocity.X > 0)
            //{
            //    rot = (float)Math.Atan2(Velocity.Y/2, Velocity.X);
            //    //rot = MathHelper.Clamp(rot, 0, 45);
            //    float thing = MathHelper.ToDegrees(rot);
            //}

            //if (Velocity.X < 0)
            //{
            //    rot = (float)Math.Atan2(Velocity.Y/2, -Velocity.X);
            //    //rot = MathHelper.Clamp(rot, -45, 0);
            //    //float thing = MathHelper.ToDegrees(rot);
            //}

            //if (Velocity.X == 0)
            //{
            //    rot = 0;
            //}



            //effect.World =
            //    Matrix.CreateTranslation(new Vector3(-Position.X - 450, -Position.Y - 150, 0)) *
            //    Matrix.CreateRotationZ(rot) *
            //    Matrix.CreateTranslation(new Vector3(Position.X + 450, Position.Y + 150, 0));
            base.Draw(graphics, effect);

            LeftDoor.Draw(graphics, effect);
            RightDoor.Draw(graphics, effect);

            //effect.World = world;
        }
    }
}
