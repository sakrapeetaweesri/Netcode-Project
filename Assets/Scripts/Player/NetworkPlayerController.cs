using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class NetworkPlayerController : NetworkTransform, IPlayerMovement
{
    private readonly float movementSpeed = 7f;
    private bool blockMovement;

    private Rigidbody2D _rigidbody2D;
    private GameObject bubble;
    private SpriteRenderer bubbleIcon;

    public NetworkVariable<TaskState> holdingTask = new NetworkVariable<TaskState>();

    public static Dictionary<ulong, NetworkPlayerController> Players = new Dictionary<ulong, NetworkPlayerController>();

    protected override bool OnIsServerAuthoritative() => false;
    public override void OnNetworkSpawn()
    {
        // Acts like a Start() function.
        if (IsOwner)
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            bubble = transform.Find("Bubble").gameObject;
            bubbleIcon = transform.Find("Bubble/BubbleIcon").GetComponent<SpriteRenderer>();
            CameraManager.Instance.AssignCameraTracker(transform);

            transform.position = RelayManager.lastPosition;

            holdingTask.OnValueChanged += HandleHoldingTask;

            if (MainLobbyManager.Instance != null)
            {
                MainLobbyManager.Instance.onCanvasEnabled += SetBlockMovement;
            }
            if (GameCanvasManager.Instance != null)
            {
                GameCanvasManager.Instance.onCanvasEnabled += SetBlockMovement;
            }

            tickFrequency = 1.0f / NetworkManager.NetworkTickSystem.TickRate;
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

        if (IsOwner)
        {
            if (MainLobbyManager.Instance != null)
            {
                MainLobbyManager.Instance.onCanvasEnabled -= SetBlockMovement;
            }
            if (GameCanvasManager.Instance != null)
            {
                GameCanvasManager.Instance.onCanvasEnabled -= SetBlockMovement;
            }
        }

        base.OnNetworkDespawn();
    }
    private void SetBlockMovement(bool state)
    {
        blockMovement = state;
    }

    private void LateUpdate()
    {
        if (!IsSpawned || !IsOwner) return;

        if (Time.realtimeSinceStartup >= teleportDelayInput)
        {
            IsTeleporting = false;
            _rigidbody2D.isKinematic = false;
        }
    }
    /// <summary>
    /// Controls the player movement.
    /// </summary>
    private void FixedUpdate()
    {
        // Returns if not owner nor spawned.
        if (!IsSpawned || !IsOwner) return;

        if (blockMovement)
        {
            _rigidbody2D.velocity = Vector2.zero;
            return;
        }

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
    protected override void Update()
    {
        base.Update();

        UpdateAnimation();
    }

    #region Movement
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

    public Vector2 GetVelocity()
    {
        if (_rigidbody2D == null) _rigidbody2D = GetComponent<Rigidbody2D>();
        return _rigidbody2D.velocity;
    }
    public Vector2 GetDirection()
    {
        var velocity = _rigidbody2D.velocity;
        if (Mathf.Abs(velocity.x) <= 0.01f && Mathf.Abs(velocity.y) <= 0.01f)
        {
            switch (currentDirection)
            {
                case PlayerDirection.Up: return Vector2.up;
                case PlayerDirection.Down: return Vector2.down;
                case PlayerDirection.Left: return Vector2.left;
                case PlayerDirection.Right: return Vector2.right;
            }
        }

        return velocity;
    }

    public bool IsTeleporting { get; private set; }
    private float tickFrequency;
    private float teleportDelayInput;

    public void TryTeleport(Vector3 destination)
    {
        if (!IsSpawned || !IsOwner || IsTeleporting) return;

        IsTeleporting = true;

        _rigidbody2D.isKinematic = true;
        teleportDelayInput = Time.realtimeSinceStartup + (3f * tickFrequency);

        Teleport(destination, transform.rotation, transform.localScale);
    }

    [ClientRpc]
    public void TeleportClientRpc(Vector3 destination)
    {
        TryTeleport(destination);
    }
    #endregion

    #region Animation
    [SerializeField] private AnimationClip[] animationClips;
    [SerializeField] private Animator _anim;

    [SerializeField] private PlayerDirection lastDirection = PlayerDirection.Down;
    [SerializeField] private PlayerDirection currentDirection = PlayerDirection.Down;

    private void UpdateAnimation()
    {
        var velocity = GetVelocity();

        if (Mathf.Abs(velocity.x) <= 0.01f && Mathf.Abs(velocity.y) <= 0.01f)
        {
            lastDirection = currentDirection;
            SetAnimation(lastDirection, false);
            return;
        }

        if (velocity.x != 0f)
        {
            currentDirection = velocity.x > 0f ? PlayerDirection.Right : PlayerDirection.Left;
        }
        if (velocity.y != 0f)
        {
            currentDirection = velocity.y > 0f ? PlayerDirection.Up : PlayerDirection.Down;
        }
        SetAnimation(currentDirection, true);

        void SetAnimation(PlayerDirection playerDirection, bool isMoving)
        {
            int animIndex = ((int)playerDirection * 2) + (isMoving ? 1 : 0);
            if (IsOwner) _anim.Play(animationClips[animIndex].name);
        }
    }
    #endregion

    [ServerRpc(RequireOwnership = false)]
    public void SetHoldingTaskServerRpc(TaskState taskState)
    {
        holdingTask.Value = taskState;
    }
    [ServerRpc(RequireOwnership = false)]
    public void DisableBubbleServerRpc()
    {
        holdingTask.Value = TaskState.None;
    }

    private void HandleHoldingTask(TaskState oldTaskState, TaskState newTaskState)
    {
        CallSetBubbleIconServerRpc(newTaskState != TaskState.None);
    }
    [ServerRpc(RequireOwnership = false)]
    private void CallSetBubbleIconServerRpc(bool active)
    {
        SetBubbleIconClientRpc(active);
    }

    [ClientRpc]
    private void SetBubbleIconClientRpc(bool active)
    {
        if (bubble == null)
        {
            bubble = transform.Find("Bubble").gameObject;
            bubbleIcon = transform.Find("Bubble/BubbleIcon").GetComponent<SpriteRenderer>();
        }
        bubble.SetActive(active);

        if ((int)holdingTask.Value - 1 <= 0) return;
        bubbleIcon.sprite = GameAssets.i.TaskSprites[(int)holdingTask.Value - 1];
    }
}