using LIFULSE.Manager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//클랜원 정보
public class ClanConfig
{
    [JsonProperty] public string Name { get; private set; }
    [JsonProperty] public string ClanName { get; private set; }
    [JsonProperty] public string Icon { get; private set; }
    [JsonProperty] public int Grade { get; private set; }
    [JsonProperty] public string Frame { get; private set; } // 테두리 아이콘
    [JsonProperty] public long LastLoginTime { get; private set; } // 마지막 로그인 시간
    [JsonProperty] public long AttendanceTime { get; private set; } // 출석 시간
    [JsonProperty] public string PublicKey { get; private set; } // 임시 데이터 생성 임시 UID
    [JsonProperty] public string ClanId { get; private set; } //가입한 클랜 Id
    [JsonProperty] public string Nation { get; private set; } //선택한 국가 (nation table tid) 기본 대한민국

    [JsonProperty] public int Level { get; private set; }
    [JsonProperty] public int TotalPower { get; private set; }      // 총합 전투력
    [JsonProperty] public int Point { get; private set; }      // 총 공헌도
    [JsonProperty] public int WeekPoint { get; private set; }      // 주간 공헌도
    [JsonProperty] public int DailyPoint { get; private set; } // 일간 공헌도

    //key 조력자를 등록한 클랜원 publickey
    //value 4개의 데이터 존재 0,1 (0이면 사용 가능  1 사용함) 앞에 두개 스테이지 뒤에 두개 컨텐츠

    public ClanConfig() { }

    public ClanConfig(JToken token)
    {
        Setup(token);
    }

    public void Setup(JToken token)
    {
        if (token == null)
            return;

        if (token["N"] != null)
            Name = token["N"].S_String();

        if (token["CN"] != null)
            ClanName = token["CN"].S_String();
        if (token["F"] != null)
            Frame = token["F"].S_String();
        if (token["IC"] != null)
            Icon = token["IC"].S_String();
        if (token["PK"] != null)
            PublicKey = token["PK"].S_String();
        //var lastLoginTime = token["LLT"].S_String();
        if (token["ID"] != null)
            ClanId = token["ID"].S_String();

        if (token["NAT"] != null)
            Nation = token["NAT"].S_String();

        if (token["G"] != null)
            Grade = token["G"].N_Int();
        if (token["LV"] != null)
            Level= token["LV"].N_Int();
        if (token["TP"] != null)
            TotalPower = token["TP"].N_Int();
        if (token["P"] != null)
            Point= token["P"].N_Int();
        if (token["DP"] != null)
            DailyPoint = token["DP"].N_Int();
        if (token["WP"] != null)
            WeekPoint= token["WP"].N_Int();

        if (token["LLT"] != null)
            LastLoginTime = token["LLT"].N_Long();
        if (token["A"] != null)
            AttendanceTime = token["A"].N_Long();
    }

    public void Setup(string publickey, string name, string icon, string frame, string clanId, string nation, string clanName, int grade, int level, int totalpower, int point)
    {
        Name =name;
        Icon = icon;
        Frame = frame;
        ClanId = clanId;
        Grade = grade;
        Level = level;
        PublicKey = publickey;
        TotalPower = totalpower;
        LastLoginTime = 0;
        Point = point;
        Nation = nation;
        ClanName = clanName;
    }

    public void SetClanName(string clanName)
    {
        ClanName = clanName;
    }

    public void SetGrade(int grade)
    {
        Grade = grade;
    }

    //이름, 아이콘, 프레임, 레벨, 전투력

    public JObject SetConfigChange()
    {
        JObject config = new JObject();
        var totalPower = GameInfoManager.Instance.GetAllDosaTotalPower();
        var account = GameInfoManager.Instance.AccountInfo;

        // 이름
        if (!Name.Equals(account.Name))
            config["N"] = account.Name.ToString();

        // 아이콘
        if (!Icon.Equals(account.GetIconID()))
            config["IC"] = account.GetIconID().ToString();

        // 프레임
        if (!Frame.Equals(account.GetLineID()))
            config["F"] = account.GetLineID().ToString();

        // 레벨
        if (Level != account.Level)
            config["LV"] = account.Level;

        // 프레임
        if (TotalPower != totalPower)
            config["TP"] = totalPower;

        return config;
    }
}
