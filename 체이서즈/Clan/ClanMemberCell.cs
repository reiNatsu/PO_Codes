using Consts;
using LIFULSE.Manager;
using Newtonsoft.Json.Linq;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

public class ClanMemberCellData
{
    [SerializeField] public ClanConfig ClanConfig;
    [SerializeField] public bool IsInfo;
    [SerializeField] public ClanConfig MyClanConfig;
    // 임시로 userclan 받아오기

    public ClanMemberCellData(ClanConfig clanconfig, ClanConfig myClanConfig, bool isInfo)
    {
        ClanConfig = clanconfig;
        MyClanConfig = myClanConfig;
        IsInfo =isInfo;
    }
}

public class ClanMemberCell : ScrollCell
{
    [SerializeField] private ExImage _icon;
    [SerializeField] private ExImage _frame = null;
    [SerializeField] private ExTMPUI _gradeTMP;
    [SerializeField] private ExTMPUI _levelTMP;
    [SerializeField] private ExTMPUI _nicknameTMP;
    [SerializeField] private ExTMPUI _totalpowerTMP;
    [SerializeField] private ExTMPUI _totalPointTMP;
    [SerializeField] private ExTMPUI _weekPointTMP;
    [SerializeField] private ExTMPUI _lastloginTMP;
    [SerializeField] private ExImage _nationImg;

    [SerializeField] private GameObject _totalPointObj;         // 총 공헌도 : 정보 팝업인 경우 미노출
    [SerializeField] private GameObject _weekPointObj;          // 주간 공헌도 : 정보 팝업인 경우 미노출
    [SerializeField] private GameObject _approveObj;
    [SerializeField] private GameObject _memberInfoObj;         // 멤버 정보 보는 돋보기 모양 버튼
   // [SerializeField] private ExButton _memberCellBtn;
    [SerializeField] private ExButton _leaveBtn;



    private List<ClanMemberCellData> _cellDatas;
    private Action _updateCellsCallback; //특정 상황에서 멤버 리스트를 초기화 해야할 때
    private Action<bool, JToken ,Action> _updateJoinCellsCallback; // 가입 승인, 거절 상황에서 멤버 리스트 업데이트 할 때.

    private string _memberKey;
    private ClanConfig _member;
    private ClanConfig _myClanComfig;

    public void SetCellDatas(List<ClanMemberCellData> datas, Action updateCellsCallback)
    {
        _cellDatas = datas;
        _updateCellsCallback = updateCellsCallback;
    }

    // UIpopupClanInfo에서 사용 됨
    public void SetCellDatas(List<ClanMemberCellData> datas, Action<bool, JToken, Action> updateCellsCallback)
    {
        _cellDatas = datas;
        _updateJoinCellsCallback = updateCellsCallback;
    }
    public override void UpdateCellData(int dataIndex)
    {
        DataIndex = dataIndex;
        _member = _cellDatas[DataIndex].ClanConfig;

        UpdateUI();
    }

    public void UpdateUI()
    {
        SetCellType();

        // 해당 셀의 claninfo publickey 와 내 publickey가 같으면 탈퇴 버튼 보이기

        SetMemberBasicInfo();
        _gradeTMP.ToTableText(GameInfoManager.Instance.SetMemberGrade(_member.Grade));
    }

    private void SetCellType()
    {
        
        if (!_cellDatas[DataIndex].IsInfo)          // 클랜 신청 리스트에서 보이는 경우
        {
            _memberInfoObj.gameObject.SetActive(false);
            //_memberCellBtn.gameObject.SetActive(false);
            _approveObj.SetActive(true);
            _totalPointObj.SetActive(false);
            _weekPointObj.SetActive(false);
        }
        else       // 클랜 정보 화면에서 보이는 경우
        {
            //_memberInfoObj.SetActive(GameInfoManager.Instance.GetMyClanCongfig().Grade == 0);
            if (_cellDatas[DataIndex].MyClanConfig != null)
                _memberInfoObj.SetActive(_cellDatas[DataIndex].MyClanConfig.Grade == 0);
            else
                _memberInfoObj.SetActive(_cellDatas[DataIndex].ClanConfig.Grade == 0);
            //_memberCellBtn.gameObject.SetActive(true);
            var uiClaninfo = UIManager.Instance.GetUI<UIClanInfo>();
            _totalPointObj.SetActive(uiClaninfo != null);
            _weekPointObj.SetActive(uiClaninfo != null);

            _approveObj.SetActive(false);

            var myKey = RestApiManager.Instance.GetPublicKey();
            if (myKey.Equals(_member.PublicKey) && _member.Grade != 0)
                _leaveBtn.gameObject.SetActive(true);
            else
                _leaveBtn.gameObject.SetActive(false);
        }
    }

    //멤버 등급 변경
    private void UpdateGrade(string publicKey, int grade, bool isUpdate)
    {
        var cell = _cellDatas.FirstOrDefault(x => x.ClanConfig.PublicKey.Equals(publicKey));

        if (cell == null)
            return;

        cell.ClanConfig.SetGrade(grade);

        if(isUpdate)
            _updateCellsCallback?.Invoke();
    }

    // 멤버 등급 변경 후

    public void SetMemberBasicInfo()
    {
        _nicknameTMP.text = _member.Name;
        _levelTMP.ToTableText("str_ui_char_level_001", _member.Level);          //Lv.{0}

        if (string.IsNullOrEmpty(_member.Icon))
            _icon.SetSprite("IC_MN_dongjasam_01_result");
        else
            _icon.SetSprite(_member.Icon);

        if (_frame == null || string.IsNullOrEmpty(_member.Frame))
            _frame.gameObject.SetActive(false);
        else
        {
            _frame.gameObject.SetActive(true);
            _frame.SetSprite(_member.Frame);
        }

        string nationicon = null;
        if (string.IsNullOrEmpty(_member.Nation))
            nationicon = "UI_Intro_Flag_KR";
        else
            nationicon = TableManager.Instance.Nation_Table[_member.Nation].Nation_Img;
        _nationImg.SetSprite(nationicon); 

        SetTotalPowerUI();
        SetTotlaPointUI();
        SetWeekPointUI();
   
        bool isMy = _member.PublicKey.Equals(RestApiManager.Instance.GetPublicKey());
        _lastloginTMP.text = GameInfoManager.Instance.GetLastLoginTime(isMy, _member.LastLoginTime);
    }

    public void SetTotalPowerUI()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(LocalizeManager.Instance.GetString("str_user_power_default"));    // 종합 전투력
        sb.Append(_member.TotalPower);

        _totalpowerTMP.text = sb.ToString();
        //_totalpowerTMP.ToTableText("str_user_power_default", _member.TotalPower);
    }
    public void SetTotlaPointUI()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(LocalizeManager.Instance.GetString("str_clan_user_contribution_02"));         // 전체 공헌도
        sb.Append(_member.Point);

        _totalPointTMP.text = sb.ToString();

        //_totalPointTMP.ToTableText("str_clan_user_contribution_02", _member.Point);
    }
    public void SetWeekPointUI()
    {
        var weeklyPoint = _member.WeekPoint + _member.DailyPoint;
        StringBuilder sb = new StringBuilder();
        sb.Append(LocalizeManager.Instance.GetString("str_clan_user_contribution_01"));     // 주간 공헌도
        sb.Append(weeklyPoint);

        _weekPointTMP.text = sb.ToString();

        //_totalPointTMP.ToTableText("str_clan_user_contribution_02", _member.Point);
    }
    public void OnClickMemeberCell()
    {
        // 클랜장이 멤버 cell을 클릭 할 경우 멤버 정보 팝업이 보여지고
        // 위임, 강퇴가 가능 함. 
        // 내가 클랜에 가입 한 상태일 때만 정보 팝업 보여주기
        if (GameInfoManager.Instance.ClanInfo.UserClanData != null 
            && GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.ClanId.Equals(_member.ClanId))
        {
            if (_cellDatas[DataIndex].MyClanConfig.Grade == 0)
            {
                Action<string, int, bool> callback = UpdateGrade;
                UIManager.Instance.Show<UIPopupClanMemberInfo>(Utils.GetUIOption(UIOption.Data, _member, UIOption.Callback, callback));
            }
        }
        else
            Debug.Log("<color=#ff00b4>가입한 클랜이 아니라서 유저 정보 팝업은 보여주지 않음</color>");
    }

    // 클랜 탈퇴 버튼(클랜원 본인이 클릭하는 버튼)
    public void OnClickLeaveClan()
    {
        // 탈퇴 하겠는지 물어보고
        Action onClick = () => {
            // TODOCLAN : 변수로 넣어야 하는 name값 확인 필요
            RestApiManager.Instance.RequestClanLeave(_member.ClanId, (response) => {
                RestApiManager.Instance.CheckIsEmptyResult(response, () =>
                {
                    UIManager.Instance.ShowToastMessage("str_clan_out_msg_02");         // 클랜을 탈퇴하였습니다.

                    UIManager.Instance.CloseAllUI();
                    UIManager.Instance.Show<UILobby>();
                });
            });
        };

        string message = LocalizeManager.Instance.GetString("str_clan_out_msg_01");        // 클랜에서 탈퇴하시겠습니까? \n 클랜 재가입은 24시간 이후에 가능합니다.

        UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK_CANCEL, message: message, onClickOK:onClick);
    }

    // 가입 승인
    public void OnClickJoinAccept()
    {
        //if (!GameInfoManager.Instance.CheckIsClanFullMember(GameInfoManager.Instance.ClanInfo.MyClanData, true))
        //    return;
        if (GameInfoManager.Instance.ClanInfo.MyClanData.Configs.Count  >= GameInfoManager.Instance.ClanInfo.MyClanData.MaxMemberCount)
        {
            UIManager.Instance.ShowToastMessage("str_clan_agree_deny_msg_01");           //클랜원이 최대여서 더 이상 승인이 불가합니다.
            return;
        }

        Action acceptAction = () => {
            string message = LocalizeManager.Instance.GetString("str_clan_join_apply", _member.Name);           //{0}의 가입을 승인하였습니다
            //string message = string.Format("{0}의 가입을 승인하였습니다", _member.Name);           //{0}의 가입을 승인하였습니다
            UIManager.Instance.ShowToastMessage(message);
        };
       
        var clanId = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.ClanId;
        RestApiManager.Instance.RequestClanJoinAccept(clanId, _member.PublicKey, (response)=> {

            if (response["result"].Type == JTokenType.Object)
            {
                //string message = LocalizeManager.Instance.GetString("str_clan_join_apply", _member.Name);           //{0}의 가입을 승인하였습니다
                //UIManager.Instance.ShowToastMessage(message);
                ClanConfig newConfig = new ClanConfig(response["result"]["claninfos"]["L"][0]["M"]["config"]["M"]);
                GameInfoManager.Instance.ClanInfo.UpdateClanMembers(newConfig);
                _updateJoinCellsCallback?.Invoke(true, response, acceptAction);
                return;
            }
            UIManager.Instance.ShowToastMessage("str_clan_agree_deny_msg_02");          //해당 클랜원은 이미 다른 클랜에 가입하였습니다
            _updateJoinCellsCallback?.Invoke(false, response, null);
        });
    }

    // 가입 거절
    public void OnClickJoinReject()
    {
        Action rejectAction = () => {
            Debug.Log("<color=#4cd311> 가입 거절 완료!</color>");
            string message = LocalizeManager.Instance.GetString("str_clan_join_refuse", _member.Name);           //{0}의 가입을 거절하였습니다
            //string message =string.Format("{0}의 가입을 거절하였습니다", _member.Name);           //{0}의 가입을 거절하였습니다
            UIManager.Instance.ShowToastMessage(message);
        };

        var clanId = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.ClanId;
        RestApiManager.Instance.RequestClanJoinReject(clanId, _member.PublicKey, (response) => {
            if (response["result"].Type == JTokenType.Object)
            {
                string message = LocalizeManager.Instance.GetString("str_clan_join_refuse", _member.Name);           //{0}의 가입을 거절하였습니다
                UIManager.Instance.ShowToastMessage(message);
                _updateJoinCellsCallback?.Invoke(false, response, rejectAction);
                return;
            }
            UIManager.Instance.ShowToastMessage("str_clan_agree_deny_msg_02");          //해당 클랜원은 이미 다른 클랜에 가입하였습니다
            _updateJoinCellsCallback?.Invoke(false, response, null);
           
        });
    }

}
    