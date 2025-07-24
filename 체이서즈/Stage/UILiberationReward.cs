using Consts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILiberationReward : UIBase
{

    [SerializeField] private ExTMPUI _areaTitleTMP;
    [SerializeField] private List<ItemCell> _rewardcells = new List<ItemCell>();
    

    private List<ItemCellData> _cellDatas = new List<ItemCellData>();
    private int _areaIndex = 0;

    public override void Init()
    {
        base.Init();
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        if (optionDict != null )
        {
            if (optionDict.TryGetValue(UIOption.List, out var rewardList))
                _cellDatas = (List<ItemCellData>)rewardList;

            if (optionDict.TryGetValue(UIOption.Index, out var areaIndex))
                _areaIndex = (int)areaIndex;
        }

        _areaTitleTMP.ToTableText("str_ui_stage_area_02", _areaIndex);
        UpdateRewardCell();
    }

    private void UpdateRewardCell()
    {
        for (int n = 0; n< _rewardcells.Count; n++)
        {
            if (n < _cellDatas.Count)
            {
                var data = _cellDatas[n];
                _rewardcells[n].gameObject.SetActive(true);
                _rewardcells[n].UpdateData(data.Tid, data.ItemType, ItemCustomValueType.RewardAmount, data.RewardAmount);
                _rewardcells[n].IsUseItemPopup = false;
                //_rewardcells[n].EnableUnSelected(true);
            }
            else
                _rewardcells[n].gameObject.SetActive(false);
        }
    }
}
