using Consts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LIFULSE.Manager
{
    public partial class GameInfoManager
    {
        private QuestBase _questBase = new QuestBase();
        public QuestBase QuestBase { get => _questBase; }

        private List<Dictionary<string, string>> _questDatas = new List<Dictionary<string, string>>();

        [SerializeField] public List<QuestData> QuestDataList = new List<QuestData>();


        // 퀘스트 Reddot 초기화
        public void UpdateRedDotQuestData()
        {
            var questtypes = TableManager.Instance.Quest_Table.GetQuestTypeList();
            for (int n = 0; n< questtypes.Count; n++)
            {
                int index = (int)questtypes[n];
                RedDotManager.Instance.UpdateRedDotDictionary("reddot_quests_"+questtypes[n].ToString(), index);
            }
        }

        // 퀘스트Tab, 퀘스트Cell-tag string return
        public string ReturnQuestTypeString(QUEST_TYPE type)
        {
            string typestring = "";
            switch (type)
            {
                case QUEST_TYPE.all:
                    typestring = "str_tap_all_01";
                    break;
                case QUEST_TYPE.daily:
                    typestring = "str_tap_daily_01";
                    break;
                case QUEST_TYPE.week:
                    typestring = "str_tap_week_01";
                    break;
                case QUEST_TYPE.feat:
                    typestring = "str_tap_feat_01";
                    break;
                case QUEST_TYPE.special:
                    typestring = "str_tap_special_01";
                    break;
            }

            return typestring;
        }

        // quest_condition_type, quest_condition_value 받아와서 보내야 하는 퀘스트 tid 리스트 반환
        // customKey 이벤트의 일일퀘와 일반 일일퀘 타입이 같음으로 이벤트 예외처리용으로 사용됨
        public List<QuestData> GetQuestDatasList(string conditiontype, string conditionvalue, string customKey = "")
        {
            // 내가 가지고 있는 퀘스트 리스트 중에 daily, week 퀘스트Tid 만 list로 저장
            // 퀘스트 type, value string으로 넘기기
            foreach (var info in _questBase.QuestSet)
            {
                if (info.Key != QUEST_TYPE.feat)
                {
                    //var questlist = _questBase.Export(info.Key);
                    var questlist = _questBase.GetQuestDataByType(info.Key);
                    foreach (var questInfo in questlist)
                    {
                        var questData = TableManager.Instance.Quest_Table[questInfo.Key];
                        if(questData == null)
                            continue;
                        if (conditiontype !=null && conditionvalue != null)
                        {
                            //Debug.LogError(questInfo.Key + " : " +conditiontype);
                            if (questData.Quest_Condition_Type.Equals(conditiontype) && !string.IsNullOrEmpty(questData.Quest_Condition_Value) && questData.Quest_Condition_Value.Equals(conditionvalue))
                            {
                                if (questInfo.Value.Status == 0)
                                {
                                    if (!QuestDataList.Contains(questInfo.Value))
                                    {
                                        QuestDataList.Add(questInfo.Value);
                                    }
                                }
                            }
                        }

                    }

                    AddQuestGroups(info.Key, QuestDataList, customKey);
                }
                else
                {
                    if (TableManager.Instance.Quest_Table.QuestTypeDict.TryGetValue(conditiontype, out var value))
                    {
                        if (value.TryGetValue(conditionvalue, out var groupId))
                        {
                            var featDict = _questBase.GetQuestDataByType(info.Key);

                            if (featDict.TryGetValue(groupId, out var featData))
                            {
                                //이거 나중에 수정 필요함
                                if(featData.Status == 0)
                                {
                                    var questData = new QuestData();

                                    questData.Tid = groupId;
                                    QuestDataList.Add(questData);
                                }
                            }
                        }
                    }
                }

            }
            return QuestDataList;
        }
        public void AddQuestGroups(QUEST_TYPE type, List<QuestData> list, string customKey = "")
        {
            var ctype = QUEST_CONDITION_TYPE.QuestGroup.ToString();
            var cvalue = string.IsNullOrEmpty(customKey) ? type.ToString() : customKey;
            //var questlist = _questBase.Export(type);
            var questlist = _questBase.GetQuestDataByType(type);
            foreach (var questInfo in questlist)
            {
                var questData = TableManager.Instance.Quest_Table[questInfo.Key];

                if (questData == null)
                    continue;

                if (questData.Quest_Condition_Type.Equals(ctype)&& !string.IsNullOrEmpty(questData.Quest_Condition_Value) && questData.Quest_Condition_Value.Equals(cvalue))
                {
                    if (questInfo.Value.Status == 0)
                    {
                        if (!QuestDataList.Contains(questInfo.Value))
                        {
                            QuestDataList.Add(questInfo.Value);
                        }
                    }
                }
            }

        }

        // QuestReward
        public List<string> GetQuestRewardTids(QUEST_TYPE type, string eventgroupid = null)
        {
            List<string> list = new List<string>();
            if (_questBase.EnableQuestss.ContainsKey(type))
            {
                var data = _questBase.EnableQuestss[type];
                for (int n = 0; n< data.Count; n++)
                {
                    Quest_TableData questdata = null;
                    if (type != QUEST_TYPE.feat)
                    {
                        questdata  = TableManager.Instance.Quest_Table[data[n]];
                        if (string.IsNullOrEmpty(eventgroupid) && string.IsNullOrEmpty(questdata.Quest_Event_Group_Id))
                            list.Add(data[n]);

                        if (!string.IsNullOrEmpty(eventgroupid) && !string.IsNullOrEmpty(questdata.Quest_Event_Group_Id) && questdata.Quest_Event_Group_Id.Equals(eventgroupid))
                            list.Add(data[n]);
                        //if (questdata.Quest_Event_Group_Id == eventgroupid)
                        //    list.Add(data[n]);
                    }
                    else
                    {
                        var featlist = TableManager.Instance.Quest_Table.GetDatasQuestGroup(data[n]);
                        if (featlist.Count > 0)
                            list.Add(data[n]);
                    }
                }
            }

            //return _questBase.EnableQuestss[type];
            return list;
        }

        public List<string> GetQuestRewardTidsAll(string eventgroupid = "")
        {
            List<string> allrewarddatas = new List<string>();
            foreach (var item in _questBase.EnableQuestss)
            {
                if (item.Value.Count > 0)
                {
                    //allrewarddatas.AddRange(_questBase.EnableQuestss[item.Key]);
                    var list = GetQuestRewardTids(item.Key, eventgroupid);
                    allrewarddatas.AddRange(list);
                }
            }
            return allrewarddatas;
        }
        public Dictionary<QUEST_TYPE, List<string>> GetQuestRewardDataall()
        {
            return _questBase.EnableQuestss;
        }
        public List<QuestData> GetQuestDatasList(Quest_TableData data)
        {
            string tid = "";
            if (data.QUEST_TYPE != QUEST_TYPE.feat)
            {
                tid = data.Tid;
            }
            else
            {
                tid = data.Group_Id;
            }

            var questList = _questBase.GetQuestDataByType(data.QUEST_TYPE);
            var questData = questList[tid];
            if (questData.Status == 1)
            {
                if (!QuestDataList.Contains(questData))
                {
                    QuestDataList.Add(questData);
                }
            }
            return QuestDataList;
        }


        public List<QuestData> GetQuestDatasList(string questsTid)
        {
            foreach (var info in _questBase.QuestSet)
            {
                //var questlist = _questBase.Export(info.Key);
                var questlist = _questBase.GetQuestDataByType(info.Key);
                foreach (var questInfo in questlist)
                {
                    Quest_TableData questData = new Quest_TableData();
                    if (info.Key != QUEST_TYPE.feat)
                    {
                        questData = TableManager.Instance.Quest_Table[questInfo.Key];
                        if (questData.Tid.Equals(questsTid))
                        {
                            if (questInfo.Value.Status == 1)
                            {
                                if (!QuestDataList.Contains(questInfo.Value))
                                {
                                    QuestDataList.Add(questInfo.Value);
                                }
                            }
                        }
                    }
                    else
                    {
                        questData = TableManager.Instance.Quest_Table[questsTid];
                        if (questData.Group_Id.Equals(questInfo.Key))
                        {
                            if (questInfo.Value.Status == 1)
                            {
                                if (!QuestDataList.Contains(questInfo.Value))
                                {
                                    QuestDataList.Add(questInfo.Value);
                                }
                            }
                        }
                    }


                }
            }
            return QuestDataList;
        }
    }

    [Serializable]
    public class ResponseQuest
    {
        public List<Dictionary<string, string>> quests = new List<Dictionary<string, string>>();
        public string NaviQuestTid { get; set; }

    }

    //d 1, w 2, f 3,
    public class QuestBase
    {
        private Dictionary<QUEST_TYPE, QuestSet> _questSetDict = new Dictionary<QUEST_TYPE, QuestSet>();
        // Status == 1, 보상 받을 수 있는 퀘스트 Dic (quest_type - key, 타입별 List<questData> - value
        private Dictionary<QUEST_TYPE, List<string>> _enableQuests = new Dictionary<QUEST_TYPE, List<string>>();
        public Dictionary<QUEST_TYPE, QuestSet> QuestSet { get { return _questSetDict; } }
        public Dictionary<QUEST_TYPE, List<string>> EnableQuestss { get { return _enableQuests; } }


        public Dictionary<string, QuestData> GetQuestDataByType(QUEST_TYPE type)
        {
            return _questSetDict[type].QuestDataDic;
        }

        public QUEST_TYPE GetQuestType(string tid)
        {
            foreach (var list in _enableQuests)
            {
                var isContains = list.Value.Contains(tid);

                if (isContains)
                {
                    var data = TableManager.Instance.Quest_Table[tid];

                    return data.QUEST_TYPE;
                }
            }

            return QUEST_TYPE.special;
        }

        public void UpdateNewQuest(JToken token)
        {
            foreach (JProperty quest in token)
            {
                var questCategory = (QUEST_TYPE)Enum.Parse(typeof(QUEST_TYPE), quest.Name);

                if (!_questSetDict.ContainsKey(questCategory))
                {
                    QuestSet questSet = new QuestSet();
                    _questSetDict.Add(questCategory, questSet);
                }

                var questValue = quest.Value.M_JToken();
                var questJp = questValue.Cast<JProperty>();

                foreach (JProperty child in questJp)
                {
                    if (child.Name == "T")
                    {
                        _questSetDict[questCategory].Date = child.Value.S_String();
                    }
                    else
                    {
                        var childValue = child.Value.M_JToken();
                        var tid = child.Name;

                        QuestData questData = new QuestData();

                        if (childValue["T"] != null)    // feat, 업적일경우
                        {
                            questData.Tid = childValue["T"].S_String();
                        }
                        else     // daily. week. 업적이 아닐경우
                        {
                            questData.Tid = tid;
                        }

                        if (CheckIsShowQuest(questData.Tid))
                        {
                            questData.Value = childValue["V"].N_Int();
                            questData.Status = childValue["S"].N_Int();
                        }
                        else
                        {
                            Debug.Log("<color=#9efc9e> CheckIsShowQuest("+questData.Tid+") 안열림</color>");
                        }
                        //questData.Value = childValue["V"].N_Int();
                        //questData.Status = childValue["S"].N_Int();

                        _questSetDict[questCategory].UpdateQuestData(tid, questData, questCategory);
                    }
                }
            }

            SetupEnableRewardQuests();
            UpdateQuestRedDot();
        }


        // 보여야하는 퀘스트인지, 아닌지 체크
        private bool CheckIsShowQuest(string tid)
        {
            bool isShow = false;
            if (TableManager.Instance.Quest_Table[tid] == null)   // 퀘스트 테이블에 해당 퀘스트 정보가 없는경우
            {
                isShow = false;
            }
            else
            {
                var data = TableManager.Instance.Quest_Table[tid];
                if (data.Quest_Enable == 0)   // 퀘스트 테이블에 Quest_Enable값이 0인 경우 > 보여줌
                {
                    isShow = true;
                }
                else    // 퀘스트 테이블에 Quest_Enable값이 0이 아닌경우 > 안보여줌
                {
                    isShow = false;
                }
            }
            return isShow;
        }

        // UIQuest 탭별 리스트 Call Function
        public List<QuestData> GetQuestDatas(QUEST_TYPE type, string eventgroupid = null)
        {
            List<QuestData> questList = new List<QuestData>();

            if (type != QUEST_TYPE.all)
            {// Daily, Week, Feat
                var questLists = _questSetDict[type].QuestDataDic.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                foreach (var questData in questLists)
                {
                    var questdata = TableManager.Instance.Quest_Table[questData.Value.Tid];
                    if (questdata == null)
                    {
                        continue;
                    }
                    var allQData = _questSetDict[type].GetQuestData(questData.Key);
                    if (!questList.Contains(questData.Value))
                    {

                        if (string.IsNullOrEmpty(eventgroupid) && string.IsNullOrEmpty(questdata.Quest_Event_Group_Id))
                            questList.Add(questData.Value);

                        if (!string.IsNullOrEmpty(eventgroupid) &&
                           (!string.IsNullOrEmpty(questdata.Quest_Event_Group_Id)&&questdata.Quest_Event_Group_Id == eventgroupid))
                            questList.Add(questData.Value);
                    }
                }
                // questList = questList.OrderBy(q => q.Status == 1 ? 0 : q.Status == 0 ? 1 : 2).ToList();
                // 정렬 조건 : 1.수령 가능 - 진행중 - 수령 완료 순 , 2. 진행중 또는 수령 가능에서 Group_Id = stage_boss_01 이면 제일 상단

                questList = questList
  .OrderBy(q => q.Status == 1 ? 0 : q.Status == 0 ? 1 : 2)
  .ThenBy(q =>
      (q.Status == 0 || q.Status == 1) && TableManager.Instance.Quest_Table[q.Tid].Group_Id.Contains("stage_boss_") ? -1 : 0)
  .ToList();
            }
            else
            {// All
                var questLists = _questSetDict.OrderBy(x => (int)x.Key).ToDictionary(x => x.Key, x => x.Value);
                foreach (var info in questLists)
                {
                    var questValues = info.Value.QuestDataDic.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                    foreach (var questData in questValues)
                    {
                        if (TableManager.Instance.Quest_Table.TryGetData(questData.Value.Tid, out var questdata))
                        {
                            if (!questList.Contains(questData.Value))
                            {
                                if (string.IsNullOrEmpty(eventgroupid) &&
                                    string.IsNullOrEmpty(questdata.Quest_Event_Group_Id))
                                    questList.Add(questData.Value);

                                if (!string.IsNullOrEmpty(eventgroupid) &&
                                    questdata.Quest_Event_Group_Id.Equals(eventgroupid))
                                    questList.Add(questData.Value);
                            }

                            var allQData = _questSetDict[info.Key].GetQuestData(questData.Key);
                        }
                    }
                }
                // questList = questList.OrderBy(q => q.Status == 1 ? 0 : q.Status == 0 ? 1 : 2).ToList();
                // 정렬 조건 : 1.수령 가능 - 진행중 - 수령 완료 순 , 2. 진행중 또는 수령 가능에서 Group_Id = stage_boss_01 이면 제일 상단
                //                    3. 일일 - 주간 - 업적 순
                questList = questList
    .OrderBy(q => q.Status == 1 ? 0 : q.Status == 0 ? 1 : 2)
    .ThenBy(q =>
        (q.Status == 0 || q.Status == 1) && TableManager.Instance.Quest_Table[q.Tid].Group_Id.Contains("stage_boss_") ? -1 : 0)
    .ToList();
            }
            // Status 값에 따라 1-0-2 순서로 정렬

            return questList;
            //return questList;
        }

        public void SetupEnableRewardQuests()
        {
            _enableQuests.Clear();
            foreach (var info in _questSetDict)
            {
                if (!_enableQuests.ContainsKey(info.Key))
                {
                    _enableQuests.Add(info.Key, new List<string>());
                }
                //List<string> enableQData = new List<string>();
                var questLists = _questSetDict[info.Key].QuestDataDic;
                foreach (var questData in questLists)
                {
                    if (questData.Value.Status == 1)
                    {
                        string addTid = ((info.Key == QUEST_TYPE.feat) ? TableManager.Instance.Quest_Table[questData.Value.Tid].Group_Id : questData.Value.Tid);

                        if (!_enableQuests[info.Key].Contains(addTid))
                            _enableQuests[info.Key].Add(addTid);
                    }
                }
                //else
                //{
                //    _enableQuests[info.Key] = enableQData;
                //}
            }
        }

        public void UpdateQuestRedDot(string eventgroupid = null)
        {
            foreach (var info in _questSetDict)
            {
                var questLists = _questSetDict[info.Key].QuestDataDic;
                foreach (var questData in questLists)
                {
                    if (questData.Value.Status == 1)
                    {
                        if (GameInfoManager.Instance.CheckIsLocked("quest"))
                        {
                            var questdata = TableManager.Instance.Quest_Table[questData.Value.Tid];
                            //if (string.IsNullOrEmpty(eventgroupid) && string.IsNullOrEmpty(questdata.Quest_Event_Group_Id))
                            //    RedDotManager.Instance.SetActiveRedDot("reddot_quests_"+info.Key, true);

                            //if (!string.IsNullOrEmpty(eventgroupid) && questdata.Quest_Event_Group_Id.Equals(eventgroupid))
                            //    RedDotManager.Instance.SetActiveRedDot("reddot_event_quest", true);
                            if (string.IsNullOrEmpty(questdata.Quest_Event_Group_Id))
                                RedDotManager.Instance.SetActiveRedDot("reddot_quests_"+info.Key, true);
                            else
                                RedDotManager.Instance.SetActiveRedDot("reddot_event_quest", true);

                        }
                    }
                }
            }
        }

        public QuestSet GetQuestSetValue(QUEST_TYPE type)
        {
            return _questSetDict[type];
        }

        public QuestData GetQuestData(QUEST_TYPE type, string questid, string groupid)
        {
            string key = questid;
            if (type == QUEST_TYPE.feat)
                key = groupid;
            QuestData data = null;
            var questlist = _questSetDict[type].QuestDataDic;
            foreach (var infos in questlist)
            {
                if (key.Equals(infos.Key))
                {
                    data = infos.Value;
                    break;
                }
            }
            return data;
        }
    }

    public class QuestSet
    {
        //업적은 사용 안함
        private string _date;
        //데일리, 위클리 퀘스트는 tid, 업적은 groupip가 key
        private Dictionary<string, QuestData> _questDataDic = new Dictionary<string, QuestData>();

        private QUEST_TYPE _questType;

        public string Date
        {
            get => _date;
            set => _date = value;
        }

        public Dictionary<string, QuestData> QuestDataDic { get { return _questDataDic; } }

        public void UpdateQuestData(string tid, QuestData data, QUEST_TYPE questType)
        {
            _questDataDic[tid] = data;
            _questType = questType;

        }

        public QuestData GetQuestData(string tid)
        {
            return _questDataDic[tid];
        }
    }

    public class QuestData
    {
        private int _value;
        private int _status;
        //feat - groupId 가 _tid, daily/week- tid가 _tid
        private string _tid;

        public int Value
        {
            get => _value;
            set => _value = value;
        }

        public int Status
        {
            get => _status;
            set => _status = value;
        }

        public string Tid
        {
            get => _tid;
            set => _tid = value;
        }
    }
}
