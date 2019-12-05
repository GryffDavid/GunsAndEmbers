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
        public Vector2 Position, Origin;
        public float Rotation;

        public Camera()
        {
            Zoom = 1.0f;
            Rotation = 0.0f;
            Position = Vector2.Zero;
            Origin = new Vector2(640, 360);
        }

        public float ZoomMethod
        {
            get { return Zoom; }
            set { Zoom = value; if (Zoom < 0.1f) Zoom = 0.1f; }
        }

        public float RotationMethod
        {
            get { return Rotation; }
            set { Rotation = value; }
        }

        public void Move(Vector2 Vector)
        {
            Position += Vector;
        }

        public Vector2 PositionMethod
        {
            get { return Position; }
            set { Position = value; }
        }

        public Matrix Transformation(GraphicsDevice graphicsDevice)
        {
            Transform = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                        Matrix.CreateTranslation(new Vector3(-Origin.X, -Origin.Y, 0)) *
                        Matrix.CreateRotationZ(Rotation) *
                        Matrix.CreateTranslation(new Vector3(Origin.X, Origin.Y, 0)) *
                        Matrix.CreateScale(new Vector3(Zoom, Zoom, 1));


            return Transform;
        }
    }
}
