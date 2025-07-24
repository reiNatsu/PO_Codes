using LIFULSE.Manager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using UnityEngine;

public partial class RestApiManager : Singleton<RestApiManager>
{
    public void RequestHelperGetData(Action<JObject> callback = null)
    {
        RestApi("userhelper/getdata", null, (state, response) =>
        {
            Debug.Log(response.ToString());
            UpdateInfo(response["result"]);
            callback?.Invoke(response);
        });
    }

    /// <summary>
    /// 조력자 등록하기
    /// </summary>
    /// <param name="helpers">Key 슬롯 인덱스, Value PcTid</param>
    public void RequestHelperSetData(Dictionary<string, HelperData> helpers, Action<JObject> callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["helperDatas"] = JsonConvert.SerializeObject(helpers);

        RestApi("userhelper/setdata", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            UpdateInfo(response["result"]);
            callback?.Invoke(response);
        });
    }

    //내 클랜 조력자 데이터 가져오기
    public void RequestHelperGetClanHelpers(string clanId, Action<JObject> callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["clanId"] = clanId;

        RestApi("userhelper/getclanhelpers", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            if (!response["result"].ToString().Equals(HelperState.NoData.ToString()))
            {
                UpdateInfo(response["result"]);
            }
            callback?.Invoke(response);
        });
    }

    //조력자 선택
    public void RequestHelperChoose(string clanId, string targetPublicKey, string slotKey, HelperData helperData, Action<JObject,bool> callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["clanId"] = clanId;
        bodys["targetPublicKey"] = targetPublicKey;
        bodys["slotKey"] = slotKey;
        bodys["helperData"] = JsonConvert.SerializeObject(helperData);

        RestApi("userhelper/choose", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());

            //true 일시 성공
            if(ResponseHelperChoose(response, targetPublicKey, slotKey))
            {
                callback?.Invoke(response,true);
            }
            else
            {
                callback?.Invoke(response,false);
            }            
        });
    }

    private bool ResponseHelperChoose(JObject response, string targetPublicKey, string slotKey)
    {
        var result = response["result"];

        if (result == null)
            return false;

        AllEquipInfo equipInfos = null;
        AbilityInfo abilityInfo = null;
        //Dictionary<string, UserAccountStatDatas> accountStatDatas = null;

        if (result.ToString() == "NotAvailable")
        {
            return false;
        }

        if (result["equipmentinfo"] != null)
            equipInfos = Utils.ToEquipInfos(result["equipmentinfo"]);

        var dataInfo = result["datainfo"];

        if (dataInfo != null)
        {
            //var abilityDatas = result["datainfo"]["M"]["ability"]["M"];
            //accountStatDatas = new Dictionary<string, UserAccountStatDatas>();

            //foreach (JProperty item in abilityDatas)
            //{
            //    if (!item.Name.Equals("point") && item.Value.N_Int()==1)
            //    {
            //        accountStatDatas.Add(item.Name, new UserAccountStatDatas(item.Name));
            //    }
            //}

            var abilityData = dataInfo["M"]["ability"];

            if (abilityData != null)
            {
                abilityInfo = new AbilityInfo();
                abilityInfo.UpdateAbilityInfo(abilityData);
            }
        }

        List<LIFULSE.Manager.CollectionInfo> infos = new List<LIFULSE.Manager.CollectionInfo>();

        if (result["monsterinfo"] != null)
        {
            JToken token = result["monsterinfo"]["M"]["L"]["M"];

            var jp = token.Cast<JToken>();

            foreach (JProperty prop in jp)
            {
                LIFULSE.Manager.CollectionInfo info = new LIFULSE.Manager.CollectionInfo(prop.Name, 0, prop.Value.N_Int());

                infos.Add(info);
            }
        }

        var characterInfos = result["characterinfo"]["M"];
        var likeAbilityInfos = result["likeAbilityInfo"];

        DosaInfo dosaInfo = new DosaInfo();

        int combatPower = 0;

        foreach (JProperty character in characterInfos)
        {
            //호감도 레벨
            int likeAbilityLevel = 1;

            if (likeAbilityInfos != null)
            {
                if (likeAbilityInfos["M"] != null && likeAbilityInfos["M"][character.Name] != null && likeAbilityInfos["M"][character.Name]["M"] != null && likeAbilityInfos["M"][character.Name]["M"]["L"] != null)
                    likeAbilityLevel = likeAbilityInfos["M"][character.Name]["M"]["L"].N_Int();
            }

            dosaInfo.SetupPvp(character.Name, character.Value["M"], abilityInfo, infos, equipInfos, likeAbilityLevel, CHARACTER_USE_TYPE.Helper);
            dosaInfo.UpdateHelperBaseInfo(targetPublicKey, slotKey);
            combatPower+= dosaInfo.GetCombatPower();

            var helperDosa = GameInfoManager.Instance.HelperInfo.HelperDosas;
            GameInfoManager.Instance.HelperInfo.UpdateHelperDosas(targetPublicKey, slotKey, dosaInfo);
        }

        return true;
    }

    //조력자 사용 완료
    public void RequestHelperUse(string targetPublicKey, string slotKey, Action callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["targetPublicKey"] = targetPublicKey;
        bodys["slotKey"] = slotKey;

        RestApi("userhelper/use", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            UpdateInfo(response["result"]);
            callback?.Invoke();
        });
    }

    //조력자 보상 수령
    public void RequestHelperGetReward(string slotKey, Action<JObject> callback = null)
    {
        var bodys = new Dictionary<string, string>();

        bodys["slotKey"] = slotKey;

        RestApi("userhelper/getreward", bodys, (state, response) =>
        {
            Debug.Log(response.ToString());
            UpdateInfo(response["result"]);
            callback?.Invoke(response);
        });
    }

    //조력자 사용 횟수 초기화
    public void RequestHelperRefreshCount(Action callback = null)
    {
        RestApi("userhelper/refreshcount", null, (state, response) =>
        {
            Debug.Log(response.ToString());
            UpdateInfo(response["result"]);
            callback?.Invoke();
        });
    }

    public void UpdateHelperInfo(JToken token)
    {
        if (token == null)
            return;

        GameInfoManager.Instance.HelperInfo.UpdateHelperInfo(token["M"]);
    }
}