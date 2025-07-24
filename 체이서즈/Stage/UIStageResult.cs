using LIFULSE.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UIStageResult : MonoBehaviour
{
    [SerializeField] private List<UICharacterResultExp> _resultExpList;

    [SerializeField] private ExTMPUI _accountLevelTMP;
    [SerializeField] private ExTMPUI _accountExpTMP;

    [SerializeField] private ExTMPUI _stageNameTMP;
    [SerializeField] private ExTMPUI _playTimeTMP;

    [SerializeField] private int[] _stars = null;
    public void Show(ResultData resultData)
    {
        gameObject.SetActive(true);

        var stageData = TableManager.Instance.Stage_Table[resultData.StageTid];

        _stageNameTMP.ToTableText(stageData.Str_Stage);

        //00:00 방식으로 표기
        TimeSpan t = TimeSpan.FromSeconds(resultData.RemainTime);

        _playTimeTMP.text = t.ToString(@"mm\:ss");

        UpdateAccountExp(stageData.Acquire_Id_Exp);
        UpdateCharacterExp(resultData.Team, stageData.Acquire_Char_Exp);
    }

    private void UpdateAccountExp(int exp)
    {
        _accountLevelTMP.text = GameInfoManager.Instance.AccountInfo.Level.ToString();
        _accountExpTMP.text = "+" + exp;
    }

    private void UpdateCharacterExp(List<string> team, int exp)
    {

        for (int i = 0; i < team.Count; i++)
        {
            Debug.Log("<color=#f57eb6>UpdateCharacterExp Team ["+team[i]+"] "+exp+"</color>");

            if (string.IsNullOrEmpty(team[i]))
                _resultExpList[i].gameObject.SetActive(false);
            else
                _resultExpList[i].Show(team[i], exp);
        }
    }

}
