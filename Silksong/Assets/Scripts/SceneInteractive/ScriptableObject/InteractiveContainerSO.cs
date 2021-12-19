using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InteractiveContainer", menuName = "Interactive/InteractiveContainer")]
public class InteractiveContainerSO : ScriptableObject
{
    [Tooltip("The list of the InteractiveItem")]
    [SerializeField] private List<InteractiveSO> _interactive_item = default;

    public List<InteractiveSO> InteractiveItem => _interactive_item;
}
