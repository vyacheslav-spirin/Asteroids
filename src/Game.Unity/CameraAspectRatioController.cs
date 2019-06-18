using JetBrains.Annotations;
using UnityEngine;

namespace Asteroids.Game.Unity
{
    public sealed class CameraAspectRatioController : MonoBehaviour
    {
        public bool clearBuffer = true;

        private Camera cam;

        private int lastScreenSizeX;
        private int lastScreenSizeY;

        [UsedImplicitly]
        private void Start()
        {
            cam = GetComponent<Camera>();

            RescaleCamera();
        }

        [UsedImplicitly]
        private void Update()
        {
            RescaleCamera();
        }

        [UsedImplicitly]
        private void OnPreCull()
        {
            if (!clearBuffer) return;

            var lastRect = cam.rect;

            cam.rect = new Rect(0, 0, 1, 1);
            GL.Clear(true, true, Color.black);

            cam.rect = lastRect;
        }

        private void RescaleCamera()
        {
            if (Screen.width == lastScreenSizeX && Screen.height == lastScreenSizeY) return;

            const float targetAspect = 16f / 9f;

            var windowAspect = Screen.width / (float) Screen.height;

            var scaleHeight = windowAspect / targetAspect;

            if (scaleHeight < 1f)
            {
                var rect = cam.rect;

                rect.width = 1f;
                rect.height = scaleHeight;
                rect.x = 0f;
                rect.y = (1f - scaleHeight) / 2f;

                cam.rect = rect;
            }
            else // add pillarbox
            {
                var scaleWidth = 1f / scaleHeight;

                var rect = cam.rect;

                rect.width = scaleWidth;
                rect.height = 1f;
                rect.x = (1f - scaleWidth) / 2f;
                rect.y = 0f;

                cam.rect = rect;
            }

            lastScreenSizeX = Screen.width;
            lastScreenSizeY = Screen.height;
        }
    }
}