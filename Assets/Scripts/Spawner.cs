using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [Range(0.1f, 1)] public float difficulty = 0.5f;

    public Color[] colors = new Color[]
    {
        new Color(0.85f, 0.55f, 0.55f),
        new Color(0.55f, 0.65f, 0.85f),
        new Color(0.60f, 0.80f, 0.65f),
        new Color(0.90f, 0.82f, 0.55f),
    };

    public List<Item> prefabs;

    public Transform spawnPoint;
    public Transform midPoint;

    public float maxSpawnDelay = 2f;

    private float _timer;


    private void OnValidate()
    {
        spawnPoint = transform;
        midPoint = GameObject.Find("MidPoint").transform;
    }

    private void Spawn()
    {
        int rand = Random.Range(0, prefabs.Count);
        int rand2 = Random.Range(0, colors.Length);
        Item spawnedObject = Instantiate(prefabs[rand], spawnPoint.position, spawnPoint.rotation);
        spawnedObject.SetColor(colors[rand2]);
        spawnedObject.transform.DOMoveY(midPoint.position.y, GameManager.Instance.moveSpeed)
            .SetEase(Ease.Linear);
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= maxSpawnDelay / difficulty)
        {
            _timer = 0;
            Spawn();
        }
    }
}