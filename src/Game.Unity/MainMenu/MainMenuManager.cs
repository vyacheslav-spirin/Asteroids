using Asteroids.Game.Unity.MainMenu.Activities;
using Asteroids.Game.Unity.UI;
using UnityEngine;

namespace Asteroids.Game.Unity.MainMenu
{
    internal sealed class MainMenuManager
    {
        internal readonly UserInterfaceManager userInterfaceManager;

        internal MainMenuManager()
        {
            userInterfaceManager = new UserInterfaceManager(GameObject.Find("Canvas"));

            var mainActivity = new MainActivity(this, userInterfaceManager.canvasObject.transform.Find("MainActivity").gameObject);
            userInterfaceManager.RegisterActivity(mainActivity);

            userInterfaceManager.RegisterActivity(
                new JoinToMatchActivity(this, userInterfaceManager.canvasObject.transform.Find("JoinToMatchActivity").gameObject));

            userInterfaceManager.ShowActivity(mainActivity.name);
        }

        internal void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && userInterfaceManager.ActivityStackSize > 1)
            {
                userInterfaceManager.HideCurrentActivity();
            }
        }
    }
}