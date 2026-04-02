using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ItemTypes/Item/Spawner")]
public class ItemTypes : ScriptableObject
{
    public List<GameObject> items;

    public List<Color> colors = new List<Color>()
    {
        new Color(0.85f, 0.55f, 0.55f),
        new Color(0.55f, 0.65f, 0.85f),
        new Color(0.60f, 0.80f, 0.65f),
        new Color(0.90f, 0.82f, 0.55f),
    };
}
