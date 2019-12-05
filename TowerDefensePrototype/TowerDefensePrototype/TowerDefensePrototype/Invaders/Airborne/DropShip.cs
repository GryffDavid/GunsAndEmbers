using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class DropShip : Invader
    {
        public DropShip(Vector2 position, Vector2? yRange = null)
            : base(position, yRange)
        {
            MaxHP = 400;
            CurrentHP = MaxHP;
            InvaderType = InvaderType.DropShip;
            YRange = new Vector2(60, 150);
        }

        public void LoadContent(ContentManager content)
        {

        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
