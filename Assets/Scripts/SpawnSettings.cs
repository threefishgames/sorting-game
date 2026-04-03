using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnSettingsEntry
{
    public float waveDuration = 36f;
    public float moveDelay = 2f;
    public float moveSpeed = 12f;
    public int itemsPerWave = 3;
}

[CreateAssetMenu(menuName = "ItemTypes/SpawnSettings")]
public class SpawnSettings : ScriptableObject
{
    public List<SpawnSettingsEntry> waves = new List<SpawnSettingsEntry>();

    [ContextMenu("Generate Default Waves (30)")]
    public void GenerateDefaultWaves()
    {
        waves.Clear();

        // --- Tuning knobs ---
        int waveCount = 30;
        int cycleLength = 5; // waves per sawtooth cycle: ramp 4, relief 1

        // Easiest values (wave 1)
        float easyDuration = 36f;
        float easyMoveDelay = 2f;
        float easyMoveSpeed = 12f;
        int easyItems = 3;

        // Hardest values (final peak)
        float hardDuration = 60f;
        float hardMoveDelay = 1f;
        float hardMoveSpeed = 8f;
        int hardItems = 6;

        for (int i = 0; i < waveCount; i++)
        {
            // Global progress 0→1 (overall difficulty floor rises over time)
            float globalT = (float)i / (waveCount - 1);

            // Position within the current cycle
            int cyclePos = i % cycleLength;
            bool isRelief = cyclePos == cycleLength - 1;

            float localT;
            if (isRelief)
            {
                // Relief wave: drop back to ~40% of the current cycle's difficulty
                localT = globalT * 0.4f;
            }
            else
            {
                // Ramp within cycle: 0→1 over the non-relief waves
                float ramp = (float)cyclePos / (cycleLength - 1);
                localT = Mathf.Lerp(globalT * 0.5f, globalT, ramp);
            }

            localT = Mathf.Clamp01(localT);

            var entry = new SpawnSettingsEntry
            {
                // Longer waves as difficulty increases (more time, but harder content)
                waveDuration = Mathf.Round(Mathf.Lerp(easyDuration, hardDuration, localT)),
                // Lower delay = items come faster = harder
                moveDelay = Mathf.Round(Mathf.Lerp(easyMoveDelay, hardMoveDelay, localT) * 10f) / 10f,
                // Lower speed = items move faster = harder
                moveSpeed = Mathf.Round(Mathf.Lerp(easyMoveSpeed, hardMoveSpeed, localT) * 10f) / 10f,
                // More items per wave = harder
                itemsPerWave = Mathf.RoundToInt(Mathf.Lerp(easyItems, hardItems, localT))
            };

            waves.Add(entry);
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Generate Hard Waves (50)")]
    public void GenerateHardWaves()
    {
        waves.Clear();

        int waveCount = 50;
        int cycleLength = 5;

        // Phase 1 (waves 1-15):  Learning — 3-4 items, comfortable pace
        // Phase 2 (waves 16-30): Mid — 4-6 items, faster pace
        // Phase 3 (waves 31-50): Brutal — 6-8 items, tight timing

        for (int i = 0; i < waveCount; i++)
        {
            float globalT = (float)i / (waveCount - 1);

            int cyclePos = i % cycleLength;
            bool isRelief = cyclePos == cycleLength - 1;

            float localT;
            if (isRelief)
            {
                localT = globalT * 0.35f;
            }
            else
            {
                float ramp = (float)cyclePos / (cycleLength - 1);
                localT = Mathf.Lerp(globalT * 0.5f, globalT, ramp);
            }

            localT = Mathf.Clamp01(localT);

            float duration, delay, speed;
            int itemCount;

            if (i < 15)
            {
                // Phase 1: Learning
                float phaseT = localT * 2f; // stretch localT for this phase
                phaseT = Mathf.Clamp01(phaseT);
                duration = Mathf.Round(Mathf.Lerp(30f, 40f, phaseT));
                delay = Mathf.Round(Mathf.Lerp(2.0f, 1.6f, phaseT) * 10f) / 10f;
                speed = Mathf.Round(Mathf.Lerp(12f, 11f, phaseT) * 10f) / 10f;
                itemCount = Mathf.RoundToInt(Mathf.Lerp(3, 4, phaseT));
            }
            else if (i < 30)
            {
                // Phase 2: Mid — introduce more combinations
                float phaseT = (localT - 0.3f) / 0.4f;
                phaseT = Mathf.Clamp01(phaseT);
                duration = Mathf.Round(Mathf.Lerp(40f, 55f, phaseT));
                delay = Mathf.Round(Mathf.Lerp(1.6f, 1.2f, phaseT) * 10f) / 10f;
                speed = Mathf.Round(Mathf.Lerp(11f, 9f, phaseT) * 10f) / 10f;
                itemCount = Mathf.RoundToInt(Mathf.Lerp(4, 6, phaseT));
            }
            else
            {
                // Phase 3: Brutal — lots of items, fast pace
                float phaseT = (localT - 0.6f) / 0.4f;
                phaseT = Mathf.Clamp01(phaseT);
                duration = Mathf.Round(Mathf.Lerp(55f, 72f, phaseT));
                delay = Mathf.Round(Mathf.Lerp(1.2f, 0.8f, phaseT) * 10f) / 10f;
                speed = Mathf.Round(Mathf.Lerp(9f, 7f, phaseT) * 10f) / 10f;
                itemCount = Mathf.RoundToInt(Mathf.Lerp(6, 8, phaseT));
            }

            waves.Add(new SpawnSettingsEntry
            {
                waveDuration = duration,
                moveDelay = delay,
                moveSpeed = speed,
                itemsPerWave = itemCount
            });
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
