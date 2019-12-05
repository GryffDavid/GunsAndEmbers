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
        public bool Active, CanBeRightClicked, JustClicked, JustRightClicked;
        public bool ButtonActive;

        string Alignment;

        public Button(string assetName, Vector2 position, string iconName = null, Vector2? scale = null, 
            Color? color = null, string text = "", string fontName = "", string alignment = "Left", Color? textColor = null, bool? canBeRightClicked = null)
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

            if (canBeRightClicked == null)
            {
                CanBeRightClicked = false;
            }
            else
            {
                CanBeRightClicked = canBeRightClicked.Value;
            }

            Alignment = alignment;

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

        public virtual void Update()
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

            if (Active == true)
            {
                if (DestinationRectangle.Contains(new Point((int)CursorPosition.X, (int)CursorPosition.Y)) &&
                    CurrentMouseState.LeftButton == ButtonState.Released &&
                    PreviousMouseState.LeftButton == ButtonState.Pressed
                    )
                {
                    if (ButtonActive == true && GetColor() != Color.Transparent)
                    JustClicked = true;
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
            Color newColor, newColor2, drawColor;
            newColor = Color.Lerp(TextColor, Color.Transparent, 0.5f);
            newColor2 = TextColor;
            drawColor = TextColor;

            if (ButtonActive == true)
            {                
                spriteBatch.Draw(ButtonStrip, DestinationRectangle, SourceRectangle, Color, MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, 0.99f);

                if (IconName != null)
                {
                    if (CurrentButtonState != ButtonSpriteState.Pressed)                    
                        spriteBatch.Draw(IconTexture, IconRectangle, null, Color.White, MathHelper.ToRadians(0), Vector2.Zero, 
                            SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.Draw(IconTexture, new Rectangle(IconRectangle.X + 2, IconRectangle.Y + 2, IconRectangle.Width, 
                            IconRectangle.Height), null, Color.White, MathHelper.ToRadians(0), Vector2.Zero, SpriteEffects.None, 0.5f);              
                }

                switch (CurrentButtonState)
                {
                    case ButtonSpriteState.Pressed:
                        drawColor = newColor2;
                        break;

                    case ButtonSpriteState.Hover:
                        drawColor = newColor2;
                        break;

                    case ButtonSpriteState.Released:
                        drawColor = newColor;
                        break;
                }

                if (Text != "")
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
            Rect =  new Rectangle((int)((1 / Scale.X) * (Mouse.GetState().X - Position.X)), (int)((1 / Scale.Y) * (Mouse.GetState().Y - Position.Y)), 1, 1);

            if (CurrentMousePosition == MousePosition.Inside && 
                new Rectangle(0, 0, 1280, 720).Contains(new Point(Mouse.GetState().X, Mouse.GetState().Y)))
            {
                if (DestinationRectangle.Contains(new Point(Mouse.GetState().X, Mouse.GetState().Y)) && Rect != null && Rect.Width == 1 && Rect.Height == 1)
                {
                    //Vector2 pos = new Vector2((1 / Scale.X) * (Mouse.GetState().X - Position.X), (1 / Scale.Y) * (Mouse.GetState().Y - Position.Y));
                    //Rectangle testRect = new Rectangle((int)((1 / Scale.X) * (Mouse.GetState().X - Position.X)), (int)((1 / Scale.Y) * (Mouse.GetState().Y - Position.Y)), 1, 1);
                    ButtonStrip.GetData<Color>(0,Rect, retrievedColor, 0, 1);
                }
            }

            return retrievedColor[0];
        }
    }
}
