using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "drop", menuName = "NewInact/drop")]
public class InactDrop : InactChildFuncSO
{
    [SerializeField] private string _drop_1 = "0";
    [SerializeField] private int _drop_1_num;
    [SerializeField] private string _drop_2 = "0";
    [SerializeField] private int _drop_2_num;
    [SerializeField] private string _drop_3 = "0";
    [SerializeField] private int _drop_3_num;
    public string Drop1 => _drop_1;
    public int Drop1Num => _drop_1_num;
    public string Drop2 => _drop_2;
    public int Drop2Num => _drop_2_num;
    public string Drop3 => _drop_3;
    public int Drop3Num => _drop_3_num;
    public override void DoInteract()
    {
        _status = EInteractStatus.DO_INTERACT;
        Debug.Log(_status);
        var items = DropManager.Instance.GetDrop4Inact(this);

        foreach (var item in items)
        {
            if (item.info.ID == "10000001")
            {
                int reduce = item.dropNum;
                int large = reduce / 5;
                reduce %= 5;
                int mid = reduce / 3;
                reduce %= 3;
                int small = reduce;

                CoinGenerator.Instance.GenerateCoins(_inactItemSO.Go, large, mid, small);
            }
            else
            {
                ItemGenerator.Instance.GenerateItems(_inactItemSO.Go, item);
            }
        }

        Next();
    }
}
