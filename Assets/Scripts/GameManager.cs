using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : Singleton<GameManager>
{
    public ItemTypes items;
    public float minSwipeDistance = 50f;
    public float moveSpeed = 2f;

    private Vector2 initialPosition;
    private bool isSwiping;
    private float fadeAwayTime = 0.2f;

    public Item currentItem;

    public static event Action<ItemData[]> OnNewWave;


    public float maxSpawnDelay = 2f;

    private float _timer;
    [Range(0.1f, 1)] public float difficulty = 0.5f;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
    }

    public void DecideLevel()
    {
    }

    private void Update()
    {
        // spawn a wave in every n seconds
        _timer += Time.deltaTime;
        if (_timer >= maxSpawnDelay / difficulty)
        {
            _timer = 0;
            GenerateWave();
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