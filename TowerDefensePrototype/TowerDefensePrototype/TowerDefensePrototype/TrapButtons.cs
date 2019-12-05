using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class TrapButtons
    {
        public Vector2 Position, Scale;
        int AvailableSlots;
        Color Color;
        public List<Button> ButtonList;
        public List<Trap> TrapList;        

        public TrapButtons(string assetName, Vector2 position, int availableSlots, Vector2? scale = null, Color? color = null)
        {
            AvailableSlots = availableSlots;
            Position = position;

            if (scale == null)
                Scale = new Vector2(1, 1);
            else
                Scale = scale.Value;

            if (color == null)
                Color = Color.White;
            else
                Color = color.Value;

            ButtonList = new List<Button>();

            for (int i = 0; i < availableSlots; i++)
            {
                ButtonList.Add(new Button(assetName, new Vector2(Position.X + (128 * i) + 256, Position.Y)));
            }

            TrapList = new List<Trap>();
            
            TrapList.Add(new Wall(100, 1));            
        }

        public void LoadContent(ContentManager contentManager)
        {
            foreach (Button trapSlot in ButtonList)
            {
                trapSlot.LoadContent(contentManager);
            }

            foreach (Trap trap in TrapList)
            {
                trap.LoadContent(contentManager);
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (Button trapSlot in ButtonList)
            {
                trapSlot.Update(gameTime);
            }

            foreach (Trap trap in TrapList)
            {
                trap.Update();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Button trapSlot in ButtonList)
            {
                trapSlot.Draw(spriteBatch);
            }

            foreach (Trap trap in TrapList)
            {
                trap.Draw(spriteBatch);
            }
        }
    }    
}
