using Consts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class RestApiManager : Singleton<RestApiManager>
{
    [Button]
    public void ReqestEventStoryGetData(Action callback = null)
    {
        if (!_isLogin)
            return;

        RestApi("eventstory/getdata", null, (state,response) =>
        {
            Debug.Log(response.ToString());
            UpdateInfo(response["result"]);
            callback?.Invoke();
        });
    }

    [Button]
    public void RequestEventStoryClear(ResultType resultType, int star1, int star2, int star3, int clearCount, string stageTid, string eventStoryTid, List<string> team,bool isStory, Action<JObject> callback = null)
    {
        if (!_isLogin)
            return;

        Dictionary<string, string> bodys = new Dictionary<string, string>();

        QUEST_CONDITION_TYPE[] types = new QUEST_CONDITION_TYPE[] { }; 
        string[] values = new string[] { };
        if (!isStory)
        {
            types = new QUEST_CONDITION_TYPE[] { QUEST_CONDITION_TYPE.StageGroup, QUEST_CONDITION_TYPE.CharacterLevel, QUEST_CONDITION_TYPE.StageGroup };
            values = new string[] { "event_main", "none","stage_main" };
        }

        var questData = GetQuestsBodyDatas(types, values,"daily_special");
        if (questData != null)
            bodys["questdata"] = JsonConvert.SerializeObject(questData);

        bodys["star1"] = star1.ToString();
        bodys["star2"] = star2.ToString();
        bodys["star3"] = star3.ToString();
        bodys["clearCount"] =  clearCount.ToString();
        bodys["resultType"] = resultType.ToString();
        bodys["stageTid"] = stageTid;
        bodys["eventStoryTid"] = eventStoryTid;
        bodys["team"] = JsonConvert.SerializeObject(team);

        RestApi("eventstory/clear", bodys, (state,response) =>
        {
            Debug.Log(response.ToString());
            UpdateReward(response, false);
            callback?.Invoke(response);
        });
    }
}
