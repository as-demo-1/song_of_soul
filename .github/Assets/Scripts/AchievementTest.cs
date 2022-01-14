using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class AchievementTest : MonoBehaviour
{
    int hitmarker = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (!SteamManager.Initialized)
        {
            return;
        }
        else
        {
            //SteamUserStats.GetStat("_achive", out _var);
        }

        string name = SteamFriends.GetPersonaName(); //Test for Steam is work
        Debug.Log(name);
        SteamUserStats.ResetAllStats(true);
    }

    // Update is called once per frame
     private void Update()
    {
        if (!SteamManager.Initialized)
        {
            return;
        }

        CheckHits();
        // if (!Input.GetKeyDown(KeyCode.Space))
        // {
        //     return;
        // }
        //
        // SteamUserStats.SetAchievement("ACH_WIN_ONE_GAME");
        // SteamUserStats.StoreStats();
    }

     public void AddHit()
     {
         hitmarker++;
     }

     public void CheckHits()
     {
         if (hitmarker >= 5)
         {
             SteamUserStats.SetAchievement("ACH_WIN_ONE_GAME");
             SteamUserStats.StoreStats();
         }
     }
}
