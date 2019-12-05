using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class Wave
    {
        List<Invader> InvaderList;

        public Wave()
        {
            InvaderList = new List<Invader>();
        }

        public void AddEnemies()
        {

        }

        public void Update(GameTime gameTime)
        {
            foreach (Invader invader in InvaderList)
            {
                invader.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Invader invader in InvaderList)
            {
                invader.Draw(spriteBatch);
            }
        }
    }
}
