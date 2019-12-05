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
        public override float OriginalSpeed { get { return 0.65f; } }

        public BatteringRam(Vector2 position, Vector2? yRange = null) 
            : base(position, yRange)
        {
            MaxHP = 20;
            ResourceMinMax = new Vector2(8, 20);

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
            //CurrentOperators = OperatorList.Count;

            //Colliding with a trap or the tower. Stop moving
            if ((TrapCollision == true || TowerCollision == true) &&
                CurrentMicroBehaviour == MicroBehaviour.MovingForwards)
            {
                CurrentMicroBehaviour = MicroBehaviour.Stationary;
            }

            //One of the operators died. Stop moving
            if (OperatorList.Any(Invader => Invader.Active == false || Invader == null || Invader.CurrentHP == 0))
            {
                CurrentMicroBehaviour = MicroBehaviour.Stationary;
                CurrentMacroBehaviour = MacroBehaviour.AttackTower;

                OperatorList.RemoveAll(Invader => Invader.Active == false || Invader == null);
            }
            
            #region Choose soldiers to operate the ram
            int MissingOperators = NeededOperators - OperatorList.Count;

            if (OperatorList.Count < NeededOperators)
            {
                //Choose soldiers to operate the ram
                List<Invader> closeInvaders = InvaderList.OrderByDescending(Invader => Vector2.Distance(Invader.Position, Position)).ToList();
                closeInvaders.RemoveAll(Invader => Invader.InvaderType != InvaderType.Soldier ||
                                                   (Invader.InvaderType == InvaderType.Soldier && Invader.OperatingVehicle != null));

                if (closeInvaders.Count >= MissingOperators)
                {
                    closeInvaders = closeInvaders.GetRange(0, MissingOperators).ToList();
                    OperatorList.AddRange(closeInvaders);
                }

                foreach (Invader pilot in OperatorList.Where(Invader => Invader.OperatingVehicle == null))
                {
                    pilot.SetOperatingVehicle(this);

                    pilot.CurrentMicroBehaviour = MicroBehaviour.FollowWaypoints;
                    pilot.Velocity = Vector2.Zero;
                    pilot.CurrentBehaviourDelay = 0;

                    Pathfinder pathfinder = new Pathfinder(TrapList, InvaderList,
                        new Vector2(pilot.Position.X, pilot.MaxY),
                        new Vector2(Position.X + DestinationRectangle.Width / 2,
                                    MaxY + 8 - (24 * OperatorList.IndexOf(pilot))));

                    pilot.Pathfinder = pathfinder;
                }
            }
            #endregion

            switch (CurrentMacroBehaviour)
            {
                #region IsVehicle
                case MacroBehaviour.IsOperated:
                    {
                        switch (CurrentMicroBehaviour)
                        {
                            #region Stationary
                            case MicroBehaviour.Stationary:
                                {
                                    Velocity.X = 0;

                                    if (TargetTrap != null)
                                    {
                                        CurrentMacroBehaviour = MacroBehaviour.AttackTraps;
                                        CurrentMicroBehaviour = MicroBehaviour.Attack;
                                    }

                                    if (TowerCollision == true)
                                    {
                                        CurrentMacroBehaviour = MacroBehaviour.AttackTower;
                                        CurrentMicroBehaviour = MicroBehaviour.Attack;
                                    }
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

                                    //Vehicle can move again. Make sure the operators move with it.
                                    if (OperatorList.Any(Invader => Invader.CurrentMicroBehaviour == MicroBehaviour.Stationary))
                                    {
                                        OperatorList.ForEach(Invader =>
                                            { 
                                                if (Invader.CurrentMicroBehaviour != MicroBehaviour.MovingForwards)
                                                    Invader.CurrentMicroBehaviour = MicroBehaviour.MovingForwards; 
                                            });
                                    }
                                }
                                break; 
                            #endregion
                        }
                    }
                    break; 
                #endregion

                case MacroBehaviour.AttackTower:
                case MacroBehaviour.AttackTraps:
                    {
                        switch (CurrentMicroBehaviour)
                        {
                            #region Stationary
                            case MicroBehaviour.Stationary:
                                {
                                    Velocity.X = 0;

                                    //Invaders have arrived to start operating this vehicle
                                    if (NeededOperators == OperatorList.Count &&
                                        OperatorList.All(Invader =>
                                        Invader.Velocity == Vector2.Zero &&
                                        Vector2.Distance(Invader.Center, this.Center) < 32))
                                    {
                                        CurrentMacroBehaviour = MacroBehaviour.IsOperated;
                                        CurrentMicroBehaviour = MicroBehaviour.MovingForwards;
                                        OperatorList.ForEach(Invader =>
                                        {
                                            Invader.CurrentMicroBehaviour = MicroBehaviour.MovingForwards;
                                            Invader.Speed = Speed;
                                        });
                                    }
                                }
                                break; 
                            #endregion

                            #region Attack
                            case MicroBehaviour.Attack:
                                {
                                    if (OperatorList.Count == NeededOperators)
                                    {
                                        //Target trap was destroyed. Start moving forwards again
                                        if (TargetTrap == null && TrapCollision == false && TowerCollision == false)
                                        {
                                            CurrentMicroBehaviour = MicroBehaviour.MovingForwards;
                                            CurrentMacroBehaviour = MacroBehaviour.IsOperated;

                                            OperatorList.ForEach(Invader =>
                                            {
                                                Invader.CurrentMicroBehaviour = MicroBehaviour.MovingForwards;
                                            });
                                        }

                                        UpdateMeleeDelay(gameTime);
                                    }
                                    else
                                    {
                                        CurrentMicroBehaviour = MicroBehaviour.Stationary;
                                    }
                                }
                                break;
                            #endregion
                        }                        
                    }
                    break;                    
            }

            base.Update(gameTime, cursorPosition);
        }
    }
}
