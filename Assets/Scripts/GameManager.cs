using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : Singleton<GameManager>
{
    public static event Action<ItemData[]> OnNewWave;
    public static event Action<int> OnScoreUpdated;
    public static event Action<int> OnLivesUpdated;
    public static event Action OnGameOver;

    [Header("Game Settings")]
    [Range(0.1f, 1)] public float difficulty = 0.5f;
    [Range(1, 10)] public int maxLives = 3;
    public ItemTypes items;
    public SpawnSettings spawnSettings;

    [Header("Dynamic Difficulty - Core")]
    [Range(0.01f, 0.2f)] public float difficultyStep = 0.05f;
    [Range(0.1f, 1f)] public float minDifficulty = 0.1f;
    [Range(0.1f, 2f)] public float maxDifficulty = 1f;
    [Range(0.01f, 2f)] public float difficultyRiseSmoothing = 0.1f;
    [Range(0.01f, 2f)] public float difficultyFallSmoothing = 0.3f;
    [Range(1f, 5f)] public float missedItemMultiplier = 2f;

    [Header("Dynamic Difficulty - Streaks")]
    [Range(0f, 1f)] public float streakBonusPerHit = 0.25f;
    [Range(1f, 5f)] public float maxStreakMultiplier = 3f;

    [Header("Dynamic Difficulty - Fast Swipe")]
    [Range(0.05f, 2f)] public float fastSwipeWindow = 0.4f;
    [Range(1f, 3f)] public float fastSwipeMultiplier = 2f;

    [Header("Dynamic Difficulty - Wave Advancement")]
    [Range(0f, 1f)] public float skipAheadThreshold = 0.75f;
    [Range(0f, 1f)] public float repeatThreshold = 0.35f;
    [Range(0, 5)] public int skipAheadSteps = 2;
    [Range(0, 3)] public int normalSteps = 1;
    [Range(0, 2)] public int repeatSteps = 0;

    private float targetDifficulty;
    private int correctStreak;
    private int incorrectStreak;
    private float currentItemReadyTime;

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

    public int Lives
    {
        get => lives;
        set
        {
            lives = Mathf.Max(0, value);
            OnLivesUpdated?.Invoke(lives);
            if (lives == 0) OnGameOver?.Invoke();
        }
    }
    private int lives;

    public bool IsGameOver => lives <= 0;

    private void Start()
    {
        targetDifficulty = difficulty;
        Lives = maxLives;
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

    public void NotifyItemReady()
    {
        currentItemReadyTime = Time.time;
    }

    public void OnCorrectSort()
    {
        Score++;
        correctStreak++;
        incorrectStreak = 0;

        float bump = difficultyStep;
        bump *= Mathf.Min(1f + correctStreak * streakBonusPerHit, maxStreakMultiplier);

        float swipeTime = Time.time - currentItemReadyTime;
        if (swipeTime <= fastSwipeWindow) bump *= fastSwipeMultiplier;

        targetDifficulty = Mathf.Min(targetDifficulty + bump, maxDifficulty);
    }

    public void OnWrongSort()
    {
        Lives--;
        incorrectStreak++;
        correctStreak = 0;

        float penalty = difficultyStep;
        penalty *= Mathf.Min(1f + incorrectStreak * streakBonusPerHit, maxStreakMultiplier);

        targetDifficulty = Mathf.Max(targetDifficulty - penalty, minDifficulty);
    }

    public void OnMissedItem()
    {
        Lives--;
        incorrectStreak++;
        correctStreak = 0;

        float penalty = difficultyStep * missedItemMultiplier;
        penalty *= Mathf.Min(1f + incorrectStreak * streakBonusPerHit, maxStreakMultiplier);

        targetDifficulty = Mathf.Max(targetDifficulty - penalty, minDifficulty);
    }

    private void Update()
    {
        float smoothing = targetDifficulty > difficulty ? difficultyRiseSmoothing : difficultyFallSmoothing;
        difficulty = Mathf.MoveTowards(difficulty, targetDifficulty, smoothing * Time.deltaTime);

        // When wave is fully done (all batches spawned + queue drained), advance and start a new wave
        if (Spawner.Instance != null && Spawner.Instance.IsWaveDone)
        {
            if (spawnSettings != null && spawnSettings.waves.Count > 0)
            {
                int step = normalSteps;
                if (difficulty > skipAheadThreshold) step = skipAheadSteps;
                else if (difficulty < repeatThreshold) step = repeatSteps;

                currentWaveIndex = (currentWaveIndex + step) % spawnSettings.waves.Count;
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