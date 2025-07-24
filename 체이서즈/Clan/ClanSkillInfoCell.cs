using LIFULSE.CharacterController;
using LIFULSE.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClanSkillInfoCell : MonoBehaviour
{
    [SerializeField] private ExImage _skillInfoIMG;
    [SerializeField] private ExTMPUI _skillInfoTMP;
    [SerializeField] private ExTMPUI _skillLevelTMP;
    [SerializeField] private UILayoutGroup _uiLayoutGroup;

    private string _skillGroupId;
    private ClanData _clanData;

    public void InitData(string groupid, ClanData clandata)
    {
        _skillGroupId = groupid;
        _clanData = clandata;
        SetSkillIcon();
        SetSkillInfoTMP();

        _uiLayoutGroup.UpdateLayoutGroup();
    }

    public void SetSkillIcon()
    {
        var tableid = TableManager.Instance.Clan_Skill_Table.GetSkillIcon(_skillGroupId);
        var table = TableManager.Instance.Item_Table[tableid];
        _skillInfoIMG.SetSprite(table.Icon_Id);
    }
    public void SetSkillInfoTMP()
    {
        var level = 1;
        //if (GameInfoManager.Instance.ClanInfo.MyClanData != null && GameInfoManager.Instance.ClanInfo.MyClanData.SkillInfos.ContainsKey(_skillGroupId))
        //    level = GameInfoManager.Instance.ClanInfo.MyClanData.SkillInfos[_skillGroupId];
        if(_clanData != null && _clanData.SkillInfos.ContainsKey(_skillGroupId))
            level = _clanData.SkillInfos[_skillGroupId];

        // var buff = GameInfoManager.Instance.GetClanSkillBuff(_skillTid) * level;
        var skillTable = TableManager.Instance.Clan_Skill_Table.GetSkillData(_skillGroupId, level);
        _skillLevelTMP.text = "Lv."+level.ToString();
        if (string.IsNullOrEmpty(skillTable.Bonus_Id))
        {
            _skillInfoTMP.text  = LocalizeManager.Instance.GetString(skillTable.Skill_Name_Text_Id);
        }
        else
        {
            var bonusinfo = TableManager.Instance.Item_Bonus_Table[skillTable.Bonus_Id];
            var rewardinfo = TableManager.Instance.Reward_Table.GetRewardDataByGroupId(bonusinfo.Reward);
            //_buffTMP.text = rewardinfo.Item_Min.ToString();

            _skillInfoTMP.text =LocalizeManager.Instance.GetString(skillTable.Skill_Name_Text_Id) +"  "+ rewardinfo.Item_Min.ToString();
        }
    }

    
}
