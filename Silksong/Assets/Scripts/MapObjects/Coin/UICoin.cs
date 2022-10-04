using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICoin : MonoBehaviour
{
    private int moneyNum = 0;       //测试用，应该从数据类拿到金币数量

    private int addAll = 0;     //增加的总量

    public float changeTime = 3.0f;     //数字记录的时间
    private float changeTimeRecord = 3.0f;
    private bool changeTimeStart = false;
    private bool changeTimeOver = false;

    public float zeroTime = 1.5f;       //数字归零的时间
    private float zeroTimeRecord = 1.5f;

    private float perReduce = 0;

    private Text moneyText;
    private Text moneyChangeText;
    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.Register<int>(EventType.onMoneyChange, ChangeMoneyNum);

        moneyText =  transform.Find("moneyCount").GetComponent<Text>();
        moneyChangeText = transform.Find("moneyChange").GetComponent<Text>();

        moneyChangeText.gameObject.SetActive(false);

        moneyNum = 0;
        moneyText.text = moneyNum.ToString();

        zeroTimeRecord = zeroTime;
        changeTimeRecord = changeTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (changeTimeStart)    //增加金币的行为开始
        {
            changeTimeRecord -= Time.deltaTime;
            if (changeTimeRecord <= 0)  //记录时间结束
            {
                changeTimeOver = true;
                changeTimeStart = false;
                PrepareCountDown();
            }
        }

        if (changeTimeOver)     //增加金币的行为结束了，即已经有一段时间没有获取到金币了，开始倒数增加金币
        {
            zeroTimeRecord -= Time.deltaTime;
            if (zeroTimeRecord >= 0)
            {
                ChangeAllNum();
            }else 
            {
                changeTimeOver = false;
                ForceZero();
                ResetStatus();
            }
        }
    }

    void ChangeMoneyNum(int changeNum) 
    {
        changeTimeRecord = changeTime;  //又获得了新的金币，重置时间

        changeTimeStart = true;
        addAll += changeNum;

        moneyChangeText.gameObject.SetActive(true);
        moneyChangeText.text = "+" + addAll.ToString();
    }

    void PrepareCountDown()
    {
        perReduce = addAll / zeroTime;
    }

    void ChangeAllNum()
    {
        float detal = perReduce * (zeroTime - zeroTimeRecord);
        moneyChangeText.text = "+" + ((int)(addAll - detal)).ToString();
        moneyText.text = ((int)(moneyNum + detal)).ToString();
    }


    /// <summary>
    /// 最后一帧强制归零
    /// </summary>
    void ForceZero() 
    {
        moneyNum += addAll;
        moneyText.text = moneyNum.ToString();
        moneyChangeText.text = "+0";
    }

    void ResetStatus() 
    {
        addAll = 0;
        changeTimeRecord = changeTime;
        changeTimeStart = false;
        changeTimeOver = false;
        zeroTimeRecord = zeroTime;       //数字归零的时间
        moneyChangeText.gameObject.SetActive(false);
    }
}
