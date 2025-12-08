using Fusion;
using UnityEngine;

public class FusionLauncher : MonoBehaviour
{
    private NetworkRunner runner;
    public NetworkObject planePrefab;

    async void Start()
    {
        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;

        var startArgs = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "ARSession",
            Scene = null,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        };

        Debug.Log("Starting Fusion...");

        var result = await runner.StartGame(startArgs);

        if (!result.Ok)
            Debug.LogError("Fusion failed: " + result.ShutdownReason);
        else
            Debug.Log("Fusion started and joined session");
    }
}
