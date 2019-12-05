using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TowerDefensePrototype
{
    class CooldownButton
    {
        public ButtonSpriteState CurrentButtonState, PreviousButtonState;
        public Vector2 CurrentPosition, CurrentSize, IconTextureSizeOffset, IconPosition, IconSize, IconSizeOffset;
        Vector3 IconOffset = Vector3.Zero;

        VertexPositionColor[] OutlineVertices = new VertexPositionColor[16];
        int[] OutlineIndices = new int[24];

        VertexPositionColor[] InteriorVertices = new VertexPositionColor[6];
        int[] InteriorIndices = new int[6];

        VertexPositionColorTexture[] IconVertices = new VertexPositionColorTexture[6];
        int[] iconIndices = new int[6];

        float CurrentCooldownTime, MaxCooldownTime, OutlineThickness;

        Rectangle MouseRectangle;
        MouseState CurrentMouseState, PreviousMouseState;
        MousePosition CurrentMousePosition, PreviousMousePosition;
        
        Color OutlineColor, InteriorColor;
        public Color IconColor;

        Texture2D Icon;

        public bool JustClicked, CoolingDown;

        public CooldownButton(Vector2 position, Vector2 size, float outlineThickness, Texture2D icon)
        {
            CurrentButtonState = ButtonSpriteState.Released;

            Icon = icon;
            IconColor = Color.White;

            OutlineColor = Color.Lerp(Color.White, Color.Transparent, 0.5f);
            OutlineThickness = outlineThickness;

            InteriorColor = Color.Lerp(Color.Black, Color.Transparent, 0.75f);

            CurrentPosition = position;
            CurrentSize = size;

            if (Icon != null)
            {
                IconPosition = CurrentSize / 2;
                IconSize = new Vector2(Icon.Width, Icon.Height);
                IconSizeOffset = new Vector2(0, 0);
                IconTextureSizeOffset = new Vector2(0, 0);
            }

            CurrentCooldownTime = 0;
            MaxCooldownTime = 2000;
            CoolingDown = false;            

            JustClicked = false;

            MouseRectangle = new Rectangle((int)CurrentPosition.X, (int)CurrentPosition.Y, (int)CurrentSize.X, (int)CurrentSize.Y);

            #region Icon vertices
            if (Icon != null)
            {
                #region First triangle
                //Top left corner
                IconVertices[0] = new VertexPositionColorTexture(
                    new Vector3(CurrentPosition.X + IconPosition.X - IconSize.X / 2,
                                CurrentPosition.Y + IconPosition.Y - IconSize.Y / 2,
                                0) + IconOffset,
                    IconColor, new Vector2(0, 0));

                //Top right corner
                IconVertices[1] = new VertexPositionColorTexture(
                    new Vector3(CurrentPosition.X + IconPosition.X + IconSize.X / 2,
                                CurrentPosition.Y + IconPosition.Y - IconSize.Y / 2,
                                0) + IconOffset,
                    IconColor, new Vector2(1, 0));

                //Bottom right corner
                IconVertices[2] = new VertexPositionColorTexture(
                    new Vector3(CurrentPosition.X + IconPosition.X + IconSize.X / 2,
                                CurrentPosition.Y + IconPosition.Y + IconSize.Y / 2,
                                0) + IconOffset,
                    IconColor, new Vector2(1, 1));
                #endregion

                #region Second triangle
                //Bottom right corner
                IconVertices[3] = new VertexPositionColorTexture(
                    new Vector3(CurrentPosition.X + IconPosition.X + IconSize.X / 2,
                                CurrentPosition.Y + IconPosition.Y + IconSize.Y / 2,
                                0) + IconOffset,
                    IconColor, new Vector2(1, 1));

                //Bottom left corner
                IconVertices[4] = new VertexPositionColorTexture(
                    new Vector3(CurrentPosition.X + IconPosition.X - IconSize.X / 2,
                                CurrentPosition.Y + IconPosition.Y + IconSize.Y / 2,
                                0) + IconOffset,
                    IconColor, new Vector2(0, 1));

                //Top left corner
                IconVertices[5] = new VertexPositionColorTexture(
                    new Vector3(CurrentPosition.X + IconPosition.X - IconSize.X / 2,
                                CurrentPosition.Y + IconPosition.Y - IconSize.Y / 2,
                                0) + IconOffset,
                    IconColor, new Vector2(0, 0));
                #endregion
            }
            #endregion

            #region Top outline
            OutlineVertices[0] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + OutlineThickness, CurrentPosition.Y, 0),
                Color = OutlineColor
            });

            OutlineVertices[1] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X, CurrentPosition.Y, 0),
                Color = OutlineColor
            });

            OutlineVertices[2] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X, CurrentPosition.Y + OutlineThickness, 0),
                Color = OutlineColor
            });

            OutlineVertices[3] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + OutlineThickness, CurrentPosition.Y + OutlineThickness, 0),
                Color = OutlineColor
            });

            OutlineIndices[0] = 0;
            OutlineIndices[1] = 1;
            OutlineIndices[2] = 2;
            OutlineIndices[3] = 2;
            OutlineIndices[4] = 3;
            OutlineIndices[5] = 0;
            #endregion

            #region Right outline
            OutlineVertices[4] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X - OutlineThickness, CurrentPosition.Y + OutlineThickness, 0),
                Color = OutlineColor
            });

            OutlineVertices[5] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X, CurrentPosition.Y + OutlineThickness, 0),
                Color = OutlineColor
            });

            OutlineVertices[6] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X, CurrentPosition.Y + CurrentSize.Y, 0),
                Color = OutlineColor
            });

            OutlineVertices[7] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X - OutlineThickness, CurrentPosition.Y + CurrentSize.Y, 0),
                Color = OutlineColor
            });

            OutlineIndices[6] = 4;
            OutlineIndices[7] = 5;
            OutlineIndices[8] = 6;
            OutlineIndices[9] = 6;
            OutlineIndices[10] = 7;
            OutlineIndices[11] = 4;
            #endregion

            #region Bottom outline
            OutlineVertices[8] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X, CurrentPosition.Y + CurrentSize.Y - OutlineThickness, 0),
                Color = OutlineColor
            });

            OutlineVertices[9] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X - OutlineThickness, CurrentPosition.Y + CurrentSize.Y - OutlineThickness, 0),
                Color = OutlineColor
            });

            OutlineVertices[10] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X - OutlineThickness, CurrentPosition.Y + CurrentSize.Y, 0),
                Color = OutlineColor
            });

            OutlineVertices[11] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X, CurrentPosition.Y + CurrentSize.Y, 0),
                Color = OutlineColor
            });

            OutlineIndices[12] = 8;
            OutlineIndices[13] = 9;
            OutlineIndices[14] = 10;
            OutlineIndices[15] = 10;
            OutlineIndices[16] = 11;
            OutlineIndices[17] = 8;
            #endregion

            #region Left outline
            OutlineVertices[12] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X, CurrentPosition.Y, 0),
                Color = OutlineColor
            });

            OutlineVertices[13] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + OutlineThickness, CurrentPosition.Y, 0),
                Color = OutlineColor
            });

            OutlineVertices[14] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + OutlineThickness, CurrentPosition.Y + CurrentSize.Y - OutlineThickness, 0),
                Color = OutlineColor
            });

            OutlineVertices[15] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X, CurrentPosition.Y + CurrentSize.Y - OutlineThickness, 0),
                Color = OutlineColor
            });

            OutlineIndices[18] = 12;
            OutlineIndices[19] = 13;
            OutlineIndices[20] = 14;
            OutlineIndices[21] = 14;
            OutlineIndices[22] = 15;
            OutlineIndices[23] = 12;
            #endregion

            #region Interior colour
            InteriorVertices[0] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + outlineThickness, CurrentPosition.Y + outlineThickness, 0),
                Color = InteriorColor
            });

            InteriorVertices[1] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X - outlineThickness, CurrentPosition.Y + outlineThickness, 0),
                Color = InteriorColor
            });

            InteriorVertices[2] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X - outlineThickness, CurrentPosition.Y + CurrentSize.Y - outlineThickness, 0),
                Color = InteriorColor
            });

            InteriorVertices[3] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + outlineThickness, CurrentPosition.Y + CurrentSize.Y - outlineThickness, 0),
                Color = InteriorColor
            });


            InteriorIndices[0] = 0;
            InteriorIndices[1] = 1;
            InteriorIndices[2] = 2;
            InteriorIndices[3] = 2;
            InteriorIndices[4] = 3;
            InteriorIndices[5] = 0;
            #endregion
        }

        public void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            CurrentMouseState = Mouse.GetState();

            if (CoolingDown == true)
            {
                CurrentCooldownTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                float CoolDownPercentage = (100 / MaxCooldownTime * CurrentCooldownTime) / 100;
                IconSizeOffset.Y = Icon.Height - (Icon.Height * CoolDownPercentage);
                IconTextureSizeOffset.Y = IconSizeOffset.Y / Icon.Height;

                IconColor = Color.Gray;
            }

            if (CurrentCooldownTime > MaxCooldownTime)
            {
                CurrentCooldownTime = 0;
                CoolingDown = false;
            }

            #region Check if the mouse is inside the button
            if (MouseRectangle.Contains(new Point((int)cursorPosition.X, (int)cursorPosition.Y)))
            {
                CurrentButtonState = ButtonSpriteState.Hover;
            }
            else
            {
                CurrentButtonState = ButtonSpriteState.Released;
            }
            #endregion

            #region Change the outline color based on the button state
            if (CurrentButtonState == ButtonSpriteState.Hover)
            {
                OutlineColor = Color.White;
            }

            if (CurrentButtonState == ButtonSpriteState.Released)
            {
                OutlineColor = Color.Lerp(Color.White, Color.Transparent, 0.5f);
            }
            #endregion

            #region Update the vertices with the new outline color if the state changes
            if (PreviousButtonState != CurrentButtonState)
            {
                for (int i = 0; i < OutlineVertices.Length; i++)
                {
                    OutlineVertices[i].Color = OutlineColor;
                }
            }
            #endregion


            #region As the mouse is clicked down store it's state
            if (CurrentMouseState.LeftButton == ButtonState.Pressed &&
                PreviousMouseState.LeftButton == ButtonState.Released)
            {
                //When the button is clicked down, check if it was inside or outside the button
                if (MouseRectangle.Contains(new Point((int)cursorPosition.X, (int)cursorPosition.Y)))
                {
                    CurrentMousePosition = MousePosition.Inside;
                }
                else
                {
                    CurrentMousePosition = MousePosition.Outside;
                }
            }
            #endregion


            #region As the mouse is released check it's state against the stored state
            if (CurrentMouseState.LeftButton == ButtonState.Released &&
                PreviousMouseState.LeftButton == ButtonState.Pressed)
            {
                //If the mouse button was just released update the mouse position and 
                //check if the current mouse position and the previous position are both inside the button
                if (MouseRectangle.Contains(new Point((int)cursorPosition.X, (int)cursorPosition.Y)))
                {
                    CurrentMousePosition = MousePosition.Inside;
                }
                else
                {
                    CurrentMousePosition = MousePosition.Outside;
                }

                if (CurrentMousePosition == MousePosition.Inside && PreviousMousePosition == MousePosition.Inside)
                {
                    if (CoolingDown == false)
                    {
                        JustClicked = true;
                    }
                }
            }
            #endregion


            #region Update the position of the icon based on whether the button state
            if (CurrentMousePosition == MousePosition.Inside && CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                if (CoolingDown == false)
                {
                    CurrentButtonState = ButtonSpriteState.Pressed;
                    IconOffset = new Vector3(2, 2, 0);
                }
            }
            else
            {
                IconOffset = new Vector3(0, 0, 0);
            }
            #endregion

            #region Set JustClicked back to false again
            if (CurrentMouseState.LeftButton == ButtonState.Released &&
                PreviousMouseState.LeftButton == ButtonState.Released)
            {
                JustClicked = false;
            }
            #endregion

            #region Update icon position and color
            if (Icon != null)
            {
                for (int i = 0; i < IconVertices.Length; i++)
                {
                    IconVertices[i].Color = IconColor;
                }

                #region First triangle
                //Top left corner
                IconVertices[0] = new VertexPositionColorTexture(
                    new Vector3(CurrentPosition.X + IconPosition.X - IconSize.X / 2,
                                CurrentPosition.Y + IconPosition.Y - IconSize.Y / 2 + IconSizeOffset.Y,
                                0) + IconOffset,
                    IconColor, new Vector2(0, IconTextureSizeOffset.Y));

                //Top right corner
                IconVertices[1] = new VertexPositionColorTexture(
                    new Vector3(CurrentPosition.X + IconPosition.X + IconSize.X / 2,
                                CurrentPosition.Y + IconPosition.Y - IconSize.Y / 2 + IconSizeOffset.Y,
                                0) + IconOffset,
                    IconColor, new Vector2(1, IconTextureSizeOffset.Y));

                //Bottom right corner
                IconVertices[2] = new VertexPositionColorTexture(
                    new Vector3(CurrentPosition.X + IconPosition.X + IconSize.X / 2,
                                CurrentPosition.Y + IconPosition.Y + IconSize.Y / 2,
                                0) + IconOffset,
                    IconColor, new Vector2(1, 1));
                #endregion

                #region Second triangle
                //Bottom right corner
                IconVertices[3] = new VertexPositionColorTexture(
                    new Vector3(CurrentPosition.X + IconPosition.X + IconSize.X / 2,
                                CurrentPosition.Y + IconPosition.Y + IconSize.Y / 2,
                                0) + IconOffset,
                    IconColor, new Vector2(1, 1));

                //Bottom left corner
                IconVertices[4] = new VertexPositionColorTexture(
                    new Vector3(CurrentPosition.X + IconPosition.X - IconSize.X / 2,
                                CurrentPosition.Y + IconPosition.Y + IconSize.Y / 2,
                                0) + IconOffset,
                    IconColor, new Vector2(0, 1));

                //Top left corner
                IconVertices[5] = new VertexPositionColorTexture(
                    new Vector3(CurrentPosition.X + IconPosition.X - IconSize.X / 2,
                                CurrentPosition.Y + IconPosition.Y - IconSize.Y / 2 + IconSizeOffset.Y,
                                0) + IconOffset,
                    IconColor, new Vector2(0, IconTextureSizeOffset.Y));
                #endregion
            }
            #endregion

            PreviousMousePosition = CurrentMousePosition;
            PreviousButtonState = CurrentButtonState;
            PreviousMouseState = CurrentMouseState;
        }

        public void Draw(BasicEffect basicEffect, GraphicsDevice graphicsDevice)
        {
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                //Draw the outline
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, OutlineVertices, 0, OutlineVertices.Length,
                    OutlineIndices, 0, OutlineIndices.Length / 3, VertexPositionColor.VertexDeclaration);

                //Draw the interior
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, InteriorVertices, 0, InteriorVertices.Length,
                    InteriorIndices, 0, InteriorIndices.Length / 3, VertexPositionColor.VertexDeclaration);
            }

            #region Draw the icon
            if (Icon != null)
            {
                basicEffect.Texture = Icon;
                basicEffect.TextureEnabled = true;

                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, IconVertices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                }
            }
            #endregion

            basicEffect.TextureEnabled = false;
        }
    }
}
