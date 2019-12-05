using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Soldier : Invader
    {
        public Soldier(Vector2 position, Vector2? yRange = null)
            : base(position, yRange)
        {
            Speed = 0.65f;
            MaxHP = 20;
            ResourceMinMax = new Vector2(8, 20);
            //YRange = new Vector2(700, 900);

            InvaderType = InvaderType.Soldier;

            InvaderAnimationState = AnimationState_Invader.Walk;
            CurrentMacroBehaviour = MacroBehaviour.AttackTower;
            CurrentMicroBehaviour = MicroBehaviour.MovingForwards;

            MeleeDamageStruct = new InvaderMeleeStruct()
            {
                CurrentAttackDelay = 0,
                MaxAttackDelay = 2000,
                Damage = 10
            };
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            if (Pathfinder != null)
            {
                Pathfinder.Map.Update(gameTime);
                Waypoints = Pathfinder.GetWaypoints();
            }

            switch (CurrentMicroBehaviour)
            {
                #region Stationary
                case MicroBehaviour.Stationary:
                    {
                        Velocity.X = 0;
                        CurrentMicroBehaviour = MicroBehaviour.Attack;
                    }
                    break;
                #endregion

                #region MovingForwards
                case MicroBehaviour.MovingForwards:
                    {
                        Direction.X = -1;

                        if (Slow == true)
                            Velocity.X = Direction.X * SlowSpeed;
                        else
                            Velocity.X = Direction.X * Speed;
                    }
                    break;
                #endregion

                #region MovingBackwards
                case MicroBehaviour.MovingBackwards:
                    {
                        Direction.X = 1;

                        if (Slow == true)
                            Velocity.X = Direction.X * SlowSpeed;
                        else
                            Velocity.X = Direction.X * Speed;
                    }
                    break;
                #endregion

                #region Attack
                case MicroBehaviour.Attack:
                    {
                        switch (CurrentMacroBehaviour)
                        {
                            #region Attack Tower
                            case MacroBehaviour.AttackTower:
                                {
                                    UpdateMeleeDelay(gameTime);
                                }
                                break; 
                            #endregion

                            #region Attack Traps
                            case MacroBehaviour.AttackTraps:
                                {

                                }
                                break;
                            #endregion
                        }
                    }
                    break;
                #endregion
            }

            switch (CurrentMacroBehaviour)
            {
                case MacroBehaviour.OperateVehicle:
                    {

                    }
                    break;
            }
            
            base.Update(gameTime, cursorPosition);
        }
    }
}
