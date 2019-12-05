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
    public class TextInput
    {
        string CurrentString = "", FontName;
        KeyboardState CurrentKeyState, PreviousKeyState;
        SpriteFont Font;
        public bool Active = true;

        Vector2 Position;
        int CharacterLimit;
        Color Color;

        public TextInput(Vector2 position, int characterLimit, string fontName, Color color)
        {
            Position = position;
            CharacterLimit = characterLimit;
            FontName = fontName;
            Color = color;
        }

        public void LoadContent(ContentManager contentManager)
        {
            Font = contentManager.Load<SpriteFont>(FontName);
        }

        public void Update(GameTime gameTime)
        {
            CurrentKeyState = Keyboard.GetState();

            if (Active == true)
                foreach (Keys key in CurrentKeyState.GetPressedKeys())
                {
                    if (PreviousKeyState.IsKeyUp(key))
                    {
                        switch (key)
                        {
                            case Keys.Back:
                                if (CurrentString.Length > 0)
                                    CurrentString = CurrentString.Remove(CurrentString.Length - 1, 1);
                                break;

                            case Keys.Space:
                                CurrentString += " ";
                                break;

                            case Keys.A:
                            case Keys.B:
                            case Keys.C:
                            case Keys.D:
                            case Keys.E:
                            case Keys.F:
                            case Keys.G:
                            case Keys.H:
                            case Keys.I:
                            case Keys.J:
                            case Keys.K:
                            case Keys.L:
                            case Keys.M:
                            case Keys.N:
                            case Keys.O:
                            case Keys.P:
                            case Keys.Q:
                            case Keys.R:
                            case Keys.S:
                            case Keys.T:
                            case Keys.U:
                            case Keys.V:
                            case Keys.W:
                            case Keys.X:
                            case Keys.Y:
                            case Keys.Z:
                                if (CurrentKeyState.IsKeyDown(Keys.LeftShift) || CurrentKeyState.IsKeyDown(Keys.RightShift))
                                {
                                    if (CurrentString.Length < CharacterLimit)
                                        CurrentString += key.ToString();
                                }
                                else
                                {
                                    if (CurrentString.Length < CharacterLimit)
                                        CurrentString += key.ToString().ToLower();
                                }
                                break;
                        }
                    }
                }

            PreviousKeyState = CurrentKeyState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Font, CurrentString, Position, Color);
        }
    }
}
