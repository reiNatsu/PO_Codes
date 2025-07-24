using LIFULSE.Manager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

// 마스터, 매니저, 멤버
public enum MemberGrade
{
       master,              // 마스터
       manager,           // 매니저
       member            // 멤버
}

public enum ClanState
{
    None,
    Join, //가입
    Kick, //퇴출
    NoPermission, //  권한 없음.
    NewMaster, //위임
    NotFound, //클랜 검색 실패
    Overlapped, //클랜 이름 중복
    AlreadyJoin, //이미 가입됨
    EmptyList, //신청하거나 신청 들어온 리스트 없음
    JoinMax, //가입 신청 가득 참
    Delete, //클랜 없어졌을 때
    InvalidTime, //클랜 가입 및 이름 변경 시 유효하지 않은 시간일 때
}

//길드 멤버 정보
public class UserClan
{
    public List<string> JoinList { get; private set; } //길드 가입 신청 리스트 최대 5개
    public List<ClanData> JoinDatas { get; private set; } //클랜 가입 신청 리스트 데이터

    public DateTime LimitTime { get; private set; } //가입 제한 시간
    public ClanState ClanState { get; private set; } = ClanState.None;

    public ClanConfig ClanConfig { get; private set; }
    public UserClan() { }

    public UserClan(JToken userclanToken)
    {
        if (userclanToken == null)
            return;

        if (userclanToken["CT"] != null)
            ClanState = (ClanState)userclanToken["CT"].N_Int();

        if (userclanToken["LT"] != null)
            LimitTime = new DateTime(userclanToken["LT"].N_Long(), DateTimeKind.Utc);

        if (userclanToken["JL"] != null)
        {
            JoinList = new List<string>();
            JArray joinlist = userclanToken["JL"].L_JArray();

            for (int n = 0; n< joinlist.Count; n++)
            {
                // TODO : 내가 가입 신청한 클랜 리스트 가지고 있기.
                JoinList.Add(joinlist[n].S_String());
            }
        }

        if (JoinDatas == null)
            JoinDatas = new List<ClanData>();
        
      
        if (ClanConfig == null)
            ClanConfig = new ClanConfig();

        ClanConfig.Setup(userclanToken["config"]["M"]);
    }

    public UserClan UpdateUserClan(UserClan userClan,JToken userclanToken)
    {
   
        if (userclanToken["LT"] != null)
            userClan.LimitTime =  new DateTime(userclanToken["LT"].N_Long(), DateTimeKind.Utc);

        if (userClan.JoinList == null)
            userClan.JoinList = new List<string>();

        if (userclanToken["JL"] != null)
        {
            userClan.JoinList.Clear();
            JArray joinlist = userclanToken["JL"].L_JArray();

            for (int n = 0; n< joinlist.Count; n++)
            {
                userClan.JoinList.Add(joinlist[n].S_String());
            }
        }

        return userClan;
    }

    public void SetJoinList(JToken token)
    {
        if (token == null)
            return;

        var result = token["result"];

        if (JoinDatas == null)
            JoinDatas = new List<ClanData>();
        else
            JoinDatas.Clear();

        // clandata
        if (result["clandatas"] != null)
        {
            JArray clanArray = result["clandatas"] as JArray;
            for (int n = 0; n< clanArray.Count; n++)
            {
                var clan = clanArray[n];

                ClanData clandata = new ClanData(clan);

                if (!JoinDatas.Contains(clandata))
                    JoinDatas.Add(clandata);
            }
        }

       
        Debug.Log("<color=#4cd311> SetClanList(JToken token)  _clanDatas Count : <b> " +JoinDatas.Count+ " </b></color>");
    }

    public void UpdateJoinDatas(UserClan userClan,string clanId)
    {
        if (userClan.JoinDatas == null)
            return;

        for (int n = 0; n < userClan.JoinDatas.Count; n++)
        {
            if (userClan.JoinDatas[n].Id.Equals(clanId))
                userClan.JoinDatas.Remove(JoinDatas[n]);
        }
    }

    public void InitializeUserData()
    {
        if (ClanConfig != null)
            ClanConfig = null;

        ClanConfig = GameInfoManager.Instance.ClanInfo.GetUserClanConfig();
    }
    public void UpdateConfig(JToken token)
    {
        if (token == null)
            return;

        if (ClanConfig == null)
            ClanConfig = new ClanConfig();

        ClanConfig.Setup(token);
    }

    public void UpdateConfig(string publickey, string name, string icon, string frame, string clanId, string nation, string clanName, int grade, int level, int totalpower, int point)
    {
        if(ClanConfig == null)
            ClanConfig = new ClanConfig();


        ClanConfig.Setup(publickey, name, icon, frame, clanId, nation, clanName, grade, level, totalpower, point);
    }

    //공헌도 MAX 여부 체크
    public bool IsMaxPoint()
    {
        var data = TableManager.Instance.Define_Table["ds_clan_contribution_max_point"];
        var dailyMax = data.Opt_01_Int;
        var weekMax = data.Opt_02_Int;

        var isDailyMax = dailyMax <= ClanConfig.DailyPoint;
        var isWeekMax = weekMax <= ClanConfig.DailyPoint;

        if (isDailyMax && isWeekMax)
            return true;

        return false;
    }
}