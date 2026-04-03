using System;
using UnityEngine;
using DG.Tweening;

public struct ItemData
{
    public int ShapeIndex;
    public int ColorIndex;
}

public class Item : MonoBehaviour
{
    public ItemData itemData = new ItemData();
    [SerializeField] private SpriteRenderer spriteRenderer;

    public float moveDistance = 5f;

    [HideInInspector] public bool isReady;
    [HideInInspector] public bool isUIElement;

    public void Init(ItemData data, Color color)
    {
        this.itemData = data;
        spriteRenderer.color = color;
    }

    public void Move(Vector3 direction)
    {
        transform.DOMove(transform.position + direction * moveDistance, GameManager.Instance.moveSpeed)
            .SetEase(Ease.OutQuad);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isUIElement) return;

        if (other.CompareTag("Splitter"))
        {
            if (GameManager.Instance.currentItem != null && GameManager.Instance.currentItem != this)
            {
                GameManager.Instance.DestroyCurrentItem();
            }

            isReady = true;
            GameManager.Instance.currentItem = this;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isUIElement) return;

        if (other.CompareTag("Splitter"))
        {
            isReady = false;
            if (GameManager.Instance.currentItem == this)
            {
                GameManager.Instance.currentItem = null;
            }
        }
    }

    public void FadeAway(float fadeAwayTime)
    {
        spriteRenderer.DOFade(0, fadeAwayTime).OnComplete(() => Destroy(gameObject));
    }
}