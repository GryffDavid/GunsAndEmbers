using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class UISlopedBar
    {
        public List<Quad> QuadList = new List<Quad>();
        public float MaxValue, CurrentValue, PreviousValue;
        public Vector2 MaxSize, Position;
        Color HalfWhite = Color.Lerp(Color.Black, Color.Transparent, 0.5f);
        Color BarColor;
        float PercentValue;
        public bool DrawMarker, Pulsing;

        public float CurrentPulseTime, MaxPulseTime, TopXOffset, BottomXOffset;
        public Vector2 CurrentScale;

        public UISlopedBar(Vector2 position, Vector2 maxSize, Color Color, bool? drawMarker = false, float? topXOffset = 0, float? bottomXOffset = 0)
        {
            Position = position;
            MaxSize = maxSize;          
            BarColor = Color;
            DrawMarker = drawMarker.Value;
            CurrentScale = new Vector2(0, 0);
            Pulsing = false;

            MaxPulseTime = 1000;
            CurrentPulseTime = 0;

            TopXOffset = topXOffset.Value;
            BottomXOffset = bottomXOffset.Value;

            Quad BackgroundQuad = new Quad(
                new Vector2[] 
                {
                    new Vector2(Position.X + BottomXOffset, Position.Y + MaxSize.Y),
                    new Vector2(Position.X + TopXOffset, Position.Y),
                    new Vector2(Position.X + MaxSize.X - MaxSize.Y, Position.Y + MaxSize.Y),
                    new Vector2(Position.X + MaxSize.X, Position.Y)
                },
                new Color[]
                {
                    HalfWhite,
                    HalfWhite,
                    HalfWhite,
                    HalfWhite
                });
            QuadList.Add(BackgroundQuad);

            Quad ValueQuad = new Quad(
                new Vector2[] 
                {
                    new Vector2(Position.X + BottomXOffset, Position.Y + MaxSize.Y),
                    new Vector2(Position.X + TopXOffset, Position.Y),
                    new Vector2(Position.X + MaxSize.X - MaxSize.Y, Position.Y + MaxSize.Y),
                    new Vector2(Position.X + MaxSize.X, Position.Y)
                },
                new Color[]
                {
                    BarColor,
                    BarColor,
                    BarColor,
                    BarColor
                });

            //The actual value quad
            QuadList.Add(ValueQuad);

            //The little white bar that indicates a change
            QuadList.Add(ValueQuad);

            //The pulsing bar that increases in size
            QuadList.Add(ValueQuad);
        }

        public void Update(float maxValue, float currentValue, GameTime gameTime)
        {
            CurrentValue = currentValue;

            if (PreviousValue < CurrentValue &&
                ((100 / maxValue * currentValue) / 100) == 1)
            {
                Pulsing = true;
            }

            if (PreviousValue != CurrentValue)
            {
                PercentValue = (100 / maxValue * currentValue) / 100;

                QuadList[1] = new Quad(
                    new Vector2[] 
                {
                    new Vector2(
                        Position.X + BottomXOffset, 
                        Position.Y + MaxSize.Y),
                    new Vector2(
                        Position.X + TopXOffset, 
                        Position.Y),
                    new Vector2(
                        MathHelper.Clamp((Position.X + (MaxSize.X * PercentValue) - MaxSize.Y), Position.X, (Position.X + MaxSize.X - MaxSize.Y)), 
                        Position.Y + MathHelper.Clamp(MaxSize.X * PercentValue, 0, MaxSize.Y)),
                    new Vector2(
                        MathHelper.Clamp(Position.X + (MaxSize.X * PercentValue), Position.X, Position.X + MaxSize.X), 
                        Position.Y)
                },
                    new Color[]
                {
                    BarColor,
                    BarColor,
                    BarColor,
                    BarColor
                });
            }

            if (DrawMarker == true && PreviousValue != CurrentValue)
            {
                QuadList[2] = new Quad(
                    new Vector2[] 
                {
                    new Vector2(
                        MathHelper.Clamp((Position.X + (MaxSize.X * PercentValue) - MaxSize.Y), Position.X, (Position.X + MaxSize.X - MaxSize.Y)) - 3, 
                        Position.Y + MathHelper.Clamp(MaxSize.X * PercentValue, 0, MaxSize.Y)),
                    new Vector2(
                        MathHelper.Clamp(Position.X + (MaxSize.X * PercentValue), Position.X, Position.X + MaxSize.X) - 3, 
                        Position.Y),
                    new Vector2(
                        MathHelper.Clamp((Position.X + (MaxSize.X * PercentValue) - MaxSize.Y), Position.X, (Position.X + MaxSize.X - MaxSize.Y)), 
                        Position.Y + MathHelper.Clamp(MaxSize.X * PercentValue, 0, MaxSize.Y)),
                    new Vector2(
                        MathHelper.Clamp(Position.X + (MaxSize.X * PercentValue), Position.X, Position.X + MaxSize.X), 
                        Position.Y)
                },
            new Color[]
                {
                    Color.White,
                    Color.White,
                    Color.White,
                    Color.White
                });
            }

            if (Pulsing == true)
            {
                CurrentPulseTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (CurrentPulseTime > MaxPulseTime)
                {
                    CurrentPulseTime = 0;
                    Pulsing = false;
                    CurrentScale = new Vector2(0, 0);
                    return;
                }

                CurrentScale += Vector2.One / ((float)gameTime.ElapsedGameTime.TotalMilliseconds * 60.0f);

                QuadList[3] = new Quad(
                new Vector2[] 
                {
                    new Vector2(Position.X - CurrentScale.X, Position.Y + MaxSize.Y + CurrentScale.Y),
                    new Vector2(Position.X - CurrentScale.X, Position.Y - CurrentScale.Y),
                    new Vector2(Position.X + MaxSize.X - (MaxSize.Y + CurrentScale.X) + CurrentScale.X, Position.Y + MaxSize.Y + CurrentScale.Y),
                    new Vector2(Position.X + MaxSize.X + (CurrentScale.X+ CurrentScale.X), Position.Y - CurrentScale.Y)
                },
                new Color[]
                {
                    Color.Lerp(Color.Transparent, BarColor,  1f - CurrentScale.X/12),
                    Color.Lerp(Color.Transparent, BarColor,  1f - CurrentScale.X/12),
                    Color.Lerp(Color.Transparent, BarColor,  1f - CurrentScale.X/12),
                    Color.Lerp(Color.Transparent, BarColor,  1f - CurrentScale.X/12)
                });
            }
            
            PreviousValue = CurrentValue;
        }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            QuadList[0].DrawQuad(graphicsDevice);
            QuadList[1].DrawQuad(graphicsDevice);

            //if (DrawMarker == true)
            //    QuadList[2].DrawQuad(graphicsDevice);
            //    graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, QuadList[2].Vertices, 0, 2, VertexPositionColor.VertexDeclaration);            

            //if (Pulsing == true)
            //    graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, QuadList[3].Vertices, 0, 2, VertexPositionColor.VertexDeclaration);

        }
    }
}
