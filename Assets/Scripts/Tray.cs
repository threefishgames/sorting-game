using System.Collections.Generic;
using UnityEngine;

public class Tray : MonoBehaviour
{
    public float offset = 0.3f;
    private List<GameObject> trayItems;
    

    public List<Item> items;
    public void SetTrayItems(Item[] items)
    {
        if (trayItems.Count == 0)
        {
            trayItems = new List<GameObject>();
            foreach (Item i in items)
            {
                
            }
        }
            
    }

    [ContextMenu("Add Item")]
    public void SpawnTray()
    {
        
        
        GameObject tray = new GameObject("CurrentTray");
        tray.transform.parent = transform;

        float xPos = transform.position.x;
        float yPos = transform.position.y;
        foreach (Item i in items)
        {
            SpriteRenderer sr = i.gameObject.GetComponent<SpriteRenderer>();
            float halfWidth = sr.bounds.size.x / 2f;

            xPos += halfWidth;
            Vector2 pos = new Vector2(xPos, yPos);
            GameObject item = Instantiate(i.gameObject, pos, Quaternion.identity, tray.transform);
            xPos += halfWidth + offset;
        }

    }
}
