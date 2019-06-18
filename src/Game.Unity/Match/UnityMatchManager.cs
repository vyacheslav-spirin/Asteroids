using Asteroids.Game.Match;
using Asteroids.Game.Match.Components;
using Asteroids.Game.Unity.Match.Activities;
using Asteroids.Game.Unity.UI;
using UnityEngine;

namespace Asteroids.Game.Unity.Match
{
    internal abstract class UnityMatchManager
    {
        internal const float TimeBetweenTicks = 1f / Game.Config.TicksPerSecond;

        internal float TicksLerp => ticksLerp;
        protected float ticksLerp;

        internal uint LocalPlayerId => localPlayerId;
        protected uint localPlayerId;

        internal readonly MatchManager matchManager;

        private readonly UnityEntityManager unityEntityManager2D;
        private readonly UnityEntityManager unityEntityManager3D;

        private readonly UnityCameraManager unityCameraManager2D;
        private readonly UnityCameraManager unityCameraManager3D;

        internal readonly UserInterfaceManager userInterfaceManager;

        private RepresentationMode currentRepresentationMode = RepresentationMode.Mode3D;

        internal UnityMatchManager()
        {
            matchManager = new MatchManager();

            unityEntityManager2D = new UnityEntityManager(this, RepresentationMode.Mode2D);
            unityEntityManager3D = new UnityEntityManager(this, RepresentationMode.Mode3D);

            unityCameraManager2D = new UnityCameraManager(RepresentationMode.Mode2D);
            unityCameraManager3D = new UnityCameraManager(RepresentationMode.Mode3D);

            userInterfaceManager = new UserInterfaceManager(GameObject.Find("Canvas"));

            var hudActivity = new HudActivity(this, userInterfaceManager.canvasObject.transform.Find("HUD").gameObject);
            userInterfaceManager.RegisterActivity(hudActivity);
            userInterfaceManager.ShowActivity(hudActivity.name);

            UpdateRepresentationMode();

            foreach (var poolItem in ExamplePoolConfig.Pools)
            {
                matchManager.entityManager.InitPool(poolItem.name, poolItem.count);

                unityEntityManager2D.InitPool(poolItem.name, poolItem.count);
                unityEntityManager3D.InitPool(poolItem.name, poolItem.count);
            }
        }

        internal virtual void OnDestroy()
        {
            userInterfaceManager.OnDestroy();
        }

        private void UpdateRepresentationMode()
        {
            unityCameraManager2D.IsEnabled = currentRepresentationMode == RepresentationMode.Mode2D;
            unityCameraManager3D.IsEnabled = currentRepresentationMode == RepresentationMode.Mode3D;

            unityEntityManager2D.isEnabled = currentRepresentationMode == RepresentationMode.Mode2D;
            unityEntityManager3D.isEnabled = currentRepresentationMode == RepresentationMode.Mode3D;
        }

        private void UpdateLocalInput()
        {
            var player = matchManager.playerManager.GetPlayerById(localPlayerId);
            if (player == null) return;

            var playerSpaceship = matchManager.entityManager.TryGetEntity(player.SpaceshipPointer);

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (playerSpaceship == null) SendLocalPlayerCommand(new Commands.Respawn(), true);
            }

            if (playerSpaceship != null)
            {
                var move = 0;
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) move = 1;
                else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) move = -1;

                var rotation = 0;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) rotation = 1;
                else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) rotation = -1;

                var controls = SpaceshipController.PackControls(move, rotation, Input.GetKey(KeyCode.Space), Input.GetKey(KeyCode.E));

                SendLocalPlayerCommand(new Commands.SetSpaceshipControls {shipEntityPointer = player.SpaceshipPointer, controls = controls}, false);
            }
        }

        protected abstract void SendLocalPlayerCommand(Commands.Command command, bool isReliable);

        internal virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                currentRepresentationMode = currentRepresentationMode == RepresentationMode.Mode2D ? RepresentationMode.Mode3D : RepresentationMode.Mode2D;

                UpdateRepresentationMode();
            }

            unityEntityManager2D.Update();
            unityEntityManager3D.Update();

            userInterfaceManager.Update();

            UpdateLocalInput();
        }
    }
}