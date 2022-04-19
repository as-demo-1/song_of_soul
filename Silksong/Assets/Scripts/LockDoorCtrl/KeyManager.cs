using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
