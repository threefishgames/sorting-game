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
    public float snapDistance = 1f;
    public float snapDuration = 0.15f;

    [Header("Highlight")]
    public float pulseScale = 1.15f;
    public float pulseDuration = 0.4f;
    public float highlightBrightness = 1.4f;

    [HideInInspector] public bool isReady;
    [HideInInspector] public bool isUIElement;
    private bool isSwiped;
    private Color baseColor;
    private Vector3 initialScale;
    private Tweener pulseTween;

    public void Init(ItemData data, Color color)
    {
        this.itemData = data;
        spriteRenderer.color = color;
        initialScale = transform.localScale;
    }

    public void Move(Vector3 direction)
    {
        isSwiped = true;
        StopHighlight();
        transform.DOKill();

        Vector3 snapTarget = transform.position + direction * snapDistance;
        float remainingDistance = moveDistance - snapDistance;

        Sequence seq = DOTween.Sequence();
        seq.SetTarget(transform);
        seq.Append(transform.DOMove(snapTarget, snapDuration).SetEase(Ease.OutQuad));
        seq.Append(transform.DOMove(snapTarget + direction * remainingDistance, GameManager.Instance.moveSpeed)
            .SetEase(Ease.Linear));
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

            // Snap to the center of the splitter
            transform.DOKill();
            Vector3 snapTarget = new Vector3(other.transform.position.x, other.transform.position.y, transform.position.z);
            transform.DOMove(snapTarget, 0.15f).SetEase(Ease.OutQuad);

            isReady = true;
            GameManager.Instance.currentItem = this;
            GameManager.Instance.NotifyItemReady();
            StartHighlight();
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
            StopHighlight();
        }
    }

    private void StartHighlight()
    {
        baseColor = spriteRenderer.color;
        Color bright = baseColor * highlightBrightness;
        bright.a = baseColor.a;
        spriteRenderer.color = bright;

        pulseTween = transform.DOScale(initialScale * pulseScale, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetTarget(transform);
    }

    private void StopHighlight()
    {
        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();

        transform.localScale = initialScale;
        spriteRenderer.color = baseColor;
    }

    public void FadeAway(float fadeAwayTime)
    {
        transform.DOKill();
        spriteRenderer.DOFade(0, fadeAwayTime).OnComplete(() =>
        {
            Spawner.Instance.ResolveItem();
            Destroy(gameObject);
        });
    }
}