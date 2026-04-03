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
}
