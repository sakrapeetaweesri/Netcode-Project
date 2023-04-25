using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task_CopyError_Paper : UI_Draggable
{
    protected override void Awake()
    {
        base.Awake();
        blockDragging = true;
    }

    public void ResetDraggable()
    {
        blockDragging = false;
        canvasGroup.blocksRaycasts = true;
        gameObject.SetActive(true);
    }

    public void InsertPaper()
    {
        blockDragging = true;

        gameObject.SetActive(false);
        Task_CopyError.Instance.FinishErrorTask();
    }
}