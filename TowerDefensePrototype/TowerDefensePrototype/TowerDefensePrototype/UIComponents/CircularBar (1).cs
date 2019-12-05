using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class CircularBar
    {
        public Texture2D Texture;
        public Vector2 Position, Size;
        public float MaxSize = 1.0f; //The total percentage that the image can show - 50% = half a circle
        public float CurrentValue, MaxValue;
        public Color BackColor, FrontColor;

        public CircularBar(Vector2 position, Vector2 size, float currentValue, float maxValue, Color backColor, Color frontColor, Texture2D texture)
        {
            Position = position;
            Size = size;
            CurrentValue = currentValue;
            MaxValue = maxValue;
            BackColor = backColor;
            FrontColor = frontColor;
            Texture = texture;
        }

        public void Update(float value, Vector2? position = null)
        {
            CurrentValue = value;

            if (position != null)
            {
                Position = position.Value;
            }
        }

        public void Draw()
        {

        }
    }
}
