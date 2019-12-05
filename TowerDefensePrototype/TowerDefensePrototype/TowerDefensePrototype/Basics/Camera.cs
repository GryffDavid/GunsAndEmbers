using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefensePrototype
{
    public class Camera
    {
        protected float Zoom;
        public Matrix Transform;
        public Vector2 Position;
        protected float Rotation;

        public Camera()
        {
            Zoom = 1;
            Rotation = 0;
            Position = new Vector2(-640,-360);
        }

        public float ZoomCamera
        {
            get { return Zoom; }
            set { Zoom = value; if (Zoom < 0.1f) Zoom = 0.1f; }
        }

        public float RotateCamera
        {
            get { return Rotation; }
            set { Rotation = value; }
        }

        public Vector2 PositionCamera
        {
            get { return Position; }
            set { Position = value; }
        }

        public Matrix Transformation(GraphicsDevice graphicsDevice)
        {
            Transform = 
                Matrix.CreateTranslation(new Vector3(Position.X, Position.Y, 0)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0));              

            return Transform;
        }
    }
}
