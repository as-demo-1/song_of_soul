using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntGamingSave : GamingSaveObj<int>
{
    public override int loadGamingData(out bool ifError)
    {
        return GameManager.Instance.gamingSave.getIntGamingData(GUID,out ifError);
    }

    public override void saveGamingData(int data)
    {
        GameManager.Instance.gamingSave.addIntGamingData(data, GUID);
    }
}
