using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using UnityEngine.Events;

public class PlayerStatusMenu : MonoBehaviour
{
    GComponent _mainView;

    public HpDamable representedDamable;

    private int moneyNum = 0;       //测试用，应该从数据类拿到金币数量

    private int addAll = 0;     //增加的总量

    public float changeTime = 3.0f;     //数字记录的时间
    private float changeTimeRecord = 3.0f;
    private bool changeTimeStart = false;
    private bool changeTimeOver = false;

    public float zeroTime = 1.5f;       //数字归零的时间
    private float zeroTimeRecord = 1.5f;
    private float perReduce = 0;

    private GProgressBar gProgressBar;
    private GList gListHp;
    private GObject gHpTail;
    private GTextField gTextCoin;
    private GTextField gTextAddCoin;

    void Start()
    {
        _mainView = this.GetComponent<UIPanel>().ui;
        gProgressBar = _mainView.GetChild("pb_magic").asProgress;
        gListHp = _mainView.GetChild("list_blood").asList;
        gListHp.itemRenderer = RenderListItem;
        gListHp.sortingOrder = 7;
        gHpTail = _mainView.GetChild("bg_blood_tail");
        gHpTail.sortingOrder = 6;

        gTextCoin = _mainView.GetChild("money").asCom.GetChild("text").asTextField;
        gTextAddCoin = _mainView.GetChild("money").asCom.GetChild("addText").asTextField;
        EventManager.Instance.Register<int>(EventType.onMoneyChange, ChangeMoneyNum);

        moneyNum = 0;
        gTextCoin.text = moneyNum.ToString();
        zeroTimeRecord = zeroTime;
        changeTimeRecord = changeTime;
    }

    void Update()
    {
        UpdateMoney();
    }

    # region Hp
    public void setRepresentedDamable(HpDamable hpDamable)//设置血量上限
    {
        if (representedDamable == null)
        {
            representedDamable = hpDamable;
            representedDamable.onHpChange.AddListener(ChangeHitPointUI);
        }
        else
        {
            deleteAllHpIcon();
        }
        int maxHp = representedDamable.MaxHp;
        gListHp.numItems = maxHp;
        gHpTail.x = gListHp.x + (52 - 7) * maxHp - 25;
    }

    private void RenderListItem(int index, GObject obj)
    {
        obj.sortingOrder = 5;
        GObject bg = obj.asCom.GetChild("bg");
        bg.sortingOrder = 1;
        GObject blood = obj.asCom.GetChild("blood");
        blood.sortingOrder = 7;
    }

    public void ChangeHitPointUI(HpDamable damageable)//血量变动时调用
    {
        for (int i = 0; i < gListHp.numItems; i++)
        {
            gListHp.GetChildAt(i).grayed = damageable.CurrentHp < i + 1;
        }
    }

    private void deleteAllHpIcon()
    {
        gListHp.numItems = 0;
    }
    # endregion

    # region Mana
    public void ChangeManaValue(PlayerCharacter playerCharacter)
    {
        gProgressBar.max = (double)playerCharacter.getMaxMana();
        gProgressBar.min = 0;
        gProgressBar.TweenValue((double)playerCharacter.Mana, 0.5f);
    }

    public void ChangeManaMax(PlayerCharacter playerCharacter)
    {
        gProgressBar.max = (double)playerCharacter.getMaxMana();
        gProgressBar.min = 0;
        gProgressBar.TweenValue((double)playerCharacter.Mana, 0.5f);
    }
    # endregion

    # region Money
    void UpdateMoney()
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

        gTextAddCoin.alpha = 1;
        gTextAddCoin.text = "+" + addAll.ToString();
    }

    void PrepareCountDown()
    {
        perReduce = addAll / zeroTime;
    }

    void ChangeAllNum()
    {
        float detal = perReduce * (zeroTime - zeroTimeRecord);
        gTextCoin.text = ((int)(moneyNum + detal)).ToString();
        gTextAddCoin.text = "+" + ((int)(addAll - detal)).ToString();
    }

    /// <summary>
    /// 最后一帧强制归零
    /// </summary>
    void ForceZero() 
    {
        moneyNum += addAll;
        gTextCoin.text = moneyNum.ToString();
        gTextAddCoin.text = "+0";
    }

    void ResetStatus() 
    {
        addAll = 0;
        changeTimeRecord = changeTime;
        changeTimeStart = false;
        changeTimeOver = false;
        zeroTimeRecord = zeroTime;       //数字归零的时间
        gTextAddCoin.alpha = 0;
    }
    # endregion
}
