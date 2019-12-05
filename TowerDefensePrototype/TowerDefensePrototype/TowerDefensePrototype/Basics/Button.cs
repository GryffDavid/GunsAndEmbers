﻿using System;
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
        public string AssetName, IconName, Text, FontName;
        public Vector2 FrameSize, Scale, Position, CursorPosition;
        public Rectangle DestinationRectangle, SourceRectangle;

        Vector2 IconPosition;
        Color Color, TextColor;
        Texture2D ButtonStrip, IconTexture;        
        Rectangle IconRectangle;
        SpriteFont Font;        

        MouseState CurrentMouseState, PreviousMouseState;
        MousePosition CurrentMousePosition, PreviousMousePosition;
        public ButtonSpriteState CurrentButtonState;

        int CurrentFrame;
        public bool Active, JustClicked;
        public bool ButtonActive;

        public Button(string assetName, Vector2 position, string iconName = null, Vector2? scale = null, 
            Color? color = null, string text = "", string fontName = "", Color? textColor = null)
        {
            ButtonActive = true;

            AssetName = assetName;
            Position = position;

            IconName = iconName;

            if (scale == null)
                Scale = new Vector2(1, 1);
            else
                Scale = scale.Value;

            if (color == null)
                Color = Color.White;
            else
                Color = color.Value;

            Text = text;

            FontName = fontName;

            if (textColor == null)
                TextColor = Color.White;
            else
                TextColor = textColor.Value;

            CurrentButtonState = ButtonSpriteState.Released;
        }

        public void LoadContent(ContentManager contentManager)
        {
            if (ButtonActive == true)
            {
                ButtonStrip = contentManager.Load<Texture2D>(AssetName);

                FrameSize = new Vector2((int)(ButtonStrip.Width / 3f), ButtonStrip.Height);
                DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)(FrameSize.X * Scale.X), (int)(FrameSize.Y * Scale.Y));
                
                if (IconName != null)
                {
                    IconTexture = contentManager.Load<Texture2D>(IconName);
                    IconPosition = new Vector2(Position.X + (DestinationRectangle.Width - IconTexture.Width) / 2, Position.Y + (DestinationRectangle.Height - IconTexture.Height) / 2);
                    IconRectangle = new Rectangle((int)IconPosition.X, (int)IconPosition.Y, IconTexture.Width, IconTexture.Height);
                }

                if (FontName != "")
                {
                    Font = contentManager.Load<SpriteFont>(FontName);
                }
            }
        }

        public void Update()
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
                    if (ButtonActive == true)
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
            if (ButtonActive == true)
            {
                spriteBatch.Draw(ButtonStrip, DestinationRectangle, SourceRectangle, Color);
                if (IconName != null)
                {
                    if (CurrentButtonState != ButtonSpriteState.Pressed)                    
                        spriteBatch.Draw(IconTexture, IconRectangle, Color.White);
                    else
                        spriteBatch.Draw(IconTexture, new Rectangle(IconRectangle.X+2, IconRectangle.Y+2, IconRectangle.Width, IconRectangle.Height), Color.White);                    
                }

                if (Text != "")
                {
                    if (CurrentButtonState != ButtonSpriteState.Pressed)
                    {
                        Vector2 TextSize = Font.MeasureString(Text);
                        spriteBatch.DrawString(Font, Text, new Vector2(DestinationRectangle.Center.X - (TextSize.X / 2), DestinationRectangle.Center.Y - (TextSize.Y / 2)), TextColor);
                    }
                    else
                    {
                        Vector2 TextSize = Font.MeasureString(Text);
                        spriteBatch.DrawString(Font, Text, new Vector2(DestinationRectangle.Center.X - (TextSize.X / 2)+2, DestinationRectangle.Center.Y - (TextSize.Y / 2)+2), TextColor);
                    }
                }                
            }
        }
    }
}
