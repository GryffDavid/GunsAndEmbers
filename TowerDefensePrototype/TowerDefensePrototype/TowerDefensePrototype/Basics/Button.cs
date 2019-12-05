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
    public enum ButtonSpriteState { Released, Hover, Pressed };
    public enum MousePosition { Inside, Outside };

    class Button
    {
        string AssetName;
        public Vector2 FrameSize, Scale, Position, CursorPosition;
        Color Color;
        Texture2D ButtonStrip;
        Rectangle DestinationRectangle, SourceRectangle;

        MouseState CurrentMouseState, PreviousMouseState;
        MousePosition CurrentMousePosition, PreviousMousePosition;
        ButtonSpriteState CurrentButtonState;

        int CurrentFrame;
        bool Active;
        public bool JustClicked;

        public Button(string assetName, Vector2 position, Vector2? scale = null, Color? color = null)
        {
            AssetName = assetName;
            Position = position;

            if (scale == null)
                Scale = new Vector2(1, 1);
            else
                Scale = scale.Value;

            if (color == null)
                Color = Color.White;
            else
                Color = color.Value;               

            CurrentButtonState = ButtonSpriteState.Released;
        }

        public void LoadContent(ContentManager contentManager)
        {
            ButtonStrip = contentManager.Load<Texture2D>(AssetName);
            FrameSize= new Vector2((int)(ButtonStrip.Width / 3f),ButtonStrip.Height);
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)(FrameSize.X * Scale.X), (int)(FrameSize.Y * Scale.Y));
        }

        public void Update(GameTime gameTime)
        {
            CurrentMouseState = Mouse.GetState();

            CursorPosition = new Vector2(CurrentMouseState.X, CurrentMouseState.Y);

            switch (CurrentButtonState)
            {
                case ButtonSpriteState.Released:
                    CurrentFrame = 0;
                    break;

                case ButtonSpriteState.Hover:
                    CurrentFrame = 1;
                    break;

                case ButtonSpriteState.Pressed:
                    CurrentFrame = 2;
                    break;
            }

            SourceRectangle = new Rectangle(CurrentFrame * (int)FrameSize.X, 0, (int)FrameSize.X, (int)FrameSize.Y);           

            if (DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)))
            {
                CurrentMousePosition = MousePosition.Inside;
            }

            if (!DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)))
            {
                CurrentMousePosition = MousePosition.Outside;
            }

            if (PreviousMousePosition == MousePosition.Outside && CurrentMousePosition == MousePosition.Inside && CurrentMouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Pressed)
            {
                Active = false;
            }

            if (PreviousMousePosition == MousePosition.Inside && CurrentMousePosition == MousePosition.Inside && CurrentMouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Released)
            {
                Active = true;
            }
          
            if (PreviousMousePosition == MousePosition.Inside && CurrentMousePosition == MousePosition.Outside)
            {
                Active = false;
            }

            if (Active == true)
            {
                if (DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)) &&
                    CurrentMouseState.LeftButton == ButtonState.Released &&
                    PreviousMouseState.LeftButton == ButtonState.Pressed
                    )
                {
                    JustClicked = true;
                }
            }

            if (PreviousMouseState.LeftButton == ButtonState.Released && CurrentMouseState.LeftButton == ButtonState.Released)
            {
                JustClicked = false;
            }

            if (DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)))
            {
                if (CurrentMouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Released)
                {
                    CurrentButtonState = ButtonSpriteState.Pressed;
                }
        
                if (CurrentMouseState.LeftButton == ButtonState.Released)
                {
                    CurrentButtonState = ButtonSpriteState.Hover;
                }
            }
            else
            {
                CurrentButtonState = ButtonSpriteState.Released;
            }

            PreviousMousePosition = CurrentMousePosition;
            PreviousMouseState = CurrentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(ButtonStrip, DestinationRectangle, SourceRectangle, Color);
        }
    }
}
