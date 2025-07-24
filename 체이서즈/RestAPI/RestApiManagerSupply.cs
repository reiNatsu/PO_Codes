using LIFULSE.Manager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using UnityEngine;

public partial class RestApiManager : Singleton<RestApiManager>
{
    //보급품 정보
    public void RequestSupplyGetData(Action<JObject> callback = null)
    {
        RestApi("supply/getdata", null, (state, response) =>
        {
            Debug.Log(response.ToString());
            UpdateInfo(response["result"]);
            callback?.Invoke(response);
        });
    }

    //보급품 수령하기
    public void RequestSupplyGetReward(string clanId, Action<JObject> callback = null)
    {
        var body = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(clanId))
            body["clanId"] = clanId;

        RestApi("supply/getreward", body, (state, response) =>
        {
            Debug.Log(response.ToString());
            UpdateInfo(response["result"]);
            UpdateReward(response, true);

            if(response["result"]["postinfo"] != null)
                GameInfoManager.Instance.PostInfo.UpdatePostData(response["result"]["postinfo"]["M"], false);

            callback?.Invoke(response);
        });
    }

    //보급품 획득 리스트 업데이트
    public void RequestSupplyUpdate(string clanId, Action<JObject> callback = null) 
    {
        var body = new Dictionary<string, string>();

        if(!string.IsNullOrEmpty(clanId))
            body["clanId"] = clanId;

        RestApi("supply/update", body, (state, response) =>
        {
            Debug.Log(response.ToString());
            if (response["result"].Type == JTokenType.Object)
                UpdateInfo(response["result"]);
            callback?.Invoke(response);
        });
    }
}