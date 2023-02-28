using UnityEngine.Events;
using UnityEngine;

public class InteractableObject : MonoBehaviour
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