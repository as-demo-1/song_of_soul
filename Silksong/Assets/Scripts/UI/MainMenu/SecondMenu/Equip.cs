using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Equip : MonoBehaviour
{
    public Fragment fragment;

    public GameObject[] panel;
    public int secondIndex = 0;
    public List<Button> btns;

    private void Start()
    {
        ChildPanel(secondIndex);
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
