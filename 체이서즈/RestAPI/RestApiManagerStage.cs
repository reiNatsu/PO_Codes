using Consts;
using LIFULSE.Manager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.UIElements;

public partial class RestApiManager : Singleton<RestApiManager>
{

    public void RequestQuestReward(string questtid, QUEST_TYPE questtype, string clanId, string eventid = null, Action<JObject> callBack = null)
    {
        if (!_isLogin)
            return;

        RequestQuestReward(new List<string> { questtid }, questtype, clanId, eventid, callBack);
    }

    // 퀘스트(임무) 보상 수령 패킷
    public void RequestQuestReward(List<string> questlist ,QUEST_TYPE questtype, string clanId, string eventid = null,Action<JObject> callBack = null)
    {
        if (!_isLogin)
            return;

        Dictionary<string, string> bodys = new Dictionary<string, string>();

        var data = GetQuestsBodyDatas(questlist);
        
        if (data.quests.Count ==  0)
            return;

        var needAddPoint = false;

        //이벤트 퀘스트는 클랜 공헌도 안줌
        if(questtype != QUEST_TYPE.feat && string.IsNullOrEmpty(eventid))
        {
            if (!string.IsNullOrEmpty(clanId) && !GameInfoManager.Instance.ClanInfo.UserClanData.IsMaxPoint())
            {
                needAddPoint = true;
                bodys["clanId"] = clanId;
            }
        }

        bodys["questdata"] = JsonConvert.SerializeObject(data);
        bodys["passarray"] = JsonConvert.SerializeObject(GameInfoManager.Instance.EventInfo.Pass.Keys.ToArray());

        RestApi("quest/questreward", bodys, (state,response) =>
        {
            Debug.Log(response.ToString());
            JArray array = response["result"]["reward"]["Quest"] as JArray;

            for (int n = 0; n< array.Count; n++)
            {
                SetAnalyticsQuestEvent(questlist[n], array[n][0].ToString(), (int)array[n][1]);
            }

            UpdateQuestRedDotValue(questlist, questtype, eventid);
            UpdateReward(response, true);

            //클랜 공헌도 관련 정보 업데이트
            if(needAddPoint)
            {
                UpdateUserClanInfo(response["result"]["claninfo"]);
                GameInfoManager.Instance.ClanInfo.MyClanData.Setup(response["result"]);
            }

            callBack?.Invoke(response);
        });
    }


    public void SetAnalyticsQuestEvent(string questTid, string itemtid, int value)
    {
        //return;
        if (AnalyticsManager.Instance.IsInitialized)
        {
            try
            {
                CustomEvent myEvent = new CustomEvent("DosaQuestCheckCompletion")
                {
                              { "time", AnalyticsManager.Instance.SetKoreaTime()},
                              { "uid", RestApiManager.Instance.GetPublicKey()},
                              {"name", GameInfoManager.Instance.AccountInfo.Name},
                              { "quest_tid",questTid},
                             // { "get_cash", getcash},
                              { "have_cash", GameInfoManager.Instance.GetAmount("i_cash")},
                             // { "get_gold", getgold},
                              { "have_gold",  GameInfoManager.Instance.GetAmount("i_gold")},
                     };

                if (itemtid.Equals(TICKET_TYPE.i_cash.ToString()) || itemtid.Equals(TICKET_TYPE.i_free_cash.ToString()))
                {
                    myEvent.Add("get_cash", value);
                    myEvent.Add("get_gold", 0);
                }
                if (itemtid.Equals(TICKET_TYPE.i_gold.ToString()))
                {
                    myEvent.Add("get_gold", value);
                    myEvent.Add("get_cash", 0);
                }

                AnalyticsManager.Instance.SendCustomEvent(myEvent);
            }
            catch (Exception e)
            {
                Debug.LogError($"Fail to send Analytics CustomEvent : DosaQuestCheckCompletion");
            }
        }
    }

    public void UpdateQuestRedDotValue(List<string> list, QUEST_TYPE type, string eventid)
    {
        if (!string.IsNullOrEmpty(eventid))
        {
            for (int m = 0; m < list.Count; m++)
            {
                RedDotManager.Instance.SetActiveRedDot("reddot_event_quest", false);
            }
        }
        else
        {
            if (type != QUEST_TYPE.all)
            {   // daily, week, feat 각각 탭에서 모두받기 눌렀을 때
                for (int m = 0; m < list.Count; m++)
                {
                    RedDotManager.Instance.SetActiveRedDot("reddot_quests_"+type, false);
                }
            }
            else
            {   // all 탭에서 모두 받기 눌렀을 때
                var tablist = GameInfoManager.Instance.QuestBase.QuestSet;
                foreach (var tabs in tablist)
                {
                    var rewardlist = GameInfoManager.Instance.GetQuestRewardDataall();
                    for (int n = 0; n < rewardlist[tabs.Key].Count; n++)
                    {
                        //RedDotManager.Instance.SetActiveRedDot("reddot_quests_tab", (int)tabs.Key-1, -1,false);
                        RedDotManager.Instance.SetActiveRedDot("reddot_quests_"+tabs.Key, false);
                    }
                }
            }
        }
    }


    // 24.11.19 김남훈
    // 삭제
    //[Button]
    //public void RequestUserDataRescueKkaebi(string kkaebigongTid, string chapterKey, Action callback = null)
    //{
    //    if (!_isInit)
    //        return;

    //    var body = new Dictionary<string, string>();

    //    body["tid"] = kkaebigongTid;
    //    body["chapterKey"] = chapterKey;

    //    RestApi("userdata/rescuekkaebi", body, (state,response) =>
    //    {
    //        Debug.Log(response.ToString());
    //        UpdateUserData(response["result"]);
    //        callback?.Invoke();
    //    });
    //}


    [Button]
    public void QuestReward(string questTid, Action<JObject> callBack = null)
    {
        if (!_isLogin)
            return;

        Dictionary<string, string> bodys = new Dictionary<string, string>();

        var data = GetQuestsBodyDatas(QUEST_CONDITION_TYPE.None, string.Empty);
        bodys["questdata"] = JsonConvert.SerializeObject(data);
        //ServerQuestData questData = new ServerQuestData();
        //questData.AddQuest("character_collection");
        //bodys["questdata"] = questData.Export();

        //var data = GetQuestsBodyDatas(questTid);
        //bodys["questdata"] = JsonConvert.SerializeObject(data);

        RestApi("quest/questreward", bodys, (state,result) =>
        {
            Debug.Log(result.ToString());
            callBack?.Invoke(result);
        });
    }


    public void RequestStageEnter(string tid, List<string> team, Action callback = null)
    {
        if (!_isInit)
            return;
        List<string> convertTeam = new List<string>();

        for (int i = 0; i < team.Count; i++)
        {
            if (!string.IsNullOrEmpty(team[i]))
                convertTeam.Add(team[i]);
        }

        var body = new Dictionary<string, string>();

        body["stage"] = tid;
        body["character"] = JsonConvert.SerializeObject(convertTeam);
        var cookItem = GameInfoManager.Instance.AccountInfo.SelectedCookItemTid;
        if (!cookItem.IsNullOrEmpty())
        {
            body["food"] = cookItem; //음식
        }
        
        RestApi("userdata/stageenter", body, (state,response) =>
        {
            Debug.Log(response.ToString());
            callback?.Invoke();
            GameInfoManager.Instance.EnterStatus = EnterStatus.Success;
        }, (state,fail) => GameInfoManager.Instance.EnterStatus = EnterStatus.Failed);
    }

    //selectStage 스토리 용
    public void RequestStageClear(ResultType resultType, int star1, int star2, int star3, int clearCount, string selectStage = null, 
        bool isStory = false, string helperPublicKey = null, string helperSlotKey = null, Action<JObject> callback = null)
    {
        if (!_isInit)
            return;

        Dictionary<string, string> bodys = new Dictionary<string, string>();
        QUEST_CONDITION_TYPE[] types = new QUEST_CONDITION_TYPE[] { }; 
        string[] values = new string[] { };
        if (!isStory)
        {
            types = new QUEST_CONDITION_TYPE[] { QUEST_CONDITION_TYPE.StageGroup, QUEST_CONDITION_TYPE.CharacterLevel };
            values = new string[] { "stage_main", "none"};
        }

        var questData = GetStageQuestDatas(selectStage, !string.IsNullOrEmpty(helperPublicKey));

        if (questData != null)
            bodys["questdata"] = JsonConvert.SerializeObject(questData);

        bodys["star1"] =star1.ToString();
        bodys["star2"] = star2.ToString();
        bodys["star3"] = star3.ToString();
        bodys["clearCount"] =  clearCount.ToString();
        bodys["resultType"] = resultType.ToString();

        if (!string.IsNullOrEmpty(helperPublicKey))
            bodys["helperPublicKey"] = helperPublicKey;

        if (!string.IsNullOrEmpty(helperSlotKey))
            bodys["helperSlotKey"] = helperSlotKey;

        if (!string.IsNullOrEmpty(selectStage))
            bodys["selectStage"] = selectStage;

        bodys["openArea"] = CheckOpneArea(selectStage).ToString();

        List<string> stagestats = CheckLiberationItem(selectStage);
        if (!string.IsNullOrEmpty(selectStage) && stagestats.Count > 0)
            bodys["liberationQuestTids"] = JsonConvert.SerializeObject(stagestats);
      

        RestApi("userdata/stageclear", bodys, (state,response) =>
        {
            Debug.Log(response.ToString());
            UpdateReward(response, false);
            UpdateInfo(response["result"]);
            callback?.Invoke(response);
        });
    }
    private int CheckOpneArea(string stagetid)
    {
        int result = 0;
        if (string.IsNullOrEmpty(stagetid))
            return result;

        var table = TableManager.Instance.Stage_Table[stagetid];
        // 다음 에리어 오픈 해야 하는 조건.
        if (table.CONTENTS_TYPE_ID == CONTENTS_TYPE_ID.stage_main && table.LEVEL_DIFFICULTY == LEVEL_DIFFICULTY.normal)
        {
            var list = TableManager.Instance.Stage_Table.GetMainStageListByLevel(table.Theme_Id, CONTENTS_TYPE_ID.stage_main, LEVEL_DIFFICULTY.normal);
            if (stagetid.Equals(list.LastOrDefault().Tid))
                result = 1;
        }
        return result;
    }

    private List<string> CheckLiberationItem(string stagetid)
    {
        if (string.IsNullOrEmpty(stagetid))
            return null;

        List<string> stage = new List<string>();
        var table = TableManager.Instance.Stage_Table[stagetid];

        // 아직 아이템을 체크 해야 하는지.
        var liberation = TableManager.Instance.Liberation_Table.GetData(table.Theme_Id);
        var itemquests = TableManager.Instance.Liberation_Quest_Table.GetItemQuests(liberation.Tid);
        var rewardList = new List<string>();
        int count = 0;
        if (table.Reward_01_Info != null && table.Reward_01_Info.Length > 0)
        {
            for (int n = 0; n < itemquests.Count; n++)
            {
                var info = itemquests[n];
                if (GameInfoManager.Instance.LiberationInfo.GetInfo(liberation.Tid, info.Tid) != null
                    && TableManager.Instance.Reward_Table.GetRewardItems(table.Reward_01_Info).Contains(info.Liberation_Quest_Condition))
                {
                    count++;
                }
            }
        }

        if (count > 0)
            stage = GameInfoManager.Instance.EnableItemQuest(liberation.Tid);

        return stage;
    }
    public void RequestChapterReward(int chapter, LEVEL_DIFFICULTY level, int index, Action<JObject> callBack = null)
    {
        if (!_isLogin)
            return;
        var bodys = new Dictionary<string, string> { };

        bodys["chapter"] =chapter.ToString();
        bodys["level"] = level.ToString();
        bodys["index"] = index.ToString();

        // 0 none , 1 star 단계 , 2 star 단계 , 3 star 단계
        RestApi("userdata/chapterreward", bodys, (state,response) =>
        {
            Debug.Log(response.ToString());
            UpdateReward(response, true);
            
            callBack?.Invoke(response);
        });
    }

    [Button]
    public void StageEnter(string tid, Action callback = null)
    {
        if (!_isInit)
            return;
        RestApi("userdata/stageenter", new Dictionary<string, string>()
        {
            {"stage" , tid},
            {"character" , JArray.FromObject(new string[]{ "CH_ayobi_01"}).ToString()},

        }, (state,result) =>
        {
            Debug.Log(result.ToString());
            callback?.Invoke();
        });
    }
    [Button]
    public void StageClear()
    {
        if (!_isInit)
            return;

        Dictionary<string, string> bodys = new Dictionary<string, string>();

        bodys["star1"] = "1";
        bodys["star2"] = "1";
        bodys["star3"] = "1";
        bodys["clearCount"] =  "1";
        bodys["resultType"] = ResultType.Victory.ToString();
        var data = GetQuestsBodyDatas(QUEST_CONDITION_TYPE.StageGroup, "stage_main");
        bodys["questdata"] = JsonConvert.SerializeObject(data);

        RestApi("userdata/stageclear", bodys, (state,response) =>
        {
            Debug.Log(response.ToString());
            UpdateReward(response, false);
        });
    }



    [Button]
    public void UpdateStageBSTest(string tid, int clearCount)
    {
        if (!_isInit)
            return;
        RestApi("userdata/updatebeststageadmin", new Dictionary<string, string>()
        {
            {"stage" , tid},
            {"clearCount" , clearCount.ToString()}

        }, (state,result) =>
        {
            Debug.Log(result.ToString());
        });
    }



    [Button]
    public void UpdateQuesAdmin(string tid, int value)
    {
        if (!_isInit)
            return;
        RestApi("quest/updatequestAdmin", new Dictionary<string, string>()
        {
            {"tid" , tid},
            {"value" , value.ToString()}

        }, (state,result) =>
        {
            Debug.Log(result.ToString());
        });
    }

}
