using Fusion;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FusionLauncher : MonoBehaviour
{
    private NetworkRunner runner;
    public NetworkObject planePrefab;
    public Text message;
    public NetworkPrefabRef gameManagerPrefab;

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
        
        //gamestart
        /*if (result.Ok)
        {
            if (runner.IsSharedModeMasterClient)
            {
                runner.Spawn(gameManagerPrefab);
            }
        }*/

        //RpcLoadedScene();

        if (!result.Ok)
        {
            RpcServerError($"{result.ShutdownReason}");
            Debug.LogError("Fusion failed: " + result.ShutdownReason);
        }
        else
            Debug.Log("Fusion started and joined session");
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
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
        if (runner.IsSharedModeMasterClient) message.text = "Please scan the image and wait for other players!";
        else message.text = "Waiting for other players and scanning...";
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcPlacedPrefab()
    {
        if (runner.IsSharedModeMasterClient) message.text = "Object is placed! Now you can start the game or move the image.";
        else message.text = "Image was scanned! We will start very soon!";
    }
    public void SpawnGameManager()
    {
        if (runner.IsRunning)
        {
            //runner.Spawn(gameManagerPrefab);
            RpcRequestStartGame();
        }
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RpcRequestStartGame()
    {
        if (!runner.IsSharedModeMasterClient) return;

        runner.Spawn(gameManagerPrefab);
    }

}
