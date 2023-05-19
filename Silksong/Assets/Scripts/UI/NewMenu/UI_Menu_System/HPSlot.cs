using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPSlot : MonoBehaviour
{
    public Image[] empty;
    public Sprite full;
    public GameObject first;
    public GameObject second;
    public int HpAdd;
	

    public void UpdateHpImg(int i)
    {
		if (i >= 2)
		{
			second.SetActive(true);
		}
		else 
		{
			first.SetActive(true);
			for(int j = 0;j<i;j++)
			{
				empty[j].sprite = full;
			}
		}
    }

    public void UpdateMpSlot(int i)
    {
		if (i >= 4)
		{
			second.SetActive(true);
		}
		else
		{
			first.SetActive(true);
			for(int j = 0; j < i; j++)
			{
				empty[j].sprite = full;
			}
		}
	}

}
