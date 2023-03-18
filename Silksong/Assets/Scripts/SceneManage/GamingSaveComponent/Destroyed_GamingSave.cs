using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GuidComponent))]
public class Destroyed_GamingSave : GamingSaveObj<bool>
{
    private void Awake()
    {
        if (ban) return;
        bool ifError;
        loadGamingData(out ifError);
    }
    public override bool loadGamingData(out bool ifError)
    {
        ifError = false;
        if (GameManager.Instance.gamingSave.checkDestroyedGameObj(GUID))
        {
            gameObject.SetActive(false);
        }
        return true;
    }

    public override void saveGamingData(bool v)
    {
        GameManager.Instance.gamingSave.addDestroyedGameObj(GUID);
    }
}
