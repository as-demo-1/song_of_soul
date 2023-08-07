using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Opsive.UltimateInventorySystem.DatabaseNames.DemoInventoryDatabaseNames;
using Opsive.UltimateInventorySystem.Core.AttributeSystem;

public class ItemPick : MonoBehaviour
{
	// Start is called before the first frame update

	//确定碰撞获取
	private CircleCollider2D circleCollider2D;


	[SerializeField] private LayerMask playerLayerMask;
	[SerializeField] private LayerMask groundLayerMask;

	public GameObject text;
	public ItemObject item;
	private Inventory inventory;

	private bool isPicking = false;

	void Start()
    {
		circleCollider2D = GetComponent<CircleCollider2D>();
		inventory = InventorySystemManager.GetInventoryIdentifier(GameManager.Instance.saveSystem.SaveData.inventoryIndex).Inventory;
		
		
	}

	//吸引用球状碰撞体
	private void OnTriggerEnter2D(Collider2D collision)
	{
		text.SetActive(true);
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if(Input.GetKeyDown(KeyCode.E))
		{
			if (!isPicking)
			{
				Debug.Log("发生");
				inventory.GetItemCollection("Main").AddItem(item.Item, 1);
				Debug.Log("拥有："+inventory.GetItemCollection("Main").GetItemAmount(item.Item));
				isPicking = true;
				Destroy(this.gameObject);
			}
			
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		text.SetActive(false);
	}



	// Update is called once per frame
	void Update()
    {
        
    }
}
