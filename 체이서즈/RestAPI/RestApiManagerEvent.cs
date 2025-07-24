using LIFULSE.Manager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//RestApiManagerEvent
public partial class RestApiManager
{
    public void RequestGetEventData(Action<JObject> callBack = null)
    {
        if (!_isLogin)
            return;
        RestApi("event/getdata", null, (state,result) =>
        {
            Debug.Log(result.ToString());
            UpdateEventData(result["result"]);
            callBack?.Invoke(result);
            //UpdateUserData(result["result"]);

        });

    }
    public void EventCheck(Action<JObject> callBack = null)
    {
        if (!_isLogin)
            return;
        
        RestApi("event/eventcheck",  null, (state,response) =>
        {
            var result = response["result"];
            if (result != null)
            {
                var eventInfo = result["eventinfo"];
                var changeList = UpdateEventData(eventInfo);
                var post = response["post"];
                if (post != null)
                {
                    GameInfoManager.Instance.PostInfo.UpdatePostData(post["M"], false);
                }
                Debug.Log(result.ToString());
                callBack?.Invoke(response);
                //eventInfo 가 존재하면 팝업이 무조건 떠야함...
            }
        });
    }
 
    /// <summary>
    /// pass -> 패스 group_id <br/>
    /// index -> 보상 받을 패스 인덱스 / 1부터 시작... <br/>
    /// passType -> pass_Type
    /// </summary>
    /// <param name="pass"></param>
    /// <param name="index"></param>
    /// <param name="passType"></param>
    /// <param name="callBack"></param>
        public void GetPassReward(string pass, int index, string passType,Action<JObject> callBack = null)
        {
            if (!_isInit)
                return;


            RestApi("event/getpassreward", new Dictionary<string, string>()
            {
                {"pass", pass},
                {"index", index.ToString()},
                {"type", passType},
            }, (state,response) =>
            {
                var result = response["result"];
                if (result != null)
                {
                    var eventInfo = result["eventinfo"];
                    var changeList = UpdateEventData(eventInfo);
                    UpdateReward(response, true);
                    Debug.Log(result.ToString());
                    callBack?.Invoke(response);
                    //eventInfo 가 존재하면 팝업이 무조건 떠야함...
                }
            });

        }
    /// <summary>
    /// pass -> 패스 group_id
    /// </summary>
    /// <param name="pass"></param>
    /// <param name="callBack"></param>
        public void GetAllPassReward(string pass,Action<JObject> callBack = null)
        {
            if (!_isInit)
                return;


            RestApi("event/getallpassreward", new Dictionary<string, string>()
            {
                {"pass", pass},
            }, (state,response) =>
            {
                var result = response["result"];
                if (result != null)
                {
                    var eventInfo = result["eventinfo"];
                    var changeList = UpdateEventData(eventInfo);
                    UpdateReward(response, true);
                    Debug.Log(result.ToString());
                    callBack?.Invoke(response);
                }
            });

        }
    /// <summary>
    /// 테스트용 api<br/>
    /// pass -> 패스 group_id<br/>
    /// exp -> 추가할 경험치
    /// </summary>
    /// <param name="pass"></param>
    /// <param name="exp"></param>
    /// <param name="callBack"></param>
        public void AddPassExp(string pass,int exp,Action<JObject> callBack = null)
        {
            if (!_isInit)
                return;


            RestApi("event/addpassexp", new Dictionary<string, string>()
            {
                {"pass", pass},
                {"exp", exp.ToString()},
            }, (state,response) =>
            {
                var result = response["result"];
                if (result != null)
                {
                    var eventInfo = result["eventinfo"];
                    var changeList = UpdateEventData(eventInfo);
                    var post = response["post"];
                    if (post != null)
                    {
                        GameInfoManager.Instance.PostInfo.UpdatePostData(post["M"], false);
                    }
                    Debug.Log(result.ToString());
                    callBack?.Invoke(response);
                    //eventInfo 가 존재하면 팝업이 무조건 떠야함...
                }
            });

        }
}
