using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LIFULSE.Manager
{
    public partial class GameInfoManager
    {

        // 각 챕터별 퍼센테이지 구해오기.
        public int GetLiberationPercent(string liberationID)
        {
            int clearcount = 0;
            if (LiberationInfo.GetQuests(liberationID) == null)
                return 0;

            var data = LiberationInfo.GetQuests(liberationID);
            foreach (var info in data)
            {
                var table = TableManager.Instance.Liberation_Quest_Table[info.Key];
                if (info.Value.Rewarded == 1)
                    clearcount++;
            }

            int percentage = Mathf.FloorToInt((float)clearcount / data.Count * 100);
            return percentage;
        }

        // 아직 미완료된 item 체크 타입 해방 임무
        public List<string> EnableItemQuest(string groupid)
        {
            List<string> list = new List<string>();
            var quests = LiberationInfo.GetQuests(groupid);
            foreach (var data in quests)
            {
                var table = TableManager.Instance.Liberation_Quest_Table[data.Key];
                if (table.Liberation_Quest_Type.Equals("Item") && (data.Value.Rewarded == 0 && data.Value.Value < table.Completed_Value))
                    list.Add(table.Tid);
            }

            return list;
        }
    }
}


