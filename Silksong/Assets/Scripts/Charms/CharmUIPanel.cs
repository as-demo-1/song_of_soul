using System;
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
    private GridLayoutGroup charmCollectionGrid;
    
    [SerializeField]
    private GridLayoutGroup orangeCharmGrid;
    
    private List<CharmImage> charmImages = new List<CharmImage>();
    private List<CharmImage> orangeCharmImages = new List<CharmImage>();

    [SerializeField]
    private GameObject charmPrefab;


    [SerializeField]
    private Text charmName;
    [SerializeField]
    private Text charmDescription;



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

    /// <summary>
    /// 护符槽字典
    /// </summary>
    [SerializeField]
    private Dictionary<CharmQuality, List<CharmSlot>> slotDic =
        new Dictionary<CharmQuality, List<CharmSlot>>(); 
    //public List<Button> btns;

    /// <summary>
    /// 选择框
    /// </summary>
    public RectTransform selecter;

    [SerializeField]
    private float selecterSize = 15.0f;
    
    /// <summary>
    /// 当前选中的护符
    /// </summary>
    public CharmImage selectCharm;

    private void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init()
    {
        foreach (Charm charm in CharmListSO.Charms)
        {
            if (!charm.HasCollected) continue;

            CharmImage charmImage = Instantiate(charmPrefab, charmCollectionGrid.transform).GetComponent<CharmImage>();
            if (charm.CharmQuality.Equals(CharmQuality.ORANGE))
            {
                charmImage.transform.SetParent(orangeCharmGrid.transform);
                charmImage.GetComponent<RectTransform>().sizeDelta  = orangeCharmGrid.cellSize; 
                orangeCharmImages.Add(charmImage);
            }
            else
            {
                charmImages.Add(charmImage);
            }
            
            charmImage.Init(charm, this);
            
            if (charm.HasEquiped)
            {
                CharmToSlot(charmImage);
            }
        }
        Debug.Log("护符初始化完成");
        
    }

    private void OnEnable()
    {
        Debug.Log(charmImages.Count);
        if (charmImages.Count <= 0) return;

        OnTabChange();
        //GameObject.Find("EventSystem(Clone)").GetComponent<EventSystem>()?.SetSelectedGameObject(charmImages[0].gameObject);
        PlayerInput.Instance.ReleaseControls();
    }

    private void OnDisable()
    {
        //PlayerInput.Instance.GainControls();
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
                
                ChangeSelect(_charmImage);
                
                return;
            }
        }

        // 装配失败，图标抖动
        _charmImage.transform.DOShakePosition(0.5f, 10.0f);
    }

    public void DisequipCharm()
    {
        selectCharm.slot.isEmpty = true;
        selectCharm.slot = null;
        selectCharm.charm.HasEquiped = false;
        if (selectCharm.charm.CharmQuality.Equals(CharmQuality.ORANGE))
        {
            selectCharm.transform.SetParent(orangeCharmGrid.transform);
        }
        else
        {
            selectCharm.transform.SetParent(charmCollectionGrid.transform);
        }
        //ChangeSelect(selectCharm);
        //selectCharm.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutCubic);
    }

    public void ChangeSelect(CharmImage _charmImage)
    {
        selectCharm = _charmImage;
        selecter.transform.SetParent(_charmImage.transform);
        //selecter.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutCubic);
        selecter.offsetMin = -selecterSize * Vector2.one;
        selecter.offsetMax = selecterSize * Vector2.one;
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

    public void OnTabChange()
    {
        if (orangeCharmGrid.gameObject.activeInHierarchy)
        {
            if (orangeCharmImages.Count > 0)
            {
                GameObject.Find("EventSystem(Clone)").GetComponent<EventSystem>()?.
                    SetSelectedGameObject(orangeCharmImages[0].gameObject);
                ChangeSelect(orangeCharmImages[0]);
            }
            else
            {
                selectCharm = null;
            }
        }
        else if(charmCollectionGrid.gameObject.activeInHierarchy)
        {
            if (charmImages.Count > 0)
            {
                GameObject.Find("EventSystem(Clone)").GetComponent<EventSystem>()?.
                    SetSelectedGameObject(charmImages[0].gameObject);
                ChangeSelect(charmImages[0]);
            }
            else
            {
                selectCharm = null;
            }
        }
    }

}
