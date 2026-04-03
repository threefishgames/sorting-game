using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : Singleton<GameManager>
{
    public static event Action<ItemData[]> OnNewWave;

    [Header("Game Settings")]
    [Range(0.1f, 1)] public float difficulty = 0.5f;
    public ItemTypes items;

    [Header("Spawning")]
    public float maxSpawnDelay = 2f;
    public float moveDelay = 3f;
    public float moveSpeed = 2f;

    [Header("Swipe")]
    public float minSwipeDistance = 50f;

    [Header("Runtime")]
    public Item currentItem;

    private bool isSwiping;
    private float fadeAwayTime = 0.2f;
    private float _timer;
    private float _moveTimer;
    private Vector2 initialPosition;

    private void Start()
    {
        _timer = maxSpawnDelay;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= maxSpawnDelay)
        {
            _timer = 0;
            GenerateWave();
        }

     
        _moveTimer += Time.deltaTime;
        if (_moveTimer >= moveDelay / difficulty)
        {
            _moveTimer = 0;
            if (Spawner.Instance != null && Spawner.Instance.spawnedObjects.Count > 0)
            {
                Spawner.Instance.MoveObject();
            }
        }

        DetectSwipe();
    }

    private void GenerateWave(int number = 3)
    {
        List<ItemData> tempItems = new List<ItemData>();
        for (int i = 0; i < number; i++)
        {
            ItemData instance = new ItemData();
            instance.ColorIndex = Random.Range(0, items.colors.Count);
            instance.ShapeIndex = Random.Range(0, items.items.Count);
            tempItems.Add(instance);
        }
        OnNewWave?.Invoke(tempItems.ToArray());
    }

    private void DetectSwipe()
    {
        if (Input.GetMouseButtonDown(0))
        {
            initialPosition = Input.mousePosition;
            isSwiping = true;
        }

        if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            isSwiping = false;
            Vector2 endPosition = Input.mousePosition;
            Vector2 swipeDelta = endPosition - initialPosition;

            if (swipeDelta.magnitude < minSwipeDistance)
                return;

            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                if (swipeDelta.x > 0)
                    OnSwipe(SwipeDirection.Right);
                else
                    OnSwipe(SwipeDirection.Left);
            }
            else
            {
                if (swipeDelta.y > 0)
                    OnSwipe(SwipeDirection.Up);
                else
                    OnSwipe(SwipeDirection.Down);
            }
        }
    }

    private void OnSwipe(SwipeDirection direction)
    {
        if (currentItem == null || !currentItem.isReady)
            return;

        Item item = currentItem;
        item.isReady = false;
        currentItem = null;

        switch (direction)
        {
            case SwipeDirection.Left:
                item.Move(Vector3.left);
                break;
            case SwipeDirection.Right:
                item.Move(Vector3.right);
                break;
            case SwipeDirection.Down:
                item.Move(Vector3.down);
                break;
        }
    }


    public void DestroyCurrentItem()
    {
        currentItem.FadeAway(fadeAwayTime);
        currentItem = null;
    }
}

public enum SwipeDirection
{
    Left,
    Right,
    Up,
    Down
}