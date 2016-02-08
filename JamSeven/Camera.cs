using Microsoft.Xna.Framework;
using System;

namespace JamSeven
{
    public class Camera
    {

        private float zoom;
        private float rotation;
        private Vector2 position;

        public Rectangle cameraRect;

        public Camera()
        {
            zoom = 1.0f;
            rotation = 0f;
            position = new Vector2(0, 0);
            cameraRect = new Rectangle(0, 0, int.MaxValue, int.MaxValue);
        }

        public Vector2 getPosition()
        {
            return position;
        }

        public Vector2 center {
            get {
                return new Vector2(Game1.graphics.PreferredBackBufferWidth / 2, Game1.graphics.PreferredBackBufferHeight / 2);
            }
        }

        public Matrix TranslationMatrix
        {
            get
            {
                return Matrix.CreateTranslation(new Vector3(position, 0)) *
                    Matrix.CreateRotationZ(rotation) *
                    Matrix.CreateScale(zoom, zoom, 1) *
                    Matrix.CreateTranslation(new Vector3(center, 0));
            }
        }

        public void adjustRotation(float amnt)
        {
            rotation += amnt;
        }

        public void resetRotation()
        {
            rotation = 0f;
        }

        public void adjustZoom(float amnt)
        {
            if (zoom + amnt < 0) return;
            zoom += amnt;
        }
        public void resetZoom()
        {
            zoom = 1.0f;
        }

        public void setPosition(Vector2 newPos)
        {
            position = -1 * newPos;
        }

        public void setCameraBounds(Vector2 cameraXBounds, Vector2 cameraYBounds)
        {
            cameraRect = new Rectangle(-1 * (int)cameraXBounds.X, -1 * (int)cameraYBounds.X,
                -1 * ((int)cameraXBounds.Y - (int)cameraXBounds.X), -1 * ((int)cameraYBounds.Y - (int)cameraYBounds.X));
            Console.WriteLine(cameraRect + " CAMERARECT");
            Console.WriteLine(cameraRect.Contains(new Vector2(145, 320)));
        }

        public void move(Vector2 amnt)
        {
            position += amnt;
        }

        public Rectangle worldBoundary()
        {
            Vector2 corner = screenToWorld(new Vector2(0, 0));
            Point pos = new Point((int)corner.X, (int)corner.Y);
            Point size = new Point(Game1.graphics.PreferredBackBufferWidth, Game1.graphics.PreferredBackBufferHeight);
            return new Rectangle(pos, size);
        }

        public Vector2 screenToWorld(Vector2 screenCoords)
        {
            return Vector2.Transform(screenCoords, Matrix.Invert(TranslationMatrix));
        }

        public Vector2 worldToScreen(Vector2 worldCoords)
        {
            return Vector2.Transform(worldCoords, TranslationMatrix);
        }


    }
}
