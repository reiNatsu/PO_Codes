using Consts;
using LIFULSE.Manager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.UI;

public class UIClanInfo : UIBase
{
    [Header("Clan Info Object")]
    [SerializeField] private UIClanInfoObject _uiClanInfoObject;

    [Header("Clan Member List UI")]
    [SerializeField] private RecycleScroll _scroll;
    [SerializeField] private GameObject _noMemberObj;

    [Header("Management UI")]
    [SerializeField] private GameObject _managementObj;
    [SerializeField] private GameObject _chatButton;
    [SerializeField] private GameObject _editClanButton;
    [SerializeField] private GameObject _editClanSkillButton;
    [SerializeField] private GameObject _joinListButton;
    [SerializeField] private GameObject _helperButton;
    [SerializeField] private GameObject _deleteClanButton;

    // 멤버 리스트
    private List<ClanMemberCell> _cells;
    private List<ClanMemberCellData> _cellDatas;

    private ClanData _clanData;
    private UserClan _userclanData;
    private bool _isOkJoin = false;
    private bool _isCheckAttendance = false;
    private bool _isConfirm = false; //자동 위임 팝업 확인 했는 지
    private bool _isChangeMaster = false;
    //private int _tabIndex;
    private ClanConfig _myConfig = new ClanConfig();
    public UIClanInfoObject UIClanInfoObject { get { return _uiClanInfoObject; } }

    public override void Close(bool needCached = true)
    {
        base.Close(needCached);
    }
    public override void Refresh()
    {
        base.Refresh();
        //_clanData = GameInfoManager.Instance.ClanInfo.MyClanData;
        //UpdateUI(_clanData);

        RestApiManager.Instance.RequestClanGetClanData(_clanData.Id, null, false, false, (response) =>
        {
            GameInfoManager.Instance.ClanInfo.SetMyClanData(response["result"]);
            _clanData = GameInfoManager.Instance.ClanInfo.MyClanData;
            if (GameInfoManager.Instance.IsKickedClan())
            {
                UIManager.Instance.ShowToastMessage("str_clan_fire_msg_03");            // 클랜에서 추방당했습니다.
                UIManager.Instance.CloseAllUI();
                UIManager.Instance.Show<UILobby>();
            }
            else
                UpdateUI(_clanData);
        });
    }

    public void UpdateUserClanInfo()
    {
        // _myConfig = GameInfoManager.Instance.GetMyClanCongfig(_clanData.Configs);
        _myConfig = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig;
    }
    public override void Init()
    {
        base.Init();

        _scroll.Init();
        _cells = _scroll.GetCellToList<ClanMemberCell>();
        _cellDatas = new List<ClanMemberCellData>();
    }

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        if (optionDict != null)
        {
            if (optionDict.TryGetValue(UIOption.Data, out var clanData))
            {
                _clanData = (ClanData)clanData;
            }

            // TODO :임시, 나중에 유저 clanData에 클랜 데이터를 넣으면 변경
            if (optionDict.TryGetValue(UIOption.Data2, out var userClanData))
            {
                _userclanData = (UserClan)userClanData;
            }
            // 로비에서 눌러서 들어왔을 경우 + 클랜에 가입 되어있을 경우에만 출석 팝업 보여주기위해
            if (optionDict.TryGetValue(UIOption.Bool, out var checkAttendance))
            {
                _isCheckAttendance = (bool)checkAttendance;
            }

            if (_isCheckAttendance)
                CheckAttence();

            //자동 위임 팝업 확인 했는 지
            if (optionDict.TryGetValue(UIOption.Bool2, out var isConfirm))
            {
                _isConfirm = (bool)isConfirm;
                //if (_isConfirm && !GameInfoManager.Instance.IsCheckConfirm)
                //    ShowNewMaster();
                //if (_isConfirm && !GameInfoManager.Instance.IsCheckConfirm)
                //    _isChangeMaster = true;
                //else
                //    _isChangeMaster = false;
                //ShowNewMaster();
            }
        }
       // _tabIndex = -1;

        //if (_isCheckAttendance)
        //    CheckAttence();
        // OnClickSelectInfo(0);
        _myConfig = GameInfoManager.Instance.GetMyClanCongfig(_clanData.Configs);
        UpdateUI(_clanData);
    }

    //자동 위임 결과 팝업
    private void ShowNewMaster()
    {
        //Debug.Log("ShowNewMaster 이전 클랜 마스터 " + _clanData.PrevMasterName);
        //Debug.Log("ShowNewMaster 지금 클랜 마스터 " + _clanData.Configs.Where(x=>x.Grade == 0).FirstOrDefault().Name);
        Debug.Log("ShowNewMaster 나 " + RestApiManager.Instance.GetPublicKey());

        string prevMaster = _clanData.PrevMasterName;
        if (string.IsNullOrEmpty(_clanData.PrevMasterName))
            prevMaster = GameInfoManager.Instance.PrevMasterName;

        StringBuilder sb = new StringBuilder();
        sb.Append(LocalizeManager.Instance.GetString("str_clan_auto_commission_msg_01"));
        sb.AppendLine();
        sb.Append(LocalizeManager.Instance.GetString("str_clan_before_master", prevMaster));
        sb.AppendLine();
        sb.Append(LocalizeManager.Instance.GetString("str_clan_after_master", _clanData.Configs.Where(x => x.Grade == 0).FirstOrDefault().Name));

        Action onClickOk = () => {
            GameInfoManager.Instance.IsCheckConfirm = true;
            GameInfoManager.Instance.PrevMasterName = null;
            RestApiManager.Instance.RequestClanGetClanData(_myConfig.ClanId, null, false, false, (response) =>
            {
                // TODOCLAN : 이름 중복, 가입하려는 승인됨, 등급 변경
                RestApiManager.Instance.CheckIsEmptyResult(/*ClanState.NotFound,*/response, () => {
                    // ShowClan(response);
                    GameInfoManager.Instance.ClanInfo.SetMyClanData(response["result"]);
                    _clanData = GameInfoManager.Instance.ClanInfo.MyClanData;
                    _myConfig = GameInfoManager.Instance.GetMyClanCongfig(_clanData.Configs);
                    UpdateUI(_clanData);
                });
            });
        };

        UIManager.Instance.ShowAlert(AlerType.Middle, PopupButtonType.OK_CANCEL,message: sb.ToString()
            , onClickOK: onClickOk, onClickCancel: onClickOk, onClickClosed: onClickOk, onClickExit:onClickOk);

        for (int i = 0; i < _clanData.ConfirmMasterList.Count; i++)
        {
            Debug.Log("ShowNewMaster 리스트 " + _clanData.ConfirmMasterList[i]);
        }
    }

    public void UpdateClanData(ClanData clandata)
    {
        _clanData =clandata;
    }
    public void UpdateUI(ClanData data)
    {
        _uiClanInfoObject.InitData(data);
        _clanData = data;

        //if (!string.IsNullOrEmpty(GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.ClanId))
        //    SetMemberGradeUI();
     
        SetMemberGradeUI();

        if (_clanData.Configs != null && _clanData.Configs.Count > 0)
        {
            _noMemberObj.SetActive(false);
            _scroll.gameObject.SetActive(true);
            UpdateClanMemberList(_clanData.Configs);
        }
        else
        {
            _noMemberObj.SetActive(true);
            _scroll.gameObject.SetActive(false);
        }
    }

    public void UpdateClanMemberList(List<ClanConfig> configs)
    {
        _cellDatas.Clear();

        //for (int n = 0; n < _clanData.MemberList.Count; n++)
        var myClanConfig = GameInfoManager.Instance.GetMyClanCongfig();
        for (int n = 0; n < configs.Count; n++)
        {
            //var member = _clanData.MemberList[n];
            var member = configs[n];
            _cellDatas.Add(new ClanMemberCellData(member, myClanConfig, true));
        }
        for (int i = 0; i < _cells.Count; i++)
        {
            _cells[i].SetCellDatas(_cellDatas, UpdateCell);
        }
        // _scroll.ActivateCells(_clanData.MemberList.Count);
        _scroll.ActivateCells(configs.Count);

        _uiClanInfoObject.SetClanMember(configs.Count);
    }

    public int CountGradeMembers(int grade)
    {
        List<ClanConfig> typemembers = new List<ClanConfig>();
        var members = _clanData.Configs;
        for (int n = 0; n< members.Count; n++)
        {
            if (members[n].Grade == grade)
                typemembers.Add(members[n]);
        }

        return typemembers.Count;
    }

    public void UpdateCell()
    {
        _scroll.ActivateCells(_cellDatas.Count);
    }

    public void SetMemberGradeUI()
    {
        // 임시, 에디터 화면은 길마일 경우만 보이도록
        _managementObj.SetActive(true);
        //var grade = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.Grade;
        var grade = GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.Grade;

        _chatButton.gameObject.SetActive(false);           // 채팅 버튼
        _helperButton.gameObject.SetActive(true);       // 조력자 버튼

      switch (_myConfig.Grade)
        {
            case 0:             // 클랜 마스터
                {
                    _editClanButton.gameObject.SetActive(true);             // 클랜 정보수정 버튼
                    _editClanSkillButton.gameObject.SetActive(true);       // 클랜 스킬 수정 버튼
                    _joinListButton.gameObject.SetActive(true);               // 신청 관리 버튼
                    _deleteClanButton.gameObject.SetActive(true);           // 클랜 해산 버튼
                }
                break;
            case 1:             // 매니저
                {
                    _editClanButton.gameObject.SetActive(false);
                    _editClanSkillButton.gameObject.SetActive(false);       
                   _joinListButton.gameObject.SetActive(true);
                    _deleteClanButton.gameObject.SetActive(false);
                }
                break;
            case 2:             // 일반 회원
                {
                    _editClanButton.gameObject.SetActive(false);
                    _editClanSkillButton.gameObject.SetActive(false);
                    _joinListButton.gameObject.SetActive(false);
                    _deleteClanButton.gameObject.SetActive(false);
                }
                break;
            default:            // 기본은 안보이도록
                _managementObj.SetActive(false);
                break;
        }
    }

    private void CheckAttence(bool isChangeMaster = false)
    {
        if (!GameInfoManager.Instance.ClanInfo.IsJoinClan())
            return;

        var attendanceTime = GameInfoManager.Instance.ClanInfo.GetUserClanConfig().AttendanceTime;
        var dateTime = new DateTime(attendanceTime);

        if (dateTime < GameInfoManager.Instance.DailyResetTime)
        {
            RestApiManager.Instance.RequestClanAttendance(() => {
                RedDotManager.Instance.SetActiveRedDot("reddot_clan_attendance", 0, 0, false);
            }
            , () =>
            {
                if (_isConfirm && !GameInfoManager.Instance.IsCheckConfirm)
                    ShowNewMaster();
            }
            );
            RedDotManager.Instance.SetActiveRedDot("reddot_clan_attendance", 0, 0, dateTime < GameInfoManager.Instance.DailyResetTime);
        }
        else
            Debug.Log("출석 완료");
    }

    public void PostJoinClan()
    {

        if (GameInfoManager.Instance.JoinClans.Count < 5 && _isOkJoin)
        {
            GameInfoManager.Instance.AddJoinClan(_clanData);
            Close();
        }
        else
            Close();

        if (GameInfoManager.Instance.JoinClans.Count < 5 && !_isOkJoin)
        {
            GameInfoManager.Instance.CancleJoinClan(_clanData);
            Close();
        }
        else
            Close();
    }

    #region[클랜 관리자 버튼]

    // 클랜 관리 버튼
    public void OnClickEditInfo()
    {
        if (!GameInfoManager.Instance.CheckEnableGrade(0))
        {
            UpdateUserClanInfo();
            SetMemberGradeUI();
            return;
        }

        UIManager.Instance.Show<UIPopupEditClan>(Utils.GetUIOption(
            UIOption.Bool, true,
            UIOption.Data, _clanData
            ));
    }
    // 스킬관리 버튼
    public void OnClickEditClanSkill()
    {
        if (!GameInfoManager.Instance.CheckEnableGrade(0))
        {
            UpdateUserClanInfo();
            SetMemberGradeUI();
            return;
        }

        UIManager.Instance.Show<UIPopupEditClan>(Utils.GetUIOption(
           UIOption.Bool, false,
           UIOption.Data, _clanData
           ));
    }
    // 클랜 신청 관리 버튼
    public void OnClickMemberJoinList()
    {
        RestApiManager.Instance.RequestClanGetMemberJoinList(_clanData.Id, (response) =>
        {
            //// 내 직급이 매니저일 경우 : 매니저 권한이 있는지 확인
            //if (!GameInfoManager.Instance.CheckEnableGrade(1))
            //    return;
            RestApiManager.Instance.CheckIsEmptyResult(response,() =>
            {
              
                var configs = new List<ClanConfig>();
                var clanInfosToken = response["result"]["claninfos"];
                if (clanInfosToken == null)
                    return;                 
                var infos = clanInfosToken["L"].ToArray();

                for (int i = 0; i < infos.Length; i++)
                {
                    var config = new ClanConfig(infos[i]["M"]["config"]["M"]);

                    configs.Add(config);
                }

                Debug.Log("<color=#4cd311> 클랜 신청 관리 버튼 클릭</color>");
                UIManager.Instance.Show<UIPopupClanManagement>(
                    Utils.GetUIOption(
                    UIOption.List, configs,
                    UIOption.EnumType, CLAN_MANAGEMENT_TYPE.joinlist,
                    UIOption.Id, _clanData.Id)
                    );
            },
            () => {
                UIManager.Instance.Show<UIPopupClanManagement>(
                       Utils.GetUIOption(
                       UIOption.List, null,
                       UIOption.EnumType, CLAN_MANAGEMENT_TYPE.joinlist,
                       UIOption.Id, _clanData.Id)
                       );
            });
        });
    }

    // 조력자 버튼
    public void OnClickHelper()
    {
        Debug.Log("<color=#4cd311> 클랜 조력자 버튼 클릭</color>");
        UIManager.Instance.Show<UIHelperCharacter>();
    }

    // 채팅 버튼
    public void OnClickChat()
    {
        Debug.Log("<color=#4cd311> 클랜 채팅 버튼 클릭</color>");
        UIManager.Instance.Show<UIPopupClanManagement>(
           Utils.GetUIOption(
           UIOption.Data, _clanData,
           UIOption.EnumType, CLAN_MANAGEMENT_TYPE.chat)
           );
    }

    // 클랜 삭제 버튼
    public void OnClickClanDelete()
    {
        Debug.Log("<color=#4cd311> 클랜 삭제 버튼 클릭</color>");

        var title = LocalizeManager.Instance.GetString("str_ui_clan_button_05");       //클랜 해산 

        if (_clanData.Configs.Count > 1)
        {
            //UIManager.Instance.ShowToastMessage("str_clan_delet_deny_01");          // 클랜원이 존재하여 클랜 해산을 진행 할 수 없습니다.
            //return;
            var message = LocalizeManager.Instance.GetString("str_ui_clan_out_error_01");      
            //클랜원이 존재하여 클랜 해산을 진행할 수 없습니다. 클랜장은 다른 클랜원에게 클랜장을 위임하기 전까지 클랜에서 탈퇴할 수 없습니다.
            UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK, title: title, message: message);
        }
        else
        {
            if (GameInfoManager.Instance.GetMyClanCongfig(_clanData.Configs).Grade != 0)
            {
                UIManager.Instance.ShowToastMessage("str_ui_data_error_01");
                OnClickClose();
                UIManager.Instance.Show<UILobby>();
            }

            Action onClickOk = () => {
                RestApiManager.Instance.RequestClanDelete(_clanData.Id, () => {
                    UIManager.Instance.ShowToastMessage("str_clan_delet_msg_02");       //클랜이 해산되었습니다.
                    UIManager.Instance.CloseAllUI();
                    UIManager.Instance.Show<UILobby>();
                });
            };

            var message = LocalizeManager.Instance.GetString("str_clan_delet_msg_01");      //클랜 해산을 진행하시겠습니까?
            UIManager.Instance.ShowAlert(AlerType.Small, PopupButtonType.OK_CANCEL,title: title, message: message, onClickOK: onClickOk);
        }
       // if (GameInfoManager.Instance.ClanInfo.UserClanData.ClanConfig.Grade != 0)
       
    }
    #endregion

}
