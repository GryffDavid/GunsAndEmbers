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
            Speed = 1.5f;
            Position = position;            
            MaxHP = 300;
            CurrentHP = MaxHP;
            ResourceMinMax = new Vector2(1, 5);
            InvaderType = InvaderType.HealDrone;
            YRange = new Vector2(100, 350);
            Airborne = true;
            InAir = true;
            InvaderAnimationState = AnimationState_Invader.Walk;

            AngleRange = new Vector2(170, 190);
            RangedDamage = 10;
            MaxFireDelay = 250;
            CurrentFireDelay = 0;
            DistanceRange = new Vector2(600, 800);
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
    }
}
