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
        ButtonSpriteState CurrentButtonState, PreviousButtonState;       
        public Vector2 CurrentPosition, CurrentSize;
        VertexPositionColor[] OutlineVertices = new VertexPositionColor[24];
        VertexPositionColor[] InteriorVertices = new VertexPositionColor[6];

        float CurrentCooldownTime, MaxCooldownTime, OutlineThickness;

        Rectangle MouseRectangle;
        MouseState CurrentMouseState, PreviousMouseState;
        MousePosition CurrentMousePosition, PreviousMousePosition;
        
        Color OutlineColor, InteriorColor;
        public Color IconColor;

        Texture2D Icon;

        public bool JustClicked;

        public CooldownButton(Vector2 position, Vector2 size, float outlineThickness, Texture2D icon)
        {
            CurrentButtonState = ButtonSpriteState.Released;
            Icon = icon;
            IconColor = Color.White;
            OutlineColor = Color.Lerp(Color.White, Color.Transparent, 0.5f);
            OutlineThickness = outlineThickness;
            CurrentCooldownTime = 0;
            MaxCooldownTime = 2000;

            CurrentPosition = position;
            CurrentSize = size;

            JustClicked = false;

            MouseRectangle = new Rectangle((int)CurrentPosition.X, (int)CurrentPosition.Y, (int)CurrentSize.X, (int)CurrentSize.Y);

            #region Top outline
            OutlineVertices[0] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + outlineThickness, CurrentPosition.Y, 0),
                Color = OutlineColor
            });

            OutlineVertices[1] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X, CurrentPosition.Y, 0),
                Color = OutlineColor
            });

            OutlineVertices[2] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X, CurrentPosition.Y + outlineThickness, 0),
                Color = OutlineColor
            });


            OutlineVertices[3] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X, CurrentPosition.Y + outlineThickness, 0),
                Color = OutlineColor
            });

            OutlineVertices[4] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + outlineThickness, CurrentPosition.Y + outlineThickness, 0),
                Color = OutlineColor
            });

            OutlineVertices[5] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + outlineThickness, CurrentPosition.Y, 0),
                Color = OutlineColor
            });
            #endregion

            #region Right outline
            OutlineVertices[6] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X, CurrentPosition.Y + outlineThickness, 0),
                Color = OutlineColor
            });

            OutlineVertices[7] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X, CurrentPosition.Y + CurrentSize.Y, 0),
                Color = OutlineColor
            });

            OutlineVertices[8] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X - outlineThickness, CurrentPosition.Y + CurrentSize.Y, 0),
                Color = OutlineColor
            });

            OutlineVertices[9] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X - outlineThickness, CurrentPosition.Y + CurrentSize.Y, 0),
                Color = OutlineColor
            });

            OutlineVertices[10] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X - outlineThickness, CurrentPosition.Y + outlineThickness, 0),
                Color = OutlineColor
            });

            OutlineVertices[11] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X, CurrentPosition.Y + outlineThickness, 0),
                Color = OutlineColor
            });
            #endregion

            #region Bottom outline
            OutlineVertices[12] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X - outlineThickness, CurrentPosition.Y + CurrentSize.Y - outlineThickness, 0),
                Color = OutlineColor
            });

            OutlineVertices[13] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X - outlineThickness, CurrentPosition.Y + CurrentSize.Y, 0),
                Color = OutlineColor
            });

            OutlineVertices[14] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X, CurrentPosition.Y + CurrentSize.Y, 0),
                Color = OutlineColor
            });

            OutlineVertices[15] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X, CurrentPosition.Y + CurrentSize.Y, 0),
                Color = OutlineColor
            });

            OutlineVertices[16] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X, CurrentPosition.Y + CurrentSize.Y - outlineThickness, 0),
                Color = OutlineColor
            });

            OutlineVertices[17] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X - outlineThickness, CurrentPosition.Y + CurrentSize.Y - outlineThickness, 0),
                Color = OutlineColor
            });
            #endregion

            #region Left outline
            OutlineVertices[18] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X, CurrentPosition.Y, 0),
                Color = OutlineColor
            });

            OutlineVertices[19] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + outlineThickness, CurrentPosition.Y, 0),
                Color = OutlineColor
            });

            OutlineVertices[20] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + outlineThickness, CurrentPosition.Y + CurrentSize.Y - outlineThickness, 0),
                Color = OutlineColor
            });

            OutlineVertices[21] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + outlineThickness, CurrentPosition.Y + CurrentSize.Y - outlineThickness, 0),
                Color = OutlineColor
            });

            OutlineVertices[22] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X, CurrentPosition.Y + CurrentSize.Y - outlineThickness, 0),
                Color = OutlineColor
            });

            OutlineVertices[23] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X, CurrentPosition.Y, 0),
                Color = OutlineColor
            });
            #endregion

            #region Interior colour
            InteriorVertices[0] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + outlineThickness, CurrentPosition.Y + outlineThickness, 0),
                Color = Color.Lerp(Color.Black, Color.Transparent, 0.75f)
            });

            InteriorVertices[1] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X - outlineThickness, CurrentPosition.Y + outlineThickness, 0),
                Color = Color.Lerp(Color.Black, Color.Transparent, 0.75f)
            });

            InteriorVertices[2] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X - outlineThickness, CurrentPosition.Y + CurrentSize.Y - outlineThickness, 0),
                Color = Color.Lerp(Color.Black, Color.Transparent, 0.75f)
            });

            InteriorVertices[3] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + CurrentSize.X - outlineThickness, CurrentPosition.Y + CurrentSize.Y - outlineThickness, 0),
                Color = Color.Lerp(Color.Black, Color.Transparent, 0.75f)
            });

            InteriorVertices[4] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + outlineThickness, CurrentPosition.Y + CurrentSize.Y - outlineThickness, 0),
                Color = Color.Lerp(Color.Black, Color.Transparent, 0.75f)
            });

            InteriorVertices[5] = (new VertexPositionColor()
            {
                Position = new Vector3(CurrentPosition.X + outlineThickness, CurrentPosition.Y + outlineThickness, 0),
                Color = Color.Lerp(Color.Black, Color.Transparent, 0.75f)
            });
            #endregion
        }

        public void Update(GameTime gameTime, Vector2 cursorPosition)
        {
            CurrentMouseState = Mouse.GetState();

            //If the mouse is inside the button
            if (MouseRectangle.Contains(new Point((int)cursorPosition.X, (int)cursorPosition.Y)))
            {
                CurrentButtonState = ButtonSpriteState.Hover;
            }
            else
            {
                CurrentButtonState = ButtonSpriteState.Released;
            }

            if (CurrentButtonState == ButtonSpriteState.Hover)
            {
                OutlineColor = Color.White;
            }

            if (CurrentButtonState == ButtonSpriteState.Released)
            {
                OutlineColor = Color.Lerp(Color.White, Color.Transparent, 0.5f);
            }

            if (PreviousButtonState != CurrentButtonState)
            {
                //Update vertices
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
                    Position = new Vector3(CurrentPosition.X + CurrentSize.X, CurrentPosition.Y + OutlineThickness, 0),
                    Color = OutlineColor
                });

                OutlineVertices[4] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X + OutlineThickness, CurrentPosition.Y + OutlineThickness, 0),
                    Color = OutlineColor
                });

                OutlineVertices[5] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X + OutlineThickness, CurrentPosition.Y, 0),
                    Color = OutlineColor
                });
                #endregion

                #region Right outline
                OutlineVertices[6] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X + CurrentSize.X, CurrentPosition.Y + OutlineThickness, 0),
                    Color = OutlineColor
                });

                OutlineVertices[7] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X + CurrentSize.X, CurrentPosition.Y + CurrentSize.Y, 0),
                    Color = OutlineColor
                });

                OutlineVertices[8] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X + CurrentSize.X - OutlineThickness, CurrentPosition.Y + CurrentSize.Y, 0),
                    Color = OutlineColor
                });

                OutlineVertices[9] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X + CurrentSize.X - OutlineThickness, CurrentPosition.Y + CurrentSize.Y, 0),
                    Color = OutlineColor
                });

                OutlineVertices[10] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X + CurrentSize.X - OutlineThickness, CurrentPosition.Y + OutlineThickness, 0),
                    Color = OutlineColor
                });

                OutlineVertices[11] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X + CurrentSize.X, CurrentPosition.Y + OutlineThickness, 0),
                    Color = OutlineColor
                });
                #endregion

                #region Bottom outline
                OutlineVertices[12] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X + CurrentSize.X - OutlineThickness, CurrentPosition.Y + CurrentSize.Y - OutlineThickness, 0),
                    Color = OutlineColor
                });

                OutlineVertices[13] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X + CurrentSize.X - OutlineThickness, CurrentPosition.Y + CurrentSize.Y, 0),
                    Color = OutlineColor
                });

                OutlineVertices[14] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X, CurrentPosition.Y + CurrentSize.Y, 0),
                    Color = OutlineColor
                });

                OutlineVertices[15] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X, CurrentPosition.Y + CurrentSize.Y, 0),
                    Color = OutlineColor
                });

                OutlineVertices[16] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X, CurrentPosition.Y + CurrentSize.Y - OutlineThickness, 0),
                    Color = OutlineColor
                });

                OutlineVertices[17] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X + CurrentSize.X - OutlineThickness, CurrentPosition.Y + CurrentSize.Y - OutlineThickness, 0),
                    Color = OutlineColor
                });
                #endregion

                #region Left outline
                OutlineVertices[18] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X, CurrentPosition.Y, 0),
                    Color = OutlineColor
                });

                OutlineVertices[19] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X + OutlineThickness, CurrentPosition.Y, 0),
                    Color = OutlineColor
                });

                OutlineVertices[20] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X + OutlineThickness, CurrentPosition.Y + CurrentSize.Y - OutlineThickness, 0),
                    Color = OutlineColor
                });

                OutlineVertices[21] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X + OutlineThickness, CurrentPosition.Y + CurrentSize.Y - OutlineThickness, 0),
                    Color = OutlineColor
                });

                OutlineVertices[22] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X, CurrentPosition.Y + CurrentSize.Y - OutlineThickness, 0),
                    Color = OutlineColor
                });

                OutlineVertices[23] = (new VertexPositionColor()
                {
                    Position = new Vector3(CurrentPosition.X, CurrentPosition.Y, 0),
                    Color = OutlineColor
                });
                #endregion
            }

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
                    JustClicked = true;
                }
            }

            if (CurrentMousePosition == MousePosition.Inside && CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                CurrentButtonState = ButtonSpriteState.Pressed;
            }

            if (CurrentMouseState.LeftButton == ButtonState.Released &&
                PreviousMouseState.LeftButton == ButtonState.Released)
            {
                JustClicked = false;
            }

            PreviousMousePosition = CurrentMousePosition;
            PreviousButtonState = CurrentButtonState;
            PreviousMouseState = CurrentMouseState;
        }

        public void Draw(BasicEffect basicEffect, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, OutlineVertices, 0, OutlineVertices.Length / 3, VertexPositionColor.VertexDeclaration);
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, InteriorVertices, 0, InteriorVertices.Length / 3, VertexPositionColor.VertexDeclaration);
            }

            if (Icon != null)
            {
                if (CurrentButtonState == ButtonSpriteState.Pressed)
                {
                    spriteBatch.Draw(Icon,
                        new Rectangle((int)(CurrentPosition.X + CurrentSize.X / 2 + 3), (int)(CurrentPosition.Y + CurrentSize.Y / 2 + 3), Icon.Width, Icon.Height),
                        null,
                        IconColor,
                        0,
                        new Vector2(Icon.Width / 2, Icon.Height / 2), SpriteEffects.None, 1f);
                }
                else
                {
                    spriteBatch.Draw(Icon,
                        new Rectangle((int)(CurrentPosition.X + CurrentSize.X / 2), (int)(CurrentPosition.Y + CurrentSize.Y / 2), Icon.Width, Icon.Height),
                        null,
                        IconColor,
                        0,
                        new Vector2(Icon.Width / 2, Icon.Height / 2), SpriteEffects.None, 1f);
                }
            }
        }
    }
}
