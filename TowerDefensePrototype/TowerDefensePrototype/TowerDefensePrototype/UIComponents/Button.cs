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
    //Using a non-generic delegate so that I can pass it to all the buttons when they're initialised
    public delegate void ButtonClickHappenedEventHandler(object source, EventArgs e);

    public class Button
    {
        public event ButtonClickHappenedEventHandler ButtonClickHappened;
        public void CreateButtonClick()
        {
            OnButtonClickHappened();
        }
        protected virtual void OnButtonClickHappened()
        {
            if (ButtonClickHappened != null)
                ButtonClickHappened(this, null);
        }

        public string Text;
        public Vector2 FrameSize, Scale, CurrentPosition, CursorPosition, IconPosition, NextPosition, IconNextPosition, NextScale;
        public Rectangle DestinationRectangle, SourceRectangle;

        public Color Color, TextColor;
        public Texture2D ButtonStrip, IconTexture;
        SpriteFont Font;
        SpriteBatch SpriteBatch;

        MouseState CurrentMouseState, PreviousMouseState;
        public ButtonSpriteState CurrentButtonState;

        int CurrentFrame;
        public bool Active, CanBeRightClicked, JustClicked, JustRightClicked;
        public bool ButtonActive;
        public Color CurrentIconColor;

        //This was used to playe the hover sound when necessary - should be replace with something neater
        //public bool PlayHover;

        float DrawDepth;
        string Alignment;

        //The position of the mouse cursor - true = in, false = out
        bool InOut, PrevInOut;

        private ButtonState _LeftButtonState;
        public ButtonState LeftButtonState
        {
            get { return _LeftButtonState; }
            set
            {
                _LeftButtonState = value;
                PrevInOut = InOut;
                if (DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)) == true &&
                    GetColor() != Color.Transparent)
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
                    CreateButtonClick();
                }
            }
        }

        public Button(Texture2D buttonStrip, Vector2 position, Texture2D icon = null, Vector2? scale = null, 
                      Color? color = null, string text = "", SpriteFont font = null, string alignment = "Left", 
                      Color? textColor = null, bool? canBeRightClicked = null)
        {
            ButtonActive = true;

            ButtonStrip = buttonStrip;
            Font = font;
            IconTexture = icon;

            CurrentPosition = position;
            NextPosition = position;

            Text = text;

            if (scale == null)
                Scale = new Vector2(1, 1);
            else
                Scale = scale.Value;

            if (color == null)
                Color = Color.White;
            else
                Color = color.Value;

            if (textColor == null)
                TextColor = Color.White;
            else
                TextColor = textColor.Value;

            if (canBeRightClicked == null)
            {
                CanBeRightClicked = false;
            }
            else
            {
                CanBeRightClicked = canBeRightClicked.Value;
            }

            DrawDepth = 0.99f;

            Alignment = alignment;

            CurrentButtonState = ButtonSpriteState.Released;

            CurrentIconColor = Color.White;

            NextScale = Vector2.One;
        }

        public void Initialize(ButtonClickHappenedEventHandler thing)
        {
            ButtonClickHappened += thing;

            if (ButtonActive == true)
            {
                FrameSize = new Vector2((int)(ButtonStrip.Width / 3f), ButtonStrip.Height);
                DestinationRectangle = new Rectangle((int)CurrentPosition.X, (int)CurrentPosition.Y, (int)(FrameSize.X * Scale.X), (int)(FrameSize.Y * Scale.Y));
            }
        }

        public virtual void Update(Vector2 cursorPosition, GameTime gameTime)
        {
            CurrentMouseState = Mouse.GetState();
            CursorPosition = cursorPosition;

            if (CurrentMouseState.LeftButton != PreviousMouseState.LeftButton)
                LeftButtonState = Mouse.GetState().LeftButton;

            #region Reposition the icon with the button
            if (IconNextPosition != IconPosition)
            {
                IconPosition = Vector2.Lerp(IconPosition, IconNextPosition, 0.15f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));

                if (Math.Abs(IconPosition.X - IconNextPosition.X) < 0.5f)
                {
                    IconPosition.X = IconNextPosition.X;
                }
            }
            #endregion

            #region Move and scale the button
            if (NextPosition != CurrentPosition)
            {
                CurrentPosition = Vector2.Lerp(CurrentPosition, NextPosition, 0.15f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));

                if (Math.Abs(CurrentPosition.X - NextPosition.X) < 0.5f)
                {
                    CurrentPosition.X = NextPosition.X;
                }
            }

            if (Scale != NextScale)
                Scale = Vector2.Lerp(Scale, NextScale, 0.15f);
            #endregion

            DestinationRectangle = new Rectangle((int)CurrentPosition.X, (int)CurrentPosition.Y, (int)(FrameSize.X * Scale.X), (int)(FrameSize.Y * Scale.Y));

            if (DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)) && GetColor() != Color.Transparent)
            {
                if (CurrentMouseState.LeftButton == ButtonState.Pressed && InOut == true)
                {
                    CurrentButtonState = ButtonSpriteState.Pressed;
                }
                else
                {
                    CurrentButtonState = ButtonSpriteState.Hover;
                }
            }
            else
            {
                CurrentButtonState = ButtonSpriteState.Released;
            }

            #region Change the frame based on the button state
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
            #endregion

            SourceRectangle = new Rectangle(CurrentFrame * (int)FrameSize.X, 0, (int)FrameSize.X, (int)FrameSize.Y);
            
            PreviousMouseState = CurrentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            SpriteBatch = spriteBatch;
            Color ReleasedColor, PressedColor, drawColor;
            ReleasedColor = Color.Lerp(TextColor, Color.Transparent, 0.5f);
            PressedColor = TextColor;
            drawColor = TextColor;

            if (ButtonActive == true)
            {                
                spriteBatch.Draw(ButtonStrip, DestinationRectangle, SourceRectangle, Color, MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, DrawDepth);

                if (IconTexture != null)
                {
                    //IconPosition = new Vector2(CurrentPosition.X + (DestinationRectangle.Width - IconTexture.Width) / 2, CurrentPosition.Y + (DestinationRectangle.Height - IconTexture.Height) / 2);
                    //IconNextPosition = IconPosition;
                    //IconRectangle = new Rectangle((int)IconPosition.X, (int)IconPosition.Y, IconTexture.Width, IconTexture.Height);

                    if (CurrentButtonState != ButtonSpriteState.Pressed)
                        spriteBatch.Draw(IconTexture, new Rectangle((int)(CurrentPosition.X + (DestinationRectangle.Width - IconTexture.Width)/2), (int)(CurrentPosition.Y + (DestinationRectangle.Height - IconTexture.Height)/2), IconTexture.Width, IconTexture.Height), null, CurrentIconColor, MathHelper.ToRadians(0), Vector2.Zero,
                            SpriteEffects.None, 0.991f);
                    else
                        spriteBatch.Draw(IconTexture, new Rectangle((int)(CurrentPosition.X + (DestinationRectangle.Width - IconTexture.Width) / 2) + 2, (int)(CurrentPosition.Y + (DestinationRectangle.Height - IconTexture.Height) / 2) + 2, IconTexture.Width, IconTexture.Height), null, CurrentIconColor, MathHelper.ToRadians(0), Vector2.Zero,
                            SpriteEffects.None, 0.991f);
                }

                switch (CurrentButtonState)
                {
                    case ButtonSpriteState.Pressed:
                        drawColor = PressedColor;
                        break;

                    case ButtonSpriteState.Hover:
                        drawColor = PressedColor;
                        break;

                    case ButtonSpriteState.Released:
                        drawColor = ReleasedColor;
                        break;
                }

                if (Text != "" && Text != null)
                {
                    if (CurrentButtonState != ButtonSpriteState.Pressed)
                    {
                        Vector2 TextSize = Font.MeasureString(Text);

                        switch (Alignment)
                        {
                            case "Centre":                                
                                spriteBatch.DrawString(Font, Text, new Vector2(DestinationRectangle.Center.X - (TextSize.X / 2),
                                    DestinationRectangle.Center.Y - (Font.MeasureString(Text[0].ToString()).Y / 2)), drawColor, MathHelper.ToRadians(0), 
                                    Vector2.Zero, 1, SpriteEffects.None, 0.5f);
                                break;

                            case "Left":
                                spriteBatch.DrawString(Font, Text, new Vector2(DestinationRectangle.Left + 16,
                                    DestinationRectangle.Center.Y - (Font.MeasureString(Text[0].ToString()).Y / 2)), drawColor, MathHelper.ToRadians(0), 
                                    Vector2.Zero, 1, SpriteEffects.None, 0.5f);
                                break;

                            case "Right":
                                spriteBatch.DrawString(Font, Text, new Vector2(DestinationRectangle.Right - TextSize.X - 16,
                                    DestinationRectangle.Center.Y - (Font.MeasureString(Text[0].ToString()).Y / 2)), drawColor, MathHelper.ToRadians(0), 
                                    Vector2.Zero, 1, SpriteEffects.None, 0.5f);
                                break;
                        }
                    }
                    else
                    {
                        Vector2 TextSize = Font.MeasureString(Text);

                        switch (Alignment)
                        {
                            case "Centre":
                                spriteBatch.DrawString(Font, Text, new Vector2(DestinationRectangle.Center.X - (TextSize.X / 2)+2,
                                    DestinationRectangle.Center.Y - (Font.MeasureString(Text[0].ToString()).Y / 2) + 2), drawColor, MathHelper.ToRadians(0), 
                                    Vector2.Zero, 1, SpriteEffects.None, 0.5f);
                                break;

                            case "Left":
                                spriteBatch.DrawString(Font, Text, new Vector2(DestinationRectangle.Left + 16+2,
                                    DestinationRectangle.Center.Y - (Font.MeasureString(Text[0].ToString()).Y / 2) + 2), drawColor, MathHelper.ToRadians(0), 
                                    Vector2.Zero, 1, SpriteEffects.None, 0.5f);
                                break;

                            case "Right":
                                spriteBatch.DrawString(Font, Text, new Vector2(DestinationRectangle.Right - TextSize.X - 16+2,
                                    DestinationRectangle.Center.Y - (Font.MeasureString(Text[0].ToString()).Y / 2) + 2), drawColor, MathHelper.ToRadians(0), 
                                    Vector2.Zero, 1, SpriteEffects.None, 0.5f);
                                break;
                        }
                    }
                }                
            }
        }

        public Color GetColor()
        {
            Color[] retrievedColor = new Color[1];
            Rectangle Rect;
            Rect = new Rectangle((int)((1 / Scale.X) * ((int)CursorPosition.X - CurrentPosition.X)), (int)((1 / Scale.Y) * ((int)CursorPosition.Y - CurrentPosition.Y)), 1, 1);

            if (DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)) &&
                new Rectangle(0, 0, 1920, 1080).Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)))
            {
                if (DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)) && Rect != null && Rect.Width == 1 && Rect.Height == 1)
                {
                    //Vector2 pos = new Vector2((1 / Scale.X) * ((int)CursorPosition.X - Position.X), (1 / Scale.Y) * ((int)CursorPosition.Y - Position.Y));
                    //Rectangle testRect = new Rectangle((int)((1 / Scale.X) * ((int)CursorPosition.X - Position.X)), (int)((1 / Scale.Y) * ((int)CursorPosition.Y - Position.Y)), 1, 1);
                    if (Rect != new Rectangle(Math.Abs((int)((1 / Scale.X) * ((int)CursorPosition.X - CurrentPosition.X))),
                        Math.Abs((int)((1 / Scale.Y) * ((int)CursorPosition.Y - CurrentPosition.Y))), 1, 1))
                        return Color.White;
                    else
                        ButtonStrip.GetData<Color>(0, Rect, retrievedColor, 0, 1);
                }
            }

            return retrievedColor[0];
        }

        public void ResetState()
        {
            CurrentFrame = 0;
            CurrentButtonState = ButtonSpriteState.Released;
            //CurrentMousePosition = MousePosition.Outside;
            SourceRectangle = new Rectangle(CurrentFrame * (int)FrameSize.X, 0, (int)FrameSize.X, (int)FrameSize.Y);
        }
    }
}
