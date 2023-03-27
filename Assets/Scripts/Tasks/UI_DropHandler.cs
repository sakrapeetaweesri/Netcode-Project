using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UI_DropHandler : MonoBehaviour, IDropHandler
{
    [Header("Properties")]
    [SerializeField] private UnityEvent customEvent;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        // Invokes and returns if custom event exists.
        if (!IsEventNull())
        {
            customEvent.Invoke();
            return;
        }

        // Handles anything else.
        PenHandler(eventData.pointerDrag);
    }

    /// <summary>
    /// Checks if the custom event is null.
    /// </summary>
    /// <returns>'true' if the custom event is null.</returns>
    private bool IsEventNull()
    {
        for (int i = 0; i < customEvent.GetPersistentEventCount(); i++)
        {
            if (customEvent.GetPersistentTarget(i) != null)
                return false;
        }
        return true;
    }

    #region Handler
    private void PenHandler(GameObject pointer)
    {
        if (pointer.TryGetComponent(out UI_Pen p))
        {
            p.SignName();
        }
    }

    #endregion
}