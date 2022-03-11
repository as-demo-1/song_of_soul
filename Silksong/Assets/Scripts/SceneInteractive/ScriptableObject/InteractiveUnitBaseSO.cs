using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "unit", menuName = "Interactive/unit")]
public class InteractiveUnitBaseSO : ScriptableObject, IInteractiveUnitSO
{
    [Tooltip("The content of the item")]
    [SerializeField] private string _content = default;
    [Tooltip("The icon of the item")]
    [SerializeField] private Sprite _icon = default;
    [Tooltip("The coord of the item")]
    [SerializeField] private Vector3 _position = default;
    [Tooltip("The face of the item")]
    [SerializeField] private bool _is_face_right = default;

    public string Content => _content;
    public Sprite Icon => _icon;
    public Vector3 Position { get => _position; set { _position = value; } }
    public bool IsFaceRight => _is_face_right;
}
