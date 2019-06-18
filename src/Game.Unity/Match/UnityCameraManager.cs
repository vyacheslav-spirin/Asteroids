using UnityEngine;

namespace Asteroids.Game.Unity.Match
{
    internal sealed class UnityCameraManager
    {
        public bool IsEnabled
        {
            get => cameraObject.activeSelf;
            set
            {
                if (cameraObject.activeSelf == value) return;

                cameraObject.SetActive(value);
            }
        }

        private readonly GameObject cameraObject;

        private readonly Camera camera;

        internal UnityCameraManager(RepresentationMode representationMode)
        {
            cameraObject = GameObject.Find("Camera" + representationMode);
            camera = cameraObject.GetComponent<Camera>();
        }
    }
}