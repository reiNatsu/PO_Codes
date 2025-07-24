using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIEditClanInfo : MonoBehaviour
{
    [Header("Clan Info UI")]
    [SerializeField] private GameObject _noIcon;
    [SerializeField] private ExImage _icon;
    [SerializeField] private ExTMPUI _nameTMP;
    [SerializeField] private ExTMPUI _noticeTMP;
    [SerializeField] private CheckBadInputText _checkBadInputText;
    [SerializeField] private GameObject _approveOnObj;          // 길드 자동가입 체크 v 표시 오브젝트(on :  자동 가입 ok, off : 가입 승인 )
    //[SerializeField] private TMP_InputField _inputTMP;
   
    private List<int> _approvetypes = new List<int>();

    private ClanData _data;

    private string _currentIcon;
    private string _currentName;
    private string _currentNotice;
    private int _currentConditionValue;

    private string _changeIcon;

    private int _tabIndex;
    private UIPopupEditClan _uiPopupEditClan;

    private bool _isApproval = false;           // 가입 승인 여부 확인(false = 자동가입, true = 가입승인)
    public void InitInfoData(ClanData data)
    {
        var uieditClan = UIManager.Instance.GetUI<UIPopupEditClan>();
        if (uieditClan != null && uieditClan.gameObject.activeInHierarchy)
            _uiPopupEditClan = uieditClan;
        else
            _uiPopupEditClan = null;

        _data = data;
        _currentIcon = _data.Icon;
        _currentName = _data.Name;
        _currentNotice = _data.Notice;
        _currentConditionValue = _data.ConditionValue;
        if (_data.JoinType == 0)
            _isApproval = false;
        else
            _isApproval = true;
        //SetInfoData();

        SetClanIcon(_currentIcon);
        SetClanName(_currentName);
        SetClanNotice(_currentNotice);
        SetIsApproval(!_isApproval);
        Debug.Log("<color=#4cd311> _isApproval : <b> " +_isApproval+ " </b></color>");
        
        //if (_data.JoinType == 0)

        //else
        //    SetIsApproval(false);
    }

    // 클랜 아이콘 UI
    public void SetClanIcon(string iconName)
    {
        if (string.IsNullOrEmpty(iconName))
        {
            _noIcon.SetActive(true);
            _icon.gameObject.SetActive(false);
        }
        else
        {
            _noIcon.SetActive(false);
            _icon.gameObject.SetActive(true);
            //_changeIcon = TableManager.Instance.Item_Table[iconName].Icon_Id;
            _currentIcon = iconName;
            var iconData = TableManager.Instance.Clan_Icon_Table[iconName];

            if(iconData != null)
                _changeIcon = iconData.Icon_Id;

            _icon.SetSprite(_changeIcon);
        }
    }
    // 클랜 이름 UI
    public void SetClanName(string clanName)
    {
        _nameTMP.text = clanName;
    }
    // 클랜 공지 UI
    public void SetClanNotice(string clanNotice)
    {

        if (string.IsNullOrEmpty(clanNotice))
        {
            _noticeTMP.ToTableText("str_clan_notice_edit_msg_02");             //작성된 클랜 공지가 없습니다.(str_clan_notice_none)
            //_inputTMP.placeholder.GetComponent<ExTMPUI>().ToTableText("str_ui_profile_boxtouch_01");     // 박스를 터치해서 내용을 입력해주세요.
        }
        else
        {
            _noticeTMP.text = clanNotice;
           // _inputTMP.placeholder.GetComponent<ExTMPUI>().text = null;
        }
    }
    
    // 아이콘 변경 버튼 클릭 > 팝업 오픈
    public void OnClickSelectClanIcon()
    {
        // _iconPopup.ShowClanIcon(_data.Icon, true);
        if (_uiPopupEditClan == null)
            return;
        _uiPopupEditClan.OpenEditIconPopup(_currentIcon);
    }

    // 클랜 이름 변경 버튼 클릭 > 팝업 오픈
    public void OnClickEditClanName()
    {
        if (!GameInfoManager.Instance.EnableEditClanName())
            return;

        if (_uiPopupEditClan == null)
            return;
        string title = LocalizeManager.Instance.GetString("str_clan_name_edit_title");          // 클랜명
        string message = LocalizeManager.Instance.GetString("str_clan_name_edit_msg_01");       //변경하실 클랜명을 입력해주세요.
        //_nameEditPopup.Show(title, message, _nameTMP.text);
        _uiPopupEditClan.OpenEditNamePopup(title, message, _nameTMP.text);
    }
    // 클랜 공지 변경 버튼 클릭 > 팝업 오픈
    public void OnClickEditClanNotice()
    {
        if (_uiPopupEditClan == null)
            return;
        string title = LocalizeManager.Instance.GetString("str_clan_notice_edit_title");
        //_noticeEditPopup.Show(title, _noticeTMP.text);
        _uiPopupEditClan.OpenEditNoticePopup(title, _noticeTMP.text);
    }
    
    // 길드 자동가입체크 버튼 클릭 함수
    public void OnClickSetIsApproval()
    {
        if (_isApproval)
            _isApproval = false;
        else
            _isApproval = true;

        SetIsApproval(!_isApproval);
    }

    public void SetIsApproval(bool isOn)
    {
        _approveOnObj.SetActive(isOn);
    }

    // 정보 변경 확인 버튼
    public void SubmitEditClanInfo(Action endSubmit = null)
    {
        int conditionType = 0;
      
        int conditionValue = 0;

        int joinType = 0;
        if (_isApproval)
            joinType = 1;

        var isChange = GameInfoManager.Instance.CheckIsNothingEdit(_data, _currentIcon, _nameTMP.text,
            _noticeTMP.text, joinType, conditionType, conditionValue);
        if (!isChange)
        {
            UIManager.Instance.ShowToastMessage("str_ui_profile_info_change_check_01"); //
            return;
        }

        // 값이 변하지 않았으면 null,  값이 변한게 있으면 변한 값 
        string icon = null;
        if (!string.IsNullOrEmpty(_currentIcon) && !_currentIcon.Equals(_data.Icon))
            icon = _currentIcon;

        string name = null;
        if (!string.IsNullOrEmpty(_nameTMP.text) && !_nameTMP.text.Equals(_data.Name))
            name = _nameTMP.text;

        string notice = null;
        if (!string.IsNullOrEmpty(_noticeTMP.text) && !_noticeTMP.text.Equals(_data.Notice))
            notice = _noticeTMP.text;

        //EditClanData editClan = new EditClanData(name, notice, icon, conditionType, conditionValue, joinType, new Dictionary<string, int>() { { "test", 1 } });
        EditClanData editClan = new EditClanData(name, notice, icon, conditionType, conditionValue, joinType, null);
        var clanId = GameInfoManager.Instance.ClanInfo.MyClanData.Id;
        RestApiManager.Instance.RequestClanSetSettings(clanId, editClan, (response) => {

            RestApiManager.Instance.CheckIsEmptyResult(/*ClanState.Overlapped,*/ response, () =>
            {
                _data = GameInfoManager.Instance.ClanInfo.MyClanData;
                var uiClanInfo = UIManager.Instance.GetUI<UIClanInfo>();
                if (uiClanInfo != null && uiClanInfo.gameObject.activeInHierarchy)
                {
                    uiClanInfo.UIClanInfoObject.UpdateUI(_data);
                }
                UIManager.Instance.ShowToastMessage("str_info_change_default");
                endSubmit?.Invoke();
            });
        });
    }
}
