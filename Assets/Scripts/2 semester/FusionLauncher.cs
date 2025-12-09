using Fusion;
using System.Linq;
using UnityEngine;

public class FusionLauncher : MonoBehaviour
{
    private NetworkRunner runner;
    public NetworkObject planePrefab;

    async void Start()
    {
        runner = GetComponent<NetworkRunner>();
        if (runner == null)
        {
            Debug.LogError("NO NETWORK RUNNER FOUND!");
        }

        runner.ProvideInput = true;

        var sceneManager = GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null)
        {
            Debug.LogError("NO NETWORK SCENE MANAGER FOUND!");
        }

        var startArgs = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "ARSession",
            Scene = null,
            SceneManager = sceneManager
        };


        Debug.Log("Starting Fusion...");
        Debug.Log("Runner session players = " + runner.ActivePlayers.Count());


        var result = await runner.StartGame(startArgs);

        if (!result.Ok)
            Debug.LogError("Fusion failed: " + result.ShutdownReason);
        else
            Debug.Log("Fusion started and joined session");
    }
}
