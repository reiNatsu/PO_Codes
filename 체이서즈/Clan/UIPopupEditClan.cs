using Consts;
using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIPopupEditClan : UIBase
{
    [SerializeField] private ExTMPUI _titleTMP;
    [SerializeField] private GameObject _submitDimd;
 
    [SerializeField] private UIEditClanInfo _uiEditClanInfo;
    [SerializeField] private UIEditClanSkill _uiEditClanSkill;
    [SerializeField] private GameObject _uiInfoDataBg;

    [SerializeField] private UIUserProfileIconPopup _iconPopup;
    [SerializeField] private UIUserProfileIntroductionPopup _noticeEditPopup;
    [SerializeField] private UIUserProfileNicknamePopup _nameEditPopup;

    private ClanData _data;
    private bool _isInfo = false;
    private Coroutine _coroutine = null;

    public UIEditClanInfo UIEditClanInfo { get { return _uiEditClanInfo; } }
    public UIEditClanSkill UIEditClanSkill { get { return _uiEditClanSkill; } }
    public override void Close(bool needCached = true)
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
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
            if (optionDict.TryGetValue(UIOption.Data, out var clanData))
            {
                _data = (ClanData)clanData;
            }
             if (optionDict.TryGetValue(UIOption.Bool, out var isInfo))
            {
                _isInfo = (bool)isInfo;
            }
        }

        _submitDimd.SetActive(false);
        ShowEditPopup();
        UpdateUI();
    }

    public void UpdateClanData(ClanData clandata)
    {
        _data =clandata;
    }

    private void UpdateUI()
    {
        if (_isInfo)
        {
            _titleTMP.ToTableText("str_clan_info_edit_title");              // 클랜 정보 수정
            _uiEditClanInfo.gameObject.SetActive(true);
            _uiEditClanSkill.gameObject.SetActive(false);
            _uiInfoDataBg.SetActive(true);

            _uiEditClanInfo.InitInfoData(_data);
        }
        else
        {
            _titleTMP.ToTableText("str_clan_skill_edit_title");             //스킬 관리
            _uiEditClanInfo.gameObject.SetActive(false);
            _uiEditClanSkill.gameObject.SetActive(true);
            _uiInfoDataBg.SetActive(false);

            _uiEditClanSkill.InitClanSkills(_data);
        }
    }

    public void ShowEditPopup(bool isIcon = false,bool isName= false, bool isNotice = false )
    {
        _iconPopup.gameObject.SetActive(isIcon);
        _nameEditPopup.gameObject.SetActive(isName);
        _noticeEditPopup.gameObject.SetActive(isNotice);
        //if (_iconPopup != null && _iconPopup.gameObject.activeInHierarchy == !isIcon)
        //    _iconPopup.gameObject.SetActive(isIcon);

        //if (_nameEditPopup != null && _nameEditPopup.gameObject.activeInHierarchy == !isName)
        //    _nameEditPopup.gameObject.SetActive(isName);

        //if (_noticeEditPopup != null && _noticeEditPopup.gameObject.activeInHierarchy == !isNotice)
        //    _noticeEditPopup.gameObject.SetActive(isNotice);
    }

    public void OpenEditIconPopup(string iconName)
    {
        ShowEditPopup(isIcon: true);
        _iconPopup.ShowClanIcon(iconName, true);
    }
    public void OpenEditNamePopup(string title, string message, string index)
    {
        ShowEditPopup(isName: true);
        _nameEditPopup.Show(title, message, index);
    }
    public void OpenEditNoticePopup(string title, string index)
    {
        ShowEditPopup(isNotice: true);
        _noticeEditPopup.Show(title, index);
    }

    public void OnClickSubmitEdit()
    {
        if (_coroutine == null)
            _coroutine = StartCoroutine(SetSubmitDimd());

        if (_isInfo)
        {
            _uiEditClanInfo.SubmitEditClanInfo(() =>
            {
                Close();
            });
        }
        else
        {
            Action endEditSkill = () =>{

                UIManager.Instance.ShowToastMessage("str_ui_skill_custom_change_01"); //스킬 정보가 수정 되었습니다.
                _submitDimd.SetActive(false);

                //var skillonfos = GameInfoManager.Instance.ClanInfo.MyClanData.SkillInfos;
                //var uiClaninfo = UIManager.Instance.GetUI<UIClanInfo>();
                //if (uiClaninfo != null && uiClaninfo.gameObject.activeInHierarchy)
                //    uiClaninfo.UIClanInfoObject.UpdateSkillInfoCells(skillonfos);
            };

            _uiEditClanSkill.SubmitEditClanSkill(endEditSkill);
        }
    }

    IEnumerator SetSubmitDimd()
    {
        _submitDimd.SetActive(true);
        yield return new WaitForSeconds(1);
        _submitDimd.SetActive(false);
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }

    public void OnClickSubmitDimd()
    {
        UIManager.Instance.ShowToastMessage("str_ui_server_error_dec_01"); //서버 요청을 처리 중입니다.
    }

    public void OnDropdownValueChanged(int selectedIndex)
    {
        // 드롭다운 리스트가 열릴 때 선택된 항목으로 포커싱
        //StartCoroutine(FocusDropdownItem(selectedIndex));
      
    }
 
}
