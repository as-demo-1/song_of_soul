using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingleInteractiveBaseSO : InteractiveBaseSO, IInteractiveUnitSO
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

    protected virtual void SetField(GameObject go)
    {
        go.transform.position = Position;

        SpriteRenderer interactiveObjectSprite = go.GetComponent<SpriteRenderer>();
        InteractiveObjectTrigger interactiveObjectTrigger = go.AddComponent<InteractiveObjectTrigger>();

        interactiveObjectSprite.sprite = Icon;
        interactiveObjectSprite.flipX = !IsFaceRight;
        interactiveObjectTrigger.InteractiveItem = this;

        UIComponentManager.Instance.SetText(go, InteractConstant.UITipText, Content);
    }

    protected override GameObject InitChild(InteractLoad load)
    {
        GameObject go = Instantiate(load.ItemPrefab, load.transform);
        SetField(go);
        return go;
    }

    protected override void SetPositionChild(GameObject go, Vector3 pos)
    {
        Position = pos;
    }
}
