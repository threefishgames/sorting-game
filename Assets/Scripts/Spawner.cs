using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Spawner : MonoBehaviour
{
    public List<GameObject> prefabs;

    public Transform spawnPoint;
    public Transform midPoint;
    public Transform targetPoint;


    public float spawnToMidTime;
    public float midToEndPoint;
    
    
    private Tween spawnToMidTween;
    public void Spawn()
    {
        int rand = Random.Range(0, prefabs.Count);
        GameObject spawnedObject = Instantiate(prefabs[rand], spawnPoint.position, spawnPoint.rotation);

        spawnToMidTween = spawnedObject.transform.DOMoveY(midPoint.position.y, spawnToMidTime);
        spawnToMidTween.SetTarget(targetPoint);
    }
}
