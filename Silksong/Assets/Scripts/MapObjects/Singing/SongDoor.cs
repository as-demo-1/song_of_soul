using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongDoor : MonoBehaviour, SingComponent
{   
    bool ifMove = false;
    public List<RisingDoor> doors;
    public void WhenSinging()
    {
        if (!ifMove)
        {
            ifMove = true;
            for (int i = 0; i < doors.Count; i++)
            {
                doors[i].Rising();
            }
        }
    }

}
