using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Equip : MonoBehaviour
{
    public List<Button> btns;

    public Fragment fragment;

    public GameObject[] panel;
    public int secondIndex = 0;



    public void ThirdButton(int childButtonIndex)
    {
        switch (childButtonIndex)
        {
            case 0:
                {
                    secondIndex++;
                    secondIndex = secondIndex > fragment.btns.Count - 1 ? 0 : secondIndex;
                    fragment.btns[secondIndex].Select();
                }
                break;
            case 1:
                {

                }
                break;
            case 2:
                {
                }
                break;
            case 3:
                {
                }
                break;
            case 4:
                {

                }
                break;
        }
    }


    public void ChildPanel(int index)
    {
        for (int i = 0; i < panel.Length; i++)
        {
            if (i == index)
            {
                if (!panel[i].activeSelf)
                {
                    panel[i].SetActive(true);
                }
            }
            else
            {
                if (panel[i].activeSelf)
                    panel[i].SetActive(false);
            }
        }
    }
}
