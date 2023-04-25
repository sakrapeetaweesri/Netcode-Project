using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;

public class Network_MainLobbyManager : NetworkBehaviour
{
    public NetworkList<PlayerState> players;

    [SerializeField] private PlayerCard[] playerCards;

    [SerializeField] private TextMeshProUGUI maxPlayerText;
    [SerializeField] private TextMeshProUGUI playerReadyCountText;

    [SerializeField] private GameObject startGameButton;
    [SerializeField] private GameObject gameHud;

    [SerializeField] private Transform testSpawnPoint;

    public NetworkVariable<bool> gameStarted = new NetworkVariable<bool>();

    public static Network_MainLobbyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            players = new NetworkList<PlayerState>();

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            players.OnListChanged += HandlePlayerStateChanged;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            NetworkManager.SceneManager.OnSceneEvent += HandleSceneEvent;

            foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(player.ClientId);
            }
        }

        UpdatePlayerCard();
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            players.OnListChanged -= HandlePlayerStateChanged;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }
    private void HandleClientConnected(ulong playerID)
    {
        players.Add(new PlayerState(playerID, RelayManager.characterId));
    }
    private void HandleClientDisconnected(ulong playerID)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientID == playerID)
            {
                players.RemoveAt(i);
                break;
            }
        }

        UpdateMaxPlayer();
        UpdatePlayerCard();
    }
    private void HandlePlayerStateChanged(NetworkListEvent<PlayerState> changeEvent)
    {
        UpdatePlayerCard();

        // Manages the number of ready players
        int playersReady = GetPlayersReady();
        playerReadyCountText.SetText(playersReady.ToString());

        // Sets the start button for the host.
        if (IsServer)
        {
            startGameButton.SetActive(playersReady == NetworkManager.Singleton.ConnectedClientsList.Count);
        }
    }
    private void HandleSceneEvent(SceneEvent sceneEvent)
    {
        if (sceneEvent.Scene == SceneManager.GetSceneByBuildIndex(2))
        {
            if (IsServer && sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted)
            {
                Debug.Log("Everyone is loaded!");
            }
        }
    }

    /// <summary>
    /// Updates the max player text UI.
    /// </summary>
    public void UpdateMaxPlayer()
    {
        maxPlayerText.SetText(NetworkPlayerController.Players.Count.ToString());
    }
    /// <summary>
    /// Updates the player's information display.
    /// </summary>
    private void UpdatePlayerCard()
    {
        for (int i = 0; i < playerCards.Length; i++)
        {
            if (players.Count > i)
            {
                playerCards[i].UpdateDisplay(players[i]);

                if (IsServer && players[i].ClientID != OwnerClientId) playerCards[i].DisplayKickButton();
            }
            else
            {
                playerCards[i].DisableVisuals();
            }
        }
    }

    /// <summary>
    /// Sets the player to ready or not. Also sends to the server.
    /// </summary>
    public void SetReadyState()
    {
        // Returns if not yet connected.
        if (!RelayManager.RelayConnected) return;

        SetReadyStateServerRpc();
    }
    /// <summary>
    /// Returns the number of players that are ready.
    /// </summary>
    /// <returns>The number of players whose IsReady in PlayerState is set to true.</returns>
    private int GetPlayersReady()
    {
        int n = 0;
        foreach (var player in players)
        {
            if (player.IsReady) n++;
        }
        return n;
    }

    /// <summary>
    /// Transfers all players to the game scene.
    /// </summary>
    public void GameStartButton()
    {
        if (!IsServer) return;

        startGameButton.SetActive(false);

        foreach (var p in NetworkPlayerController.Players)
        {
            p.Value.TeleportClientRpc(testSpawnPoint.position);
        }

        SetGameHUDClientRpc(true);

        gameStarted.Value = true;
    }
    
    [ClientRpc]
    private void SetGameHUDClientRpc(bool active)
    {
        gameHud.SetActive(active);
        MainLobbyManager.Instance.mainCanvas.enabled = !active;
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetReadyStateServerRpc(ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientID == serverRpcParams.Receive.SenderClientId)
            {
                var temp = players[i];
                players[i] = new PlayerState(temp.ClientID, temp.CharacterID, !temp.IsReady);
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetCharacterServerRpc(int id, ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientID == serverRpcParams.Receive.SenderClientId)
            {
                var temp = players[i];
                players[i] = new PlayerState(temp.ClientID, id, temp.IsReady);
            }
        }
    }
}

/// <summary>
/// Represents the state of character selection of the player.
/// This is synchronized via network and stored as a list.
/// </summary>
public struct PlayerState : INetworkSerializable, IEquatable<PlayerState>
{
    public ulong ClientID;
    public bool IsReady;
    public int CharacterID;

    public PlayerState(ulong clientID, int characterId, bool isReady = false)
    {
        ClientID = clientID;
        CharacterID = characterId;
        IsReady = isReady;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientID);
        serializer.SerializeValue(ref IsReady);
        serializer.SerializeValue(ref CharacterID);
    }

    public bool Equals(PlayerState other)
    {
        return ClientID == other.ClientID && IsReady == other.IsReady && CharacterID == other.CharacterID;
    }
}