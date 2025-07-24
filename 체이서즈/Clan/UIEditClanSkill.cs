using LIFULSE.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEditClanSkill : MonoBehaviour
{
  
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private HorizontalLayoutGroup _layout;
    [SerializeField] private GameObject _skillObj;
    [SerializeField] private GameObject _noSkillObj;
    [SerializeField] private ExTMPUI _skillPointTMP;

    private ClanData _data;
    private List<ClanSkillCell> _skillCells;
    private int _skillPoint;
    // 임시 스킬
    private Dictionary<string, int> _skillLevels = new Dictionary<string, int>();
    private List<string> _skills = new List<string>();
    public int SkillPoint { get { return _skillPoint; }}

    private void OnDisable()
    {
    }

    public void InitClanSkills(ClanData data)
    {
        if (_skillCells == null)
        {
            _skillCells = new List<ClanSkillCell>();
            _skillCells.Add(_skillObj.GetComponent<ClanSkillCell>());
        }

        _data = data;
        //foreach (var skilldata in GameInfoManager.Instance.SkillList)
        //{
        //    if (!_skills.ContainsKey(skilldata.Key))
        //    {
        //        _skills.Add(skilldata.Key, 0);
        //    }
        //}
        _skills = TableManager.Instance.Clan_Skill_Table.GetSkillList();
        if (_skills == null || _skills.Count == 0)
        {
            _noSkillObj.SetActive(true);
            _skillObj.SetActive(false);
        }
        else
        {
            _noSkillObj.SetActive(false);
            _skillObj.SetActive(true);
        }
        
        SetSkillLists(data);
        //SetClanLevelGaugeUI();
        //SetClanLevelGaugeUI();
        SetSkillPointUI(true);
    }

    private void SetSkillLists(ClanData data)
    {
        _skillPoint = data.SkillPoint;

        //foreach (var skills in data.SkillInfos)
        //{
        //    if (_skills.ContainsKey(skills.Key))
        //        _skills[skills.Key] = skills.Value;
        //}

        if (_skillCells.Count < _skills.Count)
            CreateCell(_skills.Count - _skillCells.Count);

        UpdateSkills(_skills);

    }

    public void SetSkillPointUI(bool isUp ,int point = 0)
    {
        if (isUp)
            _skillPoint -= point;
        else
            _skillPoint += point;
        //_skillPointTMP.text = _skillPoint.ToString();
        _skillPointTMP.ToTableText("str_clan_left_skill_point", _skillPoint);
    }

    private void CreateCell(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var newObj = Instantiate(_skillObj, _layout.transform);
            var skillCell = newObj.GetComponent<ClanSkillCell>();

            _skillCells.Add(skillCell);
        }
    }

    //private void UpdateSkills(Dictionary<string, int> skills)
    private void UpdateSkills(List<string> skills)
    {
        if (skills.Count >2 )
        {
            _scrollRect.movementType = ScrollRect.MovementType.Elastic;
            _layout.padding.left = 20;
        }
        else
        {
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
            _layout.padding.left = 20;
        }

        //List<string> keys = new List<string>(skills.Keys);
        //List<int> values = new List<int>(skills.Values);

        for (int i = 0; i < _skillCells.Count; i++)
        {
            if (i < skills.Count)
            {
                var level = 1;
                if (GameInfoManager.Instance.ClanInfo.MyClanData.SkillInfos != null &&  GameInfoManager.Instance.ClanInfo.MyClanData.SkillInfos.ContainsKey(skills[i]))
                    level= GameInfoManager.Instance.ClanInfo.MyClanData.SkillInfos[skills[i]]; 
                _skillCells[i].gameObject.SetActive(true);
                _skillCells[i].InitClanSkill(skills[i], level);
                if (!_skillLevels.ContainsKey(skills[i]))
                    _skillLevels.Add(skills[i], level);
                else
                    _skillLevels[skills[i]] = level;
            }
            else
                _skillCells[i].gameObject.SetActive(false);
        }
    }

    public void OnClickPointUP(bool isUp,string groupid)
    {
        int point = 1;
        if (isUp)
            _skillLevels[groupid]++;
        else
            _skillLevels[groupid]--;

        SetSkillPointUI(isUp, point);
        for (int n = 0; n < _skills.Count; n++)
        {
            _skillCells[n].EnableChangeSkillPoint();
        }
    }

    public void SubmitEditClanSkill(Action endSubmit = null)
    {
        EditClanData editClan = new EditClanData(null, null, null, _data.ConditionType, _data.ConditionValue, _data.JoinType, _skillLevels);
        var clanId = GameInfoManager.Instance.ClanInfo.MyClanData.Id;
        RestApiManager.Instance.RequestClanSetSettings(clanId, editClan, (response) =>
        {
            // 클랜 정보 화면의 ClanData도 수정 할 필요 있음
            _data = GameInfoManager.Instance.ClanInfo.MyClanData;
            var uiPopupEditClan = UIManager.Instance.GetUI<UIClanInfo>();
            if (uiPopupEditClan != null && uiPopupEditClan.gameObject.activeInHierarchy)
                uiPopupEditClan.UpdateClanData(_data);
            
            var uiClanInfo = UIManager.Instance.GetUI<UIClanInfo>();
            if (uiClanInfo != null && uiClanInfo.gameObject.activeInHierarchy)
            {
                uiClanInfo.UIClanInfoObject.UpdateUI(_data);
            }

            SetSkillLists(_data);
            SetSkillPointUI(true);
            endSubmit?.Invoke();
        });
    }

}
