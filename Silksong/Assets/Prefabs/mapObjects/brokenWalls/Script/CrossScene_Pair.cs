using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossScene_Pair : MonoBehaviour
{
    public string pairGuid;
    public string Guid;
    private void OnValidate()
    {
        Guid = GetComponent<GuidComponent>().GetGuid().ToString();
    }
    protected virtual void Awake()
    {
        bool ifPariDie = GameManager.Instance.saveSystem.ContainDestroyedGameObj(pairGuid);
        if (ifPariDie)
        {
            GamingSaveObj<bool> gamingSave;
            if (TryGetComponent(out gamingSave))
            {
                gamingSave.saveGamingData(true);
                gameObject.SetActive(false);
            }
        }
    }
}
