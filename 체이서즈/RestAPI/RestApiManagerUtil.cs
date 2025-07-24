using Consts;
using LIFULSE.Manager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static RestApiManager;
using static UnityEngine.Rendering.DebugUI;
using System.Linq;
using UnityEngine;



#if UNITY_EDITOR
using Lean.Common.Editor;
#endif
public class QusetCondition
{
    public QUEST_CONDITION_TYPE ConditonType;
    public string ConditionValue;

}
public partial class RestApiManager : Singleton<RestApiManager>
{
    //enum, enum

    private ResponseQuest GetResponseQuest()
    {
        var list = GameInfoManager.Instance.QuestDataList;
        var questSet = new HashSet<string>();

        ResponseQuest questdata = new ResponseQuest();

        for (int n = 0; n < list.Count; n++)
        {
            //업적인 경우
            if (TableManager.Instance.Quest_Table.QuestGroupDic.TryGetValue(list[n].Tid, out var groupValue))
            {
                if (!questSet.Contains(list[n].Tid))
                {
                    questdata.quests.Add(new Dictionary<string, string> { { "tid", list[n].Tid } });
                    questSet.Add(list[n].Tid);

                    UpdateNaviQuest(questdata, list[n].Tid);
                }
            }
            else
            {
                var groupId = TableManager.Instance.Quest_Table.GetFeatGroupId(list[n].Tid);

                //일간, 주간 퀘와 연관있는 업적 추가
                if (!string.IsNullOrEmpty(groupId) && !questSet.Contains(groupId))
                {
                    questdata.quests.Add(new Dictionary<string, string> { { "tid", groupId } });
                    questSet.Add(groupId);

                    UpdateNaviQuest(questdata, groupId);
                }

                questdata.quests.Add(new Dictionary<string, string> { { "tid", list[n].Tid } });
            }
        }

        return questdata;
    }

    private void UpdateNaviQuest(ResponseQuest questdata, string qusetGroupId)
    {
        if (!string.IsNullOrEmpty(questdata.NaviQuestTid))
            return;

            var naviQuestTid = GameInfoManager.Instance.NaviQuestInfo.GetCheckTid(qusetGroupId);

        if (!string.IsNullOrEmpty(naviQuestTid))
            questdata.NaviQuestTid = naviQuestTid;
    }

    private ResponseQuest GetResponseQuest(List<string> questTidList, bool isReward)
    {
        ResponseQuest questdata = new ResponseQuest();
        HashSet<string> tids = new HashSet<string>();

        for (int n = 0; n < questTidList.Count; n++)
        {
            //업적이 아닌 경우 경우
            if (!isReward && !TableManager.Instance.Quest_Table.QuestGroupDic.Contains(questTidList[n]))
            {
                var groupId = TableManager.Instance.Quest_Table.GetFeatGroupId(questTidList[n]);

                if (!string.IsNullOrEmpty(groupId) && !tids.Contains(groupId))
                {
                    questdata.quests.Add(new Dictionary<string, string> { { "tid", groupId } });
                    tids.Add(groupId);
                }
            }

            if(!tids.Contains(questTidList[n]))
            {
                questdata.quests.Add(new Dictionary<string, string> { { "tid", questTidList[n] } });
                tids.Add(questTidList[n]);
            }

        }

        return questdata;
    }

    public ResponseQuest GetQuestsBodyDatas(QUEST_CONDITION_TYPE type, QUEST_CONDITION_VALUE value)
    {
        GameInfoManager.Instance.QuestDataList = new List<QuestData>();
        string conditionsType = type.ToString();
        string conditionsValue = value.ToString();
        GameInfoManager.Instance.GetQuestDatasList(conditionsType, conditionsValue);
        ResponseQuest questdata = GetResponseQuest();

        return questdata;
    }
    //enum, string
    public ResponseQuest GetQuestsBodyDatas(QUEST_CONDITION_TYPE type, string value)
    {
        GameInfoManager.Instance.QuestDataList = new List<QuestData>();

        //string conditionsType = type.ToString();
        string conditionsType = "";
        if (type ==QUEST_CONDITION_TYPE.None)
        {
            conditionsType = string.Empty;
        }
        else
        {
            conditionsType = type.ToString();
        }
        GameInfoManager.Instance.GetQuestDatasList(conditionsType, value);
        ResponseQuest questdata = GetResponseQuest();

        return questdata;
    }


    //스테이지나 던전 클리어 관련 퀘스트 데이터 생성 해주는 함수
    public ResponseQuest GetStageQuestDatas(string stageTid, bool useHelper)
    {
        ResponseQuest questdata = new ResponseQuest();

        List<string> types = new List<string>();
        List<string> values = new List<string>();

        var stageData = TableManager.Instance.Stage_Table[stageTid];

        if (stageData == null)
            return null;

        if(!string.IsNullOrEmpty(stageData.Need_Cost_Type) && stageData.Need_Cost_Value > 0)
        {
            types.Add(QUEST_CONDITION_TYPE.ConsumCurrency.ToString());
            values.Add(stageData.Need_Cost_Type);
        }

        types.Add(QUEST_CONDITION_TYPE.StageGroup.ToString());
        values.Add(stageData.CONTENTS_TYPE_ID.ToString());

        questdata = GetQuestsBodyDatas(types, values);

        //스테이지 입장 및 클리어 시 업적과 관련된 퀘스트 데이터 추가
        if (TableManager.Instance.Quest_Table.QuestTypeDict[QUEST_CONDITION_TYPE.Stage.ToString()].TryGetValue(stageTid, out var groupId))
        {
            if(!IsContains(questdata, groupId))
            {
                questdata.quests.Add(new Dictionary<string, string>() { { "tid", groupId } });
                UpdateNaviQuest(questdata, groupId);
            }
        }
        else if (TableManager.Instance.Quest_Table.QuestTypeDict[QUEST_CONDITION_TYPE.StageGroup.ToString()].TryGetValue(stageData.CONTENTS_TYPE_ID.ToString(), out var groupId2))
        {
            if (!IsContains(questdata, groupId2))
            {
                questdata.quests.Add(new Dictionary<string, string>() { { "tid", groupId2 } });
                UpdateNaviQuest(questdata, groupId2);
            }
        }

        if (useHelper)
        {
            if (TableManager.Instance.Quest_Table.QuestTypeDict[QUEST_CONDITION_TYPE.ConsumCurrency.ToString()].TryGetValue("i_gold", out var goldGroup))
            {
                if (!IsContains(questdata, goldGroup))
                {
                    questdata.quests.Add(new Dictionary<string, string>() { { "tid", goldGroup } });
                    UpdateNaviQuest(questdata, goldGroup);
                }
            }
        }

        return questdata;
    }

    private bool IsContains(ResponseQuest questdata, string groupId)
    {
        for (int i = 0; i < questdata.quests.Count; i++)
        {
            var data = questdata.quests[i];

            if (data.ContainsValue(groupId))
                return true;
        }

        return false;
    }

    //enum[], enum[]
    public ResponseQuest GetQuestsBodyDatas(List<string> types, List<string> values)
    {
        GameInfoManager.Instance.QuestDataList = new List<QuestData>();

        for (int m = 0; m < types.Count; m++)
        {
            string conditionsType = types[m];
            string conditionsValue = values[m];

            GameInfoManager.Instance.GetQuestDatasList(conditionsType, conditionsValue);
        }

        ResponseQuest questdata = GetResponseQuest();

        return questdata;
    }

    //enum[], string[]
    // customKey 이벤트의 일일퀘와 일반 일일퀘 타입이 같음으로 이벤트 예외처리용으로 사용됨
    public ResponseQuest GetQuestsBodyDatas(QUEST_CONDITION_TYPE[] type, string[] value,string customKey = "",string customArg = "")
    {
        GameInfoManager.Instance.QuestDataList = new List<QuestData>();
        for (int m = 0; m < type.Length; m++)
        {
            string conditionsType = type[m].ToString();
            GameInfoManager.Instance.GetQuestDatasList(conditionsType, value[m],customKey);
        }

        ResponseQuest questdata = GetResponseQuest();

        if (!string.IsNullOrEmpty(customArg))
        {
            switch (customArg)
            {
                case "ch01_s08_n":
                    questdata.quests.Add(new Dictionary<string, string> { { "tid", "stage_boss_01" } });
                    break;
                case "ch02_s07_n":
                    questdata.quests.Add(new Dictionary<string, string> { { "tid", "stage_boss_02" } });
                    break;

            }
        }

        return questdata;
    }

    //List<string>
    public ResponseQuest GetQuestsBodyDatas(List<string> questTid)
    {
        ResponseQuest questdata = GetResponseQuest(questTid, true);

        return questdata;
    }
}

[System.Serializable]
public class ServerQuestData
{
    //key "tid", value tid
    public List<Dictionary<string, string>> quests = new List<Dictionary<string, string>>();
    public void AddQuest(string tid)
    {
        quests.Add(new Dictionary<string, string> { { "tid", tid } });
    }

    public void Clear()
    {
        quests.Clear();
    }

    public string Export()
    {
        return JToken.FromObject(this).ToString();
    }
}