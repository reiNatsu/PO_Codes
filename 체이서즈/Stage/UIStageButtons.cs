using Consts;
using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStageButtons : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private UIContentTeam _uiContentTeam = null;
    [SerializeField] private AlertInfo _uiUseHelperAlert = null;
    [SerializeField] private UICombatButton _uiCombatButton;
    [Header("기본")]
    [SerializeField] private GameObject _sweepButtonObj;
    [SerializeField] private GameObject _sweepDimd;
    [SerializeField] private ExImage _cookButtonEmptyIcon;
    [SerializeField] private ExImage _cookButtonIcon;
    [SerializeField] private GameObject _mockBattleButtonObj;
    [SerializeField] private GameObject _goStageButtonObj;
    [SerializeField] private GameObject _cookButtonObj;

    private Stage_TableData _data;
    private Action _onEnterStage;
    private string _stageBossId;

    public void Show(Stage_TableData data, bool isHideCombatBtn = false, bool showCook = true)
    {
        _data = data;
        _uiCombatButton.Init(data.Tid);
        _uiCombatButton.gameObject.SetActive(!isHideCombatBtn);
        _cookButtonObj.SetActive(showCook);
        _goStageButtonObj.SetActive(isHideCombatBtn);
        if (_uiUseHelperAlert != null)
            _uiUseHelperAlert.gameObject.SetActive(false);
        _mockBattleButtonObj.SetActive(false);                  // 모의전이 있을지 없을 지 모르니까 일단 꺼두기.
        SetSweepButtonUI();
        UpdateFoodUI();
    }
    // Challange, Total, Trial 등에서 보스 이름 체크하기 위해서.
    public void SetStageBossId(string bossId)
    {
        _stageBossId = bossId;
    }

    

    public void UpdateComabatUI()
    {
        _uiCombatButton.Init(_data.Tid);
    }

    // 스테이지 소탕 여부 세팅. 0: 가능, 1: 불가능
    private void SetSweepButtonUI()
    {
        if (_data.Stage_Clearing == 1)    // 소탕 불가능
        {
            _sweepButtonObj.gameObject.SetActive(false);
            _sweepDimd.SetActive(false);
        }
        else
        {
            var selectTeam = GameInfoManager.Instance.OrganizationInfo.SelectedTeam;

            //조력자가 있을 경우 소탕 불가능
            if (selectTeam != null && selectTeam.IsIncludeHelper())
            {
                _sweepButtonObj.gameObject.SetActive(false);
                _sweepDimd.SetActive(false);
            }
            else
            {
                _sweepButtonObj.gameObject.SetActive(true);
                switch (_data.CONTENTS_TYPE_ID)
                {
                    case CONTENTS_TYPE_ID.stage_main:
                    case CONTENTS_TYPE_ID.stage_sub:
                        _sweepDimd.SetActive(!GameInfoManager.Instance.StageInfo.IsClear(_data.Tid));
                        break;
                    case CONTENTS_TYPE_ID.event_main:
                    case CONTENTS_TYPE_ID.story_event:
                        _sweepDimd.SetActive(!GameInfoManager.Instance.EventStoryInfo.IsClear(_data.Tid));
                        break;
                    default:
                        _sweepDimd.SetActive(!GameInfoManager.Instance.DungeonInfo.IsClear(_data.CONTENTS_TYPE_ID, _data.Tid));
                        break;
                }
                //_sweepDimd.SetActive(!GameInfoManager.Instance.StageInfo.IsClear(_data.Tid));
            }
        }
    }

    public void OnClickEnterButton()
    {
        bool isPopup = false;
        var uistageinfopopup = UIManager.Instance.GetUI<UIStageInfoPopup>();
        if (uistageinfopopup != null && uistageinfopopup.gameObject.activeInHierarchy)
            isPopup = true;

        if (!isPopup)           // 팝업에서 여는게 아닌 해당 컨텐츠 UI에서 여는 경우
        {
            switch (_data.CONTENTS_TYPE_ID)
            {
                case CONTENTS_TYPE_ID.stage_main:
                case CONTENTS_TYPE_ID.stage_sub:
                case CONTENTS_TYPE_ID.stage_character:
                case CONTENTS_TYPE_ID.event_main:
                case CONTENTS_TYPE_ID.story_event:
                    OnClickEnterStage();
                    break;
                case CONTENTS_TYPE_ID.total:
                    OnClickEnterTotal();
                    break;
                default:
                    GameInfoManager.Instance.OnEnterDungeon(_data.CONTENTS_TYPE_ID, _data.Tid);
                    break;
            }
        }
        else                     // 팝업에서 여는 경우 전투 버튼 누르면 해당 컨텐츠 UI로 보내줘야 함.
        {
            switch (_data.CONTENTS_TYPE_ID)
            {
                case CONTENTS_TYPE_ID.stage_main:
                case CONTENTS_TYPE_ID.stage_sub:
                case CONTENTS_TYPE_ID.stage_character:
                    {
                        uistageinfopopup.OnClickClose();
                        UIManager.Instance.Show<UIStage>(Utils.GetUIOption(
                            UIOption.Index, _data.Theme_Id,
                            UIOption.Data, _data));
                    }
                    break;
              
                case CONTENTS_TYPE_ID.total:
                    break;
                default:
                    break;
            }
        }
    }

    // 스테이지 진입 버튼
    public void OnClickEnterStage()
    {
        var team = GameInfoManager.Instance.OrganizationInfo.GetTeam();
        if (GameInfoManager.Instance.OrganizationInfo.SelectedTeam.IsEmpty())
        {
            UIManager.Instance.ShowToastMessage("str_ui_part_error_001");       //1명 이상의 도사가 편성되어야 플레이가 가능합니다.
            return;
        }


        Action onClickGoStage = () => {
            var holdAmount = GameInfoManager.Instance.GetAmount("i_gold");
            if (team.GetHelperTids().Count > 0 && holdAmount < GameInfoManager.Instance.UseHelperNeedGold)
            {
                UIManager.Instance.ShowToastMessage("str_ui_stage_failed_goldshortage");    // 골드가 부족합니다.
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
            if (_uiContentTeam != null)
            {
                GameInfoManager.Instance.OrganizationInfo.SetLastPreset(_data.CONTENTS_TYPE_ID, _uiContentTeam.Index);
                infoManager.OnEnterDungeon(_data.CONTENTS_TYPE_ID, _data.Tid, StageEnter);
            }
            else
            {
                UIManager.Instance.ShowToastMessage("str_ui_part_error_001");       //1명 이상의 도사가 편성되어야 플레이가 가능합니다.
                return;
            }
        };

        // 조력자 있는치 체크

        if (team.GetHelperTids().Count > 0)
        {
            Action onClickCancle = () =>
            {
                _uiUseHelperAlert.Close();
            };

            UIManager.Instance.Show<UIPopupUseHelper>(
                Utils.GetUIOption(UIOption.Action, onClickGoStage)
                );
            //if (_uiUseHelperAlert != null && !_uiUseHelperAlert.gameObject.activeInHierarchy)
            //{

            //    //var message = LocalizeManager.Instance.GetString("str_team_formation_battle_msg_01");
            //    ////조력자를 사용하여 전투에 진입하시겠습니까?\n조력자는 1회만 사용 가능하며, 사용 시 골드가 소모됩니다.


            //    //Action onClickOk = () => {

            //    //};

            //    //if (_useHelperGoldTMP != null)
            //    //    _useHelperGoldTMP.text = TableManager.Instance.Define_Table["ds_helper_default"].Opt_01_Int.ToString();

            //    //_uiUseHelperAlert.Show(PopupButtonType.OK_CANCEL, message: message, true,
            //    //    onClickOk: onClickGoStage, onClickCancle: onClickCancle);
            //}
            //else
            //    onClickGoStage.Invoke();

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

    // Total 진입 액션
    public void OnClickEnterTotal()
    {
        if (GameInfoManager.Instance.OrganizationInfo.SelectedTeam.IsEmpty())
        {
            UIManager.Instance.ShowToastMessage("str_ui_part_error_001");       //1명 이상의 도사가 편성되어야 플레이가 가능합니다.
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

        EnterTotal();
    }

    private void EnterTotal()
    {
        if (_data.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.total && GameInfoManager.Instance.OrganizationInfo.SelectedTeam.IsContainsUsedTotal())
        {
            UIManager.Instance.ShowToastMessage("str_content_total_character_error");
            return;
        }

        bool unEntered;

        //섬멸전 입장
        Action onClick = () =>
        {
            switch (_data.CONTENTS_TYPE_ID)
            {
                case CONTENTS_TYPE_ID.total:
                    unEntered = string.IsNullOrEmpty(GameInfoManager.Instance.GetBossInfo(_data.CONTENTS_TYPE_ID).Tid);
                    break;
                default:
                    unEntered = true;
                    break;
            };

            //이미 진행 중인 섬멸전이 없을 때
            if (unEntered)
            {
                GameInfoManager.Instance.OnEnterDungeon(_data.CONTENTS_TYPE_ID, _data.Tid, 
                    () => RestApiManager.Instance.RequestTotalEnter(
                        _data.CONTENTS_TYPE_ID.ToLeaderboardId(), 
                        "0", 
                        _stageBossId, 
                        GameInfoManager.Instance.AccountInfo.Name,
                        GameInfoManager.Instance.AccountInfo.Nation,
                        GameInfoManager.Instance.ClanInfo.MyClanData.Name));
            }
            else
                GameInfoManager.Instance.OnEnterDungeon(_data.CONTENTS_TYPE_ID, _data.Tid, () => GameInfoManager.Instance.EnterStatus = EnterStatus.Success);
        };

        if (_data.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.total)
        {
            UIManager.Instance.Show<UIPopupEnterCombat>(Utils.GetUIOption(
                UIOption.Tid, _data.Tid,
                UIOption.Callback, onClick));
        }
        else
        {
            onClick?.Invoke();
        }
    }

    // 소탕 버튼 클릭
    public void OnClickSweep()
    {
        switch (_data.CONTENTS_TYPE_ID)
        {
            case CONTENTS_TYPE_ID.stage_main:
            case CONTENTS_TYPE_ID.stage_sub:
                if (TableManager.Instance.Stage_Table.TryGetData(_data.Tid, out var stageTableData))
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
                break;
            case CONTENTS_TYPE_ID.challenge:
            case CONTENTS_TYPE_ID.total:
            case CONTENTS_TYPE_ID.trial:
                if (GameInfoManager.Instance.IsEnoughItem(_data.Need_Cost_Type, _data.Need_Cost_Value))
                    UIManager.Instance.Show<UIPopupSweep>(Utils.GetUIOption(UIOption.Tid, _data.Tid));
                else
                {
                    var itemData = TableManager.Instance.Item_Table[_data.Need_Cost_Type];

                    if (itemData != null)
                        UIManager.Instance.ShowToastMessage("str_ui_ticketplz_msg".ToTableArgs(itemData.Item_Name_Text_Id.ToTableText()));
                    else
                        UIManager.Instance.ShowToastMessage("str_ui_content_enter_failed_01"); //입장권이 부족합니다.
                }
                break;
            default:
                break;
        }
       

        UIManager.Instance.Show<UIPopupSweep>(Utils.GetUIOption(
            UIOption.Tid, _data.Tid));
    }

    // 소탕 버튼 dimd 클릭
    public void OnClickSweepDimd()
    {
        //Todo 신준호 => 조력자를 편성한 경우에는 소탕 불가능
        var selectTeam = GameInfoManager.Instance.OrganizationInfo.SelectedTeam;
       
        if (selectTeam.IsIncludeHelper())
        {
            UIManager.Instance.ShowToastMessage("str_ui_team_formation_error_001");
            return;
        }

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
                str = LocalizeManager.Instance.GetString("str_content_total_difficulty_guide_02");   //클리어 시 해금
                break;
            case CONTENTS_TYPE_ID.elemental:
            case CONTENTS_TYPE_ID.exp:
            case CONTENTS_TYPE_ID.gold:
            case CONTENTS_TYPE_ID.relay_boss:
            case CONTENTS_TYPE_ID.content_class:
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
        _uiCombatButton.Init(_data.Tid);
    }

    //요리 버튼 클릭
    public void OnClickCook()
    {
        Action callback = UpdateFoodUI;
        UIManager.Instance.Show<UIUseCookPopup>(Utils.GetUIOption(UIOption.Callback, callback, UIOption.EnumType, _data.CONTENTS_TYPE_ID));
    }
    public void UpdateFoodUI() 
    {
        var foodDict = GameInfoManager.Instance.FoodInfo.FoodDict;// [_data.CONTENTS_TYPE_ID.ToContentString()];
        if (foodDict.ContainsKey(_data.CONTENTS_TYPE_ID.ToContentString()))
        {
            var itemTid = foodDict[_data.CONTENTS_TYPE_ID.ToContentString()];
            var amount = GameInfoManager.Instance.GetAmount(itemTid);
            if (amount <= 0)
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

    public void OnClickGiveUp()
    {
        if (_data.CONTENTS_TYPE_ID != CONTENTS_TYPE_ID.total)
            return;

        Action onGiveup = () =>
        {
            GameInfoManager.Instance.GetBossInfo(_data.CONTENTS_TYPE_ID).GiveUp(ShowUIPopupTotalResult);
        };

        UIManager.Instance.Show<UIPopupAlert>(
            Utils.GetUIOption(UIOption.Message, "str_ui_total_popup_giveup_info".ToTableText(),
                              UIOption.OnClickOk, onGiveup)); //포기하시겠습니까?
    }

    private void ShowUIPopupTotalResult()
    {
        UIManager.Instance.CloseAllUI();
        UIManager.Instance.Show<UILobby>();

        switch (_data.CONTENTS_TYPE_ID)
        {
            default:
                UIManager.Instance.Show<UIChallenge>(Utils.GetUIOption(UIOption.EnumType, _data.CONTENTS_TYPE_ID.ToLeaderboardId()));
                break;
            case CONTENTS_TYPE_ID.total:
                UIManager.Instance.Show<UITotal>();
                break;
        }
    }


    // 섬멸전, 시험대 등에서 모의전, 소탕 dimd 예외처리 하기 위해
    public void SetIsSeasons(bool showMock, bool isEnableSweep)
    {
        _mockBattleButtonObj.SetActive(showMock);
        _sweepDimd.SetActive(isEnableSweep);
    }

    public void OnClickGoStage()
    {
        UIManager.Instance.Show<UIStage>(Utils.GetUIOption(
           UIOption.Index, _data.Theme_Id
           , UIOption.Data, _data,
           UIOption.Bool, false));
    }
}
