using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class TaskObject : NetworkTransform
{
    [SerializeField] private float pickUpDistance;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Rigidbody2D _rigidbody;
    private Transform _transform;

    public NetworkVariable<bool> isBeingInteracted = new NetworkVariable<bool>();
    private NetworkVariable<bool> isPickedUp = new NetworkVariable<bool>();
    public NetworkVariable<TaskState> taskState = new NetworkVariable<TaskState>();

    public void SetTaskState(TaskState newTaskState)
    {
        taskState.Value = newTaskState;
    }
    private void HandleTaskState(TaskState oldTaskState, TaskState newTaskState)
    {
        if (!IsOwner) return;

        if (newTaskState == TaskState.Sent) return;

        _spriteRenderer.sprite = GameAssets.i.TaskSprites[(int)newTaskState - 1];

    }

    public override void OnNetworkSpawn()
    {
        _transform = transform;
        taskState.Value = TaskState.PlainDocument;

        taskState.OnValueChanged += HandleTaskState;

        base.OnNetworkSpawn();
    }

    protected override void Update()
    {
        base.Update();

        if (NetworkManager == null || isBeingInteracted.Value || taskState.Value == TaskState.Sent) return;

        if (isPickedUp.Value)
        {
            if (IsOwner && Input.GetKeyDown(KeyCode.Q))
            {
                ReleaseServerRpc();
            }
        }
        else
        {
            var localPlayer = NetworkManager.SpawnManager?.GetLocalPlayerObject();
            if (localPlayer == null) return;
            if (localPlayer.GetComponentInChildren<TaskObject>() != null) return;

            if ((_transform.position - localPlayer.transform.position).sqrMagnitude <= pickUpDistance)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    TryGrabServerRpc();
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSetInteractionServerRpc(bool interacting)
    {
        SetInteraction(interacting);
    }
    private void SetInteraction(bool interacting)
    {
        if (!IsOwner) return;
        isBeingInteracted.Value = interacting;
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryGrabServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (isPickedUp.Value) return;

        var senderClientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkPlayerController.Players.TryGetValue(senderClientId, out NetworkPlayerController p))
        {
            NetworkObject.ChangeOwnership(senderClientId);
            _transform.parent = p.NetworkObject.transform;

            isPickedUp.Value = true;

            p.RequestSetHoldingTask(taskState.Value);
        }

        SetSpriteRendererClientRpc(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReleaseServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (!isPickedUp.Value) return;

        NetworkObject.RemoveOwnership();

        var senderClientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkPlayerController.Players.TryGetValue(senderClientId, out NetworkPlayerController p))
        {
            p.RequestSetHoldingTask(TaskState.None);
        }

        _transform.parent = null;
        SetPositionClientRpc(p.transform.position);
        SetSpriteRendererClientRpc(true);

        _rigidbody.velocity = Vector2.zero;
        var releaseDireaction = p.GetDirection().normalized;
        _rigidbody.AddForce(releaseDireaction * 20f, ForceMode2D.Impulse);

        isPickedUp.Value = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTaskStateServerRpc(TaskState newTaskState)
    {
        if (IsServer)
        {
            SetTaskState(newTaskState);
        }

        SetSpriteRendererClientRpc(_spriteRenderer.enabled, (int)newTaskState - 1);

        if (NetworkManager.SpawnManager.GetLocalPlayerObject().TryGetComponent(out NetworkPlayerController p))
        {
            p.RequestSetHoldingTask(newTaskState);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitTaskServerRpc()
    {
        if (!IsOwner) return;

        SetTaskState(TaskState.Sent);
        SubmitTaskClientRpc();
    }
    [ClientRpc]
    private void SubmitTaskClientRpc()
    {
        _spriteRenderer.enabled = false;
    }

    [ClientRpc]
    private void SetPositionClientRpc(Vector3 destination)
    {
        if (!IsOwner) return;

        Teleport(destination, Quaternion.identity, _transform.localScale);
    }
    [ClientRpc]
    private void SetSpriteRendererClientRpc(bool active, int spriteIndex = -1)
    {
        _spriteRenderer.enabled = active;

        if (spriteIndex >= 0)
            _spriteRenderer.sprite = GameAssets.i.TaskSprites[spriteIndex];
    }
}

public enum TaskState
{
    None,
    PlainDocument,
    SignedDocument,
    TypedDocument,
    CopiedDocument,
    Sent,
}