using System.Net;
using System.Net.Sockets;
using Asteroids.Game.Unity.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Asteroids.Game.Unity.MainMenu.Activities
{
    internal sealed class JoinToMatchActivity : Activity
    {
        private readonly MainMenuManager mainMenuManager;

        private readonly InputField serverAddressField;

        private readonly Button joinButton;

        internal JoinToMatchActivity(MainMenuManager mainMenuManager, GameObject rootObject) : base("JoinToMatch", rootObject)
        {
            this.mainMenuManager = mainMenuManager;

            var windowRoot = rootObject.transform.Find("Window");

            serverAddressField = windowRoot.transform.Find<InputField>("ServerAddressField");
            serverAddressField.onValueChanged.AddListener(UpdateJoinButtonState);

            windowRoot.transform.Find<Button>("CloseButton").onClick.AddListener(OnCloseButtonClick);

            joinButton = windowRoot.transform.Find<Button>("JoinButton");
            joinButton.onClick.AddListener(OnJoinButtonClick);
            joinButton.interactable = false;

            UpdateJoinButtonState(serverAddressField.text);
        }

        private void UpdateJoinButtonState(string serverAddressFieldValue)
        {
            joinButton.interactable = IPAddress.TryParse(serverAddressField.text, out var address) && address.AddressFamily == AddressFamily.InterNetwork;
        }

        private void OnCloseButtonClick()
        {
            mainMenuManager.userInterfaceManager.HideCurrentActivity();
        }

        private void OnJoinButtonClick()
        {
            Main.LoadMultiplayerMatch(serverAddressField.text);
        }
    }
}