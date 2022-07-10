using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Picture : MonoBehaviour
{

    public List<Button> btns = new List<Button>();

    public MonsterPicture minsterPicture;
    public int secondIndex = 0;

    public void ThirdButton(int childButtonIndex)
    {
        switch (childButtonIndex)
        {
            case 0:
                {
                    
                    secondIndex++;
                    secondIndex = secondIndex > minsterPicture.btns.Count - 1 ? 0 : secondIndex;
                    minsterPicture.btns[secondIndex].Select();
                    minsterPicture.MoveDown(secondIndex);
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
        }
    }
}
