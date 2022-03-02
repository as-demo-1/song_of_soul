using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "portal", menuName = "Interactive/func/portal")]
public class PortalFuncInteractiveSO : FuncInteractiveBaseSO
{
    [SerializeField]
    [HideInInspector]
    private EFuncInteractItemType _funcInteractItemType = EFuncInteractItemType.PORTAL;

    public override EFuncInteractItemType FuncInteractItemType => _funcInteractItemType;

    [Tooltip("The pos of point a of the portal")]
    [SerializeField] private Vector3 _pos_a = default;
    [Tooltip("The icon of point a of the portal")]
    [SerializeField] private Sprite _icon_a = default;
    [Tooltip("The pos of point b of the portal")]
    [SerializeField] private Vector3 _pos_b = default;
    [Tooltip("The icon of point b of the portal")]
    [SerializeField] private Sprite _icon_b = default;

    public Vector3 PosA => _pos_a;
    public Sprite IconA => _icon_a;
    public Vector3 PosB => _pos_b;
    public Sprite IconB => _icon_b;

    protected override GameObject InitChild(InteractLoad load)
    {
        GameObject parent = Instantiate(new GameObject("portal"), load.transform);

        GameObject a = Instantiate(load.ItemPrefab, parent.transform);
        GameObject b = Instantiate(load.ItemPrefab, parent.transform);

        SetField(a);
        a.transform.position = PosA;
        SpriteRenderer aSprite = a.GetComponent<SpriteRenderer>();
        aSprite.sprite = IconA;
        PortalController portalA = a.AddComponent<PortalController>();
        portalA.Other = b;
        portalA.Point = "A";

        SetField(b);
        b.transform.position = PosB;
        SpriteRenderer bSprite = b.GetComponent<SpriteRenderer>();
        bSprite.sprite = IconB;
        PortalController portalB = b.AddComponent<PortalController>();
        portalB.Other = a;
        portalB.Point = "B";

        return parent;
    }

    protected override void SetPositionChild(GameObject go, Vector3 pos)
    {
        string point = go.GetComponent<PortalController>().Point;

        if (point == "A")
        {
            _pos_a = pos;
        }
        else if (point == "B")
        {
            _pos_b = pos;
        }
    }

    protected override void DoInteract()
    {
        PortalController portal = InteractManager.Instance.GetInteractiveItemComponent<PortalController>();
        Debug.Log("todo: teleport to：" + portal.Other.transform.position);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = portal.Other.transform.position;
        base.DoInteract();
    }
}
