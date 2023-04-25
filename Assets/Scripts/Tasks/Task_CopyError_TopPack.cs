using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Task_CopyError_TopPack : UI_Draggable
{
    private Vector2 startingPoint;
    private Vector2 distance;
    private readonly float sectionDistance = 100f;
    public int sectionPassed;

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        startingPoint = eventData.delta / (scale * canvas.scaleFactor);
        distance = startingPoint;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        distance += eventData.delta / (scale * canvas.scaleFactor);
        if (distance.x <= startingPoint.x) return;

        var dragDistance = distance.x - startingPoint.x;
        int sectionPassedCount = Mathf.Clamp(Mathf.FloorToInt(dragDistance / sectionDistance), 0, 4);
        if (sectionPassedCount > sectionPassed) sectionPassed = sectionPassedCount;
    }
    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        distance = startingPoint;
    }

    public void ClearSectionCount()
    {
        sectionPassed = 0;
        canvasGroup.blocksRaycasts = true;
        distance = new Vector2();
    }
}