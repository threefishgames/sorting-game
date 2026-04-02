using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class Spawner : Singleton<Spawner>
{
    public Transform spawnPoint;
    public Transform midPoint;

    private void OnValidate()
    {
        spawnPoint = transform;
        midPoint = GameObject.Find("MidPoint").transform;
    }
    
    

    private void Spawn(GameObject item, Color color)
    {
        var obj = Instantiate(item, spawnPoint.position, spawnPoint.rotation);
        var spawnedObject = obj.GetComponent<Item>();
        spawnedObject.SetColor(color);
        spawnedObject.transform.DOMoveY(midPoint.position.y, GameManager.Instance.moveSpeed)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (GameManager.Instance.currentItem != null && GameManager.Instance.currentItem != spawnedObject)
                {
                    GameManager.Instance.DestroyCurrentItem();
                }

                spawnedObject.isReady = true;
                GameManager.Instance.currentItem = spawnedObject;
            });
    }
}