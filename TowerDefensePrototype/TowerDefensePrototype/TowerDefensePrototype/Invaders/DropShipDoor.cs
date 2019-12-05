using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    class DropShipDoor : Drawable
    {
        public enum DoorState { Open, Closed, Opening, Closing };
        public DoorState CurrentState = DoorState.Closed;

        //Door should slide open with a puff of steam afterwards to show depressurization.
        Vector2 StartPosition, EndPosition, CurrentOffset;
        float MoveSpeed, CurrentTime;
        object Anchor; 

        public DropShipDoor(Texture2D texture, object anchor, Vector2 startPosition, Vector2 endPosition, float moveSpeed)
        {
            Texture = texture;
            Anchor = anchor;
            StartPosition = startPosition;
            EndPosition = endPosition;
            MoveSpeed = moveSpeed;
            Position = StartPosition;
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Texture.Width, (int)Texture.Height);            
        }
        
        public void Update(GameTime gameTime)
        {
            switch (CurrentState)
            {
                #region Closed
                case DoorState.Closed:
                    {
                        CurrentOffset = Vector2.Zero;
                    }
                    break; 
                #endregion
                    
                #region Opening
                case DoorState.Opening:
                    {
                        if (EndPosition.X <= CurrentOffset.X)
                        {
                            CurrentOffset.X -= 1;

                            if (CurrentOffset.X <= EndPosition.X)
                            {
                                CurrentOffset.X = EndPosition.X;
                                CurrentState = DoorState.Open;
                            }
                        }

                        if (EndPosition.X >= CurrentOffset.X)
                        {
                            CurrentOffset.X += 1;

                            if (CurrentOffset.X >= EndPosition.X)
                            {
                                CurrentOffset.X = EndPosition.X;
                                CurrentState = DoorState.Open;
                            }
                        }
                    }
                    break;
                #endregion

                #region Open
                case DoorState.Open:
                    {
                        CurrentOffset = EndPosition;

                        CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                        if (CurrentTime > 1000)
                        {
                            CurrentTime = 0;
                            CurrentState = DoorState.Closing;
                        }
                    }
                    break;
                #endregion

                #region Closing
                case DoorState.Closing:
                    {
                        if (CurrentOffset.X >= 0)
                        {
                            CurrentOffset.X -= 1;

                            if (CurrentOffset.X <= 0)
                            {
                                CurrentOffset.X = 0;
                                CurrentState = DoorState.Closed;
                            }
                        }

                        if (CurrentOffset.X <= 0)
                        {
                            CurrentOffset.X += 1;

                            if (CurrentOffset.X >= 0)
                            {
                                CurrentOffset.X = 0;
                                CurrentState = DoorState.Closed;
                            }
                        }
                    }
                    break; 
                #endregion
            }

            Position = ((Anchor as DropShip).Position + StartPosition) + CurrentOffset;


            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Texture.Width, (int)Texture.Height);

            vertices[0].Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top, 0);
            vertices[1].Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top, 0);
            vertices[2].Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top + DestinationRectangle.Height, 0);
            vertices[3].Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top + DestinationRectangle.Height, 0);
        }

        
    }
}
