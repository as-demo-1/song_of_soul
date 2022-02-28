using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "container", menuName = "Interactive/container")]
public class InteractiveContainerSO : ScriptableObject
{
    [Tooltip("The list of the InteractiveItem")]
    [SerializeField] private List<InteractiveSO> _interactive_item = default;

    public List<InteractiveSO> InteractiveItemList => _interactive_item;
}
