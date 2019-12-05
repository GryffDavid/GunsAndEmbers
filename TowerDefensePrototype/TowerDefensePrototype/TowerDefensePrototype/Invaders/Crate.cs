using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class Crate : Invader
    {
        public override float OriginalSpeed { get { return 0.0f; } }

        public Crate(Vector2 position, Vector2? yRange = null, float? maxHP = 10)
            : base(position, yRange)
        {
            Speed = 0;
            MaxHP = maxHP.Value;
            ResourceMinMax = new Vector2(5, 10);
            YRange = new Vector2(700, 900);

            InvaderType = InvaderType.Crate;

            InvaderAnimationState = AnimationState_Invader.Stand;
            CurrentMacroBehaviour = MacroBehaviour.AttackTower;
            CurrentMicroBehaviour = MicroBehaviour.Stationary;

            MeleeDamageStruct = new InvaderMeleeStruct()
            {
                CurrentAttackDelay = 0,
                MaxAttackDelay = 2000,
                Damage = 10
            };
        }

        public override void Update(GameTime gameTime, Vector2 cursorPosition)
        {

            base.Update(gameTime, cursorPosition);
        }
    }
}
