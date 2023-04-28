using DG.Tweening;
using Opsive.UltimateInventorySystem.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipSlot : MonoBehaviour, ISelectHandler
{
    public Image Img;

    public Image selected;

    public Text itemName;
    public TextMeshProUGUI description;

    private string name;
    private string descriptionText;
    
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
        itemName.text = name;
        description.text = descriptionText;
        selected.transform.DOMove(this.transform.position, 0.5f);
	}

    public void Init(Item item)
    {

		item.TryGetAttributeValue<string>("NameSid", out var NameSid);
		name = NameSid;
        item.TryGetAttributeValue<Sprite>("Icon", out var Icon);
        Img.sprite = Icon;
        Img.gameObject.SetActive(true);
        item.TryGetAttributeValue<string>("DescSid", out var DescSid);
        descriptionText = DescSid;

    }
}
