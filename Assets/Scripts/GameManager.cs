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
    public float waveDuration = 36f;
    public float moveDelay = 2f;
    public float moveSpeed = 12f;

    [Header("Swipe")]
    public float minSwipeDistance = 50f;

    [Header("Runtime")]
    public Item currentItem;

    private bool isSwiping;
    private float fadeAwayTime = 0.2f;
    private float _moveTimer;
    private Vector2 initialPosition;

    private void Start()
    {
        GenerateWave();
    }

    private void Update()
    {
        // When wave is fully done (all batches spawned + queue drained), start a new wave
        if (Spawner.Instance != null && Spawner.Instance.IsWaveDone)
        {
            GenerateWave();
        }

        _moveTimer += Time.deltaTime;
        if (_moveTimer >= moveDelay / difficulty)
        {
            _moveTimer = 0;
            if (Spawner.Instance != null)
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

        // Calculate how many respawns fit in the wave duration
        // One batch takes: numberOfItems * moveInterval
        float moveInterval = moveDelay / difficulty;
        int respawnCount = Mathf.FloorToInt(waveDuration / (number * moveInterval));
        // Subtract 1 because the first spawn is the initial, respawns are the repeats
        respawnCount = Mathf.Max(0, respawnCount - 1);

        if (Spawner.Instance != null)
            Spawner.Instance.SetRespawnCount(respawnCount);

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