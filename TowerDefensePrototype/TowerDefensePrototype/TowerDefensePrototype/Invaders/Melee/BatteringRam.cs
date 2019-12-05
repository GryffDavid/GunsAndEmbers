using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class BatteringRam : Invader
    {
        public BatteringRam(Vector2 position, Vector2? yRange = null) 
            : base(position, yRange)
        {
            Speed = 0.65f;
            MaxHP = 20;
            ResourceMinMax = new Vector2(8, 20);
            //YRange = new Vector2(700, 900);

            InvaderType = InvaderType.BatteringRam;

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
            switch (CurrentMicroBehaviour)
            {
                #region Moving Forwards
                case MicroBehaviour.MovingForwards:
                    {
                        Direction.X = -1;

                        if (CurrentOperators == NeededOperators &&
                            OperatorList.All(Invader => Vector2.Distance(Invader.Position, this.Position) < 48))
                        {
                            if (Slow == true)
                                Velocity.X = Direction.X * SlowSpeed;
                            else
                                Velocity.X = Direction.X * Speed;
                        }
                        else
                        {
                            CurrentMicroBehaviour = MicroBehaviour.Stationary;
                        }
                    }
                    break;
                #endregion

                #region Stationary
                case MicroBehaviour.Stationary:
                    {
                        Velocity.X = 0;

                        if (CurrentOperators == NeededOperators)
                        {
                            CurrentMicroBehaviour = MicroBehaviour.Attack;
                        }
                    }
                    break;
                #endregion

                #region Attack
                case MicroBehaviour.Attack:
                    {
                        int p = 0;
                    }
                    break;
                #endregion
            }

            base.Update(gameTime, cursorPosition);
        }
    }
}
