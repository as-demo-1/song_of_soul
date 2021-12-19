using UnityEngine;
using System.Collections;

public class InteractLoad : MonoBehaviour
{
    public InteractiveContainerSO InteractiveContainer;
    public GameObject ItemPrefab;

    // Use this for initialization
    void Start()
    {
        foreach (InteractiveSO interactiveItem in InteractiveContainer.InteractiveItem)
        {
            GameObject go = Instantiate(ItemPrefab, transform);
            go.transform.position = interactiveItem.Coord;

            SpriteRenderer npcSprite = go.GetComponent<SpriteRenderer>();
            NPCController npcController = go.AddComponent<NPCController>();

            npcSprite.sprite = interactiveItem.Icon;
            npcSprite.flipX = !interactiveItem.IsFaceRight;
            npcController.InteractiveItem = interactiveItem;
        }
    }
}
