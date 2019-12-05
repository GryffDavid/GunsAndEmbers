using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TowerDefensePrototype
{
    class CheckBox : Button
    {
        public bool ValueState;

        protected override void OnButtonClickHappened(MouseButton button)
        {
            switch (ValueState)
            {
                case true:
                    ValueState = false;
                    break;

                case false:
                    ValueState = true;
                    break;
            }
        }

        public CheckBox(Texture2D buttonStrip, Vector2 position, Texture2D icon)
            : base(buttonStrip, position, icon)
        {
            TextColor = Color.White;
            ValueState = false;
        }

        public override void Update(Vector2 cursorPosition, GameTime gameTime)
        {
            Text = ValueState.ToString();
            base.Update(cursorPosition, gameTime);
        }
    }
}
