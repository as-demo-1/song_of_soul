using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildrenSortInOrder : MonoBehaviour
{

    void Start()
    {
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        for(int i=0;i<spriteRenderers.Length;i++)
        {
            spriteRenderers[i].sortingOrder = i;
        }
    }


}
