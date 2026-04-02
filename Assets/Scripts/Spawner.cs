using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Spawner : Singleton<Spawner>
{
    public ItemTypes shapesAndColors;
    public Transform spawnPoint;
    public Transform midPoint;

    
    public Queue<Item> spawnedObjects = new Queue<Item>();
    private void OnValidate()
    {
        spawnPoint = transform;
        midPoint = GameObject.Find("MidPoint").transform;
    }

    private void OnEnable()
    {
        GameManager.OnNewWave += SpawnWave;
    }

    private void OnDisable()
    {
        GameManager.OnNewWave -= SpawnWave;
    }

    private void SpawnWave(ItemData[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            int shapeIndex = data[i].ShapeIndex;
            int colorIndex = data[i].ColorIndex;
            var obj = Instantiate(shapesAndColors.items[shapeIndex], spawnPoint.position, spawnPoint.rotation);
            var color = shapesAndColors.colors[colorIndex];
            var spawnedObject = obj.GetComponent<Item>();
            spawnedObject.SetColor(color);
            spawnedObjects.Enqueue(spawnedObject);
        }
    }
    
    public void MoveObject()
    {
        var obj = spawnedObjects.Dequeue();
        obj.transform.DOMoveY(midPoint.position.y, GameManager.Instance.moveSpeed)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (GameManager.Instance.currentItem != null && GameManager.Instance.currentItem != obj)
                {
                    GameManager.Instance.DestroyCurrentItem();
                }

                obj.isReady = true;
                GameManager.Instance.currentItem = obj;
            });
    }
}