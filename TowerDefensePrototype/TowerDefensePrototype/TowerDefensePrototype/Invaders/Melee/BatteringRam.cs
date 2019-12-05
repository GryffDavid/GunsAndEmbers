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
            ZDepth = 24;

            InvaderAnimationState = AnimationState_Invader.Stand;
            CurrentMacroBehaviour = MacroBehaviour.AttackTower;
            CurrentMicroBehaviour = MicroBehaviour.Stationary;

            MeleeDamageStruct = new InvaderMeleeStruct()
            {
                CurrentAttackDelay = 0,
                MaxAttackDelay = 2000,
                Damage = 25
            };
        }
        
        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            CurrentOperators = OperatorList.Count;

            switch (CurrentMicroBehaviour)
            {
                #region Moving Forwards
                case MicroBehaviour.MovingForwards:
                    {
                        Direction.X = -1;

                        if (CurrentOperators == NeededOperators &&
                            OperatorList.All(Invader => Vector2.Distance(Invader.Center, this.Center) < 26 && Invader.Velocity == Vector2.Zero && Invader.Waypoints.Count == 0))
                        {
                            OperatorList.ForEach(Invader => { Invader.CurrentMicroBehaviour = MicroBehaviour.MovingForwards; Invader.Speed = Speed; });

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
                            if (TargetTrap != null)
                                CurrentMacroBehaviour = MacroBehaviour.AttackTraps;

                            CurrentMicroBehaviour = MicroBehaviour.Attack;                            
                        }

                        if (CurrentOperators < NeededOperators && CurrentOperators > 0)
                        {
                            OperatorList.ForEach(Invader => Invader.CurrentMicroBehaviour = MicroBehaviour.Stationary);
                        }
                    }
                    break;
                #endregion

                #region Attack
                case MicroBehaviour.Attack:
                    {
                        Velocity.X = 0;

                        UpdateMeleeDelay(gameTime);
                    }
                    break;
                #endregion
            }

            base.Update(gameTime, cursorPosition);
        }
    }
}
