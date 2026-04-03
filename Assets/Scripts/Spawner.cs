using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Spawner : Singleton<Spawner>
{
    public ItemTypes shapesAndColors;
    public Transform spawnPoint;
    public Transform midPoint;

    public Queue<Item> spawnedObjects = new Queue<Item>();

    private ItemData[] currentWaveData;
    private int respawnsRemaining;
    private int activeItemCount;

    public bool IsWaveDone => spawnedObjects.Count == 0 && respawnsRemaining <= 0 && activeItemCount <= 0;

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

    public void SetRespawnCount(int count)
    {
        respawnsRemaining = count;
    }

    private void SpawnWave(ItemData[] data)
    {
        currentWaveData = data;
        activeItemCount = 0;
        SpawnFromData(data);
    }

    private void SpawnFromData(ItemData[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            int shapeIndex = data[i].ShapeIndex;
            int colorIndex = data[i].ColorIndex;
            var obj = Instantiate(shapesAndColors.items[shapeIndex], spawnPoint.position, spawnPoint.rotation);
            var color = shapesAndColors.colors[colorIndex];
            var spawnedObject = obj.GetComponent<Item>();
            spawnedObject.Init(data[i], color);
            spawnedObjects.Enqueue(spawnedObject);
        }
    }

    private void Respawn()
    {
        // Shuffle the existing wave data (Fisher-Yates)
        ItemData[] shuffled = (ItemData[])currentWaveData.Clone();
        for (int i = shuffled.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        respawnsRemaining--;
        SpawnFromData(shuffled);
    }

    public void MoveObject()
    {
        // Queue empty but respawns left — refill with shuffled same wave
        if (spawnedObjects.Count == 0 && respawnsRemaining > 0)
        {
            Respawn();
        }

        if (spawnedObjects.Count == 0)
            return;

        activeItemCount++;
        var obj = spawnedObjects.Dequeue();
        obj.transform.DOMoveY(midPoint.position.y, GameManager.Instance.moveSpeed)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (GameManager.Instance.currentItem != null && GameManager.Instance.currentItem != obj)
                {
                    GameManager.Instance.DestroyCurrentItem();
                }
            });
    }

    public void ResolveItem()
    {
        activeItemCount--;
    }
}