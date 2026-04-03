using System;
using UnityEngine;
using DG.Tweening;

public struct ItemData : IEquatable<ItemData>
{
    public int ShapeIndex;
    public int ColorIndex;

    public bool Equals(ItemData other)
    {
        return ShapeIndex == other.ShapeIndex && ColorIndex == other.ColorIndex;
    }

    public override bool Equals(object obj) => obj is ItemData other && Equals(other);

    public override int GetHashCode()
    {
        return ShapeIndex * 397 ^ ColorIndex;
    }

    public static bool operator ==(ItemData lhs, ItemData rhs) => lhs.Equals(rhs);
    public static bool operator !=(ItemData lhs, ItemData rhs) => !lhs.Equals(rhs);
}

public class Item : MonoBehaviour
{
    public ItemData itemData = new ItemData();
    [SerializeField] private SpriteRenderer spriteRenderer;

    public float moveDistance = 5f;

    [HideInInspector] public bool isReady;
    [HideInInspector] public bool isUIElement;
    private bool isSwiped;

    public void Init(ItemData data, Color color)
    {
        this.itemData = data;
        spriteRenderer.color = color;
    }

    public void Move(Vector3 direction)
    {
        isSwiped = true;
        transform.DOMove(transform.position + direction * moveDistance, GameManager.Instance.moveSpeed)
            .SetEase(Ease.OutQuad);
    }

    private void OnBecameInvisible()
    {
        if (!isSwiped) return;

        transform.DOKill();
        Spawner.Instance.ResolveItem();
        Destroy(gameObject);
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
        spriteRenderer.DOFade(0, fadeAwayTime).OnComplete(() =>
        {
            Spawner.Instance.ResolveItem();
            Destroy(gameObject);
        });
    }
}