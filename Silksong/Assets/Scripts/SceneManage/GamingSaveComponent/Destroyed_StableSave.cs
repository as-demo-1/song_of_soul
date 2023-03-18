using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyed_StableSave : GamingSaveObj<bool>
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
        if (GameManager.Instance.saveSystem.ContainDestroyedGameObj(GUID))
        {
            gameObject.SetActive(false);
        }
        return true;
    }

    public override void saveGamingData(bool v)
    {
        GameManager.Instance.saveSystem.AddDestroyedGameObj(GUID);
    }
}
