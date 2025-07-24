using Consts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupHelperReward : UIBase
{
    [SerializeField] private ExTMPUI _timeTMP;
    [SerializeField] private ExTMPUI _countTMP;

    [SerializeField] private ExTMPUI _timeRewardTMP;
    [SerializeField] private ExTMPUI _countRewardTMP;

    public override void Show(Dictionary<UIOption, object> optionDict)
    {
        //시간 관련
        if(optionDict.TryGetValue(UIOption.Time, out var time))
            UpdateTime((int)time);
        if (optionDict.TryGetValue(UIOption.Value, out var timReward))
            UpdateTimeReward((int)timReward);

        //횟수 관련
        if (optionDict.TryGetValue(UIOption.Count, out var count))
            UpdateCount((int)count);
        if (optionDict.TryGetValue(UIOption.Value2, out var countReward))
            UpdateCountReward((int)countReward);
    }

    //시간 출력
    private void UpdateTime(int sec)
    {
        var timeSpan = TimeSpan.FromSeconds(sec);
        if (timeSpan.TotalHours >= 1)
            _timeTMP.ToTableText("str_ui_post_remain_time_03", (int)timeSpan.TotalHours);
        else
            _timeTMP.ToTableText("str_ui_post_remain_time_04", (int)timeSpan.TotalMinutes);

    }

    //사용 횟수
    private void UpdateCount(int count)
    {
        _countTMP.ToTableArgs(count);
    }

    private void UpdateTimeReward(int rewardCount)
    {
        _timeRewardTMP.text = rewardCount.ToString();
    }

    private void UpdateCountReward(int rewardCount)
    {
        _countRewardTMP.text = rewardCount.ToString();
    }
}
