using System;
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
        public Vector2 HealHeightRange = new Vector2(400, 500);
        public float CurrentHeight;
        public bool IsHealing = false;
        public bool HasTarget = false;
        public Invader HealTarget;
        public LightningBolt Bolt = new LightningBolt(Vector2.One, Vector2.Zero, Color.Transparent, 1f);
        public List<LightningBolt> BoltList = new List<LightningBolt>();

        //MEDBOT
        //Flies above the other invaders and heals them when necessary.
        //Should check which invader is the most urgently in need of attention but not too close to the tower
        //Needs to then fly over to that invader to get in range and begin to heal it
        //The healing beam can be interrupted by a damage threshold - i.e. taking more than 10 damage in a single burst interrupts the heal
        //and it then needs to charge back up again
        //A red sine-wave shaped beam would be pretty cool. Just saying.
        public HealDrone(Vector2 position)
        {
            CurrentHeight = position.Y;
            Active = true;
            Direction = new Vector2(-1, 0);
            Speed = 1.5f;
            Position = position;            
            MaxHP = 300;
            CurrentHP = MaxHP;
            //MoveDelay = 20;
            ResourceMinMax = new Vector2(1, 5);
            CurrentAttackDelay = 0;
            AttackDelay = 1500;
            TowerAttackPower = 4;
            InvaderType = InvaderType.HealDrone;
            CurrentFrame = 0;
            YRange = new Vector2(100, 350);
            RangedAttackPower = 20;
            Airborne = true;
            InAir = true;

            InvaderState = InvaderState.Walk;
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            foreach (LightningBolt bolt in BoltList)
            {
                bolt.Update(gameTime);
            }

            BoltList.RemoveAll(Bolt => Bolt.Alpha <= 0);

            

            base.Update(gameTime, cursorPosition);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (HealTarget != null)
                DrawDepth = HealTarget.DrawDepth;

            //if (Jet != null)
            //    Jet.Draw(spriteBatch);

            base.Draw(spriteBatch);
        }

        public override void TrapDamage(Trap trap)
        {
            if (VulnerableToTrap == true)
            {
                switch (trap.TrapType)
                {
                    default:
                        CurrentHP -= trap.NormalDamage;

                        if (trap.InvaderDOT != null)
                            DamageOverTime(trap.InvaderDOT, trap.InvaderDOT.Color);

                        if (trap.InvaderFreeze != null)
                            Freeze(trap.InvaderFreeze, trap.InvaderDOT.Color);

                        if (trap.InvaderSlow != null)
                            MakeSlow(trap.InvaderSlow);
                        break;
                }
            }
        }
    }
}
