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
        public event ButtonClickHappenedEventHandler ButtonClickHappened;
        public void CreateButtonClick(MouseButton button)
        {
            OnButtonClickHappened(button);
        }
        protected virtual void OnButtonClickHappened(MouseButton button)
        {
            if (ButtonClickHappened != null)
                ButtonClickHappened(this, new ButtonClickEventArgs() { ClickedButton = button });
        }

        MouseState CurrentMouseState, PreviousMouseState;
        Vector2 CursorPosition;
        public Vector2 Velocity, NextPosition;

        public ButtonSpriteState CurrentBoxState;
        public Rectangle DestinationRectangle;
        bool InOut, PrevInOut;

        private ButtonState _LeftButtonState;
        public ButtonState LeftButtonState
        {
            get { return _LeftButtonState; }
            set
            {
                _LeftButtonState = value;
                PrevInOut = InOut;

                if (DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)) == true)
                {
                    //In                    
                    InOut = true;
                }
                else
                {
                    //Out
                    InOut = false;
                }

                if (PrevInOut == true && InOut == true &&
                    CurrentMouseState.LeftButton == ButtonState.Released &&
                    PreviousMouseState.LeftButton == ButtonState.Pressed)
                {
                    CreateButtonClick(MouseButton.Left);
                }
            }
        }

        public WeaponBox(Vector2 position, Turret turret, Trap trap) : base(position, turret, trap)
        {
            NextPosition = position;
        }

        public void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            CurrentMouseState = Mouse.GetState();
            CursorPosition = cursorPosition;

            //Position = Vector2.Lerp(Position, NextPosition, 0.2f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
            //Position += Velocity;


            if (new Vector2((int)Position.X, (int)Position.Y) == new Vector2((int)NextPosition.X, (int)NextPosition.Y))
            {
                Velocity = new Vector2(0, 0);
                NextPosition = Position;
            }

            if (CurrentMouseState.LeftButton != PreviousMouseState.LeftButton)
                LeftButtonState = Mouse.GetState().LeftButton;

            DestinationRectangle = new Rectangle((int)Position.X, (int)(Position.Y - BoxSize.Y), (int)BoxSize.X, (int)BoxSize.Y);

            #region Change the appearance of the tab when it's moused over
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
            #endregion            

            PreviousMouseState = CurrentMouseState;

            UpdateQuads();
            SetUpBars();
        }
    }
}