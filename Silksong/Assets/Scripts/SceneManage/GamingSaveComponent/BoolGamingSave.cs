using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoolGamingSave : GamingSaveObj<bool>
{
    public override bool loadGamingData(out bool ifError)
    {
        return GameManager.Instance.gamingSave.getBoolGamingData(GUID,out ifError);
    }

    public override void saveGamingData(bool data)
    {
        GameManager.Instance.gamingSave.addBoolGamingData(data, GUID);
    }
}
