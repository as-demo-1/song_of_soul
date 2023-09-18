using DG.Tweening;
using Opsive.UltimateInventorySystem.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipSlot : MonoBehaviour, ISelectHandler
{
    public Image Img;

    public Text itemName;

    public GameObject selected;

    public Image titleImgBg;
    public Text titleCount;
    public Text titleName;
    public TextMeshProUGUI titleDescription;

    private UIEquipView uiEquipView;

    private string Name;
    private string description;
    private int Count;

    private bool isEmpty;
	public int tabIndex;
	public int slotIndex;

	// Start is called before the first frame update
	// Start is called before the first frame update
	void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }	
    
    public void OnSelect(BaseEventData eventData)
	{
        if (uiEquipView != null)
        {
            uiEquipView.selectedEquip = this;
        }
        if (Img.sprite == null)
        {
			titleImgBg.gameObject.SetActive(false);
		}
        else
        {
            titleImgBg.sprite = Img.sprite;
			titleImgBg.gameObject.SetActive(true);
		}
        selected.SetActive(true);
        titleDescription.text = description;
        titleName.text = Name;
        titleCount.text = Count.ToString();
        selected.transform.DOMove(this.transform.position, 0.5f);
	}

    
    public void Init(Item item, UIEquipView UIEquipView,int Amount)
    {			
        uiEquipView = UIEquipView;
        if (item == null)
        {
            Clear();
		}
        else
        {

            Name = InventoryMethod.GetItemAttributeAsString(item, "NameSid");
            Img.sprite = InventoryMethod.GetItemAttributeAsSprite(item, "Icon");
            Img.gameObject.SetActive(true);
            description = InventoryMethod.GetItemAttributeAsString(item, "DescSid"); ;
            //Count = item.GetItemObjectCount();
			Count = Amount;
            
		}
    }

    /// <summary>
    /// 空格子
    /// </summary>
    public void Clear()
    {
        description = null;
        Name = null;
        Count = 0;

        itemName.text = null;
        titleName.text = null;
        titleDescription.text = null;
        titleCount.text = null;
        Img.sprite = null;
        titleImgBg.gameObject.SetActive(false);
        titleImgBg.sprite = null;
        Img.gameObject.SetActive(false);
    }

    /// <summary>
    /// 退出时清空面板信息
    /// </summary>
    public void Exict()
    {
		selected.SetActive(false);
		titleName.text = null;
		titleDescription.text = null;
		titleCount.text = null;
		titleImgBg.sprite = null;
		titleImgBg.gameObject.SetActive(false);

	}
}
