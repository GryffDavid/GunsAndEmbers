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
        public Game1 Game1;
        enum SpecificBehaviour { Approach, OpenDoors, DropOff, Retreat};
        SpecificBehaviour DropShipBehaviour;

        public override float OriginalSpeed { get { return 1.8f; } }

        public Texture2D DoorTexture, VapourTexture, RopeTexture;
        public DropShipDoor LeftDoor, RightDoor;

        Vector2 HoverRange = new Vector2(590, 610);
        List<Rope> RopeList = new List<Rope>();

        List<Invader> DropInvaders = new List<Invader>();
        float CurrentDropTime, MaxDropTime;
        int CurrentInvader = 0;

        //Drop ship needs to arrive from top right corner, but not move in normally like the other invaders
        //instead it should pull in at an angle with its decent like a helicopter getting ready to land,
        //with the front pitching down and then take off again after invaders have been dropped off
        //with the front pitching back up, taking off and flying away

        //The drop ship getting close to the ground could disrupt smoke and fire effects too. Blowing away from the 
        //center of the dropship
        public DropShip(Vector2 position, Vector2? yRange = null, params object[] invaders)
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

            CurrentDropTime = 700;
            MaxDropTime = 800;

            foreach (object thing in invaders)
            {
                DropInvaders.Add(thing as Invader);
            }
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
                        if (CurrentDropTime <= MaxDropTime)
                        {
                            CurrentDropTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        }
                        

                        if (CurrentDropTime >= MaxDropTime && CurrentInvader < DropInvaders.Count)
                        {
                            Game1.AddInvader(DropInvaders[CurrentInvader], gameTime, new Vector2(LeftDoor.DestinationRectangle.Right, LeftDoor.DestinationRectangle.Top));
                            CurrentInvader++;
                            CurrentDropTime = 0;
                        }

                        //if (DropInvaders.Count > 0)
                        //{
                        //    foreach (Invader invader in DropInvaders)
                        //    {
                        //        Game1.AddInvader(invader, gameTime, new Vector2(LeftDoor.DestinationRectangle.Right, LeftDoor.DestinationRectangle.Top));
                        //    }

                        //    DropInvaders.Clear();
                        //}



                        //if (RopeList.Count == 0)
                        //{
                        //    for (int i = 0; i < 6; i++)
                        //    {
                        //        float maxY = Random.Next(760, 900);
                        //        Rope rope = new Rope(RightDoor.Position - (i * new Vector2(36, 0)), null, maxY);
                        //        rope.DrawDepth = maxY / 1080.0f;
                        //        rope.StickTexture = RopeTexture;
                        //        RopeList.Add(rope);
                        //        rope.Sticks.RemoveAt(rope.Sticks.Count - 1);
                        //        Game1.AddDrawable(rope);
                        //        //DrawableList.Add(rope);
                        //    }
                        //}

                        if (LeftDoor.CurrentState == DropShipDoor.DoorState.Closing &&
                            RightDoor.CurrentState == DropShipDoor.DoorState.Closing)
                        {
                            DropShipBehaviour = SpecificBehaviour.Retreat;

                            //foreach (Rope rope in RopeList)
                            //{
                            //    rope.Sticks.RemoveAt(0);
                            //}
                        }
                        
                    }
                    break;
                #endregion

                #region Retreat
                case SpecificBehaviour.Retreat:
                    {
                        if (Velocity.X > -3f)
                        {
                            Velocity.X -= 0.02f;
                        }

                        //Velocity.X = -3f;

                        if (Velocity.Y > -0.5f)
                        {
                            Velocity.Y -= 0.001f;
                            //Velocity.Y = -0.5f;
                        }
                    }
                    break;
                #endregion
            }

            //foreach (Rope rope in RopeList)
            //{
            //    rope.Update(gameTime);
            //}

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

            //foreach (Rope rope in RopeList)
            //{
            //    rope.Draw(graphics, effect);
            //}

            //effect.World = world;
        }
    }
}
