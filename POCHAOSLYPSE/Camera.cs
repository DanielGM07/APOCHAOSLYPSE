using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace POCHAOSLYPSE
{
    public class Camera
    {
        private static Camera instance;
        public static Camera Instance
        {
            get
            {
                if (instance == null) throw new InvalidOperationException("camera has not been initialized");
                return instance;
            }
        }
        private static readonly object sync = new();
        public static void Initialize(int width, int height)
        {
            if (instance != null) return;
            lock(sync)
            {
                if (instance == null)
                    instance = new Camera(width, height);
            }
        }
        public bool IsRectangleVisible(Rectangle rect)
        {
            return ViewPortRectangle.Intersects(rect);
        }
        public Rectangle ViewPortRectangle
        {
            get
            {
                if (changed)
                    UpdateMatrices();

                var tl = Vector2.Transform(Vector2.Zero, Inverse);
                var tr = Vector2.Transform(new Vector2(Viewport.Width, 0), Inverse);
                var bl = Vector2.Transform(new Vector2(0, Viewport.Height), Inverse);
                var br = Vector2.Transform(new Vector2(Viewport.Width, Viewport.Height), Inverse);

                float minX = Math.Min(Math.Min(tl.X, tr.X), Math.Min(bl.X, br.X));
                float minY = Math.Min(Math.Min(tl.Y, tr.Y), Math.Min(bl.Y, br.Y));
                float maxX = Math.Max(Math.Max(tl.X, tr.X), Math.Max(bl.X, br.X));
                float maxY = Math.Max(Math.Max(tl.Y, tr.Y), Math.Max(bl.Y, br.Y));

                int left = (int)Math.Floor(minX);
                int top = (int)Math.Floor(minY);
                int width = (int)Math.Ceiling(maxX - minX);
                int height = (int)Math.Ceiling(Math.Abs(maxY - minY));

                return new Rectangle(left, top, width, height);
            }
        }
        private Matrix matrix = Matrix.Identity;
        private Matrix inverse = Matrix.Identity;
        private bool changed;

        private Vector2 position = Vector2.Zero;
        private Vector2 zoom = Vector2.One;
        private Vector2 origin = Vector2.Zero;
        private float angle = 0;

        public Viewport Viewport;

        public Camera(int width, int height)
        {
            Viewport = new Viewport();
            Viewport.Width = width;
            Viewport.Height = height;
            UpdateMatrices();
        }

        public override string ToString()
        {
            return "Camera:\nViewport: { " + Viewport.X + ", " + Viewport.Y + ", " + Viewport.Width + ", " + Viewport.Height +
                " }\nPosition: { " + position.X + ", " + position.Y +
                " }\nOrigin: { " + origin.X + ", " + origin.Y +
                " }\nZoom: { " + zoom.X + ", " + zoom.Y +
                " }\nAngle: " + angle +
                "\nRectangle: " +ViewPortRectangle;
        }

        private void UpdateMatrices()
        {
            matrix = Matrix.Identity *
                    Matrix.CreateTranslation(new Vector3(-new Vector2((int)Math.Floor(position.X), (int)Math.Floor(position.Y)), 0)) *
                    Matrix.CreateRotationZ(angle) *
                    Matrix.CreateScale(new Vector3(zoom, 1)) *
                    Matrix.CreateTranslation(new Vector3(new Vector2((int)Math.Floor(origin.X), (int)Math.Floor(origin.Y)), 0));

            inverse = Matrix.Invert(matrix);

            changed = false;
        }

        public void CopyFrom(Camera other)
        {
            position = other.position;
            origin = other.origin;
            angle = other.angle;
            zoom = other.zoom;
            changed = true;
        }

        public Matrix Matrix
        {
            get
            {
                if (changed)
                    UpdateMatrices();
                return matrix;
            }
        }

        public Matrix Inverse
        {
            get
            {
                if (changed)
                    UpdateMatrices();
                return inverse;
            }
        }

        public Vector2 Position
        {
            get { return position; }
            set
            {
                changed = true;
                position = value;
            }
        }

        public Vector2 Origin
        {
            get { return origin; }
            set
            {
                changed = true;
                origin = value;
            }
        }

        public float X
        {
            get { return position.X; }
            set
            {
                changed = true;
                position.X = value;
            }
        }

        public float Y
        {
            get { return position.Y; }
            set
            {
                changed = true;
                position.Y = value;
            }
        }

        public float Zoom
        {
            get { return zoom.X; }
            set
            {
                changed = true;
                zoom.X = zoom.Y = value;
            }
        }
        public float Angle
        {
            get { return angle; }
            set
            {
                changed = true;
                angle = value;
            }
        }

        public float Left
        {
            get
            {
                if (changed)
                    UpdateMatrices();
                return Vector2.Transform(Vector2.Zero, Inverse).X;
            }

            set
            {
                if (changed)
                    UpdateMatrices();
                X = Vector2.Transform(Vector2.UnitX * value, Matrix).X;
            }
        }

        public float Right
        {
            get
            {
                if (changed)
                    UpdateMatrices();
                return Vector2.Transform(Vector2.UnitX * Viewport.Width, Inverse).X;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public float Top
        {
            get
            {
                if (changed)
                    UpdateMatrices();
                return Vector2.Transform(Vector2.Zero, Inverse).Y;
            }

            set
            {
                if (changed)
                    UpdateMatrices();
                Y = Vector2.Transform(Vector2.UnitY * value, Matrix).Y;
            }
        }

        public float Bottom
        {
            get
            {
                if (changed)
                    UpdateMatrices();
                return Vector2.Transform(Vector2.UnitY * Viewport.Height, Inverse).Y;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /*
         *  Utils
         */

        public void CenterOrigin()
        {
            origin = new Vector2((float)Viewport.Width / 2, (float)Viewport.Height / 2);
            changed = true;
        }

        public void RoundPosition()
        {
            position.X = (float)Math.Round(position.X);
            position.Y = (float)Math.Round(position.Y);
            changed = true;
        }

        public Vector2 ScreenToCamera(Vector2 position)
        {
            return Vector2.Transform(position, Inverse);
        }

        public Vector2 CameraToScreen(Vector2 position)
        {
            return Vector2.Transform(position, Matrix);
        }

        public void Approach(Vector2 position, float ease)
        {
            Position += (position - Position) * ease;
        }

        public void Approach(Vector2 position, float ease, float maxDistance)
        {
            Vector2 move = (position - Position) * ease;
            if (move.Length() > maxDistance)
                Position += Vector2.Normalize(move) * maxDistance;
            else
                Position += move;
        }
    }
}
