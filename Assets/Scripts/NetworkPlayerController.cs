using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Components;

public class NetworkPlayerController : NetworkTransform, IPlayerMovement
{
    private readonly float movementSpeed = 7f;

    private Rigidbody2D _rigidbody2D;

    public static Dictionary<ulong, NetworkPlayerController> Players = new Dictionary<ulong, NetworkPlayerController>();

    protected override bool OnIsServerAuthoritative() => false;
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Acts like a Start() function.
            _rigidbody2D = GetComponent<Rigidbody2D>();
            CameraManager.Instance.AssignCameraTracker(transform);

            if (MainLobbyManager.Instance != null)
            {
                transform.position = MainLobbyManager.Instance.LastPosition;
            }
        }

        Players[OwnerClientId] = this;

        if (Network_MainLobbyManager.Instance != null)
        {
            Network_MainLobbyManager.Instance.UpdateMaxPlayer();
        }

        base.OnNetworkSpawn();
    }
    public override void OnNetworkDespawn()
    {
        if (Players.ContainsKey(OwnerClientId)) Players.Remove(OwnerClientId);

        base.OnNetworkDespawn();
    }

    /// <summary>
    /// Controls the player movement.
    /// </summary>
    private void FixedUpdate()
    {
        // Returns if not owner nor spawned.
        if (!IsSpawned || !IsOwner) return;

        if (MainLobbyManager.Instance != null)
        {
            if (MainLobbyManager.Instance.CurrentCanvas != null)
            {
                _rigidbody2D.velocity = Vector2.zero;
                return;
            }
        }

        // Movement
        Movement();
    }
    /// <summary>
    /// Handles the player movement.
    /// </summary>
    private void Movement()
    {
        float x_Movement = Input.GetAxisRaw("Horizontal");
        float y_Movement = Input.GetAxisRaw("Vertical");

        Vector2 moveDir = new Vector2(x_Movement, y_Movement).normalized * movementSpeed;
        _rigidbody2D.velocity = moveDir;
    }
}