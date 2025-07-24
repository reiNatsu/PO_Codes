using Consts;
using LIFULSE.Manager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIEventQuest : UIBase
{

    [SerializeField] private RecycleScroll _scroll;
    //[SerializeField] private ExTMPUI _questDesc;
    [SerializeField] private GameObject _allReciveDimed;
    [SerializeField] private List<UICharacterMenuTab> _uitab;

    [SerializeField] private Transform _tabsTransform;
    [SerializeField] private GameObject _tabObj;

    [SerializeField] private List<UILayoutGroup> _uiLayoutGroups = new List<UILayoutGroup>();

    private List<UIEventMenuTab> _tabCells;

    private List<EventQuestCell> _cells;
    private List<EventQuestCellData> _cellDatas = null;

    private List<string> _recievequestList = new List<string>();

    private Event_Story_TableData _eventData;

    private int _tabIndex = 0;
    private string _questgroupid;
    private List<QUEST_TYPE> _tabs = new List<QUEST_TYPE>();
    private QUEST_TYPE _selectType ;     // 현재 선택한 타입

    public QUEST_TYPE SelectType { get { return _selectType; } }

    public override void Close(bool needCached = true)
    {
        base.Close(needCached);
        _tabIndex = -1;
        for (int n = 0; n < _tabCells.Count; n++)
        {
            _tabCells[n].gameObject.SetActive(false);
        }
    }
    public override void GoBack()
    {
        base.GoBack();
    }

    public override void Refresh()
    {
        base.Refresh();
        Debug.Log("<color=#f57eb6>UIEventQuest Refresh %%%%</color>");
        UpdateUI();
    }

    public override void Init()
    {
        base.Init();
        _scroll.Init();

        _cells = _scroll.GetCellToList<EventQuestCell>();
        _cellDatas = new List<EventQuestCellData>();

        _tabCells = new List<UIEventMenuTab>();
        _tabCells.Add(_tabObj.GetComponent<UIEventMenuTab>());

       
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {

        if (optionDict != null)
        {
            if (optionDict.TryGetValue(UIOption.Tid, out var questGroupId))
            {
                _questgroupid = questGroupId.ToString();
            }
            if (optionDict.TryGetValue(UIOption.Data, out var eventstorydata))
            {
                _eventData = (Event_Story_TableData)eventstorydata;
            }
        }


        _tabs = TableManager.Instance.Quest_Table.GetEventQuestTabs(_questgroupid);
        // _selectType = QUEST_TYPE.daily;
        _selectType = _tabs.FirstOrDefault();

        for (int n = 0; n < _tabCells.Count; n++)
        {
            _tabCells[n].gameObject.SetActive(false);
        }

        UpdateUI();

        for (int m = 0; m <_uiLayoutGroups.Count; m++)
        {
            _uiLayoutGroups[m].UpdateLayoutGroup();
        }

        UpdateList(_selectType);

        _tabIndex = -1;
        OnClickMenuTab(0);
    }

    private void UpdateUI()
    {
        //_questDesc.ToTableText(_eventData.Event_Quest_String);

        // 퀘스트 탭 설정
        //var tabs = TableManager.Instance.Quest_Table.GetEventQuestTabs(_questgroupid);
        if(_tabs.Count > _tabCells.Count)
            CreateCell(_tabs.Count - _tabCells.Count);

        UpdateTabs(_tabCells);
    }

    private void CreateCell(int count)
    {
        for (int n = 0; n< count; n++)
        {
            var newObj = Instantiate(_tabObj, _tabsTransform);
            var tabcell = newObj.GetComponent<UIEventMenuTab>();

            _tabCells.Add(tabcell);
        }
    }

    private void UpdateTabs(List<UIEventMenuTab> tabCellDatas)
    {
        //var tabs = TableManager.Instance.Quest_Table.GetEventQuestTabs(_questgroupid);
        for (int i = 0; i < _tabCells.Count; i++)
        {
            var index = i;
            if (i < tabCellDatas.Count)
            {
                var tabIndex = LocalizeManager.Instance.GetString("str_tap_"+_tabs[i].ToString()+"_01");
                if (!_tabCells[index].gameObject.activeInHierarchy)
                    _tabCells[index].gameObject.SetActive(true);

                _tabCells[index].InitData(index, tabIndex);
                //_itemCells[i].SetFreeCashUI();
                _tabCells[index].Button.onClick.AddListener(() => OnClickMenuTab(index));
                if (index == _tabs.Count-1)
                {
                    _tabCells[i].SetDecoUI(false);
                }
            }
        }
    }

    public void UpdateList(QUEST_TYPE type)
    {

        _cellDatas.Clear();
        var questList = GameInfoManager.Instance.QuestBase.GetQuestDatas(type, _questgroupid);
        
        for (int n = 0; n< questList.Count;  n++)
        {
            _cellDatas.Add(new EventQuestCellData(questList[n]));
        }
        for (int i = 0; i< _cells.Count; i++)
        {
            _cells[i].SetCellDatas(_cellDatas, _selectType, _questgroupid);
        }

        _scroll.ActivateCells(_cellDatas.Count);


        SetQuestAllReceived();
    }


    public void OnClickMenuTab(int index)
    {
        if (_tabIndex == index)
        {
            return;
        }

        //QUEST_TYPE selecttype = TableManager.Instance.Quest_Table.GetEventQuestTabs(_questgroupid)[index];
        QUEST_TYPE selecttype = _tabs[index];
        //if (index == 0)
        //{
        //    selecttype = QUEST_TYPE.daily;
        //}
        //else
        //{
        //    selecttype = QUEST_TYPE.week;
        //}
        Debug.Log("클릭한 퀘스트 타입 : " +selecttype.ToString());
      

        _selectType = selecttype;
        // _uitab[index].SetToggle(true);
       
        if (_tabIndex != -1)
        {
            _tabCells[_tabIndex].On(false);
            //_uitab[_tabIndex].SetToggle(false);
        }
        _tabCells[index].On(true);
        _tabIndex = index;
        UpdateList(_selectType);
    }

    public void OnClickReceiveAll()
    {
        var clanId = GameInfoManager.Instance.ClanInfo.GetUserClanId();

        RestApiManager.Instance.RequestQuestReward(_recievequestList, _selectType, clanId, eventid: _eventData.Event_Quest, callBack :(res) =>
        {
            Debug.Log("모두 받기 버튼 Click → 받은 보상 : ");
            Debug.Log(res);
            UpdateQuestData(res["result"]["reward"]);
            UpdateList(_selectType);
        });
    }

    public void SetQuestAllReceived()
    {
        _recievequestList = new List<string>();
        //find_happiness_quest
        _recievequestList = GameInfoManager.Instance.GetQuestRewardTids(_selectType, _questgroupid);

        if (_recievequestList.Count == 0)
        {
            _allReciveDimed.SetActive(true);
        }
        else
        {
            _allReciveDimed.SetActive(false);
        }
    }

    public void UpdateQuestData(JToken reward)
    {
        Dictionary<string, int> rewards = new Dictionary<string, int>();
        JArray rArray = reward["Quest"]as JArray;
        for (int n = 0; n < rArray.Count; n++)
        {
            var tid = rArray[n][0].ToString();
            var amount = (int)rArray[n][1];
            if (!rewards.ContainsKey(tid))
            {
                rewards.Add(tid, amount);
            }
            else
            {
                rewards[tid] += amount;
            }
        }

        foreach (var item in rewards)
        {
            Debug.Log("퀘스트 총 보상 → "+item.Key+"("+item.Value+")");
        }
    }

    
}
