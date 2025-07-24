using LIFULSE.Manager;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class UIClanInfoObject : MonoBehaviour
{
    [Header("Clan Info Sorted UI")]
    [SerializeField] private List<UILobbyTypeTab> _selectButtons;
    [SerializeField] private GameObject _selectInfoObj;
    [SerializeField] private GameObject _selectNoticeObj;

    [Header("Clan Level UI")]
    [SerializeField] private GameObject _levelInfoButton;
    [SerializeField] private GameObject _levelInfoObj;
    [SerializeField] private ExTMPUI _dailyPointInfoTMP;
    [SerializeField] private ExTMPUI _weeklyPointInfoTMP;

    [Header("Clan Info UI")]
    [SerializeField] private ExImage _icon;
    [SerializeField] private ExTMPUI _nameTMP;
    [SerializeField] private ExTMPUI _levelTMP;
    [SerializeField] private ExTMPUI _expTMP;
    [SerializeField] private ExTMPUI _goalExpTMP;
    [SerializeField] private Slider _expSlider;
    [SerializeField] private ExTMPUI _memberTMP;
    [SerializeField] private ExTMPUI _joinTypeTMP;
    [SerializeField] private ExTMPUI _noticeTMP;
    [SerializeField] private GameObject _expObj;
    [SerializeField] private GameObject _maxObj;

    [Header("Clan Skill UI")]
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private VerticalLayoutGroup _layout;
    [SerializeField] private GameObject _skillObj;

    [Header("Clan Skill Point UI")]
    [SerializeField] private List<UILayoutGroup> _uiLayoutGroup;

    [Header("CheatUI")]
    [SerializeField] private GameObject _cheatInputObj;
    [SerializeField] private GameObject _cheatButtonObj;

    //경험치 치트
    [SerializeField] private TMP_InputField _inputField;
    private ClanData _clanData;
    private int _tabIndex;

    private List<ClanSkillInfoCell> _skillCells;
    private List<ClanSkillInfoCell> _skillPoints;

    private Dictionary<string, ClanSkillInfoCell> _skillDic = new Dictionary<string, ClanSkillInfoCell>();

    private bool _isShowPointWnd = false;
    private bool _isShowPointInfo = false;
    private void Start()
    {

        //_skillCells = new List<ClanSkillInfoCell>();
        //_skillCells.Add(_skillObj.GetComponent<ClanSkillInfoCell>());
    }

    public void InitData(ClanData data)
    {
        _cheatInputObj.SetActive(GameInfoManager.Instance.UseCheat);
        _cheatButtonObj.SetActive(GameInfoManager.Instance.UseCheat);

        #region[치트 서버별 예외처리]
        //if (RestApiManager.Instance.ServerType == ServerType.Live || RestApiManager.Instance.ServerType == ServerType.External)
        //{
        //    _cheatInputObj.SetActive(false);
        //    _cheatButtonObj.SetActive(false);
        //}
        //else
        //{
        //    _cheatInputObj.SetActive(true);
        //    _cheatButtonObj.SetActive(true);
        //}
        #endregion

        _clanData = data;
        _tabIndex = -1;
        _isShowPointWnd = false;
        _isShowPointInfo = false;
        //_skillPointWnd.SetActive(false);
        OnClickSelectInfo(0);
        UpdateUI(data);
    }

    public void UpdateUI(ClanData data)
    {
        _clanData = data;
        SetIsInfoUI();
        _levelInfoObj.SetActive(false);
        SetClanExp(_clanData.Exp);
        SetClanIcon();
        SetClanName();
        SetClanMember(_clanData.Members.Count);
        SetConditionTMP();
        
        SetClanSkills();
        SetClanNotice();

        for (int n = 0; n< _uiLayoutGroup.Count; n++)
        {
            _uiLayoutGroup[n].UpdateLayoutGroup();
        }
    }

    // 클랜 정보 화면일때만 보이는 UI 세팅
    public void SetIsInfoUI()
    {
        var uiClanInfo = UIManager.Instance.GetUI<UIClanInfo>();
        _levelInfoButton.gameObject.SetActive(uiClanInfo != null && uiClanInfo.gameObject.activeInHierarchy);

        var definedata = TableManager.Instance.Define_Table["ds_clan_contribution_max_point"];

        _dailyPointInfoTMP.ToTableText("str_clan_daily_contribution_01",  definedata.Opt_01_Int);
        _weeklyPointInfoTMP.ToTableText("str_clan_weekly_contribution_01",definedata.Opt_02_Int);
    }

    public void SetClanSkills()
    {
       // var skillList = GameInfoManager.Instance.SkillList;
        var skillList = TableManager.Instance.Clan_Skill_Table.GetSkillList();

        if (_skillCells == null)
        {
            _skillCells = new List<ClanSkillInfoCell>();
            _skillCells.Add(_skillObj.GetComponent<ClanSkillInfoCell>());
        }
     
        if (skillList.Count > _skillCells.Count)
            CreateCell(skillList.Count - _skillCells.Count);

        UpdateSkillList(skillList);
    }
    private void CreateCell(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var newObj = Instantiate(_skillObj, _layout.transform);
            var skilCell = newObj.GetComponent<ClanSkillInfoCell>();

            _skillCells.Add(skilCell);
        }
    }
    //public void UpdateSkillList(Dictionary<string, int> skillData)
    public void UpdateSkillList(List<string> skillData)
    {
       
        for (int i = 0; i < _skillCells.Count; i++)
        {
            if (i < skillData.Count)
            {
                _skillCells[i].gameObject.SetActive(true);
                _skillCells[i].InitData(skillData[i], _clanData);

                if (!_skillDic.ContainsKey(skillData[i]))
                    _skillDic.Add(skillData[i], _skillCells[i]);
            }
            else
                _skillCells[i].gameObject.SetActive(false);
        }
    }

    private void SetClanExp(int exp)
    {
        if (GameInfoManager.Instance.IsMaxClanExp(_clanData.Level, exp))
        {
            _expObj.SetActive(false);
            _maxObj.SetActive(true);
            _levelTMP.ToTableText("str_ui_char_level_001", GameInfoManager.Instance.GetMaxClanLevel());        //Lv.{0}
            
            return;
        }

        _expObj.SetActive(true);
        _maxObj.SetActive(false);
        _levelTMP.ToTableText("str_ui_char_level_001", _clanData.Level);        //Lv.{0}
        // _levelTMP.text = "Lv."+_clanData.Level.ToString();

        //_expTMP.text =  _clanData.Point.ToString();
        var levelTable = TableManager.Instance.Clan_Level_Table;
        var levelTid = "level_"+_clanData.Level;
        var goalExp = levelTable[levelTid].Clan_Level_Exp;
        _expTMP.text =  exp.ToString();
        _goalExpTMP.text = goalExp.ToString();
        _expSlider.maxValue =  goalExp;
        _expSlider.minValue =  0;
        //_expSlider.value =  _clanData.Point;
        _expSlider.value =  exp;
    }

    private void SetClanIcon()
    {
        if (!string.IsNullOrEmpty(_clanData.Icon))
        {
            var table = TableManager.Instance.Clan_Icon_Table[_clanData.Icon];

            if(table != null)
                _icon.SetSprite(table.Icon_Id);
        }
        else
            _icon.SetSprite("IC_MN_dongjasam_01_result");
    }

    private void SetClanName()
    {
        if (!string.IsNullOrEmpty(_clanData.Name))
            _nameTMP.text = _clanData.Name;
        else
            _nameTMP.text = "어쨌든 클랜 있음";
    }
    private void SetClanNotice()
    {
        if (!string.IsNullOrEmpty(_clanData.Notice))
            _noticeTMP.text = _clanData.Notice;
        else
            _noticeTMP.ToTableText("str_clan_notice_none");         //작성된 클랜 공지가 없습니다.
    }
    public void SetClanMember(int currentMembers)
    {
        int maxMembers = _clanData.MaxMemberCount;
        _memberTMP.ToTableText("str_ui_collection_monster_cost", currentMembers, maxMembers);
    }

    private void SetConditionTMP()
    {
        string approveStr = GameInfoManager.Instance.GetClanApproveString(_clanData.JoinType);
        string conditionTypeStr = GameInfoManager.Instance.GetClanConditionString(_clanData.ConditionType, _clanData.ConditionValue);

        StringBuilder sb = new StringBuilder();
        sb.Append(LocalizeManager.Instance.GetString(approveStr));
        sb.AppendLine();
        sb.Append(conditionTypeStr);

        //_joinTypeTMP.text = sb.ToString();
        _joinTypeTMP.ToTableText(approveStr);
    }
    private void SetShowClanInfoObj(bool isInfo)
    {
        _selectInfoObj.SetActive(isInfo);
        _selectNoticeObj.SetActive(!isInfo);
        //if (_skillPointWnd.activeInHierarchy)
        //    _skillPointWnd.SetActive(false);
    }

    public void OnClickSelectInfo(int index)
    {
        if (_tabIndex == index)
            return;

        if (_tabIndex != -1)
        {
            _selectButtons[_tabIndex].Off();
        }

        _selectButtons[index].On();
        _tabIndex = index;
        SetShowClanInfoObj(_tabIndex == 0);
    }

    //클랜 경험치 증가 치트
    public void OnClickCheatClanExp()
    {
        var exp = int.Parse(_inputField.text);

        if(exp > 0)
        {
            RestApiManager.Instance.RequestClanAddExp(_clanData.Id, exp, (response) => {
                var resultExp = response["result"]["clandata"]["M"]["E"].N_Int();
                SetClanExp(resultExp);
            });
        }
    }

    public void OnClickLevelInfo()
    {
        if (_isShowPointInfo == false)
            _isShowPointInfo = true;
        else
            _isShowPointInfo = false;

        _levelInfoObj.SetActive(_isShowPointInfo);
    }

}
