using Consts;
using LIFULSE.Manager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UIElements;
using static RestApiManager;

public partial class RestApiManager : Singleton<RestApiManager>
{
    #if !LIVE_BUILD&&!EXTERNAL_BUILD
    [Button]
    public void TestProduct(string tid)
    {
        Dictionary<string, string> bodys = new Dictionary<string, string>();

        bodys["store"] = tid;

        RestApi("store/purchase", bodys, (state,response) =>
        {
            Debug.Log(response.ToString());
            //UpdateUserStore(response["result"]["storeinfo"]);
            //GameInfoManager.Instance.PostInfo.UpdatePostData(response["result"]["M"], false, response["result"]["deleteKeys"]);
            GameInfoManager.Instance.PostInfo.UpdatePostData(response["result"]["postinfo"]["M"], false);
            UpdateEventData(response["result"]["eventinfo"]);
        });
    }
    #endif
    public void RequestStoreGetData(Action<bool, JToken> callBack = null)          // 처음에 상점 리스트 받아옴
    {
        if (!_isLogin)
            return;

        RestApi("store/getdata", null, (state,response) =>
        {
            Debug.Log(response.ToString());

            JToken result = response["result"]["M"];

            callBack?.Invoke(result["D"].S_String().IsNullOrEmpty(), result);
        });
    }

    [Button]
    public void ResponseStoreAllRefreshStore(List<string> refreshKeys = null, bool isDaily = false,Action<JToken> callBack = null)       // 상점 전체 초기화 : 클라에서 일정 시간마다 체크.
    {
        if (!_isLogin)
            return;

        Dictionary<string, string> bodys = new Dictionary<string, string>();
        List<string> options = new List<string>();

        if(refreshKeys != null && refreshKeys.Count > 0)
        {
            for (int i = 0; i < refreshKeys.Count; i++)
            {
                options.Add(refreshKeys[i]);
            }
        }

        bodys["options"] = JsonConvert.SerializeObject(options);

        RestApi("store/allrefreshstore", bodys, (state,response) =>
        {
           Debug.Log(response.ToString());
           JToken storeToken = response["result"][0]["json"]["response"][0]["Item"]["storeinfo"]["M"];
           SettingStoreItemsInfo(storeToken, isDaily:isDaily);
           callBack?.Invoke(storeToken);
       });
    }

    private void SettingStoreItemsInfo(JToken token, bool isDaily= false ,Action callback = null)
    {
        GameInfoManager.Instance.LastResetDayTime = new DateTime(long.Parse(token["D"].S_String()));

        if (token["W"] != null)
        {
            GameInfoManager.Instance.LastResetWeekTime = new DateTime(long.Parse(token["W"].S_String()));
        }
        if (token["M"] != null)
        {
            GameInfoManager.Instance.LastResetMonthTime = new DateTime(long.Parse(token["M"].S_String()));
        }


        bool isBuy = (token["S"] != null) ? true : false;
        // 마지막 리셋 데이, 위클리 저장.
        var list = token["SO"]["M"].Cast<JProperty>();
        GameInfoManager.Instance.UpdatePurchaseItemCount(token["S"]);
        if (isDaily)
        {
            GameInfoManager.Instance.ReUpdatePurchaseItems(token["S"]);
        }
        //if (isBuy) 
        //{
        //    // 구매 이력이 있는 아이템 갯수 업데이트 
        //    var buyToken = token["S"]["M"].Cast<JProperty>();
        //    foreach (var pitem in buyToken)
        //    {
        //        var value = token["S"]["M"][pitem.Name]["M"]["C"].N_Int();
        //        GameInfoManager.Instance.UpdatePurchaseItemCount(pitem.Name, value, list);
        //    }
        //}

        StoreInfo storeSetting = null;
        foreach (var item in list)
        {
          // 갱신 시간 후 구매이력 삭제 → 해당 상점의 구매 이력 삭제.
          //if(!isBuy)
          //  {
          //      GameInfoManager.Instance.ReUpdatePurchaseItems(item.Name);
          //  }

            var name = item.Name;
            var infos = item.Value["M"]["IS"]["SS"];
            List<Store_TableData> items = new List<Store_TableData>();
            foreach (var data in infos)
            {
                var itemInfo = TableManager.Instance.Store_Table.GetItemDataByTid(data.ToString());
                if (!items.Contains(itemInfo) && itemInfo != null)
                {
                    items.Add(itemInfo);
                }
            }
            var refreshCount = item.Value["M"]["R"].N_Int();
            storeSetting = new StoreInfo(name, items, refreshCount);
            GameInfoManager.Instance.SettingStoreInfo(storeSetting);
        }
       
        GameInfoManager.Instance.SetAllStoreTypeItemList();

        callback?.Invoke();
    }

    public void RequestStoreRefreshStore(string refreshStoreOption, Action<JObject> callBack = null)          // 갱신 버튼 누르면 초기화
    {
        if (!_isLogin)
            return;
        Dictionary<string, string> bodys = new Dictionary<string, string>();
        bodys["option"] = refreshStoreOption;
        // 지금은 다이아 소모 퀘스트가 없다.
        Debug.Log(bodys);
        RestApi("store/refreshstore", bodys, (state,response) =>
        {
            Debug.Log(response.ToString());

            SettingClickRefreshStore(refreshStoreOption, response);

              // 갱신 후  StoreInfo데이터 업데이트 
              callBack?.Invoke(response);
        });
    }
    private void SettingClickRefreshStore(string storeoptionv, JToken refresh, Action callBack = null)
    {
        //GameInfoManager.Instance.ItemPurchaseInfo.Clear();
        JArray resultArray = refresh["result"][0]["json"]["response"] as JArray;
        for (int i = 0; i <resultArray.Count; i++)
        {
            var result = resultArray[i];
            var item = result["Item"];

            UpdateInfo(item);
        }
        callBack?.Invoke();
    }


    public void RequestStoreUseStore(string _useStoreTid, string count, string type ,Action<JObject> callBack = null)
    {
        if (!_isLogin)
        {
            return;
        }
        Dictionary<string, string> bodys = new Dictionary<string, string>();
        bodys["store"] = _useStoreTid;
        bodys["count"] = count;

        QUEST_CONDITION_TYPE[] typeArr = { };
        string[] values = { };

        //var data = GetQuestsBodyDatas(QUEST_CONDITION_TYPE.ConsumCurrency, "i_gold");
        var data = SetUseStroeQuests(type);

        if(data != null)
            bodys["questdata"] = JsonConvert.SerializeObject(data);

        RestApi("store/usestore", bodys, (state,response) =>
        {
            Debug.Log(response.ToString());
            // 구매 후 정보 업데이트(재화, 아이템 수량, 유저 정보 ...
            UpdateReward(response, true);
            CloseStoreUsePopup();
            callBack?.Invoke(response);
        });
    }

    private ResponseQuest SetUseStroeQuests(string type)
    {
        ResponseQuest result = null;

        switch (type)
        {
            case "store":
                result = GetQuestsBodyDatas(QUEST_CONDITION_TYPE.ConsumCurrency, "i_gold");
                break;
            case "cook":
                {
                    QUEST_CONDITION_TYPE[] types = { QUEST_CONDITION_TYPE.Cook, QUEST_CONDITION_TYPE.ConsumCurrency };
                    string[] values = { "none", "i_gold" };
                    result = GetQuestsBodyDatas(types, values);
                }
                break;
            case "craft":
                {
                    result = GetQuestsBodyDatas(QUEST_CONDITION_TYPE.Craft, "none");
                    //QUEST_CONDITION_TYPE[] types = { QUEST_CONDITION_TYPE.Craft, QUEST_CONDITION_TYPE.ConsumCurrency };
                    //string[] values = { "none", "i_gold" };
                    //result = GetQuestsBodyDatas(types, values);
                }
                break;
        }

        return result;
    }

    public void RequestStoreLimitCheck(string useStoreTid, Action<JObject> callBack = null)
    {
        if (!_isLogin)
        {
            return;
        }
        Dictionary<string, string> bodys = new Dictionary<string, string>();
        bodys["store"] = useStoreTid;
      
        RestApi("store/storelimitcheck", bodys, (state,response) =>
        {
            Debug.Log(response.ToString());
            // 구매 후 정보 업데이트(재화, 아이템 수량, 유저 정보 ...
            callBack?.Invoke(response);
        });
    }
    public void CloseStoreUsePopup()
    {
        var usePop = UIManager.Instance.GetUI<UIPopupStoreItemBuy>();
        if (usePop != null && usePop.gameObject.activeInHierarchy)
        {
            usePop.Close();
        }
    }


    // 리셋 시간 불러 오기.
    [Button("Rest Base Get Time")]
    public void RequestBaseGetTime(Action callBack = null)
    {
        if (!_isLogin)
        {
            return;
        }
        RestApi("base/gettime", null, (state,response) =>
        {
            Debug.Log(response.ToString());
            DateTime resetDailyTime = new DateTime((long)response["result"]["resetDaily"], DateTimeKind.Utc);
            DateTime resetWeeklyTime = new DateTime((long)response["result"]["resetWeekly"], DateTimeKind.Utc);
            DateTime resetMonthTime = new DateTime((long)response["result"]["resetMonth"], DateTimeKind.Utc);

            //DateTime nowTime = new DateTime((long)response["result"]["now"], DateTimeKind.Utc);
            //DateTime testResetTime = new DateTime(2024,03,21,11,30,00);

            GameInfoManager.Instance.NowResetTime = new DateTime((long)response["result"]["now"], DateTimeKind.Utc);
            GameInfoManager.Instance.DailyResetTime = resetDailyTime;
            GameInfoManager.Instance.WeeklyResetTime = resetWeeklyTime;
            GameInfoManager.Instance.MonthlyResetTime = resetMonthTime;

            //.AddSeconds(1)
            //GameInfoManager.Instance.ResetTimeDatas[RESET_TYPE.weekly] = resetWeeklyTime;
            //GameInfoManager.Instance.ResetTimeDatas[RESET_TYPE.month]= resetMonthTime;
            //GameInfoManager.Instance.SetRefreshDailyTimer();    // TimerManager 설정 함수
            //CheckNowResetStore();

            Debug.Log("<color=#ff8000> Reset Time ━ Daily("+resetDailyTime+"), Week("+resetWeeklyTime+"), Month("+resetMonthTime+") </color>");
            callBack?.Invoke();
        });
    }

    [Button]
    public void RequestGetNowTime(Action callBack = null)
    {
        if (!_isLogin)
            return;

        RestApi("base/getnowtime", null, (state, response) =>
        {
            GameInfoManager.Instance.NowTime = new DateTime((long)response["result"]["now"], DateTimeKind.Utc);
            callBack?.Invoke();
        });
    }

    public void RequestGetNowTimeNoLogin(Action callBack = null)
    {
        RestApi("base/getnowtimenonecheck", null, (state, response) =>
        {
            GameInfoManager.Instance.NowTime = new DateTime((long)response["result"]["now"], DateTimeKind.Utc);
            callBack?.Invoke();
        });
    }

    public void ReCallSetResetTime()
    {
        RequestBaseGetTime();
    }

    // 갱신시간 이후 로그인 했을 reset 해야 하는지 비교
    public void CheckNowResetStore(Action<JToken> callBack = null)
    {
        var last = GameInfoManager.Instance.LastResetDayTime;
        var next = GameInfoManager.Instance.DailyResetTime;

        //int result = next.Day - last.Day;
        long dayresult = next.Ticks - last.Ticks;
        Debug.Log("<color=#ff8000> next.Ticks("+next.Ticks+") - last.Ticks("+last.Ticks+") = "+(next.Ticks - last.Ticks)+" </color>");

        if (dayresult>0)           //  상점 갱신 시간
        {
            //Debug.Log("<color=#ff8000> 상점 갱신시간 입니다.【  result >"+result+" 】 </color>");
            ResponseStoreAllRefreshStore(GameInfoManager.Instance.ContentStoreList, isDaily:true, callBack:callBack);
            RequestUserRefreshTicket();
            RequestPvpGetReward();
            GameInfoManager.Instance.SetHelperCountRefresh();
            //RequestHelperRefreshCount();
            // 주간 월간 체크 필요
            CheckIsWeeklyRefresh();
            CheckIsMonthlyRefresh();
        }
        else
        {
            //Debug.Log("<color=#ff8000> !! 상점 갱신시간이 아닙니다..▶Daily reset("+ (next.Day - last.Day)+"), " +
            //"Week reset ("+(GameInfoManager.Instance.WeeklyResetTime.Day - GameInfoManager.Instance.LastResetWeekTime.Day)+"," +
            //"Month reset ("+(GameInfoManager.Instance.MonthlyResetTime.Day - GameInfoManager.Instance.LastResetMonthTime.Day)+") </color>");
        }
        callBack?.Invoke(null);
    }

    //주간 갱신 시, 실행 할 함수
    public void CheckIsWeeklyRefresh()
    { 
        var last = GameInfoManager.Instance.LastResetWeekTime;
        var next = GameInfoManager.Instance.WeeklyResetTime;
        long weekresult = next.Ticks - last.Ticks;
        Debug.Log("<color=#ff8000><b>WEEKLY</b> next.Ticks("+next.Ticks+") - last.Ticks("+last.Ticks+") = "+(next.Ticks - last.Ticks)+" </color>");
        if (weekresult > 0)
        {
            // 주간 갱신으로 해야 할 일.
            var clanId = GameInfoManager.Instance.ClanInfo.GetUserClanId();

            if(!string.IsNullOrEmpty(clanId))
                RequestClanRefreshWeekPoint(clanId);               // 클랜 > 주간 공헌도 초기화 api
        }
    }

    // 월간 갱신 시, 실행 할 함수
    public void CheckIsMonthlyRefresh()
    {
        var last = GameInfoManager.Instance.LastResetMonthTime;
        var next = GameInfoManager.Instance.MonthlyResetTime;
        long monthresult = next.Ticks - last.Ticks;
        Debug.Log("<color=#ff8000><b>MONTHLY</b> next.Ticks("+next.Ticks+") - last.Ticks("+last.Ticks+") = "+(next.Ticks - last.Ticks)+" </color>");
        if (monthresult > 0)
        { 
            // 월간 갱신으로 해야 할 일
        }
    }

    //public void UsePayStore(string useStoreTid,Action callBack = null)
    public void UsePayStore(string useStoreTid,Action inApp,Action callBack = null)
    {
        RequestStoreLimitCheck(useStoreTid, (limitResult) =>
        {
            var message = limitResult["result"].Value<String>();
            var code = limitResult["code"].Value<int>();
            if (code == 1)
            {
                inApp.Invoke();
                
                
                //1.인앱 구매 진행...
                //2.영수증 검증
                //3.지급,미지급
            }
            else
            {
                //리미트 오버... message 참조
            }

        });
    }


}



// 퀘스트

//[System.Serializable]
//public class ConsumQuestsList
//{
//    public List<Dictionary<string, string>> quests = new List<Dictionary<string, string>>();
//    public void Add(string tid)
//    {
//        quests.Add(new Dictionary<string, string> { { "tid", tid } });
//    }
//    public void Clear()
//    {
//        quests.Clear();
//    }
//}
