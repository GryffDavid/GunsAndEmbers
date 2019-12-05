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
    public class Tabs
    {
        class Tab
        {
            public int Index;
            public Vector2 Position, Size;
            public Rectangle DestinationRectangle;
            public string Text;
            public Color Color = Color.Transparent;
            public MousePosition CurrentMousePosition, PreviousMousePosition;
            public ButtonSpriteState CurrentTabState;
        }

        Vector2 TabsPosition, BoxSize;
        Texture2D BoxTexture;
        SpriteFont Font;
        List<string> TabNames = new List<string>();
        public int SelectedIndex;

        List<Tab> TabList = new List<Tab>();

        MouseState CurrentMouseState, PreviousMouseState;        

        public Tabs(Vector2 position, Texture2D boxTexture, SpriteFont font, Vector2? size = null, params string[] tabNames)
        {          
            TabsPosition = position;
            SelectedIndex = 0;
            Font = font;
            TabNames = tabNames.ToList();
            BoxTexture = boxTexture;

            if (size != null)
            {
                BoxSize = size.Value;
            }
            else
            {
                BoxSize = new Vector2(1920, 48);
            }

            for (int i = 0; i < tabNames.Length; i++)
            {
                TabList.Add(new Tab()
                {
                    Position = TabsPosition + (i * new Vector2(128, 0)),
                    Index = i,
                    Size = new Vector2(128, 48), 
                    Text = TabNames[i]
                });
            }

            foreach (Tab tab in TabList)
            {
                tab.DestinationRectangle = new Rectangle((int)tab.Position.X, (int)tab.Position.Y, (int)tab.Size.X, (int)tab.Size.Y);
            }
        }

        public void Update(GameTime gameTime)
        {
            CurrentMouseState = Mouse.GetState();

            foreach (Tab tab in TabList)
            {
                #region Check if a tab has been clicked
                if (tab.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                {
                    tab.CurrentTabState = ButtonSpriteState.Hover;
                }
                else
                {
                    tab.CurrentTabState = ButtonSpriteState.Released;
                }

                #region Check whether the mouse is inside the button as it's pressed and store that state
                if (CurrentMouseState.LeftButton == ButtonState.Pressed &&
                    PreviousMouseState.LeftButton == ButtonState.Released)
                {
                    if (tab.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                    {
                        tab.CurrentMousePosition = MousePosition.Inside;
                    }
                    else
                    {
                        tab.CurrentMousePosition = MousePosition.Outside;
                    }
                }
                #endregion

                #region Compare the previous position of the mouse with the current position, if they're both inside the tab, it has been clicked
                if (CurrentMouseState.LeftButton == ButtonState.Released &&
                    PreviousMouseState.LeftButton == ButtonState.Pressed)
                {
                    if (tab.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                    {
                        tab.CurrentMousePosition = MousePosition.Inside;
                    }
                    else
                    {
                        tab.CurrentMousePosition = MousePosition.Outside;
                    }

                    if (tab.CurrentMousePosition == MousePosition.Inside && tab.PreviousMousePosition == MousePosition.Inside)
                    {
                        SelectedIndex = TabList.IndexOf(tab);
                    }
                }
                #endregion

                tab.PreviousMousePosition = tab.CurrentMousePosition;
                #endregion
            }

            PreviousMouseState = CurrentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch, BasicEffect basicEffect, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Draw(BoxTexture, new Rectangle((int)TabsPosition.X, (int)TabsPosition.Y, (int)BoxSize.X, (int)BoxSize.Y), Color.Lerp(Color.Black, Color.Transparent, 0.75f));


            foreach (Tab tab in TabList)
            {
                if (tab.Index == SelectedIndex)
                {
                    spriteBatch.Draw(BoxTexture, new Rectangle((int)tab.Position.X, (int)(tab.Position.Y + tab.Size.Y - 4), (int)tab.Size.X, 4), Color.White);
                }

                if (tab.CurrentTabState == ButtonSpriteState.Hover)
                {
                    tab.Color = Color.Lerp(Color.White, Color.Transparent, 0.5f);
                    spriteBatch.DrawString(Font, tab.Text, new Vector2(tab.Position.X + tab.Size.X / 2, tab.Position.Y + tab.Size.Y / 2), Color.White, 0, new Vector2(Font.MeasureString(tab.Text).X / 2, Font.MeasureString(tab.Text).Y / 2), 1f, SpriteEffects.None, 0);
                }
                else
                {
                    if (tab.Index == SelectedIndex)
                    {
                        spriteBatch.DrawString(Font, tab.Text, new Vector2(tab.Position.X + tab.Size.X / 2, tab.Position.Y + tab.Size.Y / 2), Color.White, 0, new Vector2(Font.MeasureString(tab.Text).X / 2, Font.MeasureString(tab.Text).Y / 2), 1f, SpriteEffects.None, 0);
                    }
                    else
                    {
                        spriteBatch.DrawString(Font, tab.Text, new Vector2(tab.Position.X + tab.Size.X / 2, tab.Position.Y + tab.Size.Y / 2), Color.Lerp(Color.White, Color.Transparent, 0.5f), 0, new Vector2(Font.MeasureString(tab.Text).X / 2, Font.MeasureString(tab.Text).Y / 2), 1f, SpriteEffects.None, 0);
                    }

                    tab.Color = Color.Transparent;
                }

                spriteBatch.Draw(BoxTexture, new Rectangle((int)tab.Position.X, (int)tab.Position.Y, (int)tab.Size.X, (int)tab.Size.Y), tab.Color);
            }
        }
    }
}
