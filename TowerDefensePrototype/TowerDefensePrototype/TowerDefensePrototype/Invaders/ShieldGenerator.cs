using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class ShieldGenerator : HeavyRangedInvader
    {
        //public Shield Shield;// = new Shield();
        public UIBar ShieldBar;
        public float ShieldRadius = 80f;
        public List<LightningBolt> BoltList = new List<LightningBolt>();

        public ShieldGenerator(Vector2 position, Vector2? yRange = null)
            : base(position, yRange)
        {
            Speed = 0.65f;
            Position = position;
            MaxHP = 20;
            ResourceMinMax = new Vector2(8, 20);
            YRange = new Vector2(700, 900);

            InvaderType = InvaderType.ShieldGenerator;

            InvaderAnimationState = AnimationState_Invader.Walk;
            CurrentMacroBehaviour = MacroBehaviour.AttackTower;
            CurrentMicroBehaviour = MicroBehaviour.MovingForwards;

            TowerDistanceRange = new Vector2(750, 850);

            //MeleeDamageStruct = new InvaderMeleeStruct()
            //{
            //    CurrentAttackDelay = 0,
            //    MaxAttackDelay = 2000,
            //    Damage = 10
            //};
            Shield = new Shield(ShieldRadius) { Active = true, MaxShield = 100, Position = Position, ShieldOn = false, ShieldTime = 500, RechargeRate = 0.5f };

            ShieldBar = new UIBar(new Vector2(Position.X, Position.Y + 32), new Vector2(32, 4), Color.DarkRed);
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            Shield.Update(gameTime, Center);

            ShieldBar.Update(Shield.MaxShield, Shield.CurrentShield, gameTime, new Vector2(Center.X, Position.Y + CurrentAnimation.FrameSize.Y + 4));

            if (CurrentBehaviourDelay > MaxBehaviourDelay)
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
                        #region Move
                        Direction.X = -1;
                        CanAttack = false;

                        if (Slow == true)
                            Velocity.X = Direction.X * SlowSpeed;
                        else
                            Velocity.X = Direction.X * Speed;
                        #endregion

                        if (DistToTower <= MinTowerRange)
                        {
                            InTowerRange = true;
                            //Shield.ShieldOn = true;
                            Shield.Active = true;
                            CurrentMicroBehaviour = MicroBehaviour.Stationary;
                        }
                    }
                    break; 
                #endregion

                #region MovingBackwards
                case MicroBehaviour.MovingBackwards:
                    {

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

            foreach (LightningBolt bolt in BoltList)
            {
                bolt.Update(gameTime);
            }

            BoltList.RemoveAll(Bolt => Bolt.Alpha <= 0);

            base.Update(gameTime, cursorPosition);
        }
    }
}
