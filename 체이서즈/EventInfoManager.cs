using System;

namespace LIFULSE.Manager
{
    public enum EVENT_STATE
    {
        NoOpen,                     // 이벤트 미오픈
        End,                     // 보상 기간 (이벤트 종료 ~ 이벤트 보상 종료)
        Play,                          // 이벤트 종료 (이벤트 시작 ~ 이벤트 종료)
        Expired,                    // 이벤트 진입 불가(이벤트 시작 전 || 이벤트 보상 종료 후)
    }

    public partial class GameInfoManager
    {
        public bool IsEventFirstEnter { get;  set; }

        public EVENT_STATE CheckEventState(string eventTid)
        {
            EVENT_STATE currentstate = EVENT_STATE.NoOpen;
            if (EventInfo.Event.ContainsKey(eventTid))
            {
                EventData storydata = EventInfo.Event[eventTid];

                var start = storydata.Start;        // 이벤트 시작 
                var end = storydata.End;            // 이벤트 종료
                                                    //var end = 638547624000000000;
                var reward = storydata.RewardEnd;       // 보상 종료
                // beforeday = TableManager.Instance.Event_Table[eventTid].Event_Before_End_Day;
                DateTime endDate = new DateTime(end);
                //var beforeendday = endDate.AddDays(-beforeday).Ticks;

                long now = DateTime.UtcNow.ToAddHours().Ticks;

                if (now < start || now > reward)
                    currentstate = EVENT_STATE.Expired;
                else
                { 
                    if(now >= start && now <= end)
                        currentstate = EVENT_STATE.Play;
                    if(now >= end && now <= reward)
                        currentstate = EVENT_STATE.End;
                }

            }
            else
                currentstate = EVENT_STATE.NoOpen;

            return currentstate;
        }

        public void LoadEnterEventState()
        {
            var key = RestApiManager.Instance.GetPublicKey() + "_IsEnterdLobby";
            if (DSPlayerPrefs.HasKey(key))
                IsEventFirstEnter = DSPlayerPrefs.GetBool(key);
            else
                IsEventFirstEnter = true;
        }

        public void SaveEnterEventState(bool state)
        {
            IsEventFirstEnter = state;
            var key = RestApiManager.Instance.GetPublicKey() + "_IsEnterdLobby";
            DSPlayerPrefs.SetBool(key, state);
        }
    }
}
