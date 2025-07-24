using LIFULSE.Manager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperData
{
    [JsonProperty] public string PcTid { get; private set; } //조력자 pc tid
    [JsonProperty] public string Name { get; private set; } //닉네임


    [JsonProperty] public long ReceivedTime { get; private set; } //보상 수령 기간 tick
    [JsonProperty] public long UpdateTime { get; private set; } //편성 시간

    [JsonProperty] public int UsedCount { get; private set; } //조력자 사용된 횟수
    [JsonProperty] public int Level { get; private set; } //레벨
    [JsonProperty] public int CombatPower { get; private set; } //전투력

    public HelperData() { }

    public HelperData(JToken token)
    {
        Setup(token);
    }

    //조력자 저장용(RequestHelperSetData)
    public HelperData(string pcTid)
    {
        var info = GameInfoManager.Instance.CharacterInfo.GetDosa(pcTid);

        PcTid = info.Tid;
        Level = info.Level;
        CombatPower = info.GetCombatPower();
        UsedCount = 0;
        Name = GameInfoManager.Instance.AccountInfo.Name;
    }

    public void Setup(JToken token)
    {
        if (token == null)
            return;

        if(token["PT"] != null)
            PcTid = token["PT"].S_String();

        if (token["N"] != null)
            Name = token["N"].S_String();

        if (token["RT"] != null)
            ReceivedTime = token["RT"].N_Long();

        if (token["UT"] != null)
            UpdateTime = token["UT"].N_Long();

        if (token["UC"] != null)
            UsedCount = token["UC"].N_Int();

        if (token["LV"] != null)
            Level = token["LV"].N_Int();

        if (token["CP"] != null)
            CombatPower = token["CP"].N_Int();
    }

    public void SetName(string name)
    {
        Name = name;
    }
}