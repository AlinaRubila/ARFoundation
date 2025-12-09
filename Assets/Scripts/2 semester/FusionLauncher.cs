using Fusion;
using Mono.Cecil;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FusionLauncher : MonoBehaviour
{
    private NetworkRunner runner;
    public NetworkObject planePrefab;
    public Text message;

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
        RpcLoadedScene();

        if (!result.Ok)
        {
            RpcServerError($"{result.ShutdownReason}");
            Debug.LogError("Fusion failed: " + result.ShutdownReason);
        }
        else
            Debug.Log("Fusion started and joined session");
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcStartGame()
    {
        message.text = "Game has started!";
        Debug.Log("Game was started!");
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    void RpcServerError(string errorMessage)
    {
        Debug.LogError("Ошибка сервера: " + errorMessage);
        message.text = $"An error occured: {errorMessage}";
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    void RpcLoadedScene()
    {
        if (runner.IsSharedModeMasterClient) message.text = "Pleace scan the image and wait for other players!";
        else message.text = "Waiting for other players and scanning...";
    }
    /*[Rpc(RpcSources.All, RpcTargets.All)]
    void RpcPlayerJoined()
    {
        message.text = $"Current players: {runner.ActivePlayers.Count()}";
        Debug.Log("A new player joined!");
    }*/

}
