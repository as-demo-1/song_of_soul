using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using System.Security.Claims;

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
    private GridLayoutGroup purpleCharmGrid;

	[SerializeField]
	private GridLayoutGroup orangeCharmGrid;

	private List<CharmImage> blueCharmImages = new List<CharmImage>();
	private List<CharmImage> purpleCharmImages = new List<CharmImage>();
	public List<Charm> orangeCharmImages = new List<Charm>();    

    [SerializeField]
    private GameObject charmPrefab;


    [SerializeField]
    private Text charmName;
    [SerializeField]
    private TextMeshProUGUI charmDescription;

    private int indexChram = 0;

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
    private float selecterSize = 60.0f;
    
    /// <summary>
    /// 当前选中的护符
    /// </summary>
    public CharmImage selectCharm;
    public CharmImage orangeCharmImage;

	private void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        Init();
	}

    public void Init()
    {
        if (CharmListSO.Charms.Count <= 0)
            return;
        foreach (Charm charm in CharmListSO.Charms)
        {
            if (!charm.HasCollected) continue;
            if (charm.CharmQuality.Equals(CharmQuality.ORANGE))
            {
                orangeCharmImages.Add(charm);
            }
            else
            {
                CharmImage charmImage = Instantiate(charmPrefab, charmCollectionGrid.transform).GetComponent<CharmImage>();
                if (charm.CharmQuality.Equals(CharmQuality.BLUE))
                {
                    blueCharmImages.Add(charmImage);
                }
                else if (charm.CharmQuality.Equals(CharmQuality.PURPLE))
                {
                    charmImage.transform.SetParent(purpleCharmGrid.transform);
                    charmImage.GetComponent<RectTransform>().sizeDelta = purpleCharmGrid.cellSize;
                    purpleCharmImages.Add(charmImage);
                }
                charmImage.Init(charm, this);

                if (charm.HasEquiped)
                {
                    CharmToSlot(charmImage);
                }
            }
        }
        if(orangeCharmImages != null)
        {
			orangeCharmImage = Instantiate(charmPrefab, slotDic[orangeCharmImages[0].CharmQuality][0].transform).GetComponent<CharmImage>();
			orangeCharmImage.GetComponent<RectTransform>().sizeDelta = slotDic[orangeCharmImages[0].CharmQuality][0].GetComponent<RectTransform>().sizeDelta;
			orangeCharmImage.Init(orangeCharmImages[0], this);
		}
        Debug.Log("护符初始化完成");
        
    }

    private void OnEnable()
    {
        Debug.Log(blueCharmImages.Count);
        if (blueCharmImages.Count <= 0) return;

        //OnTabChange();
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
            if (selectCharm.charm.CharmQuality == CharmQuality.ORANGE) return;

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
        if (selectCharm.charm.CharmQuality.Equals(CharmQuality.PURPLE))
        {
            selectCharm.transform.SetParent(purpleCharmGrid.transform);
            if(purpleCharmGrid.gameObject.activeSelf == false)
            {
				selectCharm=blueCharmImages[0];
				EventSystem.current.SetSelectedGameObject(selectCharm.gameObject);
			}
        }
        else
        {
            selectCharm.transform.SetParent(charmCollectionGrid.transform);

			if (purpleCharmGrid.gameObject.activeSelf == true)
			{
				selectCharm=purpleCharmImages[0];
				EventSystem.current.SetSelectedGameObject(selectCharm.gameObject);
			}
		}
        /*if(purpleCharmGrid.gameObject.activeSelf)
        {
			ChangeSelect(purpleCharmImages[0]);
		}
        else
        {
			ChangeSelect(blueCharmImages[0]);
		}*/
        //ChangeSelect(selectCharm);
        //selectCharm.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutCubic);
    }

    public void ChangeSelect(CharmImage _charmImage)
    {
        selectCharm = _charmImage;
        selecter.transform.SetParent(_charmImage.transform);
        //selecter.DOMove(selectCharm.transform.position, 0.5f).SetEase(Ease.InOutCubic);
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
        if (purpleCharmGrid.gameObject.activeInHierarchy)
        {
            if (purpleCharmImages.Count > 0)
            {
                GameObject.Find("EventSystem(Clone)").GetComponent<EventSystem>()?.
                    SetSelectedGameObject(purpleCharmImages[0].gameObject);
                ChangeSelect(purpleCharmImages[0]);
            }
            else
            {
                selectCharm = null;
            }
        }
        else if(charmCollectionGrid.gameObject.activeInHierarchy)
        {
            if (blueCharmImages.Count > 0)
            {
                GameObject.Find("EventSystem(Clone)").GetComponent<EventSystem>()?.
                    SetSelectedGameObject(blueCharmImages[0].gameObject);
                ChangeSelect(blueCharmImages[0]);
            }
            else
            {
                selectCharm = null;
            }
        }
    }

    /// <summary>
    /// 大护符的刷新
    /// </summary>
    public void RefreshUI()
    {		
        if (orangeCharmImages.Count == 0)
            return;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            indexChram++;
            indexChram = indexChram < orangeCharmImages.Count ? indexChram : 0;
            orangeCharmImage.Init(orangeCharmImages[indexChram], this);
            if (selectCharm.charm.CharmQuality == CharmQuality.ORANGE)
            {
                ChangeSelect(orangeCharmImage);

			}
        }
		if (Input.GetKeyDown(KeyCode.E))
		{
			indexChram--;
			indexChram = indexChram >= 0 ? indexChram : orangeCharmImages.Count-1;
            orangeCharmImage.Init(orangeCharmImages[indexChram], this);
			if (selectCharm.charm.CharmQuality == CharmQuality.ORANGE)
			{
				ChangeSelect(orangeCharmImage);

			}
		}

	}

}
