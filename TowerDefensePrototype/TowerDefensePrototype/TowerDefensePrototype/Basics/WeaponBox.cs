using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    class WeaponBox : UIWeaponInfoTip
    {
        public Nullable<TurretType> ContainsTurret = null;
        public Nullable<TrapType> ContainsTrap = null;

        MouseState CurrentMouseState, PreviousMouseState;
        public MousePosition CurrentMousePosition, PreviousMousePosition;
        public ButtonSpriteState CurrentBoxState;

        public Rectangle DestinationRectangle;

        public bool JustClicked = false;

        public WeaponBox(Vector2 position, Turret turret, Trap trap) : base(position, turret, trap)
        {
            
        }

        public void Update(GameTime gameTime)
        {
            CurrentMouseState = Mouse.GetState();
            JustClicked = false;

            DestinationRectangle = new Rectangle((int)Position.X, (int)(Position.Y - BoxSize.Y), (int)BoxSize.X, (int)BoxSize.Y);

            #region Check if a tab has been clicked
            if (DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
            {
                if (Locked == false)
                {
                    CurrentBoxState = ButtonSpriteState.Hover;
                    WeaponBoxColor = Color.Lerp(Color.White, Color.Transparent, 0.5f);
                }
            }
            else
            {                
                CurrentBoxState = ButtonSpriteState.Released;
                WeaponBoxColor = Color.Lerp(Color.Black, Color.Transparent, 0.5f);
            }

            #region Check whether the mouse is inside the button as it's pressed and store that state
            if (CurrentMouseState.LeftButton == ButtonState.Pressed &&
                PreviousMouseState.LeftButton == ButtonState.Released)
            {
                if (DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                {
                    CurrentMousePosition = MousePosition.Inside;
                }
                else
                {
                    CurrentMousePosition = MousePosition.Outside;
                }
            }
            #endregion

            #region Compare the previous position of the mouse with the current position, if they're both inside the tab, it has been clicked
            if (CurrentMouseState.LeftButton == ButtonState.Released &&
                PreviousMouseState.LeftButton == ButtonState.Pressed)
            {
                if (DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                {
                    CurrentMousePosition = MousePosition.Inside;
                }
                else
                {
                    CurrentMousePosition = MousePosition.Outside;
                }

                if (CurrentMousePosition == MousePosition.Inside && PreviousMousePosition == MousePosition.Inside)
                {
                    JustClicked = true;
                }
            }
            #endregion

            PreviousMousePosition = CurrentMousePosition;
            #endregion
            

            PreviousMouseState = CurrentMouseState;

            UpdateQuads();
        }
    }
}