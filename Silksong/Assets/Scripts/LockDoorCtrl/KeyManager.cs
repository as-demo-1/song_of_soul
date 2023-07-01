using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
/// <summary>
/// 钥匙被单独列出来了，后续物品的开发可以看看这里有没有能用的或者直接对接一下成为player的自带对象参数也不是不行
/// </summary>
public class KeyManager : MonoSingleton<KeyManager>
{
    public int keyNum;
    public TextMeshProUGUI numText;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject.transform);
    }
    // Start is called before the first frame update
    void Start()
    {
        InitKeyNum();
        EventManager.Instance.Register<int>(EventType.onKeyChange, ChangeKeyNum);
        

    }



    void InitKeyNum() {
        keyNum = 0;
        numText.text = keyNum.ToString();
    }
    public int GetKeyNum() {
        return keyNum;
    }



    void ChangeKeyNum(int changeNum) {
        keyNum += changeNum;
        numText.text = keyNum.ToString();
    }
}
