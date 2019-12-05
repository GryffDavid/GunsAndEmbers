using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class Shield
    {
        public bool ShieldOn;
        public float MaxShield, CurrentShield;
        public double CurrentShieldTime, ShieldTime;
        public BoundingSphere ShieldBoundingSphere;

        public Shield()
        {

        }

        public void Update(GameTime gameTime)
        {
            if (ShieldOn == false)
            {
                CurrentShieldTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            if (ShieldOn == false &&
                CurrentShieldTime >= ShieldTime)
            {
                CurrentShield += 0.05f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60);
                CurrentShield = MathHelper.Clamp(CurrentShield, 0, MaxShield);
            }

            if (ShieldOn == false &&
                CurrentShieldTime >= ShieldTime &&
                CurrentShield == MaxShield)
            {
                ShieldOn = true;
                CurrentShieldTime = 0;
            }
        }

        public float TakeDamage(float value)
        {
            if (ShieldOn == true)
            {
                if (CurrentShield > 0)
                {
                    CurrentShield -= value;
                    return 0;
                }
                else
                {
                    ShieldOn = false;
                    return 0;
                }
            }
            else
            {
                return 0 - value;
            }
        }
    }
}
