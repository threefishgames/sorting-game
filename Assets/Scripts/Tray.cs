using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class Tray : MonoBehaviour
{
    public ItemTypes itemType;
    public float offset = 0.3f;
    public float scale = 0.5f;
    public float animDuration = 0.4f;
    public float animHeight = 1f;

    private GameObject currentTray;
    public List<ItemData> items;

    public void SetTrayItems(ItemData[] newItems)
    {
        items = new List<ItemData>(newItems);
        SpawnTray();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Item>(out var item))
        {
        }
    }

    [ContextMenu("Add Item")]
    public void SpawnTray()
    {
        if (currentTray != null)
        {
            AnimateOut(currentTray);
        }

        currentTray = new GameObject("CurrentTray")
        {
            transform =
            {
                parent = transform
            }
        };

        float xPos = transform.position.x;
        float yPos = transform.position.y;

        foreach (ItemData i in items)
        {
            SpriteRenderer sr = itemType.items[i.ShapeIndex].gameObject.GetComponent<SpriteRenderer>();
            float spriteWidth = sr.sprite.bounds.size.x * scale;
            float halfWidth = spriteWidth / 2f;

            xPos += halfWidth;
            Vector2 spawnPos = new Vector2(xPos, yPos + animHeight);
            GameObject item = Instantiate(itemType.items[i.ShapeIndex].gameObject, spawnPos, Quaternion.identity, currentTray.transform);
            item.transform.localScale = Vector3.zero;
            item.GetComponent<Item>().Init(i,itemType.colors[i.ColorIndex]);

            SpriteRenderer itemSr = item.GetComponent<SpriteRenderer>();
            itemSr.color = new Color(itemSr.color.r, itemSr.color.g, itemSr.color.b, 0f);

            Vector3 targetScale = new Vector3(scale, scale, scale);
            itemSr.DOFade(1f, animDuration * 0.6f).SetEase(Ease.OutQuad);
            item.transform.DOScale(targetScale, animDuration).SetEase(Ease.OutBack);
            item.transform.DOMoveY(yPos, animDuration).SetEase(Ease.OutCirc);

            xPos += halfWidth + offset;
        }
    }

    private void AnimateOut(GameObject oldTray)
    {
        float targetY = transform.position.y - animHeight;
        int childCount = oldTray.transform.childCount;
        int completed = 0;

        foreach (Transform child in oldTray.transform)
        {
            child.DOKill();
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.DOKill();
                sr.DOFade(0f, animDuration * 0.5f).SetEase(Ease.InQuad).SetLink(child.gameObject);
            }
            child.DOScale(Vector3.zero, animDuration).SetEase(Ease.InBack).SetLink(child.gameObject);
            child.DOMoveY(targetY, animDuration).SetEase(Ease.InCirc).SetLink(child.gameObject)
                .OnComplete(() =>
                {
                    completed++;
                    if (completed >= childCount)
                    {
                        Destroy(oldTray);
                    }
                });
        }

        if (childCount == 0)
        {
            Destroy(oldTray);
        }
    }
}