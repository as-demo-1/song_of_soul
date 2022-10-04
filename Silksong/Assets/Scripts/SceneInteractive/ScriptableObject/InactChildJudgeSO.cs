using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "judge", menuName = "NewInact/judge")]
public class InactChildJudgeSO : InactChildBaseSO
{
    [SerializeField] protected bool is_UI;
    [SerializeField] protected InactChildBaseSO _nextChild_Y;
    [SerializeField] protected InactChildBaseSO _nextChild_N;
    [SerializeField] protected string _tip = default;

    public InactChildBaseSO NextChildY => _nextChild_Y;
    public InactChildBaseSO NextChildN => _nextChild_N;
    public bool Is_UI => is_UI;
    public string Tip => _tip;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (_nextChild_Y != null)
        {
            _eia[EInteractStatus.DO_INTERACT_Y] = () =>
            {
                _status = EInteractStatus.INIT;
                NextChildY.Init(_inactItemSO);
                NextChildY.Next();
            };
        }
        else
        {
            _eia[EInteractStatus.DO_INTERACT_Y] = Stop;
        }

        if (_nextChild_N != null)
        {
            _eia[EInteractStatus.DO_INTERACT_N] = () =>
            {
                _status = EInteractStatus.INIT;
                NextChildN.Init(_inactItemSO);
                NextChildN.Next();
            };
        }
        else
        {
            _eia[EInteractStatus.DO_INTERACT_N] = Stop;
        }
    }

    public override void DoInteract()
    {
        //base.DoInteract();
        _status = EInteractStatus.DO_INTERACT;

        if (Is_UI)
        {
            UIComponentManager.Instance.SetText(InteractConstant.UIJudgeContent, Tip);
            UIComponentManager.Instance.ShowUI(InteractConstant.UIJudge);
            UIComponentManager.Instance.UIAddListener(InteractConstant.UIJudgeLB, () => {
                _status = EInteractStatus.DO_INTERACT_Y;
                Hide();
                Next();
            });
            UIComponentManager.Instance.UIAddListener(InteractConstant.UIJudgeRB, () => {
                _status = EInteractStatus.DO_INTERACT_N;
                Hide();
                Next();
            });
        }
        else
        {
            // 条件
            if (Random.Range(1, 9) > 5)
            {
                _status = EInteractStatus.DO_INTERACT_Y;
            }
            else
            {
                _status = EInteractStatus.DO_INTERACT_N;
            }
            Hide();
            Next();
        }
    }
    protected void Hide()
    {
        UIComponentManager.Instance.HideUI(InteractConstant.UIJudge);
    }
}
