using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICoin : MonoBehaviour
{
    private int moneyNum = 0;       //�����ã�Ӧ�ô��������õ��������

    private int addAll = 0;     //���ӵ�����

    public float changeTime = 3.0f;     //���ּ�¼��ʱ��
    private float changeTimeRecord = 3.0f;
    private bool changeTimeStart = false;
    private bool changeTimeOver = false;

    public float zeroTime = 1.5f;       //���ֹ����ʱ��
    private float zeroTimeRecord = 1.5f;

    private float perReduce = 0;

    private Text moneyText;
    private Text moneyChangeText;
    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.Register<int>(EventType.onMoneyChange, ChangeMoneyNum);
        EventManager.Instance.Register<ItemInfo, int>(EventType.onItemChange, ChangeItemNum);

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
        if (changeTimeStart)    //���ӽ�ҵ���Ϊ��ʼ
        {
            changeTimeRecord -= Time.deltaTime;
            if (changeTimeRecord <= 0)  //��¼ʱ�����
            {
                changeTimeOver = true;
                changeTimeStart = false;
                PrepareCountDown();
            }
        }

        if (changeTimeOver)     //���ӽ�ҵ���Ϊ�����ˣ����Ѿ���һ��ʱ��û�л�ȡ������ˣ���ʼ�������ӽ��
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
        changeTimeRecord = changeTime;  //�ֻ�����µĽ�ң�����ʱ��

        changeTimeStart = true;
        addAll += changeNum;

        moneyChangeText.gameObject.SetActive(true);
        moneyChangeText.text = "+" + addAll.ToString();
    }

    void ChangeItemNum(ItemInfo item, int changeNum)
    {
        InventoryManager.Instance.AddItemStack(
            new ItemStack(item.ItemSO, changeNum));
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
    /// ���һ֡ǿ�ƹ���
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
        zeroTimeRecord = zeroTime;       //���ֹ����ʱ��
        moneyChangeText.gameObject.SetActive(false);
    }
}
