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
        public Soldier(Vector2 position)
        {
            Speed = 0.68f;
            Position = position;
            MaxHP = 20;
            ResourceMinMax = new Vector2(8, 20);
            YRange = new Vector2(700, 900);

            InvaderType = InvaderType.Soldier;

            InvaderAnimationState = AnimationState_Invader.Walk;
            CurrentMacroBehaviour = MacroBehaviour.AttackTower;
            CurrentMicroBehaviour = MicroBehaviour.MovingForwards;

            MeleeDamageStruct = new InvaderMeleeStruct()
            {
                CurrentAttackDelay = 0,
                MaxAttackDelay = 500,
                Damage = 1
            };
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            switch (CurrentMicroBehaviour)
            {
                #region Stationary
                case MicroBehaviour.Stationary:
                    {
                        Velocity.X = 0;
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

                    }
                    break;
                #endregion
            }
            
            base.Update(gameTime, cursorPosition);
        }
    }
}
