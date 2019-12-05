using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class AirStrikeSpecial : SpecialAbility
    {
        public bool Active;
        public float CurrentTime, TimeInterval;
        public Vector2 CurrentPosition;

        public AirStrikeSpecial()
        {
            SpecialType = SpecialType.AirStrike;
            CurrentPosition = new Vector2(0, -64);
            TimeInterval = 1000;
            Active = true;
        }

        public override void Update(GameTime gameTime)
        {
            CurrentPosition += new Vector2(200, 0) * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000f;

            if (Active == true)
            {
                CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (CurrentPosition.X > 1920)
                {
                    Active = false;
                }
            }
        }
    }
}
 