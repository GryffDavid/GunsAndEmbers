using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TowerDefensePrototype
{
    public class Light
    {
        public Texture2D LightTexture;
        public float Range;
        public float Radius;

        public Vector3 Position;
        public Color Color;
        public float Power;
        public int LightDecay;
        public bool Active;
        public float Depth, CurrentTime, MaxTime;
        public object Tether;

        public Light()
        {
            //Range = 500;
            //Radius = 250;
        }

        public void Update()
        {
            Depth = (Position.Y / 1080f);
        }
    }
}
