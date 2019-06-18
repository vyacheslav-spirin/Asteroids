using Asteroids.Game.Unity.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Asteroids.Game.Unity.Match.Activities
{
    internal sealed class HudActivity : Activity
    {
        private readonly UnityMatchManager unityMatchManager;

        private readonly Text[] playerInfo;

        internal HudActivity(UnityMatchManager unityMatchManager, GameObject activityRoot) : base("HUD", activityRoot)
        {
            this.unityMatchManager = unityMatchManager;

            playerInfo = new Text[Game.Config.MaxPlayers];

            var template = rootObject.transform.Find("PlayerInfoTemplate").gameObject;

            var srcPos = template.transform.localPosition;

            for (var i = 0; i < playerInfo.Length; i++)
            {
                var instance = Object.Instantiate(template, template.transform.parent, false);
                instance.transform.localPosition = srcPos + new Vector3(i * 258, 0, 0);

                playerInfo[i] = instance.GetComponent<Text>();
            }

            Object.Destroy(template);
        }

        public override void Update()
        {
            if (!IsVisible) return;

            //Simple, non optimized HUD

            for (var i = 0; i < playerInfo.Length; i++)
            {
                if (i >= unityMatchManager.matchManager.playerManager.PlayerCount) playerInfo[i].text = "";
                else
                {
                    var player = unityMatchManager.matchManager.playerManager.GetPlayerByIndex(i);

                    //more allocations
                    var str = "PLAYER ";
                    str += i + 1;
                    str += "\nLIVES: ";
                    str += player.LivesLeft;
                    str += "\nSCORE: ";
                    str += player.Score;
                    if (player.id == unityMatchManager.LocalPlayerId &&
                        unityMatchManager.matchManager.entityManager.TryGetEntity(player.SpaceshipPointer) == null &&
                        Time.unscaledTime % 1 >= 0.5f)
                    {
                        str += "\nPRESS ENTER TO SPAWN...";
                    }

                    playerInfo[i].text = str;
                }
            }
        }
    }
}