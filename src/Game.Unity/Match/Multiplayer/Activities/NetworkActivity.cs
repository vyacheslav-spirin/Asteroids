using Asteroids.Game.Unity.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Asteroids.Game.Unity.Match.Multiplayer.Activities
{
    internal sealed class NetworkActivity : Activity
    {
        private readonly UnityMatchManagerClient unityMatchManagerClient;

        private readonly Text message;

        internal NetworkActivity(UnityMatchManagerClient unityMatchManagerClient, GameObject activityRoot) : base("Network", activityRoot)
        {
            this.unityMatchManagerClient = unityMatchManagerClient;

            var windowRoot = rootObject.transform.Find("Window");

            message = windowRoot.Find<Text>("Message");

            windowRoot.transform.Find<Button>("MainMenuButton").onClick.AddListener(Main.LoadMainMenu);
        }

        public override void Update()
        {
            //TODO: optimize

            if (Time.unscaledTime - unityMatchManagerClient.LastDataReceiveTime > 1f)
            {
                if (!IsVisible) unityMatchManagerClient.userInterfaceManager.ShowActivity("Network");

                message.text = "Connecting to server...\n" + unityMatchManagerClient.serverAddress;
            }
            else if (unityMatchManagerClient.ServerIsFull)
            {
                if (!IsVisible) unityMatchManagerClient.userInterfaceManager.ShowActivity("Network");

                message.text = "Server is full!\nWaiting for empty slot...";
            }
            else if (IsVisible) unityMatchManagerClient.userInterfaceManager.HideCurrentActivity();
        }
    }
}