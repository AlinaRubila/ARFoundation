using UnityEngine;
using Unity.Netcode;
public class SetworkManager : NetworkBehaviour
{
    private void Start()
    {

        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
            string serverAddress = "localhost";
            //NetworkManager.Singleton.ConnectAsync(serverAddress);
        }
    }
}
