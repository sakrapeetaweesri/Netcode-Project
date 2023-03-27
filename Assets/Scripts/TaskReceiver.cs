using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskReceiver : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out TaskObject t))
        {
            if (t.taskState.Value != TaskState.CopiedDocument) return;

            t.SubmitTaskServerRpc();
            GameCanvasManager.Instance.CountCompletedTaskServerRpc();
        }
    }
}