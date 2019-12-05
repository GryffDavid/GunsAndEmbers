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
        public string CurrentString = "", RealString = "", FontName;
        KeyboardState CurrentKeyState, PreviousKeyState;
        SpriteFont Font;
        public bool Active = true;

        Vector2 Position;
        public int SpaceLimit, TypePosition, PreviousTypePosition;
        Color Color;

        public TextInput(Vector2 position, int spaceLimit, string fontName, Color color)
        {
            Position = position;
            SpaceLimit = spaceLimit;
            FontName = fontName;
            Color = color;
            TypePosition = 0;
        }

        public void LoadContent(ContentManager contentManager)
        {
            Font = contentManager.Load<SpriteFont>(FontName);
        }

        public void Update()
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
                                if (RealString.Length > 0 && TypePosition > 0)
                                {
                                    RealString = RealString.Remove(TypePosition - 1, 1);
                                    TypePosition--;
                                }
                                break;

                            case Keys.Space:
                                if (Font.MeasureString(RealString).X < SpaceLimit)
                                {
                                    RealString = RealString.Insert(TypePosition, " ");
                                    TypePosition++;
                                }
                                break;

                            case Keys.Left:
                                if (TypePosition > 0)
                                    TypePosition--;
                                break;

                            case Keys.Right:
                                if (TypePosition < RealString.Length)
                                    TypePosition++;
                                break;

                            case Keys.Home:
                                TypePosition = 0;
                                break;

                            case Keys.End:
                                TypePosition = RealString.Length;
                                break;

                            case Keys.Delete:
                                if (RealString.Length > 0 && TypePosition != RealString.Length)
                                {
                                    RealString = RealString.Remove(TypePosition, 1);
                                }
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
                                    if (Font.MeasureString(RealString).X < SpaceLimit)
                                    {
                                        RealString = RealString.Insert(TypePosition, key.ToString());
                                        TypePosition++;
                                    }
                                }
                                else
                                {
                                    if (Font.MeasureString(RealString).X < SpaceLimit)
                                    {
                                        RealString = RealString.Insert(TypePosition, key.ToString().ToLower());
                                        TypePosition++;
                                    }
                                }
                                break;
                        }
                    }
                }
            CurrentString = RealString.Insert(TypePosition, "|");

            MathHelper.Clamp(TypePosition, 0, CurrentString.Length);

            PreviousTypePosition = TypePosition;
            PreviousKeyState = CurrentKeyState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Font, CurrentString, Position, Color);
        }
    }
}
