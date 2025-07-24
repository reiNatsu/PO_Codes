using Consts;
using LIFULSE.Manager;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStoryInfo : UIBase
{
    [SerializeField] private RecycleScroll _scroll;
    [SerializeField] private StoryListCell _infoObject;

    private List<StoryEpisodeCell> _cells;
    private List<StoryEpisodeCellData> _cellDatas;

    private Story_Group_TableData _groupData;
    private CONTENTS_TYPE_ID _type;
    private List<Stage_TableData> _list = new List<Stage_TableData>();
    private Stage_TableData _lastreadStage = null;
    public override void Init()
    {
        base.Init();
        _scroll.Init();
        _cellDatas = new List<StoryEpisodeCellData>();
        _cells = _scroll.GetCellToList<StoryEpisodeCell>();
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        if (optionDict != null)
        {
            if (optionDict.TryGetValue(UIOption.Data, out var groupData))
            {
                _groupData = (Story_Group_TableData)groupData;
            }
            if (optionDict.TryGetValue(UIOption.EnumType, out var type))
                _type = (CONTENTS_TYPE_ID)type;
            if (optionDict.TryGetValue(UIOption.Data2, out var stagedata))
                _lastreadStage = (Stage_TableData)stagedata;
        }

        _list =  TableManager.Instance.Stage_Table.GetStoryDatas(_type, _groupData.Theme_Id);
        UpdateUI();
        SetFocusing(_lastreadStage.Stage_Id);
    }

    private void UpdateUI()
    {
        _infoObject.UpdateInfoUII(_groupData);
        SetEpisodeList();
    }

    public void SetEpisodeList()
    {
        _cellDatas.Clear();
        for (int n = 0; n< _list.Count; n++)
        {
            _cellDatas.Add(new StoryEpisodeCellData(_list[n]));
        }
        for (int i = 0; i < _cells.Count; i++)
        {
            _cells[i].SetCellDatas(_cellDatas);
        }
        _scroll.ActivateCells(_list.Count);

    }

    public void SetFocusing(int stageno)
    {
        if (stageno > 4 &&stageno < _list.Count - 4)
            _scroll.FocusLine(stageno - 2, padding:0.5f,isCustom:true, isMoveSmooth: true);
        
        if(stageno >= _list.Count - 4)
            _scroll.FocusLine(_list.Count -4);
    }
}
