using Consts;
using LIFULSE.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIProfile : UIBase
{
    [SerializeField] public ProfileCell sample;

    [SerializeField] public Transform[] tr;

    private QuestData[] _questData;
    public override void Close(bool needCached = true)
    {
        base.Close(needCached);
    }
    public override void GoBack()
    {
        base.GoBack();
    }

    public override void Init()
    {
        base.Init();
        foreach (var item in tr)
        {
            Clear(item);
        }
    }
    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        /*
        _questData  = new QuestData[3]
        {
            GameInfoManager.Instance.QuestBase._dailyQuestData,
            GameInfoManager.Instance.QuestBase._weeklyQuestData,
            GameInfoManager.Instance.QuestBase._totalQuestData
        };
        foreach (var item in tr)
        {
            Clear(item);
        }


        int i = 0;
        foreach (var item in tr)
        {
            QuestDataLog(item, _questData[i]);
            i++;
        }

        var collect = GameInfoManager.Instance.KkaebiBase;
        foreach (var item in collect.Box)
        {
            sample.SetLogData("박스 수집 번호 :: ", $"{item._index} // {item._amount}개", tr[2]);
        }
        foreach (var item in collect.KKAEBI)
        {
            sample.SetLogData("깨비공 번호 :: ", $"{item.Key} // {item.Value.Count}개", tr[2]);
        }
        sample.gameObject.SetActive(false);
        */
    }
    public void QuestDataLog(Transform tr, QuestData questData)
    {
        /*
        sample.SetLogData("사용한 캐쉬 ::", questData.SpendCash.ToString(), tr);
        sample.SetLogData("사용한 골드 ::", questData.SpendGold.ToString(), tr);
        sample.SetLogData("사용한 곡옥 ::", questData.SpendActionPoint.ToString(), tr);
        sample.SetLogData("가차 횟수 ::", questData.GachaCount.ToString(), tr);
        sample.SetLogData("던전 입장 횟수 ::", questData.EnterDungeon.ToString(), tr);


        foreach (var item in questData.CompleteQuest)
        {
            sample.SetLogData("완료한 퀘스트 ::", $"{TableManager.Instance.Quest_Table[item].Str_Quest.ToTableText()}", tr);
        }
        foreach (var item in questData.SpendItem)
        {
            sample.SetLogData("사용한 아이템 ::", $"{TableManager.Instance.Item_Table[item.Key].Item_Name_Text_Id.ToTableText()}::{item.Value}개", tr);
        }
        foreach (var item in questData.MonsterKill)
        {
            sample.SetLogData("죽인 몬스터 ::", $"{item.Key}::{item.Value}마리", tr);
        }
*/
    }

    public void Clear(Transform tr)
    {
        foreach (Transform item in tr)
        {
            if (item != sample.transform)
                Destroy(item.gameObject);
        }

    }
}
