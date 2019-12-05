using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Airship : RangedInvader
    {
        public Airship(Vector2 position)
        {
            Active = true;            
            MoveVector = new Vector2(-1, 0);
            Position = position;
            AssetName = "Airship";
            MaxHP = 300;
            CurrentHP = MaxHP;
            MoveDelay = 20;
            ResourceMinMax = new Vector2(1, 5);
            CurrentAttackDelay = 0;
            AttackDelay = 1500;
            AttackPower = 4;
            InvaderType = TowerDefensePrototype.InvaderType.Airship;
            FrameSize = new Vector2(125, 73);
            FrameDelay = 120;
            TotalFrames = 1;
            CurrentFrame = 0;
            YRange = new Vector2(60, 150);
            RangedAttackPower = 20;
            Range = new Vector2(200, 800);
            AngleRange = new Vector2(110, 160);
            PowerRange = new Vector2(9, 12);     
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void TrapDamage(TrapType trapType)
        {            
            throw new NotImplementedException();
        }
    }
}
