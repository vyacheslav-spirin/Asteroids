using System;
using Asteroids.Math;
using UnityEngine;

namespace Asteroids.Game.Unity.Match.Components
{
    internal class UnityTransform : UnityComponent
    {
        private readonly Renderer renderer;

        private byte rendererState;

        private Game.Match.Components.Transform transform;

        private UnityEngine.Vector2 oldTickPosition;
        private float oldTickRotation;

        private UnityEngine.Vector2 currentTickPosition;
        private float currentTickRotation;

        private uint lastTeleportationCount;

        internal UnityTransform(UnityEntity unityEntity, bool invisibleOnFirstTick = false) : base(unityEntity)
        {
            if (invisibleOnFirstTick) renderer = unityEntity.gameObject.GetComponentInChildren<Renderer>();
        }

        internal override void OnCreate()
        {
            transform = unityEntity.Entity.GetComponent<Game.Match.Components.Transform>();

            if (transform == null) throw new Exception("Could not find base component!");

            oldTickPosition = currentTickPosition = Convert.Vector2(transform.Position);
            oldTickRotation = currentTickRotation = transform.Rotation.ToFloat().RadToUnityDeg();

            lastTeleportationCount = transform.TeleportationCount;

            if (renderer != null)
            {
                renderer.enabled = false;

                rendererState = 0;
            }
        }

        internal override void OnDestroy()
        {
            transform = null;
        }

        internal override void TickChanged()
        {
            if (renderer != null)
            {
                if (rendererState == 0) rendererState++;
                else if (rendererState == 1)
                {
                    rendererState = 2;

                    renderer.enabled = true;
                }
            }

            ForceSyncTransform();
        }

        private void ForceSyncTransform()
        {
            oldTickPosition = currentTickPosition;
            oldTickRotation = currentTickRotation;
            currentTickPosition = Convert.Vector2(transform.Position);
            currentTickRotation = transform.Rotation.ToFloat().RadToUnityDeg();
        }

        internal override void Update()
        {
            var gameObjectTransform = unityEntity.gameObject.transform;
            if (gameObjectTransform.parent != null) return;

            float t;

            if (lastTeleportationCount == transform.TeleportationCount)
            {
                t = unityEntity.unityEntityManager.unityMatchManager.TicksLerp;
            }
            else
            {
                lastTeleportationCount = transform.TeleportationCount;

                ForceSyncTransform();

                t = 1;
            }

            var currentFramePos = UnityEngine.Vector2.LerpUnclamped(oldTickPosition, currentTickPosition, t);

            gameObjectTransform.localPosition = new Vector3(currentFramePos.x, 0, currentFramePos.y);

            var currentFrameRot = Utility.LerpAngleUnclamped(oldTickRotation, currentTickRotation, t);

            gameObjectTransform.localRotation = Quaternion.Euler(0, currentFrameRot, 0);
        }
    }
}