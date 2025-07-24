using LIFULSE.Manager;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;


public class ClanSkillCell : MonoBehaviour
{
    [SerializeField] private ExImage _icon;
    [SerializeField] private ExTMPUI _levelTMP;
    [SerializeField] private ExTMPUI _titleTMP;
    [SerializeField] private ExTMPUI _pointTMP;
    [SerializeField] private ExTMPUI _buffTMP;
    [SerializeField] private ExButton _upButton;
    [SerializeField] private GameObject _upButtonDimd;
    [SerializeField] private ExButton _downButton;
    [SerializeField] private GameObject _downButtonDimd;

    [SerializeField] private List<UILayoutGroup> _uiLayoutGroups;

    private string _skillGroupId;
    private int _level = 0;

    private int _minLevel;
    private int _maxLevel;

    private Clan_Skill_TableData _data;
    
    private UIEditClanSkill _uIEditClanSkill;
    public ExButton UpButton { get { return _upButton; } }

    public void InitClanSkill(string groupid, int level)
    {
        _skillGroupId = groupid;
        _level = level;
        _data = TableManager.Instance.Clan_Skill_Table.GetSkillData(_skillGroupId, _level);

        var skilltable = TableManager.Instance.Clan_Skill_Table.GetSkillDatas(groupid);
        _minLevel = skilltable.FirstOrDefault().Clan_Skill_Level;
        _maxLevel = skilltable.LastOrDefault().Clan_Skill_Level;

        var uieditclan = UIManager.Instance.GetUI<UIPopupEditClan>();
        if (uieditclan != null && uieditclan.gameObject.activeInHierarchy)
            _uIEditClanSkill = uieditclan.UIEditClanSkill;

        //_pointTMP.text = "필요 포인트 : " + _point;
        _titleTMP.ToTableText(_data.Skill_Name_Text_Id);
        EnableChangeSkillPoint();
        SetLevelUI();
        SetSkillIconUI();
        
        for (int n = 0; n< _uiLayoutGroups.Count; n++)
        {
            _uiLayoutGroups[n].UpdateLayoutGroup();
        }
    }

    private void SetSkillIconUI()
    {
        var table = TableManager.Instance.Item_Table[_data.Clan_Item_Tid];
        _icon.SetSprite(table.Icon_Id);
    }
    private void SetSkillBuffUI(int level)
    {
        var data = TableManager.Instance.Clan_Skill_Table.GetSkillData(_skillGroupId, level);
        var bonusinfo = TableManager.Instance.Item_Bonus_Table[data.Bonus_Id];
        var rewardinfo = TableManager.Instance.Reward_Table.GetRewardDataByGroupId(bonusinfo.Reward);
        _buffTMP.text = rewardinfo.Item_Min.ToString();

        //_buffTMP.text = GameInfoManager.Instance.SetSkillBuffIndex(data.Bonus_Id, "str_ui_percentage_value_default");   // //{0}%
    }

    private void SetLevelUI()
    {
        _levelTMP.text = "Lv."+_level;

        SetSkillBuffUI(_level);
    }

    public void EnableChangeSkillPoint()
    {
        if (_uIEditClanSkill.SkillPoint < 1 || _level >= _maxLevel)
        {
            _pointTMP.SetColor("#FF6B6B");
            _upButtonDimd.SetActive(true);
        }
        else
        {
            _pointTMP.SetColor("#717171");
            _upButtonDimd.SetActive(false);
        }
        if (_level > _minLevel)
            _downButtonDimd.SetActive(false);
        else
            _downButtonDimd.SetActive(true);
    }

    public void OnClickLevelUP()
    {
        _level++;
        SetLevelUI();
        var uieditclan = UIManager.Instance.GetUI<UIPopupEditClan>();
        if (uieditclan != null && uieditclan.gameObject.activeInHierarchy)
            uieditclan.UIEditClanSkill.OnClickPointUP(true, _skillGroupId);
    }

    public void OnClickLevelDown()
    {
        _level--;
        SetLevelUI();
        var uieditclan = UIManager.Instance.GetUI<UIPopupEditClan>();
        if (uieditclan != null && uieditclan.gameObject.activeInHierarchy)
            uieditclan.UIEditClanSkill.OnClickPointUP(false, _skillGroupId);
    }

    public void OnClickLevelUPDimd()
    {
        //if (_uIEditClanSkill.SkillPoint < 1)
        //    UIManager.Instance.ShowToastMessage("스킬포인트가 부족합니다!");
        //if(_level >= _maxLevel)
        //    UIManager.Instance.ShowToastMessage("스킬이 이미 최대 레벨입니다.");
    }
    public void OnClickLevelDownDimd()
    {
        //UIManager.Instance.ShowToastMessage("스킬이 최소 레벨입니다!");
    }
}
