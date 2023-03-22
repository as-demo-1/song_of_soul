using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
[System.Serializable]
public class Charm
{
    /// <summary>
    /// 护符名称
    /// </summary>
    [VerticalGroup("护符描述"), LabelWidth(60)]
    [LabelText("护符名称")]
    public string CharmName;
    /// <summary>
    /// 护符图片
    /// </summary>
    [PreviewField(Height = 90)] [TableColumnWidth(90, Resizable = false)]
    public Sprite charmImage;

    /// <summary>
    /// 是否已获得该护符
    /// </summary>
    bool hasCollected;
    [ShowInInspector] [VerticalGroup("护符状态"), LabelWidth(90)]
    public bool HasCollected { get => hasCollected; set => hasCollected = value; }

    /// <summary>
    /// 是否已装备该护符
    /// </summary>
    bool hasEquiped;
    [ShowInInspector] [VerticalGroup("护符状态"), LabelWidth(80)]
    public bool HasEquiped
    {
        get => hasEquiped;
        set
        {
            hasEquiped = value;
            ChangeEquip();
        }
    }


    /// <summary>
    /// 护符品质
    /// </summary>
    [Tooltip("护符品质")] 
    [SerializeField] [ShowInInspector] [VerticalGroup("护符状态"), LabelWidth(80)]
    public CharmQuality CharmQuality;

    /// <summary>
    /// 护符效果描述
    /// </summary>
    [Tooltip("护符效果文字描述")]
    [SerializeField]
    [TextArea(4,4)] [VerticalGroup("护符描述"), LabelWidth(40)]
    public string effectText;

    // <summary>
    /// 护符背景描述
    /// </summary>
    [LabelText("护符描述")]
    [SerializeField]
    [TextArea(4,4)] [VerticalGroup("护符描述"), LabelWidth(40)]
    public string desText;

    /// <summary>
    /// 是否是易碎护符
    /// </summary>
    [SerializeField] [VerticalGroup("护符状态"), LabelWidth(80)]
    [Tooltip("是否为易碎护符")]
    private bool isFragile;

    [TableColumnWidth(90)]
    public List<CharmEffect> effects = new List<CharmEffect>();
    

    private BuffManager bm;

    public void InitCharm(BuffManager _bm)
    {
        bm = _bm;
        if (HasEquiped)
        {
            ChangeEquip();
        }
    }
    public void ChangeEquip()
    {
        if (!bm) return;
        
        
            
        
        foreach (var charmEffect in effects)
        {
            if (HasEquiped)
            {
                bm.AddBuff(charmEffect.Property, charmEffect.val);
            }
            else
            {
                bm.DecreaseBuff(charmEffect.Property, charmEffect.val);
            }
        }
    }
    

}
