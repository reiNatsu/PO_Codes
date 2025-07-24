using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

//길드 가입 조건
public enum JoinCondition
{
    All,
    Level,
}
// 길드 가입 승인 여부 타입
public enum ApproveClanType
{
    Immediately,             // 바로 가입
    Approval                     // 승인 가입
}

public class ClanData
{
    public Dictionary<string, int> SkillInfos { get; private set; } //SL
    public List<string> Members { get; private set; }    // ML
    public List<string> ConfirmMasterList { get; private set; } //CML 클랜 자동 위임 시 팝업 확인 해야하는 리스트
    private ClanConfig _clanConfig;

    public List<ClanConfig> Configs { get; private set; } //클랜원들의 config 데이터
                                                          // CL, JL이 필요 => 내 클렌 데이터의 경우
    public List<ClanConfig> JoinList { get; private set; } // 가입신청한 사람들 

    public string Id { get; private set; } //ID 길드 아이디
    public string Name { get; private set; } //Name 길드 이름
    public string Notice { get; private set; } //NT 공지사항
    public string Icon { get; private set; }//IC
    public string MasterPublicKey { get; private set; }//MPK
    public string PrevMasterName { get; private set; }//클랜 자동 위임 시 이전 마스터 닉네임

    public int Grade { get; private set; } // 직급
    public int SkillPoint { get; private set; }//SP
    public int Level { get; private set; }//LV
    public int Exp { get; private set; }//E
    public int ConditionType { get; private set; } //CT 가입 조건 (없음, 레벨 등)
    public int ConditionValue { get; private set; } //CV 가입 조건 수치 (레벨 : x 이상)
    public int MaxMemberCount { get; private set; }//MMC
    public int CurrentMemberCount { get; private set; }//CMC
    public int JoinType { get; private set; } //즉시 가입 여부 0 즉시 가입 1 수락

    public DateTime MasterActiveTime { get; private set; } //MAT 마스터 로그아웃 시간 (자동 위임 체크용)
    public DateTime WeekRefreshTime { get; private set; } //WRT 주간 공헌도 초기화 시간
    public DateTime NameChangeTime { get; private set; } //NCT 클랜 이름 변경 시간

    public RankData RankData { get; private set; }

    // 클랜 데이터만 받아오는 경우.
    public ClanData() { }

    public ClanData(JToken result) 
    {
        Setup(result);
    }

    

    public void Setup(JToken result)
    {
        if (result == null)
            return;

        JToken clanDataToken = result["clandata"];
        JToken clanInfosToken = result["claninfos"];

        if (clanDataToken == null)
            return;

        // 임시 주석. 나중에 수정 해야 함.
        if (clanInfosToken != null)
        {
            Configs = new List<ClanConfig>();
            var infos = clanInfosToken["L"].ToArray();

            for (int i = 0; i < infos.Length; i++)
            {
                var config = new ClanConfig(infos[i]["M"]["config"]["M"]);

                Configs.Add(config);
            }
            Configs = Configs.OrderBy(c => c.Grade).ToList();
        }

        if (clanDataToken["M"]["ID"] != null)
            Id =clanDataToken["M"]["ID"].S_String();

        if (clanDataToken["M"]["MPK"] != null)
            MasterPublicKey =clanDataToken["M"]["MPK"].S_String();

        if (clanDataToken["M"]["PMN"] != null)
            PrevMasterName = clanDataToken["M"]["PMN"].S_String();

        if (clanDataToken["M"]["N"] != null)
            Name =clanDataToken["M"]["N"].S_String();

        if (clanDataToken["M"]["SP"] != null)
            SkillPoint = clanDataToken["M"]["SP"].N_Int();

        if(clanDataToken["M"]["G"] != null)
            Grade = clanDataToken["M"]["G"].N_Int();

        if(clanDataToken["M"]["NT"] != null)
            Notice = clanDataToken["M"]["NT"].S_String();

        if (clanDataToken["M"]["IC"] != null)
            Icon =clanDataToken["M"]["IC"].S_String();

        if(clanDataToken["M"]["LV"] != null)
            Level = clanDataToken["M"]["LV"].N_Int();

        if(clanDataToken["M"]["E"] != null)
            Exp = clanDataToken["M"]["E"].N_Int();

        if (clanDataToken["M"]["CT"] != null)
            ConditionType = clanDataToken["M"]["CT"].N_Int();

        if (clanDataToken["M"]["CV"] != null)
            ConditionValue = clanDataToken["M"]["CV"].N_Int();

        if (clanDataToken["M"]["MMC"] != null)
            MaxMemberCount = clanDataToken["M"]["MMC"].N_Int();

        if (clanDataToken["M"]["JT"] != null)
          JoinType = clanDataToken["M"]["JT"].N_Int();

        if (clanDataToken["M"]["CMC"] != null)
            CurrentMemberCount = clanDataToken["M"]["CMC"].N_Int();

        if (clanDataToken["M"]["MAT"] != null)
            MasterActiveTime = new DateTime(clanDataToken["M"]["MAT"].N_Long());

        if (clanDataToken["M"]["WRT"] != null)
            WeekRefreshTime = new DateTime(clanDataToken["M"]["WRT"].N_Long());

        if (clanDataToken["M"]["NCT"] != null)
            NameChangeTime = new DateTime(clanDataToken["M"]["NCT"].N_Long());

        if (clanDataToken["M"]["ML"] != null)
        {
            Members = new List<string>();
            var members = clanDataToken["M"]["ML"]["L"].ToArray();
            for (var m = 0; m<members.Length;m++)
            {
                Members.Add(members[m].S_String());
            }
        }

        if (clanDataToken["M"]["CML"] != null)
        {
            ConfirmMasterList = new List<string>();
            var members = clanDataToken["M"]["CML"]["L"].ToArray();

            for (var m = 0; m<members.Length; m++)
            {
                ConfirmMasterList.Add(members[m].S_String());
            }
        }

        if (clanDataToken["M"]["JL"] != null)
        {
            JoinList = new List<ClanConfig>();
            var joinlist = clanDataToken["M"]["JL"]["L"].ToArray();
            for (int j = 0; j < joinlist.Length; j++)
            {
                // 나중에 수정 해야 함.
                var config = new ClanConfig(joinlist[j]["M"]);
                JoinList.Add(config);
            }
        }

        if(SkillInfos == null)
            SkillInfos = new Dictionary<string, int>();

        if (clanDataToken["M"]["SL"] != null)
        {
            var skilllist = clanDataToken["M"]["SL"]["M"].Cast<JProperty>();

            foreach (var info in skilllist)
             {
                if (!SkillInfos.ContainsKey(info.Name))
                    SkillInfos.Add(info.Name, info.Value.N_Int());
                else
                    SkillInfos[info.Name] = info.Value.N_Int();
            }
        }
    }

    // 클랜 마스터 정보 가지고 오기
    public ClanConfig GetClanMasterInfo()
    {
        ClanConfig master = new ClanConfig();
        for (int n = 0; n< Configs.Count; n++)
        {
            if (Configs[n].Grade == 0)
                master = Configs[n];   
        }
        return master;
    }
}

public class EditClanData
{ 
    public string Name { get; private set; }
    public string Notice { get; private set; }
    public string Icon { get; private set; }
    public int ConditionType { get; private set; }
    public int ConditionValue { get; private set; }
    public int JoinType { get; private set; }
    public Dictionary<string, int> SkillInfos { get; private set; } //SL

    public EditClanData() { }
    public EditClanData(string name, string notice, string icon, int conditiontype, int conditionvalue, int jointype, Dictionary<string,int> skills )
    {

        Name = name;
        Notice = notice;
        Icon = icon;
        ConditionType = conditiontype;
        ConditionValue = conditionvalue;
        JoinType = jointype;
        SkillInfos = skills;
    }
    
}