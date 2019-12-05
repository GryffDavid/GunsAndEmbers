using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class UIBar
    {
        public List<Quad> QuadList = new List<Quad>();
        public float MaxValue, CurrentValue;
        Vector2 MaxSize, Position;
        Color HalfWhite = Color.Lerp(Color.Gray, Color.Transparent, 0.5f);
        Color BarColor;
        float PercentValue;

        public UIBar(Vector2 position, Vector2 maxSize, Color Color)
        {
            Position = position;
            MaxSize = maxSize;          
            BarColor = Color;
            
            Quad BackgroundQuad = new Quad(
                new Vector2[] 
                {
                    new Vector2(Position.X, Position.Y + MaxSize.Y),
                    new Vector2(Position.X, Position.Y),
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
                    new Vector2(0,0),
                    new Vector2(0,0),
                    new Vector2(0,0),
                    new Vector2(0,0)
                },
                new Color[]
                {
                    BarColor,
                    BarColor,
                    BarColor,
                    BarColor
                });
            QuadList.Add(ValueQuad);
        }

        public void Update(float maxValue, float currentValue)
        {
            PercentValue = (100/maxValue * currentValue) / 100;

            QuadList[1] = new Quad(
                new Vector2[] 
                {
                    new Vector2(Position.X, Position.Y + MaxSize.Y),
                    new Vector2(Position.X, Position.Y),
                    new Vector2(MathHelper.Clamp((Position.X + (MaxSize.X * PercentValue) - MaxSize.Y), Position.X, (Position.X + MaxSize.X - MaxSize.Y)), 
                                Position.Y + MathHelper.Clamp(MaxSize.X * PercentValue, 0, MaxSize.Y)),
                    new Vector2(MathHelper.Clamp(Position.X + (MaxSize.X * PercentValue), Position.X, Position.X + MaxSize.X), Position.Y)
                },
                new Color[]
                {
                    BarColor,
                    BarColor,
                    BarColor,
                    BarColor
                }); 
        }
    }
}
