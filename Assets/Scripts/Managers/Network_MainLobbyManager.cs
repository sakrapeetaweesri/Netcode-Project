using System;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class Network_MainLobbyManager : NetworkBehaviour
{
    public NetworkList<PlayerState> players;

    [SerializeField] private PlayerCard[] playerCards;

    [SerializeField] private TextMeshProUGUI maxPlayerText;
    [SerializeField] private TextMeshProUGUI playerReadyCountText;

    public static Network_MainLobbyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            players = new NetworkList<PlayerState>();
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
        players.Add(new PlayerState(playerID));
        Debug.Log("New player added, current player count: " + players.Count);
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
    }

    /// <summary>
    /// Updates the max player text UI.
    /// </summary>
    public void UpdateMaxPlayer()
    {
        maxPlayerText.SetText(NetworkPlayerController.Players.Count.ToString());
    }
    private void UpdatePlayerCard()
    {
        for (int i = 0; i < playerCards.Length; i++)
        {
            if (players.Count > i)
            {
                playerCards[i].UpdateDisplay(players[i]);
            }
            else
            {
                playerCards[i].DisableVisuals();
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

    public PlayerState(ulong clientID)
    {
        ClientID = clientID;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientID);
    }

    public bool Equals(PlayerState other)
    {
        return ClientID == other.ClientID;
    }
}