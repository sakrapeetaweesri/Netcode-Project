using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Pen : UI_Draggable
{
    [SerializeField] private GameObject sign;

    public void ResetDraggable()
    {
        blockDragging = false;
        canvasGroup.blocksRaycasts = true;
    }

    public void SignName()
    {
        sign.SetActive(true);
        blockDragging = true;

        var pos = rect.anchoredPosition;
        pos.x += 200f;
        StartCoroutine(Utils.SlideCoroutine(rect, rect.anchoredPosition, pos, 10f));

        StartCoroutine(FinishSignName());
    }

    private IEnumerator FinishSignName()
    {
        yield return new WaitForSeconds(1f);
        GameCanvasManager.Instance.FinishDeskTask();
    }
}