using Asteroids.Game.Unity.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Asteroids.Game.Unity.MainMenu.Activities
{
    internal sealed class MainActivity : Activity
    {
        private readonly MainMenuManager mainMenuManager;

        internal MainActivity(MainMenuManager mainMenuManager, GameObject activityRoot) : base("Main", activityRoot)
        {
            this.mainMenuManager = mainMenuManager;

            rootObject.transform.Find<Button>("HostButton").onClick.AddListener(OnHostButtonClick);

            rootObject.transform.Find<Button>("JoinButton").onClick.AddListener(OnJoinButtonClick);

            rootObject.transform.Find<Button>("QuitButton").onClick.AddListener(() => { Application.Quit(0); });
        }

        private void OnHostButtonClick()
        {
            Main.LoadMultiplayerMatch(null);
        }

        private void OnJoinButtonClick()
        {
            mainMenuManager.userInterfaceManager.ShowActivity("JoinToMatch");
        }
    }
}