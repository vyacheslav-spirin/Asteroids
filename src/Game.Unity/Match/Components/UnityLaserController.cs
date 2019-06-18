using Asteroids.Game.Match.Components;
using Asteroids.Math;
using UnityEngine;

namespace Asteroids.Game.Unity.Match.Components
{
    internal sealed class UnityLaserController : UnityComponent
    {
        private readonly LineRenderer lineRenderer;

        private readonly Material lineMaterial;

        private LaserController laserController;

        internal UnityLaserController(UnityEntity unityEntity) : base(unityEntity)
        {
            lineRenderer = unityEntity.gameObject.GetComponentInChildren<LineRenderer>();

            lineMaterial = lineRenderer.material;
        }

        internal override void OnCreate()
        {
            laserController = unityEntity.Entity.GetComponent<LaserController>();
        }

        internal override void OnDestroy()
        {
            laserController = null;
        }

        internal override void Update()
        {
            lineRenderer.SetPosition(0, new Vector3(laserController.StartPoint.x.ToFloat(), 0, laserController.StartPoint.y.ToFloat()));
            lineRenderer.SetPosition(1, new Vector3(laserController.EndPoint.x.ToFloat(), 0, laserController.EndPoint.y.ToFloat()));

            var unityMatchManager = unityEntity.unityEntityManager.unityMatchManager;

            var t = (laserController.CreationTime + LaserController.Lifetime - unityMatchManager.matchManager.CurrentTick) *
                    UnityMatchManager.TimeBetweenTicks - unityMatchManager.TicksLerp * UnityMatchManager.TimeBetweenTicks;

            var lifetimeSeconds = LaserController.Lifetime / (float) Game.Config.TicksPerSecond;

            var color = lineMaterial.GetColor(ShaderPropertyHashes._Color);

            color.a = t / lifetimeSeconds;

            lineMaterial.SetColor(ShaderPropertyHashes._Color, color);
        }
    }
}