using Consts;
using LIFULSE.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UIPause : UIBase
{
    [SerializeField] private GameObject _timeObj;
    [SerializeField] private GameObject _giveUpObj;
    [SerializeField] private GameObject _restartObj;
    [SerializeField] private ExTMPUI _remainTMP;
    
    private Stage_TableData _stageData;

    public override void Close(bool needCached = true)
    {
        GameManager.Instance.CombatController.Pause(false);
        base.Close(needCached);
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        if (optionDict.TryGetValue(UIOption.Tid, out var stageTid))
            _stageData = TableManager.Instance.Stage_Table[stageTid.ToString()];

        //var isShowGiveUp = (GameInfoManager.Instance.AccountInfo.PrograssIndex != AccountInfo.PrologueStep.PrologueCombat1&&
        //                    GameInfoManager.Instance.AccountInfo.PrograssIndex != AccountInfo.PrologueStep.PrologueCombat2)
        //                    && _stageData.CONTENTS_TYPE_ID != CONTENTS_TYPE_ID.pvp;

        _giveUpObj.SetActive(_stageData.CONTENTS_TYPE_ID != CONTENTS_TYPE_ID.pvp);
        SetReStartBtn(_stageData.CONTENTS_TYPE_ID);
        switch (_stageData.CONTENTS_TYPE_ID)
        {
            case CONTENTS_TYPE_ID.stage_main:
            case CONTENTS_TYPE_ID.stage_sub:
            case CONTENTS_TYPE_ID.stage_character:
            case CONTENTS_TYPE_ID.prologue:
            case CONTENTS_TYPE_ID.exp:
            case CONTENTS_TYPE_ID.pvp:
                _timeObj.SetActive(false);
                break;
            default:
                {
                    var rTime = (int)(_stageData.Limit_Time * 0.001f) -  GameManager.Instance.CombatController.RemainTime;
                    TimeSpan timeSpan = TimeSpan.FromSeconds(GameManager.Instance.CombatController.RemainTime);

                    _remainTMP.text = timeSpan.ToString(@"mm\:ss\.fff");
                    _timeObj.SetActive(true);
                }
                break;
        }

      
    }

    private void SetReStartBtn(CONTENTS_TYPE_ID type)
    {
        switch (type)
        {
            case CONTENTS_TYPE_ID.pvp:
            case CONTENTS_TYPE_ID.prologue:
                _restartObj.SetActive(false);
                break;
            default:
                _restartObj.SetActive(true);
                break;
        }
    }

    public void OnClickSetting()
    {
        UIManager.Instance.Show<UISetting>();
    }

    public void OnClickContinue()
    {
        //GameManager.Instance.CombatController.Pause(false);
        //OnClickClose();
        Close();
    }

    public void OnClickReStart()
    {
        //return;
        var message = LocalizeManager.Instance.GetString("str_ui_pause_popup_info_01"); //전투를 재시작 하시겠습니까?
        UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK_CANCEL,message: message
            ,onClickOK: OnReStart);
        //UIManager.Instance.Show<UIPopupAlert>(Utils.GetUIOption(
        //                        UI
        //                        UIOption.Message, message,
        //                        UIOption.OnClickOk, (Action)OnReStart)); 
    }

    public void OnClickGiveUp()
    {
        if (GameInfoManager.Instance.IsDaily)
        {
            GameInfoManager.Instance.IsDailyInitialization();
            return;
        }
        var message = LocalizeManager.Instance.GetString("str_ui_pause_popup_info_02"); //전투를 포기합니다
        UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK_CANCEL, message: message
           , onClickOK: OnGiveUp);

        //UIManager.Instance.Show<UIPopupAlert>(Utils.GetUIOption(
        //                UIOption.Message, "str_ui_pause_popup_info_02".ToTableText(),//전투를 포기합니다
        //                UIOption.OnClickOk, (Action)OnGiveUp)); 
    }

    private void OnReStart()
    {
        Action callback = null;
        GameManager.Instance.CombatController.EventController.EndStageStop(ResultType.GiveUp);
        GameManager.Instance.CombatController.EventController.IsEnd = true;
        GameManager.Instance.CombatController.Pause(false);

        //요리
        switch (_stageData.CONTENTS_TYPE_ID)
        {
            case CONTENTS_TYPE_ID.challenge:
            case CONTENTS_TYPE_ID.trial:
            case CONTENTS_TYPE_ID.total:
            case CONTENTS_TYPE_ID.pvp:
                break;
            default:
                {
                    var selectFood = "";

                    GameInfoManager.Instance.FoodInfo.FoodDict.TryGetValue(_stageData.CONTENTS_TYPE_ID.ToContentString(), out var foodTid);

                    if (!foodTid.IsNullOrEmpty())
                    {
                        var item = GameInfoManager.Instance.GetItemInfo(foodTid);
                        if (item !=null && item.Amount>0)
                        {
                            selectFood = foodTid;
                        }
                    }

                    GameInfoManager.Instance.AccountInfo.SelectedCookItemTid = selectFood;// foodTid.IsNullOrEmpty() ? "" : foodTid;
                }
                break;
        }

        switch (_stageData.CONTENTS_TYPE_ID)
        {
            case CONTENTS_TYPE_ID.stage_main:
            case CONTENTS_TYPE_ID.stage_sub:
            case CONTENTS_TYPE_ID.stage_character:
            case CONTENTS_TYPE_ID.gold:
            case CONTENTS_TYPE_ID.elemental:
            case CONTENTS_TYPE_ID.exp:
            case CONTENTS_TYPE_ID.content_class:
            case CONTENTS_TYPE_ID.relay_boss:
            case CONTENTS_TYPE_ID.character_guide:
            case CONTENTS_TYPE_ID.manjang:
                if (GameManager.Instance.CombatController != null)
                    GameManager.Instance.CombatController.IsEndStage = true;

                var stageData = TableManager.Instance.Stage_Table[GameInfoManager.Instance.EnterStageTid];
                SceneManager.Instance.LoadCombatScene(stageData.Stage_Scene);
                //callback = () =>
                //{
                //    var team = GameInfoManager.Instance.OrganizationInfo.GetTeam();
                //    RestApiManager.Instance.RequestStageEnter(_stageData.Tid, team.GetTids(), () => callback?.Invoke());
                //};
                break;
            case CONTENTS_TYPE_ID.challenge:
            case CONTENTS_TYPE_ID.total:
            case CONTENTS_TYPE_ID.trial:
                if (GameManager.Instance.CombatController != null)
                    GameManager.Instance.CombatController.IsEndStage = true;

                //callback = () => GameInfoManager.Instance.EnterStatus = EnterStatus.Success;
                var contentStageData = TableManager.Instance.Stage_Table[GameInfoManager.Instance.EnterStageTid];
                SceneManager.Instance.LoadCombatScene(contentStageData.Stage_Scene);
                break;
        }
        //GameInfoManager.Instance.OnEnterDungeon(_stageData.CONTENTS_TYPE_ID, _stageData.Tid, callback);
    }

    //해당 컨텐츠 UI로
    private void OnGiveUp()
    {
        GameManager.Instance.CombatController.Pause(false);
        GameInfoManager.Instance.IsSkipInteractionSounde = true;
        UIManager.Instance.GoContent(_stageData.Tid,null,true);
    }
}
