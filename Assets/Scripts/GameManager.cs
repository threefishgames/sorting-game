using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : Singleton<GameManager>
{
    public static event Action<ItemData[]> OnNewWave;
    public static event Action<int> OnScoreUpdated;

    [Header("Game Settings")]
    [Range(0.1f, 1)] public float difficulty = 0.5f;
    public ItemTypes items;
    public SpawnSettings spawnSettings;

    [Header("Dynamic Difficulty")]
    [Range(0.01f, 0.2f)] public float difficultyStep = 0.05f;
    [Range(0.1f, 1f)] public float minDifficulty = 0.1f;
    [Range(0.1f, 1f)] public float maxDifficulty = 1f;
    [Range(0.01f, 0.5f)] public float difficultySmoothing = 0.1f;
    private float targetDifficulty;

    [Header("Runtime - Current Wave Settings")]
    public float moveSpeed;
    private float waveDuration;
    private float moveDelay;
    private int itemsPerWave;
    private int currentWaveIndex;

    [Header("Swipe")]
    public float minSwipeDistance = 50f;

    [Header("Runtime")]
    public Item currentItem;

    private bool isSwiping;
    private float fadeAwayTime = 0.2f;
    private float _moveTimer;
    private Vector2 initialPosition;

    public int Score
    {
        get => score;
        set
        {
            score = value;
            OnScoreUpdated?.Invoke(value);
        }
    }
    private int score;

    private void Start()
    {
        targetDifficulty = difficulty;
        currentWaveIndex = 0;
        ApplyWaveSettings();
        GenerateWave();
    }

    private void ApplyWaveSettings()
    {
        if (spawnSettings == null || spawnSettings.waves.Count == 0)
            return;

        var entry = spawnSettings.waves[currentWaveIndex];
        waveDuration = entry.waveDuration;
        moveDelay = entry.moveDelay;
        moveSpeed = entry.moveSpeed;
        itemsPerWave = entry.itemsPerWave;
    }

    public void OnCorrectSort()
    {
        Score++;
        targetDifficulty = Mathf.Min(targetDifficulty + difficultyStep, maxDifficulty);
    }

    public void OnWrongSort()
    {
        Score--;
        targetDifficulty = Mathf.Max(targetDifficulty - difficultyStep, minDifficulty);
    }

    public void OnMissedItem()
    {
        targetDifficulty = Mathf.Max(targetDifficulty - difficultyStep * 2f, minDifficulty);
    }

    private void Update()
    {
        difficulty = Mathf.MoveTowards(difficulty, targetDifficulty, difficultySmoothing * Time.deltaTime);

        // When wave is fully done (all batches spawned + queue drained), advance and start a new wave
        if (Spawner.Instance != null && Spawner.Instance.IsWaveDone)
        {
            if (spawnSettings != null && spawnSettings.waves.Count > 0)
            {
                currentWaveIndex = (currentWaveIndex + 1) % spawnSettings.waves.Count;
                ApplyWaveSettings();
            }
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
    
    private void GenerateWave()
    {
        List<ItemData> tempItems = new List<ItemData>();
        int maxCombinations = items.colors.Count * items.items.Count;
        int count = Mathf.Min(itemsPerWave, maxCombinations);

        while (tempItems.Count < count)
        {
            ItemData instance = new ItemData();
            instance.ColorIndex = Random.Range(0, items.colors.Count);
            instance.ShapeIndex = Random.Range(0, items.items.Count);

            if (!tempItems.Contains(instance))
                tempItems.Add(instance);
        }

        // Calculate how many respawns fit in the wave duration
        // One batch takes: numberOfItems * moveInterval
        float moveInterval = moveDelay / difficulty;
        int respawnCount = Mathf.FloorToInt(waveDuration / (itemsPerWave * moveInterval));
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
        OnMissedItem();
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