using LIFULSE.Manager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class RestApiManager
{
    [BoxGroup("Reward")]

    [Button]
    public void RequestRewardRefreshData(List<string> rewardTypes, Action callback = null)
    {
        Dictionary<string, string> bodys = new Dictionary<string, string>();

        rewardTypes = new List<string>();

        rewardTypes.Add(RewardType.kkaebidungeon.ToString());
        rewardTypes.Add(RewardType.manjang.ToString());

        bodys["rewardTypes"] = JsonConvert.SerializeObject(rewardTypes);

        RestApi("userreward/refreshdata", bodys, (state,response) =>
        {
            Debug.Log(response.ToString());

            UpdateReward(response, true);

            callback?.Invoke();
        });
    }

    //포인트 달성 보상
    public void RequestRewardGetKkaebiLimit(List<string> limitTids, Action callback = null)
    {
        Dictionary<string, string> bodys = new Dictionary<string, string>();

        bodys["limitTids"] = JsonConvert.SerializeObject(limitTids);

        RestApi("userreward/getkkaebilimit", bodys, (state,response) =>
        {
            Debug.Log(response.ToString());

            UpdateReward(response, true);
            
            callback?.Invoke();
        });
    }

    //시즌 보상
    public void RequestRewardGetKkaebiSeasonReward(List<string> rewardTids, Action callback = null)
    {
        Dictionary<string, string> bodys = new Dictionary<string, string>();

        bodys["rewardTids"] = JsonConvert.SerializeObject(rewardTids);

        RestApi("userreward/getkkaebiseasonreward", bodys, (state,response) =>
        {
            Debug.Log(response.ToString());
            UpdateReward(response, true);
            //GameInfoManager.Instance.RewardInfo.SetKkaebiDungeionRedDot();
            callback?.Invoke();
        });
    }

    public void RequestRewardUseRandomBox(string itemTid, int useCount,bool showRewardUI, Action<JObject> callback = null)
    {
        Dictionary<string, string> bodys = new Dictionary<string, string>();

        bodys["itemTid"] = itemTid;
        bodys["useCount"] = useCount.ToString();

        RestApi("userreward/userandombox", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            UpdateInfo(response["result"]);
            UpdateReward(response, showRewardUI);
            callback?.Invoke(response);
        });
    }

    public void RequestRewardUseChoiceBox(string itemTid, Dictionary<string, int> rewardDatas, Action<JObject> callback = null)
    {
        Dictionary<string, string> bodys = new Dictionary<string, string>();

        bodys["itemTid"] = itemTid;
        bodys["rewardDatas"] = JsonConvert.SerializeObject(rewardDatas);

        RestApi("userreward/usechoicebox", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            UpdateInfo(response["result"]);
            UpdateReward(response, true);
            callback?.Invoke(response);
        });
    }

    public void RequestRewardGetAccountLevelReward(List<string> accountLevelTids, Action<JObject> callback = null)
    {
        Dictionary<string, string> bodys = new Dictionary<string, string>();

        if (accountLevelTids != null)
            bodys["accountLevelTids"] = JsonConvert.SerializeObject(accountLevelTids);

        RestApi("userreward/getaccountlevelreward", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            UpdateInfo(response["result"]);
            UpdateReward(response, true);
            callback?.Invoke(response);
        });
    }
}
