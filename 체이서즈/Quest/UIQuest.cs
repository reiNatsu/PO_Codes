using Consts;
using LIFULSE.Manager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Unity.Services.Analytics;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif
using UnityEngine;

public class UIQuest : UIBase
{
    [SerializeField] private RecycleScroll _recycleScroll;

    [Header("Tap")]
    // [SerializeField] private UIQuestTab originTap;
    [SerializeField] private RectTransform _questContent;
    [SerializeField] private Dictionary<QUEST_TYPE, UITab> _questTabDic = new Dictionary<QUEST_TYPE, UITab>();
    [SerializeField] private UITabMenu _uiTabMenu;
    [SerializeField] private UITab _uiTab;
    [SerializeField] private GameObject _allBtnDimd;

    // 보상 수령 가능 한 퀘스트 목록 저장. QUEST_TYPE = Key, QuestData = Value

    private List<UITab> _uiTabs = new List<UITab>();
    private List<QuestCell> _questCells;
    private List<QuestCellData> _cellDatas = null;

    private QUEST_TYPE _currentType = QUEST_TYPE.all;
    private Quest_Table _table;

    private int _tabIndex;
    private QUEST_TYPE _selectedTab;        // 현재 선택 한 탭
    private List<string> _rewardTidList = new List<string>();

    public QUEST_TYPE SelectedTab { get {return _selectedTab; } }
    private bool _tapInit = false;
    public string QuestTabAllRDTid { get; set; }

    public override void Close(bool needCached = true)
    {
        base.Close(needCached);
    }

    public override void GoBack()
    {
        base.GoBack();
    }

    public override void Init()
    {
        base.Init();

        if (_table == null)
            _table = TableManager.Instance.Quest_Table;
        _recycleScroll.Init();

        _questCells = _recycleScroll.GetCellToList<QuestCell>();
        _cellDatas = new List<QuestCellData>();
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        _selectedTab = QUEST_TYPE.all;
        _tabIndex = (int)QUEST_TYPE.all;
        // _uiTabMenu.RefreshTab((int)_selectedTab);
        // 1. 탭 메뉴를 생성 하고 0번째 탭이 on 되도록 했음. 
        _uiTabMenu.SetUITabMenu(OnClickMenuTab);
        _uiTabMenu.RefreshTab(_tabIndex);
       
        UpdateTab(_selectedTab);
    }

    public void ChangeTab(QUEST_TYPE selecttab)
    {
        _selectedTab = selecttab;
        _tabIndex = (int)_selectedTab;
        UpdateTab(_selectedTab);
    }

    public void UpdateTab(QUEST_TYPE type)
    {
        //GameInfoManager.Instance.QuestBase.SetupEnableRewardQuests();
        for (int n = 0; n < (int)QUEST_TYPE.special; n++)
        {
            if (n == 0)
            {
                _uiTabMenu.GetCell(n).SetTabRedDotInfo("reddot_quests_all", 0);
            }
            else
            {
                QUEST_TYPE name = (QUEST_TYPE)n;
                _uiTabMenu.GetCell(n).SetTabRedDotInfo("reddot_quests_"+name.ToString(), n);
            }
        }

        _currentType = type;
        _cellDatas.Clear();

        var questList = GameInfoManager.Instance.QuestBase.GetQuestDatas(type);
        Debug.Log("<color=#9efc9e>["+type+"]  questList.Count = "+questList.Count+"</color>");
        for (int n = 0; n < questList.Count; n++)
        {
            if (CheckQuestCellEnable(questList[n].Tid))
            {
                _cellDatas.Add(new QuestCellData(questList[n]));
            }
            else
            {
                Debug.Log("<color=#9efc9e>["+type+"]"+questList[n].Tid+" Quest show State is "+CheckQuestCellEnable(questList[n].Tid)+"</color>");
            }
        }

        for (int i = 0; i < _questCells.Count; i++)
        {
            _questCells[i].SetCellDatas(_cellDatas, _currentType);
        }

        _recycleScroll.ActivateCells(_cellDatas.Count);

        SetAllRewardUI();
    }

    private bool CheckQuestCellEnable(string questtid)
    {
        bool isShow = false;
        if (TableManager.Instance.Quest_Table[questtid] != null)
        {
            var info = TableManager.Instance.Quest_Table[questtid];
            if (info.QUEST_TYPE == QUEST_TYPE.feat)
            {
                if (info.Quest_Enable == 0)
                {
                    isShow = true;
                }
                else
                {
                    isShow = false;
                }
            }
            else
            {
                isShow = true;
            }
        }
        return isShow;
    }

    private void SetAllRewardUI()
    {
        _rewardTidList = new List<string>();
        Debug.Log("OnClickRewardAll(QUEST_TYPE = " +_selectedTab+")");

        if (_selectedTab != QUEST_TYPE.all)
        {
            _rewardTidList = GameInfoManager.Instance.GetQuestRewardTids(_selectedTab);
        }
        else
        {
            _rewardTidList = GameInfoManager.Instance.GetQuestRewardTidsAll();
        }

        if (_rewardTidList.Count == 0)
        {
            _allBtnDimd.SetActive(true);
        }
        else
        {
            _allBtnDimd.SetActive(false);
        }
    }

    public void OnClickMenuTab(int type)
    {
        Debug.Log("클릭한 퀘스트 타입 : " +(QUEST_TYPE)type);
        _tabIndex = type;
        _selectedTab = (QUEST_TYPE)type;
        UpdateTab((QUEST_TYPE)type);
    }

    public void OnClickRewardAll()
    {
        var clanId = GameInfoManager.Instance.ClanInfo.GetUserClanId();

        RestApiManager.Instance.RequestQuestReward(_rewardTidList, _selectedTab, clanId, eventid: null , callBack: (res) =>
        {
            Debug.Log("모두 받기 버튼 Click → 받은 보상 : ");
            Debug.Log(res);
            SetQuestEvent(res["result"]["reward"]);
            UpdateTab(_selectedTab);
            GameInfoManager.Instance.EventInfo.UpdatePassReddot();
        });

        //List<string> rewardtids = new List<string>();

        //Debug.Log("OnClickRewardAll(QUEST_TYPE = " +_selectedTab+")");

        //if (_selectedTab != QUEST_TYPE.all)
        //{
        //    rewardtids = GameInfoManager.Instance.GetQuestRewardTids(_selectedTab);
        //}
        //else
        //{
        //    rewardtids = GameInfoManager.Instance.GetQuestRewardTidsAll();
        //}

        //if (rewardtids.Count == 0)
        //{
        //    _allBtnDimd.SetActive(true);
        //    //string str = LocalizeManager.Instance.GetString("보상 수령 가능한 임무가 없습니다.");
        //    //UIManager.Instance.ShowToastMessage($"{str}");
        //}
        //else
        //{
        //    _allBtnDimd.SetActive(false);

        //}

    }

    public void OnClickRewardAllDimd()
    {
        string str = LocalizeManager.Instance.GetString("보상 수령 가능한 임무가 없습니다.");
        UIManager.Instance.ShowToastMessage($"{str}");
    }

    public void SetQuestEvent(JToken reward)
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


    public override void Refresh()
    {
        base.Refresh();
        _uiTabMenu.RefreshTab(_tabIndex);
        UpdateTab(_selectedTab);
    }

   
}

