using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "container", menuName = "Interactive/container")]
public class InteractiveContainerSO : ScriptableObject
{
    [Tooltip("The list of the InteractiveItem")]
    [SerializeField] private List<InteractiveBaseSO> _interactive_item_list = default;

    public List<InteractiveBaseSO> InteractiveItemList => _interactive_item_list;
}
