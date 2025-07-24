using Consts;
using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class  StoryTabs
{
    [SerializeField] public CONTENTS_TYPE_ID _type;
    [SerializeField] public UICategory _uiCategory;
}

public class UIStoryList : UIBase
{
    [SerializeField] private GameObject _mainStoryObj;
    [SerializeField] private GameObject _eventStoryObj;
    [SerializeField] private RecycleScroll _mainscroll;
    [SerializeField] private RecycleScroll _eventscroll;
    [SerializeField] private List<StoryTabs> _tabs = new List<StoryTabs>();
    //[SerializeField] private List<CONTENTS_TYPE_ID> _tabs = new List<CONTENTS_TYPE_ID>();
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private VerticalLayoutGroup _layout;
    [SerializeField] private GameObject _tabObj;

    private List<UICategory> _tabCells;

    private List<StoryListCell> _mainCells;
    private List<StoryListCellData> _mainCellDatas;

    private List<StoryListCell> _eventCells;
    private List<StoryListCellData> _eventCellDatas;

    //private string _mainGroupId;
    //private CONTENTS_TYPE_ID _mainTypeId;

    private int _tabIndex = -1;

    public override void Close(bool needCached = true)
    {
        base.Close(needCached);
    }
    public override void Init()
    {
        base.Init();
        _mainscroll.Init();
        _eventscroll.Init();

        _mainCells = _mainscroll.GetCellToList<StoryListCell>();
        _mainCellDatas = new List<StoryListCellData>();
        
        _eventCells = _eventscroll.GetCellToList<StoryListCell>();
        _eventCellDatas = new List<StoryListCellData>();
    }

    public override void Refresh()
    {
        base.Refresh();
        OnClickTab(_tabIndex);
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        SetTabs();
         _tabIndex = -1;
        OnClickTab(0);
    }
    private void SetTabs()
    {
        for (int n = 0; n< _tabs.Count; n++)
        {
            var index = n;
            string sprite = null;
            if (_tabs[index]._type == CONTENTS_TYPE_ID.story_main)
                sprite = "UI_Story_Category_Main_Icon";
            else
                sprite = "UI_Story_Category_Event_Icon";
            var data = TableManager.Instance.Story_Group_Table.GetDatas(_tabs[index]._type.ToString());
            bool isOn = data != null && data.Count > 0;
            _tabs[index]._uiCategory.gameObject.SetActive(isOn);
            var list = TableManager.Instance.Stage_Table.GetStoryDatas(_tabs[index]._type);
            _tabs[index]._uiCategory.SetCategory(GameInfoManager.Instance.GetStroyGroupName(_tabs[index]._type), sprite);
            bool isShow = list != null && list.Count > 0;
            
        }
    }
   
    public void SetMainStoryLists()
    {
        _mainCellDatas.Clear();
        var list = TableManager.Instance.Stage_Table.GetStoryDatas(_tabs[_tabIndex]._type);

        for (int n = 0; n< list.Count; n++)
        {
            var groupData = TableManager.Instance.Story_Group_Table.GetData(list[n]);
            if (groupData.Story_Disable == 0)
                _mainCellDatas.Add(new StoryListCellData(_tabs[_tabIndex]._type, groupData));
        }

        for (int i = 0; i < _mainCells.Count; i++)
        {
            _mainCells[i].SetCellDatas(_mainCellDatas);
        }
        _mainscroll.ActivateCells(_mainCellDatas.Count);
    }
    public void SetEventStoryLists()
    {
        _eventCellDatas.Clear();
        var list = TableManager.Instance.Stage_Table.GetStoryDatas(_tabs[_tabIndex]._type);

        if (list.Count == 0)
        { 
            
        }

        for (int n = 0; n< list.Count; n++)
        {
            var groupData = TableManager.Instance.Story_Group_Table.GetData(list[n]);
            _eventCellDatas.Add(new StoryListCellData(_tabs[_tabIndex]._type, groupData));
        }

        for (int i = 0; i < _eventCells.Count; i++)
        {
            _eventCells[i].SetCellDatas(_eventCellDatas);
        }
        _eventscroll.ActivateCells(_eventCellDatas.Count);
    }
    public void OnClickTab(int index)
    {
        if (_tabIndex == index)
            return;

        if (_tabIndex != -1)
            _tabs[_tabIndex]._uiCategory.Active(false);

        _tabs[index]._uiCategory.Active(true);

        _tabIndex = index;
        // 여기는 임시. Story_Group_Table하고 Story_MainGroup_Table을 합치면 수정 필요

        SetShowTypeObj(_tabs[_tabIndex]._type == CONTENTS_TYPE_ID.story_main);
    }

    private void SetShowTypeObj(bool isMain)
    {
        _mainStoryObj.SetActive(isMain);
        _eventStoryObj.SetActive(!isMain);
        if(isMain)
            SetMainStoryLists();
        else
            SetEventStoryLists();
    }
}
