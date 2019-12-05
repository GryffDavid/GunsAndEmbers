using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype.Basics
{
    class Gibs : VerletObject
    {
        //The idea for this is that an explosion will send limb gibs everywhere that leak blood from one end
        //Could also be used for the circular saw etc.
        public Emitter BloodEmitter; //The emitter that makes it appear as thos this gib is spraying blood everywhere
        
        public Gibs()
        {
            //3 Nodes
            //2 Stick
            //= 1 Limb
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
