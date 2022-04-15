using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUpgrade : MonoBehaviour
{

    private int item_amount;

    public List<int> Upgrade_Level;

    private int Current_Level;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void On_ItemAmountChange(int amount)
    {
        item_amount += amount;
        if (Upgrade_Level.Count != 0 )
        {
            if (Upgrade_Level[Current_Level].Equals(item_amount))
            {
                //TODO::血量升级
                Current_Level++;
            }
        }
    }
}
