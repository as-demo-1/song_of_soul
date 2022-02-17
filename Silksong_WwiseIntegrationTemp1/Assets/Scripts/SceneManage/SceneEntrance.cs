using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 场景入口
/// </summary>
public class SceneEntrance : MonoBehaviour
{
    public bool EntranceFaceLeft;
    public enum EntranceTag//标号区分,不够可以增加
    {
        A, B, C, D, E, F, G,
    }

    public EntranceTag destinationTag;

    public static SceneEntrance GetDestination(EntranceTag EntranceTag)
    {

        SceneEntrance[] entrances = FindObjectsOfType<SceneEntrance>();
        for (int i = 0; i < entrances.Length; i++)
        {
            if (entrances[i].destinationTag == EntranceTag)
                return entrances[i];
        }
        Debug.Log("No entrance was found with the " + EntranceTag + " tag.");
        return null;//若场景没有入口 说明不是有玩家的游戏场景 
    }
}
