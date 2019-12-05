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

    public class Button
    {
        public string Text;
        public Vector2 FrameSize, Scale, CurrentPosition, CursorPosition, IconPosition, NextPosition, IconNextPosition, NextScale;
        public Rectangle DestinationRectangle, SourceRectangle;

        public Color Color, TextColor;
        public Texture2D ButtonStrip, IconTexture;       
        SpriteFont Font;
        SpriteBatch SpriteBatch;

        MouseState CurrentMouseState, PreviousMouseState;
        public MousePosition CurrentMousePosition, PreviousMousePosition;
        public ButtonSpriteState CurrentButtonState;

        public int CurrentFrame;
        public bool Active, CanBeRightClicked, JustClicked, JustRightClicked;
        public bool ButtonActive;
        public Color CurrentIconColor;
        public bool PlayHover;
        public float DrawDepth;
        string Alignment;

        public Button(Texture2D buttonStrip, Vector2 position, Texture2D icon = null, Vector2? scale = null, 
            Color? color = null, string text = "", SpriteFont font = null, string alignment = "Left", Color? textColor = null, bool? canBeRightClicked = null)
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

        public void LoadContent()
        {
            if (ButtonActive == true)
            {
                FrameSize = new Vector2((int)(ButtonStrip.Width / 3f), ButtonStrip.Height);
                DestinationRectangle = new Rectangle((int)CurrentPosition.X, (int)CurrentPosition.Y, (int)(FrameSize.X * Scale.X), (int)(FrameSize.Y * Scale.Y));

                //if (IconName != null)
                //{
                //    IconTexture = contentManager.Load<Texture2D>(IconName);
                //    IconPosition = new Vector2(CurrentPosition.X + (DestinationRectangle.Width - IconTexture.Width) / 2, CurrentPosition.Y + (DestinationRectangle.Height - IconTexture.Height) / 2);
                //    IconNextPosition = IconPosition;
                //    IconRectangle = new Rectangle((int)IconPosition.X, (int)IconPosition.Y, IconTexture.Width, IconTexture.Height);
                //}

                //if (FontName != "")
                //{
                //    Font = contentManager.Load<SpriteFont>(FontName);
                //}
            }
        }

        public virtual void Update(Vector2 cursorPosition, GameTime gameTime)
        {
            CurrentMouseState = Mouse.GetState();
            CursorPosition = cursorPosition;

            if (IconNextPosition != IconPosition)
            {
                IconPosition = Vector2.Lerp(IconPosition, IconNextPosition, 0.15f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));

                if (Math.Abs(IconPosition.X - IconNextPosition.X) < 0.5f)
                {
                    IconPosition.X = IconNextPosition.X;
                }
            }

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
                                   
            DestinationRectangle = new Rectangle((int)CurrentPosition.X, (int)CurrentPosition.Y, (int)(FrameSize.X * Scale.X), (int)(FrameSize.Y * Scale.Y));

            //if (IconName != null)
            //IconRectangle = new Rectangle((int)IconPosition.X, (int)IconPosition.Y, IconTexture.Width, IconTexture.Height);

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

            if (DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)) &&
                GetColor() == Color.Transparent)
            {
                CurrentMousePosition = MousePosition.Outside;
            }

            if (PreviousMousePosition == MousePosition.Outside &&
                CurrentMousePosition == MousePosition.Inside)
            {
                if (CurrentMouseState.LeftButton == ButtonState.Pressed &&
                    PreviousMouseState.LeftButton == ButtonState.Pressed)
                {
                    Active = false;
                }

                if (CurrentMouseState.RightButton == ButtonState.Pressed &&
                    PreviousMouseState.RightButton == ButtonState.Pressed && CanBeRightClicked == true)
                {                    
                    Active = false;
                }
            }

            if (PreviousMousePosition == MousePosition.Inside &&
                CurrentMousePosition == MousePosition.Inside)
            {
                if (CurrentMouseState.LeftButton == ButtonState.Pressed &&
                    PreviousMouseState.LeftButton == ButtonState.Released)
                {
                    Active = true;
                }

                if (CurrentMouseState.RightButton == ButtonState.Pressed &&
                    PreviousMouseState.RightButton == ButtonState.Released && CanBeRightClicked == true)
                {
                    Active = true;
                }
            }

            if (PreviousMousePosition == MousePosition.Inside && 
                CurrentMousePosition == MousePosition.Outside)
            {
                Active = false;
            }

            if (PreviousMousePosition == MousePosition.Outside &&
                CurrentMousePosition == MousePosition.Inside && 
                PlayHover == false &&
                GetColor() != Color.Transparent)
            {
                PlayHover = true;
            }
            else
            {
                PlayHover = false;
            }            

            if (Active == true)
            {
                if (DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)) &&
                    CurrentMouseState.LeftButton == ButtonState.Released &&
                    PreviousMouseState.LeftButton == ButtonState.Pressed
                    )
                {
                    if (ButtonActive == true && GetColor() != Color.Transparent)
                    {
                        JustClicked = true;                        
                    }
                }
            }

            if (PreviousMouseState.LeftButton == ButtonState.Released && CurrentMouseState.LeftButton == ButtonState.Released)
            {
                JustClicked = false;
            }

            if (DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)))
            {
                if (GetColor() != Color.Transparent)
                {
                    if (CurrentMouseState.LeftButton == ButtonState.Pressed)
                    {
                        if (PreviousMouseState.LeftButton == ButtonState.Released)
                            CurrentButtonState = ButtonSpriteState.Pressed;
                    }

                    if (CurrentMouseState.LeftButton == ButtonState.Released)
                    {
                        if (CanBeRightClicked == true)
                        {
                            if (CurrentMouseState.RightButton == ButtonState.Released)
                                CurrentButtonState = ButtonSpriteState.Hover;
                        }
                        else
                        {
                            CurrentButtonState = ButtonSpriteState.Hover;
                        }
                    }

                    if (CanBeRightClicked == true)
                        if (CurrentMouseState.RightButton == ButtonState.Pressed)
                        {
                            if (PreviousMouseState.RightButton == ButtonState.Released)
                                CurrentButtonState = ButtonSpriteState.Pressed;
                        }
                }
                else
                {
                    CurrentButtonState = ButtonSpriteState.Released;
                }
            }
            else
            {
                CurrentButtonState = ButtonSpriteState.Released;
            }

            #region Right Click
            if (Active == true && CanBeRightClicked == true)
            {
                if (DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)) &&
                    CurrentMouseState.RightButton == ButtonState.Released &&
                    PreviousMouseState.RightButton == ButtonState.Pressed
                    )
                {
                    if (ButtonActive == true && GetColor() != Color.Transparent)
                        JustRightClicked = true;
                }
            }

            if (PreviousMouseState.RightButton == ButtonState.Released
                && CurrentMouseState.RightButton == ButtonState.Released
                && CanBeRightClicked == true)
            {
                JustRightClicked = false;
            }
            #endregion

            PreviousMousePosition = CurrentMousePosition;
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
            Rect =  new Rectangle((int)((1 / Scale.X) * ((int)CursorPosition.X - CurrentPosition.X)), (int)((1 / Scale.Y) * ((int)CursorPosition.Y - CurrentPosition.Y)), 1, 1);

            if (CurrentMousePosition == MousePosition.Inside && 
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
            CurrentMousePosition = MousePosition.Outside;
            SourceRectangle = new Rectangle(CurrentFrame * (int)FrameSize.X, 0, (int)FrameSize.X, (int)FrameSize.Y);
        }
    }
}
