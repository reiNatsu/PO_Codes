using LIFULSE.Manager;
using Newtonsoft.Json.Linq;

public class NaviQuestInfo
{
    //현재 진행 중인 navi quset table tid
    //나머지 상태(카운트, desc 등) quest data를 통해 업데이트
    //Tid가 empty면 navi quest close 해줘야함

    public string Tid { get; private set; }
    public int Count { get; private set; } //퀘스트 진행도

    public void UpdateNaviInfo(JToken token)
    {
        if (token == null)
            return;

        if (token["T"] != null)
            Tid = token["T"].S_String();

        if (token["C"] != null)
            Count = token["C"].N_Int();
    }

    //퀘스트 데이터 보낼 때 네비도 같이 보내야함
    public string GetCheckTid(string questGroupId) 
    {
        if (string.IsNullOrEmpty(Tid))
            return null;

        //체크할 퀘스트
        var naviQuestData = TableManager.Instance.Navi_Quest_Table[Tid];

        //네비에 연결된 퀘스트
        if (naviQuestData == null)
            return null;

        var targetQuestData = TableManager.Instance.Quest_Table[naviQuestData.Connect_Quest_Id];

        if (targetQuestData == null)
            return null;

        if (naviQuestData.Quest_Check_Type == 0 && targetQuestData.Group_Id.Equals(questGroupId))
            return naviQuestData.Tid;

        return null;
    }
}