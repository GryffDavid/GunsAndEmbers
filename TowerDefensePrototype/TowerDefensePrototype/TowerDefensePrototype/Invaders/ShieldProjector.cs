using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class ShieldProjector : Invader
    {
        public BoundingSphere ShieldBoundingSphere;
        public float MaxShieldValue, CurrentShieldValue;

        public ShieldProjector(Vector2 position)
        {
            Speed = 0.65f;
            Position = position;
            MaxHP = 20;
            ResourceMinMax = new Vector2(8, 20);
            YRange = new Vector2(700, 900);

            InvaderType = InvaderType.ShieldProjector;

            InvaderAnimationState = AnimationState_Invader.Walk;
            CurrentMacroBehaviour = MacroBehaviour.AttackTower;
            CurrentMicroBehaviour = MicroBehaviour.MovingForwards;

            //MeleeDamageStruct = new InvaderMeleeStruct()
            //{
            //    CurrentAttackDelay = 0,
            //    MaxAttackDelay = 2000,
            //    Damage = 10
            //};
        }

        public void Update(GameTime gameTime)
        {

        }
    }
}
