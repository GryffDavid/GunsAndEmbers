using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class FireElemental : Invader
    {
    
        public FireElemental(Vector2 position)
        {
            Active = true;
            MoveVector = new Vector2(-1, 0);
            Position = position;
            CurrentHP = 50;
            MaxHP = 50;
            MoveDelay = 10;
            ResourceMinMax = new Vector2(50, 100);
            CurrentAttackDelay = 0;
            AttackDelay = 3000;
            AttackPower = 1;
            CurrentFrame = 0;
            InvaderType = InvaderType.FireElemental;
            YRange = new Vector2(525, 630);

            CurrentAnimation = new Animation()
            {
                AssetName = "Blank",
                TotalFrames = 1,
                FrameDelay = 500
            };

            Color FireColor = new Color(Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 200);
            Color FireColor2 = new Color(Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 90);

            DustEmitter = new Emitter("Particles/FireParticle", new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Top),
                                new Vector2(180, 180), new Vector2(0.5f, 0.75f), new Vector2(40, 80), 0.01f, true, new Vector2(-20, 20),
                                new Vector2(-4, 4), new Vector2(1, 2), FireColor, FireColor2, 0.2f, -1, 10, 1, true, new Vector2(MaxY + 100, MaxY + 100),
                                false,null,true, false);
        }

        public override void Update(GameTime gameTime)
        {
            if (Active == true)
            {
                if (DustEmitter != null)
                {                    
                    DustEmitter.Update(gameTime);
                    DustEmitter.Position = new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Top);
                    DustEmitter.MaxY = MaxY;
                    DustEmitter.DrawDepth = DrawDepth;
                }
            }
            base.Update(gameTime);
        }

        public override void TrapDamage(TrapType trapType)
        {

        }
    }
}
