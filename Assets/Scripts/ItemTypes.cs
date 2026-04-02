using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ItemTypes/Item/Spawner")]
public class ItemTypes : ScriptableObject
{
    public List<GameObject> items;
    public List<Color> colors;
}
