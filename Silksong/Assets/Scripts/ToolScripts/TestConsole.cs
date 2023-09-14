using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IngameDebugConsole;
using UnityEngine.SceneManagement;

public class TestConsole : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ConsoleMethod("DB", "输出一段话")]
    public static void DebugLog(string str)
    {
        Debug.Log(str);
    }

    [ConsoleMethod("Atk", "增加攻击力")]
    public static void ChangeAttackDamage(int num)
    {
        PlayerController.Instance.playerCharacter.buffManager.AddBuff(BuffProperty.ATTACK, num);
        Debug.Log("主角攻击力增加"+num);
    }

    [ConsoleMethod("Set", "移动到存档点")]
    public static void ResetPos()
    {
        PlayerController.Instance.transform.position =
            GameObject.FindWithTag("SavePoint").transform.position + Vector3.up;
        Debug.Log("快速传送到存档点");
    }

    [ConsoleMethod("ToLevel", "快速传送到关卡")]
    public static void TransferToScene(string scene)
    {
        SceneController.TransitionToScene(scene);
        Debug.Log("快速传送到关卡"+scene);
    }

    [ConsoleMethod("Inv", "玩家无敌开关")]
    public static void PlayerInvincible(bool option)
    {
        if (option)
        {
            PlayerController.Instance.GetComponent<InvulnerableDamable>().enableInvulnerability(true);
        }
        else
        {
            PlayerController.Instance.GetComponent<InvulnerableDamable>().disableInvulnerability();
        }
        //Debug.Log((option?"开启":"关闭")+ "玩家无敌");
    }
}
