using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
/// <summary>
/// 用于管理护符的UI交互
/// </summary> 作者：次元

public class CharmUIPanel : MonoBehaviour
{
    /// <summary>
    /// 可以装备护符
    /// </summary> 用于控制护符的可装备状态，可以设置为在休息点才能更换护符
    [SerializeField]
    private bool ableEquipCharm;
    public bool AbleEquipCharm { get => ableEquipCharm; set => ableEquipCharm = value; }

    [SerializeField]
    private CharmListSO CharmListSO = default;

    [SerializeField]
    private Transform charmCollectionGrid;

    [SerializeField]
    private Transform charmEquipGrid;

    private List<GameObject> charmObjects = new List<GameObject>();

    [SerializeField]
    private GameObject charmPrefab;


    [SerializeField]
    private Text charmName;
    [SerializeField]
    private Text charmDescription;

    [SerializeField]
    private EventSystem EventSystem;

    [SerializeField]
    private List<GameObject> blueSlots = new List<GameObject>();

    [SerializeField]
    private List<GameObject> purpleSlots = new List<GameObject>();

    [SerializeField]
    private List<GameObject> orangeSlots = new List<GameObject>();

    private Dictionary<CharmQuality, List<GameObject>> slotDic = new Dictionary<CharmQuality, List<GameObject>>();

    public List<Button> btns;

    private void Awake()
    {
        slotDic[CharmQuality.BLUE] = blueSlots;
        slotDic[CharmQuality.PURPLE] = purpleSlots;
        slotDic[CharmQuality.ORANGE] = orangeSlots;
    }
    // Start is called before the first frame update
    void Start()
    {
        EventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        foreach (var item in CharmListSO.Charms)
        {
            GameObject charm = Instantiate(charmPrefab, charmCollectionGrid);
            charmObjects.Add(charm);
            CharmImage charmImage = charm.GetComponent<CharmImage>();
            charmImage.SetSOData(item, CharmListSO, this);
            if (item.HasEquiped)
            {
                TryEquipCharm(charm);
            }
        }
        RefreshIcon();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ShowCharmText(CharmSO _charm)
    {
        if (_charm==null)
        {
            charmName.text = "";
            charmDescription.text = "";
        }
        else
        {
            charmName.text = _charm.CharmName;
            charmDescription.text = _charm.effectText;
        }
    }
    public void RefreshIcon()
    {
        foreach (var item in charmObjects)
        {
            CharmImage charmImage = item.GetComponent<CharmImage>();
            if (charmImage.charmSO.HasEquiped)
            {
                charmImage.CharmEquipDisplay();
            }
            else if (charmImage.charmSO.HasCollected)
            {
                charmImage.CharmCollectDisplay();
            }
            else
            {
                charmImage.CharmLockDisplay();
            }
        }
    }
    public bool TryEquipCharm(GameObject _icon)
    {
        if (!AbleEquipCharm)
        {
            return false;
        }

        CharmSO _charmSO = _icon.GetComponent<CharmImage>().charmSO;
        foreach (var slot in slotDic[_charmSO.CharmQuality])
        {
            CharmImage slotImage = slot.GetComponent<CharmImage>();
            if (slotImage.SlotEmpty)
            {
                CharmListSO.EquipCharm(_charmSO);
                slotImage.charmSO = _charmSO;
                slotImage.CharmSlotDisplay(true);
                slotImage.SlotEmpty = false;

                _icon.GetComponent<CharmImage>().CharmEquipDisplay();
                return true;
            }
        }
        return false;
    }
    public bool TryDisEquipCharm(GameObject _slot)
    {
        if (!AbleEquipCharm)
        {
            return false;
        }
        CharmImage slotImage = _slot.GetComponent<CharmImage>();
        CharmListSO.DisEquipCharm(slotImage.charmSO);
        slotImage.CharmSlotDisplay(false);
        slotImage.SlotEmpty = true;
        slotImage.charmSO = null;
        RefreshIcon();
        return true;
    }

}
