using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class Network_InteractableObject : MonoBehaviour
{
    [SerializeField] private bool interactable;
    [SerializeField] private UnityEvent onInteracted;
    public ulong interactorId;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPlayerMovement _))
        {
            interactable = true;
            interactorId = collision.gameObject.GetComponent<NetworkPlayerController>().OwnerClientId;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPlayerMovement _))
        {
            interactable = false;
            interactorId = 99999;
        }
    }

    private void Update()
    {
        if (!interactable) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            onInteracted?.Invoke();
        }
    }
}