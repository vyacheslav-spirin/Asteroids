using System;
using System.Collections;
using Asteroids.Game.Unity.MainMenu;
using Asteroids.Game.Unity.Match;
using Asteroids.Game.Unity.Match.Multiplayer;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Asteroids.Game.Unity
{
    public sealed class Main : MonoBehaviour
    {
        private static Main instance;

        [UsedImplicitly]
        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);

                return;
            }

            DontDestroyOnLoad(gameObject);

            instance = this;
        }

        [UsedImplicitly]
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance.DestroyCurrentObjects();

                instance = null;
            }
        }


        public GameState GameState { get; private set; } = GameState.Init;

        private MainMenuManager mainMenuManager;

        private UnityMatchManager unityMatchManager;

        private void DestroyCurrentObjects()
        {
            mainMenuManager = null;

            if (unityMatchManager != null)
            {
                unityMatchManager.OnDestroy();

                unityMatchManager = null;
            }
        }

        public static void LoadMainMenu()
        {
            if ((instance.GameState & GameState.Loading) != 0) throw new Exception("Other loading process already in progress!");

            instance.StartCoroutine(instance.LoadingMainMenuProcess());
        }

        private IEnumerator LoadingMainMenuProcess()
        {
            DestroyCurrentObjects();

            GameState = GameState.Loading | GameState.MainMenu;

            SceneManager.LoadScene("Loading", LoadSceneMode.Single);

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);

            yield return new WaitForEndOfFrame();

            mainMenuManager = new MainMenuManager();

            GameState &= ~GameState.Loading;
        }

        public static void LoadMultiplayerMatch(string serverAddress)
        {
            if ((instance.GameState & GameState.Loading) != 0) throw new Exception("Other loading process already in progress!");

            instance.StartCoroutine(instance.LoadingMatchProcess(
                serverAddress == null ? MatchType.MultiplayerServer : MatchType.MultiplayerClient, serverAddress));
        }

        private enum MatchType
        {
            MultiplayerServer,
            MultiplayerClient
        }

        private IEnumerator LoadingMatchProcess(MatchType matchType, string serverAddress)
        {
            DestroyCurrentObjects();

            GameState = GameState.Loading | GameState.Match;

            SceneManager.LoadScene("Loading", LoadSceneMode.Single);

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            SceneManager.LoadScene("Match", LoadSceneMode.Single);
            SceneManager.LoadScene("HUD", LoadSceneMode.Additive);

            yield return new WaitForEndOfFrame();

            switch (matchType)
            {
                case MatchType.MultiplayerServer:
                    unityMatchManager = new UnityMatchManagerServer();
                    break;
                case MatchType.MultiplayerClient:
                    unityMatchManager = new UnityMatchManagerClient(serverAddress);
                    break;
                default:
                    throw new Exception("Unknown match type!");
            }

            GameState &= ~GameState.Loading;
        }

        [UsedImplicitly]
        private void Update()
        {
            if (GameState == GameState.Init)
            {
                LoadMainMenu();

                return;
            }

            unityMatchManager?.Update();

            mainMenuManager?.Update();
        }
    }
}