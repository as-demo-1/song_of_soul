using System.Collections.Generic;
using UnityEngine;

public abstract class InteractiveSO : ScriptableObject
{
    [Tooltip("The id of interactive item")]
    [SerializeField] private int _interactiveID = default;
    [Tooltip("The name of the item")]
    [SerializeField] private string _name = default;
    [Tooltip("The content of the item")]
    [SerializeField] private string _content = default;
    [Tooltip("The icon of the item")]
    [SerializeField] private Sprite _icon = default;
    [Tooltip("The coord of the item")]
    [SerializeField] private Vector3 _position = default;
    [Tooltip("The face of the item")]
    [SerializeField] private bool _is_face_right = default;

    public int InteractiveID => _interactiveID;
    public string Name => _name;
    public virtual EInteractiveItemType ItemType { get; }
    public string Content => _content;
    public Sprite Icon => _icon;
    public Vector3 Position { get => _position; set { _position = value; } }
    public bool IsFaceRight => _is_face_right;

    public void Init(InteractLoad load)
    {
        InitChild(load);
    }

    public void SetPosition(GameObject go, Vector3 pos)
    {
        SetPositionChild(go, pos);
    }

    protected virtual void SetPositionChild(GameObject go, Vector3 pos)
    {
        Position = pos;
    }

    protected void SetField(GameObject go)
    {
        go.transform.position = Position;

        SpriteRenderer interactiveObjectSprite = go.GetComponent<SpriteRenderer>();
        InteractiveObjectTrigger interactiveObjectTrigger = go.AddComponent<InteractiveObjectTrigger>();

        interactiveObjectSprite.sprite = Icon;
        interactiveObjectSprite.flipX = !IsFaceRight;
        interactiveObjectTrigger.InteractiveItem = this;
    }

    protected virtual GameObject InitChild(InteractLoad load)
    {
        GameObject go = Instantiate(load.ItemPrefab, load.transform);
        SetField(go);
        return go;
    }
}
