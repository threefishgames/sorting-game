using System;
using UnityEngine;
using DG.Tweening;

public class Item : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public float moveDistance = 5f;

    [HideInInspector] public bool isReady;

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }

    public void Move(Vector3 direction)
    {
        transform.DOMove(transform.position + direction * moveDistance, GameManager.Instance.moveSpeed)
            .SetEase(Ease.OutQuad);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
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

    public void FadeAway(float fadeAwayTime)
    {
        spriteRenderer.DOFade(0, fadeAwayTime).OnComplete(() => Destroy(gameObject));
    }
}