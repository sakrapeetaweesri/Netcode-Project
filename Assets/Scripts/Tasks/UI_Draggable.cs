using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UI_Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] protected RectTransform rect;
    [SerializeField] protected Canvas canvas;
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected bool blocksRaycasts;
    protected bool blockDragging;
    protected float scale = 1f;
    protected Vector2 origin;

    protected virtual void Awake()
    {
        origin = rect.anchoredPosition;

        CalculateScale();
    }

    /// <summary>
    /// Calculates the scale for this ui. This affects dragging sensitivity.
    /// </summary>
    protected void CalculateScale()
    {
        // Initializes the scale at 1.
        scale = 1;

        // Gathers the parents.
        List<Transform> parentRects = new List<Transform>();
        Transform indexer = transform;
        while (true)
        {
            if (indexer.parent != null)
            {
                parentRects.Add(indexer.parent);
                indexer = indexer.parent;
            }
            else
            {
                break;
            }
        }

        // Scales the factor.
        foreach (Transform r in parentRects)
        {
            if (r.GetComponent<Canvas>() != null)
                continue;

            scale *= r.localScale.x;
        }
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (blockDragging) return;
        canvasGroup.blocksRaycasts = false;
    }
    public virtual void OnDrag(PointerEventData eventData)
    {
        if (blockDragging) return;
        rect.anchoredPosition += eventData.delta / (scale * canvas.scaleFactor);
    }
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (blockDragging) return;
        canvasGroup.blocksRaycasts = blocksRaycasts;
    }
}