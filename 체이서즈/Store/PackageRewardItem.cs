using Consts;
using LIFULSE.Manager;
using System.Collections.Generic;
using UnityEngine;


public class PackageRewardItemData
{
    [SerializeField] public Reward_TableData rewardData;
    [SerializeField] public string Tid;

    public PackageRewardItemData(Reward_TableData data)
    {
        rewardData = data;
        Tid = data.Tid;
    }
}
public class PackageRewardItem : ScrollCell
{
    [SerializeField] private ItemCell _itemCell;
    private List<PackageRewardItemData> _cellDatas;

    

    protected override void Init()
    {
        base.Init();
    }

    public override void UpdateCellData(int dataIndex)
    {
        DataIndex = dataIndex;

        var data = TableManager.Instance.Reward_Table[_cellDatas[DataIndex].Tid];
      
        UpdateUI(data);
    }

    private void UpdateUI(Reward_TableData data)
    {
        _itemCell.UpdateData(data.Item_Tid, data.ITEM_TYPE, ItemCustomValueType.RewardAmount, data.Item_Min);
        //_itemCell.SetFreeCashUI();
    }

    public void SetCellDatas(List<PackageRewardItemData> datas)
    {
        _cellDatas = datas;
    }
}
