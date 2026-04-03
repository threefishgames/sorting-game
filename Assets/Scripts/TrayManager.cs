using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrayManager : MonoBehaviour
{
    public Tray[] trays;
    public ItemTypes itemTypes;
    public int mockItemCount = 6;

    [ContextMenu("Mock Set Trays")]
    public void MockSetTrays()
    {
        List<ItemData> mockItems = new List<ItemData>();
        for (int i = 0; i < mockItemCount; i++)
        {
            mockItems.Add(new ItemData
            {
                ShapeIndex = Random.Range(0, itemTypes.items.Count),
                ColorIndex = Random.Range(0, itemTypes.colors.Count)
            });
        }
        SetTrays(mockItems);
    }

    public void OnScoreIncreased()
    {
       Debug.Log("Score increased"); 
       GameManager.Instance.OnCorrectSort();
    }

    public void OnScoreDecreased()
    {
       Debug.Log("Score decreased"); 
       GameManager.Instance.OnWrongSort();
    }

    private void Awake()
    {
        GameManager.OnNewWave += GameManagerOnOnNewWave ;
        foreach (Tray tray in trays)
        {
            tray.Init(OnScoreIncreased, OnScoreDecreased);
        }
    }

    private void GameManagerOnOnNewWave(ItemData[] obj)
    {
        SetTrays(obj.ToList());
    }

    public void SetTrays(List<ItemData> itemDatas)
    {
        int trayCount = trays.Length;
        int baseCount = itemDatas.Count / trayCount;
        int remainder = itemDatas.Count % trayCount;

        List<ItemData> shuffled = new List<ItemData>(itemDatas);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        // Randomly pick which trays get an extra item
        List<int> extraIndices = new List<int>();
        for (int i = 0; i < trayCount; i++) extraIndices.Add(i);
        for (int i = extraIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (extraIndices[i], extraIndices[j]) = (extraIndices[j], extraIndices[i]);
        }

        int index = 0;
        for (int i = 0; i < trayCount; i++)
        {
            int count = baseCount + (extraIndices.IndexOf(i) < remainder ? 1 : 0);
            List<ItemData> trayItems = shuffled.GetRange(index, count);
            trays[i].SetTrayItems(trayItems.ToArray());
            index += count;
        }
    }
        
}
