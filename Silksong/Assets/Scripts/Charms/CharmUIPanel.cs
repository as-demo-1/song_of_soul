using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Sirenix.OdinInspector;

/// <summary>
/// 用于管理护符的UI交互
/// </summary> 作者：次元

public class CharmUIPanel : SerializedMonoBehaviour
{
    /// <summary>
    /// 可以装备护符
    /// </summary> 用于控制护符的可装备状态，可以设置为在休息点才能更换护符
    [SerializeField]
    private bool ableEquipCharm;

    [SerializeField]
    private CharmListSO CharmListSO = default;

    [SerializeField]
    private Transform charmCollectionGrid;
    

    private List<Transform> charmTransforms = new List<Transform>();

    [SerializeField]
    private GameObject charmPrefab;


    [SerializeField]
    private Text charmName;
    [SerializeField]
    private Text charmDescription;

    [SerializeField]
    private EventSystem EventSystem;

    // 三种护符槽
    // [SerializeField]
    // private List<GameObject> blueSlots = new List<GameObject>();
    //
    // [SerializeField]
    // private List<GameObject> purpleSlots = new List<GameObject>();
    //
    // [SerializeField]
    // private List<GameObject> orangeSlots = new List<GameObject>();

    //private Dictionary<CharmQuality, List<GameObject>> slotDic = new Dictionary<CharmQuality, List<GameObject>>();

    [SerializeField]
    private Dictionary<CharmQuality, List<CharmSlot>> slotDic =
        new Dictionary<CharmQuality, List<CharmSlot>>(); 
    //public List<Button> btns;

    public RectTransform selecter;
    public CharmImage selectCharm;

    private void Awake()
    {
        Init();
    }
    // Start is called before the first frame update
    void Start()
    {
        // EventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        // foreach (var item in CharmListSO.Charms)
        // {
        //     GameObject charm = Instantiate(charmPrefab, charmCollectionGrid);
        //     charmObjects.Add(charm);
        //     CharmImage charmImage = charm.GetComponent<CharmImage>();
        //     //charmImage.SetData(item, CharmListSO, this);
        //     if (item.HasEquiped)
        //     {
        //         //TryEquipCharm(charm);
        //     }
        // }
        //RefreshIcon();
    }

    public void Init()
    {
        EventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        foreach (Charm charm in CharmListSO.Charms)
        {
            CharmImage charmImage = Instantiate(charmPrefab, charmCollectionGrid).GetComponent<CharmImage>();
            charmImage.Init(charm, this);
            charmTransforms.Add(charmImage.transform);
            if (charm.HasEquiped)
            {
                CharmToSlot(charmImage);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (selectCharm == null) return;
            if (selectCharm.charm.HasEquiped)
            {
                DisequipCharm();
            }
            else
            {
                    
                CharmToSlot(selectCharm);
            }
        }
    }


    public void CharmToSlot(CharmImage _charmImage)
    {
        if (!ableEquipCharm) return;

        foreach (var slot in slotDic[_charmImage.charm.CharmQuality])
        {
            if (slot.isEmpty)
            {
                _charmImage.transform.SetParent(this.transform);
                _charmImage.transform.DOMove(slot.transform.position, 0.3f).SetEase(Ease.InOutCubic);
                _charmImage.slot = slot;
                slot.isEmpty = false;
                _charmImage.charm.HasEquiped = true;
                return;
            }
        }

        _charmImage.transform.DOShakePosition(0.5f, 10.0f);
    }

    public void DisequipCharm()
    {
        selectCharm.slot.isEmpty = true;
        selectCharm.slot = null;
        selectCharm.charm.HasEquiped = false;
        selectCharm.transform.SetParent(charmCollectionGrid);
        //selectCharm.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutCubic);
    }

    public void ChangeSelect(CharmImage _charmImage)
    {
        if (selectCharm && selectCharm.Equals(_charmImage)) return;

        selectCharm = _charmImage;
        selecter.DOMove(selectCharm.transform.position, 0.5f).SetEase(Ease.InOutCubic);
        if (_charmImage==null)
        {
            charmName.text = "";
            charmDescription.text = "";
        }
        else
        {
            charmName.text = _charmImage.charm.CharmName;
            charmDescription.text = _charmImage.charm.effectText;
        }
    }
    // public void RefreshIcon()
    // {
    //     foreach (var item in charmObjects)
    //     {
    //         CharmImage charmImage = item.GetComponent<CharmImage>();
    //         if (charmImage.charmSO.HasEquiped)
    //         {
    //             charmImage.CharmEquipDisplay();
    //         }
    //         else if (charmImage.charmSO.HasCollected)
    //         {
    //             charmImage.CharmCollectDisplay();
    //         }
    //         else
    //         {
    //             charmImage.CharmLockDisplay();
    //         }
    //     }
    // }
    // public bool TryEquipCharm(GameObject _icon)
    // {
    //     if (!AbleEquipCharm)
    //     {
    //         return false;
    //     }
    //
    //     CharmSO _charmSO = _icon.GetComponent<CharmImage>().charmSO;
    //     foreach (var slot in slotDic[_charmSO.CharmQuality])
    //     {
    //         CharmImage slotImage = slot.GetComponent<CharmImage>();
    //         if (slotImage.SlotEmpty)
    //         {
    //             CharmListSO.EquipCharm(_charmSO);
    //             slotImage.charmSO = _charmSO;
    //             slotImage.CharmSlotDisplay(true);
    //             slotImage.SlotEmpty = false;
    //
    //             _icon.GetComponent<CharmImage>().CharmEquipDisplay();
    //             return true;
    //         }
    //     }
    //     return false;
    // }
    // public bool TryDisEquipCharm(GameObject _slot)
    // {
    //     if (!AbleEquipCharm)
    //     {
    //         return false;
    //     }
    //     CharmImage slotImage = _slot.GetComponent<CharmImage>();
    //     CharmListSO.DisEquipCharm(slotImage.charmSO);
    //     slotImage.CharmSlotDisplay(false);
    //     slotImage.SlotEmpty = true;
    //     slotImage.charmSO = null;
    //     RefreshIcon();
    //     return true;
    // }

}
