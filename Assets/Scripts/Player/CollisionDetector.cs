using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [field: SerializeField] public bool IsColliding { get; private set; }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((layerMask.value & (1 << collision.gameObject.layer)) > 0)
            IsColliding = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((layerMask.value & (1 << collision.gameObject.layer)) > 0)
            IsColliding = false;
    }
}