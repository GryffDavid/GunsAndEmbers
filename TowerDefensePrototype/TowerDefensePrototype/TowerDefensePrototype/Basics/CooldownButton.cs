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

        Color OutlineColor, InteriorColor;

        public CooldownButton(Vector2 position, Vector2 size, float outlineThickness)
        {
            CurrentButtonState = ButtonSpriteState.Released;
            OutlineColor = Color.Lerp(Color.White, Color.Transparent, 0.25f);
            OutlineThickness = outlineThickness;
            CurrentCooldownTime = 0;
            MaxCooldownTime = 2000;

            CurrentPosition = position;
            CurrentSize = size;

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

            PreviousButtonState = CurrentButtonState;
            PreviousMouseState = CurrentMouseState;
        }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, OutlineVertices, 0, OutlineVertices.Length/3, VertexPositionColor.VertexDeclaration);
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, InteriorVertices, 0, InteriorVertices.Length / 3, VertexPositionColor.VertexDeclaration);
        }
    }
}
