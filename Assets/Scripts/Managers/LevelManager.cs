using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class LevelManager : NetworkBehaviour
{
    [SerializeField] private Transform spawnPoint;

    private NetworkVariable<int> clientsLoaded = new NetworkVariable<int>(0);

    public static LevelManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetSpawnPoint()
    {
        if (!IsServer) return;

        float x_Distance = 2f;
        float y_Distance = 2f;
        float current_X = -x_Distance;
        float current_Y = y_Distance;
        var players = NetworkPlayerController.Players;
        var playerIDs = Network_MainLobbyManager.Instance.players;
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        {
            players[playerIDs[i].ClientID].transform.position = new Vector2(current_X, current_Y);

            current_X += x_Distance;
            if (current_X >= x_Distance)
            {
                current_X = -x_Distance;
                current_Y -= y_Distance;
            }
        }
    }

    

    [ServerRpc(RequireOwnership = false)]
    private void LoadCompletedServerRpc(ServerRpcParams serverRpcParams = default)
    {
        clientsLoaded.Value++;
    }
}