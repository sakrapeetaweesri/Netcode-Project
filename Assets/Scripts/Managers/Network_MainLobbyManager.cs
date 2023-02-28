using System;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using TMPro;

public class Network_MainLobbyManager : NetworkBehaviour
{
    public NetworkList<PlayerState> players;

    [SerializeField] private PlayerCard[] playerCards;

    [SerializeField] private TextMeshProUGUI maxPlayerText;
    [SerializeField] private TextMeshProUGUI playerReadyCountText;

    public static NetworkVariable<bool> NetworkConnected = new NetworkVariable<bool>();

    public static Network_MainLobbyManager Instance { get; private set; }

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

        players = new NetworkList<PlayerState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(player.ClientId);
            }
        }
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }
    private void HandleClientConnected(ulong playerID)
    {
        players.Add(new PlayerState(playerID));

        for (int i = 0; i < players.Count; i++)
        {
            playerCards[i].UpdateDisplay(players[i]);
        }

        NetworkConnected.Value = true;
        MainLobbyManager.Instance.SwitchTelephoneInteraction(true);
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

        if (players.Count == 0)
        {
            NetworkConnected.Value = false;
            MainLobbyManager.Instance.SwitchTelephoneInteraction(false);
        }
    }

    /// <summary>
    /// Updates the max player text UI.
    /// </summary>
    public void UpdateMaxPlayer()
    {
        maxPlayerText.SetText(NetworkPlayerController.Players.Count.ToString());
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