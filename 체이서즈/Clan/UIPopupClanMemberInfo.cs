using Consts;
using LIFULSE.Manager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupClanMemberInfo : UIBase
{
    [SerializeField] private ExImage _icon;
    [SerializeField] private ExImage _frame;
    [SerializeField] private ExTMPUI _gradeTMP;
    [SerializeField] private ExTMPUI _nicnameTMP;
    [SerializeField] private ExTMPUI _levelTMP;
    [SerializeField] private ExTMPUI _totalPowrtTMP;
    [SerializeField] private ExTMPUI _totalPointTMP;
    [SerializeField] private ExTMPUI _weekPointTMP;
    [SerializeField] private ExTMPUI _lastLoginTimeTMP;

    // 클랜 장만 보이는 UI
    [SerializeField] private ExButton _delegationManagerBtn;       // 매니저 위임 버튼
    [SerializeField] private ExButton _delegationMemberBtn;       // 멤버 강등 버튼
    [SerializeField] private GameObject _setGradeObj;           // 매니저, 일반맴버 변경 버튼 상위 오브젝트
    [SerializeField] private ExButton _delegationMasterBtn;       // 마스터 위임 버튼
    
    [SerializeField] private ExButton _kickOutBtn;            // 강퇴 버튼

    private ClanConfig _clanConfig;
    private Action<string, int, bool> _callback;


    public override void Close(bool needCached = true)
    {
        base.Close(needCached);
    }
    public override void Init()
    {
        base.Init();
    }
    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        if (optionDict != null)
        {
            if (optionDict.TryGetValue(UIOption.Data, out var userConfig))
            {
                _clanConfig = (ClanConfig)userConfig;
            }

            if (optionDict.TryGetValue(UIOption.Callback, out var callback))
                _callback = (Action<string, int, bool>)callback;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        _icon.SetSprite(_clanConfig.Icon);
        _frame.SetSprite(_clanConfig.Frame);
        _nicnameTMP.text = _clanConfig.Name;
        _gradeTMP.ToTableText(GameInfoManager.Instance.SetMemberGrade(_clanConfig.Grade));
        _levelTMP.text = "Lv."+_clanConfig.Level.ToString();
        _totalPowrtTMP.text = _clanConfig.TotalPower.ToString();
        _totalPointTMP.text =_clanConfig.Point.ToString();

        var weekPoint = _clanConfig.DailyPoint + _clanConfig.WeekPoint;
        _weekPointTMP.text =weekPoint.ToString();
        _lastLoginTimeTMP.text = _clanConfig.LastLoginTime.ToString();

        //int grade = -1;
        //if (GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig != null)
        //{
        //    if (GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.ClanId.Equals(_clanConfig.ClanId))
        //    {
        //        var myClanConfig = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig;
        //        Debug.Log("@@ClanMemberInfo => "+myClanConfig.Grade);
        //        grade = myClanConfig.Grade;
        //    }
        //}
        SetButtonsStateByGrade(_clanConfig.Grade);
    }

    private void SetButtonsStateByGrade(int grade)
    {
        switch (grade)
        {
            case 0:             // 클랜 마스터
                {
                    _delegationMasterBtn.gameObject.SetActive(false);
                    _delegationManagerBtn.gameObject.SetActive(false);
                    _delegationMemberBtn.gameObject.SetActive(false);
                    _kickOutBtn.gameObject.SetActive(false);
                }
                break;
            case 1:             // 매니저
                {
                    _delegationMasterBtn.gameObject.SetActive(true);
                    _delegationManagerBtn.gameObject.SetActive(false);
                    _delegationMemberBtn.gameObject.SetActive(true);
                    _kickOutBtn.gameObject.SetActive(true);
                }
                break;
            case 2:             // 일반 회원
                {
                    _delegationMasterBtn.gameObject.SetActive(true);
                    _delegationManagerBtn.gameObject.SetActive(true);
                    _delegationMemberBtn.gameObject.SetActive(false);
                    _kickOutBtn.gameObject.SetActive(true);
                }
                break;
            //default:            // 기본은 안보이도록
            //    _delegationManagerBtn.gameObject.SetActive(false);
            //    //_delegationMasterBtn.gameObject.SetActive(false);
            //    SetMemberGradeButton(false);
            //    _kickOutBtn.gameObject.SetActive(false);
            //    break;
        }
    }
    //마스터 0 매니저 1 멤버 2
    public void OnClickMemberSetGrade(int grade)
    {
        RestApiManager.Instance.RequestClanSetGrade(_clanConfig.PublicKey, _clanConfig.ClanId, grade,
            (response) =>
            {
                RestApiManager.Instance.CheckIsEmptyResult(response, () =>
                {
                    UpdateMemberInfo(_clanConfig.PublicKey, response);
                });
            });
    }

    // 마스터 위임 버튼
    public void OnClickDelifationMaster()
    {
        Action onClickOk = () => {
            RestApiManager.Instance.RequestClanSetGrade(_clanConfig.PublicKey, _clanConfig.ClanId, 0,
               (response) => {
                   RestApiManager.Instance.CheckIsEmptyResult(response, () =>
                   {
                       UpdateMemberInfo(_clanConfig.PublicKey, response);
                   });
               });

            string str = LocalizeManager.Instance.GetString("str_clan_master_grant_msg_02", _clanConfig.Name);     // {0}에게 마스터를 위임하였습니다.
            UIManager.Instance.ShowToastMessage(str);
            Close();
        };

        string message = LocalizeManager.Instance.GetString("str_clan_master_grant_msg_01", _clanConfig.Name);    //{0}에게 마스터를 위임하겠습니까? \n 마스터 위임 시 마스터 권한이 사라집니다.
        UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK_CANCEL, message: message, onClickOK: onClickOk);
    }

    // 매니저 위임 버튼

    public void OnClickDelifationManager()
    {
        // 매니저 수 체크.(임시로 3명)
        var count = UIManager.Instance.GetUI<UIClanInfo>().CountGradeMembers(1);
        var maxManagerCount = TableManager.Instance.Define_Table["ds_clan_max_manager"].Opt_01_Int;
        if (count >= maxManagerCount)
        {
            UIManager.Instance.ShowToastMessage("str_clan_manager_grant_deny_msg_01"); //클랜 매니저가 5명이여서 추가 위임이 불가합니다.
            Debug.Log("<color=#ff00b4>매니저 수("+count+")</color>");
            return;
        }
        Action onClickOk = () =>
        {
            RestApiManager.Instance.RequestClanSetGrade(_clanConfig.PublicKey, _clanConfig.ClanId, 1,
            (response) => {
                RestApiManager.Instance.CheckIsEmptyResult(response, () =>
                {
                    UpdateMemberInfo(_clanConfig.PublicKey, response);
                });
            });

            string str = LocalizeManager.Instance.GetString("str_clan_manager_grant_msg_02", _clanConfig.Name);     // {0}에게 매니저를 위임하였습니다.
            UIManager.Instance.ShowToastMessage(str);
            Close();
        };

        string message = LocalizeManager.Instance.GetString("str_clan_manager_grant_msg_01", _clanConfig.Name);    //{0}에게 매니저를 위임하겠습니까? \n 매니저는 최대 5명까지 위임 가능합니다.
        UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK_CANCEL, message: message, onClickOK: onClickOk);
    }

    // 일반 멤버로 강등 버튼
    public void OnClickDelifationMember()
    {
        Action onClickOk = () => {
            RestApiManager.Instance.RequestClanSetGrade(_clanConfig.PublicKey, _clanConfig.ClanId, 2,
               (response) =>  {
                RestApiManager.Instance.CheckIsEmptyResult(response, () =>
                {
                    UpdateMemberInfo(_clanConfig.PublicKey, response);
                });
            });

            string str = LocalizeManager.Instance.GetString("str_clan_member_grant_msg_02", _clanConfig.Name);     // {0}을 멤버로 변경하였습니다.
            UIManager.Instance.ShowToastMessage(str);
            Close();
        };

        string message = LocalizeManager.Instance.GetString("str_clan_member_grant_msg_01", _clanConfig.Name);    //{0}를 멤버로 변경하시겠습니까? 
        UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK_CANCEL, message: message, onClickOK: onClickOk);
    }

    private void UpdateMemberInfo(string publicKey, JToken response)
    {
       
            var targetinfo = response["result"]["targetinfo"];
            var targetGrage = targetinfo["M"]["config"]["M"]["G"].N_Int();

            var claninfo = response["result"]["claninfo"];

            if (claninfo != null)
            {
                var grade = claninfo["M"]["config"]["M"]["G"].N_Int();

                _callback?.Invoke(publicKey, targetGrage, false);
                _callback?.Invoke(RestApiManager.Instance.GetPublicKey(), grade, true);
            }
            else
                _callback?.Invoke(publicKey, targetGrage, true);
    }

    // 강퇴 버튼
    public void OnClickKickOut()
    {
        Action onClickOk = () => {
            RestApiManager.Instance.RequestClanOut(_clanConfig.ClanId, _clanConfig.PublicKey, (response) => {

                var clanData = new ClanData(response["result"]);
                var str = LocalizeManager.Instance.GetString("str_clan_fire_msg_02", _clanConfig.Name);     //{0}을 추방하였습니다.
                UIManager.Instance.ShowToastMessage(str);
                ClanConfig outConfig = new ClanConfig(response["result"]["claninfos"]["L"][0]["M"]["config"]["M"]);
                GameInfoManager.Instance.ClanInfo.UpdateClanMembersOut(outConfig);
                var clan = GameInfoManager.Instance.ClanInfo.MyClanData;
                var uiClanInfo = UIManager.Instance.GetUI<UIClanInfo>();
                if (uiClanInfo != null && uiClanInfo.gameObject.activeInHierarchy)
                    uiClanInfo.UpdateUI(clan);
                Close();
            });
        };

        string message = LocalizeManager.Instance.GetString("str_clan_fire_msg_01", _clanConfig.Name);    //{0}을 추방하시겠습니까? , {0}=>클랜원의 이름
        UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK_CANCEL, message: message, onClickOK: onClickOk);

    }
}
