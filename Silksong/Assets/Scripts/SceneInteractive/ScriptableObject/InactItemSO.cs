using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "item", menuName = "NewInact/item")]
public class InactItemSO : ScriptableObject, IInact
{
    [HideInInspector]
    [SerializeField] private string _guid;
    public string Guid => _guid;
    [SerializeField]
    [HideInInspector]
    protected int _iid = default;
    public int Iid => _iid;
    [SerializeField]
    protected InactChildBaseSO _inactChild = default;
    public InactChildBaseSO InactChild => _inactChild;

    [HideInInspector]
    [SerializeField]
    public InactChildBaseSO CurInactChild = default;

    [SerializeField]
    [HideInInspector]
    private InactContainerSO _parent;
    public InactContainerSO GetParent => _parent;

    [Tooltip("The content of the item")]
    [SerializeField] private string _content = default;
    [Tooltip("The icon of the item")]
    [SerializeField] private Sprite _icon = default;
    public string Content => _content;
    public Sprite Icon => _icon;

    [SerializeField] private EInact2 _inact_item_type = default;
    public EInact2 InactItemType => _inact_item_type;

    //[SerializeField]
    //[HideInInspector]
    private GameObject _go;
    public GameObject Go => _go;

    public void AddGo(GameObject go, bool isPoint = false)
    {
        _go = go;

        SpriteRenderer spriteRender = go.GetComponent<SpriteRenderer>();
        spriteRender.sprite = Icon;
        if (!isPoint)
        {
            UIComponentManager.Instance.SetText(go, InteractConstant.UITipText, Content);
        }
    }

    public void Init(InactContainerSO parent)
    {
        _iid = InteractSystem.Instance.SetIid(this);
        _parent = parent;
        if (InactChild != null)
        {
            InactChild.Init(this);
        }
        else
        {
            CurInactChild = default;
        }
    }

    public void Interact()
    {
        InteractManager.Instance.StopEvent();
        CurInactChild = InactChild;
        InactChild.Next();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        var path = AssetDatabase.GetAssetPath(this);
        _guid = AssetDatabase.AssetPathToGUID(path);
    }
#endif

    public void Stop()
    {
        InteractManager.Instance.ContinueEvent();
    }

    public void Next()
    {
        CurInactChild?.Next();
    }

    public void GoThere()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = _go.transform.position;
    }
}
