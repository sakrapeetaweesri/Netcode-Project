using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Network_InteractableObject : MonoBehaviour
{
    [SerializeField] private bool interactable;
    [SerializeField] private UnityEvent onInteracted;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPlayerMovement _))
            interactable = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPlayerMovement _))
            interactable = false;
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