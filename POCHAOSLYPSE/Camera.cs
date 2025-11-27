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
            lock (sync)
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

                int left   = (int)Math.Floor(minX);
                int top    = (int)Math.Floor(minY);
                int width  = (int)Math.Ceiling(maxX - minX);
                int height = (int)Math.Ceiling(Math.Abs(maxY - minY));

                return new Rectangle(left, top, width, height);
            }
        }

        private Matrix matrix  = Matrix.Identity;
        private Matrix inverse = Matrix.Identity;
        private bool   changed;

        private Vector2 position = Vector2.Zero;
        private Vector2 zoom     = Vector2.One;
        private Vector2 origin   = Vector2.Zero;
        private float   angle    = 0;

        public Viewport Viewport;

        // 🔹 Seguimiento al jugador
        private float followHorizontal;
        private float followVertical;
        private bool  followInitialized;

        // 🔹 Shake
        private float   shakeDuration;
        private float   shakeTimer;
        private float   shakeMagnitude;
        private Vector2 shakeOffset;
        private static readonly Random rng = new();

        // 🔹 Zoom suave
        private float targetZoom = 1f;
        private float zoomSpeed  = 3f;    // qué tan rápido interpola
        public  float MinZoom    = 0.5f;
        public  float MaxZoom    = 3f;

        public Camera(int width, int height)
        {
            Viewport = new Viewport
            {
                Width  = width,
                Height = height
            };

            zoom       = Vector2.One;
            targetZoom = 1f;

            UpdateMatrices();
        }

        public override string ToString()
        {
            return "Camera:\nViewport: { " + Viewport.X + ", " + Viewport.Y + ", " + Viewport.Width + ", " + Viewport.Height +
                " }\nPosition: { " + position.X + ", " + position.Y +
                " }\nOrigin: { " + origin.X + ", " + origin.Y +
                " }\nZoom: { " + zoom.X + ", " + zoom.Y +
                " }\nAngle: " + angle +
                "\nRectangle: " + ViewPortRectangle;
        }

        private void UpdateMatrices()
        {
            // Aplicar shake sobre la posición
            Vector2 posWithShake = position + shakeOffset;

            matrix = Matrix.Identity *
                     Matrix.CreateTranslation(new Vector3(-new Vector2(
                         (int)Math.Floor(posWithShake.X),
                         (int)Math.Floor(posWithShake.Y)), 0)) *
                     Matrix.CreateRotationZ(angle) *
                     Matrix.CreateScale(new Vector3(zoom, 1)) *
                     Matrix.CreateTranslation(new Vector3(new Vector2(
                         (int)Math.Floor(origin.X),
                         (int)Math.Floor(origin.Y)), 0));

            inverse = Matrix.Invert(matrix);

            changed = false;
        }

        public void CopyFrom(Camera other)
        {
            position = other.position;
            origin   = other.origin;
            angle    = other.angle;
            zoom     = other.zoom;
            targetZoom = zoom.X;
            changed  = true;
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
            get => position;
            set
            {
                changed  = true;
                position = value;
            }
        }

        public Vector2 Origin
        {
            get => origin;
            set
            {
                changed = true;
                origin  = value;
            }
        }

        public float X
        {
            get => position.X;
            set
            {
                changed    = true;
                position.X = value;
            }
        }

        public float Y
        {
            get => position.Y;
            set
            {
                changed    = true;
                position.Y = value;
            }
        }

        public float Zoom
        {
            get => zoom.X;
            set
            {
                changed = true;
                zoom.X  = zoom.Y = value;
            }
        }

        public float Angle
        {
            get => angle;
            set
            {
                changed = true;
                angle   = value;
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
            origin  = new Vector2((float)Viewport.Width / 2, (float)Viewport.Height / 2);
            changed = true;
        }

        public void RoundPosition()
        {
            position.X = (float)Math.Round(position.X);
            position.Y = (float)Math.Round(position.Y);
            changed    = true;
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

        // 🔹 Pedir un zoom objetivo
        public void SetTargetZoom(float zoomValue)
        {
            zoomValue  = MathHelper.Clamp(zoomValue, MinZoom, MaxZoom);
            targetZoom = zoomValue;
        }

        // 🔹 Actualizar zoom suave (llamar desde Update de la escena)
        public void UpdateZoom(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Zoom = MathHelper.Lerp(Zoom, targetZoom, zoomSpeed * dt);
        }

        // 🔹 Seguir al player con look-ahead y shake
        public void FollowPlayer(GameTime gameTime, Player player)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update del shake
            if (shakeTimer > 0f)
            {
                shakeTimer -= dt;
                float t = Math.Max(shakeTimer / shakeDuration, 0f);
                float currentMag = shakeMagnitude * t;

                float dx = (float)(rng.NextDouble() * 2 - 1);
                float dy = (float)(rng.NextDouble() * 2 - 1);
                Vector2 dir = new(dx, dy);
                if (dir != Vector2.Zero)
                    dir.Normalize();
                shakeOffset = dir * currentMag;

                changed = true;
            }
            else
            {
                if (shakeOffset != Vector2.Zero)
                {
                    shakeOffset = Vector2.Zero;
                    changed     = true;
                }
            }

            var bounds       = player.BoundingBox;
            var playerCenter = new Vector2(bounds.Center.X, bounds.Center.Y);

            var lookAhead = player.FacingLeft ? -200f : 200f;

            if (Math.Abs(player.Velocity.X) > 20f)
            {
                lookAhead = player.FacingLeft ? -600f : 600f;
            }

            var desiredFocus = new Vector2(playerCenter.X + lookAhead, playerCenter.Y);

            if (!followInitialized)
            {
                followHorizontal  = desiredFocus.X;
                followVertical    = desiredFocus.Y;
                followInitialized = true;
            }

            var lerpEase = 0.5f * dt * 60f;
            followHorizontal = MathHelper.Lerp(followHorizontal, desiredFocus.X, lerpEase);
            followVertical   = MathHelper.Lerp(followVertical, desiredFocus.Y, lerpEase);

            var desiredPosition = new Vector2(followHorizontal, followVertical);
            Position = Vector2.Lerp(Position, desiredPosition, 0.05f * dt * 60f);
        }

        // 🔹 Iniciar un shake
        public void Shake(float magnitude, float duration)
        {
            if (magnitude <= 0 || duration <= 0) return;

            shakeMagnitude = magnitude;
            shakeDuration  = duration;
            shakeTimer     = duration;
        }
    }
}
