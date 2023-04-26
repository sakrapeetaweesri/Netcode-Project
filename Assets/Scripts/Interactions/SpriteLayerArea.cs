using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteLayerArea : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out NetworkPlayerController p))
        {
            var spriteRenderers = p.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].sortingOrder = spriteRenderers[i].gameObject.name == "BubbleIcon" ? 11 : 10;
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out NetworkPlayerController p))
        {
            var spriteRenderers = p.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].sortingOrder = spriteRenderers[i].gameObject.name == "BubbleIcon" ? 2 : 1;
            }
        }
    }
}