using Consts;
using LIFULSE.Manager;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIStageInfo : UIBase
{
    [Header("Stage Info UI")]
    [SerializeField] private ExTMPUI _title;
    [SerializeField] private ExTMPUI _needLevelTxt;
    [SerializeField] private List<UIConditionItem> _conditions = new List<UIConditionItem>();

    [Header("Button Info")]
    //[SerializeField] private UICombatButton _uiCombatButton;
    [SerializeField] private ExImage _cookButtonEmptyIcon;
    [SerializeField] private ExImage _cookButtonIcon;

    [Header("Team Setting UI")]
    [SerializeField] private UIContentTeam _uiContentTeam;

    [Header("Monster Info UI")]
    [SerializeField] private GameObject _monsterItem;
    [SerializeField] private Transform _monsterContent;

    [Header("Reward RecycleScroll UI")]
    [SerializeField] private RecycleScroll _rewardScroll;

    [Header("Reward Sweep UI")]
    [SerializeField] private GameObject _sweepRewardTitleObj;
    [SerializeField] private RecycleScroll _sweepRewardScroll;

    [Header("조력자 사용시 알람 팝업")]
    [SerializeField] private AlertInfo _uiUseHelperAlert;
    [SerializeField] private ExTMPUI _useHelperGoodsTMP;


    [SerializeField] private RectTransform _rewardRect;
    [SerializeField] private RectTransform _sweepRewardRect;

    //초회 보상
    [SerializeField] private ItemCell _firstItemCell;
    [SerializeField] private GameObject _firstTitleObj;
    [SerializeField] private GameObject _rewardInfoObj;

    //소탕
    [SerializeField] private GameObject _sweepDimmed;
    [SerializeField] private ExButton _sweepButton;
    [SerializeField] private ExTMPUI _stageTipTMP;

    // 소탕 보상
    private List<ItemCellData> _sweepItemCellDatas = new List<ItemCellData>();
    private List<ItemCell> _sweepCells;

    // 보상 아이템 
    private List<ItemCellData> _itemCellDatas = new List<ItemCellData>();
    private List<ItemCell> _cells;

    private Stage_TableData _data;
    private Stage_Mission_TableData _missionData;
    private List<MissionData> _missioList = new List<MissionData>();
    private List<UITeamSlot> _monsterSlotList = new List<UITeamSlot>();

    private Action _onEnterStage;

    private Dictionary<string, int> _settingRewards = new Dictionary<string, int>(); 

    private List<int> _stars;
    private string _stageId;

    private void OnDisable()
    {
        _monsterSlotList.Clear();
    }

    public override void Init()
    {
        base.Init();

        _itemCellDatas = new List<ItemCellData>();
        _rewardScroll.Init();
        _sweepItemCellDatas = new List<ItemCellData>();
        _sweepRewardScroll.Init();

        _cells = _rewardScroll.GetCellToList<ItemCell>();
        _sweepCells = _sweepRewardScroll.GetCellToList<ItemCell>();
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        
        if (optionDict.TryGetValue(UIOption.Tid, out var tid))
        {
            _stageId = (string)tid;
        }

        if (optionDict.TryGetValue(UIOption.Callback, out var callback))
        {
            _onEnterStage = (Action)callback;
        }
       
        // 깨비공 보상 정보 
        if (optionDict.TryGetValue(UIOption.List, out var list))
        {
            _settingRewards = (Dictionary<string, int>)list;
        }

        Debug.Log("<color=#9efc9e>UIStageInfo StageTid :: "+_stageId+"</color>");
        _uiUseHelperAlert.gameObject.SetActive(false);
        UpdateData();
        UpdateTicket();
    }


    public override void Refresh()
    {
        base.Refresh();
        var haveMission = TableManager.Instance.Stage_Mission_Table.CheckHaveMission(_data.Tid);
        UpdateStarCondition(haveMission);
        UpdateData();
        _uiContentTeam.Show(_data);
    }

    private void UpdateData()
    {
        // 정보 
        _data = TableManager.Instance.Stage_Table[_stageId];
        bool enableSweep;
        _stars = new List<int>();
        List<int> starlist = new List<int>();

        switch (_data.CONTENTS_TYPE_ID)
        {
            case CONTENTS_TYPE_ID.stage_main:
            case CONTENTS_TYPE_ID.stage_sub:
            case CONTENTS_TYPE_ID.stage_character:
            case CONTENTS_TYPE_ID.event_main:
            {
                    if (_data.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.event_main)
                        enableSweep = GameInfoManager.Instance.EventStoryInfo.IsMaxStar(_data.Tid);
                    else
                        enableSweep = GameInfoManager.Instance.StageInfo.IsMaxStar(_data.Tid);

                    _stageTipTMP.gameObject.SetActive(false);

                    _firstItemCell.gameObject.SetActive(false);
                    _firstTitleObj.SetActive(false);
                    _rewardInfoObj.SetActive(true);
                    
                    UpdateStageLists();
                    for (int n = 0; n < _monsterContent.childCount; n++)
                    {
                        _monsterContent.GetChild(n).gameObject.SetActive(false);
                    }
                    //UpdateStarCondition(true);
                    SetSweepRewardsUI(false);
                }
                break;
            case CONTENTS_TYPE_ID.manjang:
                {
                    enableSweep = GameInfoManager.Instance.DungeonInfo.IsClear(_data.CONTENTS_TYPE_ID, _data.Tid);

                    _firstItemCell.gameObject.SetActive(false);
                    _firstTitleObj.SetActive(false);
                    UpdateStageLists();
                    //UpdateStarCondition(true);
                    SetSweepRewardsUI(false);
                    _rewardInfoObj.SetActive(true);
                }
                break;
            default:
                {
                    enableSweep = GameInfoManager.Instance.DungeonInfo.IsClear(_data.CONTENTS_TYPE_ID, _data.Tid);
                    _rewardInfoObj.SetActive(true);

                    SetActiveFirstObj(false);
                    SetSweepRewardsUI(false);
                    //UpdateStarCondition(false);
                }
                break;
        }

        var haveMission = TableManager.Instance.Stage_Mission_Table.CheckHaveMission(_data.Stage_Mission);
        UpdateStarCondition(haveMission);


        UpdateUI(enableSweep);
       
    }

    private void UpdateStageLists()
    {
        _missioList = TableManager.Instance.Stage_Mission_Table.GetStageMissionList(_data.Stage_Mission);

        if (_missioList.IsNullOrEmpty())
            return;

        for (int s = 0; s< _missioList.Count; s++)
        {
            if (_missioList[s].stage_clear_mission_type != STAGE_CLEAR_MISSION_TYPE.none)
            {
                if (!_conditions[s].gameObject.activeInHierarchy)
                {
                    _conditions[s].gameObject.SetActive(true);
                }
            }
            else
            {
                _conditions[s].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateTicket()
    {
        //var stageData = TableManager.Instance.Stage_Table.GetData(_data.CONTENTS_TYPE_ID, 0);
        var stageData = TableManager.Instance.Stage_Table[_data.Tid];

        //재화 필요 없는 경우 제외
        if (!string.IsNullOrEmpty(stageData.Need_Cost_Type))
        {
            //var type = (TICKET_TYPE)Enum.Parse(typeof(TICKET_TYPE), stageData.Need_Cost_Type);
            var type =stageData.Need_Cost_Type;

            if (!string.IsNullOrEmpty(type))
                SetTicket(new string[] { type });

            //if (type != TICKET_TYPE.none)
            //    SetTicket(new TICKET_TYPE[] { type });
        }
        else
            SetTicket(new string[] { });

        //SetTicket(new TICKET_TYPE[] { });
    }

    private void SetActiveFirstObj(bool isActive)
    {
        _firstItemCell.gameObject.SetActive(isActive);
        _firstTitleObj.SetActive(isActive);
    }

    private void UpdateStageTip()
    {
        var contentData = TableManager.Instance.Content_Table.GetDataByTypeID(_data.CONTENTS_TYPE_ID);
        if (contentData.Count == 0)
        {
            return;
        }
        if (_data.CONTENTS_TYPE_ID != CONTENTS_TYPE_ID.manjang)
        {
            if(string.IsNullOrEmpty(contentData[0].Str_Desc_Content_Stage))
                _stageTipTMP.gameObject.SetActive(false);
            else 
            {
                _stageTipTMP.ToTableText(contentData[0].Str_Desc_Content_Stage);
                 _stageTipTMP.gameObject.SetActive(true);
            }
        }
        else
            _stageTipTMP.gameObject.SetActive(false);
    }

    //별조건 
    private void UpdateStarCondition(bool isHaveMission)
    {
        _stars.Clear();
       
        if (isHaveMission)
        {
            //UpdateStageLists();

            switch (_data.CONTENTS_TYPE_ID)
            {
                case CONTENTS_TYPE_ID.stage_main:
                case CONTENTS_TYPE_ID.stage_sub:
                case CONTENTS_TYPE_ID.stage_character:
                case CONTENTS_TYPE_ID.event_main:
                    {
                        var clearStage = new Dictionary<string, int[]>();
                        if (_data.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.event_main)
                            clearStage = GameInfoManager.Instance.EventStoryInfo.ClearEventStage;
                        else
                            clearStage = GameInfoManager.Instance.StageInfo.ClearStage;

                        if (clearStage.ContainsKey(_data.Tid))
                        {
                            for (int i = 0; i < clearStage[_data.Tid].Length -1; i++)
                            {
                                _stars.Add(clearStage[_data.Tid][i]);
                            }
                        }
                        else
                        {
                            for (int s = 0; s< _missioList.Count; s++)
                            {
                                _stars.Add(0);
                            }
                        }
                    }
                    break;
                case CONTENTS_TYPE_ID.manjang:
                    {
                        var clearStage = GameInfoManager.Instance.DungeonInfo.GetStarValue(_data.CONTENTS_TYPE_ID, _data.Tid);

                        if (clearStage != null)
                        {
                            _stars =  GameInfoManager.Instance.DungeonInfo.GetStarValue(_data.CONTENTS_TYPE_ID, _data.Tid).ToList();
                        }
                        else
                        {
                            for (int i = 0; i < _missioList.Count; i++) 
                            {
                                _stars.Add(0);
                            }
                        }
                    }
                    break;
            }
            for (int s = 0; s < _stars.Count; s++)
            {
                _conditions[s].Init(_stars[s], _missioList[s].stage_clear_mission_type, _missioList[s].stage_clear_mission_value);
            }
        }
        else
        {
            for (int m = 0; m < _conditions.Count; m++)
            {
                _conditions[m].gameObject.SetActive(false);
            }
        }
    
    }

    private void UpdateUI(bool enableSweep)
    {
        if (_data.CONTENTS_TYPE_ID.Equals(CONTENTS_TYPE_ID.manjang))
        {
            enableSweep = true;
            _needLevelTxt.gameObject.SetActive(false);
        }
        else
        {
            _needLevelTxt.gameObject.SetActive(true);
        }

        _title.ToTableText("str_ui_Instance_Result_clear_stageflag");
        string text = LocalizeManager.Instance.GetString("str_ui_content_dungeon_condition_power_01");
        _needLevelTxt.ToTableText(text + _data.Need_Team_Power.ToString());

        SetMonsterInfo();

        //_uiCombatButton.Init(_data.Tid);
        _uiContentTeam.Show(_data);
        //Debug.Log("<color=#feee00>UIStageInfo enableSweep : "+enableSweep+"</color>");
        SetSweepButtonUI(enableSweep);
        //_sweepDimmed.SetActive(!enableSweep);
        //_sweepButton.gameObject.SetActive(!_data.CONTENTS_TYPE_ID.Equals(CONTENTS_TYPE_ID.manjang));
        _useHelperGoodsTMP.text = GameInfoManager.Instance.UseHelperNeedGold.ToString();
        SetRewardItemInfo(_data.CONTENTS_TYPE_ID);
        UpdateFoodUI();
        UpdateStageTip();
    }

    private void SetSweepButtonUI(bool isEnableSweep)
    {
        if (_data.Stage_Clearing == 1)    // 소탕 가능
        {
            _sweepButton.gameObject.SetActive(false);
            _sweepDimmed.SetActive(false);
        }
        else
        {
            _sweepButton.gameObject.SetActive(true);
            _sweepDimmed.SetActive(!isEnableSweep);
        }
    }

    public void SetMonsterInfo()
    {
        var monsterList = TableManager.Instance.Stage_Table.GetEnemyTidList(_data.Tid);
        int count = monsterList.Count  - _monsterContent.childCount;
        if (count > 0)
        {
            for (int m = 0; m < count; m++)
            {
                GameObject go = GameObject.Instantiate(_monsterItem, _monsterContent);
                UITeamSlot uiTeamSlot = go.GetComponent<UITeamSlot>();
                _monsterSlotList.Add(uiTeamSlot);
            }
        }
        else
        {
            for (int m = 0; m<  _monsterContent.childCount; m++)
            {
                _monsterContent.GetChild(m).gameObject.SetActive(false);
            }
        }
        for (int n = 0; n < monsterList.Count; n++)
        {
            var slot = _monsterContent.GetChild(n).gameObject;
            slot.SetActive(true);
            UITeamSlot teamSlot = slot.GetComponent<UITeamSlot>();
            teamSlot.SetupMonster(monsterList[n]);
            _monsterSlotList.Add(teamSlot);
        }
    }

    private void UpdateFirstReward()
    {
        SetActiveFirstObj(true);

        var datas = TableManager.Instance.Reward_Table.GetDatas(_data.First_Reward_Clear.FirstOrDefault());
        var data = datas[0];
        var isClear = GameInfoManager.Instance.StageInfo.IsClear(_data.Tid);

        _firstItemCell.UpdateData(data.Item_Tid, data.ITEM_TYPE, ItemCustomValueType.RewardAmount, data.Item_Min);
        _firstItemCell.SetActiveAcquired(isClear);
    }

    public void SetRewardItemInfo(CONTENTS_TYPE_ID typeid)
    {
        _itemCellDatas.Clear();

        var groupids = _data.Reward_01_Info;
        for (int i = 0; i < groupids.Length; i++)
        {
            var rewarddata = Utils.ToItemCellDatas(groupids[i]);

            // 중복된 tid가 있으면 item_mis 값이 가장 작은 것만 노출 
            var groupdata = rewarddata.GroupBy(data => data.Tid)
                .Select(group => group.OrderBy(data => data.RewardAmount).FirstOrDefault());
            _itemCellDatas.AddRange(groupdata);

            //_itemCellDatas.AddRange(Utils.ToItemCellDatas(groupids[i]));
        }

        if (_data.First_Reward_Clear.Length > 0)
        {
            var datas = TableManager.Instance.Reward_Table.GetDatas(_data.First_Reward_Clear.FirstOrDefault());
            var data = datas[0];
            bool isClear = false;

            switch (typeid)
            {
                case CONTENTS_TYPE_ID.stage_main:
                case CONTENTS_TYPE_ID.stage_sub:
                case CONTENTS_TYPE_ID.stage_character:
                    isClear = GameInfoManager.Instance.StageInfo.IsClear(_data.Tid);
                    break;
                case CONTENTS_TYPE_ID.manjang:
                    isClear = GameInfoManager.Instance.DungeonInfo.IsClear(_data.CONTENTS_TYPE_ID, _data.Tid);
                    break;
                case CONTENTS_TYPE_ID.event_main:
                    isClear = GameInfoManager.Instance.EventStoryInfo.IsClear(_data.Tid);
                    break;
            }

            //if (typeid == CONTENTS_TYPE_ID.manjang)
            //{
            //    isClear = GameInfoManager.Instance.DungeonInfo.IsClear(_data.CONTENTS_TYPE_ID, _data.Tid);
            //    //UpdateFirstReward();
            //}
            //else
            //{
               
            //    isClear = GameInfoManager.Instance.StageInfo.IsClear(_data.Tid);
            //}

            if (!isClear)
            {
                //_cells[0].SetDataCells(new ItemCellData(new RewardCellData(data.Item_Tid, data.Item_Min, data.ITEM_TYPE), ItemCustomValueType.RewardAmount));
                _itemCellDatas.Insert(0, new ItemCellData(new RewardCellData(data.Item_Tid, data.Item_Min, data.ITEM_TYPE), ItemCustomValueType.RewardAmount, true));
            }
        }
        for (int i = 0; i< _cells.Count; i++)
        {
            //if (!isClear)
            //{
            //    _cells[0].SetDataCells(_itemCellDatas, true);
            //}
            _cells[i].SetDataCells(_itemCellDatas);
            _cells[i].EnableUnSelected(true);
        }
        _rewardScroll.ActivateCells(_itemCellDatas.Count);
      
    }

    // sweep 보상 리스트 또는 메인 보상 외 서브 보상 리스트
    public void SetSweepRewardsUI(bool isOn)
    {
        _sweepRewardTitleObj.SetActive(isOn);
        //_sweepRewardScroll.gameObject.SetActive(isOn);
        _sweepRewardRect.gameObject.SetActive(isOn);

        // 보상 보여주기
        if (isOn)
        {
            _rewardRect.sizeDelta = new Vector2(1100, _rewardRect.sizeDelta.y);
            _sweepItemCellDatas.Clear();
            var stageDatas = TableManager.Instance.Stage_Table.GetDatas(_data.CONTENTS_TYPE_ID);
            var stageRewardTids = new Dictionary<string, int>();
            var clearData = GameInfoManager.Instance.DungeonInfo.GetClearData(_data.CONTENTS_TYPE_ID);
            int highPoint = int.Parse(clearData.First());

            for (int i = 0; i < stageDatas.Count; i++)
            {
                if (highPoint < i * 5)
                    break;

                var rewardData = TableManager.Instance.Reward_Table.GetRewardDataByGroupId(stageDatas[i].Reward_01_Info.FirstOrDefault());

                if (!stageRewardTids.ContainsKey(rewardData.Item_Tid))
                {
                    stageRewardTids.Add(rewardData.Item_Tid, rewardData.Item_Min);
                }
                else
                {
                    stageRewardTids[rewardData.Item_Tid] += rewardData.Item_Min;
                }
            }

            foreach (var rewarInfo in stageRewardTids)
            {
                var sweepkkaebi = TableManager.Instance.Item_Table[rewarInfo.Key];
                _sweepItemCellDatas.Add(new ItemCellData(new RewardCellData(rewarInfo.Key, rewarInfo.Value, sweepkkaebi.ITEM_TYPE), ItemCustomValueType.RewardAmount));
            }
            for (int n = 0; n < _sweepCells.Count; n++)
            {
                _sweepCells[n].SetDataCells(_sweepItemCellDatas);
                _sweepCells[n].EnableUnSelected(true);
            }

            _sweepRewardScroll.ActivateCells(_sweepItemCellDatas.Count);
        }
        else
        {
            _rewardRect.sizeDelta = new Vector2(1940, _rewardRect.sizeDelta.y);
        }
    }

    public void UpdateFoodUI()
    {
        var foodDict = GameInfoManager.Instance.FoodInfo.FoodDict;// [_data.CONTENTS_TYPE_ID.ToContentString()];
        if (foodDict.ContainsKey(_data.CONTENTS_TYPE_ID.ToContentString()))
        {
            var itemTid = foodDict[_data.CONTENTS_TYPE_ID.ToContentString()];
            var amount = GameInfoManager.Instance.GetAmount(itemTid);
            if(amount <= 0)
            {
                //선택 메뉴 초기화
                foodDict[_data.CONTENTS_TYPE_ID.ToContentString()]="";
            }
            UpdateCookItemInfo(amount >0); 
        }
        else
        {
            UpdateCookItemInfo(false);
        }
        //RestApiManager.Instance.FoodCheck(_data.CONTENTS_TYPE_ID.ToString(), UpdateCookItemInfo);
    }

    private void UpdateCookItemInfo(bool haveFood = false)
    {
        if (!haveFood)
        {
            _cookButtonIcon.gameObject.SetActive(false);
            _cookButtonEmptyIcon.gameObject.SetActive(true);
            return;
        }

        var foodTid = GameInfoManager.Instance.FoodInfo.FoodDict[_data.CONTENTS_TYPE_ID.ToContentString()];

        _cookButtonIcon.gameObject.SetActive(true);
        _cookButtonEmptyIcon.gameObject.SetActive(false);
        var itemData = TableManager.Instance.Item_Table[foodTid];
        if (itemData ==null)
        {
            Debug.LogError($"Not Found Item Data {foodTid}");
            return;
        }
        _cookButtonIcon.SetSprite(itemData.Icon_Id);
    }

    public void OnClickEnterStage()
    {
        var team = GameInfoManager.Instance.OrganizationInfo.GetTeam();

        Action onClickGoStage = () => {
            var holdAmount = GameInfoManager.Instance.GetAmount("i_gold");
            if (team.GetHelperTids().Count > 0 && holdAmount < GameInfoManager.Instance.UseHelperNeedGold)
            {
                UIManager.Instance.ShowToastMessage("str_ui_stage_failed_goldshortage"); //골드가 부족합니다
                return;
            }
          
            var infoManager = GameInfoManager.Instance;
            infoManager.FoodInfo.FoodDict.TryGetValue(_data.CONTENTS_TYPE_ID.ToContentString(), out var foodTid);
            var selectFood = "";
            if (!foodTid.IsNullOrEmpty())
            {
                var item = infoManager.GetItemInfo(foodTid);
                if (item !=null && item.Amount>0)
                {
                    selectFood = foodTid;
                }
            }

            infoManager.AccountInfo.SelectedCookItemTid =selectFood;// foodTid.IsNullOrEmpty() ? "" : foodTid;        else
            if (infoManager.AccountInfo.SelectedCookItemTid.IsNullOrEmpty() || GameInfoManager.Instance.GetAmount(infoManager.AccountInfo.SelectedCookItemTid) == 0)
            {
                RestApiManager.Instance.SetFood(_data.CONTENTS_TYPE_ID.ToString(), "");
            }
            GameInfoManager.Instance.OrganizationInfo.SetLastPreset(_data.CONTENTS_TYPE_ID, _uiContentTeam.Index);
            infoManager.OnEnterDungeon(_data.CONTENTS_TYPE_ID, _data.Tid, StageEnter);
        };

        // 조력자 있는치 체크
       
        if (team.GetHelperTids().Count > 0)
        {
            if (_uiUseHelperAlert != null && !_uiUseHelperAlert.gameObject.activeInHierarchy)
            {
                var message = LocalizeManager.Instance.GetString("str_team_formation_battle_msg_01");
                Action onClickCancle = () => { 
                        _uiUseHelperAlert.Close();
                    };

                Action onClickOk = () => { 
                    
                };
                _uiUseHelperAlert.Show(PopupButtonType.OK_CANCEL, message: message, true, 
                    onClickOk: onClickGoStage, onClickCancle: onClickCancle);
            }
            else
                onClickGoStage.Invoke();
          
        }
        else
            onClickGoStage.Invoke();

        //var infoManager = GameInfoManager.Instance;
        //infoManager.FoodInfo.FoodDict.TryGetValue(_data.CONTENTS_TYPE_ID.ToContentString(), out var foodTid);
        //var selectFood = "";
        //if (!foodTid.IsNullOrEmpty())
        //{
        //    var item = infoManager.GetItemInfo(foodTid);
        //    if (item !=null && item.Amount>0)
        //    {
        //        selectFood = foodTid;
        //    }
        //}

        //infoManager.AccountInfo.SelectedCookItemTid =selectFood;// foodTid.IsNullOrEmpty() ? "" : foodTid;        else
        //if(infoManager.AccountInfo.SelectedCookItemTid.IsNullOrEmpty() || GameInfoManager.Instance.GetAmount(infoManager.AccountInfo.SelectedCookItemTid) == 0)
        //{
        //    RestApiManager.Instance.SetFood(_data.CONTENTS_TYPE_ID.ToString(), "");
        //}
        //GameInfoManager.Instance.OrganizationInfo.SetLastPreset(_data.CONTENTS_TYPE_ID, _uiContentTeam.Index);
        //infoManager.OnEnterDungeon(_data.CONTENTS_TYPE_ID, _data.Tid, StageEnter);
    }

    public void OnClickSweep()
    {
        if(TableManager.Instance.Stage_Table.TryGetData(_data.Tid, out var stageTableData))
        {
            var costType = stageTableData.Need_Cost_Type;
            var cost = stageTableData.Need_Cost_Value;
            var amount = GameInfoManager.Instance.GetAmount(costType);

            if (Enum.TryParse(typeof(TICKET_TYPE), costType, out var ticketType))
            {
                if (amount < cost)
                {
                    UIManager.Instance.Show<UIPopupPurchaseGoods>(Utils.GetUIOption(UIOption.Tid, ticketType));
                    return;
                }
            }
        }

        UIManager.Instance.Show<UIPopupSweep>(Utils.GetUIOption(
            UIOption.Tid, _data.Tid));
    }

    public void OnClickCook()
    {
        Action callback = UpdateFoodUI;
        UIManager.Instance.Show<UIUseCookPopup>(Utils.GetUIOption(UIOption.Callback, callback, UIOption.EnumType, _data.CONTENTS_TYPE_ID));
    }

    private void StageEnter()
    {
        switch (_data.CONTENTS_TYPE_ID)
        {
            case CONTENTS_TYPE_ID.stage_main:
            case CONTENTS_TYPE_ID.stage_sub:
            case CONTENTS_TYPE_ID.stage_character:
                var team = GameInfoManager.Instance.OrganizationInfo.GetTeam();
                RestApiManager.Instance.RequestStageEnter(_data.Tid, team.GetTids(), () => _onEnterStage?.Invoke());
                //RestApiManager.Instance.RequestStageEnter(_data.Tid, team.GetHoldTids(), () => _onEnterStage?.Invoke());
                break;
            default:
                _onEnterStage?.Invoke();
                break;
        }
    }

    public void OnClickSweepDimd()
    {
        string str = "";
        switch (_data.CONTENTS_TYPE_ID)
        {
            case CONTENTS_TYPE_ID.stage_main:
            case CONTENTS_TYPE_ID.stage_sub:
            case CONTENTS_TYPE_ID.stage_character:
            case CONTENTS_TYPE_ID.event_main:
                str = LocalizeManager.Instance.GetString("str_ui_if_clear_open_02");
                break;
            case CONTENTS_TYPE_ID.challenge:
            case CONTENTS_TYPE_ID.trial:
            case CONTENTS_TYPE_ID.elemental:
            case CONTENTS_TYPE_ID.exp:
            case CONTENTS_TYPE_ID.gold:
            case CONTENTS_TYPE_ID.content_class:
            case CONTENTS_TYPE_ID.relay_boss:
                str = LocalizeManager.Instance.GetString("str_ui_if_clear_open_01");
                break;
            default:
                _onEnterStage?.Invoke();
                break;
        }

      // string str = LocalizeManager.Instance.GetString("str_ui_if_clear_open_02");
        UIManager.Instance.ShowToastMessage($"{str}");
    }

    public void RefreshCombatButton()
    {
        //_uiCombatButton.Init(_data.Tid);
    }

    public void Close()
    {

    }



}
